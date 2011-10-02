using System;
using System.Collections.Generic;
using System.Text;

namespace BountyBandits
{
    public class XPManager
    {
        List<int> xpToLevelUp = new List<int>() { 100 };
        List<int> killXPPerLevel = new List<int>() { 8 };
        public int getXPToLevelUp(int level)
        {
            if (xpToLevelUp.Count <= level)
                xpToLevelUp.Add((int)(150 * Math.Pow(Math.E, .15 * level) - 48) + getXPToLevelUp(level-1));
            return xpToLevelUp[level];
        }
        public int getKillXPPerLevel(int level)
        {
            if (killXPPerLevel.Count <= level)
                killXPPerLevel.Add((int)Math.Pow(getKillXPPerLevel(level - 1), 1.08));
            return killXPPerLevel[level];
        }
    }
}
