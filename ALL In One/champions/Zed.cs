using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace ALL_In_One.champions
{
    class Zed
    {
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        static Spell Q, W, E, R;

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 900f, TargetSelector.DamageType.Physical);
            W = new Spell(SpellSlot.W, 400f, TargetSelector.DamageType.Physical) { Speed = 1600};
            E = new Spell(SpellSlot.E, 290f, TargetSelector.DamageType.Physical) { Delay = 0.1f};
            R = new Spell(SpellSlot.R, 650f, TargetSelector.DamageType.Physical);

            Q.SetSkillshot(0.25f, 45f, 902f, false, SkillshotType.SkillshotLine);

            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();
            //AIO_Menu.Champion.Combo.addUseR();

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseW();
            AIO_Menu.Champion.Harass.addUseE();

            AIO_Menu.Champion.Lasthit.addUseQ();
            AIO_Menu.Champion.Lasthit.addUseE();

            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addUseE();

            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseE();

            AIO_Menu.Champion.Misc.addHitchanceSelector();

            AIO_Menu.Champion.Drawings.addQRange();
            AIO_Menu.Champion.Drawings.addWRange();
            AIO_Menu.Champion.Drawings.addERange();
            AIO_Menu.Champion.Drawings.addRRange();

            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalking.CanMove(100))
            {
                switch (Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Combo:
                        Combo();
                        break;
                    case Orbwalking.OrbwalkingMode.Mixed:
                        Harass();
                        break;
                    case Orbwalking.OrbwalkingMode.LastHit:
                        Lasthit();
                        break;
                    case Orbwalking.OrbwalkingMode.LaneClear:
                        Laneclear();
                        Jungleclear();
                        break;
                    case Orbwalking.OrbwalkingMode.None:
                        break;
                }
            }

            Q.MinHitChance = AIO_Menu.Champion.Misc.SelectedHitchance;
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = AIO_Menu.Champion.Drawings.QRange;
            var drawW = AIO_Menu.Champion.Drawings.WRange;
            var drawE = AIO_Menu.Champion.Drawings.ERange;
            var drawR = AIO_Menu.Champion.Drawings.RRange;

            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color, 3);

            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color, 3);

            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color, 3);

            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color, 3);
        }

        static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe || !target.IsValidTarget())
                return;

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {

            }
        }

        static void Combo()
        {

            if (AIO_Menu.Champion.Combo.UseQ && Q.IsReady())
            {
                var qTarget = TargetSelector.GetTarget(Q.Range, Q.DamageType);

                if (qTarget != null)
                    Q.Cast(qTarget);
            }

            if (AIO_Menu.Champion.Combo.UseW && W.IsReady())
            {
            }

            if (AIO_Menu.Champion.Combo.UseE && E.IsReady())
            {
                if (AIO_Func.SelfAOE_Prediction.HitCount(E.Delay, E.Range) >= 1 || AIO_Func.SelfAOE_Prediction.HitCount(E.Delay, E.Range) >= 1)
                    E.Cast();
            }

            if (AIO_Menu.Champion.Combo.UseR && R.IsReady())
            { }
        }

        static void Harass()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Harass.IfMana))
                return;

            if (AIO_Menu.Champion.Harass.UseQ && Q.IsReady())
            {
                var qTarget = TargetSelector.GetTarget(Q.Range, Q.DamageType);

                if (qTarget != null)
                    Q.Cast(qTarget);
            }

            if (AIO_Menu.Champion.Harass.UseW && W.IsReady())
            { }

            if (AIO_Menu.Champion.Harass.UseE && E.IsReady())
            {
                if (AIO_Func.SelfAOE_Prediction.HitCount(E.Delay, E.Range) >= 1 || AIO_Func.SelfAOE_Prediction.HitCount(E.Delay, E.Range) >= 1)
                    E.Cast();
            }
        }

        static void Lasthit()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Lasthit.IfMana))
                return;

            var Minions = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (AIO_Menu.Champion.Lasthit.UseQ && Q.IsReady())
            {
 
            }
        }

        static void Laneclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Laneclear.IfMana))
                return;

            var Minions = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (AIO_Menu.Champion.Laneclear.UseQ && Q.IsReady())
            {
 
            }

            if (AIO_Menu.Champion.Laneclear.UseE && E.IsReady())
            {
                if (Minions.Any(x => x.IsValidTarget(E.Range)))
                    E.Cast();
            }

            if (AIO_Menu.Champion.Laneclear.UseR && R.IsReady())
            { }
        }

        static void Jungleclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Jungleclear.IfMana))
                return;

            var Mobs = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (AIO_Menu.Champion.Jungleclear.UseQ && Q.IsReady())
            { }

            if (AIO_Menu.Champion.Jungleclear.UseW && W.IsReady())
            { }

            if (AIO_Menu.Champion.Jungleclear.UseE && E.IsReady())
            {
                if (Mobs.Any(x => x.IsValidTarget(E.Range)))
                    E.Cast();
            }

            if (AIO_Menu.Champion.Jungleclear.UseR && R.IsReady())
            { }
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
