/*
#region

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace LeagueSharp.Common
{
    public static class Damage
    {
        static Damage()
        {
            Spells.Add(
                "DrMundo", new List<DamageSpell>
                {
                    //Q
                    new DamageSpell
                    {
                        Slot = SpellSlot.Q,
                        DamageType = DamageType.Magical,
                        Damage = (source, target, level) =>
                        {
                            if (target is Obj_AI_Minion)
                            {
                                return Math.Max
                                    (Math.Min(
                                    new double[] { 300, 400, 500, 600, 700 }[level],
                                    new double[] { 15, 18, 21, 23, 25 }[level] / 100 * target.Health),
                                    new double[] { 80, 130, 180, 230, 280 }[level]);
                            }
                            else
                            return Math.Max(
                                new double[] { 80, 130, 180, 230, 280 }[level],
                                new double[] { 15, 18, 21, 23, 25 }[level] / 100 * target.Health);
                        }
                    },

                });
        }
    }
}*/