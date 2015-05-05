using System;
using System.Linq;
using System.Collections.Generic;

using LeagueSharp;
using LeagueSharp.Common;
using LSConsole = LeagueSharp.Console.Console;

namespace ALL_In_One
{
    static class AIO_Func
    {
        internal static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}}
        static float SWDuration { get { var buff = AIO_Func.getBuffInstance(ObjectManager.Player, "MasterySpellWeaving"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } } // Player 많이 쓰는데 괜히 ObjectManager.Player라고 하는거 너무길어요~~.
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } } // 이거 지우면 평캔 관련 다날라갑니다. 절대 지우기 ㄴ

		
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

        internal static void sendDebugMsg(string message, string tag = "[TeamProjects] ALL In One: ")
        {
            Console.WriteLine(tag + message);
            LSConsole.WriteLine(tag + message);
            
        }

        internal static bool anyoneValidInRange(float range)
        {
            return HeroManager.Enemies.Any(x => x.IsValidTarget(range));
        }
		
		internal static void CCast(Spell spell, Obj_AI_Base target) //for Circular spells
		{
			if(spell.Type == SkillshotType.SkillshotCircle || spell.Type == SkillshotType.SkillshotCone) // Cone 스킬은 임시로
			{
				if(spell != null && target !=null)
				{
					var pred = spell.GetPrediction(target, true);
					SharpDX.Vector2 castVec = (pred.UnitPosition.To2D() + target.ServerPosition.To2D()) / 2 ;

					if (target.IsValidTarget(spell.Range))
					{
						if(target.MoveSpeed*spell.Delay <= spell.Width*2/3)
							spell.Cast(target.ServerPosition);
						else if(pred.Hitchance >= AIO_Menu.Champion.Misc.SelectedHitchance)
						{
							if(target.MoveSpeed*spell.Delay <= spell.Width*6/5)
							spell.Cast(castVec);
							else
							spell.Cast(pred.CastPosition);
						}
					}
				}
				else
					sendDebugMsg(spell.ToString()+" can't cast on"+target.ToString()+". Debug needed");
			}
			else
			sendDebugMsg("It is not circular skill. Debug needed");
		}
		
		internal static void LCast(Spell spell, Obj_AI_Base target, float alpha = 50f, float colmini = float.MaxValue) //for Linar spells  사용예시 AIO_Func.LCast(Q,Qtarget,50,0)  
		{							//        AIO_Func.LCast(E,Etarget,Menu.Item("Misc.Etg").GetValue<Slider>().Value,float.MaxValue); <- 이런식으로 사용.
			if(spell.Type != SkillshotType.SkillshotLine)
			sendDebugMsg("It is not linar skill. Debug needed");
			else
			{
				if(spell != null && target !=null)
				{
					var pred = spell.GetPrediction(target, true);
					var collision = spell.GetCollision(Player.ServerPosition.To2D(), new List<SharpDX.Vector2> { pred.CastPosition.To2D() });
					var minioncol = collision.Where(x => !(x is Obj_AI_Hero)).Count(x => x.IsMinion);

					if (target.IsValidTarget(spell.Range - target.MoveSpeed * (spell.Delay + Player.Distance(target.ServerPosition) / spell.Speed) + alpha) && minioncol <= colmini && pred.Hitchance >= AIO_Menu.Champion.Misc.SelectedHitchance)
					{
						spell.Cast(pred.CastPosition);
					}
				}
				else
					sendDebugMsg(spell.ToString()+" can't cast on"+target.ToString()+". Debug needed");
			}
		}

		
		internal static void LH(Spell spell, float ALPHA = 0f) // For Last hit with skill for farming 사용법은 매우 간단. AIO_Func.LH(Q,0) or AIO_Func(Q,float.MaxValue) 이런식으로. 럭스나 베이가같이 타겟이 둘 가능할 경우엔 AIO_Func.LH(Q,1) 이런식.
		{
			if(spell == null || !spell.IsReady())
			return;

				var _m = MinionManager.GetMinions(spell.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth).FirstOrDefault(m => isKillable(m,spell,0) && HealthPrediction.GetHealthPrediction(m, (int)(Player.Distance(m, false) / spell.Speed), (int)(spell.Delay * 1000 + Game.Ping / 2)) > 0);
				if(spell.Type == SkillshotType.SkillshotLine) // 선형 스킬일경우
                LCast(spell,_m,50f,ALPHA);
				else if(spell.Type == SkillshotType.SkillshotCircle) // 원형 스킬일경우
				CCast(spell,_m);
				else if(spell.Type == SkillshotType.SkillshotCone) //원뿔 스킬
				spell.Cast(_m);
				else
				spell.Cast(_m);
				
		}
		

        internal static void MotionCancel()
        {
            Game.Say("/d");
        }
		
		internal static bool AfterAttack()
		{
			return SWDuration > 4.95 && utility.Activator.AfterAttack.AIO;
		}
		
		internal static void AASkill(Spell spell)
		{
			if(spell.IsReady())
			utility.Activator.AfterAttack.SkillCasted = false;
			else
			utility.Activator.AfterAttack.SkillCasted = true;
		}
		
		internal static void AALcJc(Spell spell, float ExtraTargetDistance = 150f,float ALPHA = float.MaxValue, float Cost = 1f) //지금으로선 새 방식으로 메뉴 만든 경우에만 사용가능. AALaneclear AAJungleclear 대체
		{// 아주 편하게 평캔 Lc, Jc를 구현할수 있습니다(그것도 분리해서!!). 그냥 AIO_Func.AALcJc(Q); 이렇게 쓰세요. 선형 스킬일 경우 세부 설정을 원할 경우 AIO_Func.AALcJc(E,ED,0f); 이런식으로 쓰세요.
			if (Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.LaneClear)
			return;
			
			var Minions = MinionManager.GetMinions(Player.AttackRange/2+200f, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth);
			var Mobs = MinionManager.GetMinions(Player.AttackRange/2+200f, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
			
			if((Menu.Item("Laneclear.Use " + spell.Slot.ToString(), true).GetValue<bool>() || Menu.Item("LcUse" + spell.Slot.ToString(), true).GetValue<bool>())
			&& spell.IsReady() && utility.Activator.AfterAttack.ALLCancelItemsAreCasted && (getManaPercent(Player) > AIO_Menu.Champion.Laneclear.IfMana || Cost != 1f))
			{
				if (Minions.Count > 0)
				{
					if(spell.IsSkillshot)
					{
					if(spell.Type == SkillshotType.SkillshotLine)
					LCast(spell,Minions[0],ExtraTargetDistance,ALPHA);
					else if(spell.Type == SkillshotType.SkillshotCircle)
					CCast(spell,Minions[0]);
					else if(spell.Type == SkillshotType.SkillshotCone)
					spell.Cast(Minions[0]);
					}
					else if(!spell.IsSkillshot)
					spell.Cast(Minions[0]);
					else
					spell.Cast();
				}
			}
			
			if((Menu.Item("Jungleclear.Use " + spell.Slot.ToString(), true).GetValue<bool>() || Menu.Item("JcUse" + spell.Slot.ToString(), true).GetValue<bool>())
			&& spell.IsReady() && utility.Activator.AfterAttack.ALLCancelItemsAreCasted && (getManaPercent(Player) > AIO_Menu.Champion.Jungleclear.IfMana || Cost != 1f))
			{
				if (Mobs.Count > 0)
				{
					if(spell.IsSkillshot)
					{
					if(spell.Type == SkillshotType.SkillshotLine)
					LCast(spell,Mobs[0],ExtraTargetDistance,ALPHA);
					else if(spell.Type == SkillshotType.SkillshotCircle)
					CCast(spell,Mobs[0]);
					else if(spell.Type == SkillshotType.SkillshotCone)
					spell.Cast(Mobs[0]);
					}
					else if(!spell.IsSkillshot)
					spell.Cast(Mobs[0]);
					else
					spell.Cast();
				}
			}
		}
		
		internal static void AACb(Spell spell, float ExtraTargetDistance = 150f,float ALPHA = float.MaxValue, float Cost = 1f) //지금으로선 새 방식으로 메뉴 만든 경우에만 사용가능.
		{ // 아주 편하게 평캔 Cb, Hrs를 구현할수 있습니다. 그냥 AIO_Func.AACb(Q); 이렇게 쓰세요. Line 스킬일 경우에만 AIO_Func.AACb(E,ED,0f) 이런식으로 쓰시면 됩니다.
			var target = TargetSelector.GetTarget(Player.AttackRange + 50,TargetSelector.DamageType.Physical, true); //
			
			if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
			{
				if((Menu.Item("Combo.Use " + spell.Slot.ToString(), true).GetValue<bool>() || Menu.Item("CbUse" + spell.Slot.ToString(), true).GetValue<bool>())
				&& spell.IsReady() && utility.Activator.AfterAttack.ALLCancelItemsAreCasted)
				{
					if(spell.IsSkillshot)
					{
						if(spell.Type == SkillshotType.SkillshotLine) // 선형 스킬일경우
						LCast(spell,target,ExtraTargetDistance,ALPHA);
						else if(spell.Type == SkillshotType.SkillshotCircle) // 원형 스킬일경우
						CCast(spell,target);
						else if(spell.Type == SkillshotType.SkillshotCone) //원뿔 스킬
						spell.Cast(target);
					}
					else if(!spell.IsSkillshot)
					spell.Cast(target);
					else
					spell.Cast();
				}
			}
			else if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
			{
				if((Menu.Item("Harass.Use " + spell.Slot.ToString(), true).GetValue<bool>() || Menu.Item("HrsUse" + spell.Slot.ToString(), true).GetValue<bool>())
				&& spell.IsReady() && utility.Activator.AfterAttack.ALLCancelItemsAreCasted && (getManaPercent(Player) > AIO_Menu.Champion.Harass.IfMana || Cost != 1f))
				{
					if(spell.IsSkillshot)
					{
						if(spell.Type == SkillshotType.SkillshotLine) // 선형 스킬일경우
						LCast(spell,target,ExtraTargetDistance,ALPHA);
						else if(spell.Type == SkillshotType.SkillshotCircle) // 원형 스킬일경우
						CCast(spell,target);
						else if(spell.Type == SkillshotType.SkillshotCone) //원뿔 스킬
						spell.Cast(target);
					}
					else if(!spell.IsSkillshot)
					spell.Cast(target);
					else
					spell.Cast();
				}
			}
		}
		
		internal static void SC(Spell spell, float ExtraTargetDistance = 150f,float ALPHA = float.MaxValue, float Cost = 1f) //
		{ // 
			var target = TargetSelector.GetTarget(spell.Range, spell.DamageType, true); //
			
			if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
			{
				if((Menu.Item("Combo.Use " + spell.Slot.ToString(), true).GetValue<bool>() || Menu.Item("CbUse" + spell.Slot.ToString(), true).GetValue<bool>())
				&& spell.IsReady())
				{
					if(spell.IsSkillshot)
					{
						if(spell.Type == SkillshotType.SkillshotLine)
						LCast(spell,target,ExtraTargetDistance,ALPHA);
						else if(spell.Type == SkillshotType.SkillshotCircle)
						CCast(spell,target);
						else if(spell.Type == SkillshotType.SkillshotCone)
						spell.Cast(target);
					}
					else
					spell.Cast(target);
				}
			}
			else if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
			{
				if((Menu.Item("Harass.Use " + spell.Slot.ToString(), true).GetValue<bool>() || Menu.Item("HrsUse" + spell.Slot.ToString(), true).GetValue<bool>())
				&& spell.IsReady() && (getManaPercent(Player) > AIO_Menu.Champion.Harass.IfMana || Cost != 1f))
				{
					if(spell.IsSkillshot)
					{
						if(spell.Type == SkillshotType.SkillshotLine)
						LCast(spell,target,ExtraTargetDistance,ALPHA);
						else if(spell.Type == SkillshotType.SkillshotCircle)
						CCast(spell,target);
						else if(spell.Type == SkillshotType.SkillshotCone)
						spell.Cast(target);
					}
					else
					spell.Cast(target);
				}
			}
			else if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
			{			
				var Minions = MinionManager.GetMinions(spell.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth);
				var Mobs = MinionManager.GetMinions(spell.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
				
				if((Menu.Item("Laneclear.Use " + spell.Slot.ToString(), true).GetValue<bool>() || Menu.Item("LcUse" + spell.Slot.ToString(), true).GetValue<bool>())
				&& spell.IsReady() && (getManaPercent(Player) > AIO_Menu.Champion.Laneclear.IfMana || Cost != 1f))
				{
					if (Minions.Count > 0)
					{
						if(spell.IsSkillshot)
						{
							if(spell.Type == SkillshotType.SkillshotLine)
							{
								if(ALPHA > 1f)
								LCast(spell,Minions[0],ExtraTargetDistance,ALPHA);
								else
								LH(spell, ALPHA);
							}
							else if(spell.Type == SkillshotType.SkillshotCircle)
							CCast(spell,Minions[0]);
							else if(spell.Type == SkillshotType.SkillshotCone)
							spell.Cast(Minions[0]);
						}
						else
						LH(spell);
					}
				}
				
				if((Menu.Item("Jungleclear.Use " + spell.Slot.ToString(), true).GetValue<bool>() || Menu.Item("JcUse" + spell.Slot.ToString(), true).GetValue<bool>())
				&& spell.IsReady() && (getManaPercent(Player) > AIO_Menu.Champion.Jungleclear.IfMana || Cost != 1f))
				{
					if (Mobs.Count > 0)
					{
						if(spell.IsSkillshot)
						{
							if(spell.Type == SkillshotType.SkillshotLine)
							LCast(spell,Mobs[0],ExtraTargetDistance,ALPHA);
							else if(spell.Type == SkillshotType.SkillshotCircle)
							CCast(spell,Mobs[0]);
							else if(spell.Type == SkillshotType.SkillshotCone)
							spell.Cast(Mobs[0]);
						}
						else
						spell.Cast(Mobs[0]);
					}
				}
			}
			else if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit)
			{
				if(Menu.Item("Lasthit.Use " + spell.Slot.ToString(), true).GetValue<bool>()
				&& spell.IsReady() && (getManaPercent(Player) > AIO_Menu.Champion.Lasthit.IfMana || Cost != 1f))
				LH(spell,ALPHA);
			}
		}
		
		internal static List<Obj_AI_Hero> GetEnemyList()// 어짜피 원 기능은 중복되니 추가적으로 옵션을 줌.
		{
			return ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy && x.IsValid && !x.IsDead && !x.IsInvulnerable).ToList();
		}
		
		internal static int EnemyCount(float range, float min = 0, float max = 100)// 어짜피 원 기능은 중복되니 추가적으로 옵션을 줌. 특정 체력% 초과 특정 체력% 이하의 적챔프 카운트
		{
			return GetEnemyList().Where(x => x.Distance(Player.ServerPosition) <= range && getHealthPercent(x) > min && getHealthPercent(x) <= max).Count();
		}
		
		internal static int ECTarget(Obj_AI_Hero target, float range, float min = 0, float max = 100)// 어짜피 원 기능은 중복되니 추가적으로 옵션을 줌. 특정 체력% 초과 특정 체력% 이하의 적챔프 카운트
		{
			return GetEnemyList().Where(x => x.Distance(target.ServerPosition) <= range && getHealthPercent(x) > min && getHealthPercent(x) <= max).Count();
		}

        internal static double UnitIsImmobileUntil(Obj_AI_Base unit)
        {
            var result =
                unit.Buffs.Where(
                    buff =>
                        buff.IsActive && Game.Time <= buff.EndTime &&
                        (buff.Type == BuffType.Charm || buff.Type == BuffType.Knockup || buff.Type == BuffType.Stun ||
                         buff.Type == BuffType.Suppression || buff.Type == BuffType.Snare))
                    .Aggregate(0d, (current, buff) => Math.Max(current, buff.EndTime));
            return (result - Game.Time);
        }

        internal static bool CollisionCheck(Obj_AI_Hero source, Obj_AI_Hero target, float width)
        {
            var input = new PredictionInput
            {
                Radius = width,
                Unit = source,
            };

            input.CollisionObjects[0] = CollisionableObjects.Heroes;
            input.CollisionObjects[1] = CollisionableObjects.YasuoWall;

            return Collision.GetCollision(new List<SharpDX.Vector3> { target.ServerPosition }, input).Where(x => x.NetworkId != x.NetworkId).Any();
        }

        internal static int CountEnemyMinionsInRange(this SharpDX.Vector3 point, float range)
        {
            return ObjectManager.Get<Obj_AI_Minion>().Count(h => h.IsValidTarget(range, true, point));
        }

        internal class SelfAOE_Prediction
        {
            internal static int HitCount(float delay, float range)
            {
                byte hitcount = 0;

                foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(range)))
                {
                    var pred = Prediction.GetPrediction(enemy, delay);

                    if (Player.ServerPosition.Distance(pred.UnitPosition) <= range)
                        hitcount++;
                }

                return hitcount;
            }

            internal static int HitCount(float delay, float range, SharpDX.Vector3 sourcePosition)
            {
                byte hitcount = 0;

                foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(range)))
                {
                    var pred = Prediction.GetPrediction(enemy, delay);

                    if (sourcePosition.Distance(pred.UnitPosition) <= range)
                        hitcount++;
                }

                return hitcount;
            }
        }
    }
}

