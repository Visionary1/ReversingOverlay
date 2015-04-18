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
        static Menu Menu { get { return ALL_IN_ONE_Menu.MainMenu_Manual; } } //메뉴얼 오브워커 넣기는 했지만. 음.. 
        static Orbwalking.Orbwalker Orbwalker { get { return ALL_IN_ONE_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Items.Item tiamatItem, hydraItem; //히드라 평캔을 위함

        static Spell Q, W, E, R;
        static List<Items.Item> itemsList = new List<Items.Item>(); //척후병 샤브르
		static Spell Smite;
		static SpellSlot smiteSlot = SpellSlot.Unknown;
		static Items.Item s0, s1, s2, s3, s4;
        static float smrange = 700f;
		
        static float getQBuffDuration { get { var buff = ALL_IN_ONE_Func.getBuffInstance(Player, "TalonNoxianDiplomacyBuff"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }
        static float getEBuffDuration { get { var buff = ALL_IN_ONE_Func.getBuffInstance(Player, "TalonDamageAmp"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }
        static float getRBuffDuration { get { var buff = ALL_IN_ONE_Func.getBuffInstance(Player, "TalonDisappear"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }

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
			
            ALL_IN_ONE_Menu.Champion.Combo.addUseQ();
            ALL_IN_ONE_Menu.Champion.Combo.addUseW();
            ALL_IN_ONE_Menu.Champion.Combo.addUseE();
            ALL_IN_ONE_Menu.Champion.Combo.addUseR();
            ALL_IN_ONE_Menu.Champion.Combo.addItem("Use Hydra", true);

            ALL_IN_ONE_Menu.Champion.Harass.addUseW();
            ALL_IN_ONE_Menu.Champion.Harass.addIfMana();

            ALL_IN_ONE_Menu.Champion.Laneclear.addUseQ();
            ALL_IN_ONE_Menu.Champion.Laneclear.addUseW();
            ALL_IN_ONE_Menu.Champion.Laneclear.addUseE();
            ALL_IN_ONE_Menu.Champion.Laneclear.addItem("Use Hydra", true);
            ALL_IN_ONE_Menu.Champion.Laneclear.addIfMana();


            ALL_IN_ONE_Menu.Champion.Jungleclear.addUseQ();
            ALL_IN_ONE_Menu.Champion.Jungleclear.addUseW();
            ALL_IN_ONE_Menu.Champion.Jungleclear.addUseE();
            ALL_IN_ONE_Menu.Champion.Jungleclear.addItem("Use Hydra", true);
            ALL_IN_ONE_Menu.Champion.Jungleclear.addIfMana();

            ALL_IN_ONE_Menu.Champion.Misc.addUseKillsteal();
            //ALL_IN_ONE_Menu.Champion.Misc.addUseAntiGapcloser();
            //ALL_IN_ONE_Menu.Champion.Misc.addUseInterrupter();
            ALL_IN_ONE_Menu.Champion.Drawings.addQRange();
            ALL_IN_ONE_Menu.Champion.Drawings.addERange();
            ALL_IN_ONE_Menu.Champion.Drawings.addRRange();
            ALL_IN_ONE_Menu.Champion.Drawings.addItem("Q Timer", new Circle(true, Color.LightGreen));
            ALL_IN_ONE_Menu.Champion.Drawings.addItem("E Timer", new Circle(true, Color.LightGreen));
            ALL_IN_ONE_Menu.Champion.Drawings.addItem("R Timer", new Circle(true, Color.LightGreen));
			
			ALL_IN_ONE_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

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
            if (ALL_IN_ONE_Menu.Champion.Misc.UseKillsteal)
                Killsteal();
            #endregion
			
			setSmiteSlot();
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawW = ALL_IN_ONE_Menu.Champion.Drawings.WRange;
            var drawE = ALL_IN_ONE_Menu.Champion.Drawings.ERange;
            var drawR = ALL_IN_ONE_Menu.Champion.Drawings.RRange;
			var drawQTimer = ALL_IN_ONE_Menu.Champion.Drawings.getCircleValue("Q Timer");
			var drawETimer = ALL_IN_ONE_Menu.Champion.Drawings.getCircleValue("E Timer");
			var drawRTimer = ALL_IN_ONE_Menu.Champion.Drawings.getCircleValue("R Timer");

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
            if (!ALL_IN_ONE_Menu.Champion.Misc.UseAntiGapcloser || Player.IsDead)
                return;


        }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!ALL_IN_ONE_Menu.Champion.Misc.UseInterrupter || Player.IsDead)
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
                if (ALL_IN_ONE_Menu.Champion.Combo.UseQ && Q.IsReady()
                    && HeroManager.Enemies.Any(x => Orbwalking.InAutoAttackRange(x)))
                    Q.Cast();
				
				if (ALL_IN_ONE_Menu.Champion.Combo.getBoolValue("Use Hydra")
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
                if (ALL_IN_ONE_Menu.Champion.Combo.UseQ && Q.IsReady()
                    && HeroManager.Enemies.Any(x => Orbwalking.InAutoAttackRange(x)))
                    Q.Cast();					
				
				if (ALL_IN_ONE_Menu.Champion.Combo.getBoolValue("Use Hydra")
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
            if (ALL_IN_ONE_Menu.Champion.Combo.UseE && E.IsReady())
            {
                E.CastOnBestTarget();
            }
			
            if (ALL_IN_ONE_Menu.Champion.Harass.UseW && W.IsReady())
            {
               
                var wTarget = TargetSelector.GetTarget(W.Range, W.DamageType, true);

        
                if (wTarget != null && !Player.IsDashing())
                    W.Cast(wTarget);       
            }

            if (ALL_IN_ONE_Menu.Champion.Combo.UseR && R.IsReady()
			&& HeroManager.Enemies.Any(x => Orbwalking.InAutoAttackRange(x)))
            {
                R.Cast();
            }
				
        }

        static void Harass()
        {
            if (!(ALL_IN_ONE_Func.getManaPercent(Player) > ALL_IN_ONE_Menu.Champion.Harass.IfMana))
                return;
				
            if (ALL_IN_ONE_Menu.Champion.Harass.UseW && W.IsReady())
            {
               
                var wTarget = TargetSelector.GetTarget(W.Range, W.DamageType, true);

        
                if (wTarget != null && !Player.IsDashing())
                    W.Cast(wTarget);       
            }

        }
		
        static void AALaneclear()
        {
            if (!(ALL_IN_ONE_Func.getManaPercent(Player) > ALL_IN_ONE_Menu.Champion.Laneclear.IfMana))
                return;

				var Minions = MinionManager.GetMinions(E.Range, MinionTypes.All, MinionTeam.Enemy);

				if (Minions.Count <= 0)
                return;
				
                if (ALL_IN_ONE_Menu.Champion.Laneclear.UseQ && Q.IsReady())
                    Q.Cast();
					
				if (ALL_IN_ONE_Menu.Champion.Laneclear.getBoolValue("Use Hydra"))
				{
					if(tiamatItem.IsReady())
						tiamatItem.Cast();
					else if(hydraItem.IsReady())
						hydraItem.Cast();
				}
        }

        static void AAJungleclear()
        {
            if (!(ALL_IN_ONE_Func.getManaPercent(Player) > ALL_IN_ONE_Menu.Champion.Jungleclear.IfMana))
                return;

            var Mobs = MinionManager.GetMinions(E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;
				
            if (ALL_IN_ONE_Menu.Champion.Jungleclear.UseQ && Q.IsReady())
                Q.Cast();
					
			if (ALL_IN_ONE_Menu.Champion.Jungleclear.getBoolValue("Use Hydra"))
			{
			    if(tiamatItem.IsReady())
			        tiamatItem.Cast();
				else if(hydraItem.IsReady())
				    hydraItem.Cast();
			}			
        }


        static void Laneclear()
        {
            if (!(ALL_IN_ONE_Func.getManaPercent(Player) > ALL_IN_ONE_Menu.Champion.Laneclear.IfMana))
                return;

            var Minions = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;
				
				
            if (ALL_IN_ONE_Menu.Champion.Laneclear.UseW && W.IsReady())
                W.Cast(Minions[0]);

            if (ALL_IN_ONE_Menu.Champion.Laneclear.UseE && E.IsReady())
                E.Cast(Minions[0]);
        }

        static void Jungleclear()
        {
            if (!(ALL_IN_ONE_Func.getManaPercent(Player) > ALL_IN_ONE_Menu.Champion.Jungleclear.IfMana))
                return;

            var Mobs = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (ALL_IN_ONE_Menu.Champion.Jungleclear.UseW && W.IsReady())
                W.Cast(Mobs[0]);

            if (ALL_IN_ONE_Menu.Champion.Jungleclear.UseE && E.IsReady())
                E.Cast(Mobs[0]);
        }

        static void Killsteal()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (W.CanCast(target) && ALL_IN_ONE_Func.isKillable(target, W))
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
                damage = damage * (1+0.03f * E.Level);

            return damage;
        }
    }
}
