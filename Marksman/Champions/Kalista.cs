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
    internal class EnemyMarker
    {
        public string ChampionName { get; set; }
        public double ExpireTime { get; set; }
        public int BuffCount { get; set; }
    }

    internal class Kalista : Champion
    {
        public static Font font;
        public static Spell E;
        public static Dictionary<Vector3, Vector3> JumpPos = new Dictionary<Vector3, Vector3>();
        private static readonly List<EnemyMarker> xEnemyMarker = new List<EnemyMarker>();
        public Obj_AI_Hero CoopStrikeAlly;
        public float CoopStrikeAllyRange = 1250f;
        public Dictionary<String, int> MarkedChampions = new Dictionary<String, int>();
        public Spell Q;
        public Spell R;
        public Spell W;

        public Kalista()
        {
            Utils.Utils.PrintMessage("Kalista loaded.");

            Q = new Spell(SpellSlot.Q, 1170);
            W = new Spell(SpellSlot.W, 5500);
            E = new Spell(SpellSlot.E, 950);
            R = new Spell(SpellSlot.R, 1250);

            Q.SetSkillshot(0.25f, 40f, 1200f, true, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.25f, 80f, 1600f, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(1f, 160f, 2000f, false, SkillshotType.SkillshotLine);

            font = new Font(
                Drawing.Direct3DDevice,
                new FontDescription
                {
                    FaceName = "Segoe UI",
                    Height = 45,
                    OutputPrecision = FontPrecision.Default,
                    Quality = FontQuality.Default
                });
        }

        public int KalistaMarkerCount
        {
            get
            {
                return (from enemy in ObjectManager.Get<Obj_AI_Hero>().Where(tx => tx.IsEnemy && !tx.IsDead)
                    where ObjectManager.Player.Distance(enemy) < E.Range
                    from buff in enemy.Buffs
                    where buff.Name.Contains("kalistaexpungemarker")
                    select buff).Select(buff => buff.Count).FirstOrDefault();
            }
        }

        public override void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
        }

        public override void Drawing_OnDraw(EventArgs args)
        {
            if (ObjectManager.Player.IsDead)
            {
                return;
            }

            var killableMinionCount = 0;
            foreach (
                var m in
                    MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range)
                        .Where(x => E.CanCast(x) && x.Health <= E.GetDamage(x)))
            {
                if (m.SkinName.ToLower() == "sru_chaosminionsiege" || m.SkinName.ToLower() == "sru_chaosminionsuper")
                    killableMinionCount += 2;
                else
                    killableMinionCount++;

                Render.Circle.DrawCircle(m.Position, (float) (m.BoundingRadius*1.5), Color.White, 5);
            }

//            if (killableMinionCount >= 3 && E.IsReady() && ObjectManager.Player.ManaPercent > 15)
            //              E.Cast();

            foreach (
                var m in
                    MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range, MinionTypes.All,
                        MinionTeam.Neutral).Where(m => E.CanCast(m) && m.Health <= E.GetDamage(m) + 20))
            {
                if (m.SkinName.ToLower().Contains("baron") || m.SkinName.ToLower().Contains("dragon") && E.CanCast(m))
                    E.Cast(m);
                else
                    Render.Circle.DrawCircle(m.Position, (float) (m.BoundingRadius*1.5), Color.White, 5);
            }

            Spell[] spellList = {Q, W, E, R};
            foreach (var spell in spellList)
            {
                var menuItem = GetValue<Circle>("Draw" + spell.Slot);
                if (menuItem.Active && spell.Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, spell.Range, menuItem.Color);
            }
            var drawEStackCount = GetValue<Circle>("DrawEStackCount");
            if (drawEStackCount.Active)
            {
                xEnemyMarker.Clear();
                foreach (
                    var xEnemy in
                        HeroManager.Enemies.Where(
                            tx => tx.IsEnemy && !tx.IsDead && ObjectManager.Player.Distance(tx) < E.Range))
                {
                    foreach (var buff in xEnemy.Buffs.Where(buff => buff.Name.Contains("kalistaexpungemarker")))
                    {
                        xEnemyMarker.Add(new EnemyMarker
                        {
                            ChampionName = xEnemy.ChampionName,
                            ExpireTime = Game.Time + 4,
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
                            Utils.Utils.DrawText(font, display, (int) enemy.HPBarPosition.X - 10,
                                (int) enemy.HPBarPosition.Y, SharpDX.Color.Wheat);
                            //Drawing.DrawText(enemy.HPBarPosition.X + 145, enemy.HPBarPosition.Y + 20, drawEStackCount.Color, display);
                        }
                    }
                }
            }
            var drawJumpPos = GetValue<Circle>("DrawJumpPos");
            if (drawJumpPos.Active)
            {
                foreach (var pos in JumpPos)
                {
                    if (ObjectManager.Player.Distance(pos.Key) <= 500f ||
                        ObjectManager.Player.Distance(pos.Value) <= 500f)

                    {
                        Drawing.DrawCircle(pos.Key, 75f, drawJumpPos.Color);
                        Drawing.DrawCircle(pos.Value, 75f, drawJumpPos.Color);
                    }
                    if (ObjectManager.Player.Distance(pos.Key) <= 35f ||
                        ObjectManager.Player.Distance(pos.Value) <= 35f)
                    {
                        Render.Circle.DrawCircle(pos.Key, 70f, Color.GreenYellow);
                        Render.Circle.DrawCircle(pos.Value, 70f, Color.GreenYellow);
                    }
                }
            }
        }

        public void JumpTo()
        {
            if (!Q.IsReady())
            {
                Drawing.DrawText(Drawing.Width*0.44f, Drawing.Height*0.80f, Color.Red,
                    "Q is not ready! You can not Jump!");
                return;
            }

            Drawing.DrawText(Drawing.Width*0.39f, Drawing.Height*0.80f, Color.White,
                "Jumping Mode is Active! Go to the nearest jump point!");

            foreach (var xTo in from pos in JumpPos
                where ObjectManager.Player.Distance(pos.Key) <= 35f ||
                      ObjectManager.Player.Distance(pos.Value) <= 35f
                let xTo = pos.Value
                select ObjectManager.Player.Distance(pos.Key) < ObjectManager.Player.Distance(pos.Value)
                    ? pos.Value
                    : pos.Key)
            {
                Q.Cast(new Vector2(xTo.X, xTo.Y), true);
                //Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(xTo.X, xTo.Y)).Send();
                ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, xTo);
            }
        }

        private static float GetEDamage(Obj_AI_Base t)
        {
            return E.IsReady() ? E.GetDamage(t) : 0;
        }

        public override void Game_OnGameUpdate(EventArgs args)
        {
            var t1 =
                HeroManager.Enemies.FirstOrDefault(
                    x =>
                        !x.HasBuffOfType(BuffType.Invulnerability) && !x.HasBuffOfType(BuffType.SpellShield) &&
                        E.CanCast(x) && (x.Health + (x.HPRegenRate/2) + x.Level) <= E.GetDamage(x));

            if (E.CanCast(t1))
            {
                E.Cast();
            }

            if (this.LaneClearActive && E.IsReady())
            {
                var minECount = GetValue<Slider>("UseEL").Value;
                var killableMinionCount = 0;
                foreach (
                    var m in
                        MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range)
                            .Where(x => E.CanCast(x) && x.Health <= E.GetDamage(x)))
                {
                    if (m.SkinName.ToLower() == "sru_chaosminionsiege" || m.SkinName.ToLower() == "sru_chaosminionsuper")
                        killableMinionCount += 2;
                    else
                        killableMinionCount++;
                }

                if (killableMinionCount >= minECount && E.IsReady() && ObjectManager.Player.ManaPercent > E.ManaCost*2)
                {
                    E.Cast();
                }
            }

            if (this.JungleClearActive)
            {
                if (Q.IsReady())
                {
                    //var jungleMobs = GetValue<StringList>("UseEJ").SelectedIndex == 1
                    //    ? Utils.Utils.GetMobs(Q.Range)
                    //    : Utils.Utils.GetMobs(Q.Range, Utils.Utils.MobTypes.BigBoys);

                    var jungleMobs = Utils.Utils.GetMobs(Q.Range,
                        GetValue<StringList>("UseEJ").SelectedIndex == 1
                            ? Utils.Utils.MobTypes.All
                            : Utils.Utils.MobTypes.BigBoys);

                    if (jungleMobs != null && ObjectManager.Player.Mana > E.ManaCost + Q.ManaCost)
                        Q.Cast(jungleMobs);
                }

                if (E.IsReady())
                {
                    var jungleMobs = Utils.Utils.GetMobs(E.Range,
                        GetValue<StringList>("UseEJ").SelectedIndex == 1
                            ? Utils.Utils.MobTypes.All
                            : Utils.Utils.MobTypes.BigBoys);

                    if (jungleMobs != null && E.CanCast(jungleMobs) && jungleMobs.Health <= E.GetDamage(jungleMobs) + 20)
                        E.CastOnUnit(jungleMobs);
                }
            }

            if (GetValue<Circle>("DrawJumpPos").Active)
                fillPositions();

            if (GetValue<KeyBind>("JumpTo").Active)
            {
                JumpTo();
            }

            foreach (
                var myBoddy in
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(
                            obj => obj.Name == "RobotBuddy" &&
                                   obj.IsAlly && ObjectManager.Player.Distance(obj) < 1500))

            {
                Render.Circle.DrawCircle(myBoddy.Position, 75f, Color.Red);
            }


            Obj_AI_Hero t;

            if (Q.IsReady() && GetValue<KeyBind>("UseQTH").Active)
            {
                if (ObjectManager.Player.HasBuff("Recall"))
                {
                    return;
                }

                t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
                if (t.IsValidTarget(Q.Range) && ObjectManager.Player.Mana > E.ManaCost + Q.ManaCost)
                {
                    Q.Cast(t);
                }
            }

            if (ComboActive || HarassActive)
            {
                var useQ = GetValue<bool>("UseQ" + (ComboActive ? "C" : "H"));

                if (Orbwalking.CanMove(100))
                {
                    if (Q.IsReady() && useQ)
                    {
                        t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
                        if (t.IsValidTarget(Q.Range) && ObjectManager.Player.Mana > E.ManaCost + Q.ManaCost)
                        {
                            Q.Cast(t);
                        }
                    }
                }
            }
        }

        public override bool ComboMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseQC" + Id, "Use Q").SetValue(true));
            config.AddItem(new MenuItem("UseWC" + Id, "Use W").SetValue(true));
            config.AddItem(new MenuItem("UseEC" + Id, "Use E").SetValue(true));
            config.AddItem(new MenuItem("UseRC" + Id, "Use R").SetValue(true));
            return true;
        }

        public override bool HarassMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseQH" + Id, "Use Q").SetValue(true));
            config.AddItem(
                new MenuItem("UseQTH" + Id, "Use Q (Toggle)").SetValue(new KeyBind("H".ToCharArray()[0],
                    KeyBindType.Toggle)));
            return true;
        }

        public override bool MiscMenu(Menu config)
        {
            config.AddItem(
                new MenuItem("JumpTo" + Id, "JumpTo").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
            config.AddItem(new MenuItem("JumpTo2" + Id, "JumpTo2").SetValue(true));
            return true;
        }

        public override bool DrawingMenu(Menu config)
        {
            config.AddItem(
                new MenuItem("DrawQ" + Id, "Q range").SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
            config.AddItem(
                new MenuItem("DrawW" + Id, "W range").SetValue(new Circle(false, Color.FromArgb(100, 255, 255, 255))));
            config.AddItem(
                new MenuItem("DrawE" + Id, "E range").SetValue(new Circle(false, Color.FromArgb(100, 255, 255, 255))));
            config.AddItem(
                new MenuItem("DrawR" + Id, "R range").SetValue(new Circle(false, Color.FromArgb(100, 255, 255, 255))));
            config.AddItem(
                new MenuItem("DrawEStackCount" + Id, "E Stack Count").SetValue(new Circle(true, Color.White)));
            config.AddItem(new MenuItem("DrawJumpPos" + Id, "Jump Positions").SetValue(new Circle(false, Color.HotPink)));

            var damageAfterE = new MenuItem("DamageAfterE", "Damage After E").SetValue(true);
            config.AddItem(damageAfterE);

            Utility.HpBarDamageIndicator.DamageToUnit = GetEDamage;
            Utility.HpBarDamageIndicator.Enabled = damageAfterE.GetValue<bool>();
            damageAfterE.ValueChanged +=
                delegate(object sender, OnValueChangeEventArgs eventArgs)
                {
                    Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
                };

            return true;
        }

        public override bool LaneClearMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseEL" + this.Id, "Use E").SetValue(new Slider(2, 1, 3)));
            return true;
        }


        public override bool JungleClearMenu(Menu config)
        {
            config.AddItem(
                new MenuItem("UseQJ" + this.Id, "Use Q").SetValue(
                    new StringList(new[] {"Off", "On", "Just big Monsters"}, 1)));
            config.AddItem(
                new MenuItem("UseEJ" + this.Id, "Use E").SetValue(
                    new StringList(new[] {"Off", "On", "Just big Monsters"}, 1)));
            return true;
        }

        public static void fillPositions()
        {
        }
    }
}
