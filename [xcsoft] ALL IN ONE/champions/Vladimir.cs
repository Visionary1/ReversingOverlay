using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace _xcsoft__ALL_IN_ONE.champions
{
    class Vladimir
    {
        static Orbwalking.Orbwalker Orbwalker { get { return ALL_IN_ONE_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        static Spell Q, W, E, R;

        static int getEBuffStacks { get { var buff = ALL_IN_ONE_Func.getBuffInstance(Player, "vladimirtidesofbloodcost"); return buff != null ? buff.Count : 0; } }
        static float getEBuffDuration { get { var buff = ALL_IN_ONE_Func.getBuffInstance(Player, "vladimirtidesofbloodcost"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }
        static float getWBuffDuration { get { var buff = ALL_IN_ONE_Func.getBuffInstance(Player, "VladimirSanguinePool"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 600f, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W, 300f, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E, 590f, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R, 625f, TargetSelector.DamageType.Magical);

            Q.SetTargetted(0.25f, float.MaxValue);
            R.SetSkillshot(0.389f, 300f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            ALL_IN_ONE_Menu.Champion.Combo.addUseQ();
            ALL_IN_ONE_Menu.Champion.Combo.addUseW(false);
            ALL_IN_ONE_Menu.Champion.Combo.addUseE();
            ALL_IN_ONE_Menu.Champion.Combo.addUseR();

            ALL_IN_ONE_Menu.Champion.Harass.addUseQ();
            ALL_IN_ONE_Menu.Champion.Harass.addUseE();

            ALL_IN_ONE_Menu.Champion.Laneclear.addUseQ();
            ALL_IN_ONE_Menu.Champion.Laneclear.addUseE();

            ALL_IN_ONE_Menu.Champion.Jungleclear.addUseQ();
            ALL_IN_ONE_Menu.Champion.Jungleclear.addUseE();

             ALL_IN_ONE_Menu.Champion.Misc.addUseKillsteal();
             ALL_IN_ONE_Menu.Champion.Misc.addUseAntiGapcloser();
             ALL_IN_ONE_Menu.Champion.Misc.addItem("Auto-E For Keep Statcks", true);

            ALL_IN_ONE_Menu.Champion.Drawings.addQRange();
            ALL_IN_ONE_Menu.Champion.Drawings.addWRange(false);
            ALL_IN_ONE_Menu.Champion.Drawings.addERange();
            ALL_IN_ONE_Menu.Champion.Drawings.addRRange(false);
            ALL_IN_ONE_Menu.Champion.Drawings.addItem("W TImer", new Circle(true, Color.GreenYellow));

            ALL_IN_ONE_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

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
            if (ALL_IN_ONE_Menu.Champion.Misc.UseKillsteal)
                Killsteal();
            #endregion

            #region AutoE
            if (ALL_IN_ONE_Menu.Champion.Misc.getBoolValue("Auto-E For Keep Stacks") && !Player.IsRecalling())
                AutoE(); 
            #endregion
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = ALL_IN_ONE_Menu.Champion.Drawings.QRange;
            var drawW = ALL_IN_ONE_Menu.Champion.Drawings.WRange;
            var drawE = ALL_IN_ONE_Menu.Champion.Drawings.ERange;
            var drawR = ALL_IN_ONE_Menu.Champion.Drawings.RRange;
            var drawWTimer = ALL_IN_ONE_Menu.Champion.Drawings.getCircleValue("W Timer");

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
            if (!ALL_IN_ONE_Menu.Champion.Misc.UseAntiGapcloser || Player.IsDead)
                return;

            if (W.IsReady() && Player.Distance(gapcloser.End, false) <= W.Range)
                W.Cast();
        }

        static void Combo()
        {
            if (ALL_IN_ONE_Menu.Champion.Combo.UseQ && Q.IsReady())
                Q.CastOnBestTarget();

            if (ALL_IN_ONE_Menu.Champion.Combo.UseW && W.IsReady())
            {
                if (ALL_IN_ONE_Func.anyoneValidInRange(W.Range))
                    W.Cast();
            }

            if (ALL_IN_ONE_Menu.Champion.Combo.UseE && E.IsReady())
            {
                if (ALL_IN_ONE_Func.anyoneValidInRange(E.Range))
                    E.Cast();
            }

            if (ALL_IN_ONE_Menu.Champion.Combo.UseR && R.IsReady())
            {
                var rTarget = HeroManager.Enemies.FirstOrDefault(x=> R.GetPrediction(x, true).Hitchance >= HitChance.High);

                if(rTarget != null)
                    R.Cast(rTarget, false, true);
            }
        }

        static void Harass()
        {
            if (ALL_IN_ONE_Menu.Champion.Harass.UseQ && Q.IsReady())
            { 
                Q.CastOnBestTarget(); 
            }

            if (ALL_IN_ONE_Menu.Champion.Harass.UseQ && E.IsReady())
            {
                if (ALL_IN_ONE_Func.anyoneValidInRange(E.Range))
                    E.Cast(); 
            }
        }

        static void Laneclear()
        {
            var Minions = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (ALL_IN_ONE_Menu.Champion.Laneclear.UseQ && Q.IsReady())
            {
                var qTarget = Minions.Where(x => x.IsValidTarget(Q.Range) && Q.IsKillable(x)).OrderByDescending(x=>x.Health).FirstOrDefault();

                if (qTarget != null)
                    Q.Cast(qTarget);
            }

            if (ALL_IN_ONE_Menu.Champion.Laneclear.UseE && E.IsReady())
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

            if (ALL_IN_ONE_Menu.Champion.Jungleclear.UseQ && Q.IsReady())
            {
                var qTarget = Mobs.FirstOrDefault(x => x.IsValidTarget(Q.Range));

                if (qTarget != null)
                    Q.Cast(qTarget);
            }

            if (ALL_IN_ONE_Menu.Champion.Jungleclear.UseE && E.IsReady())
            {
                if (Mobs.Any(x => x.IsValidTarget(E.Range)))
                    E.Cast();
            }
        }

        static void Killsteal()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && ALL_IN_ONE_Func.isKillable(target, Q))
                    Q.Cast(target);

                if (E.CanCast(target) && ALL_IN_ONE_Func.isKillable(target, E))
                    E.Cast(target);

                if (R.CanCast(target) && ALL_IN_ONE_Func.isKillable(target, R))
                    R.Cast(target, false, true);
            }
        }

        static void AutoE()
        {
            if (!E.IsReady())
                return;

            if (getEBuffStacks < 4)
                E.Cast();

            if (getEBuffStacks == 4 && getEBuffDuration <= 0.5f)
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
