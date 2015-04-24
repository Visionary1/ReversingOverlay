using System;
using System.Linq;
using System.Collections.Generic;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One
{
    class AIO_Func
    {
        internal static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}}
        static float SWDuration { get { var buff = AIO_Func.getBuffInstance(ObjectManager.Player, "MasterySpellWeaving"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }

        internal static float getHealthPercent(Obj_AI_Base unit)
        {
            return unit.Health / unit.MaxHealth * 100;
        }

        internal static float getManaPercent(Obj_AI_Base unit)
        {
            return unit.Mana / unit.MaxMana * 100;
        }

        internal static List<Obj_AI_Base> getCollisionMinions(Obj_AI_Hero source, SharpDX.Vector3 targetPos, float predDelay, float predWidth, float predSpeed)
        {
            var input = new PredictionInput
            {
                Unit = source,
                Radius = predWidth,
                Delay = predDelay,
                Speed = predSpeed,
            };

            input.CollisionObjects[0] = CollisionableObjects.Minions;

            return Collision.GetCollision(new List<SharpDX.Vector3> { targetPos }, input).OrderBy(obj => obj.Distance(source, false)).ToList();
        }

        internal static BuffInstance getBuffInstance(Obj_AI_Base target, string buffName)
        {
            return target.Buffs.Find(x => x.Name == buffName && x.IsValidBuff());
        }

        internal static BuffInstance getBuffInstance(Obj_AI_Base target, string buffName, Obj_AI_Base buffCaster)
        {
            return target.Buffs.Find(x => x.Name == buffName && x.Caster.NetworkId == buffCaster.NetworkId && x.IsValidBuff());
        }

        internal static bool isKillable(Obj_AI_Base target, float damage)
        {
            return target.Health + target.HPRegenRate <= damage;
        }

        internal static bool isKillable(Obj_AI_Base target, Spell spell, int stage = 0)
        {
            return target.Health + (target.HPRegenRate/2) <= spell.GetDamage(target, stage);
        }

        internal static void sendDebugMsg(string message, bool printchat = true, string tag = "AIO_DebugMsg: ")
        {
            if (printchat)
                Game.PrintChat(tag + message);

            Console.WriteLine(tag + message);
        }

        internal static bool anyoneValidInRange(float range)
        {
            return HeroManager.Enemies.Any(x => x.IsValidTarget(range));
        }

        internal static String colorChat(System.Drawing.Color color, String text) 
        { 
            return "<font color = \"" + colorToHex(color) + "\">" + text + "</font>"; 
        }

        internal static String colorToHex(System.Drawing.Color c)
        { 
            return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2"); 
        }
		
		internal static void CCast(Spell spell, Obj_AI_Base target) //for Circular spells
		{
			if(spell.Type == SkillshotType.SkillshotCircle || spell.Type == SkillshotType.SkillshotCone) // Cone 스킬은 임시로
			{
				if(spell != null && target !=null)
				{
					var pred = spell.GetPrediction(target, true);
					SharpDX.Vector2 castVec = (pred.UnitPosition.To2D() + target.Position.To2D()) / 2 ;

					if (target.IsValidTarget(spell.Range))
					{
						if(target.MoveSpeed*spell.Delay <= spell.Width*2/3)
							spell.Cast(target.Position);
						else if(pred.Hitchance >= AIO_Menu.Champion.Misc.SelectedHitchance)
						{
							if(target.MoveSpeed*spell.Delay <= spell.Width*4/3)
							spell.Cast(castVec);
							else
							spell.Cast(pred.CastPosition);
						}
					}
				}
				else
					sendDebugMsg(spell.ToString()+" can't cast on"+target.ToString()+". Debug needed",true);
			}
			else
			sendDebugMsg("It is not circular skill. Debug needed");
		}
		
		internal static void LCast(Spell spell, Obj_AI_Base target, float alpha, float colmini) //for Linar spells  사용예시 AIO_Func.LCast(Q,Qtarget,50,0)  
		{							//        AIO_Func.LCast(E,Etarget,Menu.Item("Misc.Etg").GetValue<Slider>().Value,float.MaxValue); <- 이런식으로 사용.
			if(spell.Type != SkillshotType.SkillshotLine)
			sendDebugMsg("It is not linar skill. Debug needed");
			else
			{
				if(spell != null && target !=null)
				{
					var pred = spell.GetPrediction(target, true);
					var collision = spell.GetCollision(ObjectManager.Player.ServerPosition.To2D(), new List<SharpDX.Vector2> { pred.CastPosition.To2D() });
					var minioncol = collision.Where(x => !(x is Obj_AI_Hero)).Count(x => x.IsMinion);

					if (target.IsValidTarget(spell.Range - target.MoveSpeed * (spell.Delay + ObjectManager.Player.Distance(target.Position) / spell.Speed) + alpha) && minioncol <= colmini && pred.Hitchance >= AIO_Menu.Champion.Misc.SelectedHitchance)
					{
						spell.Cast(pred.CastPosition);
					}
				}
				else
					sendDebugMsg(spell.ToString()+" can't cast on"+target.ToString()+". Debug needed",true);
			}
		}

		
		internal static void LH(Spell spell, float ALPHA) // For Last hit with skill for farming 사용법은 매우 간단. AIO_Func.LH(Q,0) or AIO_Func(Q,float.MaxValue) 이런식으로. 럭스나 베이가같이 타겟이 둘 가능할 경우엔 AIO_Func.LH(Q,1) 이런식.
		{
			if(spell == null)
			return;
				var _m = MinionManager.GetMinions(spell.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth).FirstOrDefault(m => isKillable(m,spell,0) && HealthPrediction.GetHealthPrediction(m, (int)(ObjectManager.Player.Distance(m, false) / spell.Speed), (int)(spell.Delay * 1000 + Game.Ping / 2)) > 0);
				if(spell.Type == SkillshotType.SkillshotLine) // 선형 스킬일경우
                LCast(spell,_m,50,ALPHA);
				else if(spell.Type == SkillshotType.SkillshotCircle) // 원형 스킬일경우
				CCast(spell,_m);
				else if(spell.Type == SkillshotType.SkillshotCone) //원뿔 스킬
				spell.Cast(_m);
				else if(!spell.IsSkillshot)
				spell.Cast(_m);
				
		}
		

        internal static void MotionCancel()
        {
            Game.Say("/d");
        }
		
		internal static bool AfterAttack()
		{
			return SWDuration > 4.85 && utility.Activator.AfterAttack.AIO;
		}
		
		internal static void AASkill(Spell spell)
		{
			if(spell.IsReady())
			utility.Activator.AfterAttack.SkillCasted = false;
			else
			utility.Activator.AfterAttack.SkillCasted = true;
		}
		
		internal static void AALcJc(Spell spell) //지금으로선 새 방식으로 메뉴 만든 경우에만 사용가능. AALaneclear AAJungleclear 대체
		{
				var Minions = MinionManager.GetMinions(ObjectManager.Player.AttackRange, MinionTypes.All, MinionTeam.Enemy);
				var Mobs = MinionManager.GetMinions(ObjectManager.Player.AttackRange, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
				if((Menu.Item("Laneclear.Use " + spell.Slot.ToString(), true).GetValue<bool>() || Menu.Item("LcUse" + spell.Slot.ToString(), true).GetValue<bool>()) && Minions.Count >= 1 && getManaPercent(ObjectManager.Player) > AIO_Menu.Champion.Laneclear.IfMana)
				{
					if(!spell.IsSkillshot)
					spell.Cast(Minions[0]);
					else
					spell.Cast();
				}
				if((Menu.Item("Jungleclear.Use " + spell.Slot.ToString(), true).GetValue<bool>() || Menu.Item("JcUse" + spell.Slot.ToString(), true).GetValue<bool>()) && Mobs.Count >= 1 && getManaPercent(ObjectManager.Player) > AIO_Menu.Champion.Jungleclear.IfMana)
				{
					if(!spell.IsSkillshot)
					spell.Cast(Mobs[0]);
					else
					spell.Cast();
				}
		}
		
    }
}

