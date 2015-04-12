using System;

using LeagueSharp;
using LeagueSharp.Common;

namespace _xcsoft__ALL_IN_ONE.utility
{
    class Activator
    {
        //아이템 목록 && 설명
        //http://lol.inven.co.kr/dataninfo/item/list.php
        //http://www.lolking.net/items/

        internal static void Load()
        {
            xcsoftMenu.addSubMenu("Activator");

            xcsoftMenu.Menu_Manual.SubMenu("Activator").AddItem(new MenuItem( "DEBUG", "Hello World!"));
        }
    }
}
