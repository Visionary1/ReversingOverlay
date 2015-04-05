using System;

using LeagueSharp.Common;

namespace _xcsoft__ALL_IN_ONE
{
    class Program
    {
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            initializer.initialize();
        }
    }
}