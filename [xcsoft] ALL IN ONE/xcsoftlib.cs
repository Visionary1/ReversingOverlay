using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

using LeagueSharp;
using LeagueSharp.Common;

namespace _xcsoft__ALL_IN_ONE
{
    internal static class xcsoftlib
    {
        internal static float getHealthPercent(this Obj_AI_Base unit)
        {
            return unit.Health / unit.MaxHealth * 100;
        }

        internal static float getManaPercent(this Obj_AI_Base unit)
        {
            return unit.Mana / unit.MaxMana * 100;
        }

        internal static List<Obj_AI_Base> getCollisionMinions(Obj_AI_Hero source, SharpDX.Vector3 targetPos, float delay, float width, float speed)
        {
            var input = new PredictionInput
            {
                Unit = source,
                Radius = width,
                Delay = delay,
                Speed = speed,
            };

            input.CollisionObjects[0] = CollisionableObjects.Minions;

            return Collision.GetCollision(new List<SharpDX.Vector3> { targetPos }, input).OrderBy(obj => obj.Distance(source, false)).ToList();
        }

        internal static BuffInstance getBuffInstance(string buffName)
        {
            return ObjectManager.Player.Buffs.Find(x => x.Name == buffName && x.IsValidBuff());
        }

        internal static BuffInstance getBuffInstance(string buffName, Obj_AI_Base buffCaster)
        {
            return ObjectManager.Player.Buffs.Find(x => x.Name == buffName && x.Caster.NetworkId == buffCaster.NetworkId && x.IsValidBuff());
        }

        internal static bool Killable(Obj_AI_Base target, float damage)
        {
            return target.Health + target.HPRegenRate <= damage;
        }

        internal static bool Killable(Obj_AI_Base target, Spell spell)
        {
            return target.Health + (target.HPRegenRate/2) <= spell.GetDamage(target);
        }
    }
}
