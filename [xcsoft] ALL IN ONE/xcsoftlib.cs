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

        internal static BuffInstance getBuffInstance(string buffname)
        {
            return ObjectManager.Player.Buffs.Find(x => x.Name == buffname);
        }
    }
}
