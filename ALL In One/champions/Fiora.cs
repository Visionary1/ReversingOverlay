using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.IO;
using SharpDX;

namespace ALL_In_One.champions
{
    class Fiora // RL244
    {
        static Menu Menu { get { return AIO_Menu.MainMenu_Manual; } }
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Spell Q, W, E, R;
        static List<Items.Item> itemsList = new List<Items.Item>(); //척후병 샤브르 //RS
		static Spell Smite; //RS
		static SpellSlot smiteSlot = SpellSlot.Unknown; //RS
		static Items.Item s0, s1, s2, s3, s4; //RS
        static float smrange = 700f; //RS
		
        static float getQBuffDuration { get { var buff = AIO_Func.getBuffInstance(Player, "fioraqcd"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }
        static float getWBuffDuration { get { var buff = AIO_Func.getBuffInstance(Player, "FioraRiposte"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }
        static float getEBuffDuration { get { var buff = AIO_Func.getBuffInstance(Player, "FioraFlurry"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 600f, TargetSelector.DamageType.Physical);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 400f, TargetSelector.DamageType.Physical);

            Q.SetTargetted(0.25f, float.MaxValue);

            Menu.SubMenu("Champion").SubMenu("Combo").AddItem(new MenuItem("CbUseQ", "Use Q", true).SetValue(true));
            Menu.SubMenu("Champion").SubMenu("Combo").AddItem(new MenuItem("CbUseQD", "Q Distance", true).SetValue(new Slider(150, 0, 600)));
            Menu.SubMenu("Champion").SubMenu("Combo").AddItem(new MenuItem("CbUseW", "Use W", true).SetValue(true));
            Menu.SubMenu("Champion").SubMenu("Combo").AddItem(new MenuItem("CbUseE", "Use E", true).SetValue(true));
            Menu.SubMenu("Champion").SubMenu("Combo").AddItem(new MenuItem("CbUseR", "Use R", true).SetValue(true));

            Menu.SubMenu("Champion").SubMenu("Harass").AddItem(new MenuItem("HrsUseE", "Use E", true).SetValue(true));
            Menu.SubMenu("Champion").SubMenu("Harass").AddItem(new MenuItem("HrsMana", "if Mana % >", true).SetValue(new Slider(60, 0, 100)));

            Menu.SubMenu("Champion").SubMenu("Laneclear").AddItem(new MenuItem("LcUseE", "Use E", true).SetValue(true));
            Menu.SubMenu("Champion").SubMenu("Laneclear").AddItem(new MenuItem("LcMana", "if Mana % >", true).SetValue(new Slider(60, 0, 100)));

            Menu.SubMenu("Champion").SubMenu("Jungleclear").AddItem(new MenuItem("JcUseQ", "Use Q", true).SetValue(true));
            Menu.SubMenu("Champion").SubMenu("Jungleclear").AddItem(new MenuItem("JcUseW", "Use W", true).SetValue(true));
            Menu.SubMenu("Champion").SubMenu("Jungleclear").AddItem(new MenuItem("JcUseE", "Use E", true).SetValue(true));
            Menu.SubMenu("Champion").SubMenu("Jungleclear").AddItem(new MenuItem("JcMana", "if Mana % >", true).SetValue(new Slider(20, 0, 100)));

            Menu.SubMenu("Champion").SubMenu("Misc").AddItem(new MenuItem("credit", "Made By RL244", true));
            Menu.SubMenu("Champion").SubMenu("Misc").AddItem(new MenuItem("miscKsQ", "Use Q KillSteal", true).SetValue(true));
            Menu.SubMenu("Champion").SubMenu("Misc").AddItem(new MenuItem("miscKsR", "Use R KillSteal", true).SetValue(true));
			
            Menu.SubMenu("Champion").SubMenu("Drawings").AddItem(new MenuItem("drawQ", "Q Range", true).SetValue(new Circle(true, Color.Red)));
            Menu.SubMenu("Champion").SubMenu("Drawings").AddItem(new MenuItem("drawR", "R Range", true).SetValue(new Circle(true, Color.Blue)));
            Menu.SubMenu("Champion").SubMenu("Drawings").AddItem(new MenuItem("drawQTimer", "Q Timer", true).SetValue(new Circle(true, Color.LightGreen)));
            Menu.SubMenu("Champion").SubMenu("Drawings").AddItem(new MenuItem("drawWTimer", "W Timer", true).SetValue(new Circle(true, Color.Black)));
            Menu.SubMenu("Champion").SubMenu("Drawings").AddItem(new MenuItem("drawETimer", "E Timer", true).SetValue(new Circle(true, Color.Red)));

			AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            Orbwalking.OnAttack += Orbwalking_OnAttack;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
			
	    InitializeItems(); //RS
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
            if (Menu.Item("miscKsQ", true).GetValue<bool>())
                KillstealQ();
            if (Menu.Item("miscKsR", true).GetValue<bool>())
                KillstealR();
            #endregion
			
		setSmiteSlot(); //RS
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
		
        static void setSmiteSlot() //RS
        {
            foreach (var spell in ObjectManager.Player.Spellbook.Spells.Where(spell => String.Equals(spell.Name, "s5_summonersmiteduel", StringComparison.CurrentCultureIgnoreCase)))
            {
                smiteSlot = spell.Slot;
                Smite = new Spell(smiteSlot, smrange);
                return;
            }
        }
		
        static bool CheckInv() //RS
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
		
        static void InitializeItems() //RS
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

	static readonly string[] MinionNames =
	{
	"SRU_Blue", "SRU_Gromp", "SRU_Murkwolf", "SRU_Razorbeak", "SRU_Red", "SRU_Krug", "SRU_Dragon", "SRU_BaronSpawn"
	};
	
	static Obj_AI_Minion Chase(Vector3 pos)
	{
		var minions =
		ObjectManager.Get<Obj_AI_Minion>()
		.Where(minion => minion.IsValid && minion.IsEnemy && !minion.IsDead
		&& Player.Distance(minion.Position) <= 600 && Player.Distance(minion.Position) > 400);
		var objAiMinions = minions as Obj_AI_Minion[] ?? minions.ToArray();
		Obj_AI_Minion sMinion = objAiMinions.FirstOrDefault();
		double? nearest = null;
		foreach (Obj_AI_Minion minion in objAiMinions)
		{
			double distance = Vector3.Distance(pos, minion.Position);
			if (nearest == null || nearest > distance)
			{
				nearest = distance;
				sMinion = minion;
			}
		}
		return sMinion;
	}
		
        static readonly string[] Attacks = { "jarvanivcataclysmattack", "monkeykingdoubleattack", "shyvanadoubleattack", "shyvanadoubleattackdragon", "caitlynheadshotmissile", "frostarrow", "garenslash2", "kennenmegaproc", "masteryidoublestrike", "quinnwenhanced", "renektonexecute", "renektonsuperexecute", "rengarnewpassivebuffdash", "trundleq", "xenzhaothrust", "viktorqbuff", "xenzhaothrust2", "xenzhaothrust3" };
        static readonly string[] NoAttacks = { "zyragraspingplantattack", "zyragraspingplantattack2", "zyragraspingplantattackfire", "zyragraspingplantattack2fire" };
        static readonly string[] OHSP = { "Parley", "EzrealMysticShot"};
        static readonly string[] AttackResets = { "dariusnoxiantacticsonh", "fioraflurry", "garenq", "hecarimrapidslash", "jaxempowertwo", "jaycehypercharge", "leonashieldofdaybreak", "monkeykingdoubleattack", "mordekaisermaceofspades", "nasusq", "nautiluspiercinggaze", "netherblade", "parley", "poppydevastatingblow", "powerfist", "renektonpreexecute", "rengarq", "shyvanadoubleattack", "sivirw", "takedown", "talonnoxiandiplomacy", "trundletrollsmash", "vaynetumble", "vie", "volibearq", "xenzhaocombotarget", "yorickspectral" };

        static bool IsOnHit(string name)
        {
            return !(name.ToLower().Contains("tower")) &&!(name.ToLower().Contains("turret")) && !(name.ToLower().Contains("mini")) && (name.ToLower().Contains("attack")) && !NoAttacks.Contains(name.ToLower()) ||
            Attacks.Contains(name.ToLower()) && AttackResets.Contains(name.ToLower()) && OHSP.Contains(name.ToLower());
        }

        static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (IsOnHit(args.SData.Name) && args.Target.IsMe && Player.Distance(args.End) < 150)
		{

			W.Cast();
		}

        }

	static void Orbwalking_OnAttack(AttackableUnit unit, AttackableUnit target) 
	{
		var Target = (Obj_AI_Base)target;
		if (!unit.IsMe || Target == null)
                return;
				
            	if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo) //RS
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
				
				if(Minions.Count >= 1)
				AALaneclear();
				if(Mobs.Count >= 1)
				AAJungleclear();
			}
			
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
			{	
                if (Menu.Item("HrsUseE", true).GetValue<bool>() && E.IsReady()
                    && HeroManager.Enemies.Any(x => Orbwalking.InAutoAttackRange(x))
				&& utility.Activator.AfterAttack.ALLCancleItemsAreCasted)
                    E.Cast();

			}
				
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                if (Menu.Item("CbUseE", true).GetValue<bool>() && E.IsReady()
                    && HeroManager.Enemies.Any(x => Orbwalking.InAutoAttackRange(x))
					&& utility.Activator.AfterAttack.ALLCancleItemsAreCasted)
                    E.Cast();
					
				foreach (var rtarget in HeroManager.Enemies.OrderByDescending(x => x.Health))
				{
				if (Menu.Item("CbUseR", true).GetValue<bool>() && R.IsReady()
				&& utility.Activator.AfterAttack.ALLCancleItemsAreCasted && !E.IsReady()
				&& HeroManager.Enemies.Any(x => x.IsValidTarget(R.Range)))
				R.Cast(rtarget);
				}
				
			}
        }

        static void Combo()
        {
            if (Menu.Item("CbUseQ", true).GetValue<bool>() && Q.IsReady()) 
                {
			var qd = Menu.Item("CbUseQD", true).GetValue<Slider>().Value;
			var qTarget = TargetSelector.GetTarget(Q.Range, Q.DamageType);
			var fqTarget = TargetSelector.GetTarget(Q.Range * 2, Q.DamageType);
//				var fminion = ObjectManager.Get<Obj_AI_Minion>().OrderBy(t => t.Distance(fqTarget.Position)).
//				Where(t => !t.IsAlly && Player.Distance(t.Position) <= 600 && Player.Distance(t.Position) >= 300 && fqTarget.Distance(t.Position) <= 600).First();
			var mchase = Chase(fqTarget.Position);
			
			
			if(qTarget.Distance(Player.ServerPosition) >= qd || getQBuffDuration < 1)
				Q.Cast(qTarget);
				
			if(fqTarget.Distance(Player.ServerPosition) > 600) //Chasing Enemy
				Q.Cast(mchase);
		}
				
        }

        static void Harass()
        {
            if (!(AIO_Func.getManaPercent(Player) > Menu.Item("HrsMana", true).GetValue<Slider>().Value))
                return;

        }

        static void AALaneclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > Menu.Item("LcMana", true).GetValue<Slider>().Value))
                return;

				var Minions = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy);

				if (Minions.Count <= 0)
                return;
				
                if (Menu.Item("LcUseE", true).GetValue<bool>() && E.IsReady()
                {    
				E.Cast();
				}
        }

        static void AAJungleclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > Menu.Item("JcMana", true).GetValue<Slider>().Value))
                return;

            var Mobs = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;
				
                if (Menu.Item("JcUseE", true).GetValue<bool>() && E.IsReady()
                {    
				E.Cast();
				}
        }
		
		
        static void Laneclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > Menu.Item("LcMana", true).GetValue<Slider>().Value))
                return;

            var Minions = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy);

        }

        static void Jungleclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > Menu.Item("JcMana", true).GetValue<Slider>().Value))
                return;

            var Mobs = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

				
            if (Menu.Item("JcUseW", true).GetValue<bool>() && W.IsReady())
                W.Cast();
				

            if (Menu.Item("JcUseQ", true).GetValue<bool>() && Q.IsReady())
                Q.Cast(Mobs[0]);
        }

        static void KillstealQ()
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
                if (R.CanCast(target) && AIO_Func.isKillable(target, R))
                    R.Cast(target);
            }
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage(enemy) * 2;
				
            if (R.IsReady() && Menu.Item("CbUseR", true).GetValue<bool>())
                damage += R.GetDamage(enemy);
				
            if (W.IsReady())
                damage += W.GetDamage(enemy);
				
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

            if(!Player.IsWindingUp)
                damage += (float)Player.GetAutoAttackDamage(enemy, true);

	    if(E.IsReady())
                damage += (float)Player.GetAutoAttackDamage(enemy, true) * 2;
				
            return damage;
        }
    }
}

