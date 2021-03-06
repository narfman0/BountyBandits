﻿using System;
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

        public MapManager(String campaignPath){
            levels = new List<Level>();
            currentCampaignPath =  campaignPath;
            loadCampaign(campaignPath);
        }

        public List<Level> getLevels() { return levels; }

        public Level getCurrentLevel() { return getLevelByNumber(currentLevelIndex); }

        public void addLevel(Level level)
        {
            levels.Add(level);
        }

        public void removeLevel(int index)
        {
            levels.RemoveAt(index);
        }

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

        public int getNextUnusedLevelIndex()
        {
            for (int i = 0; i < int.MaxValue; i++)
                if (Game.instance.mapManager.getLevelByNumber(i) == null)
                    return i;
            return -1;
        }

        public void loadCampaign(String campaignPath)
        {
            levels.Clear();
            XmlDocument mapdoc = new XmlDocument();
            mapdoc.Load(new FileStream(CONTENT_PATH + campaignPath + MAP_FILENAME, FileMode.Open, FileAccess.Read));
            guid = new Guid(mapdoc.GetElementsByTagName("guid").Item(0).FirstChild.Value);
            foreach (XmlElement node in mapdoc.GetElementsByTagName("level"))
                if (node.HasAttribute("name"))	//use level, not enemy level
                    levels.Add(Level.fromXML(node, Game.instance, campaignPath));
            worldBackground = Game.instance.Content.Load<Texture2D>(campaignPath + "worldBackground");
        }
        public void saveCampaign(String campaignPath)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement rootElement = doc.CreateElement("Root");
            XMLUtil.addElementValue(doc, rootElement, "guid", guid.ToString());
            doc.AppendChild(rootElement);
            foreach (Level level in levels)
                rootElement.AppendChild(level.asXML(doc));
            doc.Save(new FileStream(CONTENT_PATH + campaignPath + MAP_FILENAME, FileMode.OpenOrCreate, FileAccess.Write));
        }
    }
}
