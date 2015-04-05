using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

namespace _xcsoft__ALL_IN_ONE
{
    internal static class xcsoftMenu
    {
        internal static Menu Menu;
        internal static Orbwalking.Orbwalker Orbwalker;

        internal static void initialize(string RootMenuDisplayName)
        {
            Menu = new Menu(RootMenuDisplayName, RootMenuDisplayName, true);
            Menu.AddToMainMenu();
        }

        internal static void initialize(string RootMenuDisplayName, bool AddOrbwalker = false, bool AddTargetSelector = false, string tag = "")
        {
            Menu = new Menu(RootMenuDisplayName, RootMenuDisplayName, true);
            Menu.AddToMainMenu();

            if (AddOrbwalker)
                addOrbwalker(tag);

            if (AddTargetSelector)
                addTargetSelector(tag);
        }

        internal static void addOrbwalker(string tag)
        {
            Orbwalker = new Orbwalking.Orbwalker(Menu.AddSubMenu(new Menu((tag != string.Empty ? tag + ": " : string.Empty) + "Orbwalker", "Orbwalker")));
        }

        internal static void addTargetSelector(string tag)
        {
            TargetSelector.AddToMenu(Menu.AddSubMenu(new Menu((tag != string.Empty ? tag + ": " : string.Empty) + "Target Selector", "Target Selector")));
        }

        internal static void addSubMenu_ChampTemplate()
        {
            addSubMenu("Combo", "Combo");
            addSubMenu("Harass", "Harass");
            addSubMenu("Laneclear", "Laneclear");
            addSubMenu("Jungleclear", "Jungleclear");
            addSubMenu("Misc", "Misc");
            addSubMenu("Drawings", "Drawings");
        }

        internal static void addSubMenu_ChampTemplate(string tag)
        {
            if(tag != string.Empty)
                tag += ": ";

            addSubMenu("Combo", tag + "Combo");
            addSubMenu("Harass", tag + "Harass");
            addSubMenu("Laneclear", tag + "Laneclear");
            addSubMenu("Jungleclear",tag + "Jungleclear");
            addSubMenu( "Misc",tag + "Misc");
            addSubMenu("Drawings", tag + "Drawings");
        }

        internal static void addComboitems(object[][] items)
        {
            for (int i = 0; i < items.Length; i++)
                Menu.SubMenu("Combo").AddItem(new MenuItem("CbUse" + items[i][0], "Use " + items[i][0], true).SetValue(items[i][1]));
        }

        internal static void addHarassitems(object[][] items, bool ifMana)
        {
            for (int i = 0; i < items.Length; i++)
                Menu.SubMenu("Harass").AddItem(new MenuItem("HrsUse" + items[i][0], "Use " + items[i][0], true).SetValue(items[i][1]));

            if(ifMana)
                Menu.SubMenu("Harass").AddItem(new MenuItem("HrsMana", "if Mana % >", true).SetValue(new Slider(60, 0, 100)));
        }

        internal static void addLanclearitems(object[][] items, bool ifMana)
        {
            for (int i = 0; i < items.Length; i++)
                Menu.SubMenu("Laneclear").AddItem(new MenuItem("LcUse" + items[i][0], "Use " + items[i][0], true).SetValue(items[i][1]));

            if (ifMana)
                Menu.SubMenu("Laneclear").AddItem(new MenuItem("LcMana", "if Mana % >", true).SetValue(new Slider(60, 0, 100)));
        }

        internal static void addJungleclearitems(object[][] items, bool ifMana)
        {
            for (int i = 0; i < items.Length; i++)
                Menu.SubMenu("Jungleclear").AddItem(new MenuItem("JcUse" + items[i][0], "Use " + items[i][0], true).SetValue(items[i][1]));

            if (ifMana)
                Menu.SubMenu("Jungleclear").AddItem(new MenuItem("JcMana", "if Mana % >", true).SetValue(new Slider(20, 0, 100)));
        }

        internal static void addMiscitems(object[][] items)
        {
            for (int i = 0; i < items.Length; i++)
                Menu.SubMenu("Misc").AddItem(new MenuItem("Misc" + items[i][0], "Use " + items[i][0], true).SetValue(items[i][1]));
        }

        internal static void addDrawingsitems(object[][] items)
        {
            for (int i = 0; i < items.Length; i++)
            {
                Menu.SubMenu("Drawings").AddItem(new MenuItem("Draw" + items[i][0], ""+items[i][0], true).SetValue(items[i][1]));
            }
        }

        internal static void addSubMenu(string DisplayName)
        {
            Menu.AddSubMenu(new Menu(DisplayName, DisplayName));
        }

        internal static void addSubMenu(string Name,string DisplayName)
        {
            Menu.AddSubMenu(new Menu(DisplayName, Name));
        }

        internal static void addItem(string DisplayName)
        {
            Menu.AddItem(new MenuItem(DisplayName, DisplayName));
        }

        internal static void addItem(string Name,string DisplayName)
        {
            Menu.AddItem(new MenuItem(Name, DisplayName));
        }

        internal static void addItem(string Name, string DisplayName, object Value)
        {
            Menu.AddItem(new MenuItem(Name, DisplayName).SetValue(Value));
        }

        internal static void addItem(string Name, string DisplayName, bool ChampionUniq, object Value)
        {
            Menu.AddItem(new MenuItem(Name, DisplayName, ChampionUniq).SetValue(Value));
        }

        internal static void addItemChampionUniq(string Name, string DisplayName, object Value)
        {
            Menu.AddItem(new MenuItem(Name, DisplayName, true).SetValue(Value));
        }
    }
}
