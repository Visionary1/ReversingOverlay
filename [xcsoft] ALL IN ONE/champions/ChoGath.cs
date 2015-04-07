using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace _xcsoft__ALL_IN_ONE.champions
{
    class Chogath
    {
        static Orbwalking.Orbwalker Orbwalker { get { return xcsoftMenu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        static Spell Q, W, E, R;

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 950f,  TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W, 600f, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E, 500f,  TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R, 275f,  TargetSelector.DamageType.True);

            Q.SetSkillshot(1.35f, 250f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            W.SetSkillshot(0.60f, 60f * (float)Math.PI / 180f, float.MaxValue, false, SkillshotType.SkillshotCone);
            R.SetTargetted(0.25f, float.MaxValue);

            xcsoftMenu.Combo.addUseQ();
            xcsoftMenu.Combo.addUseW();
            xcsoftMenu.Combo.addUseR();

            xcsoftMenu.Harass.addUseQ();
            xcsoftMenu.Harass.addUseW();

            xcsoftMenu.Laneclear.addUseQ();
            xcsoftMenu.Laneclear.addUseW();

            xcsoftMenu.Jungleclear.addUseQ();
            xcsoftMenu.Jungleclear.addUseW();

            xcsoftMenu.Misc.addUseKillsteal();
            xcsoftMenu.Misc.addUseAntiGapcloser();
            xcsoftMenu.Misc.addUseInterrupter();

            xcsoftMenu.Drawings.addQrange();
            xcsoftMenu.Drawings.addWrange();
            xcsoftMenu.Drawings.addErange();
            xcsoftMenu.Drawings.addRrange();

            xcsoftMenu.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
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

            #region Killsteal
            if (xcsoftMenu.Misc.UseKillsteal)
                Killsteal();
            #endregion
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = xcsoftMenu.Drawings.DrawQRange;
            var drawW = xcsoftMenu.Drawings.DrawWRange;
            var drawE = xcsoftMenu.Drawings.DrawERange;
            var drawR = xcsoftMenu.Drawings.DrawRRange;

            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);

            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color);

            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);

            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);
        }

        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!xcsoftMenu.Misc.UseAntiGapcloser || Player.IsDead)
                return;

            if (Q.CanCast(gapcloser.Sender))
                Q.Cast(gapcloser.Sender,false, true);
        }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!xcsoftMenu.Misc.UseInterrupter || Player.IsDead)
                return;

            if (Q.CanCast(sender))
                Q.Cast(sender, false, true);
        }

        static void Combo()
        {
            if (xcsoftMenu.Combo.UseQ && Q.IsReady())
            {
                Q.CastOnBestTarget(0f, false, true);
            }

            if (xcsoftMenu.Combo.UseW && W.IsReady())
            {
                W.CastOnBestTarget(0f, false, true);
            }

            if (xcsoftMenu.Combo.UseR && R.IsReady())
            {
                var rTarget = HeroManager.Enemies.Where(x => x.IsValidTarget(R.Range) && xcsoftFunc.isKillable(x, R)).OrderByDescending(x => x.Health).FirstOrDefault();

                if (rTarget != null)
                    R.CastOnUnit(rTarget);
            }
        }

        static void Harass()
        {
            if (!(xcsoftFunc.getManaPercent(Player) > xcsoftMenu.Harass.ifMana))
                return;

            if (xcsoftMenu.Harass.UseQ && Q.IsReady())
            {
                Q.CastOnBestTarget(0f, false, true);
            }

            if (xcsoftMenu.Harass.UseW && W.IsReady())
            {
                W.CastOnBestTarget(0f, false, true);
            }
        }

        static void Laneclear()
        {
            if (!(xcsoftFunc.getManaPercent(Player) > xcsoftMenu.Laneclear.ifMana))
                return;

            var Minions = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (xcsoftMenu.Laneclear.UseQ && Q.IsReady())
            {
                var Qloc = Q.GetCircularFarmLocation(Minions);

                if (Qloc.MinionsHit >= 3)
                    Q.Cast(Qloc.Position);
            }

            if (xcsoftMenu.Laneclear.UseW && W.IsReady())
            {
                if (W.CanCast(Minions.FirstOrDefault()))
                    W.Cast(Minions.FirstOrDefault(), false, true);
            }
        }

        static void Jungleclear()
        {
            if (!(xcsoftFunc.getManaPercent(Player) > xcsoftMenu.Jungleclear.ifMana))
                return;

            var Mobs = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (xcsoftMenu.Jungleclear.UseQ && Q.IsReady())
            {
                if (Q.CanCast(Mobs.FirstOrDefault()))
                    Q.Cast(Mobs.FirstOrDefault(), false, true);
            }

            if (xcsoftMenu.Jungleclear.UseW && W.IsReady())
            {
                if (W.CanCast(Mobs.FirstOrDefault()))
                    W.Cast(Mobs.FirstOrDefault(), false, true);
            }
        }

        static void Killsteal()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && xcsoftFunc.isKillable(target, Q))
                    Q.Cast(target, false, true);

                if (W.CanCast(target) && xcsoftFunc.isKillable(target, W))
                    W.Cast(target, false, true);

                if (R.CanCast(target) && xcsoftFunc.isKillable(target, R))
                    R.Cast(target);
            }
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
