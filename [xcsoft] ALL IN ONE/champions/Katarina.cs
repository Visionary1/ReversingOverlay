using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace _xcsoft__ALL_IN_ONE.champions
{
    class Katarina
    {
        static Orbwalking.Orbwalker Orbwalker { get { return xcsoftMenu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        static Spell Q, W, E, R;

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 675f, TargetSelector.DamageType.Physical);
            W = new Spell(SpellSlot.W, 375f, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E, 700f);
            R = new Spell(SpellSlot.R, 550f);

            Q.SetTargetted(0.25f, 2000f);
            E.SetTargetted(0.25f, float.MaxValue);
            
            xcsoftMenu.Combo.addUseQ();
            xcsoftMenu.Combo.addUseW();
            xcsoftMenu.Combo.addUseE();
            xcsoftMenu.Combo.addUseR();

            xcsoftMenu.Harass.addUseQ();
            xcsoftMenu.Harass.addUseW();
            xcsoftMenu.Harass.addUseE(false);

            xcsoftMenu.Laneclear.addUseQ();
            xcsoftMenu.Laneclear.addUseW();
            xcsoftMenu.Laneclear.addUseE(false);

            xcsoftMenu.Jungleclear.addUseQ();
            xcsoftMenu.Jungleclear.addUseW();
            xcsoftMenu.Jungleclear.addUseE();

            xcsoftMenu.Misc.addUseKillsteal();

            xcsoftMenu.Drawings.addQrange();
            xcsoftMenu.Drawings.addWrange();
            xcsoftMenu.Drawings.addErange();
            xcsoftMenu.Drawings.addRrange();

            xcsoftMenu.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
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

            if (xcsoftMenu.Misc.UseKillsteal)
                Killsteal();
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

        static void Combo()
        {
            if (xcsoftMenu.Combo.UseQ && Q.IsReady())
            {
                Q.CastOnBestTarget();
            }

            if (xcsoftMenu.Combo.UseW && W.IsReady())
            {
                if (HeroManager.Enemies.Any(x => x.IsValidTarget(W.Range)))
                    W.Cast();
            }

            if (xcsoftMenu.Combo.UseE && E.IsReady())
            {
                if(E.CastOnBestTarget() ==  Spell.CastStates.SuccessfullyCasted)
                {
                    if (xcsoftMenu.Combo.UseW && W.IsReady())
                        W.Cast();
                }
            }

            if (xcsoftMenu.Combo.UseR && R.IsReady())
            {
                if (HeroManager.Enemies.Any(x => x.IsValidTarget(100)))
                    R.Cast();
            }
        }

        static void Harass()
        {
            if (xcsoftMenu.Harass.UseQ && Q.IsReady())
            {
                Q.CastOnBestTarget();
            }

            if (xcsoftMenu.Harass.UseW && W.IsReady())
            {
                if (HeroManager.Enemies.Any(x => x.IsValidTarget(W.Range)))
                    W.Cast();
            }

            if (xcsoftMenu.Harass.UseE && E.IsReady())
            {
                E.CastOnBestTarget();
            }
        }

        static void Laneclear()
        {
            var Minions = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (xcsoftMenu.Laneclear.UseQ && Q.IsReady())
            {
                var qTarget = Minions.FirstOrDefault(x => x.IsValidTarget(Q.Range));

                if (qTarget != null)
                    Q.Cast(qTarget);
            }

            if (xcsoftMenu.Laneclear.UseW && W.IsReady())
            {
                if (Minions.Any(x => x.IsValidTarget(W.Range)))
                    W.Cast();
            }

            if (xcsoftMenu.Laneclear.UseE && E.IsReady())
            {
                var eTarget = Minions.FirstOrDefault(x => x.IsValidTarget(E.Range));

                if (eTarget != null)
                    E.Cast(eTarget);
            }
        }

        static void Jungleclear()
        {
            var Mobs = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (xcsoftMenu.Jungleclear.UseQ && Q.IsReady())
            {
                if (Q.CanCast(Mobs.FirstOrDefault()))
                    Q.Cast(Mobs.FirstOrDefault());
            }

            if (xcsoftMenu.Jungleclear.UseW && W.IsReady())
            {
                if (Mobs.Any(x=>x.IsValidTarget(W.Range)))
                    W.Cast();
            }

            if (xcsoftMenu.Jungleclear.UseE && E.IsReady())
            {
                if (E.CanCast(Mobs.FirstOrDefault()))
                    E.Cast(Mobs.FirstOrDefault());
            }
        }

        static void Killsteal()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && xcsoftFunc.isKillable(target, Q))
                    Q.Cast(target);

                if (W.CanCast(target) && xcsoftFunc.isKillable(target, W))
                    W.Cast();

                if (E.CanCast(target) && xcsoftFunc.isKillable(target, E))
                    E.Cast(target);

                if (R.CanCast(target) && xcsoftFunc.isKillable(target, R))
                    R.Cast();
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
