using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;


namespace _xcsoft__ALL_IN_ONE.champions
{
    class Sivir
    {
        static Orbwalking.Orbwalker Orbwalker { get { return xcsoftMenu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }


        static Spell Q, W, E, R;

        public static void Load()
        {

            Q = new Spell(SpellSlot.Q, 1245f, TargetSelector.DamageType.Physical);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 1000f);


            Q.SetSkillshot(0.25f, 90f, 1350f, false, SkillshotType.SkillshotLine);




            xcsoftMenu.Combo.addUseQ();
            xcsoftMenu.Combo.addUseW();
            xcsoftMenu.Combo.addUseE();
            xcsoftMenu.Combo.addUseR();



            xcsoftMenu.Harass.addUseQ();
            xcsoftMenu.Harass.addUseW();
            xcsoftMenu.Harass.addifMana(60);

            xcsoftMenu.Laneclear.addUseQ();
            xcsoftMenu.Laneclear.addUseW();
            xcsoftMenu.Laneclear.addifMana();

            xcsoftMenu.Jungleclear.addUseQ();
            xcsoftMenu.Jungleclear.addUseW();
            xcsoftMenu.Jungleclear.addifMana();

            xcsoftMenu.Misc.addUseKillsteal();
            xcsoftMenu.Misc.addItem("AutoQ", true);

            xcsoftMenu.Drawings.addQrange();
            xcsoftMenu.Drawings.addRrange();

        
            xcsoftMenu.Drawings.addDamageIndicator(getComboDamage);

            
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            //Orbwalking.AfterAttack += Orbwalking_OnAfterAttack; broken
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
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

                AutoQ();
            }

            if (xcsoftMenu.Misc.UseKillsteal)
                Killsteal();
        }

        static void Drawing_OnDraw(EventArgs args)
        {
          
            if (Player.IsDead)
                return;

  
            var drawQ = xcsoftMenu.Drawings.DrawQRange;
            var drawR = xcsoftMenu.Drawings.DrawRRange;

            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);

            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);
        }

        static void Orbwalking_OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!W.IsReady() || !unit.IsMe || !(target.Type == GameObjectType.obj_AI_Hero))
                return;

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit)
                return;


            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && xcsoftMenu.Combo.UseW)
            {
                W.Cast();
                Orbwalking.ResetAutoAttackTimer();
            }
            else
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && xcsoftMenu.Harass.UseW && xcsoftFunc.getManaPercent(Player) > xcsoftMenu.Harass.ifMana)
                {
                    W.Cast();
                    Orbwalking.ResetAutoAttackTimer();
                }
        }
        static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!xcsoftMenu.Combo.UseE || Player.IsDead)
                return;

            if (sender is Obj_AI_Hero && sender.IsEnemy && args.Target.IsMe && !args.SData.IsAutoAttack() && E.IsReady())
                E.Cast();

        }
        static float getBuffDuration // afterattack tempfix
        {
            get
            {
                var buff = xcsoftFunc.getBuffInstance(Player, "sivirpassivespeed");

                return buff != null ? buff.EndTime - Game.ClockTime : 0;
            }
        }

        static void AutoQ()
        {
            if (!xcsoftMenu.Misc.getBoolValue("AutoQ"))
                return;
            foreach (Obj_AI_Hero target in HeroManager.Enemies.Where(x => x.IsValidTarget(Q.Range) && !x.HasBuffOfType(BuffType.Invulnerability)))
            {
                if (target != null)
                {
                    if (Q.CanCast(target) && Q.GetPrediction(target).Hitchance >= HitChance.Immobile)
                        Q.Cast(target);
                }
            }
        }
        static void Combo()
        {
            if (xcsoftMenu.Combo.UseQ && Q.IsReady())
            {
                var qTarget = TargetSelector.GetTarget(Q.Range, Q.DamageType, true);

                if (qTarget != null && Q.GetPrediction(qTarget).Hitchance >= HitChance.VeryHigh)
                    Q.Cast(qTarget);

            }

            if (xcsoftMenu.Combo.UseW && W.IsReady()) // afterattack tempfix
            {
                if (getBuffDuration > 1.95 && HeroManager.Enemies.Any(x => Orbwalking.InAutoAttackRange(x)))
                {
                    W.Cast();
                    Orbwalking.ResetAutoAttackTimer();
                }
            }
        }

        static void Harass()
        {

            if (!(xcsoftFunc.getManaPercent(Player) > xcsoftMenu.Harass.ifMana))
                return;

            if (xcsoftMenu.Harass.UseQ && Q.IsReady())
            {
                var qTarget = TargetSelector.GetTarget(Q.Range, Q.DamageType, true);

                if (qTarget != null && Q.GetPrediction(qTarget).Hitchance >= HitChance.VeryHigh)
                    Q.Cast(qTarget);
            }

            if (xcsoftMenu.Combo.UseW && W.IsReady()) // afterattack tempfix
            {
                if (getBuffDuration > 1.95 && HeroManager.Enemies.Any(x => Orbwalking.InAutoAttackRange(x)))
                {
                    W.Cast();
                    Orbwalking.ResetAutoAttackTimer();
                }
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
                var farmloc = Q.GetLineFarmLocation(Minions);

                if (farmloc.MinionsHit >= 3)
                    Q.Cast(farmloc.Position);
            }

            if (xcsoftMenu.Laneclear.UseW && W.IsReady())
            {
                if (Minions.Count >= 4)
                    W.Cast();
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
                    Q.Cast(Mobs.FirstOrDefault());
            }

            if (xcsoftMenu.Jungleclear.UseW && W.IsReady())
            {
                if (W.CanCast(Mobs.FirstOrDefault()))
                    W.Cast(Mobs.FirstOrDefault());
            }

        }

        static void Killsteal()
        {

            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && xcsoftFunc.isKillable(target, Q))
                    Q.Cast(target);

            }
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {

            float damage = 0;


            if (Q.IsReady())
                damage += Q.GetDamage(enemy);

            if (W.IsReady())
                damage += W.GetDamage(enemy);


            return damage;
        }
    }
}
