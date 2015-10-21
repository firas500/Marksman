#region

using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Marksman.Champions
{
    using Utils = LeagueSharp.Common.Utils;

    internal class Jinx : Champion
    {
        public static Spell Q, W, E, R;

        public Jinx()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 1500f);
            E = new Spell(SpellSlot.E, 900f);
            R = new Spell(SpellSlot.R, 25000f);

            W.SetSkillshot(0.6f, 60f, 3300f, true, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.7f, 120f, 1750f, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.6f, 140f, 1700f, false, SkillshotType.SkillshotLine);

            Obj_AI_Base.OnBuffAdd += (sender, args) =>
                {
                    BuffInstance aBuff =
                        (from fBuffs in
                             sender.Buffs.Where(
                                 s =>
                                 sender.Team != ObjectManager.Player.Team
                                 && sender.Distance(ObjectManager.Player.Position) < E.Range)
                         from b in
                             new[]
                                 {
                                     "teleport_target", "pantheon_grandskyfall_jump", "crowstorm", "zhonya", "katarinar",
                                     "MissFortuneBulletTime"
                                 }
                         where b.Contains(args.Buff.Name.ToLower())
                         select fBuffs).FirstOrDefault();

                    if (aBuff != null && E.IsReady())
                    {
                        E.Cast(sender.Position);
                    }
                };
            Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
            Utility.HpBarDamageIndicator.Enabled = true;

            Marksman.Utils.Utils.PrintMessage("Jinx loaded.");
        }

        private static float GetComboDamage(Obj_AI_Hero t)
        {
            var fComboDamage = 0f;

            fComboDamage += R.IsReady() ? R.GetDamage(t) : 0;

            if (ObjectManager.Player.GetSpellSlot("summonerdot") != SpellSlot.Unknown
                && ObjectManager.Player.Spellbook.CanUseSpell(ObjectManager.Player.GetSpellSlot("summonerdot"))
                == SpellState.Ready && ObjectManager.Player.Distance(t) < 550) fComboDamage += (float)ObjectManager.Player.GetSummonerSpellDamage(t, Damage.SummonerSpell.Ignite);

            if (Items.CanUseItem(3144) && ObjectManager.Player.Distance(t) < 550) fComboDamage += (float)ObjectManager.Player.GetItemDamage(t, Damage.DamageItems.Bilgewater);

            if (Items.CanUseItem(3153) && ObjectManager.Player.Distance(t) < 550) fComboDamage += (float)ObjectManager.Player.GetItemDamage(t, Damage.DamageItems.Botrk);

            return fComboDamage;
        }


        public float QAddRange
        {
            get
            {
                return 50 + 25 * ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Level;
            }
        }

        private static bool FishBoneActive
        {
            get
            {
                return ObjectManager.Player.AttackRange > 565f;
            }
        }

        private static int PowPowStacks
        {
            get
            {
                return
                    ObjectManager.Player.Buffs.Where(buff => buff.DisplayName.ToLower() == "jinxqramp")
                        .Select(buff => buff.Count)
                        .FirstOrDefault();
            }
        }

        public override void Drawing_OnDraw(EventArgs args)
        {

            if (GetValue<bool>("DrawCH"))
            {
                var enemies = HeroManager.Enemies.Where(e => e.IsEnemy);
                //e.IsVisible && e.Health < R.GetDamage(e) && !e.IsDead && !e.IsZombie);
                var objAiHeroes = enemies as Obj_AI_Hero[] ?? enemies.ToArray();

                Marksman.Utils.Utils.DrawText(
                    Marksman.Utils.Utils.SmallText,
                    "Kill with Ulti!",
                    Drawing.Width * 0.895f,
                    Drawing.Height * 0.419f,
                    SharpDX.Color.Wheat);
                for (var i = 0; i < objAiHeroes.Count(); i++)
                {
                    Drawing.DrawLine(
                        Drawing.Width * 0.892f + 0,
                        Drawing.Height * 0.419f + (float)(i + 1) * 20,
                        Drawing.Width * 0.895f + 150,
                        Drawing.Height * 0.419f + (float)(i + 1) * 20,
                        16,
                        Color.Black);
                    Drawing.DrawLine(
                        Drawing.Width * 0.892f + 1,
                        Drawing.Height * 0.420f + (float)(i + 1) * 20,
                        Drawing.Width * 0.895f + 149,
                        Drawing.Height * 0.420f + (float)(i + 1) * 20,
                        14,
                        Color.BurlyWood);

                    var hPercent = objAiHeroes[i].HealthPercent;
                    if (hPercent > 0)
                    {
                        Drawing.DrawLine(
                            Drawing.Width * 0.892f + 1,
                            Drawing.Height * 0.420f + (float)(i + 1) * 20,
                            Drawing.Width * 0.895f + hPercent - 1,
                            Drawing.Height * 0.420f + (float)(i + 1) * 20,
                            14,
                            hPercent < 50 && hPercent > 30
                                ? Color.Yellow
                                : objAiHeroes[i].Health <= R.GetDamage(objAiHeroes[i])
                                      ? Color.Red
                                      : Color.DarkOliveGreen);
                    }
                    Marksman.Utils.Utils.DrawText(
                        Marksman.Utils.Utils.SmallText,
                        objAiHeroes[i].ChampionName,
                        Drawing.Width * 0.895f,
                        Drawing.Height * 0.42f + (float)(i + 1) * 20,
                        SharpDX.Color.Black);
                }
            }
            /*----------------------------------------------------*/
            var drawQbound = GetValue<Circle>("DrawQBound");
            foreach (var spell in new[] { W, E })
            {
                var menuItem = GetValue<Circle>("Draw" + spell.Slot);
                if (menuItem.Active)
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, spell.Range, menuItem.Color);
                }
            }

            if (drawQbound.Active)
            {
                if (FishBoneActive)
                {
                    Render.Circle.DrawCircle(
                        ObjectManager.Player.Position,
                        525f + ObjectManager.Player.BoundingRadius + 65f,
                        drawQbound.Color);
                }
                else
                {
                    Render.Circle.DrawCircle(
                        ObjectManager.Player.Position,
                        525f + ObjectManager.Player.BoundingRadius + 65f + QAddRange + 20f,
                        drawQbound.Color);
                }
            }
        }

        public override void Game_OnGameUpdate(EventArgs args)
        {
            //if (Q.IsReady() && GetValue<bool>("SwapDistance") && Program.CClass.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            if (Q.IsReady() && this.GetValue<bool>("SwapDistance"))
            {
                var activeQ = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Level * 25 + 650;
                var t = TargetSelector.GetTarget(activeQ, TargetSelector.DamageType.Physical);

                if (t.IsValidTarget() && ObjectManager.Player.Distance(t) > Orbwalking.GetRealAutoAttackRange(null) + 65)
                {
                    if (!FishBoneActive)
                    {
                        Q.Cast();
                        this.Orbwalker.ForceTarget(t);
                        return;
                    }
                }
                if (!t.IsValidTarget() && FishBoneActive)
                {
                    Q.Cast();
                    return;
                }

            }

            if (GetValue<bool>("PingCH"))
            {
                foreach (var enemy in
                    HeroManager.Enemies.Where(
                        t =>
                        R.IsReady() && t.IsValidTarget() && R.GetDamage(t) > t.Health
                        && t.Distance(ObjectManager.Player) > Orbwalking.GetRealAutoAttackRange(null) + 65 + QAddRange))
                {
                    Marksman.Utils.Utils.MPing.Ping(enemy.Position.To2D());
                }
            }

            var autoEi = GetValue<bool>("AutoEI");
            var autoEs = GetValue<bool>("AutoES");
            var autoEd = GetValue<bool>("AutoED");

            if (autoEs || autoEi || autoEd)
            {
                foreach (
                    var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsValidTarget(E.Range - 150)))
                {
                    if (autoEs && E.IsReady() && enemy.HasBuffOfType(BuffType.Slow))
                    {
                        var castPosition =
                            Prediction.GetPrediction(
                                new PredictionInput
                                    {
                                        Unit = enemy, Delay = 0.7f, Radius = 120f, Speed = 1750f, Range = 900f,
                                        Type = SkillshotType.SkillshotCircle
                                    }).CastPosition;


                        if (GetSlowEndTime(enemy) >= (Game.Time + E.Delay + 0.5f))
                        {
                            E.Cast(castPosition);
                        }
                    }

                    if (autoEi && E.IsReady()
                        && (enemy.HasBuffOfType(BuffType.Stun) || enemy.HasBuffOfType(BuffType.Snare)
                            || enemy.HasBuffOfType(BuffType.Charm) || enemy.HasBuffOfType(BuffType.Fear)
                            || enemy.HasBuffOfType(BuffType.Taunt) || enemy.HasBuff("zhonyasringshield")
                            || enemy.HasBuff("Recall")))
                    {
                        E.CastIfHitchanceEquals(enemy, HitChance.High);
                    }

                    if (autoEd && E.IsReady() && enemy.IsDashing())
                    {
                        E.CastIfHitchanceEquals(enemy, HitChance.Dashing);
                    }
                }
            }


            if (GetValue<KeyBind>("CastR").Active && R.IsReady())
            {
                var t = TargetSelector.GetTarget(1500, TargetSelector.DamageType.Physical);
                if (t.IsValidTarget())
                {
                    if (ObjectManager.Player.GetSpellDamage(t, SpellSlot.R) > t.Health && !t.IsZombie)
                    {
                        //R.Cast(target);
                        R.CastIfHitchanceEquals(t, HitChance.High, false);
                    }
                }
            }

            if (this.GetValue<bool>("SwapQ") && FishBoneActive && !this.ComboActive)
            {
                Q.Cast();
            }
            /*
            if (GetValue<bool>("SwapQ") && FishBoneActive && (LaneClearActive ||
                 (HarassActive && TargetSelector.GetTarget(675f + QAddRange, TargetSelector.DamageType.Physical) == null)))
            {
                Q.Cast();
            }
            */

            if ((!ComboActive && !HarassActive) || !Orbwalking.CanMove(100))
            {
                return;
            }

            var useW = GetValue<bool>("UseW" + (ComboActive ? "C" : "H"));
            var useR = GetValue<bool>("UseRC");

            if (useW && W.IsReady())
            {
                var t = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);
                var minW = GetValue<Slider>("MinWRange").Value;

                if (t.IsValidTarget() && GetRealDistance(t) >= minW)
                {
                    if (W.Cast(t) == Spell.CastStates.SuccessfullyCasted)
                    {
                        return;
                    }
                }
            }
            /*
            if (useQ)
            {
                foreach (var t in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(t => t.IsValidTarget(GetRealPowPowRange(t) + QAddRange + 20f)))
                {
                    var swapDistance = GetValue<bool>("SwapDistance");
                    var swapAoe = GetValue<bool>("SwapAOE");
                    var distance = GetRealDistance(t);
                    var powPowRange = GetRealPowPowRange(t);

                    if (swapDistance && Q.IsReady())
                    {
                        if (distance > powPowRange && !FishBoneActive)
                        {
                            if (Q.Cast())
                            {
                                return;
                            }
                        }
                        else if (distance < powPowRange && FishBoneActive)
                        {
                            if (Q.Cast())
                            {
                                return;
                            }
                        }
                    }

                    if (swapAoe && Q.IsReady())
                    {
                        if (distance > powPowRange && PowPowStacks > 2 && !FishBoneActive && CountEnemies(t, 150) > 1)
                        {
                            if (Q.Cast())
                            {
                                return;
                            }
                        }
                    }
                }
            }

            */
            if (useR && R.IsReady())
            {
                var checkRok = GetValue<bool>("ROverKill");
                var minR = GetValue<Slider>("MinRRange").Value;
                var maxR = GetValue<Slider>("MaxRRange").Value;
                var t = TargetSelector.GetTarget(maxR, TargetSelector.DamageType.Physical);

                if (t.IsValidTarget())
                {
                    var distance = GetRealDistance(t);

                    if (!checkRok)
                    {
                        if (ObjectManager.Player.GetSpellDamage(t, SpellSlot.R, 1) > t.Health && !t.IsZombie)
                        {
                            R.CastIfHitchanceEquals(t, HitChance.High, false);
                            //if (R.Cast(t) == Spell.CastStates.SuccessfullyCasted) { }
                        }
                    }
                    else if (distance > minR)
                    {
                        var aDamage = ObjectManager.Player.GetAutoAttackDamage(t);
                        var wDamage = ObjectManager.Player.GetSpellDamage(t, SpellSlot.W);
                        var rDamage = ObjectManager.Player.GetSpellDamage(t, SpellSlot.R);
                        var powPowRange = GetRealPowPowRange(t);

                        if (distance < (powPowRange + QAddRange) && !(aDamage * 3.5 > t.Health))
                        {
                            if (!W.IsReady() || !(wDamage > t.Health) || W.GetPrediction(t).CollisionObjects.Count > 0)
                            {
                                if (CountAlliesNearTarget(t, 500) <= 3)
                                {
                                    if (rDamage > t.Health && !t.IsZombie /*&& !ObjectManager.Player.IsAutoAttacking &&
                                        !ObjectManager.Player.IsChanneling*/)
                                    {
                                        R.CastIfHitchanceEquals(t, HitChance.High, false);
                                        //if (R.Cast(t) == Spell.CastStates.SuccessfullyCasted) { }
                                    }
                                }
                            }
                        }
                        else if (distance > (powPowRange + QAddRange))
                        {
                            if (!W.IsReady() || !(wDamage > t.Health) || distance > W.Range
                                || W.GetPrediction(t).CollisionObjects.Count > 0)
                            {
                                if (CountAlliesNearTarget(t, 500) <= 3)
                                {
                                    if (rDamage > t.Health && !t.IsZombie /*&& !ObjectManager.Player.IsAutoAttacking &&
                                        !ObjectManager.Player.IsChanneling*/)
                                    {
                                        R.CastIfHitchanceEquals(t, HitChance.High, false);
                                        //if (R.Cast(t) == Spell.CastStates.SuccessfullyCasted) { }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if ((ComboActive || HarassActive) && unit.IsMe && (target is Obj_AI_Hero))
            {
                var useQ = GetValue<bool>("UseQ" + (ComboActive ? "C" : "H"));
                var useW = GetValue<bool>("UseW" + (ComboActive ? "C" : "H"));

                if (useW && W.IsReady())
                {
                    var t = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);
                    var minW = GetValue<Slider>("MinWRange").Value;

                    if (t.IsValidTarget() && GetRealDistance(t) >= minW)
                    {
                        if (W.Cast(t) == Spell.CastStates.SuccessfullyCasted)
                        {
                            return;
                        }
                    }
                }

                if (useQ)
                {
                    foreach (var t in
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(t => t.IsValidTarget(GetRealPowPowRange(t) + QAddRange + 20f)))
                    {
                        var swapDistance = GetValue<bool>("SwapDistance");
                        var swapAoe = GetValue<bool>("SwapAOE");
                        var distance = GetRealDistance(t);
                        var powPowRange = GetRealPowPowRange(t);

                        if (swapDistance && Q.IsReady())
                        {
                            if (distance > powPowRange && !FishBoneActive)
                            {
                                if (Q.Cast())
                                {
                                    return;
                                }
                            }
                            else if (distance < powPowRange && FishBoneActive)
                            {
                                if (Q.Cast())
                                {
                                    return;
                                }
                            }
                        }

                        if (swapAoe && Q.IsReady())
                        {
                            if (distance > powPowRange && PowPowStacks > 2 && !FishBoneActive
                                && CountEnemies(t, 150) > 1)
                            {
                                if (Q.Cast())
                                {
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

        private static int CountEnemies(Obj_AI_Base target, float range)
        {
            return
                ObjectManager.Get<Obj_AI_Hero>()
                    .Count(
                        hero =>
                        hero.IsValidTarget() && hero.Team != ObjectManager.Player.Team
                        && hero.ServerPosition.Distance(target.ServerPosition) <= range);
        }

        private int CountAlliesNearTarget(Obj_AI_Base target, float range)
        {
            return
                ObjectManager.Get<Obj_AI_Hero>()
                    .Count(
                        hero =>
                        hero.Team == ObjectManager.Player.Team
                        && hero.ServerPosition.Distance(target.ServerPosition) <= range);
        }

        private static float GetRealPowPowRange(GameObject target)
        {
            return 525f + ObjectManager.Player.BoundingRadius + target.BoundingRadius;
        }

        private static float GetRealDistance(GameObject target)
        {
            return ObjectManager.Player.Position.Distance(target.Position) + ObjectManager.Player.BoundingRadius
                   + target.BoundingRadius;
        }

        private static float GetSlowEndTime(Obj_AI_Base target)
        {
            return
                target.Buffs.OrderByDescending(buff => buff.EndTime - Game.Time)
                    .Where(buff => buff.Type == BuffType.Slow)
                    .Select(buff => buff.EndTime)
                    .FirstOrDefault();
        }

        public override bool ComboMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseQC" + Id, "Use Q").SetValue(true));
            config.AddItem(new MenuItem("UseWC" + Id, "Use W").SetValue(true));
            config.AddItem(new MenuItem("UseRC" + Id, "Use R").SetValue(true));
            config.AddItem(new MenuItem("PingCH" + Id, "Ping Killable Enemy with R").SetValue(true));
            return true;
        }

        public override bool HarassMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseQH" + Id, "Use Q").SetValue(true));
            config.AddItem(new MenuItem("UseWH" + Id, "Use W").SetValue(false));
            return true;
        }

        public override bool LaneClearMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseQL" + Id, "Use Q for minion:").SetValue(new Slider(3, 0, 6)));
            return true;
        }

        public override bool MiscMenu(Menu config)
        {
            config.AddItem(new MenuItem("SwapQ" + Id, "Always swap to Minigun").SetValue(false));
            config.AddItem(new MenuItem("SwapDistance" + Id, "Swap Q for distance").SetValue(true));
            config.AddItem(new MenuItem("SwapAOE" + Id, "Swap Q for AOE").SetValue(false));
            config.AddItem(new MenuItem("MinWRange" + Id, "Min W range").SetValue(new Slider(525 + 65 * 2, 0, 1200)));
            config.AddItem(new MenuItem("AutoEI" + Id, "Auto-E on immobile").SetValue(true));
            config.AddItem(new MenuItem("AutoES" + Id, "Auto-E on slowed").SetValue(true));
            config.AddItem(new MenuItem("AutoED" + Id, "Auto-E on dashing").SetValue(false));
            config.AddItem(
                new MenuItem("CastR" + Id, "Cast R (2000 Range)").SetValue(
                    new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
            config.AddItem(new MenuItem("ROverKill" + Id, "Check R Overkill").SetValue(true));
            config.AddItem(new MenuItem("MinRRange" + Id, "Min R range").SetValue(new Slider(300, 0, 1500)));
            config.AddItem(new MenuItem("MaxRRange" + Id, "Max R range").SetValue(new Slider(1700, 0, 4000)));
            return true;
        }

        public override bool DrawingMenu(Menu config)
        {
            config.AddItem(
                new MenuItem("DrawQBound" + Id, "Draw Q bound").SetValue(
                    new Circle(true, Color.FromArgb(100, 255, 0, 0))));
            config.AddItem(new MenuItem("DrawE" + Id, "E range").SetValue(new Circle(false, Color.CornflowerBlue)));
            config.AddItem(new MenuItem("DrawW" + Id, "W range").SetValue(new Circle(false, Color.CornflowerBlue)));
            config.AddItem(new MenuItem("DrawCH" + Id, "Draw Killable Enemy with R").SetValue(true));
            return true;
        }

        public override bool JungleClearMenu(Menu config)
        {
            return false;
        }
    }
}
