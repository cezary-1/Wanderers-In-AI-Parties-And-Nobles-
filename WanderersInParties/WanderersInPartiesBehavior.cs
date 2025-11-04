using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace WanderersInParties
{

    internal class WanderersInPartiesBehavior : CampaignBehaviorBase
    {
        // Look through every ItemObject in the game…
        private MBReadOnlyList<ItemObject> allItems = MBObjectManager.Instance.GetObjectTypeList<ItemObject>();
        private readonly MBReadOnlyList<CultureObject> _cultures =
            MBObjectManager.Instance.GetObjectTypeList<CultureObject>();

        public WanderersInPartiesBehavior()
        {
            // Bind the MCM button to our method
            WanderersSettings.Instance.RefreshWanderers = RefreshAllTavernWanderers;
            WanderersSettings.Instance.ClearWanderers = ClearExcessWanderers;
        }

        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickClanEvent.AddNonSerializedListener(this, OnDailyTickClan);
            CampaignEvents.AfterSettlementEntered.AddNonSerializedListener(this, OnAfterSettlementEntered);
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
            CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, DailyTickSettlementEvent);
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoadedEvent);
            CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, OnGameLoadedEvent);
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, OnSettlementEntered);
            CampaignEvents.OnSettlementLeftEvent.AddNonSerializedListener(this, OnSettlementLeftEvent);
            CampaignEvents.BeforeHeroKilledEvent.AddNonSerializedListener(this, BeforeHeroKilled);
        }

        //Dialog

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            AddCompanionDialogOptions(starter);
            AddNobleDialogOptions(starter);
        }

        private void AddCompanionDialogOptions(CampaignGameStarter game)
        {
            var s = WanderersSettings.Instance;
            if (s.EnableMakeFamilyDialogues)
            {
                var family = "none";
                // 1) Root “family” invite
                game.AddPlayerLine(
                    "wp_family_root",
                    "hero_main_options",
                    "wp_family_menu",
                    "{=wp_family_root}I want to make you part of my family.",
                    new ConversationSentence.OnConditionDelegate(() =>
                        Hero.OneToOneConversationHero != null &&
                        Hero.OneToOneConversationHero.CompanionOf == Clan.PlayerClan &&
                        !Hero.OneToOneConversationHero.Children.Any(h => h.IsHumanPlayerCharacter) &&
                        !Hero.OneToOneConversationHero.Siblings.Any(h => h.IsHumanPlayerCharacter) &&
                        !Hero.MainHero.Children.Any(h => h == Hero.OneToOneConversationHero)
                    ),
                    null, 100, null, null
                );

                // 0) NPC prompt so the engine shows the submenu options
                game.AddDialogLine(
                    "wp_family_menu_npc",
                    "wp_family_menu",
                    "wp_family_menu",
                    "{=wp_family_menu_npc}Very well—how shall I join your family?", //they join to our
                    null,
                    null,
                    100,
                    null
                );

                // 2) The three explicit branches
                game.AddPlayerLine(
                    "wp_family_as_parent", "wp_family_menu", "wp_family_confirm",
                    "{=wp_family_as_parent}…as my parent.",
                    null,
                    new ConversationSentence.OnConsequenceDelegate(() =>
                        family = "parent"
                    ), 90, null, null
                );
                game.AddPlayerLine(
                    "wp_family_as_sibling", "wp_family_menu", "wp_family_confirm",
                    "{=wp_family_as_sibling}…as my sibling.",
                    null,
                    new ConversationSentence.OnConsequenceDelegate(() =>
                        family = "sibling"
                    ), 
                    80, 
                    new ConversationSentence.OnClickableConditionDelegate((out TextObject explanation) => 
                    {
                        explanation = TextObject.Empty;
                        if (Hero.MainHero.Father == null && Hero.MainHero.Mother == null && Hero.OneToOneConversationHero.Father == null && Hero.OneToOneConversationHero.Mother == null)
                        {
                            explanation = new TextObject("{=wp_family_sibling_error}Can't, no parents for both heroes.");
                            return false;
                        }
                        return true;
                    }),
                    null
                );
                game.AddPlayerLine(
                    "wp_family_as_child", "wp_family_menu", "wp_family_confirm",
                    "{=wp_family_as_child}…as my child.",
                    null,
                    new ConversationSentence.OnConsequenceDelegate(() =>
                        family = "child"
                    ), 70, null, null
                );
                // Cancel
                game.AddPlayerLine(
                    "wp_family_cancel", "wp_family_menu", "lord_pretalk",
                    "{=wp_family_cancel}Actually, never mind.",
                    null, null, 60, null, null
                );

                // 3) Confirmations
                game.AddDialogLine(
                    "wp_family_confirm", "wp_family_confirm", "lord_pretalk",
                    "{=wp_family_confirm}I am grateful to have become a member of your family", //npc says he is grateful
                    null,
                    new ConversationSentence.OnConsequenceDelegate(() =>
                    {
                        var npc = Hero.OneToOneConversationHero;
                        if (npc == null) return;

                        switch (family)
                        {
                            case "parent":

                                if (npc.IsFemale) Hero.MainHero.Mother = npc;
                                else Hero.MainHero.Father = npc;

                                break;

                            case "sibling":
                                if (Hero.MainHero.Father != null) npc.Father = Hero.MainHero.Father;
                                else if (npc.Father != null) Hero.MainHero.Father = npc.Father;
                                if (Hero.MainHero.Mother != null) npc.Mother = Hero.MainHero.Mother;
                                else if (npc.Mother != null) Hero.MainHero.Mother = npc.Mother;

                                if (npc.Mother != Hero.MainHero.Mother && npc.Father != Hero.MainHero.Father)
                                {
                                    InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=wp_family_sibling_error}Can't, no parents for both heroes.")
                                        .ToString()));
                                }

                                break;

                            case "child":
                                if (Hero.MainHero.IsFemale) npc.Mother = Hero.MainHero;
                                else npc.Father = Hero.MainHero;

                                break;
                        }
                        if (npc.IsPlayerCompanion) npc.CompanionOf = null;

                        if (s.ChangeName) npc.SetName(npc.FirstName, npc.FirstName);

                        npc.Clan = Clan.PlayerClan;

                        if (npc.Occupation != Occupation.Lord)
                        {
                            npc.SetNewOccupation(Occupation.Lord);
                        }

                    }),
                    100, null
                );
            }

            if (s.EnableMakeNobleDialogue)
            {
                // 4) Promote to noble
                game.AddPlayerLine(
                    "wp_comp_promote_root", "hero_main_options", "wp_comp_promote_menu",
                    "{=wp_comp_promote_root}I want you to be made a noble.",
                    new ConversationSentence.OnConditionDelegate(() =>
                        Hero.OneToOneConversationHero != null &&
                        Hero.OneToOneConversationHero.CompanionOf == Clan.PlayerClan &&
                        Hero.OneToOneConversationHero.Occupation != Occupation.Lord
                    ),
                    null, 50, null, null
                );
                game.AddDialogLine(
                    "wp_promote_npc", "wp_comp_promote_menu", "wp_comp_promote_menu",
                    "{=wp_promote_npc}Are you sure, my Lord?",
                    null, null, 40, null
                );
                game.AddPlayerLine(
                    "wp_promote_confirm", "wp_comp_promote_menu", "wp_promote_done",
                    "{=wp_promote_confirm}Rise up—henceforth you are a noble of my realm!",
                    null, null, 40, null, null
                );
                game.AddPlayerLine(
                    "wp_promote_cancel", "wp_comp_promote_menu", "lord_pretalk",
                    "{=wp_promote_cancel}On second thought, never mind.",
                    null, null, 40, null, null
                );
                game.AddDialogLine(
                    "wp_promote_done", "wp_promote_done", "lord_pretalk",
                    "{=wp_promote_done}Thank you my lord, I won't disappoint you.",
                    new ConversationSentence.OnConditionDelegate(() => true),
                    new ConversationSentence.OnConsequenceDelegate(() =>
                    {
                        var npc = Hero.OneToOneConversationHero;
                        if (npc == null) return;
                        if (npc.IsPlayerCompanion) npc.CompanionOf = null;

                        if (s.ChangeName) npc.SetName(npc.FirstName, npc.FirstName);

                        npc.Clan = Clan.PlayerClan;
                        npc.SetNewOccupation(Occupation.Lord);
                    }),
                    100, null
                );
            }

        }

        private void AddNobleDialogOptions(CampaignGameStarter game)
        {
            var s = WanderersSettings.Instance;
            if (s == null) return;
            if (!s.EnableSpeakNobleDialogue) return;
            // 1) Root ask if you can speak with other hero
            game.AddPlayerLine(
                "wp_other_hero_root",
                "hero_main_options",
                "wp_other_hero_menu",
                "{=wp_other_hero_root}I want to speak with other member of your party.",
                new ConversationSentence.OnConditionDelegate(() =>
                    PlayerEncounter.EncounteredMobileParty != null &&
                    Hero.OneToOneConversationHero != null &&
                    Hero.OneToOneConversationHero.PartyBelongedTo != null &&
                    Hero.OneToOneConversationHero.PartyBelongedTo != MobileParty.MainParty &&
                    Hero.OneToOneConversationHero.PartyBelongedTo.MemberRoster.TotalHeroes > 1
                ),
                new ConversationSentence.OnConsequenceDelegate(() =>
                ShowSearchInquiry()
                ), 100, null, null
            );
            game.AddDialogLine(
                "wp_other_hero_menu_npc",
                "wp_other_hero_menu",
                "hero_main_options",
                "{=wp_other_hero_menu_npc}Very well – who would you like to talk to?", //they ask
                null,
                null,
                100,
                null
            );
        }
        
        //Inquiry
        private void ShowSearchInquiry()
        {
            var speaker = Hero.OneToOneConversationHero;
            if (speaker == null) return;
            var party = speaker.PartyBelongedTo;
            if(party == null ) return;
            var heroes = party.MemberRoster.GetTroopRoster().Where(h => h.Character.IsHero && h.Character.HeroObject != speaker).Select(h => h.Character.HeroObject).ToList();

            var elems = new List<InquiryElement>();

            // Add heroes first
            foreach (var h in heroes)
            {
                elems.Add(new InquiryElement(
                    h.StringId,
                    h.Name.ToString(),
                    new ImageIdentifier(CharacterCode.CreateFrom(h.CharacterObject)),
                    true,
                    ""
                ));
            }

            var resultData = new MultiSelectionInquiryData(
                "",
                "",
                elems,
                true,
                1,
                1,
                GameTexts.FindText("str_ok").ToString(),
                GameTexts.FindText("str_cancel").ToString(),

                // onConfirm
                list =>
                {
                    var id = (string)list[0].Identifier;
                    var chosenHero = heroes.FirstOrDefault(x => x.StringId == id);
                    if (chosenHero == null) return;
                    
                    // End the current encounter if needed (keeps behaviour from your earlier attempt)
                    if (PlayerEncounter.Current != null)
                        PlayerEncounter.LeaveEncounter = true;

                    // Open a new campaign-map conversation between player and the chosen hero:
                    
                    CampaignMapConversation.OpenConversation(
                        new ConversationCharacterData(CharacterObject.PlayerCharacter),
                        new ConversationCharacterData(chosenHero.CharacterObject)
                    );
                },

                // onCancel
                _ => { }
            );

            MBInformationManager.ShowMultiSelectionInquiry(resultData);
        }

        //companions in settlements fix
        public void OnSettlementEntered(MobileParty mobileParty, Settlement settlement, Hero hero)
        {
            if (LocationComplex.Current != null && PlayerEncounter.LocationEncounter != null)
            {
                if (mobileParty != null)
                {
                    if (mobileParty == MobileParty.MainParty)
                    {
                        AddOtherPartiesHeroesFromSettlement(settlement);
                        return;
                    }
                    if (MobileParty.MainParty.CurrentSettlement == settlement && mobileParty != MobileParty.MainParty && (settlement.IsFortification || settlement.IsVillage))
                    {
                        AddOtherPartiesHeroesToSettlement(mobileParty, settlement);
                        return;
                    }
                }
            }
        }

        // Token: 0x0600370C RID: 14092 RVA: 0x000F7218 File Offset: 0x000F5418
        public void OnSettlementLeftEvent(MobileParty mobileParty, Settlement settlement)
        {
            if (mobileParty != null && mobileParty != MobileParty.MainParty && MobileParty.MainParty.CurrentSettlement == settlement && mobileParty.LeaderHero != null && LocationComplex.Current != null)
            {
                Hero leaderHero = mobileParty.LeaderHero;
                var roster = mobileParty.MemberRoster.GetTroopRoster().ToList();
                foreach (var trElem in roster)
                {
                    if (!trElem.Character.IsHero) continue;
                    var hero = trElem.Character.HeroObject;
                    if (hero == null) continue;
                    if (hero == leaderHero || hero == Hero.MainHero) continue;
                    if (!hero.IsAlive || !hero.IsActive) continue;
                    if (hero.PartyBelongedToAsPrisoner != null) continue;
                    if (hero.PartyBelongedTo != mobileParty) continue; // safety check

                    Location locationOfCharacter = LocationComplex.Current.GetLocationOfCharacter(hero);
                    if (locationOfCharacter != null)
                    {
                        locationOfCharacter.RemoveCharacter(hero);
                    }
                }

            }
        }

        //Events

        private void DailyTickSettlementEvent(Settlement settlement)
        {
            try
            {

                var s = WanderersSettings.Instance;
                if (!s.TavernSpawn || settlement == null) return;
                if (settlement.IsUnderSiege) return;

                if (settlement.IsTown)
                    EnforceTavern(settlement);
            }
            catch (Exception e)
            {
                InformationManager.DisplayMessage(
                    new InformationMessage($"[WIP] Error in DailyTickSettleemntEvent: {e.Message}")
                );
            }


        }

        private void OnGameLoadedEvent(CampaignGameStarter starter)
        {
            try
            {

                var s = WanderersSettings.Instance;
                if (!s.TavernSpawn) return;
                var towns = Town.AllTowns.ToList();
                foreach (var town in towns)
                {
                    EnforceTavern(town.Settlement);
                }
            }
            catch (Exception e)
            {
                InformationManager.DisplayMessage(
                    new InformationMessage($"[WIP] Error in OnGameLoadedEvent: {e.Message}")
                );
            }

        }

        private void OnAfterSettlementEntered(MobileParty party, Settlement settlement, Hero hero)
        {
            try
            {
                //heroes, lords only
                if (hero == null || hero.Clan == null || !hero.IsLord || hero.IsHumanPlayerCharacter || hero.Clan == Clan.PlayerClan) return;
                var s = WanderersSettings.Instance;

                // Only clan leaders (for recruit)
                if (hero == hero.Clan.Leader && s.RecruitmentProbability > 0)
                {

                    var wanderers = settlement.HeroesWithoutParty.Where(h => h.IsWanderer && h.Clan == null && EligibleWanderer(h, hero.Clan)).OrderByDescending(h=> h.Culture == hero.Clan.Culture).ToList();
                    if(wanderers != null)
                    TryRecruitFromSettlement(hero.Clan, wanderers);
                }

                if (party != null || settlement.IsTown)
                {
                    var companions = hero.Clan.Heroes.Where(h => h.IsWanderer && h.CurrentSettlement == settlement && h?.PartyBelongedTo == party).ToList();
                    foreach (var companion in companions)
                    {

                        // First try to buy missing gear
                        if (s.BuyChancePerPiece > 0)
                            TryBuyEquipment(companion, settlement);

                        // Then try to upgrade one piece
                        if (s.UpgradeChancePerPiece > 0)
                            TryUpgradeEquipment(companion);


                    }
                }
            }
            catch (Exception e)
            {
                InformationManager.DisplayMessage(
                    new InformationMessage($"[WIP] Error in OnSettlementEntered: {e.Message}")
                );
            }


        }

        
        private void OnDailyTickClan(Clan clan)
        {
            try
            {
                if (clan == null || clan == Clan.PlayerClan || clan.IsBanditFaction) return;
                var s = WanderersSettings.Instance;
                //nobles in parties
                //leave
                if (s.NoblesParty)
                {
                    int partylimit = Campaign.Current.Models.ClanTierModel.GetPartyLimitForTier(clan, clan.Tier);
                    int count = clan.WarPartyComponents.Count;
                    if(count > 0)
                    {
                        var noblesInParties = clan.Heroes.Where(h => h.IsActive && h.IsAlive && h.GovernorOf == null && !h.IsPartyLeader && h.PartyBelongedTo != null && h.PartyBelongedToAsPrisoner == null && h.Age > (float)Campaign.Current.Models.AgeModel.HeroComesOfAge && h.CharacterObject.Occupation == Occupation.Lord).Where(h=> s.NoblesPartyLeaveConsider * h.PartyBelongedTo.LimitedPartySize <= h.PartyBelongedTo.MemberRoster.TotalManCount).OrderByDescending(n => n.GetTraitLevel(DefaultTraits.Commander)).ToList();
                        if (noblesInParties != null && noblesInParties.Count > 0)
                        {
                            var needRemove = noblesInParties.Where(n => !EligibleNoble(n)).ToList();
                            if(needRemove != null && needRemove.Count > 0)
                            {
                                foreach (var n in needRemove)
                                {
                                    var party = n.PartyBelongedTo;

                                    // only remove from *ready* parties
                                    if (party.IsActive)
                                    {
                                        party.MemberRoster.RemoveTroop(n.CharacterObject);
                                        if (s.Debug) InformationManager.DisplayMessage(new InformationMessage($"{n.Name} was removed from the: {party.Name}"));
                                    }
                                }
                                noblesInParties = clan.Heroes.Where(h => h.IsActive && h.IsAlive && h.GovernorOf == null && !h.IsPartyLeader && h.PartyBelongedTo != null && h.PartyBelongedToAsPrisoner == null && h.Age > (float)Campaign.Current.Models.AgeModel.HeroComesOfAge && h.CharacterObject.Occupation == Occupation.Lord).Where(h => s.NoblesPartyLeaveConsider * h.PartyBelongedTo.LimitedPartySize <= h.PartyBelongedTo.MemberRoster.TotalManCount).OrderByDescending(n => n.GetTraitLevel(DefaultTraits.Commander)).ToList();
                            }

                            if(noblesInParties != null && noblesInParties.Count > 0)
                            {
                                var noblesCanLead = clan.Heroes.Where(h => h.IsActive && h.IsAlive && h.GovernorOf == null && h.PartyBelongedTo == null && h.PartyBelongedToAsPrisoner == null && h.Age > (float)Campaign.Current.Models.AgeModel.HeroComesOfAge && h.CharacterObject.Occupation == Occupation.Lord).OrderByDescending(n => n.GetTraitLevel(DefaultTraits.Commander)).ToList();
                                // pick the top‐ranked potential leaders
                                var bestInParty = noblesInParties.FirstOrDefault();
                                var bestCanLead = noblesCanLead?.FirstOrDefault();
                                var clanLeader = noblesInParties.FirstOrDefault(h => h.IsClanLeader) ?? null;


                                bool modeClanLeader = count < partylimit && clanLeader != null && clanLeader.IsClanLeader;
                                bool mode1 = count < partylimit && s.NoblesPartyLeave == 1 && !noblesCanLead.Any();
                                bool mode2 = count < partylimit && s.NoblesPartyLeave == 2 && (
                                                   !noblesCanLead.Any()
                                                   || (
                                                        bestInParty != null
                                                     && bestCanLead != null
                                                     && bestInParty.GetTraitLevel(DefaultTraits.Commander)
                                                        > bestCanLead.GetTraitLevel(DefaultTraits.Commander)
                                                      )
                                                );
                                bool mode3 = s.NoblesPartyLeave == 3;

                                if (modeClanLeader)
                                {
                                    var party = clanLeader.PartyBelongedTo;
                                    if (party != null)
                                    {
                                        // only remove from *ready* parties
                                        if (party.IsActive)
                                        {
                                            party.MemberRoster.RemoveTroop(clanLeader.CharacterObject);
                                            if (s.Debug) InformationManager.DisplayMessage(new InformationMessage($"{clanLeader.Name} was removed from the: {party.Name}"));
                                        }
                                    }


                                }
                                else if (mode1 || mode2)
                                {
                                    var desired = partylimit - count;
                                    int toRemove = Math.Min(noblesInParties.Count, desired);
                                    var removeList = noblesInParties.Take(toRemove).ToList();
                                    foreach(var w in removeList)
                                    {
                                        if (MBRandom.RandomFloat >= s.NoblesPartyLeaveChance) continue;
                                        var party = w.PartyBelongedTo;
                                        if (party == null) continue;

                                        // only remove from *ready* parties
                                        if (party.IsActive)
                                        {
                                            party.MemberRoster.RemoveTroop(w.CharacterObject);
                                            if (s.Debug) InformationManager.DisplayMessage(new InformationMessage($"{w.Name} was removed from the: {party.Name}"));
                                        }
                                    }
                                }
                                else if (mode3)
                                {
                                    int toRemove = MBRandom.RandomInt(1, noblesInParties.Count);
                                    var removeList = noblesInParties.Take(toRemove).ToList();
                                    foreach (var w in removeList)
                                    {
                                        if (MBRandom.RandomFloat >= s.NoblesPartyLeaveChance) continue;
                                        var party = w.PartyBelongedTo;
                                        if (party == null) continue;

                                        // only remove from *ready* parties
                                        if (party.IsActive)
                                        {
                                            party.MemberRoster.RemoveTroop(w.CharacterObject);
                                            if (s.Debug) InformationManager.DisplayMessage(new InformationMessage($"{w.Name} was removed from the: {party.Name}"));
                                        }
                                    }
                                }
                            }

                        }
                        
                    }
                    //join
                    if (count >= partylimit || s.NoblesPartyLimit)
                    {
                        var nobles = clan.Heroes
                            .Where(h => h.IsActive && h.IsAlive && h.GovernorOf == null && h.PartyBelongedTo == null && h.PartyBelongedToAsPrisoner == null && h.Age > (float)Campaign.Current.Models.AgeModel.HeroComesOfAge && h.CharacterObject.Occupation == Occupation.Lord)
                            .Where(h => EligibleNoble(h))
                            .OrderByDescending(n => n.GetTraitLevel(DefaultTraits.Fighter))
                            .ThenByDescending(n=> n.Age).ToList();
                        foreach (var n in nobles)
                        {
                            if (!s.NoblesPartyClanLeader && n.IsClanLeader) continue;

                            if (MBRandom.RandomFloat >= s.NoblesPartyChance) continue;
                            WandererJoin(n, n.Clan);

                            if (n.PartyBelongedTo == null) break;
                        }
                    }
                }

                var heroes = clan.Heroes.Where(h => h.IsWanderer && !h.Clan.Leader.IsHumanPlayerCharacter && h.IsAlive).ToList();
                if (heroes == null || heroes.Count == 0) return;

                var current = heroes.Count;
                var maxWanderers = s.MaxWanderersClan + (clan.Tier * s.MaxWanderersClanTier);

                if (s.Debug) InformationManager.DisplayMessage(new InformationMessage($"Wanderers in {clan.Name} amount:{current}"));

                var wandRemove = current - maxWanderers;
                var AllNobles = clan.Heroes.Where(h => !h.IsChild && h.IsAlive && h.IsLord).ToList();
                var noblesCount = 0;
                if (AllNobles != null && AllNobles.Count > 0) noblesCount = AllNobles.Count;

                var noblesLimit = s.NoblesPerClan + (s.NoblesPerClanTier * clan.Tier);
                foreach (var hero in heroes)
                {
                    if(wandRemove > 0)
                    {
                        CleanupHeroRoles(hero);
                        wandRemove--;
                        continue; // skip rest
                    }

                    // Try promotion
                    if (MBRandom.RandomFloat < s.PromotionChance && noblesCount < noblesLimit)
                    {
                        if(s.ChangeName) hero.SetName(hero.FirstName, hero.FirstName);
                        
                        hero.SetNewOccupation(Occupation.Lord);
                        GiveNobleKit(hero);
                        continue; // no firing same day
                    }

                    // Try firing
                    if ((MBRandom.RandomFloat < s.FiringChance || !EligibleWanderer(hero, hero.Clan)) && CanTransferHero(hero))
                    {
                        CleanupHeroRoles(hero);
                        continue; // no party joining same day
                    }
                    var party = hero.PartyBelongedTo;
                    if (party == null && hero.HeroState == Hero.CharacterStates.Active && hero.IsWanderer && EligibleWanderer(hero, hero.Clan))
                    {
                        if(s.Debug) InformationManager.DisplayMessage(new InformationMessage($"We check rejoining for {hero.Name}"));
                        WandererJoin(hero, hero.Clan);
                    }
                    else if(party != null && hero.IsWanderer && !EligibleWanderer(hero, hero.Clan))
                    {
                        // only remove from *ready* parties
                        if (party.IsActive)
                        {
                            party.MemberRoster.RemoveTroop(hero.CharacterObject);
                            if (s.Debug) InformationManager.DisplayMessage(new InformationMessage($"{hero.Name} was removed from the: {party.Name}"));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                InformationManager.DisplayMessage(
                new InformationMessage($"[WIP] Error in OnDailyTickClan: {e.Message}")
                );
            }

        }


        private void BeforeHeroKilled(Hero victim, Hero killer, KillCharacterAction.KillCharacterActionDetail detail, bool showNotification)
        {
            try
            {
                if (victim == null || !victim.IsLord) return;

                var destroyedClan = victim.Clan;
                var s = WanderersSettings.Instance;

                if (destroyedClan == null || destroyedClan == Clan.PlayerClan || destroyedClan.Leader != victim || s == null) return;

                // Are there any true nobles left? (exclude wanderers and the fallen)
                bool anyRealNoblesLeft = destroyedClan.Heroes
                    .Any(h => victim != h && h.IsAlive && h.IsLord && !h.IsWanderer && !h.IsChild);


                if (anyRealNoblesLeft)
                    return;  // still other nobles, nothing to do

                // Otherwise, find all wanderers in this clan
                var wanderers = destroyedClan.Heroes
                    .Where(h => h.IsWanderer && h.IsAlive)
                    .ToList();


                if (wanderers == null || wanderers.Count == 0)
                    return;  // no wanderers to elevate



                if (s.SaveClan)
                {
                    // Pick the highest‐level wanderer
                    var best = wanderers
                        .OrderByDescending(h => h.Level)
                        .First();

                    // Promote them
                    if (s.ChangeName) best.SetName(best.FirstName, best.FirstName);
                    best.SetNewOccupation(Occupation.Lord);
                    GiveNobleKit(best);
                    destroyedClan.SetLeader(best);
                    


                }
                else
                {
                    foreach (var w in wanderers)
                    {
                        CleanupHeroRoles(w);
                    }
                }

            }
            catch (Exception e)
            {
                InformationManager.DisplayMessage(
                    new InformationMessage($"[WIP] Error in BeforeHeroKilled: {e.Message}")
                );
            }
        }


        private void TryRecruitFromSettlement(Clan clan, IEnumerable<Hero> wanderers)
        {
            var s = WanderersSettings.Instance;

            if (s.RecruitmentProbability <= 0 || s.MaxWanderersClan <= 0) return;

            // snapshot so we can safely modify the source during recruitment:
            var candidates = wanderers.ToList();
            // How many wanderers already in clan?
            var maxWanderers = s.MaxWanderersClan + (clan.Tier * s.MaxWanderersClanTier);
            int current = clan.Heroes.Count(h => h.IsWanderer && h.IsAlive);

            if(s.Debug) InformationManager.DisplayMessage(new InformationMessage($"Amount of wanderers in {clan.Name}: {current},  Max = {maxWanderers}"));

            if (current >= maxWanderers) return;

            foreach (var wanderer in candidates)
            {
                if (current >= maxWanderers) break;
                if (MBRandom.RandomFloat < s.RecruitmentProbability)
                {
                    
                    int level = wanderer.Level;
                    var equipment = wanderer.BattleEquipment;
                    int equipmentValue = 0;
                    for (int i = 0; i < Equipment.EquipmentSlotLength; i++)
                    {
                        var item = equipment[i];
                        if (item.IsEmpty) continue;
                        equipmentValue += item.ItemValue;
                    }


                    
                    int cost = (int)(level * equipmentValue * s.CostMultiplier);

                    if (clan.Leader.Gold >= cost)
                    {
                        
                        clan.Leader.Gold -= cost;
                        WandererJoin(wanderer, clan);
                        if(s.TroopKit) GiveTroopKit(wanderer, clan);
                        current++;
                    }
                }
            }
        }

        private void WandererJoin(Hero wanderer, Clan newClan)
        {
            var s = WanderersSettings.Instance;
            if (s == null) return;
            // 1) Assign to clan & update home
            if (wanderer.Clan != newClan)
            {
                wanderer.Clan = newClan;
                wanderer.UpdateHomeSettlement();
            }

            var limit = 0;
            if (wanderer.IsWanderer) limit = s.PartyHeroes;
            else if (wanderer.IsLord) limit = s.NoblesPartyHeroes;

            

                // 2) Pick a random active clan party, or fallback to leader’s
                var clanParties = Campaign.Current.MobileParties
                    .Where(p =>
                        p.IsActive &&
                        p.IsLordParty &&
                        p.LeaderHero != null &&
                        p.LeaderHero.Clan == newClan
                    )
                    .Where(p =>
                    {
                        if (wanderer.IsWanderer)
                            return p.MemberRoster.TotalHeroes < s.PartyHeroes;
                        else if (wanderer.IsLord)
                            return p.MemberRoster.TotalHeroes < s.NoblesPartyHeroes;

                        return false;
                    })
                    .ToList();

            MobileParty targetParty = null;
            if (clanParties.Count > 0)
            {
                // choose one at random
                var parties = clanParties
                    .Where(p => p.LimitedPartySize > p.MemberRoster.TotalManCount)
                    .OrderBy(p => p.MemberRoster.TotalManCount)
                    .ToList();
                if(parties.Count == 1)
                {
                    targetParty = parties[0];
                }
                else if(parties.Count > 0)
                {

                    if (wanderer.IsWanderer)
                    {
                        if (s.WhichParty == 1)
                        {
                            targetParty = parties.OrderBy(p=> p.MemberRoster.TotalRegulars).FirstOrDefault();
                        }
                        else if (s.WhichParty == 2)
                        {
                            targetParty = parties.OrderBy(p => p.MemberRoster.TotalHeroes).FirstOrDefault();
                        }
                        else if (s.WhichParty == 3)
                        {
                            targetParty = parties.OrderByDescending(p => p.LeaderHero.IsClanLeader).FirstOrDefault();
                        }
                        else if (s.WhichParty == 4)
                        {
                            targetParty = parties.OrderByDescending(p => p.LeaderHero.GetRelation(wanderer)).FirstOrDefault();
                        }

                    }

                    if (wanderer.IsLord)
                    {
                        if (s.NoblesWhichParty == 1)
                        {
                            targetParty = parties.OrderBy(p => p.MemberRoster.TotalRegulars).FirstOrDefault();
                        }
                        else if (s.NoblesWhichParty == 2)
                        {
                            targetParty = parties.OrderBy(p => p.MemberRoster.TotalHeroes).FirstOrDefault();
                        }
                        else if (s.NoblesWhichParty == 3)
                        {
                            targetParty = parties.OrderByDescending(p => p.LeaderHero.IsClanLeader).FirstOrDefault();
                        }
                        else if (s.NoblesWhichParty == 4)
                        {
                            targetParty = parties.OrderByDescending(p => p.LeaderHero.GetRelation(wanderer)).FirstOrDefault();
                        }

                    }

                    if (targetParty == null)
                    targetParty = parties[MBRandom.RandomInt(parties.Count)];

                }
                else
                {
                    targetParty = clanParties[MBRandom.RandomInt(clanParties.Count)];
                }

            }

            if (targetParty == null)
            {
                if (s.FireNoParty && wanderer.IsWanderer && CanTransferHero(wanderer))
                {
                    CleanupHeroRoles(wanderer);
                }
                return;
            }

            // 3) Add to that party’s roster if not already present
            var roster = targetParty.MemberRoster;
            if (roster != null && !roster.Contains(wanderer.CharacterObject))
            {
                    targetParty.AddElementToMemberRoster(wanderer.CharacterObject, 1);
                if (s.Debug) InformationManager.DisplayMessage(new InformationMessage($"{wanderer.Name} joined the: {targetParty.Name}"));

                bool can = wanderer.IsLord || (s.LeadParty && wanderer.IsWanderer);

                if(s.NoblesPartySwap != 0 && !targetParty.LeaderHero.IsClanLeader && can) 
                {
                    if (wanderer.IsClanLeader) targetParty.ChangePartyLeader(wanderer);
                    else if(s.NoblesPartySwap == 2 && wanderer.GetTraitLevel(DefaultTraits.Commander) > targetParty.LeaderHero.GetTraitLevel(DefaultTraits.Commander)) targetParty.ChangePartyLeader(wanderer);
                    else if (s.NoblesPartySwap == 2 && wanderer.GetTraitLevel(DefaultTraits.Commander) == targetParty.LeaderHero.GetTraitLevel(DefaultTraits.Commander) && wanderer.Age > targetParty.LeaderHero.Age) targetParty.ChangePartyLeader(wanderer);
                    else if (s.NoblesPartySwap == 3 && wanderer.Age > targetParty.LeaderHero.Age) targetParty.ChangePartyLeader(wanderer);
                    else if (s.NoblesPartySwap == 3 && wanderer.Age == targetParty.LeaderHero.Age && wanderer.GetTraitLevel(DefaultTraits.Commander) > targetParty.LeaderHero.GetTraitLevel(DefaultTraits.Commander)) targetParty.ChangePartyLeader(wanderer);

                    if(s.Debug && targetParty.LeaderHero == wanderer)
                    {
                        InformationManager.DisplayMessage(new InformationMessage($"{wanderer.Name} became new leader of: {targetParty.Name}"));
                    }
                }
            }
        }

        public static void CleanupHeroRoles(Hero hero)
        {
            // Unassign as governor
            if (hero.GovernorOf != null)
            {
                ChangeGovernorAction.RemoveGovernorOf(hero);
            }

            if (hero.PartyBelongedTo != null)
            {
                MobileParty party = hero.PartyBelongedTo;
                if (party.Army != null && party.Army.LeaderParty == party)
                {
                    DisbandArmyAction.ApplyByUnknownReason(party.Army);
                }
                party.Army = null;

                if (party.Party.IsActive && party.Party.LeaderHero == hero)
                {
                    DisbandPartyAction.StartDisband(party);
                    party.Party.SetCustomOwner(null);
                    DestroyPartyAction.Apply(null, party); // test
                }
                else if (party.IsActive)
                {
                    party.MemberRoster.RemoveTroop(hero.CharacterObject);
                }
            }



            if (hero.CompanionOf != null)
            {
                hero.CompanionOf = null;
            }

            if (hero.BornSettlement == null)
            {
                hero.BornSettlement = SettlementHelper.FindRandomSettlement((Settlement x) => x.IsTown);
            }

            hero.Clan = null;
        }

        //eligible noble
        private bool EligibleNoble(Hero hero)
        {
            var s = WanderersSettings.Instance;
            if (hero.IsPregnant && !s.NoblesPartyPregnant) return false;
            if (hero.Age < s.NoblesPartyAge) return false;

            if (hero.IsFemale && s.NoblesPartyGender == 1) return false;
            else if (!hero.IsFemale && s.NoblesPartyGender == 2) return false;


            return true;
        }

        //eligible wanderer
        private bool EligibleWanderer(Hero hero, Clan clan)
        {
            var s = WanderersSettings.Instance;
            if (hero.IsPregnant && !s.PartyPregnant) return false;
            if (hero.Age < s.PartyAge) return false;

            if (hero.IsFemale && s.PartyGender == 1) return false;
            else if (!hero.IsFemale && s.PartyGender == 2) return false;

            if(clan != null)
            {
                if(s.SameCulture && hero.Culture != clan.Culture) return false;
            }


            return true;
        }

        public bool CanTransferHero(Hero hero)
        {
            if (hero.PartyBelongedTo != null && (hero.PartyBelongedTo.MapEvent != null || hero.PartyBelongedTo.SiegeEvent != null))
            {
                return false;
            }
            return true;
        }

        // 3) Implement TryBuyEquipment:
        private void TryBuyEquipment(Hero wanderer, Settlement settlement)
        {

            var equip = wanderer.BattleEquipment;

            // 1) Collect empty slots, *excluding* HorseHarness if no mount
            var emptySlots = new List<int>();
            for (int i = 0; i < Equipment.EquipmentSlotLength; i++)
            {
                if (!equip[i].IsEmpty)
                    continue;

                var slot = (EquipmentIndex)i;
                // If this is the harness slot, skip it unless the wanderer already has a mount
                if (slot == EquipmentIndex.HorseHarness)
                {
                    var hasMount = wanderer.CharacterObject.HasMount();

                    if (!hasMount)
                        continue;
                }

                emptySlots.Add(i);
            }
            if (emptySlots.Count == 0) return;

            var s = WanderersSettings.Instance;
            // 2) Pick one at random
            int slotIndex = emptySlots[MBRandom.RandomInt(emptySlots.Count)];
            if (MBRandom.RandomFloat >= s.BuyChancePerPiece) return;
            var eqSlot = (EquipmentIndex)slotIndex;

            var leader = wanderer.Clan.Leader;
            int minValue = (int)(leader.Gold * s.MinValue);
            int maxValue = (int)(leader.Gold * s.MaxValue);

            // 3) Gather all valid town‑stock candidates
            var townCandidates = new List<EquipmentElement>();
            foreach (var item in allItems)
            {
                if (!Equipment.IsItemFitsToSlot(eqSlot, item))
                    continue;
                // respect culture?
                if (s.RespectCulture && item.Culture != wanderer.Culture)
                    continue;
                // price range
                if (item.Value < minValue || item.Value > maxValue)
                    continue;
                // horse slot: only true mounts
                if (eqSlot == EquipmentIndex.Horse)
                {
                    // only true mounts
                    var horseComp = item.HorseComponent;
                    if (horseComp == null || !horseComp.IsMount)
                        continue;
                }
                if (settlement.ItemRoster.GetItemNumber(item) > 0)
                {
                    townCandidates.Add(new EquipmentElement(item));
                }
            }

            // 4) Pick one at random (if any)
            EquipmentElement chosen;
            if (townCandidates.Count > 0)
            {
                chosen = townCandidates[MBRandom.RandomInt(townCandidates.Count)];
            }
            else
            {
                chosen = default; // or fall back as before
            }

            // 4) Buy & equip if valid
            if (!chosen.IsEmpty && leader.Gold >= (chosen.ItemValue * s.ItemCostMultiplier) )
            {
                leader.Gold -= (int)(chosen.ItemValue * s.ItemCostMultiplier);
                equip.AddEquipmentToSlotWithoutAgent(eqSlot, chosen);
                if (s.Debug) InformationManager.DisplayMessage(new InformationMessage($"Bought {chosen.Item.Name} for {wanderer.Name}") );
            }
        }


        // 3) The upgrade helper:
        private void TryUpgradeEquipment(Hero wanderer)
        {
            var equip = wanderer.BattleEquipment;
            // Collect indices of non‑empty slots
            var filled = new List<int>();
            for (int i = 0; i < Equipment.EquipmentSlotLength; i++)
            {
                if (!equip[i].IsEmpty)
                    filled.Add(i);
            }

            if (filled.Count == 0) return;

            var s = WanderersSettings.Instance;
            // Pick a random slot
            int slotIndex = filled[MBRandom.RandomInt(filled.Count)];
            if (MBRandom.RandomFloat < s.UpgradeChancePerPiece)
            {
                var oldItem = equip[slotIndex];
                
                EquipmentIndex eqSlot = (EquipmentIndex)slotIndex;
                var leader = wanderer.Clan.Leader;
                var newItem = GetBetterEquipmentElement(oldItem, eqSlot, leader.Gold, wanderer.CurrentSettlement, wanderer);
                if (!newItem.IsEmpty && leader.Gold >= (newItem.ItemValue * s.ItemCostMultiplier ))
                {
                    leader.Gold -= (int)(newItem.ItemValue * s.ItemCostMultiplier);
                    equip.AddEquipmentToSlotWithoutAgent(eqSlot, newItem);
                    if (s.Debug) InformationManager.DisplayMessage(new InformationMessage($"Upgraded {oldItem.Item.Name} to {newItem.Item.Name} for {wanderer.Name}"));
                }
            }
        }

        private EquipmentElement GetBetterEquipmentElement(EquipmentElement oldItem, EquipmentIndex slotIndex, int leaderGold, Settlement settlement, Hero hero)
        {
            // Determine if this slot is a weapon slot
            bool isWeaponSlot = slotIndex == EquipmentIndex.Weapon0
                             || slotIndex == EquipmentIndex.Weapon1
                             || slotIndex == EquipmentIndex.Weapon2
                             || slotIndex == EquipmentIndex.Weapon3;


            WeaponClass? desiredClass = null;
            if (isWeaponSlot)
            {
                if (oldItem.Item.WeaponComponent != null)
                    desiredClass = oldItem.Item.WeaponComponent.PrimaryWeapon.WeaponClass;
            }

            var s = WanderersSettings.Instance;
            //max and min value of item
            int minValue = (int)(leaderGold * s.MinValue);
            int maxValue = (int)(leaderGold * s.MaxValue);            // Keep only those that actually fit into our slot
            var candidates = allItems
                .Where(item => Equipment.IsItemFitsToSlot(slotIndex, item) && settlement.ItemRoster.GetItemNumber(item) > 0)
                .Where(item =>
                {
                    if (s.RespectCulture && item.Culture != hero.Culture)
                        return false;

                    return true;
                })
                .Where(item =>
                {
                    if (slotIndex != EquipmentIndex.Horse)
                        return true;
                    var hc = item.HorseComponent;
                    return hc != null && hc.IsMount;
                })
                .Where(item =>
                {
                    if (!isWeaponSlot) return true;
                    var wc = item.WeaponComponent?.PrimaryWeapon?.WeaponClass;
                    return wc.HasValue && desiredClass.HasValue && wc.Value == desiredClass.Value;
                })
                .Select(item => new EquipmentElement(item))
                .Where(elem => !elem.IsEmpty && elem.ItemValue > oldItem.ItemValue && elem.ItemValue >= minValue && elem.ItemValue <= maxValue)
                .OrderBy(elem => elem.ItemValue)
                .ToList();

            if (candidates.Count == 0)
                return default;

            // Return the cheapest strictly better one, or default (empty) if none
            return candidates[MBRandom.RandomInt(candidates.Count)];
        }

        public static int GetHeroTier(CharacterObject character)
        {
            return MathF.Min(MathF.Max(MathF.Ceiling(((float)character.Level - 5f) / 5f), 0), Campaign.Current.Models.CharacterStatsModel.MaxCharacterTier);
        }

        public static void GiveTroopKit(Hero wanderer, Clan clan)
        {
            var s = WanderersSettings.Instance;
            if (s == null) return;
            var donors = CharacterObject.All.Where(c=> c.IsSoldier && c.Culture == clan.Culture && c.Tier == GetHeroTier(wanderer.CharacterObject)).ToList();
            if(donors.Count == 0) return;
            if (s.Debug) InformationManager.DisplayMessage(new InformationMessage($"first donor: {donors.FirstOrDefault().Name}, level: {donors.FirstOrDefault().Level}"));
            // 2) Pick a random donor
            var donor = donors[MBRandom.RandomInt(donors.Count)];

            var sourceEquipments = donor.BattleEquipments.ToList();

            var sourceEquip = sourceEquipments[MBRandom.RandomInt(sourceEquipments.Count)];
            var targetEquip = wanderer.BattleEquipment;

            // 3) Overwrite each non‐empty slot
            for (int i = 0; i < Equipment.EquipmentSlotLength; i++)
            {
                var elem = sourceEquip[i];
                if (!elem.IsEmpty)
                {
                    targetEquip.AddEquipmentToSlotWithoutAgent((EquipmentIndex)i, elem);
                }
            }
        }

        public static void GiveNobleKit(Hero newLord)
        {
            // 1) Build our donor pools in priority order
            bool isOtherLord(Hero h) => h.IsLord && h != newLord && !h.IsChild;

            // 1a) Same‐clan, same‐gender
            var donors = newLord.Clan.Heroes
                .Where(isOtherLord)
                .Where(h => h.IsFemale == newLord.IsFemale)
                .ToList();

            // 1b) Same‐clan, any gender
            if (donors.Count == 0)
            {
                donors = newLord.Clan.Heroes
                    .Where(isOtherLord)
                    .ToList();
            }

            // 1c) Same‐kingdom, same‐gender
            if (donors.Count == 0 && newLord.Clan.Kingdom != null)
            {
                donors = newLord.Clan.Kingdom.Heroes
                    .Where(isOtherLord)
                    .Where(h => h.IsFemale == newLord.IsFemale)
                    .ToList();
            }

            // 1d) Same‐kingdom, any gender
            if (donors.Count == 0 && newLord.Clan.Kingdom != null)
            {
                donors = newLord.Clan.Kingdom.Heroes
                    .Where(isOtherLord)
                    .ToList();
            }

            // 1e) Anywhere: any surviving lord
            if (donors.Count == 0)
            {
                donors = Hero.AllAliveHeroes
                    .Where(isOtherLord)
                    .ToList();
            }

            // If we still have nobody, give up
            if (donors.Count == 0) return;

            // 2) Pick a random donor
            var donor = donors[MBRandom.RandomInt(donors.Count)];

            var sourceEquip = donor.BattleEquipment;
            var targetEquip = newLord.BattleEquipment;

            // 3) Overwrite each non‐empty slot
            for (int i = 0; i < Equipment.EquipmentSlotLength; i++)
            {
                var elem = sourceEquip[i];
                if (!elem.IsEmpty)
                {
                    targetEquip.AddEquipmentToSlotWithoutAgent((EquipmentIndex)i, elem);
                }
            }
        }

        private int EnforceTavern(Settlement settlement)
        {
            try
            {
                var s = WanderersSettings.Instance;
                int minWant = s.MinTavernWanderers;
                int maxWant = s.MaxTavernWanderers;

                if (minWant > maxWant) return 0;

                // collect current tavern wanderers
                var list = settlement.HeroesWithoutParty.Where(h => h.IsWanderer && h.Clan == null).OrderByDescending(h => h.Age).ToList();
                int count = list.Count;

                // 1) remove extras
                if (count > maxWant)
                {
                    int toRemove = count - maxWant;
                    var rnd = new Random();
                    for (int i = 0; i < toRemove; i++)
                    {
                        var victim = list[0];
                        list.Remove(victim);

                        if(victim.CurrentSettlement != null)
                        LeaveSettlementAction.ApplyForCharacterOnly(victim);

                        CleanupHeroRoles(victim);
                        KillCharacterAction.ApplyByRemove(victim);

                    }
                    if (s.Debug) InformationManager.DisplayMessage(new InformationMessage($"Removed: {toRemove}"));
                    count = maxWant;
                }

                // 2) spawn missing
                int spawned = 0;
                if (count < minWant)
                {
                    int toSpawn = minWant - count;

                    for (int i = 0; i < toSpawn; i++)
                    {
                        CreateAndPlaceWanderer(settlement);
                        spawned++;
                    }
                }
                if (s.Debug) InformationManager.DisplayMessage(new InformationMessage($"Spawned: {spawned}"));
                return spawned;
            }
            catch (Exception e)
            {
                InformationManager.DisplayMessage(
                    new InformationMessage($"[WIP] Error in EnforceTavern: {e.Message}")
                );
                // swallow so AI keeps running
                return 0;
            }
        }

        private void CreateAndPlaceWanderer(Settlement settlement)
        {
            // Pick a random culture’s wanderer template
            var templates = new List<CharacterObject>();
            foreach (var cul in _cultures)
                foreach (var co in cul.NotableAndWandererTemplates)
                    if (co.Occupation == Occupation.Wanderer)
                        templates.Add(co);
            if (templates.Count == 0) return;
            var tmpl = templates[MBRandom.RandomInt(templates.Count)];

            // Create the hero
            var hero = HeroCreator.CreateSpecialHero(
                tmpl, settlement, null, null,
                MBRandom.RandomInt(Campaign.Current.Models.AgeModel.HeroComesOfAge, Campaign.Current.Models.AgeModel.BecomeOldAge));
            hero.ChangeState(Hero.CharacterStates.Active);

            // Send them to the tavern party
            EnterSettlementAction.ApplyForCharacterOnly(
                hero, settlement);
        }

        private void RefreshAllTavernWanderers()
        {
            var s = WanderersSettings.Instance;
            int totalCleared = 0, totalSpawned = 0;

            foreach (var town in Town.AllTowns)
            {
                var sett = town.Settlement;
                if (!sett.IsTown) continue;

                // Clear current wanderers
                var list = sett.HeroesWithoutParty.Where(h => h.IsWanderer && h.Clan == null).ToList();
                var count = list.Count;
                int maxWant = s.MaxTavernWanderers;
                if (count > maxWant)
                {
                    totalCleared += count - maxWant;
                }


                // Enforce min/max (returns how many spawned)
                totalSpawned += EnforceTavern(sett);
            }

            InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=wp_refresh_done}Cleared {CLEAR} and spawned {SPAWN} wanderers across all towns.")
                .SetTextVariable("CLEAR", totalCleared)
                .SetTextVariable("SPAWN", totalSpawned)
                .ToString()));
        }


        private void ClearExcessWanderers()
        {
            var s = WanderersSettings.Instance;

            // 1) Gather the pool to count
            var all = Hero.AllAliveHeroes
                .Where(h => h.IsWanderer);

            IEnumerable<Hero> countPool = s.AllClans
                ? all
                : all.Where(h => h.Clan == null);

            int currentCount = countPool.Count();
            int maxAllowed = s.MaxTotalWanderers;

            int toRemove = currentCount - maxAllowed;
            if (toRemove <= 0)
            {
                InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=wp_excess_wanderers}No excess wanderers to clear ({COUNT}/{MAX}).")
                    .SetTextVariable("COUNT", currentCount)
                    .SetTextVariable("MAX", maxAllowed)
                    .ToString()));
                return;
            }

            // 2) Build list of candidates to remove
            IEnumerable<Hero> removalPool = s.AllWanderers
                ? all
                : all.Where(h => h.Clan == null);

            var list = removalPool.ToList();
            var rnd = new Random();

            // 3) Randomly remove `toRemove` heroes
            int removed = 0;
            for (int i = 0; i < toRemove && list.Count > 0; i++)
            {
                int idx = rnd.Next(list.Count);
                var victim = list[idx];
                list.RemoveAt(idx);

                if (victim.CurrentSettlement != null)
                    LeaveSettlementAction.ApplyForCharacterOnly(victim);

                // Dismiss them from any clan, then clear from settlement
                CleanupHeroRoles(victim);
                KillCharacterAction.ApplyByRemove(victim);

                removed++;
            }

            InformationManager.DisplayMessage(
                new InformationMessage(
                    $"[WanderersInParties] Cleared {removed} wanderers (kept {maxAllowed})."
                )
            );

            InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=wp_clear_done}Cleared {REMOVE} wanderers (kept {MAX}).")
                .SetTextVariable("REMOVE", removed)
                .SetTextVariable("MAX", maxAllowed)
                .ToString()));
        }

        //companions in settlement fix
        private static Tuple<string, Monster> GetActionSetAndMonster(CharacterObject co)
        {
            var monstersuffix = TaleWorlds.Core.FaceGen.GetMonsterWithSuffix(co.Race, "_settlement");
            return new Tuple<string, Monster>(ActionSetCode.GenerateActionSetNameWithSuffix(monstersuffix, co.IsFemale, "_lord"), monstersuffix);
        }

        private void AddOtherPartiesHeroesFromSettlement(Settlement settlement)
        {
            var s = WanderersSettings.Instance;

            if (s.Debug) InformationManager.DisplayMessage(new InformationMessage("AddFrom Fired"));

            if (settlement == null) return;
            var parties = settlement.Parties;
            foreach(var party in parties)
            {
                if(party == MobileParty.MainParty) continue;

                Hero leaderHero = party.LeaderHero;
                // iterate troop roster to find hero troops
                var roster = party.MemberRoster.GetTroopRoster().ToList();
                foreach (var trElem in roster)
                {
                    if (!trElem.Character.IsHero) continue;
                    var hero = trElem.Character.HeroObject;
                    if (hero == null) continue;
                    if (hero == leaderHero || hero == Hero.MainHero) continue;
                    if (!hero.IsAlive || !hero.IsActive) continue;
                    if (hero.PartyBelongedToAsPrisoner != null) continue;
                    if (hero.PartyBelongedTo != party) continue; // safety check

                    var actionSetAndMonster = GetActionSetAndMonster(hero.CharacterObject);
                    if (actionSetAndMonster == null) continue;
                    if (s.Debug) InformationManager.DisplayMessage(new InformationMessage("actionSetAndMonster is alright"));
                    IFaction mapFaction = hero.MapFaction;
                    uint color1 = (mapFaction != null) ? mapFaction.Color : 4291609515U;
                    uint color2 = color1;

                    var origin = new PartyAgentOrigin(party.Party, hero.CharacterObject, -1, default(UniqueTroopDescriptor), false);

                    AgentData agentData = new AgentData(origin)
                        .Monster(actionSetAndMonster.Item2)
                        .NoHorses(true)
                        .ClothingColor1(color1)
                        .ClothingColor2(color2);

                    Location location;
                    if (settlement.IsFortification)
                    {
                        location = LocationComplex.Current.GetLocationWithId("lordshall");
                        if (hero.IsWanderer && !settlement.IsCastle) location = LocationComplex.Current.GetLocationWithId("tavern");
                    }
                    else if (settlement.IsVillage) location = LocationComplex.Current.GetLocationWithId("village_center");
                    else location = LocationComplex.Current.GetLocationWithId("center");
                    if (s.Debug) InformationManager.DisplayMessage(new InformationMessage($"{hero.Name} check locations in {settlement.Name}"));
                    if (location == null) continue;

                    // Avoid creating obvious duplicates by skipping if hero already in the location characters (best-effort)
                    // Unfortunately Location API for enumerating characters is internal in some versions; skip this if unsafe.
                    // You can add dedupe checks here later if you see duplication.
                    if (s.Debug) InformationManager.DisplayMessage(new InformationMessage($"added {hero.Name} to {settlement.Name}"));
                    location.AddCharacter(new LocationCharacter(
                        agentData,
                        new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddFixedCharacterBehaviors),
                        "sp_notable",
                        true,
                        LocationCharacter.CharacterRelations.Neutral,
                        actionSetAndMonster.Item1,
                        !settlement.IsVillage,
                        false,
                        null,
                        false,
                        false,
                        true));
                }
            }


        }

        private void AddOtherPartiesHeroesToSettlement(MobileParty party, Settlement settlement)
        {
            var s = WanderersSettings.Instance;

            Hero leaderHero = party.LeaderHero;
            if (leaderHero == null || leaderHero == Hero.MainHero || party == null)
            {
                return;
            }

            if (settlement == null) return;

            // iterate troop roster to find hero troops
            var roster = party.MemberRoster.GetTroopRoster().ToList();
                foreach (var trElem in roster)
                {
                    if (!trElem.Character.IsHero) continue;
                    var hero = trElem.Character.HeroObject;
                    if (hero == null) continue;
                    if (hero == leaderHero || hero == Hero.MainHero) continue;
                    if (!hero.IsAlive || !hero.IsActive) continue;
                    if (hero.PartyBelongedToAsPrisoner != null) continue;
                    if (hero.PartyBelongedTo != party) continue; // safety check

                    var actionSetAndMonster = GetActionSetAndMonster(hero.CharacterObject);
                    if (actionSetAndMonster == null) continue;

                    IFaction mapFaction = hero.MapFaction;
                    uint color1 = (mapFaction != null) ? mapFaction.Color : 4291609515U;
                    uint color2 = color1;

                    var origin = new PartyAgentOrigin(party.Party, hero.CharacterObject, -1, default(UniqueTroopDescriptor), false);

                    AgentData agentData = new AgentData(origin)
                        .Monster(actionSetAndMonster.Item2)
                        .NoHorses(true)
                        .ClothingColor1(color1)
                        .ClothingColor2(color2);

                    Location location;
                    if (settlement.IsFortification) 
                    {
                       location = LocationComplex.Current.GetLocationWithId("lordshall");
                       if(hero.IsWanderer && !settlement.IsCastle) location = LocationComplex.Current.GetLocationWithId("tavern");
                    } 
                    else if (settlement.IsVillage) location = LocationComplex.Current.GetLocationWithId("village_center");
                    else location = LocationComplex.Current.GetLocationWithId("center");

                    if (location == null) continue;

                    // Avoid creating obvious duplicates by skipping if hero already in the location characters (best-effort)
                    // Unfortunately Location API for enumerating characters is internal in some versions; skip this if unsafe.
                    // You can add dedupe checks here later if you see duplication.
                    if (s.Debug) InformationManager.DisplayMessage(new InformationMessage($"added {hero.Name} to {settlement.Name}"));

                    location.AddCharacter(new LocationCharacter(
                        agentData,
                        new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddFixedCharacterBehaviors),
                        "sp_notable",
                        true,
                        LocationCharacter.CharacterRelations.Neutral,
                        actionSetAndMonster.Item1,
                        !settlement.IsVillage,
                        false,
                        null,
                        false,
                        false,
                        true));
                }
            }



        public override void SyncData(IDataStore dataStore)
        {

        }


    }
}
