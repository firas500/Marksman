#region

using System;
using System.Drawing;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Marksman.Champions
{
    internal class Caitlyn : Champion
    {
        public static Spell R;

        public Spell E;

        public Spell Q;

        public bool ShowUlt;

        public string UltTarget;

        public Spell W;

        private bool canCastR = true;

        public Caitlyn()
        {
            Utils.Utils.PrintMessage("Caitlyn loaded.");

            this.Q = new Spell(SpellSlot.Q, 1240);
            this.W = new Spell(SpellSlot.W, 820);
            this.E = new Spell(SpellSlot.E, 800);
            R = new Spell(SpellSlot.R, 2000);

            this.Q.SetSkillshot(0.25f, 60f, 2000f, false, SkillshotType.SkillshotLine);
            this.E.SetSkillshot(0.25f, 80f, 1600f, true, SkillshotType.SkillshotLine);

            AntiGapcloser.OnEnemyGapcloser += this.AntiGapcloser_OnEnemyGapcloser;
            Drawing.OnEndScene += DrawingOnOnEndScene;
            Obj_AI_Base.OnProcessSpellCast += this.Obj_AI_Hero_OnProcessSpellCast;
        }

        public void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (this.E.IsReady() && gapcloser.Sender.IsValidTarget(this.E.Range))
            {
                this.E.CastOnUnit(gapcloser.Sender);
            }
        }

        public void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsEnemy && sender is Obj_AI_Turret && args.Target.IsMe)
            {
                this.canCastR = false;
            }
            else
            {
                this.canCastR = true;
            }
        }

        public override void Drawing_OnDraw(EventArgs args)
        {
            Spell[] spellList = { this.Q, this.E, R };
            foreach (var spell in spellList)
            {
                var menuItem = this.GetValue<Circle>("Draw" + spell.Slot);
                if (menuItem.Active) Render.Circle.DrawCircle(ObjectManager.Player.Position, spell.Range, menuItem.Color);
            }

            var drawUlt = this.GetValue<Circle>("DrawUlt");
            if (drawUlt.Active && this.ShowUlt)
            {
                //var playerPos = Drawing.WorldToScreen(ObjectManager.Player.Position);
                //Drawing.DrawText(playerPos.X - 65, playerPos.Y + 20, drawUlt.Color, "Hit R To kill " + UltTarget + "!");
            }
        }

        private static void DrawingOnOnEndScene(EventArgs args)
        {
            var rCircle2 = Program.Config.Item("Draw.UltiMiniMap").GetValue<Circle>();
            if (rCircle2.Active)
            {
                Utility.DrawCircle(ObjectManager.Player.Position, R.Range, rCircle2.Color, 1, 23, true);
            }
        }

        public override void Game_OnGameUpdate(EventArgs args)
        {
            R.Range = 500 * (R.Level == 0 ? 1 : R.Level) + 1500;

            Obj_AI_Hero t;

            if (this.W.IsReady() && this.GetValue<bool>("AutoWI"))
            {
                t = TargetSelector.GetTarget(this.W.Range, TargetSelector.DamageType.Physical);
                if (t.IsValidTarget(this.W.Range)
                    && (t.HasBuffOfType(BuffType.Stun) || t.HasBuffOfType(BuffType.Snare)
                        || t.HasBuffOfType(BuffType.Taunt) || t.HasBuff("zhonyasringshield") || t.HasBuff("Recall")))
                {
                    this.W.Cast(t.Position);
                }
            }



            if (this.Q.IsReady() && this.GetValue<bool>("AutoQI"))
            {
                t = TargetSelector.GetTarget(this.Q.Range, TargetSelector.DamageType.Physical);
                if (t.IsValidTarget(this.Q.Range)
                    && (t.HasBuffOfType(BuffType.Stun) || t.HasBuffOfType(BuffType.Snare)
                        || t.HasBuffOfType(BuffType.Taunt)
                        && (t.Health <= ObjectManager.Player.GetSpellDamage(t, SpellSlot.Q)
                            || !Orbwalking.InAutoAttackRange(t))))
                {
                    this.Q.Cast(t, false, true);
                }
            }

            if (R.IsReady())
            {
                t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
                if (t.IsValidTarget(R.Range) && t.Health <= R.GetDamage(t))
                {
                    if (this.GetValue<KeyBind>("UltHelp").Active && this.canCastR) R.Cast(t);

                    this.UltTarget = t.ChampionName;
                    this.ShowUlt = true;
                }
                else
                {
                    this.ShowUlt = false;
                }
            }
            else
            {
                this.ShowUlt = false;
            }

            if (this.GetValue<KeyBind>("Dash").Active && this.E.IsReady())
            {
                var pos = ObjectManager.Player.ServerPosition.To2D().Extend(Game.CursorPos.To2D(), -300).To3D();
                this.E.Cast(pos, true);
            }

            if (this.GetValue<KeyBind>("UseEQC").Active && this.E.IsReady() && this.Q.IsReady())
            {
                t = TargetSelector.GetTarget(this.E.Range, TargetSelector.DamageType.Physical);
                if (t.IsValidTarget(this.E.Range)
                    && t.Health
                    < ObjectManager.Player.GetSpellDamage(t, SpellSlot.Q)
                    + ObjectManager.Player.GetSpellDamage(t, SpellSlot.E) + 20 && this.E.CanCast(t))
                {
                    this.E.Cast(t);
                    this.Q.Cast(t, false, true);
                }
            }

            // PQ you broke it D:
            if ((!this.ComboActive && !this.HarassActive) || !Orbwalking.CanMove(100)) return;

            var useQ = this.GetValue<bool>("UseQ" + (this.ComboActive ? "C" : "H"));
            var useE = this.GetValue<bool>("UseEC");
            var useR = this.GetValue<bool>("UseRC");

            if (this.Q.IsReady() && useQ)
            {
                t = TargetSelector.GetTarget(this.Q.Range, TargetSelector.DamageType.Physical);
                if (t != null)
                {
                    this.Q.Cast(t, false, true);
                }
            }
            else if (this.E.IsReady() && useE)
            {
                t = TargetSelector.GetTarget(this.E.Range, TargetSelector.DamageType.Physical);
                if (t != null && t.Health <= this.E.GetDamage(t))
                {
                    this.E.Cast(t);
                }
            }

            if (R.IsReady() && useR)
            {
                t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
                if (t != null && t.Health <= R.GetDamage(t) && !Orbwalking.InAutoAttackRange(t) && this.canCastR)
                {
                    R.CastOnUnit(t);
                }
            }
        }

        public override void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var t = target as Obj_AI_Hero;
            if (t == null || (!this.ComboActive && !this.HarassActive) || unit.IsMe) return;

            var useQ = this.GetValue<bool>("UseQ" + (this.ComboActive ? "C" : "H"));
            if (useQ) this.Q.Cast(t, false, true);
        }

        public override bool ComboMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseQC" + this.Id, "Use Q").SetValue(true));
            config.AddItem(new MenuItem("UseEC" + this.Id, "Use E").SetValue(true));
            config.AddItem(new MenuItem("UseRC" + this.Id, "Use R").SetValue(true));
            return true;
        }

        public override bool HarassMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseQH" + this.Id, "Use Q").SetValue(true));
            return true;
        }

        public override bool DrawingMenu(Menu config)
        {
            config.AddItem(new MenuItem("Champion.Drawings", ObjectManager.Player.ChampionName + " Draw Options"));
            config.AddItem(
                new MenuItem("DrawQ" + this.Id, Program.Tab + "Q range").SetValue(
                    new Circle(true, Color.FromArgb(100, 255, 0, 255))));
            config.AddItem(
                new MenuItem("DrawE" + this.Id, Program.Tab + "E range").SetValue(
                    new Circle(false, Color.FromArgb(100, 255, 255, 255))));
            config.AddItem(
                new MenuItem("DrawR" + this.Id, Program.Tab + "R range").SetValue(
                    new Circle(false, Color.FromArgb(100, 255, 255, 255))));
            config.AddItem(
                new MenuItem("DrawUlt" + this.Id, Program.Tab + "Ult Text").SetValue(
                    new Circle(true, Color.FromArgb(255, 255, 255, 255))));
            config.AddItem(
                new MenuItem("Draw.UltiMiniMap", Program.Tab + "Draw Ulti Minimap").SetValue(
                    new Circle(true, Color.FromArgb(255, 255, 255, 255))));
            return true;
        }

        public override bool MiscMenu(Menu config)
        {
            config.AddItem(new MenuItem("UltHelp" + this.Id, "Ult Target on R").SetValue(new KeyBind("R".ToCharArray()[0], KeyBindType.Press)));
            config.AddItem(new MenuItem("UseEQC" + this.Id, "Use E-Q Combo").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
            config.AddItem(new MenuItem("Dash" + this.Id, "Dash to Mouse").SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));
            config.AddItem(new MenuItem("AutoQI" + this.Id, "Auto Q (Stun/Snare/Taunt/Slow)").SetValue(true));
            config.AddItem(new MenuItem("AutoWI" + this.Id, "Auto W (Stun/Snare/Taunt)").SetValue(true));

            return true;
        }


        public override bool LaneClearMenu(Menu config)
        {
            return true;
        }

        public override bool JungleClearMenu(Menu config)
        {
            return false;
        }

    }
}
