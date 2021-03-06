﻿using System;
using System.CodeDom;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using EloBuddy;
using Slutty_ryze.Properties;

namespace Slutty_ryze
{
    internal class Program
    {
        static readonly Random Seeder = new Random();
        private static bool _casted;
        private static int _lastw;

        public static int rRange { get; private set; }
        #region onload
        private static void Main(string[] args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));
            // So you can test if it in VS wihout crahses
#if !DEBUG

            EloBuddy.SDK.Events.Loading.OnLoadingComplete += OnLoad;
#endif
        }

        private static void OnLoad(EventArgs args)
        {
            if (GlobalManager.GetHero.ChampionName != Champion.ChampName)
                return;

            Console.WriteLine(@"Loading Your Slutty Ryze");

            Humanizer.AddAction("generalDelay",35.0f);

            Champion.Q = new Spell(SpellSlot.Q, 865);
            Champion.Qn = new Spell(SpellSlot.Q, 865);
            Champion.W = new Spell(SpellSlot.W, 585);
            Champion.E = new Spell(SpellSlot.E, 585);
            Champion.R = new Spell(SpellSlot.R);

            Champion.Q.SetSkillshot(0.25f, 50f, 1700f, true, SkillshotType.SkillshotLine);
            Champion.Qn.SetSkillshot(0.25f, 50f, 1700f, false, SkillshotType.SkillshotLine);

            //assign menu from MenuManager to Config
            Console.WriteLine(@"Loading Your Slutty Menu...");
            GlobalManager.Config = MenuManager.GetMenu();
            GlobalManager.Config.AddToMainMenu();
            Printmsg("Ryze Assembly Loaded! Make sure to test new combo! (IMPROVED BIG TIME! GIVE IT A TRY!");
            Printmsg1("Current Version: " + typeof(Program).Assembly.GetName().Version);
            Printmsg2("Don't Forget To " + "<font color='#00ff00'>[Upvote]</font> <font color='#FFFFFF'>" + "The Assembly In The Databse" + "</font>");
            //Other damge inficators in MenuManager ????
            GlobalManager.DamageToUnit = Champion.GetComboDamage;

            Drawing.OnDraw += DrawManager.Drawing_OnDraw;
            Game.OnUpdate += Game_OnUpdate;
            AntiGapcloser.OnEnemyGapcloser += Champion.OnGapClose;
            Interrupter2.OnInterruptableTarget+= Champion.RyzeInterruptableSpell;
            Orbwalking.BeforeAttack += Champion.Orbwalking_BeforeAttack;
            //CustomEvents.Unit.OnDash += Champion;
            ShowDisplayMessage();

        }

        #endregion
        private static void Printmsg(string message)
        {
            Chat.Print(
                "<font color='#6f00ff'>[Slutty Ryze]:</font> <font color='#FFFFFF'>" + message + "</font>");
        }

        private static void Printmsg1(string message)
        {
            Chat.Print(
                "<font color='#ff00ff'>[Slutty Ryze]:</font> <font color='#FFFFFF'>" + message + "</font>");
        }

        private static void Printmsg2(string message)
        {
            Chat.Print(
                "<font color='#00abff'>[Slutty Ryze]:</font> <font color='#FFFFFF'>" + message + "</font>");
        }
        #region onGameUpdate

        private static void ShowDisplayMessage()
        {
            var r = new Random();
      
            var txt = Resources.display.Split('\n');
            switch (r.Next(1, 3))
            {
                case 2:
                    txt = Resources.display2.Split('\n');
                    break;
                case 3:
                    txt = Resources.display3.Split('\n');
                    break;
            }

            foreach (var s in txt)
                Console.WriteLine(s);
            #region L# does not allow D:
            //try
            //{
            //    var sr = new System.IO.StreamReader(System.Net.WebRequest.Create(string.Format("http://www.fiikus.net/asciiart/pokemon/{0}{1}{2}.txt", r.Next(0, 1), r.Next(0, 3), r.Next(0, 9))).GetResponse().GetResponseStream());
            //    string line;
            //    while ((line = sr.ReadLine()) != null)
            //    {
            //        Console.WriteLine(line);
            //    }
            //}

            //catch
            //{
            //    // ignored
            //}
            #endregion
        }
        private static void Game_OnUpdate(EventArgs args)
        {
            try // lazy
            {
                //var target2 = TargetSelector.GetTarget(Champion.Q.Range, TargetSelector.DamageType.Magical);
                //if (target2.IsValidTarget())
                //    Game.PrintChat(Champion.Q.GetPrediction(target2).CollisionObjects.Count.ToString());



                if (GlobalManager.Config.Item("chase").GetValue<KeyBind>().Active)
                {
                    switch (Champion.R.Level)
                    {
                        case 1:
                            rRange = 1500;
                            break;
                        case 2:
                            rRange = 3000;
                            break;
                    }
                    Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                    var targets = TargetSelector.GetTarget(rRange, TargetSelector.DamageType.Magical);
                    if (!targets.IsValidTarget())
                        return;

                    if (GlobalManager.Config.Item("usewchase").GetValue<bool>() && targets.IsValidTarget(Champion.E.Range))
                        LaneOptions.CastW(targets);

                    var target1 = TargetSelector.GetSelectedTarget();
                    if (!target1.IsValidTarget(rRange)) return;
                    if (GlobalManager.Config.Item("chaser").GetValue<bool>() &&
                        target1.Distance(GlobalManager.GetHero) > Champion.W.Range + 200 &&
                        targets.Distance(GlobalManager.GetHero) < rRange
                        && Champion.R.IsReady())
                    {
                            Champion.R.Cast(GlobalManager.GetHero.Position.Extend(target1.Position,
                                target1.Distance(GlobalManager.GetHero.Position) + 260));                      
                    }
                }
           
                if (GlobalManager.GetHero.IsDead)
                    return;
                if (GlobalManager.GetHero.IsRecalling())
                    return;

                if (Champion.casted == false)
                {
                    MenuManager.Orbwalker.SetAttack(true);
                }

                var target = TargetSelector.GetTarget(Champion.Q.Range, TargetSelector.DamageType.Magical);


                if (GlobalManager.Config.Item("doHuman").GetValue<bool>())
                {
                    if (!Humanizer.CheckDelay("generalDelay")) // Wait for delay for all other events
                    {
                        return;
                    }
                    //Console.WriteLine("Seeding Human Delay");
                    var nDelay = Seeder.Next(GlobalManager.Config.Item("minDelay").GetValue<Slider>().Value, GlobalManager.Config.Item("maxDelay").GetValue<Slider>().Value); // set a new random delay :D
                    Humanizer.ChangeDelay("generalDelay", nDelay);
                }

                if (MenuManager.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                {

                    var expires = (GlobalManager.GetHero.Spellbook.GetSpell(SpellSlot.Q).CooldownExpires);
                    var CD =
                        (int)
                            (expires -
                             (Game.Time - 1));
                    if (Champion.W.IsReady() && !(CD < 2.5f))
                    {
                        MenuManager.Orbwalker.SetAttack(true);
                    }
                    else
                    {
                        MenuManager.Orbwalker.SetAttack(false);
                    }

                    Champion.AABlock();
                    LaneOptions.Combo();

                    MenuManager.Orbwalker.SetAttack(!(target.Distance(GlobalManager.GetHero) >=
                                                      GlobalManager.Config.Item("minaarange").GetValue<Slider>().Value));
                }

                if (MenuManager.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
                {
                    LaneOptions.Mixed();
                    MenuManager.Orbwalker.SetAttack(true);
                }

                if (MenuManager.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
                {
                    LaneOptions.JungleClear();
                        LaneOptions.LaneClear();
                }

                if (MenuManager.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit)
                    LaneOptions.LastHit();


                if (MenuManager.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.None)
                {
                    if (GlobalManager.Config.Item("tearS").GetValue<KeyBind>().Active)
                        ItemManager.TearStack();

                    ItemManager.Potion();
                    MenuManager.Orbwalker.SetAttack(true);
                }

                if (GlobalManager.Config.Item("UseQauto").GetValue<bool>() && target != null)
                {
                    if (Champion.Q.IsReady() && target.IsValidTarget(Champion.Q.Range))
                        Champion.Q.Cast(target);
                }


                // Seplane();
                ItemManager.Item();
                Champion.KillSteal();
                ItemManager.Potion();

                if (GlobalManager.Config.Item("level").GetValue<bool>())
                {
                    AutoLevelManager.LevelUpSpells();
                }

                if (!GlobalManager.Config.Item("autow").GetValue<bool>() || !target.UnderTurret(true)) return;

                if (target == null)
                    return;

                if (!ObjectManager.Get<Obj_AI_Turret>()
                    .Any(turret => turret.IsValidTarget(300) && turret.IsAlly && turret.Health > 0))
                    return;

                 Champion.W.CastOnUnit(target);
                // DebugClass.ShowDebugInfo(true);
            }
            catch
            {
                // ignored
            }
        }
        #endregion

        /*
        private static void Seplane()
        {
            if (GlobalManager.GetHero.IsValid &&
                GlobalManager.Config.Item("seplane").GetValue<KeyBind>().Active)
            {
                ObjectManager.GlobalManager.GetHero.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                LaneClear();
            }
        }
         */



    }
}
