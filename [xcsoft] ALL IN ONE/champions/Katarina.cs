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
        static Orbwalking.Orbwalker Orbwalker { get { return ALL_IN_ONE_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        static Spell Q, W, E, R;

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 675f, TargetSelector.DamageType.Physical);
            W = new Spell(SpellSlot.W, 375f, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E, 700f);
            R = new Spell(SpellSlot.R, 550f);

            Q.SetTargetted(0.25f, 1800f);
            E.SetTargetted(0.25f, float.MaxValue);
            
            ALL_IN_ONE_Menu.Champion.Combo.addUseQ();
            ALL_IN_ONE_Menu.Champion.Combo.addUseW();
            ALL_IN_ONE_Menu.Champion.Combo.addUseE();
            ALL_IN_ONE_Menu.Champion.Combo.addUseR();

            ALL_IN_ONE_Menu.Champion.Harass.addUseQ();
            ALL_IN_ONE_Menu.Champion.Harass.addUseW();
            ALL_IN_ONE_Menu.Champion.Harass.addUseE(false);

            ALL_IN_ONE_Menu.Champion.Lasthit.addUseQ();
            ALL_IN_ONE_Menu.Champion.Lasthit.addUseW();

            ALL_IN_ONE_Menu.Champion.Laneclear.addUseQ();
            ALL_IN_ONE_Menu.Champion.Laneclear.addUseW();
            ALL_IN_ONE_Menu.Champion.Laneclear.addUseE(false);

            ALL_IN_ONE_Menu.Champion.Jungleclear.addUseQ();
            ALL_IN_ONE_Menu.Champion.Jungleclear.addUseW();
            ALL_IN_ONE_Menu.Champion.Jungleclear.addUseE();

            ALL_IN_ONE_Menu.Champion.Misc.addUseKillsteal();

            ALL_IN_ONE_Menu.Champion.Drawings.addQRange();
            ALL_IN_ONE_Menu.Champion.Drawings.addQRange();
            ALL_IN_ONE_Menu.Champion.Drawings.addERange();
            ALL_IN_ONE_Menu.Champion.Drawings.addRRange();

            ALL_IN_ONE_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

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

                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit)
                    Lasthit();

                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
                {
                    Laneclear();
                    Jungleclear();
                }
            }

            if (ALL_IN_ONE_Menu.Champion.Misc.UseKillsteal)
                Killsteal();
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = ALL_IN_ONE_Menu.Champion.Drawings.QRange;
            var drawW = ALL_IN_ONE_Menu.Champion.Drawings.WRange;
            var drawE = ALL_IN_ONE_Menu.Champion.Drawings.ERange;
            var drawR = ALL_IN_ONE_Menu.Champion.Drawings.RRange;

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
            if (ALL_IN_ONE_Menu.Champion.Combo.UseQ && Q.IsReady())
            {
                Q.CastOnBestTarget();
            }

            if (ALL_IN_ONE_Menu.Champion.Combo.UseW && W.IsReady())
            {
                if (HeroManager.Enemies.Any(x => x.IsValidTarget(W.Range)))
                    W.Cast();
            }

            if (ALL_IN_ONE_Menu.Champion.Combo.UseE && E.IsReady())
            {
                if(E.CastOnBestTarget() ==  Spell.CastStates.SuccessfullyCasted)
                {
                    if (ALL_IN_ONE_Menu.Champion.Combo.UseW && W.IsReady())
                        W.Cast();
                }
            }

            if (ALL_IN_ONE_Menu.Champion.Combo.UseR && R.IsReady())
            {
                if (HeroManager.Enemies.Any(x => x.IsValidTarget(R.Range)) && R.Instance.Name == "KatarinaR")
                    R.Cast();
                else
                    R.Cast();
            }
        }

        static void Harass()
        {
            if (ALL_IN_ONE_Menu.Champion.Harass.UseQ && Q.IsReady())
            {
                Q.CastOnBestTarget();
            }

            if (ALL_IN_ONE_Menu.Champion.Harass.UseW && W.IsReady())
            {
                if (HeroManager.Enemies.Any(x => x.IsValidTarget(W.Range)))
                    W.Cast();
            }

            if (ALL_IN_ONE_Menu.Champion.Harass.UseE && E.IsReady())
            {
                if (E.CastOnBestTarget() == Spell.CastStates.SuccessfullyCasted)
                {
                    if (ALL_IN_ONE_Menu.Champion.Combo.UseW && W.IsReady())
                        W.Cast();
                }
            }
        }

        static void Lasthit()
        {
            var Minions = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if(ALL_IN_ONE_Menu.Champion.Lasthit.UseQ && Q.IsReady())
            {
                var qTarget = Minions.Where(x => x.IsValidTarget(Q.Range) && ALL_IN_ONE_Func.isKillable(x, Q)).OrderBy(x => x.Health).FirstOrDefault();

                if (qTarget != null)
                    Q.Cast(qTarget);
            }

            if (ALL_IN_ONE_Menu.Champion.Lasthit.UseW && W.IsReady())
            {
                if (Minions.Any(x => x.IsValidTarget(W.Range) && ALL_IN_ONE_Func.isKillable(x, W)))
                    W.Cast();
            }
        }

        static void Laneclear()
        {
            var Minions = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (ALL_IN_ONE_Menu.Champion.Laneclear.UseQ && Q.IsReady())
            {
                var qTarget = Minions.FirstOrDefault(x => x.IsValidTarget(Q.Range));

                if (qTarget != null)
                    Q.Cast(qTarget);
            }

            if (ALL_IN_ONE_Menu.Champion.Laneclear.UseW && W.IsReady())
            {
                if (Minions.Any(x => x.IsValidTarget(W.Range)))
                    W.Cast();
            }

            if (ALL_IN_ONE_Menu.Champion.Laneclear.UseE && E.IsReady())
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

            if (ALL_IN_ONE_Menu.Champion.Jungleclear.UseQ && Q.IsReady())
            {
                if (Q.CanCast(Mobs.FirstOrDefault()))
                    Q.Cast(Mobs.FirstOrDefault());
            }

            if (ALL_IN_ONE_Menu.Champion.Jungleclear.UseW && W.IsReady())
            {
                if (Mobs.Any(x=>x.IsValidTarget(W.Range)))
                    W.Cast();
            }

            if (ALL_IN_ONE_Menu.Champion.Jungleclear.UseE && E.IsReady())
            {
                if (E.CanCast(Mobs.FirstOrDefault()))
                    E.Cast(Mobs.FirstOrDefault());
            }
        }

        static void Killsteal()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && ALL_IN_ONE_Func.isKillable(target, Q))
                    Q.Cast(target);

                if (W.CanCast(target) && ALL_IN_ONE_Func.isKillable(target, W))
                    W.Cast();

                if (E.CanCast(target) && ALL_IN_ONE_Func.isKillable(target, E.GetDamage(target) + Q.GetDamage(target) + W.GetDamage(target)))
                    E.Cast(target);

                if (R.CanCast(target) && ALL_IN_ONE_Func.isKillable(target, R))
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
