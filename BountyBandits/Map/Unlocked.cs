using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace BountyBandits.Map
{
    public enum DifficultyEnum{Normal, Hard, Scaled}

    public class UnlockedManager
    {
        private Dictionary<Guid, DifficultyProgress> campaignsUnlocked = new Dictionary<Guid, DifficultyProgress>();

        /// <summary>
        /// Append xml to inputNode
        /// </summary>
        /// <param name="inputNode">Node getting appended</param>
        /// <returns></returns>
        public void asXML(XmlNode inputNode)
        {
            XmlNode node = inputNode.OwnerDocument.CreateNode(XmlNodeType.Element, "levelsUnlocked", "");
            foreach (Guid guid in campaignsUnlocked.Keys)
            {
                XmlElement guidnode = (XmlElement)inputNode.OwnerDocument.CreateNode(XmlNodeType.Element, "difficultyUnlocked", "");
                guidnode.SetAttribute("guid", guid.ToString());
                foreach (DifficultyEnum diffENum in campaignsUnlocked[guid].difficultiesUnlocked.Keys)
                {
                    XmlElement diffnode = (XmlElement)inputNode.OwnerDocument.CreateNode(XmlNodeType.Element, "levels", "");
                    diffnode.SetAttribute("difficulty", diffENum.ToString());
                    String levels = "";
                    foreach (int level in campaignsUnlocked[guid].difficultiesUnlocked[diffENum].levelsUnlocked)
                        levels += level + ",";
                    diffnode.Value = levels;
                    guidnode.AppendChild(diffnode);
                }
                node.AppendChild(guidnode);
            }
            inputNode.AppendChild(node);
        }

        /// <summary>
        /// Verify if the level is unlocked
        /// </summary>
        /// <param name="guid">campaign guid</param>
        /// <param name="difficulty"></param>
        /// <param name="level">Level to be checked. 1 based, NOT INDEX</param>
        /// <returns>true if level is unlocked and playable</returns>
        public bool isUnlocked(Guid guid, DifficultyEnum difficulty, int level)
        {
            if (campaignsUnlocked.ContainsKey(guid) && campaignsUnlocked[guid].difficultiesUnlocked.ContainsKey(difficulty) &&
                campaignsUnlocked[guid].difficultiesUnlocked[difficulty].isUnlocked(level-1))
                return true;
            return false;
        }

        public void add(MapManager mapManager, DifficultyEnum difficulty)
        {
            if (!campaignsUnlocked.ContainsKey(mapManager.guid))
                campaignsUnlocked.Add(mapManager.guid, new DifficultyProgress());
            DifficultyProgress diffProgress = campaignsUnlocked[mapManager.guid];
            if (!diffProgress.difficultiesUnlocked.ContainsKey(difficulty))
                diffProgress.difficultiesUnlocked.Add(difficulty, new LevelsUnlocked());
            LevelsUnlocked unlocked = diffProgress.difficultiesUnlocked[difficulty];
            if(!unlocked.isUnlocked(mapManager.currentLevelIndex))
                unlocked.levelsUnlocked.Add(mapManager.currentLevelIndex);
        }
    }

    public class DifficultyProgress
    {
        public Dictionary<DifficultyEnum, LevelsUnlocked> difficultiesUnlocked = new Dictionary<DifficultyEnum, LevelsUnlocked>();
    }

    public class LevelsUnlocked
    {
        public List<int> levelsUnlocked = new List<int>();
        public bool isUnlocked(int unlocked)
        {
            foreach (int lvl in levelsUnlocked)
                if (lvl == unlocked)
                    return true;
            return false;
        }
    }
}
