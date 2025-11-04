using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace WanderersInParties
{
    public class WanderersInPartiesModule : MBSubModuleBase
    {
        private Harmony _harmony;
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            InformationManager.DisplayMessage(
                new InformationMessage("WanderersInPartiesModule Mod loaded successfully."));

            _harmony = new Harmony("WanderersInParties");
            _harmony.PatchAll();

        }

        protected override void OnSubModuleUnloaded()
        {
            base.OnSubModuleUnloaded();
        }

        protected override void OnGameStart(Game game, IGameStarter starter)
        {
            base.OnGameStart(game, starter);
            if (game.GameType is Campaign)
            {
                ((CampaignGameStarter)starter).AddBehavior(new WanderersInPartiesBehavior());
            }
        }
    }
}
