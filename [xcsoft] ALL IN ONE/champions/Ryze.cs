using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace _xcsoft__ALL_IN_ONE.champions
{
    class Ryze
    {
        static Menu Menu { get { return xcsoftMenu.Menu_Manual; } }
        static Orbwalking.Orbwalker Orbwalker { get { return xcsoftMenu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        static Spell Q, W, E, R;

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 625f, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W, 600f, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E, 600f, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R);

            Q.SetTargetted(0.25f, 2000f);
            W.SetTargetted(0.25f, float.MaxValue);
            E.SetTargetted(0.25f, 2000f);

            xcsoftMenu.Combo.addUseQ();
            xcsoftMenu.Combo.addUseW();
            xcsoftMenu.Combo.addUseE();
            xcsoftMenu.Combo.addUseR();

            xcsoftMenu.Harass.addUseQ();
            xcsoftMenu.Harass.addUseW();
            xcsoftMenu.Harass.addUseE();
            xcsoftMenu.Harass.addifMana();

            xcsoftMenu.Lasthit.isEmpty();

            xcsoftMenu.Laneclear.addUseQ();
            xcsoftMenu.Laneclear.addUseW();
            xcsoftMenu.Laneclear.addUseE();
            xcsoftMenu.Laneclear.addifMana();

            xcsoftMenu.Jungleclear.addUseQ();
            xcsoftMenu.Jungleclear.addUseW();
            xcsoftMenu.Jungleclear.addUseE();
            xcsoftMenu.Jungleclear.addifMana();

            xcsoftMenu.Misc.addUseKillsteal();
            xcsoftMenu.Misc.addUseAntiGapcloser();
            xcsoftMenu.Misc.addUseInterrupter();

            xcsoftMenu.Drawings.addQrange();
            xcsoftMenu.Drawings.addWrange();
            xcsoftMenu.Drawings.addErange();

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

            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);

            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color);

            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);
        }

        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!xcsoftMenu.Misc.UseAntiGapcloser || Player.IsDead)
                return;

            if (W.CanCast(gapcloser.Sender))
                W.Cast(gapcloser.Sender);
        }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!xcsoftMenu.Misc.UseInterrupter || Player.IsDead)
                return;

            if (W.CanCast(sender))
                W.Cast(sender);
        }

        static void Combo()
        {
            if (xcsoftMenu.Combo.UseQ && Q.IsReady())
            {
                Q.CastOnBestTarget();
            }

            if (xcsoftMenu.Combo.UseW && W.IsReady())
            {
                W.CastOnBestTarget();
            }

            if (xcsoftMenu.Combo.UseE && E.IsReady())
            {
                E.CastOnBestTarget();
            }

            if (xcsoftMenu.Combo.UseR && R.IsReady())
            {
                if(!Q.IsReady() && !E.IsReady() && xcsoftFunc.getHealthPercent(Player) <= 98)
                    R.Cast();
            }
        }

        static void Harass()
        {
            if (!(xcsoftFunc.getManaPercent(Player) > xcsoftMenu.Harass.ifMana))
                return;

            if (xcsoftMenu.Harass.UseQ && Q.IsReady())
            {
                Q.CastOnBestTarget();
            }

            if (xcsoftMenu.Harass.UseW && W.IsReady())
            {
                W.CastOnBestTarget();
            }

            if (xcsoftMenu.Harass.UseE && E.IsReady())
            {
                E.CastOnBestTarget();
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
                var qTarget = Minions.Where(x => Q.CanCast(x) && Q.IsKillable(x)).OrderByDescending(x => x.Health).FirstOrDefault();

                if (Q.CanCast(qTarget))
                    Q.Cast(qTarget);
            }

            if (xcsoftMenu.Laneclear.UseW && W.IsReady())
            {
                var wTarget = Minions.Where(x => W.CanCast(x) && W.IsKillable(x)).OrderByDescending(x => x.Health).FirstOrDefault();

                if (W.CanCast(wTarget))
                    W.Cast(wTarget);
            }

            if (xcsoftMenu.Laneclear.UseE && E.IsReady())
            {
                if (E.CanCast(Minions[0]))
                    E.Cast(Minions[0]);
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
                if (Q.CanCast(Mobs[0]))
                    Q.Cast(Mobs[0]);
            }

            if (xcsoftMenu.Jungleclear.UseW && W.IsReady())
            {
                if (Q.CanCast(Mobs[0]))
                    Q.Cast(Mobs[0]);
            }

            if (xcsoftMenu.Jungleclear.UseE && E.IsReady())
            {
                if (E.CanCast(Mobs[0]))
                    E.Cast(Mobs[0]);
            }
        }

        static void Killsteal()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x=>x.Health))
            {
                if (Q.CanCast(target) && xcsoftFunc.isKillable(target, Q))
                    Q.Cast(target);

                if (W.CanCast(target) && xcsoftFunc.isKillable(target, W))
                    W.Cast(target);

                if (E.CanCast(target) && xcsoftFunc.isKillable(target, E))
                    E.Cast(target);
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

            if (!Player.IsWindingUp)
                damage += (float)Player.GetAutoAttackDamage(enemy, true) + (float)Player.GetAutoAttackDamage(enemy, false);

            return damage;
        }
    }
}
