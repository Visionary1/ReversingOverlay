using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

namespace _xcsoft__ALL_IN_ONE
{
    internal static class xcsoftMenu//by xcsoft
    {
        internal static Menu Menu_Manual;
        internal static Orbwalking.Orbwalker Orbwalker;

        internal static class Combo
        {
            internal static void addItems(object[][] items)
            {
                for (int i = 0; i < items.Length; i++)
                    Menu_Manual.SubMenu("Combo").AddItem(new MenuItem("Combo." + items[i][0].ToString(), items[i][0].ToString(), true).SetValue(items[i][1]));
            }

            internal static Circle getCircleValue(string itemName)
            {
                return Menu_Manual.Item("Combo." + itemName, true).GetValue<Circle>();
            }

            internal static Boolean getBoolValue(string itemName)
            {
                return Menu_Manual.Item("Combo." + itemName, true).GetValue<Boolean>();
            }

            internal static Slider getSliderValue(string itemName)
            {
                return Menu_Manual.Item("Combo." + itemName, true).GetValue<Slider>();
            }

            internal static void addUseQ(bool enable = true)
            {
                Menu_Manual.SubMenu("Combo").AddItem(new MenuItem("Combo.Use Q", "Use Q", true).SetValue(enable));
            }

            internal static void addUseW(bool enable = true)
            {
                Menu_Manual.SubMenu("Combo").AddItem(new MenuItem("Combo.Use W", "Use W", true).SetValue(enable));
            }

            internal static void addUseE(bool enable = true)
            {
                Menu_Manual.SubMenu("Combo").AddItem(new MenuItem("Combo.Use E", "Use E", true).SetValue(enable));
            }

            internal static void addUseR(bool enable = true)
            {
                Menu_Manual.SubMenu("Combo").AddItem(new MenuItem("Combo.Use R", "Use R", true).SetValue(enable));
            }

            internal static bool UseQ
            {
                get { return Menu_Manual.Item("Combo.Use Q", true).GetValue<bool>(); }
            }

            internal static bool UseW
            {
                get { return Menu_Manual.Item("Combo.Use W", true).GetValue<bool>(); }
            }

            internal static bool UseE
            {
                get { return Menu_Manual.Item("Combo.Use E", true).GetValue<bool>(); }
            }

            internal static bool UseR
            {
                get { return Menu_Manual.Item("Combo.Use R", true).GetValue<bool>(); }
            }
        }

        internal static class Harass
        {
            internal static void addItems(object[][] items, bool ifMana)
            {
                for (int i = 0; i < items.Length; i++)
                    Menu_Manual.SubMenu("Harass").AddItem(new MenuItem("Harass." + items[i][0].ToString(), items[i][0].ToString(), true).SetValue(items[i][1]));

                if (ifMana)
                    Menu_Manual.SubMenu("Harass").AddItem(new MenuItem("Harass.ifMana", "if Mana % >", true).SetValue(new Slider(60, 0, 100)));
            }

            internal static Circle getCircleValue(string itemName)
            {
                return Menu_Manual.Item("Harass." + itemName, true).GetValue<Circle>();
            }

            internal static Boolean getBoolValue(string itemName)
            {
                return Menu_Manual.Item("Harass." + itemName, true).GetValue<Boolean>();
            }

            internal static Slider getSliderValue(string itemName)
            {
                return Menu_Manual.Item("Harass." + itemName, true).GetValue<Slider>();
            }

            internal static void addUseQ(bool enable = true)
            {
                Menu_Manual.SubMenu("Harass").AddItem(new MenuItem("Harass.Use Q", "Use Q", true).SetValue(enable));
            }

            internal static void addUseW(bool enable = true)
            {
                Menu_Manual.SubMenu("Harass").AddItem(new MenuItem("Harass.Use W", "Use W", true).SetValue(enable));
            }

            internal static void addUseE(bool enable = true)
            {
                Menu_Manual.SubMenu("Harass").AddItem(new MenuItem("Harass.Use E", "Use E", true).SetValue(enable));
            }

            internal static void addUseR(bool enable = true)
            {
                Menu_Manual.SubMenu("Harass").AddItem(new MenuItem("Harass.Use R", "Use R", true).SetValue(enable));
            }

            internal static void addifMana(int value = 60)
            {
                Menu_Manual.SubMenu("Harass").AddItem(new MenuItem("Harass.ifMana", "if Mana % >", true).SetValue(new Slider(value, 0, 100)));
            }

            internal static bool UseQ
            {
                get { return Menu_Manual.Item("Harass.Use Q", true).GetValue<bool>(); }
            }

            internal static bool UseW
            {
                get { return Menu_Manual.Item("Harass.Use W", true).GetValue<bool>(); }
            }

            internal static bool UseE
            {
                get { return Menu_Manual.Item("Harass.Use E", true).GetValue<bool>(); }
            }

            internal static bool UseR
            {
                get { return Menu_Manual.Item("Harass.Use R", true).GetValue<bool>(); }
            }

            internal static int ifMana
            {
                get { return Menu_Manual.Item("Harass.ifMana", true).GetValue<Slider>().Value; }
            }
        }

        internal static class Laneclear
        {
            internal static void addItems(object[][] items, bool ifMana)
            {
                for (int i = 0; i < items.Length; i++)
                    Menu_Manual.SubMenu("Laneclear").AddItem(new MenuItem("Laneclear." + items[i][0].ToString(), items[i][0].ToString(), true).SetValue(items[i][1]));

                if (ifMana)
                    Menu_Manual.SubMenu("Laneclear").AddItem(new MenuItem("Laneclear.ifMana", "if Mana % >", true).SetValue(new Slider(60, 0, 100)));
            }

            internal static Circle getCircleValue(string itemName)
            {
                return Menu_Manual.Item("Laneclear." + itemName, true).GetValue<Circle>();
            }

            internal static Boolean getBoolValue(string itemName)
            {
                return Menu_Manual.Item("Laneclear." + itemName, true).GetValue<Boolean>();
            }

            internal static Slider getSliderValue(string itemName)
            {
                return Menu_Manual.Item("Laneclear." + itemName, true).GetValue<Slider>();
            }

            internal static void addUseQ(bool enable = true)
            {
                Menu_Manual.SubMenu("Laneclear").AddItem(new MenuItem("Harass.Use Q", "Use Q", true).SetValue(enable));
            }

            internal static void addUseW(bool enable = true)
            {
                Menu_Manual.SubMenu("Laneclear").AddItem(new MenuItem("Harass.Use W", "Use W", true).SetValue(enable));
            }

            internal static void addUseE(bool enable = true)
            {
                Menu_Manual.SubMenu("Laneclear").AddItem(new MenuItem("Harass.Use E", "Use E", true).SetValue(enable));
            }

            internal static void addUseR(bool enable = true)
            {
                Menu_Manual.SubMenu("Laneclear").AddItem(new MenuItem("Harass.Use R", "Use R", true).SetValue(enable));
            }

            internal static void addifMana(int value = 60)
            {
                Menu_Manual.SubMenu("Laneclear").AddItem(new MenuItem("Laneclear.ifMana", "if Mana % >", true).SetValue(new Slider(value, 0, 100)));
            }

            internal static bool UseQ
            {
                get { return Menu_Manual.Item("Laneclear.Use Q", true).GetValue<bool>(); }
            }

            internal static bool UseW
            {
                get { return Menu_Manual.Item("Laneclear.Use W", true).GetValue<bool>(); }
            }

            internal static bool UseE
            {
                get { return Menu_Manual.Item("Laneclear.Use E", true).GetValue<bool>(); }
            }

            internal static bool UseR
            {
                get { return Menu_Manual.Item("Laneclear.Use R", true).GetValue<bool>(); }
            }

            internal static int ifMana
            {
                get { return Menu_Manual.Item("Laneclear.ifMana", true).GetValue<Slider>().Value; }
            }
        }

        internal static class Jungleclear
        {
            internal static void addItems(object[][] items, bool ifMana)
            {
                for (int i = 0; i < items.Length; i++)
                    Menu_Manual.SubMenu("Jungleclear").AddItem(new MenuItem("Jungleclear." + items[i][0].ToString(), items[i][0].ToString(), true).SetValue(items[i][1]));

                if (ifMana)
                    Menu_Manual.SubMenu("Jungleclear").AddItem(new MenuItem("Jungleclear.ifMana", "if Mana % >", true).SetValue(new Slider(20, 0, 100)));
            }

            internal static Circle getCircleValue(string itemName)
            {
                return Menu_Manual.Item("Jungleclear." + itemName, true).GetValue<Circle>();
            }

            internal static Boolean getBoolValue(string itemName)
            {
                return Menu_Manual.Item("Jungleclear." + itemName, true).GetValue<Boolean>();
            }

            internal static Slider getSliderValue(string itemName)
            {
                return Menu_Manual.Item("Jungleclear." + itemName, true).GetValue<Slider>();
            }

            internal static void addUseQ(bool enable = true)
            {
                Menu_Manual.SubMenu("Jungleclear").AddItem(new MenuItem("Jungleclear.Use Q", "Use Q", true).SetValue(enable));
            }

            internal static void addUseW(bool enable = true)
            {
                Menu_Manual.SubMenu("Jungleclear").AddItem(new MenuItem("Jungleclear.Use W", "Use W", true).SetValue(enable));
            }

            internal static void addUseE(bool enable = true)
            {
                Menu_Manual.SubMenu("Jungleclear").AddItem(new MenuItem("Jungleclear.Use E", "Use E", true).SetValue(enable));
            }

            internal static void addUseR(bool enable = true)
            {
                Menu_Manual.SubMenu("Jungleclear").AddItem(new MenuItem("Jungleclear.Use R", "Use R", true).SetValue(enable));
            }

            internal static void addifMana(int value = 60)
            {
                Menu_Manual.SubMenu("Jungleclear").AddItem(new MenuItem("Jungleclear.ifMana", "if Mana % >", true).SetValue(new Slider(value, 0, 100)));
            }

            internal static bool UseQ
            {
                get { return Menu_Manual.Item("Jungleclear.Use Q", true).GetValue<bool>(); }
            }

            internal static bool UseW
            {
                get { return Menu_Manual.Item("Jungleclear.Use W", true).GetValue<bool>(); }
            }

            internal static bool UseE
            {
                get { return Menu_Manual.Item("Jungleclear.Use E", true).GetValue<bool>(); }
            }

            internal static bool UseR
            {
                get { return Menu_Manual.Item("Jungleclear.Use R", true).GetValue<bool>(); }
            }

            internal static int ifMana
            {
                get { return Menu_Manual.Item("Jungleclear.ifMana", true).GetValue<Slider>().Value; }
            }
        }

        internal static class Misc
        {
            internal static void addItems(object[][] items)
            {
                for (int i = 0; i < items.Length; i++)
                    Menu_Manual.SubMenu("Misc").AddItem(new MenuItem("Misc." + items[i][0].ToString(), items[i][0].ToString(), true).SetValue(items[i][1]));
            }

            internal static Circle getCircleValue(string itemName)
            {
                return Menu_Manual.Item("Misc." + itemName, true).GetValue<Circle>();
            }

            internal static Boolean getBoolValue(string itemName)
            {
                return Menu_Manual.Item("Misc." + itemName, true).GetValue<Boolean>();
            }

            internal static Slider getSliderValue(string itemName)
            {
                return Menu_Manual.Item("Misc." + itemName, true).GetValue<Slider>();
            }

            internal static void addUseKillsteal(bool enable = true)
            {
                Menu_Manual.SubMenu("Misc").AddItem(new MenuItem("Misc.Use Killsteal", "Use Killsteal", true).SetValue(enable));
            }

            internal static void addUseInterrupter(bool enable = true)
            {
                Menu_Manual.SubMenu("Misc").AddItem(new MenuItem("Misc.Use Interrupter", "Use Interrupter", true).SetValue(enable));
            }

            internal static void addUseAntiGapcloser(bool enable = true)
            {
                Menu_Manual.SubMenu("Misc").AddItem(new MenuItem("Misc.Use Anti-Gapcloser", "Use Anti-Gapcloser", true).SetValue(enable));
            }

            internal static bool UseKillsteal
            {
                get { return Menu_Manual.Item("Misc.Use Killsteal", true).GetValue<bool>(); }
            }

            internal static bool UseInterrupter
            {
                get { return Menu_Manual.Item("Misc.Use Interrupter", true).GetValue<bool>(); }
            }

            internal static bool UseAntiGapcloser
            {
                get { return Menu_Manual.Item("Misc.Use Anti-Gapcloser", true).GetValue<bool>(); }
            }
        }

        internal static class Drawings
        {
            internal static void addItems(object[][] items)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    Menu_Manual.SubMenu("Drawings").AddItem(new MenuItem("Drawings." + items[i][0].ToString(), items[i][0].ToString(), true).SetValue(items[i][1]));
                }
            }

            internal static Circle getCircleValue(string itemName)
            {
                return Menu_Manual.Item("Drawings." + itemName, true).GetValue<Circle>();
            }

            internal static Boolean getBoolValue(string itemName)
            {
                return Menu_Manual.Item("Drawings." + itemName, true).GetValue<Boolean>();
            }

            internal static Slider getSliderValue(string itemName)
            {
                return Menu_Manual.Item("Drawings." + itemName, true).GetValue<Slider>();
            }

            internal static void addBlankItem()
            {
                Menu_Manual.SubMenu("Drawings").AddItem(new MenuItem("Drawings.BLANK", string.Empty));
            }

            internal static void addDamageIndicator(DamageIndicator.DamageToUnitDelegate damage)
            {
                var drawDamageMenu = new MenuItem("Draw_Damage", "Draw Combo Damage", true).SetValue(true);
                var drawDamageFill = new MenuItem("Draw_Fill", "Draw Combo Damage Fill", true).SetValue(new Circle(true, System.Drawing.Color.FromArgb(100, 255, 228, 0)));

                Menu_Manual.SubMenu("Drawings").AddItem(drawDamageMenu);
                Menu_Manual.SubMenu("Drawings").AddItem(drawDamageFill);

                DamageIndicator.DamageToUnit = damage;
                DamageIndicator.Enabled = drawDamageMenu.GetValue<bool>();
                DamageIndicator.Fill = drawDamageFill.GetValue<Circle>().Active;
                DamageIndicator.FillColor = drawDamageFill.GetValue<Circle>().Color;

                drawDamageMenu.ValueChanged +=
                delegate(object sender, OnValueChangeEventArgs eventArgs)
                {
                    DamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
                };

                drawDamageFill.ValueChanged +=
                delegate(object sender, OnValueChangeEventArgs eventArgs)
                {
                    DamageIndicator.Fill = eventArgs.GetNewValue<Circle>().Active;
                    DamageIndicator.FillColor = eventArgs.GetNewValue<Circle>().Color;
                };
            }

            internal static void addQrange(bool enable = true)
            {
                Menu_Manual.SubMenu("Drawings").AddItem(new MenuItem("Drawings.Q Range", "Q Range", true).SetValue(new Circle(enable, System.Drawing.Color.GreenYellow;)));
            }

            internal static void addWrange(bool enable = true)
            {
                Menu_Manual.SubMenu("Drawings").AddItem(new MenuItem("Drawings.W Range", "W Range", true).SetValue(new Circle(enable, System.Drawing.Color.GreenYellow;)));
            }

            internal static void addErange(bool enable = true)
            {
                Menu_Manual.SubMenu("Drawings").AddItem(new MenuItem("Drawings.E Range", "E Range", true).SetValue(new Circle(enable, System.Drawing.Color.GreenYellow;)));
            }

            internal static void addRrange(bool enable = true)
            {
                Menu_Manual.SubMenu("Drawings").AddItem(new MenuItem("Drawings.R Range", "R Range", true).SetValue(new Circle(enable, System.Drawing.Color.GreenYellow;)));
            }

            internal static Circle DrawQRange
            {
                get { return Menu_Manual.Item("Drawings.Q Range", true).GetValue<Circle>(); }
            }

            internal static Circle DrawWRange
            {
                get { return Menu_Manual.Item("Drawings.W Range", true).GetValue<Circle>(); }
            }

            internal static Circle DrawERange
            {
                get { return Menu_Manual.Item("Drawings.E Range", true).GetValue<Circle>(); }
            }

            internal static Circle DrawRRange
            {
                get { return Menu_Manual.Item("Drawings.R Range", true).GetValue<Circle>(); }
            }
        }

        //------------------------
        internal static void initialize(string RootMenuDisplayName)
        {
            Menu_Manual = new Menu(RootMenuDisplayName, RootMenuDisplayName, true);
            Menu_Manual.AddToMainMenu();
        }

        internal static void initialize(string RootMenuDisplayName, bool AddOrbwalker = false, bool AddTargetSelector = false, string tag = "")
        {
            Menu_Manual = new Menu(RootMenuDisplayName, RootMenuDisplayName, true);
            Menu_Manual.AddToMainMenu();

            if (AddOrbwalker)
                addOrbwalker(tag);

            if (AddTargetSelector)
                addTargetSelector(tag);
        }

        internal static void addOrbwalker(string tag)
        {
            Orbwalker = new Orbwalking.Orbwalker(Menu_Manual.AddSubMenu(new Menu((tag != string.Empty ? tag + ": " : string.Empty) + "Orbwalker", "Orbwalker")));
        }

        internal static void addTargetSelector(string tag)
        {
            TargetSelector.AddToMenu(Menu_Manual.AddSubMenu(new Menu((tag != string.Empty ? tag + ": " : string.Empty) + "Target Selector", "Target Selector")));
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
            if (tag != string.Empty)
                tag += ": ";

            addSubMenu("Combo", tag + "Combo");
            addSubMenu("Harass", tag + "Harass");
            addSubMenu("Laneclear", tag + "Laneclear");
            addSubMenu("Jungleclear", tag + "Jungleclear");
            addSubMenu("Misc", tag + "Misc");
            addSubMenu("Drawings", tag + "Drawings");
        }

        internal static void addSubMenu(string DisplayName)
        {
            Menu_Manual.AddSubMenu(new Menu(DisplayName, DisplayName));
        }

        internal static void addSubMenu(string Name,string DisplayName)
        {
            Menu_Manual.AddSubMenu(new Menu(DisplayName, Name));
        }

        internal static void addItem(string DisplayName)
        {
            Menu_Manual.AddItem(new MenuItem(DisplayName, DisplayName));
        }

        internal static void addItem(string Name,string DisplayName)
        {
            Menu_Manual.AddItem(new MenuItem(Name, DisplayName));
        }

        internal static void addItem(string Name, string DisplayName, object Value)
        {
            Menu_Manual.AddItem(new MenuItem(Name, DisplayName).SetValue(Value));
        }

        internal static void addItem(string Name, string DisplayName, bool ChampionUniq, object Value)
        {
            Menu_Manual.AddItem(new MenuItem(Name, DisplayName, ChampionUniq).SetValue(Value));
        }

        internal static void addItemChampionUniq(string Name, string DisplayName, object Value)
        {
            Menu_Manual.AddItem(new MenuItem(Name, DisplayName, true).SetValue(Value));
        }

        internal static void addBlankItem()
        {
            Menu_Manual.AddItem(new MenuItem("BLANK", string.Empty));
        }

        internal static Circle getCircleValue(string itemName)
        {
            return Menu_Manual.Item(itemName).GetValue<Circle>();
        }

        internal static Boolean getBoolValue(string itemName)
        {
            return Menu_Manual.Item(itemName).GetValue<Boolean>();
        }

        internal static Slider getSliderValue(string itemName)
        {
            return Menu_Manual.Item(itemName).GetValue<Slider>();
        }
    }
}
