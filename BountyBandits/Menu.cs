using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace BountyBandits
{
    public class Menu
    {
        public enum MenuScreens
        {
            Stats, Inv, Data
        };
        #region Fields
        bool menuActive = false;
        int activeMenuItem = 0, timeMenuItemChanged = 0, timeMenuScreenChanged = 0;
        Menu.MenuScreens activeMenuScreen = Menu.MenuScreens.Data;
        #endregion
        public void changeMenuItem(bool up)
        {
            if (Environment.TickCount - timeMenuItemChanged > 300)
            {
                timeMenuItemChanged = Environment.TickCount;
                if (up) activeMenuItem--;
                else ++activeMenuItem;

                if(activeMenuItem<0) activeMenuItem = 0;
                if (activeMenuScreen == MenuScreens.Data)
                    if (activeMenuItem > 3) activeMenuItem = 3;
                if (activeMenuScreen == MenuScreens.Inv)
                    if (activeMenuItem > 0) activeMenuItem = 0;
                if (activeMenuScreen == MenuScreens.Stats)
                    if (activeMenuItem > 0) activeMenuItem = 0;
            }
        }
        public void changeMenuScreen(bool right)
        {
            if (Environment.TickCount - timeMenuScreenChanged > 300)
            {
                timeMenuScreenChanged = Environment.TickCount;
                activeMenuItem = 0;
                if (right)
                {
                    if (activeMenuScreen == MenuScreens.Data) activeMenuScreen = MenuScreens.Inv;
                    else if (activeMenuScreen == MenuScreens.Inv) activeMenuScreen = MenuScreens.Stats;
                    else if (activeMenuScreen == MenuScreens.Stats) activeMenuScreen = MenuScreens.Data;
                }
                else
                {
                    if (activeMenuScreen == MenuScreens.Data) activeMenuScreen = MenuScreens.Stats;
                    else if (activeMenuScreen == MenuScreens.Inv) activeMenuScreen = MenuScreens.Data;
                    else if (activeMenuScreen == MenuScreens.Stats) activeMenuScreen = MenuScreens.Inv;
                }
            }
        }
        public bool getMenuActive() { return menuActive; }
        public Color getMenuColor(int menuNum)
        {
            return (menuNum == getMenuItem() && (Environment.TickCount / 200) % 2 == 0) ? new Color(45, 45, 45) : Color.Black;
        }
        public int getMenuItem() { return activeMenuItem; }
        public MenuScreens getMenuScreen() { return activeMenuScreen; }
        public void toggleMenu()
        {
            menuActive = !menuActive;
        }
    }
}
