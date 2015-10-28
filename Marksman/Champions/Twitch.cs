#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using SharpDX.Direct3D9;

#endregion

namespace Marksman.Champions
{

    internal class Twitch : Champion
    {
        internal class EnemyMarker
        {
            public string ChampionName { get; set; }
            public double ExpireTime { get; set; }
            public int BuffCount { get; set; }
        }
        public static Font font;
        public static Spell W;
        public static Spell E;
        private static readonly List<EnemyMarker> xEnemyMarker = new List<EnemyMarker>();
        private static bool canCastE = false;
        public Twitch()
        {
            W = new Spell(SpellSlot.W, 950);
            W.SetSkillshot(0.25f, 120f, 1400f, false, SkillshotType.SkillshotCircle);
            E = new Spell(SpellSlot.E, 1200);

            Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
            Utility.HpBarDamageIndicator.Enabled = true;
            /*
            font = new Font(
                Drawing.Direct3DDevice,
                new FontDescription
                {
                    FaceName = "Segoe UI",
                    Height = 45,
                    OutputPrecision = FontPrecision.Default,
                    Quality = FontQuality.Default
                });
            */
            Utils.Utils.PrintMessage("Twitch loaded.");
        }

        private static void OnDeleteObject(GameObject sender, EventArgs args)
        {
            if ((sender.Name.ToLower().Contains("twitch_poison_counter_06.troy")))
            {
                canCastE = false;
            }
        }

        private static void OnCreateObject(GameObject sender, EventArgs args)
        {
                if ((sender.Name.ToLower().Contains("twitch_poison_counter_06.troy")))
                {
                    canCastE = true;
                }
        }

        public override void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var t = target as Obj_AI_Hero;
            if (t == null || (!ComboActive && !HarassActive) || !unit.IsMe)
                return;

            var useW = GetValue<bool>("UseW" + (ComboActive ? "C" : "H"));

            if (useW && W.IsReady())
                W.Cast(t, false, true);
        }

        public override void Drawing_OnDraw(EventArgs args)
        {
            /*
                xEnemyMarker.Clear();
                foreach (
                    var xEnemy in
                        HeroManager.Enemies.Where(
                            tx => tx.IsEnemy && !tx.IsDead && ObjectManager.Player.Distance(tx) < E.Range))
                {
                    foreach (var buff in xEnemy.Buffs.Where(buff => buff.Name.Contains("twitchdeadlyvenom")))
                    {
                        xEnemyMarker.Add(new EnemyMarker
                        {
                            ChampionName = xEnemy.ChampionName,
                            ExpireTime = Game.Time + 6,
                            BuffCount = buff.Count
                        });
                    }
                }

                foreach (var markedEnemies in xEnemyMarker)
                {
                    foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (enemy.IsEnemy && !enemy.IsDead && ObjectManager.Player.Distance(enemy) <= E.Range &&
                            enemy.ChampionName == markedEnemies.ChampionName)
                        {
                            if (!(markedEnemies.ExpireTime > Game.Time))
                            {
                                continue;
                            }
                            var xCoolDown = TimeSpan.FromSeconds(markedEnemies.ExpireTime - Game.Time);
                            var display = string.Format("{0}", markedEnemies.BuffCount);
                            Utils.Utils.DrawText(font, display, (int)enemy.HPBarPosition.X - 10, (int)enemy.HPBarPosition.Y, SharpDX.Color.Wheat);
                        }
                    }
                }

            */
            Spell[] spellList = {W};
            foreach (var spell in spellList)
            {
                var menuItem = GetValue<Circle>("Draw" + spell.Slot);
                if (menuItem.Active)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, spell.Range, menuItem.Color);
            }
        }

        public override void Game_OnGameUpdate(EventArgs args)
        {
            //foreach (var e in HeroManager.Enemies)
            //{
            //    foreach (var b in e.Buffs.Where(b => b.DisplayName.ToLower() == "twitchdeadlyvenom"))
            //    {
            //        Game.PrintChat(e.ChampionName + " : " + b.Count);
            //    }
            //}

            var killableMinionCount = 0;
            foreach (
                var m in
                    MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range)
                        .Where(x => E.CanCast(x) && x.Health <= E.GetDamage(x)))
            {
                if (m.SkinName == "SRU_ChaosMinionSiege" || m.SkinName == "SRU_ChaosMinionSuper")
                    killableMinionCount += 2;
                else
                    killableMinionCount++;
                Render.Circle.DrawCircle(m.Position, (float) (m.BoundingRadius*1.5), Color.White);
            }

            if (killableMinionCount >= 3 && E.IsReady() && ObjectManager.Player.ManaPercent > 15)
            {
                E.Cast();
            }

            foreach (
                var m in
                    MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range, MinionTypes.All,
                        MinionTeam.Neutral).Where(m => E.CanCast(m) && m.Health <= E.GetDamage(m)))
            {
                if (m.SkinName.ToLower().Contains("baron") || m.SkinName.ToLower().Contains("dragon") && E.CanCast(m))
                    E.Cast(m);
                else
                    Render.Circle.DrawCircle(m.Position, (float) (m.BoundingRadius*1.5), Color.White);
            }

            if (Orbwalking.CanMove(100) && (ComboActive || HarassActive))
            {
                var useW = GetValue<bool>("UseW" + (ComboActive ? "C" : "H"));
                var useE = GetValue<bool>("UseE" + (ComboActive ? "C" : "H"));

                if (useW)
                {
                    var wTarget = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);
                    if (W.IsReady() && wTarget.IsValidTarget())
                        W.Cast(wTarget, false, true);
                }

                if (useE && E.IsReady())
                {
                    if (useE && canCastE && E.IsReady())
                    {
                        E.Cast();
                    }

                    var eTarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
                    if (eTarget.IsValidTarget(E.Range))
                    {
                        foreach (
                            var buff in
                                eTarget.Buffs.Where(buff => buff.DisplayName.ToLower() == "twitchdeadlyvenom")
                                    .Where(buff => buff.Count == 6))
                        {
                            E.Cast();
                        }
                    }
                }
            }

            if (GetValue<bool>("UseEM") && E.IsReady())
            {
                foreach (
                    var hero in
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(
                                hero =>
                                    hero.IsValidTarget(E.Range) &&
                                    (ObjectManager.Player.GetSpellDamage(hero, SpellSlot.E) - 10 > hero.Health)))
                {
                    E.Cast();
                }
            }
        }

        private static float GetComboDamage(Obj_AI_Hero t)
        {
            var fComboDamage = 0f;

            if (E.IsReady())
                fComboDamage += (float) ObjectManager.Player.GetSpellDamage(t, SpellSlot.E);

            if (ObjectManager.Player.GetSpellSlot("summonerdot") != SpellSlot.Unknown &&
                ObjectManager.Player.Spellbook.CanUseSpell(ObjectManager.Player.GetSpellSlot("summonerdot")) ==
                SpellState.Ready && ObjectManager.Player.Distance(t) < 550)
                fComboDamage += (float) ObjectManager.Player.GetSummonerSpellDamage(t, Damage.SummonerSpell.Ignite);

            if (Items.CanUseItem(3144) && ObjectManager.Player.Distance(t) < 550)
                fComboDamage += (float) ObjectManager.Player.GetItemDamage(t, Damage.DamageItems.Bilgewater);

            if (Items.CanUseItem(3153) && ObjectManager.Player.Distance(t) < 550)
                fComboDamage += (float) ObjectManager.Player.GetItemDamage(t, Damage.DamageItems.Botrk);

            return fComboDamage;
        }

        public override bool ComboMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseWC" + Id, "Use W").SetValue(true));
            config.AddItem(new MenuItem("UseEC" + Id, "Use E max Stacks").SetValue(true));
            return true;
        }

        public override bool HarassMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseWH" + Id, "Use W").SetValue(false));
            config.AddItem(new MenuItem("UseEH" + Id, "Use E at max Stacks").SetValue(false));
            return true;
        }

        public override bool DrawingMenu(Menu config)
        {
            config.AddItem(
                new MenuItem("DrawW" + Id, "W range").SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));

            var dmgAfterComboItem = new MenuItem("DamageAfterCombo", "Damage After Combo").SetValue(true);
            config.AddItem(dmgAfterComboItem);

            return true;
        }

        public override bool MiscMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseEM" + Id, "Use E KS").SetValue(true));
            return true;
        }

        public override bool LaneClearMenu(Menu config)
        {

            return true;
        }
        public override bool JungleClearMenu(Menu config)
        {
            return true;
        }
    }
}
