using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace _xcsoft__ALL_IN_ONE.champions
{
    class Vladimir//by xcsoft
    {
        static Orbwalking.Orbwalker Orbwalker { get { return xcsoftMenu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        static Spell Q, W, E, R;

        static int getEBuffStacks { get { var buff = xcsoftFunc.getBuffInstance("vladimirtidesofbloodcost"); return buff != null ? buff.Count : 0; } }
        static float getEBuffDuration { get { var buff = xcsoftFunc.getBuffInstance("vladimirtidesofbloodcost"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }
        static float getWBuffDuration { get { var buff = xcsoftFunc.getBuffInstance("VladimirSanguinePool"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 600f, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W, 300f, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E, 590f, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R, 625f, TargetSelector.DamageType.Magical);

            Q.SetTargetted(0.25f, float.MaxValue);
            R.SetSkillshot(0.389f, 300f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            xcsoftMenu.Combo.addItems(new object[][] { new object[] 
            { "Use Q", true },                                      new object[] 
            { "Use W", false },                                     new object[] 
            { "Use E", true },                                      new object[] 
            { "Use R", true }                                       });

            xcsoftMenu.Harass.addItems(new object[][] { new object[] 
            { "Use Q", true },                                      new object[] 
            { "Use E", true }                                       }, false);

            xcsoftMenu.Laneclear.addItems(new object[][] { new object[] 
            { "Use Q", true},                                       new object[] 
            { "Use E", true}                                        }, false);

            xcsoftMenu.Jungleclear.addItems(new object[][] { new object[] 
            { "Use Q", true },                                      new object[] 
            { "Use E", true }                                       }, false);

            xcsoftMenu.Misc.addItems(new object[][] { new object[] 
            { "Use Killsteal" ,true},                               new object[] 
            { "Use Anti-Gapcloser", true },                         new object[] 
            { "Auto-E For Keep Stacks", true}                       });

            xcsoftMenu.Drawings.addItems(new object[][] { new object[] 
            { "Q Range", new Circle(true, Color.GreenYellow) },     new object[] 
            { "W Range", new Circle(false, Color.GreenYellow) },    new object[] 
            { "E Range", new Circle(true, Color.GreenYellow) },     new object[] 
            { "R Range", new Circle(false, Color.GreenYellow) },    new object[] 
            { "W Timer", new Circle(false, Color.GreenYellow) }     });

            xcsoftMenu.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalking.CanMove(10))
            {
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                    Combo();

                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
                    Harass();

                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
                {
                    Laneclear();
                    Jungleclear();
                }
            }

            //Orbwalker.SetAttack(!Player.HasBuff("VladimirSanguinePool"));
            Orbwalker.SetAttack(Player.IsTargetable);

            #region Killsteal
            if (xcsoftMenu.Misc.UseKillsteal)
                Killsteal();
            #endregion

            #region AutoE
            if (xcsoftMenu.Misc.getBoolValue("Auto-E For Keep Stacks") && !Player.IsRecalling())
                AutoE(); 
            #endregion
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = xcsoftMenu.Drawings.DrawQRange;
            var drawW = xcsoftMenu.Drawings.DrawQRange;
            var drawE = xcsoftMenu.Drawings.DrawQRange;
            var drawR = xcsoftMenu.Drawings.DrawQRange;
            var drawWTimer = xcsoftMenu.Drawings.getCircleValue("W Timer");

            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);

            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color);

            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);

            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);

            if (drawWTimer.Active && getWBuffDuration > 0)
            {
                var pos_temp = Drawing.WorldToScreen(Player.Position);
                Drawing.DrawText(pos_temp[0], pos_temp[1], drawWTimer.Color, "W: " + getWBuffDuration.ToString("0.00"));
            }
        }

        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!xcsoftMenu.Misc.UseAntiGapcloser || Player.IsDead)
                return;

            if (W.IsReady() && Player.Distance(gapcloser.End, false) <= W.Range)
                W.Cast();
        }

        static void Combo()
        {
            if (xcsoftMenu.Combo.UseQ && Q.IsReady())
                Q.CastOnBestTarget();

            if (xcsoftMenu.Combo.UseW && W.IsReady())
            {
                if (xcsoftFunc.anyoneValidInRange(W.Range))
                    W.Cast();
            }

            if (xcsoftMenu.Combo.UseE && E.IsReady())
            {
                if (xcsoftFunc.anyoneValidInRange(E.Range))
                    E.Cast();
            }

            if (xcsoftMenu.Combo.UseR && R.IsReady())
            {
                var rTarget = HeroManager.Enemies.FirstOrDefault(x=> R.GetPrediction(x, true).Hitchance >= HitChance.High);

                if(rTarget != null)
                    R.Cast(rTarget, false, true);
            }
        }

        static void Harass()
        {
            if (xcsoftMenu.Harass.UseQ && Q.IsReady())
            { 
                Q.CastOnBestTarget(); 
            }

            if (xcsoftMenu.Harass.UseQ && E.IsReady())
            {
                if (xcsoftFunc.anyoneValidInRange(E.Range))
                    E.Cast(); 
            }
        }

        static void Laneclear()
        {
            var Minions = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (xcsoftMenu.Laneclear.UseQ && Q.IsReady())
            {
                var qTarget = Minions.Where(x => x.IsValidTarget(Q.Range) && Q.IsKillable(x)).OrderByDescending(x=>x.Health).FirstOrDefault();

                if (qTarget != null)
                    Q.Cast(qTarget);
            }

            if (xcsoftMenu.Laneclear.UseE && E.IsReady())
            {
                if (Minions.Any(x => x.IsValidTarget(E.Range)))
                    E.Cast();
            }
        }

        static void Jungleclear()
        {
            var Mobs = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (xcsoftMenu.Jungleclear.UseQ && Q.IsReady())
            {
                var qTarget = Mobs.FirstOrDefault(x => x.IsValidTarget(Q.Range));

                if (qTarget != null)
                    Q.Cast(qTarget);
            }

            if (xcsoftMenu.Jungleclear.UseE && E.IsReady())
            {
                if (Mobs.Any(x => x.IsValidTarget(E.Range)))
                    E.Cast();
            }
        }

        static void Killsteal()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && xcsoftFunc.isKillable(target, Q))
                    Q.Cast(target);

                if (E.CanCast(target) && xcsoftFunc.isKillable(target, E))
                    E.Cast(target);

                if (R.CanCast(target) && xcsoftFunc.isKillable(target, R))
                    R.Cast(target, false, true);
            }
        }

        static void AutoE()
        {
            if (!E.IsReady())
                return;

            if (getEBuffStacks < 4)
                E.Cast();

            if (getEBuffStacks == 4 && getEBuffDuration <= 1f)
                E.Cast();
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage(enemy);

            if (W.IsReady())
                damage += W.GetDamage(enemy);

            if (E.IsReady())
                damage += E.GetDamage(enemy);

            if (R.IsReady())
                damage += R.GetDamage(enemy);

            return damage;
        }
    }
}
