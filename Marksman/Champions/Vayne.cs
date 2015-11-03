#region

using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

#endregion

namespace Marksman.Champions
{
    internal class Vayne : Champion
    {
        public static Spell Q;
        public static Spell E;

        public Vayne()
        {
            Q = new Spell(SpellSlot.Q, 300f);
            E = new Spell(SpellSlot.E, 650f);

            E.SetTargetted(0.25f, 2200f);

            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;

            Utils.Utils.PrintMessage("Vayne loaded");
        }

        public void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (GetValue<bool>("UseEGapcloser") && E.IsReady() && gapcloser.Sender.IsValidTarget(E.Range))
                E.CastOnUnit(gapcloser.Sender);
        }

        private void Interrupter2_OnInterruptableTarget(Obj_AI_Hero unit, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (GetValue<bool>("UseEInterrupt") && unit.IsValidTarget(550f))
                E.Cast(unit);
        }

        public override void Game_OnGameUpdate(EventArgs args)
        {
            if (this.JungleClearActive)
            {
                this.ExecJungleClear();
            }

            
            if ((ComboActive || HarassActive))
            {
                if (GetValue<bool>("FocusW"))
                {
                    var silverBuffMarkedEnemy = VayneData.GetSilverBuffMarkedEnemy;
                    if (silverBuffMarkedEnemy != null)
                    {
                        TargetSelector.SetTarget(silverBuffMarkedEnemy);
                    }
                    else
                    {
                        var attackRange = Orbwalking.GetRealAutoAttackRange(ObjectManager.Player);
                        TargetSelector.SetTarget(
                            TargetSelector.GetTarget(attackRange, TargetSelector.DamageType.Physical));
                    }
                }
                var useQ = GetValue<bool>("UseQ" + (ComboActive ? "C" : "H"));
                var useE = GetValue<bool>("UseE" + (ComboActive ? "C" : "H"));

                var t = TargetSelector.GetTarget(Q.Range + Orbwalking.GetRealAutoAttackRange(null) + 35, TargetSelector.DamageType.Physical);
                if (t.IsValidTarget() && useQ)
                {
                    if (!t.IsValidTarget(Orbwalking.GetRealAutoAttackRange(null) + 35))
                    {
                        Q.Cast(t.Position);
                    }
                    else
                    {
                        Q.Cast(Game.CursorPos);
                    }
                    Orbwalker.ForceTarget(t);
                }

                if (Q.IsReady() && GetValue<bool>("CompleteSilverBuff"))
                {
                    if (VayneData.GetSilverBuffMarkedEnemy != null && VayneData.GetSilverBuffMarkedCount == 2)
                    {
                        Q.Cast(Game.CursorPos);
                    }
                }

                if (E.IsReady() && useE)
                {
                    t = TargetSelector.GetTarget(E.Range + Q.Range, TargetSelector.DamageType.Physical);
                    if (t.IsValidTarget())
                    {
                        for (var i = 1; i < 8; i++)
                        {
                            var targetBehind = t.Position +
                                               Vector3.Normalize(t.ServerPosition - ObjectManager.Player.Position)*i*50;

                            if (targetBehind.IsWall() && t.IsValidTarget(E.Range))
                            {
                                E.CastOnUnit(t);
                                return;
                            }
                        }
                    }
                    /*
                    foreach (var hero in
                        from hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(550f))
                        let prediction = E.GetPrediction(hero)
                        where
                            NavMesh.GetCollisionFlags(
                                prediction.UnitPosition.To2D()
                                    .Extend(
                                        ObjectManager.Player.ServerPosition.To2D(),
                                        -GetValue<Slider>("PushDistance").Value)
                                    .To3D()).HasFlag(CollisionFlags.Wall) ||
                            NavMesh.GetCollisionFlags(
                                prediction.UnitPosition.To2D()
                                    .Extend(
                                        ObjectManager.Player.ServerPosition.To2D(),
                                        -(GetValue<Slider>("PushDistance").Value/2))
                                    .To3D()).HasFlag(CollisionFlags.Wall)
                        select hero)
                    {
                        E.Cast(hero);
                    }
                    */
                }
            }

            if (this.LaneClearActive)
            {
                var useQ = this.GetValue<bool>("UseQL");

                if (Q.IsReady() && useQ)
                {
                    var vMinions = MinionManager.GetMinions(ObjectManager.Player.Position, Q.Range);
                    foreach (var minions in
                        vMinions.Where(
                            minions => minions.Health < ObjectManager.Player.GetSpellDamage(minions, SpellSlot.Q)))
                        Q.Cast(minions);
                }
            }
        }

        public void ExecJungleClear()
        {
            var jungleMobs = Marksman.Utils.Utils.GetMobs(Q.Range + Orbwalking.GetRealAutoAttackRange(null) + 65,
                Marksman.Utils.Utils.MobTypes.All);

            if (jungleMobs != null)
            {
                switch (this.GetValue<StringList>("UseQJ").SelectedIndex)
                {
                    case 1:
                        {
                            if (!jungleMobs.SkinName.ToLower().Contains("baron") || !jungleMobs.SkinName.ToLower().Contains("dragon"))
                            {
                                if (jungleMobs.IsValidTarget(Orbwalking.GetRealAutoAttackRange(null) + 65))
                                    Q.Cast(jungleMobs.IsValidTarget(Orbwalking.GetRealAutoAttackRange(null) + 65)
                                        ? Game.CursorPos
                                        : jungleMobs.Position);
                            }
                            break;
                        }
                    case 2:
                        {
                            if (!jungleMobs.SkinName.ToLower().Contains("baron") || !jungleMobs.SkinName.ToLower().Contains("dragon"))
                            {
                                jungleMobs = Marksman.Utils.Utils.GetMobs(Q.Range + Orbwalking.GetRealAutoAttackRange(null) + 65,
                                    Marksman.Utils.Utils.MobTypes.BigBoys);
                                if (jungleMobs != null)
                                {
                                    Q.Cast(jungleMobs.IsValidTarget(Orbwalking.GetRealAutoAttackRange(null) + 65)
                                        ? Game.CursorPos
                                        : jungleMobs.Position);
                                }
                            }
                            break;
                        }
                }

                switch (this.GetValue<StringList>("UseEJ").SelectedIndex)
                {
                    case 1:
                        {
                            if (jungleMobs.IsValidTarget(E.Range))
                                E.CastOnUnit(jungleMobs);
                            break;
                        }
                    case 2:
                        {
                            jungleMobs = Marksman.Utils.Utils.GetMobs(E.Range, Marksman.Utils.Utils.MobTypes.BigBoys);
                            if (jungleMobs != null)
                            {
                                for (var i = 1; i < 8; i++)
                                {
                                    var targetBehind = jungleMobs.Position
                                                       + Vector3.Normalize(jungleMobs.ServerPosition - ObjectManager.Player.Position) * i
                                                       * 50;

                                    if (targetBehind.IsWall() && jungleMobs.IsValidTarget(E.Range))
                                    {
                                        E.CastOnUnit(jungleMobs);
                                    }
                                }

                                if (ObjectManager.Player.Distance(jungleMobs) < ObjectManager.Player.AttackRange/2)
                                {
                                    E.CastOnUnit(jungleMobs);
                                }

                            }
                            break;
                        }
                }
            }
        }

        public override void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var useQ = this.GetValue<bool>(
                    "UseQ" +
                    (this.ComboActive
                        ? this.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo ? "C" : ""
                        : this.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed ? "H" : ""));
            if (unit.IsMe && useQ)
                Q.Cast(Game.CursorPos);
        }

        public override bool ComboMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseQC" + Id, "Use Q").SetValue(true));
            config.AddItem(new MenuItem("UseEC" + Id, "Use E").SetValue(true));
            config.AddItem(new MenuItem("FocusW" + Id, "Force Focus Marked Enemy").SetValue(true));
            return true;
        }

        public override bool HarassMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseQH" + Id, "Use Q").SetValue(true));
            config.AddItem(new MenuItem("UseEH" + Id, "Use E").SetValue(true));
            return true;
        }

        public override bool MiscMenu(Menu config)
        {
            config.AddItem(
                new MenuItem("UseET" + Id, "Use E (Toggle)").SetValue(new KeyBind("T".ToCharArray()[0],
                    KeyBindType.Toggle)));
            config.AddItem(new MenuItem("UseEInterrupt" + Id, "Use E To Interrupt").SetValue(true));
            config.AddItem(new MenuItem("UseEGapcloser" + Id, "Use E To Gapcloser").SetValue(true));
            config.AddItem(new MenuItem("PushDistance" + Id, "E Push Distance").SetValue(new Slider(425, 475, 300)));
            config.AddItem(new MenuItem("CompleteSilverBuff" + Id, "Complete Silver Buff With Q").SetValue(true));
            return true;
        }

        public override bool LaneClearMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseQL" + Id, "Use Q").SetValue(true));
            return true;
        }

        public override bool DrawingMenu(Menu config)
        {
            config.AddItem(new MenuItem("DrawQ" + this.Id, "Q range").SetValue(new StringList(new[] { "Off", "Q Range", "Q + AA Range" }, 2)));
            config.AddItem(new MenuItem("DrawE" + this.Id, "E range").SetValue(new StringList(new[] { "Off", "E Range", "E Stun Status", "Both" }, 3)));

            return true;
        }

        public override void Drawing_OnDraw(EventArgs args)
        {
            var drawE = GetValue<StringList>("DrawE").SelectedIndex;
            if (E.IsReady() && drawE != 0)
            {
                if (drawE == 1 || drawE == 3)
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, Color.BurlyWood, 1);
                }
                if (drawE == 2 || drawE == 3)
                { 
                    var t = TargetSelector.GetTarget(E.Range + Q.Range, TargetSelector.DamageType.Physical);
                    if (t.IsValidTarget())
                    {
                        var color = System.Drawing.Color.Red;
                        for (var i = 1; i < 8; i++)
                        {
                            var targetBehind = t.Position
                                               + Vector3.Normalize(t.ServerPosition - ObjectManager.Player.Position)*i
                                               *50;


                            if (!targetBehind.IsWall())
                            {
                                color = System.Drawing.Color.Aqua;
                            }
                            else
                            {
                                color = System.Drawing.Color.Red;
                            }

                        }

                        var tt = t.Position + Vector3.Normalize(t.ServerPosition - ObjectManager.Player.Position)*8*50;

                        var startpos = t.Position;
                        var endpos = tt;
                        var endpos1 = tt +
                                      (startpos - endpos).To2D().Normalized().Rotated(45*(float) Math.PI/180).To3D()*
                                      t.BoundingRadius;
                        var endpos2 = tt +
                                      (startpos - endpos).To2D().Normalized().Rotated(-45*(float) Math.PI/180).To3D()*
                                      t.BoundingRadius;

                        var width = 2;

                        var x = new Geometry.Polygon.Line(startpos, endpos);
                        x.Draw(color, width);
                        var y = new Geometry.Polygon.Line(endpos, endpos1);
                        y.Draw(color, width);
                        var z = new Geometry.Polygon.Line(endpos, endpos2);
                        z.Draw(color, width);
                    }
                }
            }

            var drawQ = GetValue<StringList>("DrawQ").SelectedIndex;
            switch (drawQ)
            {
                case 1:
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.Aqua);
                    break;
                case 2:
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range + Orbwalking.GetRealAutoAttackRange(null) + 65, System.Drawing.Color.Aqua);
                    break;
            }
        }
        public override bool JungleClearMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseQJ" + this.Id, "Use Q").SetValue(new StringList(new[] { "Off", "On", "Just big Monsters" }, 2)));
            config.AddItem(new MenuItem("UseEJ" + this.Id, "Use E").SetValue(new StringList(new[] { "Off", "On", "Just big Monsters" }, 2)));
            return true;
        }

        public class VayneData
        {
            public static int GetSilverBuffMarkedCount
            {
                get
                {
                    if (GetSilverBuffMarkedEnemy == null)
                        return 0;

                    return
                        GetSilverBuffMarkedEnemy.Buffs.Where(buff => buff.Name == "vaynesilvereddebuff")
                            .Select(xBuff => xBuff.Count)
                            .FirstOrDefault();
                }
            }

            public static Obj_AI_Hero GetSilverBuffMarkedEnemy
            {
                get
                {
                    return
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(
                                enemy =>
                                    !enemy.IsDead &&
                                    enemy.IsValidTarget(
                                        (Q.IsReady() ? Q.Range : 0) +
                                        Orbwalking.GetRealAutoAttackRange(ObjectManager.Player)))
                            .FirstOrDefault(
                                enemy => enemy.Buffs.Any(buff => buff.Name == "vaynesilvereddebuff" && buff.Count > 0));
                }
            }
        }
    }
}
