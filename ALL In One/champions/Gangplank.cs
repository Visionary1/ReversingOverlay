using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One.champions
{
    class Gangplank //RL244
    {
        static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}} //
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Spell Q, W, E, R;
        static List<Items.Item> itemsList = new List<Items.Item>(); //척후병 샤브르
		static Spell Smite;
		static SpellSlot smiteSlot = SpellSlot.Unknown;
		static Items.Item s0, s1, s2, s3, s4;
        static float smrange = 700f;
        static float getEBuffDuration { get { var buff = AIO_Func.getBuffInstance(Player, "RaiseMoraleBuff"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 625f, TargetSelector.DamageType.Physical);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 1200f);
            R = new Spell(SpellSlot.R, 25000f, TargetSelector.DamageType.Magical);

            Q.SetTargetted(0.25f, float.MaxValue);
            R.SetSkillshot(0.25f, 575f, float.MaxValue, false, SkillshotType.SkillshotCircle);

			
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR();

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addIfMana();

            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addUseW();
            AIO_Menu.Champion.Laneclear.addUseE();
            AIO_Menu.Champion.Laneclear.addIfMana();


            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseW();
            AIO_Menu.Champion.Jungleclear.addUseE();
            AIO_Menu.Champion.Jungleclear.addIfMana();

            AIO_Menu.Champion.Misc.addUseKillsteal();
            AIO_Menu.Champion.Misc.addItem("KillstealR", false);
            AIO_Menu.Champion.Drawings.addQRange();
            AIO_Menu.Champion.Drawings.addERange();
            AIO_Menu.Champion.Drawings.addItem("E Timer", new Circle(true, Color.LightGreen));
			
			AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            Orbwalking.OnAttack += Orbwalking_OnAttack;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
			
			InitializeItems();
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
            if (AIO_Menu.Champion.Misc.UseKillsteal)
            Killsteal();
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealR"))
			KillstealR();
            #endregion
			
	    setSmiteSlot();
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

		var drawQ = AIO_Menu.Champion.Drawings.QRange;
		var drawE = AIO_Menu.Champion.Drawings.ERange;
		var drawETimer = AIO_Menu.Champion.Drawings.getCircleValue("E Timer");
		
		if (Q.IsReady() && drawQ.Active)
		Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);
		
		if (E.IsReady() && drawE.Active)
		Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);

		var pos_temp = Drawing.WorldToScreen(Player.Position);
		
		if (drawETimer.Active && getEBuffDuration > 0)
		Drawing.DrawText(pos_temp[0], pos_temp[1], drawETimer.Color, "E: " + getEBuffDuration.ToString("0.00"));
		
        }
		
        static void setSmiteSlot()
        {
            foreach (var spell in ObjectManager.Player.Spellbook.Spells.Where(spell => String.Equals(spell.Name, "s5_summonersmiteduel", StringComparison.CurrentCultureIgnoreCase)))
            {
                smiteSlot = spell.Slot;
                Smite = new Spell(smiteSlot, smrange);
                return;
            }
        }
		
        static bool CheckInv()
        {
            bool b = false;
            foreach(var item in itemsList)
            {
                if(Player.InventoryItems.Any(f => f.Id == (ItemId)item.Id))
                {
                    b = true;
                }
            }
            return b;
        }
		
        static void InitializeItems()
        {
            s0 = new Items.Item(3714, smrange);
            itemsList.Add(s0);
            s1 = new Items.Item(3715, smrange);
            itemsList.Add(s1);
            s2 = new Items.Item(3716, smrange);
            itemsList.Add(s2);
            s3 = new Items.Item(3717, smrange);
            itemsList.Add(s3);
            s4 = new Items.Item(3718, smrange);
            itemsList.Add(s4);
        }

        static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe || Player.IsDead)
                return;
				

        }

	static void Orbwalking_OnAttack(AttackableUnit unit, AttackableUnit target)
	{
            var Target = (Obj_AI_Base)target;
            
	    if (!unit.IsMe || Target == null)
                return;
				
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
		{
		if (!CheckInv())
		return;
		 Smite.Slot = smiteSlot;
		if(smiteSlot.IsReady())
		 Player.Spellbook.CastSpell(smiteSlot, Target);
		}
	}
		
        static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var Target = (Obj_AI_Base)target;
            if (!unit.IsMe || Target == null)
                return;

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
			var Minions = MinionManager.GetMinions(E.Range, MinionTypes.All, MinionTeam.Enemy);
			var Mobs = MinionManager.GetMinions(E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

			if(Minions.Count + Mobs.Count <= 0)
			return;
			
			if (Minions.Count >= 1)
			AALaneclear();
			
			if (Mobs.Count >= 1)
			AAJungleclear();
					
			}
			
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                if (AIO_Menu.Champion.Combo.UseQ && Q.IsReady() && utility.Activator.AfterAttack.ALLCancleItemsAreCasted
                    && Q.CanCast(Target))
                    Q.Cast(Target);
			}
				
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                if (AIO_Menu.Champion.Combo.UseQ && Q.IsReady() && utility.Activator.AfterAttack.ALLCancleItemsAreCasted
                    && Q.CanCast(Target))
                    Q.Cast(Target);					
			}
        }

        static void Combo()
        {
            if (AIO_Menu.Champion.Combo.UseE && E.IsReady())
            {
                E.Cast();
            }

            if (AIO_Menu.Champion.Combo.UseQ && Q.IsReady())
            {
               
                var qTarget = TargetSelector.GetTarget(Q.Range, Q.DamageType, true);
      
                if (qTarget != null && !Player.IsDashing())
                    Q.Cast(qTarget);       
            }
			
            if (AIO_Menu.Champion.Combo.UseR && R.IsReady())
            {
                var rTarget = TargetSelector.GetTarget(Q.Range, R.DamageType, true);
                if (rTarget != null && !Player.IsDashing() && AIO_Func.getHealthPercent(rTarget) <= 50)
                R.Cast(rTarget.Position);
            }
				
        }

        static void Harass()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Harass.IfMana))
                return;
				
            if (AIO_Menu.Champion.Harass.UseQ && Q.IsReady())
            {
               
                var qTarget = TargetSelector.GetTarget(Q.Range, Q.DamageType, true);

        
                if (qTarget != null && !Player.IsDashing())
                    Q.Cast(qTarget);       
            }

        }
		
        static void AALaneclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Laneclear.IfMana))
                return;

			var Minions = MinionManager.GetMinions(E.Range, MinionTypes.All, MinionTeam.Enemy);

			if (Minions.Count <= 0)
                return;
				
                //if (AIO_Menu.Champion.Laneclear.UseQ && Q.IsReady() && utility.Activator.AfterAttack.ALLCancleItemsAreCasted)
                //    Q.Cast(Minions[0]);
        }

        static void AAJungleclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Jungleclear.IfMana))
                return;

            var Mobs = MinionManager.GetMinions(E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;
				
            //if (AIO_Menu.Champion.Jungleclear.UseQ && Q.IsReady() && utility.Activator.AfterAttack.ALLCancleItemsAreCasted)
            //    Q.Cast(Mobs[0]);
        }


        static void Laneclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Laneclear.IfMana))
                return;

            var Minions = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

				
            if (AIO_Menu.Champion.Laneclear.UseQ && Q.IsReady())
            {
				var _m = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth).FirstOrDefault(m => m.Health < ((Player.GetSpellDamage(m, SpellSlot.Q))));
      
                if (_m != null && !Player.IsDashing())
                    Q.Cast(_m);       
            }
				
            if (AIO_Menu.Champion.Laneclear.UseE && E.IsReady())
                E.Cast();
        }

        static void Jungleclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Jungleclear.IfMana))
                return;

            var Mobs = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (AIO_Menu.Champion.Jungleclear.UseQ && Q.IsReady())
            {
				var _m = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth).FirstOrDefault(m => m.Health < ((Player.GetSpellDamage(m, SpellSlot.Q))));
      
                if (_m != null && !Player.IsDashing())
                    Q.Cast(_m);       
            }
			
            if (AIO_Menu.Champion.Jungleclear.UseE && E.IsReady())
                E.Cast();
        }

        static void Killsteal()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && AIO_Func.isKillable(target, Q))
                    Q.Cast(target);
            }
        }
		
        static void KillstealR()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (R.CanCast(target) && R.GetDamage(target)*((R.Width/(target.MoveSpeed*0.75f))/7) >= target.Health + target.HPRegenRate)
                    R.Cast(target.Position);
            }
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
			{
                damage += Q.GetDamage(enemy);
                damage += (float)Player.GetAutoAttackDamage(enemy, true);
			}
			
            if (Items.CanUseItem((int)ItemId.Tiamat_Melee_Only))
			{
			damage += (float)Player.GetItemDamage(enemy, Damage.DamageItems.Tiamat);
			damage += (float)Player.GetAutoAttackDamage(enemy, true);
			}
			
            if (Items.CanUseItem((int)ItemId.Ravenous_Hydra_Melee_Only))
			{
			damage += (float)Player.GetItemDamage(enemy, Damage.DamageItems.Hydra);
			damage += (float)Player.GetAutoAttackDamage(enemy, true);
			}

            if (R.IsReady() && AIO_Menu.Champion.Combo.UseR)
                damage += R.GetDamage(enemy)*((R.Width/(enemy.MoveSpeed*0.75f))/7);

            if(!Player.IsWindingUp)
                damage += (float)Player.GetAutoAttackDamage(enemy, true);

            return damage;
        }
    }
}
