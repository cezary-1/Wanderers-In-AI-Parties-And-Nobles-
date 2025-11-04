using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace WanderersInParties
{
    public class WanderersInPartiesCommands
    {
        [CommandLineFunctionality.CommandLineArgumentFunction("debug_remove_wanderers", "wanderersinparties")]
        private static string DebugRemoveWanderers(List<string> args)
        {

            var heroes = Hero.AllAliveHeroes.Where(h => h.IsWanderer && h.Clan != null && h.Clan != Clan.PlayerClan).ToList();

            if (heroes == null || heroes.Count <= 0) return $"No wanderers in clans";

            foreach(var hero in heroes)
            {
                WanderersInPartiesBehavior.CleanupHeroRoles(hero);
            }

            return $"Wanderers were succesfully removed from clans";
        }
    }
}
