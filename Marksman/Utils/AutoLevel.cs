using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace Marksman.Utils
{
    public class AutoLevel
    {
        public static Menu LocalMenu;

        public static int[] SpellLevels;

        public AutoLevel()
        {
            LocalMenu = new Menu("Auto Level", "Auto Level");
            LocalMenu.AddItem(
                new MenuItem("AutoLevel.Active", "Auto Level Active!").SetValue(
                    new KeyBind("L".ToCharArray()[0], KeyBindType.Toggle))).Permashow(true, "Marksman: Auto Level");

            var championName = ObjectManager.Player.ChampionName.ToLowerInvariant();
            switch (championName)
            {
                case "ashe":
                    SpellLevels = new int[] { 2, 1, 3, 1, 1, 4, 1, 2, 1, 2, 4, 2, 2, 3, 3, 4, 3, 3 };
                    LocalMenu.AddItem(new MenuItem("AutoLevel." + championName, GetLevelStr(SpellLevels)));
                    break;

                case "caitlyn":
                    SpellLevels = new int[] { 1, 2, 3, 1, 1, 4, 1, 3, 1, 3, 4, 3, 3, 2, 2, 4, 2, 2 };
                    LocalMenu.AddItem(new MenuItem("AutoLevel." + championName, GetLevelStr(SpellLevels)));
                    break;

                case "corki":
                    SpellLevels = ObjectManager.Player.PercentMagicDamageMod
                                  > ObjectManager.Player.PercentPhysicalDamageMod
                                      ? new int[] { 1, 2, 3, 1, 1, 4, 1, 2, 1, 2, 4, 2, 2, 3, 3, 4, 3, 3 }
                                      : new int[] { 1, 2, 3, 1, 1, 4, 1, 3, 1, 3, 4, 3, 3, 2, 2, 4, 2, 2 };
                    LocalMenu.AddItem(new MenuItem("AutoLevel." + championName, GetLevelStr(SpellLevels)));
                    break;

                case "draven":
                    SpellLevels = new int[] { 1, 2, 3, 1, 1, 4, 1, 2, 1, 2, 4, 2, 2, 3, 3, 4, 3, 3 };
                    break;

                case "ezreal":
                    SpellLevels = ObjectManager.Player.PercentMagicDamageMod
                                  > ObjectManager.Player.PercentPhysicalDamageMod
                                      ? new int[] { 2, 3, 1, 2, 1, 4, 1, 3, 1, 3, 4, 3, 3, 2, 2, 4, 2, 2 }
                                      : new int[] { 1, 2, 3, 2, 2, 4, 2, 1, 2, 1, 4, 1, 1, 3, 3, 4, 3, 3 };
                    LocalMenu.AddItem(new MenuItem("AutoLevel." + championName, GetLevelStr(SpellLevels)));
                    break;

                case "graves":
                    SpellLevels = new int[] { 1, 3, 2, 1, 1, 4, 1, 3, 1, 3, 4, 3, 3, 2, 2, 4, 2, 2 };
                    LocalMenu.AddItem(new MenuItem("AutoLevel." + championName, GetLevelStr(SpellLevels)));
                    break;

                case "gnar":
                    SpellLevels = new int[] { 1, 2, 3, 1, 1, 4, 1, 2, 1, 2, 4, 2, 2, 3, 3, 4, 3, 3 };
                    LocalMenu.AddItem(new MenuItem("AutoLevel." + championName, GetLevelStr(SpellLevels)));
                    break;

                case "jinx":
                    SpellLevels = new int[] { 1, 3, 2, 1, 1, 4, 1, 2, 1, 2, 4, 2, 2, 3, 3, 4, 3, 3 };
                    LocalMenu.AddItem(new MenuItem("AutoLevel." + championName, GetLevelStr(SpellLevels)));
                    break;

                case "kalista":
                    SpellLevels = new int[] { 2, 1, 3, 3, 3, 4, 1, 3, 3, 1, 4, 1, 1, 2, 2, 4, 2, 2 };
                    LocalMenu.AddItem(new MenuItem("AutoLevel." + championName, GetLevelStr(SpellLevels)));
                    break;

                case "kindred":
                    SpellLevels = new int[] { 2, 1, 3, 3, 3, 4, 1, 3, 3, 1, 4, 1, 1, 2, 2, 4, 2, 2 };
                    LocalMenu.AddItem(new MenuItem("AutoLevel." + championName, GetLevelStr(SpellLevels)));
                    break;

                case "kogmaw":
                    SpellLevels = ObjectManager.Player.PercentMagicDamageMod
                                  > ObjectManager.Player.PercentPhysicalDamageMod
                                      ? new int[] { 2, 1, 3, 2, 2, 4, 2, 1, 2, 1, 4, 1, 1, 3, 3, 4, 3, 3 }
                                      : new int[] { 3, 2, 1, 3, 3, 4, 3, 1, 3, 1, 4, 1, 1, 2, 2, 4, 2, 2 };
                    LocalMenu.AddItem(new MenuItem("AutoLevel." + championName, GetLevelStr(SpellLevels)));
                    break;

                case "lucian":
                    SpellLevels = new int[] { 1, 3, 2, 1, 1, 4, 1, 2, 1, 2, 4, 2, 2, 3, 3, 4, 3, 3 };
                    LocalMenu.AddItem(new MenuItem("AutoLevel." + championName, GetLevelStr(SpellLevels)));
                    break;

                case "missfortune":
                    SpellLevels = new int[] { 1, 2, 3, 2, 2, 4, 2, 1, 2, 1, 4, 1, 1, 3, 3, 4, 3, 3 };
                    LocalMenu.AddItem(new MenuItem("AutoLevel." + championName, GetLevelStr(SpellLevels)));
                    break;

                case "quinn":
                    SpellLevels = new int[] { 1, 3, 2, 1, 1, 4, 1, 3, 1, 3, 4, 3, 3, 2, 2, 4, 2, 2 };
                    LocalMenu.AddItem(new MenuItem("AutoLevel." + championName, GetLevelStr(SpellLevels)));
                    break;

                case "sivir":
                    SpellLevels = new int[] { 1, 2, 3, 1, 1, 4, 1, 2, 1, 2, 4, 2, 2, 3, 3, 4, 3, 3 };
                    LocalMenu.AddItem(new MenuItem("AutoLevel." + championName, GetLevelStr(SpellLevels)));
                    break;

                case "teemo":
                    SpellLevels = new int[] { 3, 1, 2, 3, 3, 4, 3, 1, 3, 1, 4, 1, 1, 2, 2, 4, 2, 2 };
                    LocalMenu.AddItem(new MenuItem("AutoLevel." + championName, GetLevelStr(SpellLevels)));
                    break;

                case "tristana":
                    SpellLevels = new int[] { 3, 2, 3, 1, 3, 4, 6, 1, 3, 1, 4, 1, 1, 2, 2, 4, 2, 2 };
                    LocalMenu.AddItem(new MenuItem("AutoLevel." + championName, GetLevelStr(SpellLevels)));
                    break;

                case "twitch":
                    SpellLevels = new int[] { 3, 2, 3, 1, 3, 4, 3, 1, 3, 1, 4, 1, 1, 2, 2, 4, 2, 2 };
                    LocalMenu.AddItem(new MenuItem("AutoLevel." + championName, GetLevelStr(SpellLevels)));
                    break;

                case "urgot":
                    SpellLevels = new int[] { 3, 1, 2, 1, 1, 4, 1, 2, 1, 2, 4, 2, 2, 3, 3, 4, 3, 3 };
                    LocalMenu.AddItem(new MenuItem("AutoLevel." + championName, GetLevelStr(SpellLevels)));
                    break;

                case "vayne":
                    SpellLevels = new int[] { 1, 3, 2, 2, 2, 4, 2, 1, 2, 1, 4, 1, 1, 3, 3, 4, 3, 3 };
                    LocalMenu.AddItem(new MenuItem("AutoLevel." + championName, GetLevelStr(SpellLevels)));
                    break;

                case "varus":
                    SpellLevels = new int[] { 1, 2, 3, 1, 1, 4, 1, 2, 1, 2, 4, 2, 2, 3, 3, 4, 3, 3 };
                    LocalMenu.AddItem(new MenuItem("AutoLevel." + championName, GetLevelStr(SpellLevels)));
                    break;
            }
            LocalMenu.Item("AutoLevel.Active").SetValue(new KeyBind("L".ToCharArray()[0], KeyBindType.Toggle, false));

            Program.Config.AddSubMenu(LocalMenu);
            Game.OnUpdate += Game_OnUpdate;

        }
        
        private static string GetLevelStr(int[] spellLevels)
        {
            var x = "";
            foreach (var i in spellLevels)
            {
                switch (i)
                {
                    case 1:
                        x += "Q";
                        break;
                    case 2:
                        x += "W";
                        break;
                    case 3:
                        x += "E";
                        break;
                    case 4:
                        x += "R";
                        break;
                }
                x += " - ";
            }

            if (x != "")
            {
                return x.Substring(0, x.Length - 2);
            }

            return "";
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (!LocalMenu.Item("AutoLevel.Active").GetValue<KeyBind>().Active)
            {
                return;
            }

            var qLevel = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Level;
            var wLevel = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Level;
            var eLevel = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Level;
            var rLevel = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Level;

            if (qLevel + wLevel + eLevel + rLevel >= ObjectManager.Player.Level)
            {
                return;
            }

            var level = new int[] { 0, 0, 0, 0 };
            for (var i = 0; i < ObjectManager.Player.Level; i++)
            {
                level[SpellLevels[i] - 1] = level[SpellLevels[i] - 1] + 1;
            }

            if (qLevel < level[0])
            {
                ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.Q);
            }

            if (wLevel < level[1])
            {
                ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.W);
            }

            if (eLevel < level[2])
            {
                ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.E);
            }

            if (rLevel < level[3])
            {
                ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.R);
            }
        }
    }
}
