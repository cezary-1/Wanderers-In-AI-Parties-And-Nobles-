using HarmonyLib;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CampaignBehaviors.AiBehaviors;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace WanderersInParties.Patches
{
    [HarmonyPatch(typeof(DefaultClanTierModel), nameof(DefaultClanTierModel.GetCompanionLimit))]
    static class CompanionLimitPatch
    {
        // Postfix on the *public* method
        static void Postfix(Clan clan, ref int __result)
        {
            var s = WanderersSettings.Instance;
            if (s == null) return;
            if (!s.EnableCompanions) return;

            // 1) Compute what vanilla would have given *before* perk bonuses:
            int vanillaBase = clan.Tier + 3;

            // 2) Isolate the amount added by perks:
            int perkBonus = __result - vanillaBase;

            // 3) Compute your mod’s base + tier bonus:
            int newBase = vanillaBase;
            if (s.MaxCompanions >= 0)
                newBase = s.MaxCompanions;
            if (s.MaxCompanionsTier >= 0)
                newBase += clan.Tier * s.MaxCompanionsTier;

            if (newBase == 0) __result = newBase;
            else __result = newBase + perkBonus;
            // 4) Recombine: new base + original perk bonus


        }
    }

    [HarmonyPatch(typeof(CompanionsCampaignBehavior), "get__desiredTotalCompanionCount")]
    public static class CompanionDesiredTotalCompanionPatch
    {
        // Token: 0x0600000B RID: 11 RVA: 0x00002272 File Offset: 0x00000472
        [HarmonyPostfix]
        private static void Postfix(ref float __result)
        {
            var s = WanderersSettings.Instance;
            if (s == null) return;
            if (s.TavernSpawn && s.MaxTavernWanderers != 0 && s.MinTavernWanderers <= s.MaxTavernWanderers)
            {
                var desired = (s.MinTavernWanderers + s.MaxTavernWanderers) / 2;
                __result = (float)(desired * Town.AllTowns.Count);
            }
                
        }
    }

    [HarmonyPatch(typeof(CompanionsCampaignBehavior), "OnNewGameCreated")]
    static class CompanionsCampaignBehaviorNewGameCreatedPatch
    {
        // We're prefixing the *private* method OnNewGameCreated(CampaignGameStarter)
        [HarmonyPrefix]
        static bool Prefix(CampaignGameStarter starter)
        {
            var s = WanderersSettings.Instance;
            // if tavern spawns are *enabled*, skip the vanilla spawn
            if (s != null && s.TavernSpawn && s.MinTavernWanderers <= s.MaxTavernWanderers)
                return false;

            // otherwise, let the original Bannerlord code run
            return true;
        }
    }

    [HarmonyPatch(typeof(LordConversationsCampaignBehavior))]
    static class LiberateWanderersPatches
    {
        // Extend the “known hero” condition to also include any freed wanderer‐companion
        [HarmonyPostfix]
        [HarmonyPatch("conversation_liberate_known_hero_on_condition")]
        static void KnownHero_Postfix(ref bool __result)
        {
            if (!__result)
            {
                var hero = Hero.OneToOneConversationHero;
                if (hero != null
                    && Campaign.Current.CurrentConversationContext == ConversationContext.FreedHero
                    && !Campaign.Current.ConversationManager.CurrentConversationIsFirst
                    // here we accept either lords OR any companion/wanderer
                    && (hero.Occupation == Occupation.Lord || hero.Occupation == Occupation.Wanderer))
                {
                    __result = true;
                }
            }
        }

        // Same for the “unmet hero” variation
        [HarmonyPostfix]
        [HarmonyPatch("conversation_liberate_unmet_hero_on_condition")]
        static void UnmetHero_Postfix(ref bool __result)
        {
            if (!__result)
            {
                var hero = Hero.OneToOneConversationHero;
                if (hero != null
                    && Campaign.Current.CurrentConversationContext == ConversationContext.FreedHero
                    && Campaign.Current.ConversationManager.CurrentConversationIsFirst
                    && (hero.Occupation == Occupation.Lord || hero.Occupation == Occupation.Wanderer))
                {
                    __result = true;
                }
            }
        }

        //Can't recruit wanderers with clan
        [HarmonyPostfix]
        [HarmonyPatch("conversation_hero_hire_on_condition")]
        static void HireHero_Postfix(ref bool __result)
        {
            if (__result)
            {
                var hero = Hero.OneToOneConversationHero;
                if (hero != null 
                    && !hero.IsPlayerCompanion 
                    && hero.Clan != null)
                {
                    __result = false;
                }
            }
        }

    }


    //Tick errors

    // Catch & swallow any exception in AiVisitSettlementBehavior.AiHourlyTick
    [HarmonyPatch(typeof(AiVisitSettlementBehavior), "AiHourlyTick")]
    static class AiVisitSettlementBehavior_Finalizer
    {
        [HarmonyFinalizer]
        static Exception Finalizer(Exception __exception, MobileParty mobileParty)
        {
            if (__exception != null)
            {
                var s = WanderersSettings.Instance;
                if (s == null) return null;
                MobileParty party = mobileParty;

                var name = party?.Name?.ToString() ?? "unknown party";

                if (s.Debug)
                    InformationManager.DisplayMessage(
                        new InformationMessage($"[WIP] Skipped AiVisitSettlementBehavior for {name} from {party?.ActualClan?.Name}: {__exception.Message}, disbanding party/removing hero from party", Colors.Red)
                    );

                    if (party.Army != null && party.Army.LeaderParty == party)
                    {
                        DisbandArmyAction.ApplyByUnknownReason(party.Army);
                    }
                    party.Army = null;

                    if (party.Party.IsActive)
                    {
                        DisbandPartyAction.StartDisband(party);
                        party.Party.SetCustomOwner(null);
                        DestroyPartyAction.Apply(null, party); // test
                    }
                
            }
            // returning null swallows the exception
            return null;
        }
    }

    // Catch & swallow any exception in AiPartyThinkBehavior.PartyHourlyAiTick
    [HarmonyPatch(typeof(AiPartyThinkBehavior), "PartyHourlyAiTick")]
    static class AiPartyThinkBehavior_Finalizer
    {
        [HarmonyFinalizer]
        static Exception Finalizer(Exception __exception, MobileParty mobileParty)
        {
            if (__exception != null)
            {
                var s = WanderersSettings.Instance;
                if(s == null) return null;
                MobileParty party = mobileParty;

                var name = party?.Name?.ToString() ?? "unknown party";

                if(s.Debug)
                InformationManager.DisplayMessage(
                    new InformationMessage($"[WIP] Skipped PartyHourlyAiTick for {name} from {party?.ActualClan?.Name}: {__exception.Message}, disbanding party/removing hero from party", Colors.Red)
                );

                if (party.Army != null && party.Army.LeaderParty == party)
                {
                    DisbandArmyAction.ApplyByUnknownReason(party.Army);
                }
                party.Army = null;

                if (party.Party.IsActive)
                {
                    DisbandPartyAction.StartDisband(party);
                    party.Party.SetCustomOwner(null);
                    DestroyPartyAction.Apply(null, party); // test
                }

            }
            return null;
        }
    }

    // Catch & swallow any exception in MapEventSide.HandleMapEventEndForPartyInternal
    [HarmonyPatch(typeof(MapEventSide), "HandleMapEventEndForPartyInternal")]
    static class HandleMapEventEndForPartyInternal_Finalizer
    {
        [HarmonyFinalizer]
        static Exception Finalizer(Exception __exception, PartyBase party)
        {
            if (__exception != null)
            {
                var s = WanderersSettings.Instance;
                if (s == null || party == null) return null;

                var name = party?.Name?.ToString() ?? "unknown party";

                if (s.Debug)
                    InformationManager.DisplayMessage(
                        new InformationMessage($"[WIP] Skipped HandleMapEventEndForPartyInternal for {name} from {party?.Owner?.Clan?.Name}: {__exception.Message}, disbanding party/removing hero from party", Colors.Red)
                    );


            }
            return null;
        }
    }

    // Player Hire Cost
    [HarmonyPatch(typeof(DefaultCompanionHiringPriceCalculationModel), nameof(DefaultCompanionHiringPriceCalculationModel.GetCompanionHiringPrice))]
    static class GetCompanionHiringPrice_Postfix
    {
        [HarmonyPostfix]
        static void Postfix(Hero companion, ref int __result)
        {
            var s = WanderersSettings.Instance;
            if (__result == 0 || s.PlayerCostMultiplier <= 0) return;

            __result = (int)(__result * s.PlayerCostMultiplier);
        }
    }

    [HarmonyPatch(typeof(HeroSpawnCampaignBehavior), "GetBestAvailableCommander")]
    static class WandererLeadParty_GetBestAvailableCommanderPatch
    {
        // We're prefixing the *private* method OnNewGameCreated(CampaignGameStarter)
        [HarmonyPrefix]
        static bool Prefix(Clan clan, ref Hero __result)
        {
            var s = WanderersSettings.Instance;
            if (s == null || !s.LeadParty) return true;

            Hero hero = null;
            float num = 0f;
            foreach (Hero hero2 in clan.Heroes)
            {
                if (hero2.IsActive && hero2.IsAlive && hero2.PartyBelongedTo == null && hero2.PartyBelongedToAsPrisoner == null && hero2.CanLeadParty() && hero2.Age > (float)Campaign.Current.Models.AgeModel.HeroComesOfAge && (hero2.CharacterObject.Occupation == Occupation.Lord || hero2.CharacterObject.Occupation == Occupation.Wanderer))
                {
                    float heroPartyCommandScore = GetHeroPartyCommandScore(hero2);
                    if (heroPartyCommandScore > num)
                    {
                        num = heroPartyCommandScore;
                        hero = hero2;
                    }
                }
            }
            if (hero != null)
            {
                __result = hero;
                if (s.Debug && hero.IsWanderer) InformationManager.DisplayMessage(new InformationMessage($"New leader is : {hero.Name}"));
            }
            if (clan != Clan.PlayerClan)
            {
                foreach (Hero hero3 in clan.Heroes)
                {
                    if (hero3.IsActive && hero3.IsAlive && hero3.PartyBelongedTo == null && hero3.PartyBelongedToAsPrisoner == null && hero3.Age > (float)Campaign.Current.Models.AgeModel.HeroComesOfAge && (hero3.CharacterObject.Occupation == Occupation.Lord || hero3.CharacterObject.Occupation == Occupation.Wanderer))
                    {
                        float heroPartyCommandScore2 = GetHeroPartyCommandScore(hero3);
                        if (heroPartyCommandScore2 > num)
                        {
                            num = heroPartyCommandScore2;
                            hero = hero3;
                        }
                    }
                }
            }
            __result = hero;
            if (s.Debug && hero != null && hero.IsWanderer) InformationManager.DisplayMessage(new InformationMessage($"New leader is : {hero.Name}"));

            return false;
        }

        private static float GetHeroPartyCommandScore(Hero hero)
        {
            return 3f * (float)hero.GetSkillValue(DefaultSkills.Tactics) + 2f * (float)hero.GetSkillValue(DefaultSkills.Leadership) + (float)hero.GetSkillValue(DefaultSkills.Scouting) + (float)hero.GetSkillValue(DefaultSkills.Steward) + (float)hero.GetSkillValue(DefaultSkills.OneHanded) + (float)hero.GetSkillValue(DefaultSkills.TwoHanded) + (float)hero.GetSkillValue(DefaultSkills.Polearm) + (float)hero.GetSkillValue(DefaultSkills.Riding) + ((hero.Clan.Leader == hero) ? 1000f : 0f) + ((hero.GovernorOf == null) ? 500f : 0f);
        }
    }

    //Player Transfer Wanderers
    [HarmonyPatch(typeof(PartyScreenManager), nameof(PartyScreenManager.ClanManageTroopAndPrisonerTransferableDelegate))]
    static class ClanManageTroopAndPrisonerTransferableDelegate_Postfix
    {
        [HarmonyPostfix]
        static void Postfix(CharacterObject character, PartyScreenLogic.TroopType type, PartyScreenLogic.PartyRosterSide side, PartyBase LeftOwnerParty, ref bool __result)
        {
            var s = WanderersSettings.Instance;
            if (__result == true || !s.CompanionTransfer || !character.IsHero || character.HeroObject.IsPartyLeader) return;

            
            __result = true;
            if (s.Debug) InformationManager.DisplayMessage(new InformationMessage($"Transfer for {character.Name} is:{__result}"));
        }
    }
}
