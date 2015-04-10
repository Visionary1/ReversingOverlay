using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace _xcsoft__ALL_IN_ONE.champions
{
    class Fiora
    {
        static Menu Menu { get { return xcsoftMenu.Menu_Manual; } }
        static Orbwalking.Orbwalker Orbwalker { get { return xcsoftMenu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Items.Item tiamatItem, hydraItem;

        static Spell Q, W, E, R;

        static void Wcancel() { Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos); }

        static List<Items.Item> itemsList = new List<Items.Item>(); //Red Smite
	static Spell Smite; //Red Smite
	static SpellSlot smiteSlot = SpellSlot.Unknown; //Red Smite
	static Items.Item s0, s1, s2, s3, s4; //Red Smite
        static float smrange = 700f; //Red Smite
		
        static float getQBuffDuration { get { var buff = xcsoftFunc.getBuffInstance(Player, "fioraqcd"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }
        static float getWBuffDuration { get { var buff = xcsoftFunc.getBuffInstance(Player, "FioraRiposte"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }
        static float getEBuffDuration { get { var buff = xcsoftFunc.getBuffInstance(Player, "FioraFlurry"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 600f, TargetSelector.DamageType.Physical);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 400f, TargetSelector.DamageType.Physical);

            Q.SetTargetted(0.25f, float.MaxValue);
            hydraItem = new Items.Item((int)ItemId.Ravenous_Hydra_Melee_Only, 250f);
            tiamatItem = new Items.Item((int)ItemId.Tiamat_Melee_Only, 250f);

            Menu.SubMenu("Combo").AddItem(new MenuItem("CbUseQ", "Use Q", true).SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("CbUseQD", "Q Least Distance", true).SetValue(new Slider(150, 0, 600)));
            Menu.SubMenu("Combo").AddItem(new MenuItem("CbUseW", "Use W (AABlock)", true).SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("CbUseH", "Hidra", true).SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("CbUseE", "E(평캔)", true).SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("CbUseR", "Use R", true).SetValue(true));

//            Menu.SubMenu("Harass").AddItem(new MenuItem("HrsUseQ", "Use Q", true).SetValue(true));
            Menu.SubMenu("Harass").AddItem(new MenuItem("HrsMana", "if Mana % >", true).SetValue(new Slider(60, 0, 100)));
            Menu.SubMenu("Harass").AddItem(new MenuItem("HrsUseE", "E(평캔)", true).SetValue(true));
            Menu.SubMenu("Harass").AddItem(new MenuItem("HrsUseH", "히드라(AAC)", true).SetValue(true));

			
			
            Menu.SubMenu("Laneclear").AddItem(new MenuItem("LcUseE", "Use E", true).SetValue(true));
            Menu.SubMenu("Laneclear").AddItem(new MenuItem("LcUseH", "히드라(AAC)", true).SetValue(true));
            Menu.SubMenu("Laneclear").AddItem(new MenuItem("LcMana", "if Mana % >", true).SetValue(new Slider(60, 0, 100)));

            Menu.SubMenu("Jungleclear").AddItem(new MenuItem("JcUseQ", "Use Q", true).SetValue(true));
            Menu.SubMenu("Jungleclear").AddItem(new MenuItem("JcUseW", "Use W", true).SetValue(true));
            Menu.SubMenu("Jungleclear").AddItem(new MenuItem("JcUseE", "Use E", true).SetValue(true));
            Menu.SubMenu("Jungleclear").AddItem(new MenuItem("JcMana", "if Mana % >", true).SetValue(new Slider(20, 0, 100)));

            Menu.SubMenu("Misc").AddItem(new MenuItem("miscKs", "Use KillSteal", true).SetValue(true));
            Menu.SubMenu("Misc").AddItem(new MenuItem("credit", "RL144", true).SetValue(true));
			
            Menu.SubMenu("Drawings").AddItem(new MenuItem("drawQ", "Q Range", true).SetValue(new Circle(true, Color.Red)));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("drawR", "R Range", true).SetValue(new Circle(true, Color.Blue)));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("drawQTimer", "Q Timer", true).SetValue(new Circle(false, Color.LightGreen)));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("drawWTimer", "W Timer", true).SetValue(new Circle(false, Color.Black)));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("drawETimer", "E Timer", true).SetValue(new Circle(true, Color.Red)));
	
		xcsoftMenu.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            Orbwalking.OnAttack += Orbwalking_OnAttack;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
			
		InitializeItems(); // RedSmite
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
			Orbwalker.SetAttack(Player.IsTargetable);
            #region Killsteal
            if (Menu.Item("miscKs", true).GetValue<bool>())
                Killsteal();
            #endregion
			
			setSmiteSlot(); //RedSmite
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = Menu.Item("drawQ", true).GetValue<Circle>();
            var drawR = Menu.Item("drawR", true).GetValue<Circle>();
            var drawQTimer = Menu.Item("drawQTimer", true).GetValue<Circle>();
			var drawWTimer = Menu.Item("drawWTimer", true).GetValue<Circle>();
            var drawETimer = Menu.Item("drawETimer", true).GetValue<Circle>();

            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);
            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);
            if (drawQTimer.Active && getQBuffDuration > 0)
            {
                var pos_temp = Drawing.WorldToScreen(Player.Position);
                Drawing.DrawText(pos_temp[0], pos_temp[1], drawQTimer.Color, "Q: " + getQBuffDuration.ToString("0.00"));
            }
            if (drawWTimer.Active && getWBuffDuration > 0)
            {
                var pos_temp = Drawing.WorldToScreen(Player.Position);
                Drawing.DrawText(pos_temp[0], pos_temp[1], drawWTimer.Color, "W: " + getWBuffDuration.ToString("0.00"));
            }
            if (drawETimer.Active && getEBuffDuration > 0)
            {
                var pos_temp = Drawing.WorldToScreen(Player.Position);
                Drawing.DrawText(pos_temp[0], pos_temp[1], drawETimer.Color, "E: " + getEBuffDuration.ToString("0.00"));
            }
        }
		
        static void setSmiteSlot() //Red Smite
        {
            foreach (var spell in ObjectManager.Player.Spellbook.Spells.Where(spell => String.Equals(spell.Name, "s5_summonersmiteduel", StringComparison.CurrentCultureIgnoreCase)))
            {
                smiteSlot = spell.Slot;
                Smite = new Spell(smiteSlot, smrange);
                return;
            }
        }
		
        static bool CheckInv() //Red Smite
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
		 
        static void InitializeItems() //Red Smite
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

		static void Orbwalking_OnAttack(AttackableUnit unit, AttackableUnit target) //AA Block
		{
            		var Target = (Obj_AI_Base)target;
            
			if(unit.IsEnemy && target.IsMe)
			W.Cast();
			else if (!unit.IsMe || Target == null)
                return;
				
            		if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo) //Red Smite
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
				var Minions = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy);
				var Mobs = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
				if(Minions.Count + Mobs.Count <= 0)
				return;
				
                		if (Menu.Item("LcUseE", true).GetValue<bool>() && E.IsReady()
				&& !tiamatItem.IsReady() && !hydraItem.IsReady())
				{    
				E.Cast();
				}
				if (Menu.Item("LcUseH", true).GetValue<bool>())
				{
					if(tiamatItem.IsReady())
						tiamatItem.Cast();
					else if(hydraItem.IsReady())
						hydraItem.Cast();
				}
			}
			
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            	{
		                if (Menu.Item("HrsUseE", true).GetValue<bool>() && E.IsReady()
	                    	&& HeroManager.Enemies.Any(x => Orbwalking.InAutoAttackRange(x))
				&& !tiamatItem.IsReady() && !hydraItem.IsReady())
		                    E.Cast();
				
				if (Menu.Item("HrsUseH", true).GetValue<bool>()
				&& HeroManager.Enemies.Any(x => Orbwalking.InAutoAttackRange(x)))
				{
					if(tiamatItem.IsReady())
						tiamatItem.Cast();
					else if(hydraItem.IsReady())
						hydraItem.Cast();
				}
		}
				
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                		if (Menu.Item("CbUseE", true).GetValue<bool>() && E.IsReady()
                    		&& HeroManager.Enemies.Any(x => Orbwalking.InAutoAttackRange(x))
				&& !tiamatItem.IsReady() && !hydraItem.IsReady())
                    		E.Cast();
					
				foreach (var rtarget in HeroManager.Enemies.OrderByDescending(x => x.Health))
				{
				if (Menu.Item("CbUseR", true).GetValue<bool>() && R.IsReady()
				&& !tiamatItem.IsReady() && !hydraItem.IsReady() && !E.IsReady()
				&& HeroManager.Enemies.Any(x => x.IsValidTarget(R.Range)))
				R.Cast(rtarget);
				}
				
				if (Menu.Item("CbUseH", true).GetValue<bool>()
					&& HeroManager.Enemies.Any(x => Orbwalking.InAutoAttackRange(x)))
				{
					if(tiamatItem.IsReady())
						tiamatItem.Cast();
					else if(hydraItem.IsReady())
						hydraItem.Cast();
				}
			}
        }

        static void Combo()
        {
 		if (Menu.Item("CbUseW", true).GetValue<bool>() && W.IsReady() 
                && HeroManager.Enemies.Any(x => x.IsValidTarget(Q.Range)))
                W.Cast();

    		if (Menu.Item("CbUseQ", true).GetValue<bool>() && Q.IsReady())  //<- Q 진입
                {
		var qd = Menu.Item("CbUseQD", true).GetValue<Slider>().Value;
		var qTarget = TargetSelector.GetTarget(Q.Range, Q.DamageType);
		var fqTarget = TargetSelector.GetTarget(Q.Range * 2, Q.DamageType);
		var fminion = ObjectManager.Get<Obj_AI_Minion>().Any(t => !t.IsAlly && Player.Distance(t.Position) <= 600 && fqTarget.Distance(t.Position) <= 600);
		if(qTarget.Distance(Player.ServerPosition) >= qd || getQBuffDuration < 1)
			Q.Cast(qTarget);
			
		if(fqTarget.Distance(Player.ServerPosition) > 600 //Chasing Enemy
		&& ObjectManager.Get<Obj_AI_Minion>().Any(t => !t.IsAlly && Player.Distance(t.Position) <= 600 && fqTarget.Distance(t.Position) <= 600))
			Q.Cast(fminion);
		}
				
        }

        static void Harass()
        {
            if (!(xcsoftFunc.getManaPercent(Player) > Menu.Item("HrsMana", true).GetValue<Slider>().Value))
                return;

        }

        static void Laneclear()
        {
            if (!(xcsoftFunc.getManaPercent(Player) > Menu.Item("LcMana", true).GetValue<Slider>().Value))
                return;

            var Minions = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy);

        }

        static void Jungleclear()
        {
            if (!(xcsoftFunc.getManaPercent(Player) > Menu.Item("JcMana", true).GetValue<Slider>().Value))
                return;

            var Mobs = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

				
            if (Menu.Item("JcUseW", true).GetValue<bool>() && W.IsReady())
                W.Cast();
			
            if (Menu.Item("JcUseQ", true).GetValue<bool>() && Q.IsReady())
                Q.Cast(Mobs[0]);
				


        }

        static void Killsteal()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && xcsoftFunc.isKillable(target, Q))
                    Q.Cast(target);
                if (Menu.Item("CbUseR", true).GetValue<bool>() && R.CanCast(target) && xcsoftFunc.isKillable(target, R))
                    R.Cast(target);
            }
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage(enemy);
				
            if (R.IsReady())
                damage += R.GetDamage(enemy);

            if(!Player.IsWindingUp)
                damage += (float)Player.GetAutoAttackDamage(enemy, true);

			if(E.IsReady())
                damage += (float)Player.GetAutoAttackDamage(enemy, true);
				
            return damage;
        }
    }
}
