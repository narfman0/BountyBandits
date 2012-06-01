using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using BountyBandits.Story;

namespace BountyBandits.Map
{
    public class MapManager
    {
        public const string CAMPAIGNS_PATH = @"Campaigns\",
            CONTENT_PATH = @"Content\",
            DEFAULT_CAMPAIGN_PATH = CAMPAIGNS_PATH + @"default\",
            MAP_FILENAME = "map.xml";
        List<Level> levels;
        public int currentLevelIndex;
        public Guid guid;
        public Texture2D worldBackground;
        public String currentCampaignPath;
        public int getCurrentLevelIndex() { return currentLevelIndex; }

        public MapManager(Game gameref, String campaignPath){
            levels = new List<Level>();
            currentCampaignPath =  campaignPath;
            FileStream fs = new FileStream(CONTENT_PATH + campaignPath + MAP_FILENAME, FileMode.Open, FileAccess.Read);
            XmlDocument mapdoc = new XmlDocument();
            mapdoc.Load(fs);
            guid = new Guid(mapdoc.GetElementsByTagName("guid").Item(0).FirstChild.Value);
            foreach(XmlElement node in mapdoc.GetElementsByTagName("level"))
                if(node.HasAttribute("name"))	//use level, not enemy level
                    levels.Add(Level.fromXML(node, gameref, campaignPath));
            worldBackground = gameref.Content.Load<Texture2D>(campaignPath + "worldBackground");
        }
        public List<Level> getLevels() { return levels; }
        public Level getCurrentLevel() { return getLevelByNumber(currentLevelIndex); }
        public void incrementCurrentLevel(bool up)
        {
            if(up && getLevelByNumber(currentLevelIndex+1) != null)
                currentLevelIndex++;
            if(!up && currentLevelIndex > 0)
                currentLevelIndex--;
        }
        public Level getLevelByNumber(int number)
        {
            foreach (Level level in levels)
                if (level.number.Equals(number))
                    return level;
            return null;
        }
    }
}
