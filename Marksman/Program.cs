#region

using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Marksman.Champions;
using Marksman.Utils;
using SharpDX;
using SharpDX.Direct3D9;
using Activator = Marksman.Utils.Activator;
using Color = System.Drawing.Color;

#endregion

namespace Marksman
{
    using System.Collections.Generic;
    
    internal class Program
    {
        public static Menu Config;
        public static Menu OrbWalking;
        public static Menu QuickSilverMenu;
        public static Menu MenuActivator;

//        public static Menu MenuInterruptableSpell;
        public static Champion CClass;
        public static Activator AActivator;
        public static Utils.AutoLevel AutoLevel;
        public static Utils.EarlyEvade EarlyEvade;

        public static double ActivatorTime;
        private static Obj_AI_Hero xSelectedTarget;
        private static float AsmLoadingTime = 0;

        public static SpellSlot SmiteSlot = SpellSlot.Unknown;

        public static Spell Smite;

        private static readonly int[] SmitePurple = {3713, 3726, 3725, 3726, 3723};
        private static readonly int[] SmiteGrey = {3711, 3722, 3721, 3720, 3719};
        private static readonly int[] SmiteRed = {3715, 3718, 3717, 3716, 3714};
        private static readonly int[] SmiteBlue = {3706, 3710, 3709, 3708, 3707};

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Config = new Menu("Marksman", "Marksman", true).SetFontStyle(FontStyle.Regular, SharpDX.Color.GreenYellow);
            CClass = new Champion();
            AActivator = new Activator();


            var BaseType = CClass.GetType();

            /* Update this with Activator.CreateInstance or Invoke
               http://stackoverflow.com/questions/801070/dynamically-invoking-any-function-by-passing-function-name-as-string 
               For now stays cancer.
             */
            var championName = ObjectManager.Player.ChampionName.ToLowerInvariant();

            switch (championName)
            {
                case "ashe":
                    CClass = new Ashe();
                    break;
                case "caitlyn":
                    CClass = new Caitlyn();
                    break;
                case "corki":
                    CClass = new Corki();
                    break;
                case "draven":
                    CClass = new Draven();
                    break;
                case "ezreal":
                    CClass = new Ezreal();
                    break;
                case "graves":
                    CClass = new Graves();
                    break;
                case "gnar":
                    CClass = new Gnar();
                    break;
                case "jinx":
                    CClass = new Jinx();
                    break;
                case "kalista":
                    CClass = new Kalista();
                    break;
                case "kindred":
                    CClass = new Kindred();
                    break;
                case "kogmaw":
                    CClass = new Kogmaw();
                    break;
                case "lucian":
                    CClass = new Lucian();
                    break;
                case "missfortune":
                    CClass = new MissFortune();
                    break;
                case "quinn":
                    CClass = new Quinn();
                    break;
                case "sivir":
                    CClass = new Sivir();
                    break;
                case "teemo":
                    CClass = new Teemo();
                    break;
                case "tristana":
                    CClass = new Tristana();
                    break;
                case "twitch":
                    CClass = new Twitch();
                    break;
                case "urgot":
                    CClass = new Urgot();
                    break;
                case "vayne":
                    CClass = new Vayne();
                    break;
                case "varus":
                    CClass = new Varus();
                    break;
            }
            Config.DisplayName = "Marksman | " + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(championName);

            CClass.Id = ObjectManager.Player.CharData.BaseSkinName;
            CClass.Config = Config;

            OrbWalking = Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            CClass.Orbwalker = new Orbwalking.Orbwalker(OrbWalking);

            OrbWalking.AddItem(new MenuItem("Orb.AutoWindUp", "Marksman - Auto Windup").SetValue(false)).ValueChanged +=
                (sender, argsEvent) => { if (argsEvent.GetNewValue<bool>()) CheckAutoWindUp(); };

            MenuActivator = new Menu("Activator", "Activator").SetFontStyle(FontStyle.Regular, SharpDX.Color.Aqua);
            {
                AutoLevel = new Utils.AutoLevel();

                EarlyEvade = new Utils.EarlyEvade();
                MenuActivator.AddSubMenu(EarlyEvade.MenuLocal);

                /* Menu Items */
                var items = MenuActivator.AddSubMenu(new Menu("Items", "Items"));
                items.AddItem(new MenuItem("BOTRK", "BOTRK").SetValue(true));
                items.AddItem(new MenuItem("GHOSTBLADE", "Ghostblade").SetValue(true));
                items.AddItem(new MenuItem("SWORD", "Sword of the Divine").SetValue(true));
                items.AddItem(new MenuItem("MURAMANA", "Muramana").SetValue(true));
                QuickSilverMenu = new Menu("QSS", "QuickSilverSash");
                items.AddSubMenu(QuickSilverMenu);
                QuickSilverMenu.AddItem(new MenuItem("AnyStun", "Any Stun").SetValue(true));
                QuickSilverMenu.AddItem(new MenuItem("AnySlow", "Any Slow").SetValue(true));
                QuickSilverMenu.AddItem(new MenuItem("AnySnare", "Any Snare").SetValue(true));
                QuickSilverMenu.AddItem(new MenuItem("AnyTaunt", "Any Taunt").SetValue(true));
                foreach (var t in AActivator.BuffList)
                {
                    foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy))
                    {
                        if (t.ChampionName == enemy.ChampionName)
                            QuickSilverMenu.AddItem(new MenuItem(t.BuffName, t.DisplayName).SetValue(t.DefaultValue));
                    }
                }
                items.AddItem(
                    new MenuItem("UseItemsMode", "Use items on").SetValue(
                        new StringList(new[] {"No", "Mixed mode", "Combo mode", "Both"}, 2)));

                new PotionManager(MenuActivator);

                /* Menu Summoners */
                var summoners = MenuActivator.AddSubMenu(new Menu("Summoners", "Summoners"));
                {
                    var summonersHeal = summoners.AddSubMenu(new Menu("Heal", "Heal"));
                    {
                        summonersHeal.AddItem(new MenuItem("SUMHEALENABLE", "Enable").SetValue(true));
                        summonersHeal.AddItem(
                            new MenuItem("SUMHEALSLIDER", "Min. Heal Per.").SetValue(new Slider(20, 99, 1)));
                    }

                    var summonersBarrier = summoners.AddSubMenu(new Menu("Barrier", "Barrier"));
                    {
                        summonersBarrier.AddItem(new MenuItem("SUMBARRIERENABLE", "Enable").SetValue(true));
                        summonersBarrier.AddItem(
                            new MenuItem("SUMBARRIERSLIDER", "Min. Heal Per.").SetValue(new Slider(20, 99, 1)));
                    }

                    var summonersIgnite = summoners.AddSubMenu(new Menu("Ignite", "Ignite"));
                    {
                        summonersIgnite.AddItem(new MenuItem("SUMIGNITEENABLE", "Enable").SetValue(true));
                    }
                }
            }
            Config.AddSubMenu(MenuActivator);
            //var Extras = Config.AddSubMenu(new Menu("Extras", "Extras"));
            //new PotionManager(Extras);

            // If Champion is supported draw the extra menus
            if (BaseType != CClass.GetType())
            {
                SetSmiteSlot();

                var combo = new Menu("Combo", "Combo").SetFontStyle(FontStyle.Regular, SharpDX.Color.GreenYellow);
                if (CClass.ComboMenu(combo))
                {
                    if (SmiteSlot != SpellSlot.Unknown)
                        combo.AddItem(new MenuItem("ComboSmite", "Use Smite").SetValue(true));

                    Config.AddSubMenu(combo);
                }

                var harass = new Menu("Harass", "Harass");
                if (CClass.HarassMenu(harass))
                {
                    harass.AddItem(new MenuItem("HarassMana", "Min. Mana Percent").SetValue(new Slider(50, 100, 0)));
                    Config.AddSubMenu(harass);
                }

                var laneclear = new Menu("Lane Mode", "LaneClear");
                if (CClass.LaneClearMenu(laneclear))
                {
                    laneclear.AddItem(
                        new MenuItem("LaneClearMana", "Min. Mana Percent").SetValue(new Slider(50, 100, 0)));
                    Config.AddSubMenu(laneclear);
                }

                var jungleClear = new Menu("Jungle Mode", "JungleClear");
                if (CClass.JungleClearMenu(jungleClear))
                {
                    jungleClear.AddItem(new MenuItem("Jungle.Mana", "Min. Mana %:").SetValue(new Slider(50, 100, 0)));
                    Config.AddSubMenu(jungleClear);
                }

                var misc = new Menu("Misc", "Misc").SetFontStyle(FontStyle.Regular, SharpDX.Color.DarkOrange);
                if (CClass.MiscMenu(misc))
                {
                    Config.AddSubMenu(misc);
                }
                /*
                                var extras = new Menu("Extras", "Extras");
                                if (CClass.ExtrasMenu(extras))
                                {
                                    Config.AddSubMenu(extras);
                                }
                 */

                var marksmanDrawings = new Menu("Drawings", "MDrawings");
                Config.AddSubMenu(marksmanDrawings);

                var drawing = new Menu(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(championName), "Drawings").SetFontStyle(FontStyle.Regular, SharpDX.Color.Aquamarine);
                if (CClass.DrawingMenu(drawing))
                {
                    marksmanDrawings.AddSubMenu(drawing);
                }

                var GlobalDrawings = new Menu("Global", "GDrawings");
                {
                    string[] strQ = new string[6];
                    strQ[0] = "Off";
                    var i = 1;
                    foreach (var e in HeroManager.Enemies)
                    {
                        strQ[i] = e.ChampionName;
                        i += 1;
                    }
                    GlobalDrawings.AddItem(new MenuItem("Marksman.Compare", "Compare me with").SetValue(new StringList(strQ, 0)));
                    GlobalDrawings.AddItem(new MenuItem("Draw.KillableEnemy", "Killable Enemy Text").SetValue(true));
                    GlobalDrawings.AddItem(new MenuItem("drawMinionLastHit", "Minion Last Hit").SetValue(new Circle(true, Color.GreenYellow)));
                    GlobalDrawings.AddItem(new MenuItem("drawMinionNearKill", "Minion Near Kill").SetValue(new Circle(true, Color.Gray)));
                    GlobalDrawings.AddItem(new MenuItem("drawJunglePosition", "Jungle Farm Position").SetValue(true));
                    GlobalDrawings.AddItem(new MenuItem("Draw.DrawMinion", "Draw Minions Sprite").SetValue(false));
                    GlobalDrawings.AddItem(new MenuItem("Draw.DrawTarget", "Draw Target Sprite").SetValue(true));
                    marksmanDrawings.AddSubMenu(GlobalDrawings);
                }
            }
            LoadDefaultCompareChampion();

            CClass.MainMenu(Config);

            if (championName == "sivir")
            {
                Evade.Evade.Initiliaze();
                Evade.Config.Menu.DisplayName = "E";
                Config.AddSubMenu(Evade.Config.Menu);
            }

            //Evade.Evade.Initiliaze();
            //Config.AddSubMenu(Evade.Config.Menu);

            Config.AddToMainMenu();
            Sprite.Load();
            CheckAutoWindUp();

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
            
            AsmLoadingTime = Game.Time;
            //Game.OnWndProc += Game_OnWndProc;
        }


        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (args.Msg != 0x201)
                return;

            foreach (var objAiHero in (from hero in ObjectManager.Get<Obj_AI_Hero>()
                where hero.IsValidTarget()
                select hero
                into h
                orderby h.Distance(Game.CursorPos) descending
                select h
                into enemy
                where enemy.Distance(Game.CursorPos) < 150f
                select enemy).Where(objAiHero => objAiHero != null && objAiHero != xSelectedTarget))
            {
                xSelectedTarget = objAiHero;
                TargetSelector.SetTarget(objAiHero);
            }
        }

        private static void CheckAutoWindUp()
        {
            var additional = 0;

            if (Game.Ping >= 100)
            {
                additional = Game.Ping/100*10;
            }
            else if (Game.Ping > 40 && Game.Ping < 100)
            {
                additional = Game.Ping/100*20;
            }
            else if (Game.Ping <= 40)
            {
                additional = +20;
            }
            var windUp = Game.Ping + additional;
            if (windUp < 40)
            {
                windUp = 40;
            }
            OrbWalking.Item("ExtraWindup").SetValue(windUp < 200 ? new Slider(windUp, 200, 0) : new Slider(200, 200, 0));
        }

        private static void LoadDefaultCompareChampion()
        {
            var enemyChampions = new[]
                                     {
                                         "Ashe", "Caitlyn", "Corki", "Draven", "Ezreal", "Graves", "Jinx", "Kalista",
                                         "Kindred", "KogMaw", "Lucian", "MissFortune", "Quinn", "Sivir", "Tristana",
                                         "Twitch", "Urgot", "Varus", "Vayne"
                                     };

            List<Obj_AI_Hero> mobs = HeroManager.Enemies;

            Obj_AI_Hero compChampion =
                (from fMobs in mobs from fBigBoys in enemyChampions where fBigBoys == fMobs.ChampionName select fMobs)
                    .FirstOrDefault();

            if (compChampion != null)
            {
                var selectedIndex = 0;
                string[] strQ = new string[6];
                strQ[0] = "Off";
                var i = 1;
                foreach (var e in HeroManager.Enemies)
                {
                    strQ[i] = e.ChampionName;
                    if (e.ChampionName == compChampion.ChampionName)
                    {
                        selectedIndex = i;
                    }
                    i += 1;
                }
                Config.Item("Marksman.Compare").SetValue(new StringList(strQ, selectedIndex));
            }
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            var myChampionKilled = ObjectManager.Player.ChampionsKilled;
            var myAssists = ObjectManager.Player.Assists;
            var myDeaths = ObjectManager.Player.Deaths;
            var myMinionsKilled = ObjectManager.Player.MinionsKilled;

            if (Config.Item("Marksman.Compare").GetValue<StringList>().SelectedIndex != 0)
            {
                Obj_AI_Hero compChampion = null;
                foreach (Obj_AI_Hero e in HeroManager.Enemies.Where(e => e.ChampionName == Config.Item("Marksman.Compare").GetValue<StringList>().SelectedValue))
                {
                    compChampion = e;
                }

                var compChampionKilled = compChampion.ChampionsKilled;
                var compAssists = compChampion.Assists;
                var compDeaths = compChampion.Deaths;
                var compMinionsKilled = compChampion.MinionsKilled;
                var xText = "You: " + myChampionKilled + " / " + myDeaths + " / " + myAssists + " | " + myMinionsKilled +
                            "      vs      " + 
                            compChampion.ChampionName + " : " +  compChampionKilled + " / " + compDeaths + " | " + compAssists + " | " + compMinionsKilled;                
                
                DrawBox(new Vector2(Drawing.Width*0.400f, Drawing.Height*0.132f), 350, 26, Color.FromArgb(100, 255, 200, 37), 1, Color.Black);
                Utils.Utils.DrawText(Utils.Utils.Text, xText, Drawing.Width * 0.422f, Drawing.Height * 0.140f, SharpDX.Color.Wheat);
                
                if (Game.Time - AsmLoadingTime < 10)
                {
                    var timer = string.Format("0:{0:D2}", (int)10 - (int)(Game.Time - AsmLoadingTime));
                    Utils.Utils.DrawText(Utils.Utils.Text,"You can turn on/off this text. Use Marksman -> Global Drawings -> Compare with me",Drawing.Width * 0.350f,Drawing.Height * 0.165f,SharpDX.Color.Wheat);
                    Utils.Utils.DrawText(Utils.Utils.Text, "This message will self destruct in 10 secs " + timer + "  - Mission L# Kappa - ", Drawing.Width * 0.400f, Drawing.Height * 0.185f, SharpDX.Color.Aqua);
                }                
            }

            if (Config.Item("Draw.KillableEnemy").GetValue<bool>())
            {
                var t = KillableEnemyAA;
                if (t.Item1 != null && t.Item1.IsValidTarget(Orbwalking.GetRealAutoAttackRange(null) + 1400) &&
                    t.Item2 > 0)
                {
                    Utils.Utils.DrawText(Utils.Utils.Text,string.Format("{0}: {1} x AA Damage = Kill", t.Item1.ChampionName, t.Item2),(int) t.Item1.HPBarPosition.X + 145,(int) t.Item1.HPBarPosition.Y + 5,SharpDX.Color.White);
                }
            }

/*            var toD = CClass.Config.Item("Draw.ToD").GetValue<bool>();
            if (toD)
            {
                var enemyCount =
                    CClass.Config.Item("Draw.ToDMinEnemy").GetValue<Slider>().Value;
                var controlRange =
                    CClass.Config.Item("Draw.ToDControlRange").GetValue<Slider>().Value;

                var xEnemies = HeroManager.Enemies.Count(enemies => enemies.IsValidTarget(controlRange));
                if (xEnemies >= enemyCount)
                    return;

                var toDRangeColor =
                    CClass.Config.Item("Draw.ToDControlRangeColor").GetValue<Circle>();
                if (toDRangeColor.Active)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, controlRange, toDRangeColor.Color);

            }
            */
            /*
            var t = TargetSelector.SelectedTarget;
            if (!t.IsValidTarget())
            {
                t = TargetSelector.GetTarget(1100, TargetSelector.DamageType.Physical);
                TargetSelector.SetTarget(t);
            }

            if (t.IsValidTarget() && ObjectManager.Player.Distance(t) < 1110)
            {
                Render.Circle.DrawCircle(t.Position, 150, Color.Yellow);
            }
            */
            var drawJunglePosition = CClass.Config.Item("drawJunglePosition").GetValue<bool>();
            {
                if (drawJunglePosition)
                    Utils.Utils.Jungle.DrawJunglePosition();
            }

            var drawMinionLastHit = CClass.Config.Item("drawMinionLastHit").GetValue<Circle>();
            var drawMinionNearKill = CClass.Config.Item("drawMinionNearKill").GetValue<Circle>();
            if (drawMinionLastHit.Active || drawMinionNearKill.Active)
            {
                var xMinions =
                    MinionManager.GetMinions(ObjectManager.Player.Position,
                        ObjectManager.Player.AttackRange + ObjectManager.Player.BoundingRadius + 300, MinionTypes.All,
                        MinionTeam.Enemy, MinionOrderTypes.MaxHealth);

                foreach (var xMinion in xMinions)
                {
                    if (drawMinionLastHit.Active && ObjectManager.Player.GetAutoAttackDamage(xMinion, true) >=
                        xMinion.Health)
                    {
                        Render.Circle.DrawCircle(xMinion.Position, xMinion.BoundingRadius, drawMinionLastHit.Color);
                    }
                    else if (drawMinionNearKill.Active &&
                             ObjectManager.Player.GetAutoAttackDamage(xMinion, true)*2 >= xMinion.Health)
                    {
                        Render.Circle.DrawCircle(xMinion.Position, xMinion.BoundingRadius, drawMinionNearKill.Color);
                    }
                }
            }

            if (CClass != null)
            {
                CClass.Drawing_OnDraw(args);
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Items.HasItem(3139) || Items.HasItem(3140))
                CheckChampionBuff();

            //Update the combo and harass values.
            CClass.ComboActive = CClass.Config.Item("Orbwalk").GetValue<KeyBind>().Active;

            var vHarassManaPer = Config.Item("HarassMana").GetValue<Slider>().Value;
            CClass.HarassActive = CClass.Config.Item("Farm").GetValue<KeyBind>().Active &&
                                  ObjectManager.Player.ManaPercent >= vHarassManaPer;

            CClass.ToggleActive = ObjectManager.Player.ManaPercent >= vHarassManaPer;

            var vLaneClearManaPer = Config.Item("LaneClearMana").GetValue<Slider>().Value;
            CClass.LaneClearActive = CClass.Config.Item("LaneClear").GetValue<KeyBind>().Active &&
                                     ObjectManager.Player.ManaPercent >= vLaneClearManaPer;

            CClass.JungleClearActive = CClass.Config.Item("LaneClear").GetValue<KeyBind>().Active &&
                                       ObjectManager.Player.ManaPercent >=
                                       Config.Item("Jungle.Mana").GetValue<Slider>().Value;

            CClass.Game_OnGameUpdate(args);

            UseSummoners();
            var useItemModes = Config.Item("UseItemsMode").GetValue<StringList>().SelectedIndex;

            //Items
            if (
                !((CClass.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo &&
                   (useItemModes == 2 || useItemModes == 3))
                  ||
                  (CClass.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed &&
                   (useItemModes == 1 || useItemModes == 3))))
                return;

            var botrk = Config.Item("BOTRK").GetValue<bool>();
            var ghostblade = Config.Item("GHOSTBLADE").GetValue<bool>();
            var sword = Config.Item("SWORD").GetValue<bool>();
            var muramana = Config.Item("MURAMANA").GetValue<bool>();
            var target = CClass.Orbwalker.GetTarget() as Obj_AI_Base;

            var smiteReady = (SmiteSlot != SpellSlot.Unknown &&
                              ObjectManager.Player.Spellbook.CanUseSpell(SmiteSlot) == SpellState.Ready);

            if (smiteReady && CClass.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                Smiteontarget(target as Obj_AI_Hero);

            if (botrk)
            {
                if (target != null && target.Type == ObjectManager.Player.Type &&
                    target.ServerPosition.Distance(ObjectManager.Player.ServerPosition) < 550)
                {
                    var hasCutGlass = Items.HasItem(3144);
                    var hasBotrk = Items.HasItem(3153);

                    if (hasBotrk || hasCutGlass)
                    {
                        var itemId = hasCutGlass ? 3144 : 3153;
                        var damage = ObjectManager.Player.GetItemDamage(target, Damage.DamageItems.Botrk);
                        if (hasCutGlass || ObjectManager.Player.Health + damage < ObjectManager.Player.MaxHealth)
                            Items.UseItem(itemId, target);
                    }
                }
            }

            if (ghostblade && target != null && target.Type == ObjectManager.Player.Type &&
                !ObjectManager.Player.HasBuff("ItemSoTD", true) /*if Sword of the divine is not active */
                && Orbwalking.InAutoAttackRange(target))
                Items.UseItem(3142);

            if (sword && target != null && target.Type == ObjectManager.Player.Type &&
                !ObjectManager.Player.HasBuff("spectralfury", true) /*if ghostblade is not active*/
                && Orbwalking.InAutoAttackRange(target))
                Items.UseItem(3131);

            if (muramana && Items.HasItem(3042))
            {
                if (target != null && CClass.ComboActive &&
                    target.Position.Distance(ObjectManager.Player.Position) < 1200)
                {
                    if (!ObjectManager.Player.HasBuff("Muramana", true))
                    {
                        Items.UseItem(3042);
                    }
                }
                else
                {
                    if (ObjectManager.Player.HasBuff("Muramana", true))
                    {
                        Items.UseItem(3042);
                    }
                }
            }
        }

        public static void UseSummoners()
        {
            if (ObjectManager.Player.IsDead)
                return;

            const int xDangerousRange = 1100;

            if (Config.Item("SUMHEALENABLE").GetValue<bool>())
            {
                var xSlot = ObjectManager.Player.GetSpellSlot("summonerheal");
                var xCanUse = ObjectManager.Player.Health <=
                              ObjectManager.Player.MaxHealth/100*Config.Item("SUMHEALSLIDER").GetValue<Slider>().Value;

                if (xCanUse && !ObjectManager.Player.InShop() &&
                    (xSlot != SpellSlot.Unknown || ObjectManager.Player.Spellbook.CanUseSpell(xSlot) == SpellState.Ready)
                    && ObjectManager.Player.CountEnemiesInRange(xDangerousRange) > 0)
                {
                    ObjectManager.Player.Spellbook.CastSpell(xSlot);
                }
            }

            if (Config.Item("SUMBARRIERENABLE").GetValue<bool>())
            {
                var xSlot = ObjectManager.Player.GetSpellSlot("summonerbarrier");
                var xCanUse = ObjectManager.Player.Health <=
                              ObjectManager.Player.MaxHealth/100*
                              Config.Item("SUMBARRIERSLIDER").GetValue<Slider>().Value;

                if (xCanUse && !ObjectManager.Player.InShop() &&
                    (xSlot != SpellSlot.Unknown || ObjectManager.Player.Spellbook.CanUseSpell(xSlot) == SpellState.Ready)
                    && ObjectManager.Player.CountEnemiesInRange(xDangerousRange) > 0)
                {
                    ObjectManager.Player.Spellbook.CastSpell(xSlot);
                }
            }

            if (Config.Item("SUMIGNITEENABLE").GetValue<bool>())
            {
                var xSlot = ObjectManager.Player.GetSpellSlot("summonerdot");
                var t = CClass.Orbwalker.GetTarget() as Obj_AI_Hero;

                if (t != null && xSlot != SpellSlot.Unknown &&
                    ObjectManager.Player.Spellbook.CanUseSpell(xSlot) == SpellState.Ready)
                {
                    if (ObjectManager.Player.Distance(t) < 650 &&
                        ObjectManager.Player.GetSummonerSpellDamage(t, Damage.SummonerSpell.Ignite) >=
                        t.Health)
                    {
                        ObjectManager.Player.Spellbook.CastSpell(xSlot, t);
                    }
                }
            }
        }

        private static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            CClass.Orbwalking_AfterAttack(unit, target);
        }

        private static void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            CClass.Orbwalking_BeforeAttack(args);
        }

        private static void CheckChampionBuff()
        {
            var canUse3139 = Items.HasItem(3139) && Items.CanUseItem(3139);
            var canUse3140 = Items.HasItem(3140) && Items.CanUseItem(3140);

            foreach (var t1 in ObjectManager.Player.Buffs)
            {
                foreach (var t in QuickSilverMenu.Items)
                {
                    if (QuickSilverMenu.Item(t.Name).GetValue<bool>())
                    {
                        if (t1.Name.ToLower().Contains(t.Name.ToLower()))
                        {
                            var t2 = t1;
                            foreach (var bx in AActivator.BuffList.Where(bx => bx.BuffName == t2.Name))
                            {
                                if (bx.Delay > 0)
                                {
                                    if (ActivatorTime + bx.Delay < Game.Time)
                                        ActivatorTime = Game.Time;

                                    if (ActivatorTime + bx.Delay <= Game.Time)
                                    {
                                        if (canUse3139)
                                            Items.UseItem(3139);
                                        else if (canUse3140)
                                            Items.UseItem(3140);
                                        ActivatorTime = Game.Time;
                                    }
                                }
                                else
                                {
                                    if (canUse3139)
                                        Items.UseItem(3139);
                                    else if (canUse3140)
                                        Items.UseItem(3140);
                                }
                            }
                        }
                    }

                    if (QuickSilverMenu.Item("AnySlow").GetValue<bool>() &&
                        ObjectManager.Player.HasBuffOfType(BuffType.Slow))
                    {
                        if (canUse3139)
                            Items.UseItem(3139);
                        else if (canUse3140)
                            Items.UseItem(3140);
                    }
                    if (QuickSilverMenu.Item("AnySnare").GetValue<bool>() &&
                        ObjectManager.Player.HasBuffOfType(BuffType.Snare))
                    {
                        if (canUse3139)
                            Items.UseItem(3139);
                        else if (canUse3140)
                            Items.UseItem(3140);
                    }
                    if (QuickSilverMenu.Item("AnyStun").GetValue<bool>() &&
                        ObjectManager.Player.HasBuffOfType(BuffType.Stun))
                    {
                        if (canUse3139)
                            Items.UseItem(3139);
                        else if (canUse3140)
                            Items.UseItem(3140);
                    }
                    if (QuickSilverMenu.Item("AnyTaunt").GetValue<bool>() &&
                        ObjectManager.Player.HasBuffOfType(BuffType.Taunt))
                    {
                        if (canUse3139)
                            Items.UseItem(3139);
                        else if (canUse3140)
                            Items.UseItem(3140);
                    }
                }
            }
        }

        private static string Smitetype
        {
            get
            {
                if (SmiteBlue.Any(i => Items.HasItem(i)))
                    return "s5_summonersmiteplayerganker";

                if (SmiteRed.Any(i => Items.HasItem(i)))
                    return "s5_summonersmiteduel";

                if (SmiteGrey.Any(i => Items.HasItem(i)))
                    return "s5_summonersmitequick";

                if (SmitePurple.Any(i => Items.HasItem(i)))
                    return "itemsmiteaoe";

                return "summonersmite";
            }
        }

        private static void SetSmiteSlot()
        {
            foreach (
                var spell in
                    ObjectManager.Player.Spellbook.Spells.Where(
                        spell => String.Equals(spell.Name, Smitetype, StringComparison.CurrentCultureIgnoreCase)))
            {
                SmiteSlot = spell.Slot;
                Smite = new Spell(SmiteSlot, 700);
            }
        }

        private static void Smiteontarget(Obj_AI_Hero t)
        {
            var useSmite = Config.Item("ComboSmite").GetValue<bool>();
            var itemCheck = SmiteBlue.Any(i => Items.HasItem(i)) || SmiteRed.Any(i => Items.HasItem(i));
            if (itemCheck && useSmite &&
                ObjectManager.Player.Spellbook.CanUseSpell(SmiteSlot) == SpellState.Ready &&
                t.Distance(ObjectManager.Player.Position) < Smite.Range)
            {
                ObjectManager.Player.Spellbook.CastSpell(SmiteSlot, t);
            }
        }
        public static void DrawBox(Vector2 position, int width, int height, Color color, int borderwidth, Color borderColor)
        {
            Drawing.DrawLine(position.X, position.Y, position.X + width, position.Y, height, color);

            if (borderwidth > 0)
            {
                Drawing.DrawLine(position.X, position.Y, position.X + width, position.Y, borderwidth, borderColor);
                Drawing.DrawLine(position.X, position.Y + height, position.X + width, position.Y + height, borderwidth, borderColor);
                Drawing.DrawLine(position.X, position.Y + 2, position.X, position.Y + height, borderwidth, borderColor);
                Drawing.DrawLine(position.X + width, position.Y + 2, position.X + width, position.Y + height, borderwidth, borderColor);
            }
        }
        private static Tuple<Obj_AI_Hero, int> KillableEnemyAA
        {
            get
            {
                var x = 0;
                var t = TargetSelector.GetTarget(Orbwalking.GetRealAutoAttackRange(null) + 1400,
                    TargetSelector.DamageType.Physical);
                {
                    if (t.IsValidTarget())
                    {
                        if (t.Health
                            < ObjectManager.Player.TotalAttackDamage
                            *(1/ObjectManager.Player.AttackCastDelay > 1400 ? 8 : 4))
                        {
                            x = (int) Math.Ceiling(t.Health/ObjectManager.Player.TotalAttackDamage);
                        }
                        return new Tuple<Obj_AI_Hero, int>(t, x);
                    }

                }
                return new Tuple<Obj_AI_Hero, int>(t, x);
            }
        }
    }
}
