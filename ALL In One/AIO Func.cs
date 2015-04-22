using System;
using System.Linq;
using System.Collections.Generic;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One
{
    class AIO_Func
    {
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
		
		internal static void LCast(Spell spell, Obj_AI_Base target, float alpha, float colmini) //for Linar spells  사용예시 AIO_Func.LCast(Q,Qtarget,50,0)  
		{							//        AIO_Func.LCast(E,Etarget,Menu.Item("Misc.Etg").GetValue<Slider>().Value,float.MaxValue); <- 이런식으로 사용.

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

		/*
		internal static void LH(Spell spell) // For Last hit with skill for farming
		{
			if(spell == null)
			return
				var _m = MinionManager.GetMinions(spell.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth).FirstOrDefault(m => m.Health < ((Player.GetSpellDamage(m, SpellSlot.spell))) && HealthPrediction.GetHealthPrediction(m, (int)(Player.Distance(m, false) / spell.Speed), (int)(spell.Delay * 1000 + Game.Ping / 2)) > 0);			
                if (_m != null)
				{
				if() // 선형 스킬일경우
                LCast(spell,_m,50,0);
				else if() // 원형 스킬일경우
				CCast(spell,_m);
				}
		}
		*/

        internal static void MotionCancel()
        {
            Game.Say("/d");
        }
		
		internal static bool AfterAttack()
		{
			return SWDuration > 4.9;
		}
		
    }
}
