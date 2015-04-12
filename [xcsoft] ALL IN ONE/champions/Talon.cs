using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using LeagueSharp;
using LeagueSharp.Common;

namespace _xcsoft__ALL_IN_ONE.champions
{
    class Talon //RL144
    {
        static Menu Menu { get { return xcsoftMenu.Menu_Manual; } } //메뉴얼 오브워커 넣기는 했지만. 음.. 
        static Orbwalking.Orbwalker Orbwalker { get { return xcsoftMenu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Items.Item tiamatItem, hydraItem; //히드라 평캔을 위함

        static Spell Q, W, E, R;
        static List<Items.Item> itemsList = new List<Items.Item>(); //척후병 샤브르
		static Spell Smite;
		static SpellSlot smiteSlot = SpellSlot.Unknown;
		static Items.Item s0, s1, s2, s3, s4;
        static float smrange = 700f;
		
        static float getQBuffDuration { get { var buff = xcsoftFunc.getBuffInstance(Player, "TalonNoxianDiplomacyBuff"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }
        static float getEBuffDuration { get { var buff = xcsoftFunc.getBuffInstance(Player, "TalonDamageAmp"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }
        static float getRBuffDuration { get { var buff = xcsoftFunc.getBuffInstance(Player, "TalonDisappear"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 600f, TargetSelector.DamageType.Physical);
            E = new Spell(SpellSlot.E, 700f, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R, 500f, TargetSelector.DamageType.Physical);

            W.SetSkillshot(0.25f, 60f * (float)Math.PI / 180, 2000f, false, SkillshotType.SkillshotCone);
            E.SetTargetted(0.25f, float.MaxValue);
            hydraItem = new Items.Item((int)ItemId.Ravenous_Hydra_Melee_Only, 250f);
            tiamatItem = new Items.Item((int)ItemId.Tiamat_Melee_Only, 250f);
			
            xcsoftMenu.Combo.addUseQ();
            xcsoftMenu.Combo.addUseW();
            xcsoftMenu.Combo.addUseE();
            xcsoftMenu.Combo.addUseR();
            xcsoftMenu.Combo.addItem("Use Hydra", true);

            xcsoftMenu.Harass.addUseW();
            xcsoftMenu.Harass.addifMana();

            xcsoftMenu.Laneclear.addUseQ();
            xcsoftMenu.Laneclear.addUseW();
            xcsoftMenu.Laneclear.addUseE();
            xcsoftMenu.Laneclear.addItem("Use Hydra", true);
            xcsoftMenu.Laneclear.addifMana();


            xcsoftMenu.Jungleclear.addUseQ();
            xcsoftMenu.Jungleclear.addUseW();
            xcsoftMenu.Jungleclear.addUseE();
            xcsoftMenu.Jungleclear.addItem("Use Hydra", true);
            xcsoftMenu.Jungleclear.addifMana();

            xcsoftMenu.Misc.addUseKillsteal();
            //xcsoftMenu.Misc.addUseAntiGapcloser();
            //xcsoftMenu.Misc.addUseInterrupter();
            xcsoftMenu.Drawings.addWrange();
            xcsoftMenu.Drawings.addErange();
            xcsoftMenu.Drawings.addRrange();
            xcsoftMenu.Drawings.addItem("Q Timer", new Circle(true, Color.LightGreen));
            xcsoftMenu.Drawings.addItem("E Timer", new Circle(true, Color.LightGreen));
            xcsoftMenu.Drawings.addItem("R Timer", new Circle(true, Color.LightGreen));
			
			xcsoftMenu.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            Orbwalking.OnAttack += Orbwalking_OnAttack;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            //AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            //Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
			
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
            if (xcsoftMenu.Misc.UseKillsteal)
                Killsteal();
            #endregion
			
			setSmiteSlot();
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawW = xcsoftMenu.Drawings.DrawWRange;
            var drawE = xcsoftMenu.Drawings.DrawERange;
            var drawR = xcsoftMenu.Drawings.DrawRRange;
			var drawQTimer = xcsoftMenu.Drawings.getCircleValue("Q Timer");
			var drawETimer = xcsoftMenu.Drawings.getCircleValue("E Timer");
			var drawRTimer = xcsoftMenu.Drawings.getCircleValue("R Timer");

            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);

            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);

            var pos_temp = Drawing.WorldToScreen(Player.Position);

            if (drawQTimer.Active && getQBuffDuration > 0)
                Drawing.DrawText(pos_temp[0], pos_temp[1], drawQTimer.Color, "Q: " + getQBuffDuration.ToString("0.00"));

            if (drawETimer.Active && getEBuffDuration > 0)
                Drawing.DrawText(pos_temp[0], pos_temp[1], drawETimer.Color, "E: " + getEBuffDuration.ToString("0.00"));

            if (drawRTimer.Active && getRBuffDuration > 0)
                Drawing.DrawText(pos_temp[0], pos_temp[1], drawRTimer.Color, "R: " + getRBuffDuration.ToString("0.00"));
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
		
        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!xcsoftMenu.Misc.UseAntiGapcloser || Player.IsDead)
                return;


        }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!xcsoftMenu.Misc.UseInterrupter || Player.IsDead)
                return;


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
                if (xcsoftMenu.Combo.UseQ && Q.IsReady()
                    && HeroManager.Enemies.Any(x => Orbwalking.InAutoAttackRange(x)))
                    Q.Cast();
				
				if (xcsoftMenu.Combo.getBoolValue("Use Hydra")
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
                if (xcsoftMenu.Combo.UseQ && Q.IsReady()
                    && HeroManager.Enemies.Any(x => Orbwalking.InAutoAttackRange(x)))
                    Q.Cast();					
				
				if (xcsoftMenu.Combo.getBoolValue("Use Hydra")
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
            if (xcsoftMenu.Combo.UseE && E.IsReady())
            {
                E.CastOnBestTarget();
            }
			
            if (xcsoftMenu.Harass.UseW && W.IsReady())
            {
               
                var wTarget = TargetSelector.GetTarget(W.Range, W.DamageType, true);

        
                if (wTarget != null && !Player.IsDashing())
                    W.Cast(wTarget);       
            }

            if (xcsoftMenu.Combo.UseR && R.IsReady()
			&& HeroManager.Enemies.Any(x => Orbwalking.InAutoAttackRange(x)))
            {
                R.Cast();
            }
				
        }

        static void Harass()
        {
            if (!(xcsoftFunc.getManaPercent(Player) > xcsoftMenu.Harass.ifMana))
                return;
				
            if (xcsoftMenu.Harass.UseW && W.IsReady())
            {
               
                var wTarget = TargetSelector.GetTarget(W.Range, W.DamageType, true);

        
                if (wTarget != null && !Player.IsDashing())
                    W.Cast(wTarget);       
            }

        }
		
        static void AALaneclear()
        {
            if (!(xcsoftFunc.getManaPercent(Player) > xcsoftMenu.Laneclear.ifMana))
                return;

				var Minions = MinionManager.GetMinions(E.Range, MinionTypes.All, MinionTeam.Enemy);

				if (Minions.Count <= 0)
                return;
				
                if (xcsoftMenu.Laneclear.UseQ && Q.IsReady())
                    Q.Cast();
					
				if (xcsoftMenu.Laneclear.getBoolValue("Use Hydra"))
				{
					if(tiamatItem.IsReady())
						tiamatItem.Cast();
					else if(hydraItem.IsReady())
						hydraItem.Cast();
				}
        }

        static void AAJungleclear()
        {
            if (!(xcsoftFunc.getManaPercent(Player) > xcsoftMenu.Jungleclear.ifMana))
                return;

            var Mobs = MinionManager.GetMinions(E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;
				
            if (xcsoftMenu.Jungleclear.UseQ && Q.IsReady())
                Q.Cast();
					
			if (xcsoftMenu.Jungleclear.getBoolValue("Use Hydra"))
			{
			    if(tiamatItem.IsReady())
			        tiamatItem.Cast();
				else if(hydraItem.IsReady())
				    hydraItem.Cast();
			}			
        }


        static void Laneclear()
        {
            if (!(xcsoftFunc.getManaPercent(Player) > xcsoftMenu.Laneclear.ifMana))
                return;

            var Minions = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;
				
				
            if (xcsoftMenu.Laneclear.UseW && W.IsReady())
                W.Cast(Minions[0]);

            if (xcsoftMenu.Laneclear.UseE && E.IsReady())
                E.Cast(Minions[0]);
        }

        static void Jungleclear()
        {
            if (!(xcsoftFunc.getManaPercent(Player) > xcsoftMenu.Jungleclear.ifMana))
                return;

            var Mobs = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (xcsoftMenu.Jungleclear.UseW && W.IsReady())
                W.Cast(Mobs[0]);

            if (xcsoftMenu.Jungleclear.UseE && E.IsReady())
                E.Cast(Mobs[0]);
        }

        static void Killsteal()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (W.CanCast(target) && xcsoftFunc.isKillable(target, W))
                    W.Cast(target);
            }
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage(enemy);

            if (W.IsReady())
                damage += W.GetDamage(enemy);

            if (R.IsReady())
                damage += R.GetDamage(enemy);

//            if(!Player.IsWindingUp)
//                damage += (float)Player.GetAutoAttackDamage(enemy, true);

            if (E.IsReady())
                damage = damage * 3 * E.Level;

            return damage;
        }
    }
}
