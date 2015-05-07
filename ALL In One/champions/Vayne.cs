using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One.champions
{
    class Vayne// By RL244 VayneInquistion enchantment_slayer_stacks vaynetumblefade vaynetumblebonus s5test_dragonslayerbuff
    {
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}}
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Spell Q, W, E, R;
        static float getRBuffDuration { get { var buff = AIO_Func.getBuffInstance(Player, "vayneinquisition"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }


        public static void Load()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 550f, TargetSelector.DamageType.Physical);
            R = new Spell(SpellSlot.R, Player.AttackRange+50f, TargetSelector.DamageType.Physical) {Delay = 0.25f};

            E.SetTargetted(0.25f, 2400f);
            
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR();

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseW();
            AIO_Menu.Champion.Harass.addUseE();
            AIO_Menu.Champion.Harass.addIfMana();

            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addUseW();
            AIO_Menu.Champion.Laneclear.addUseE(false);
            AIO_Menu.Champion.Laneclear.addIfMana();
            
            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseW();
            AIO_Menu.Champion.Jungleclear.addUseE();
            AIO_Menu.Champion.Jungleclear.addIfMana();

            AIO_Menu.Champion.Misc.addHitchanceSelector();
            AIO_Menu.Champion.Misc.addItem("KillstealE", true);
            AIO_Menu.Champion.Misc.addUseAntiGapcloser();
            AIO_Menu.Champion.Misc.addUseInterrupter();

            AIO_Menu.Champion.Drawings.addWRange();
            AIO_Menu.Champion.Drawings.addItem("R Timer", new Circle(true, Color.Blue));
            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            Orbwalking.AfterAttack += Orbwalking_OnAfterAttack;
            //Obj_AI_Base.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalking.CanMove(10))
            {
				AIO_Func.SC(R);
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                    Combo();
				if(Player.HasBuff("vaynetumblefade")) //은신 시간동안 평타 X
				Orbwalker.SetAttack(false);
            }

            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealE"))
                KillstealE();
                
            #region AfterAttack
            AIO_Func.AASkill(Q);
            if(AIO_Func.AfterAttack())
            AA();
            #endregion
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawW = AIO_Menu.Champion.Drawings.WRange;
            var drawRTimer = AIO_Menu.Champion.Drawings.getCircleValue("R Timer");
            var pos_temp = Drawing.WorldToScreen(Player.Position);
            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color);
            if (drawRTimer.Active && getRBuffDuration > 0)
                Drawing.DrawText(pos_temp[0], pos_temp[1], drawRTimer.Color, "R: " + getRBuffDuration.ToString("0.00"));
        }

        
        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!AIO_Menu.Champion.Misc.UseAntiGapcloser || Player.IsDead)
                return;

            if (E.IsReady()
                && Player.Distance(gapcloser.Sender.Position) <= E.Range)
                E.Cast(gapcloser.Sender);
        }
        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!AIO_Menu.Champion.Misc.UseInterrupter || Player.IsDead)
                return;

            if (E.IsReady()
            && Player.Distance(sender.Position) <= E.Range)
                E.Cast(sender);
        }
        static void Orbwalking_OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var Target = (Obj_AI_Base)target;
            if (!unit.IsMe || (Target == null))
                return;
			if(!(Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo))
            AIO_Func.MouseSC(Q);
            AIO_Func.AALcJc(E);
            
            if(!utility.Activator.AfterAttack.AIO)
            AA();
        }
        
        static void AA()
        {
            var Target = TargetSelector.GetTarget(Player.AttackRange, E.DamageType);
			var buff = AIO_Func.getBuffInstance(Target, "vaynesilvereddebuf");
            AIO_Func.MouseSC(Q);
			if(buff.Count > 1 && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            AIO_Func.AACb(E);
        }
    
        static void Combo()
        {
            if (AIO_Menu.Champion.Combo.UseE && E.IsReady())
            {
                foreach (var En in HeroManager.Enemies.Where(hero => hero.IsValidTarget(E.Range) && !hero.HasBuffOfType(BuffType.SpellShield) && !hero.HasBuffOfType(BuffType.SpellImmunity)))
                {
                    //Part of VayneHunterRework. 콤보 E 로직만 참고함.

                    var EPred = E.GetPrediction(En);
                    int pushDist = 425;
                    var FinalPosition = EPred.UnitPosition.To2D().Extend(Player.ServerPosition.To2D(), -pushDist).To3D();

                    for (int i = 1; i < pushDist; i += (int)En.BoundingRadius)
                    {
                        SharpDX.Vector3 loc3 = EPred.UnitPosition.To2D().Extend(Player.ServerPosition.To2D(), -i).To3D();

                        if (loc3.IsWall() || isAllyFountain(FinalPosition))
                            E.Cast(En);
                    }
                }
            }
        }
        static bool isAllyFountain(SharpDX.Vector3 Position)
        {
            float fountainRange = 750;
            var map = Utility.Map.GetMap();
            if (map != null && map.Type == Utility.Map.MapType.SummonersRift)
            {
                fountainRange = 1050;
            }
            return
                ObjectManager.Get<GameObject>().Where(spawnPoint => spawnPoint is Obj_SpawnPoint && spawnPoint.IsAlly).Any(spawnPoint => SharpDX.Vector2.Distance(Position.To2D(), spawnPoint.Position.To2D()) < fountainRange);
        }

        static void KillstealE()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (E.CanCast(target) && AIO_Func.isKillable(target, E))
                E.Cast(target);
            }
        }
        
        
        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage(enemy) + (float)Player.GetAutoAttackDamage(enemy, true);

            if (W.IsReady())
                damage += W.GetDamage(enemy);
                
            if (E.IsReady())
                damage += E.GetDamage(enemy);
				
            if(!Player.IsWindingUp)
                damage += (float)Player.GetAutoAttackDamage(enemy, true);
            return damage;
        }
    }
}
