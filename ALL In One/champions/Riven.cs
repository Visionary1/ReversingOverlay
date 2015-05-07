using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace ALL_In_One.champions
{
    class Riven// By RL244 rivenpassiveaaboost rivenpassive rivenwindslashready RivenFengShuiEngine RivenFeint riventricleavesoundone riventricleavesoundtwo
       //리븐을 만드려면 멘탈이 강해야합니다. 굿럭 - 오토평캔 만들다 때려치운 xcsoft. By RL244 왕위를 계승하는 중입니다..soft시여..
    {
        static Menu Menu { get { return AIO_Menu.MainMenu_Manual; } }
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Spell Q, W, E, R;
        static int Qtimer = 0;
        static float Qps {get {var buff = AIO_Func.getBuffInstance(Player, "rivenpassiveaaboost"); return buff != null ? buff.Count : 0; } }
        static float getRBuffDuration { get { var buff = AIO_Func.getBuffInstance(Player, "RivenFengShuiEngine"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }
        static bool NextQCastAllowed {get {return Qps <= 1;}} //NextQCastAllowed 쓰는거 잠시 보류
        
        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 260f + 37.5f, TargetSelector.DamageType.Physical);
            W = new Spell(SpellSlot.W, 250f + 37.5f, TargetSelector.DamageType.Magical){Delay = 0.25f};
            E = new Spell(SpellSlot.E, 325f + Player.AttackRange, TargetSelector.DamageType.Physical);//그냥 접근기로 쓰게 넣었음
            R = new Spell(SpellSlot.R, 550f, TargetSelector.DamageType.Magical);

            Q.SetSkillshot(0.25f, 112.5f, 2000f, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.25f, 150f, 2000f, false, SkillshotType.SkillshotCircle); //그냥 접근기로 쓰게 넣었음
            R.SetSkillshot(0.25f, 60f * (float)Math.PI / 180, 2200f, false, SkillshotType.SkillshotCone);
            
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR();

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseW();
            AIO_Menu.Champion.Harass.addUseE();
            
            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addUseW();

            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseW();
            AIO_Menu.Champion.Jungleclear.addUseE();

            AIO_Menu.Champion.Misc.addHitchanceSelector();
            AIO_Menu.Champion.Misc.addItem("KillstealR", true);
            AIO_Menu.Champion.Misc.addItem("Inteligent Q", true);
            AIO_Menu.Champion.Drawings.addQRange();
            AIO_Menu.Champion.Drawings.addWRange();
            AIO_Menu.Champion.Drawings.addERange();
            AIO_Menu.Champion.Drawings.addRRange();
            AIO_Menu.Champion.Drawings.addItem("R Timer", new Circle(true, Color.Red));

        
            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            AttackableUnit.OnDamage += OnDamage;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
			Spellbook.OnCastSpell += OnCastSpell;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalking.CanMove(10))
            {
                AIO_Func.SC(W,0,0,0f);
                AIO_Func.SC(E,50f,float.MaxValue,0f);
                switch (Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Combo:
                        Orbwalker.SetAttack(true);
                        Combo();
                        break;
                    case Orbwalking.OrbwalkingMode.Mixed:
                        Orbwalker.SetAttack(true);
                        Harass();
                        break;
                }
            }

            #region Killsteal
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealR"))
                KillstealR();
            #endregion
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

            var drawQ = AIO_Menu.Champion.Drawings.QRange;
            var drawW = AIO_Menu.Champion.Drawings.WRange;
            var drawE = AIO_Menu.Champion.Drawings.ERange;
            var drawR = AIO_Menu.Champion.Drawings.RRange;
            var drawRTimer = AIO_Menu.Champion.Drawings.getCircleValue("R Timer");
            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);
            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color);
            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range - Player.AttackRange, drawE.Color);
            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);
            var pos_temp = Drawing.WorldToScreen(Player.Position);
            if (drawRTimer.Active && getRBuffDuration > 0)
                Drawing.DrawText(pos_temp[0], pos_temp[1], drawRTimer.Color, "R: " + getRBuffDuration.ToString("0.00"));
        }
        
        static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe)
            return;
            if (args.SData.Name == "RivenTriCleave")
            Qtimer = Utils.TickCount;
        }
		
        static void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (!sender.Owner.IsMe)
            return;
            if (args.Slot == SpellSlot.W)
			{
			if(Items.HasItem((int)ItemId.Ravenous_Hydra_Melee_Only) && Items.CanUseItem((int)ItemId.Ravenous_Hydra_Melee_Only))
            Items.UseItem((int)ItemId.Ravenous_Hydra_Melee_Only);
			if(Items.HasItem((int)ItemId.Tiamat_Melee_Only) && Items.CanUseItem((int)ItemId.Tiamat_Melee_Only))
			Items.UseItem((int)ItemId.Tiamat_Melee_Only);
			}
		}
        
        static void OnDamage(AttackableUnit sender, AttackableUnitDamageEventArgs args)
        {
            if (sender == null || args.TargetNetworkId != sender.NetworkId)
            return;
            
            if ((int) args.Type != 70)
            return;
            
            if(Qtimer > Utils.TickCount - 120)
            {
            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            Utility.DelayAction.Add(15, Orbwalking.ResetAutoAttackTimer);
            }
        }
        
        static void AA()
        {
            if(Qtimer < Utils.TickCount - 250)
            AIO_Func.AACb(Q,0,0,0);
        }
        
        static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var Target = (Obj_AI_Base)target;
            if (!unit.IsMe || Target == null)
                return;
			if(Qtimer < Utils.TickCount - 250)
            AIO_Func.AALcJc(Q,0,0,0);
            if(!utility.Activator.AfterAttack.AIO)
            AA();
        }
        
        static void Combo()
        {
            if (AIO_Menu.Champion.Combo.UseR && R.IsReady())
            {
                var rTarget = TargetSelector.GetTarget(W.Range, R.DamageType, true);
                if(rTarget != null && !Player.HasBuff("rivenwindslashready"))
                R.Cast();
                if(Player.HasBuff("rivenwindslashready"))
                {
                    foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
                    {
                        if (R.CanCast(target) && AIO_Func.isKillable(target, R))
                            R.Cast(target);
                    }
                }
            }
            if (AIO_Menu.Champion.Combo.UseQ && Q.IsReady() && AIO_Menu.Champion.Misc.getBoolValue("Inteligent Q"))
            {
                var qTarget = TargetSelector.GetTarget(Q.Range+40, Q.DamageType, true);
                if(qTarget != null && (qTarget.Distance(Player.ServerPosition) > Player.AttackRange + 90 || Qtimer < Utils.TickCount - 1200))
                Q.Cast(qTarget.ServerPosition);
            }
        }
        
        static void Harass()
        {
            if (AIO_Menu.Champion.Harass.UseQ && Q.IsReady() && AIO_Menu.Champion.Misc.getBoolValue("Inteligent Q"))
            {
                var qTarget = TargetSelector.GetTarget(Q.Range+40, Q.DamageType, true);
                if(qTarget != null && (qTarget.Distance(Player.ServerPosition) > Player.AttackRange + 90 || Qtimer < Utils.TickCount - 1200))
                Q.Cast(qTarget.ServerPosition);
            }
        }

        static void KillstealR()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (R.CanCast(target) && AIO_Func.isKillable(target, R))
                    R.Cast(target);
            }
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage(enemy)*3 + (float)Player.GetAutoAttackDamage(enemy, false)*4;
            
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
