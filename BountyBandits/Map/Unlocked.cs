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
        public XmlNode asXML(XmlNode inputNode)
        {
            XmlNode node = inputNode.OwnerDocument.CreateElement("levelsUnlocked");
            foreach (Guid guid in campaignsUnlocked.Keys)
            {
                XmlElement guidnode = campaignsUnlocked[guid].asXML(node);
                guidnode.SetAttribute("guid", guid.ToString());
                node.AppendChild(guidnode);
            }
            return node;
        }

        public static UnlockedManager fromXML(XmlElement xmlElement)
        {
            UnlockedManager manager = new UnlockedManager();
            foreach(XmlElement element in xmlElement.GetElementsByTagName("difficultyUnlocked"))
            {
                DifficultyProgress prog = DifficultyProgress.fromXML(element);
                manager.campaignsUnlocked.Add(new Guid(element.GetAttribute("guid")), prog);
            }
            return manager;
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
            if(!unlocked.isUnlocked(mapManager.getCurrentLevelIndex()))
                unlocked.levelsUnlocked.Add(mapManager.getCurrentLevelIndex());
        }
    }

    public class DifficultyProgress
    {
        public Dictionary<DifficultyEnum, LevelsUnlocked> difficultiesUnlocked = new Dictionary<DifficultyEnum, LevelsUnlocked>();

        public XmlElement asXML(XmlNode parentNode)
        {
            XmlElement diffUnlockedNode = parentNode.OwnerDocument.CreateElement("difficultyUnlocked");
            foreach (DifficultyEnum diffENum in difficultiesUnlocked.Keys)
            {
                XmlElement levelsNode = parentNode.OwnerDocument.CreateElement("levels");
                levelsNode.SetAttribute("difficulty", diffENum.ToString());
                String levels = "";
                foreach (int level in difficultiesUnlocked[diffENum].levelsUnlocked)
                    levels += level + ",";
                levelsNode.SetAttribute("levels", levels);
                diffUnlockedNode.AppendChild(levelsNode);
            }
            return diffUnlockedNode;
        }

        public static DifficultyProgress fromXML(XmlElement element)
        {
            DifficultyProgress progress = new DifficultyProgress();
            foreach (XmlElement levelsNode in element)
            {
                DifficultyEnum difficulty = (DifficultyEnum)Enum.Parse(typeof(DifficultyEnum), levelsNode.GetAttribute("difficulty"));
                List<int> levels = new List<int>();
                foreach(String level in levelsNode.GetAttribute("levels").Split(','))
                    try
                    {
                        levels.Add(int.Parse(level));
                    }
                    catch (Exception e) { System.Console.Write(e.StackTrace); }
                progress.difficultiesUnlocked.Add(difficulty, new LevelsUnlocked(levels));
            }
            return progress;
        }
    }

    public class LevelsUnlocked
    {
        public List<int> levelsUnlocked = new List<int>();
        public LevelsUnlocked() { }
        public LevelsUnlocked(List<int> levelsUnlocked)
        {
            this.levelsUnlocked = levelsUnlocked;
        }
        public bool isUnlocked(int unlocked)
        {
            foreach (int lvl in levelsUnlocked)
                if (lvl == unlocked)
                    return true;
            return false;
        }
    }
}
