using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace Marksman.Utils
{
    internal class IncomingDangerous
    {
        private static MenuItem menuItem;
        private static Obj_AI_Base incomingDangerous = null;
        private static Spell ChampionSpell;
        public static void Initialize()
        {
            menuItem =
                new MenuItem("Activator.IncomingDangerous", "Incoming Dangerous").SetValue(
                    new StringList(new[] {"Off", "Warn with Ping", "Warn with Line", "Both"}, 3));
            Program.MenuActivator.AddItem(menuItem);

            ChampionSpell = GetSpell();

            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnBuffAdd += Obj_AI_Base_OnBuffAdd;
        }

        private static void Obj_AI_Base_OnBuffAdd(Obj_AI_Base sender, Obj_AI_BaseBuffAddEventArgs args)
        {
            var IncomingDangerous = Program.MenuActivator.Item("Activator.IncomingDangerous").GetValue<StringList>().SelectedIndex;
            if (IncomingDangerous != 0)
            {
                BuffInstance aBuff =
                    (from fBuffs in
                         sender.Buffs.Where(
                             s => sender.IsEnemy && sender.Distance(ObjectManager.Player.Position) < 2500)
                     from b in new[]
                                   {
                                       // TODO: Add a ping warning and draw line to teleport position 
                                       "teleport_", "pantheon_grandskyfall_jump", "crowstorm", "gate",
                                   }
                     where args.Buff.Name.ToLower().Contains(b)
                     select fBuffs).FirstOrDefault();

                incomingDangerous = null;
                if (aBuff != null)
                {
                    if (IncomingDangerous == 1 || IncomingDangerous == 3)
                    {
                        Utils.MPing.Ping(sender.Position.To2D(), 3);
                    }
                    if (IncomingDangerous == 2 || IncomingDangerous == 3)
                    {
                        incomingDangerous = sender;
                    }
                }
            }

            //if (ObjectManager.Player.ChampionName == "Jinx" || ObjectManager.Player.ChampionName == "Caitlyn" || ObjectManager.Player.ChampionName == "Teemo")
            //{
            //    if (ChampionSpell.IsReady())
            //    {
            //        BuffInstance aBuff =
            //            (from fBuffs in
            //                sender.Buffs.Where(
            //                    s =>
            //                        sender.Team != ObjectManager.Player.Team
            //                        && sender.Distance(ObjectManager.Player.Position) < ChampionSpell.Range)
            //                from b in new[]
            //                {
            //                    "teleport_", /* Teleport */ "pantheon_grandskyfall_jump", /* Pantheon */ 
            //                    "crowstorm", /* FiddleScitck */
            //                    "zhonya", "katarinar", /* Katarita */
            //                    "MissFortuneBulletTime", /* MissFortune */
            //                    "gate", /* Twisted Fate */
            //                    "chronorevive" /* Zilean */
            //                }
            //                where args.Buff.Name.ToLower().Contains(b)
            //                select fBuffs).FirstOrDefault();

            //        if (aBuff != null)
            //        {
            //            ChampionSpell.Cast(sender.Position);
            //        }
            //    }
            //}
        }

        private static Spell GetSpell()
        {
            switch (ObjectManager.Player.ChampionName)
            {
                case "Jinx":
                    {
                        return new Spell(SpellSlot.E, 900f);
                    }
                case "Caitlyn":
                    {
                        return new Spell(SpellSlot.W, 820);
                    }
                case "Teemo":
                    {
                        return new Spell(SpellSlot.R, ObjectManager.Player.Level * 300);
                    }
            }
            return null;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            var sender = incomingDangerous;
            if (sender != null)
            {
                Utils.DrawLine(ObjectManager.Player.Position, sender.Position, System.Drawing.Color.Red,
                    " !!!!! Danger !!!!!");
            }
        }
    }
}
