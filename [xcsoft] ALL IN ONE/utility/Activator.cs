using System;

using LeagueSharp;
using LeagueSharp.Common;

namespace _xcsoft__ALL_IN_ONE.utility
{
    class Activator
    {
        internal static void Load()
        {
            xcsoftMenu.addSubMenu("Activator");

            xcsoftMenu.Menu_Manual.SubMenu("Activator").AddItem(new MenuItem( "DEBUG", "Hello World!"));
        }
    }
}
