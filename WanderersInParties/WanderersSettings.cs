using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Attributes;
using MCM.Abstractions.Base.Global;
using System;
using TaleWorlds.Localization;

namespace WanderersInParties
{
    public sealed class WanderersSettings : AttributeGlobalSettings<WanderersSettings>
    {
        public override string Id => "WanderersInParties";
        public override string DisplayName => new TextObject("{=WP_WANDERERS_IN_PARTIES}Wanderers In Parties").ToString();
        public override string FolderName => "WanderersInParties";
        public override string FormatType => "json";

        [SettingPropertyFloatingInteger(
            "{=WP_MCM_RECRUIT}Recruitment Probability", 0f, 1f, "#0%", Order = 0, RequireRestart = false,
            HintText = "{=WP_MCM_RECRUIT_HINT}Chance each wanderer joins when lord enter a settlement. (Default: 0.25)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General")]
        public float RecruitmentProbability { get; set; } = 0.25f;

        [SettingPropertyBool(
          "{=WP_MCM_RECRUIT_CULTURE}Recruit Same Culture?",
          Order = 1, RequireRestart = false,
          HintText = "{=WP_MCM_RECRUIT_CULTURE_HINT}Should clans recruit wanderers with the same cultures? (Default: false)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General")]
        public bool SameCulture { get; set; } = false;

        [SettingPropertyFloatingInteger(
            "{=WP_MCM_COST}Cost Multiplier", 0f, 10f, Order = 1, RequireRestart = false,
            HintText = "{=WP_MCM_COST_HINT}Multiplier applied to (level × equipmentValue) when recruiting. (Default: 1)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General")]
        public float CostMultiplier { get; set; } = 1f;

        [SettingPropertyInteger(
            "{=WP_MCM_MAX_AI}Base Max Wanderers In AI Clans", 0, 100, Order = 2, RequireRestart = false,
            HintText = "{=WP_MCM_MAX_AI_HINT}Base Maximum wanderers for ai clans. (Default: 2)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General")]
        public int MaxWanderersClan { get; set; } = 2;

        [SettingPropertyInteger(
            "{=WP_MCM_MAX_AI_BONUS}Max Wanderers In AI Clans Bonus", 0, 100, Order = 3, RequireRestart = false,
            HintText = "{=WP_MCM_MAX_AI_BONUS_HINT}Bonus for Maximum wanderers for ai clans by tier level. (Default: 2)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General")]
        public int MaxWanderersClanTier { get; set; } = 2;

        [SettingPropertyBool(
          "{=WP_MCM_TROOP}Troop Equipment?",
          Order = 4, RequireRestart = false,
          HintText = "{=WP_MCM_TROOP_HINT}Should wanderer get troop equipement upon joining? (Will get random troop, from the party leader culture, with the same tier, equipment) (Default: false)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General")]
        public bool TroopKit { get; set; } = false;

        [SettingPropertyFloatingInteger(
            "{=WP_MCM_PROMOTE}Promotion Chance", 0f, 1f, "#0%", Order = 4, RequireRestart = false,
            HintText = "{=WP_MCM_PROMOTE_HINT}Daily chance for a wanderer to become a noble. (Default: 0.01)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General")]
        public float PromotionChance { get; set; } = 0.01f;

        [SettingPropertyBool(
          "{=WP_MCM_CHANGE_NAME}Change Name On Promotion?",
          Order = 4, RequireRestart = false,
          HintText = "{=WP_MCM_CHANGE_NAME_HINT}Should wanderers get rid of their nicknames on promotion? (Default: true)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General")]
        public bool ChangeName { get; set; } = true;

        [SettingPropertyFloatingInteger(
            "{=WP_MCM_FIRE}Firing Chance", 0f, 1f, "#0%", Order = 5, RequireRestart = false,
            HintText = "{=WP_MCM_FIRE_HINT}Daily chance for a wanderer to be dismissed. (Default: 0.02)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General")]
        public float FiringChance { get; set; } = 0.02f;

        [SettingPropertyFloatingInteger(
            "{=WP_MCM_BUY}Buy Chance per Piece", 0f, 1f, "#0%", Order = 6, RequireRestart = false,
            HintText = "{=WP_MCM_BUY_HINT}Chance to purchase a missing item when entering a settlement. (Default: 0.2)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General")]
        public float BuyChancePerPiece { get; set; } = 0.2f;

        [SettingPropertyFloatingInteger(
            "{=WP_MCM_UPGRADE}Upgrade Chance per Piece", 0f, 1f, "#0%", Order = 7, RequireRestart = false,
            HintText = "{=WP_MCM_UPGRADE_HINT}Chance to attempt an equipment upgrade when entering a settlement. (Default: 0.1)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General")]
        public float UpgradeChancePerPiece { get; set; } = 0.1f;

        [SettingPropertyFloatingInteger(
            "{=WP_MCM_MIN_GOLD}Min gold value for buying/upgrading", 0f, 1f, "#0%", Order = 8, RequireRestart = false,
            HintText = "{=WP_MCM_MIN_GOLD_HINT}Min gold value for buying/upgrading (lord gold * MinValue) (Default: 0.25)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General")]
        public float MinValue { get; set; } = 0.25f;

        [SettingPropertyFloatingInteger(
            "{=WP_MCM_MAX_GOLD}Max gold value for buying/upgrading", 0f, 1f, "#0%", Order = 9, RequireRestart = false,
            HintText = "{=WP_MCM_MAX_GOLD_HINT}Max gold value for buying/upgrading (lord gold * MaxValue) (Default: 0.25)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General")]
        public float MaxValue { get; set; } = 0.25f;

        [SettingPropertyBool(
          "{=WP_MCM_CULTURE}Respect Culture",
          Order = 10, RequireRestart = false,
          HintText = "{=WP_MCM_CULTURE_HINT}Only buy/upgrade items from the wanderer’s culture. (Default: false)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General")]
        public bool RespectCulture { get; set; } = false;

        [SettingPropertyFloatingInteger(
            "{=WP_MCM_ITEM_COST}Item Cost Multiplier", 0f, 10f, Order = 10, RequireRestart = false,
            HintText = "{=WP_MCM_ITEM_COST_HINT}Multiplier applied to (itemValue) when upgrading/buying. (Default: 1)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General")]
        public float ItemCostMultiplier { get; set; } = 1f;

        [SettingPropertyBool("{=WP_MCM_NOBLE_SAVE}Wanderers To Noble save Clan", Order = 10, RequireRestart = false, HintText = "{=WP_MCM_NOBLE_SAVE_HINT}Should Wanderers becomes nobles to save clan from destruction? (If not they just become again clanless wanderers) (Default: true)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General")]
        public bool SaveClan { get; set; } = true;

        [SettingPropertyInteger(
            "{=WP_MCM_PARTY_WHICH}Which party to join?", 0, 4, Order = 11, RequireRestart = false,
            HintText = "{=WP_MCM_PARTY_WHICH_HINT}0 = random, 1 = with lowest troops, 2 = with lowest heroes, 3 = clan leader, 4 = best relation with clan leader (Default: 0)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General")]
        public int WhichParty { get; set; } = 0;

        [SettingPropertyInteger(
            "{=WP_MCM_HEROES_LIMIT}Per Party Hero Limit (For Wanderers)", 0, 100, Order = 12, RequireRestart = false,
            HintText = "{=WP_MCM_HEROES_LIMIT_HINT}How many heroes can a party have? (Default: 20)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General")]
        public int PartyHeroes { get; set; } = 20;

        [SettingPropertyInteger(
            "{=WP_MCM_NOBLES_LIMIT}Per Clan Nobles", 0, 100, Order = 12, RequireRestart = false,
            HintText = "{=WP_MCM_NOBLES_LIMIT_HINT}How many adult nobles can a clan have before stopping promotions? (Default: 10)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General")]
        public int NoblesPerClan { get; set; } = 10;

        [SettingPropertyInteger(
            "{=WP_MCM_NOBLES_TIER_LIMIT}Per Clan Nobles Tier Bonus", 0, 100, Order = 12, RequireRestart = false,
            HintText = "{=WP_MCM_NOBLES_TIER_LIMIT_HINT}Bonus for each tier to per clan nobles. (Default: 1)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General")]
        public int NoblesPerClanTier { get; set; } = 1;

        [SettingPropertyBool(
          "{=WP_MCM_LEAD_PARTY}Wanderers Leading Parties?",
          Order = 13, RequireRestart = false,
          HintText = "{=WP_MCM_LEAD_PARTY_HINT}If true, wanderers will be able to lead parties. (Default: false)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General")]
        public bool LeadParty { get; set; } = false;

        [SettingPropertyBool(
          "{=WP_MCM_FIRE_NO_PARTY}Fire Wanderers If Couldn't Rejoin?",
          Order = 14, RequireRestart = false,
          HintText = "{=WP_MCM_FIRE_NO_PARTY_HINT}If true, wanderers will be fired if they couldn't rejoin because no party was found. (Default: false)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General")]
        public bool FireNoParty { get; set; } = false;

        [SettingPropertyInteger(
          "{=WP_MCM_AGE}Eligible Wanderers Age", 18, 127,
          Order = 0, RequireRestart = false,
          HintText = "{=WP_MCM_AGE_HINT}How old wanderers should be able to join parties? (Default: 22)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_FILTERS}Filters")]
        public int PartyAge { get; set; } = 22;

        [SettingPropertyInteger(
          "{=WP_MCM_GENDER}Eligible Wanderer Gender", 0, 2,
          Order = 1, RequireRestart = false,
          HintText = "{=WP_MCM_GENDER_HINT}0 = all, 1 = male only, 2 = female only (Default: 0)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_FILTERS}Filters")]
        public int PartyGender { get; set; } = 0;

        [SettingPropertyBool(
          "{=WP_MCM_PREGNANT}Eligible Wanderer Pregnant",
          Order = 1, RequireRestart = false,
          HintText = "{=WP_MCM_PREGNANT_HINT}Should pregnant wanderers be in parties? (Default: false)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_FILTERS}Filters")]
        public bool PartyPregnant { get; set; } = false;

        [SettingPropertyBool("{=WP_MCM_TAVERN_ENABLE}Enable/Disable tavern spawns", Order = 11, RequireRestart = false, HintText = "{=WP_MCM_TAVERN_ENABLE_HINT}Enable/Disable tavern spawns daily mechanics (Default: true)")]
        [SettingPropertyGroup("{=MCM_TAVERN_SPAWNS}Tavern Spawns")]
        public bool TavernSpawn { get; set; } = true;

        [SettingPropertyInteger(
            "{=WP_MCM_MIN_TAVERN}Min Tavern Wanderers", 0, 100, Order = 11, RequireRestart = false,
            HintText = "{=WP_MCM_MIN_TAVERN_HINT}Minimum number of wanderers in each town's tavern. (checks daily) (Default: 3)")]
        [SettingPropertyGroup("{=MCM_TAVERN_SPAWNS}Tavern Spawns")]
        public int MinTavernWanderers { get; set; } = 3;

        [SettingPropertyInteger(
            "{=WP_MCM_MAX_TAVERN}Max Tavern Wanderers", 0, 200, Order = 12, RequireRestart = false,
            HintText = "{=WP_MCM_MAX_TAVERN_HINT}Maximum number of wanderers in each town's tavern. (checks daily) (Default: 12)")]
        [SettingPropertyGroup("{=MCM_TAVERN_SPAWNS}Tavern Spawns")]
        public int MaxTavernWanderers { get; set; } = 12;

        [SettingPropertyButton("{=WP_MCM_REFRESH_TAVERN}Refresh Tavern Wanderers", Content = "{=WP_MCM_REFRESH_TAVERN}Refresh Wanderers", Order = 13, RequireRestart = false, HintText = "{=WP_MCM_REFRESH_TAVERN_HINT}Immediately refresh wandereres.")]
        [SettingPropertyGroup("{=MCM_TAVERN_SPAWNS}Tavern Spawns")]
        public Action RefreshWanderers { get; set; }

        [SettingPropertyInteger(
            "{=WP_MCM_MAX_CLEAN}Cleanup/Max Total Wanderers", 0, 1000, Order = 14, RequireRestart = false,
            HintText = "{=WP_MCM_MAX_CLEAN_HINT}Maximum total wanderers to allow that won't be affected by clearing. (Default: 200)")]
        [SettingPropertyGroup("{=MCM_CLEANUP}Cleanup")]
        public int MaxTotalWanderers { get; set; } = 200;

        [SettingPropertyBool(
            "{=WP_MCM_CLEAN_REMOVE}Cleanup/Remove from Clans Too", Order = 15, RequireRestart = false,
            HintText = "{=WP_MCM_CLEAN_REMOVE_HINT}If true, Clear Wanderers also clear wanderers who are already in clans. (Default: false)")]
        [SettingPropertyGroup("{=MCM_CLEANUP}Cleanup")]
        public bool AllWanderers { get; set; } = false;

        [SettingPropertyBool(
            "{=WP_MCM_CLEAN_CLAN}Cleanup/Count Claned Wanderers", Order = 16, RequireRestart = false,
            HintText = "{=WP_MCM_CLEAN_CLAN_HINT}If true, claned wanderers counts to the MaxTotalWanderers limit . (Default: false)")]
        [SettingPropertyGroup("{=MCM_CLEANUP}Cleanup")]
        public bool AllClans { get; set; } = false;

        [SettingPropertyButton(
            "{=WP_MCM_CLEAN}Cleanup/Clear Wanderers",
            Content = "{=WP_MCM_CLEAN}Clear Wanderers",
            RequireRestart = false,
            Order = 17,
            HintText = "{=WP_MCM_CLEAN_HINT}Remove excess wanderers up to MaxTotalWanderers according to your cleanup settings.")]
        [SettingPropertyGroup("{=MCM_CLEANUP}Cleanup")]
        public Action ClearWanderers { get; set; }

        [SettingPropertyBool(
            "{=WP_MCM_PLAYER_MAX_ENABLE}Enable Companions Options",
            Order = 18, RequireRestart = false,
            HintText = "{=WP_MCM_PLAYER_MAX_ENABLE_HINT}Should player companion limit options be enabled? (Default: true)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_PLAYER}Player")]
        public bool EnableCompanions { get; set; } = true;

        [SettingPropertyInteger(
            "{=WP_MCM_PLAYER_MAX}Companions/Base Max Companions", 0, 100, Order = 18, RequireRestart = false,
            HintText = "{=WP_MCM_PLAYER_MAX_HINT}The base maximum number of companions you can have. (Vanilla: 3, Default: 7)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_PLAYER}Player")]
        public int MaxCompanions { get; set; } = 7;


        [SettingPropertyFloatingInteger(
            "{=WP_MCM_PLAYER_COST}Player Wanderer Cost Multiplier", 0f, 100f, Order = 18, RequireRestart = false,
            HintText = "{=WP_MCM_PLAYER_COST_HINT}Multiplier applied to cost of recruiting wanderer. (Default: 1)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_PLAYER}Player")]
        public float PlayerCostMultiplier { get; set; } = 1f;

        [SettingPropertyInteger(
            "{=WP_MCM_PLAYER_TIER}Companions/Bonus Per Tier", 0, 100, Order = 19, RequireRestart = false,
            HintText = "{=WP_MCM_PLAYER_TIER_HINT}Additional companion slots per clan tier beyond base. (Vanilla and Default: 1)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_PLAYER}Player")]
        public int MaxCompanionsTier { get; set; } = 1;

        [SettingPropertyBool(
            "{=WP_MCM_PLAYER_COMPANION_TRANSFER}Should Companions Be Transferable?",
            Order = 19, RequireRestart = false,
            HintText = "{=WP_MCM_PLAYER_COMPANION_TRANSFER_HINT}Should player be able to add other heroes to other player clan's parties? (Default: true)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_PLAYER}Player")]
        public bool CompanionTransfer { get; set; } = true;

        [SettingPropertyBool(
            "{=WP_MCM_DIAL_FAMILY}Dialogues/Enable ‘Make Family’ Dialogues",
            Order = 20, RequireRestart = false,
            HintText = "{=WP_MCM_DIAL_FAMILY_HINT}Show a dialogue option to adopt a companion as parent/sibling/child. (Require save reload) (Default: true)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_PLAYER}Player")]
        public bool EnableMakeFamilyDialogues { get; set; } = true;

        [SettingPropertyBool(
            "{=WP_MCM_DIAL_NOBLE}Dialogues/Enable ‘Make Noble’ Dialogue",
            Order = 21, RequireRestart = false,
            HintText = "{=WP_MCM_DIAL_NOBLE_HINT}Show a dialogue option to ennoble a companion. (Require save reload) (Default: true)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_PLAYER}Player")]
        public bool EnableMakeNobleDialogue { get; set; } = true;

        [SettingPropertyBool(
            "{=WP_MCM_DIAL_OTHER}Dialogues/Enable ‘Speak With Other Member’ Dialogue",
            Order = 22, RequireRestart = false,
            HintText = "{=WP_MCM_DIAL_OTHER_HINT}Show a dialogue option to speak with other party member. (Require save reload) (Default: true)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_PLAYER}Player")]
        public bool EnableSpeakNobleDialogue { get; set; } = true;

        [SettingPropertyBool(
            "{=MCM_DEBUG}Debug Info",
            Order = 0, RequireRestart = false,
            HintText = "{=WP_MCM_DEBUG_HINT}Show debug information. (Default: false)")]
        [SettingPropertyGroup("{=MCM_DEBUG}Debug")]
        public bool Debug { get; set; } = false;

        [SettingPropertyBool(
          "{=WP_MCM_NOBLES}Nobles In Parties",
          Order = 0, RequireRestart = false,
          HintText = "{=WP_MCM_NOBLES_HINT}Should not leading any parties nobles join to other parties? (Default: true)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_NOBLES_IN_PARTIES}Nobles In Parties")]
        public bool NoblesParty { get; set; } = true;

        [SettingPropertyBool(
          "{=WP_MCM_NOBLES_CLANLEADER}Allow Clan Leader",
          Order = 0, RequireRestart = false,
          HintText = "{=WP_MCM_NOBLES_CLANLEADER_HINT}Should clan leaders be allowed to join other parties? (Default: true)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_NOBLES_IN_PARTIES}Nobles In Parties")]
        public bool NoblesPartyClanLeader { get; set; } = true;

        [SettingPropertyBool(
          "{=WP_MCM_NOBLES_LIMIT}Before Reached Party Limit?",
          Order = 1, RequireRestart = false,
          HintText = "{=WP_MCM_NOBLES_LIMIT_HINT}Should they join even when clan didn't reach it's party limit? (Default: false)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_NOBLES_IN_PARTIES}Nobles In Parties")]
        public bool NoblesPartyLimit { get; set; } = false;

        [SettingPropertyFloatingInteger(
            "{=WP_MCM_NOBLES_CHANCE}Noble Join Chance", 0f, 1f, "#0%", Order = 2, RequireRestart = false,
            HintText = "{=WP_MCM_NOBLES_CHANCE_HINT}Daily chance for noble to join party. (Default: 0.1)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_NOBLES_IN_PARTIES}Nobles In Parties")]
        public float NoblesPartyChance { get; set; } = 0.1f;

        [SettingPropertyInteger(
            "{=WP_MCM_NOBLES_HEROES_LIMIT}Per Party Hero Limit (For Nobles)", 0, 100, Order = 2, RequireRestart = false,
            HintText = "{=WP_MCM_NOBLES_HEROES_LIMIT_HINT}How many heroes can a party have? (Default: 20)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_NOBLES_IN_PARTIES}Nobles In Parties")]
        public int NoblesPartyHeroes { get; set; } = 20;

        [SettingPropertyInteger(
            "{=WP_MCM_NOBLES_PARTY_WHICH}Which party to join?", 0, 4, Order = 2, RequireRestart = false,
            HintText = "{=WP_MCM_NOBLES_PARTY_WHICH_HINT}0 = random, 1 = with lowest troops, 2 = with lowest heroes, 3 = clan leader, 4 = best relation with party leader (Default: 0)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_NOBLES_IN_PARTIES}Nobles In Parties")]
        public int NoblesWhichParty { get; set; } = 0;

        [SettingPropertyInteger(
          "{=WP_MCM_NOBLES_LEAVE}Leave parties?", 0, 3,
          Order = 3, RequireRestart = false,
          HintText = "{=WP_MCM_NOBLES_LEAVE_HINT}Should some of them leave parties? 0 = Never, 1 = when there is no available noble, 2 = same as 1 but also when they are more qualified to lead party than others, 3 = always (Default: 2)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_NOBLES_IN_PARTIES}Nobles In Parties")]
        public int NoblesPartyLeave { get; set; } = 2;

        [SettingPropertyBool(
          "{=WP_MCM_NOBLES_LEAVE_CLANLEADER}Should Clan Leader Leave When Can Lead?",
          Order = 3, RequireRestart = false,
          HintText = "{=WP_MCM_NOBLES_LEAVE_CLANLEADER_HINT}Should clan leader always first leave party if they can create their own? (Default: true)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_NOBLES_IN_PARTIES}Nobles In Parties")]
        public bool NoblesPartyLeaveClanLeader { get; set; } = true;

        [SettingPropertyFloatingInteger(
            "{=WP_MCM_NOBLES_LEAVE_CONSIDER}Consider Leave At Party Size", 0f, 1f, "#0%", Order = 4, RequireRestart = false,
            HintText = "{=WP_MCM_NOBLES_LEAVE_CONSIDER_HINT}Consider leaving when party reaches at least this percent of it's limit party size.(set to 0 if you want to ignore party size limit) (Default: 0)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_NOBLES_IN_PARTIES}Nobles In Parties")]
        public float NoblesPartyLeaveConsider { get; set; } = 0f;

        [SettingPropertyFloatingInteger(
            "{=WP_MCM_NOBLES_LEAVE_CHANCE}Noble Leave Chance", 0f, 1f, "#0%", Order = 5, RequireRestart = false,
            HintText = "{=WP_MCM_NOBLES_LEAVE_CHANCE_HINT}Daily chance for noble to leave party (depends on previous options). (Default: 0.2)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_NOBLES_IN_PARTIES}Nobles In Parties")]
        public float NoblesPartyLeaveChance { get; set; } = 0.2f;

        [SettingPropertyInteger(
          "{=WP_MCM_NOBLES_SWAP}Swap Leadership In Parties?", 0, 3,
          Order = 6, RequireRestart = false,
          HintText = "{=WP_MCM_NOBLES_SWAP_HINT}Should leadership in parties change when noble joins? 0 = Never, 1 = Clan Leader, 2 = Clan Leader -> Commanding Skills -> Age, 3 = Clan Leader -> Age -> Commanding Skills (Default: 2)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_NOBLES_IN_PARTIES}Nobles In Parties")]
        public int NoblesPartySwap { get; set; } = 2;

        [SettingPropertyInteger(
          "{=WP_MCM_NOBLES_AGE}Eligible Nobles Age", 18, 127,
          Order = 0, RequireRestart = false,
          HintText = "{=WP_MCM_NOBLES_AGE_HINT}How old nobles should be able to join parties? (Default: 22)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_NOBLES_IN_PARTIES}Nobles In Parties/{=MCM_FILTERS}Filters")]
        public int NoblesPartyAge { get; set; } = 22;

        [SettingPropertyInteger(
          "{=WP_MCM_NOBLES_GENDER}Eligible Nobles Gender", 0, 2,
          Order = 1, RequireRestart = false,
          HintText = "{=WP_MCM_NOBLES_GENDER_HINT}0 = all, 1 = male only, 2 = female only (Default: 0)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_NOBLES_IN_PARTIES}Nobles In Parties/{=MCM_FILTERS}Filters")]
        public int NoblesPartyGender { get; set; } = 0;

        [SettingPropertyBool(
          "{=WP_MCM_NOBLES_PREGNANT}Eligible Nobles Pregnant",
          Order = 1, RequireRestart = false,
          HintText = "{=WP_MCM_NOBLES_PREGNANT_HINT}Should pregnant nobles be in parties? (Default: false)")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_NOBLES_IN_PARTIES}Nobles In Parties/{=MCM_FILTERS}Filters")]
        public bool NoblesPartyPregnant { get; set; } = false;

    }
}
