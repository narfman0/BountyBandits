using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BountyBandits.Story;
using System.Xml;

namespace BountyBandits.Map
{
    public class Level
    {
        #region Map editor relevant
        public int number = -1, levelLength;
        public string name = "default";
        public List<int> adjacent = new List<int>();
        public List<int> prereq = new List<int>();
        public List<BackgroundItemStruct> backgroundItems = new List<BackgroundItemStruct>();
        public Vector2 loc = Vector2.Zero;
        #endregion

        #region In game specific
        public Texture2D horizon;
        public List<StoryElement> storyElements = new List<StoryElement>();
        public List<GameItem> items = new List<GameItem>();
        public List<SpawnPoint> spawns = new List<SpawnPoint>();
        public bool autoProgress = false;
        #endregion

        public void resetStoryElements()
        {
            foreach (StoryElement ele in storyElements)
                ele.resetExecuted();
        }

        public StoryElement popStoryElement(float aveX)
        {
            foreach (StoryElement ele in storyElements)
                if (!ele.executed && ele.startX <= aveX)
                {
                    ele.executed = true;
                    return ele;
                }
            return null;
        }

        public static Level fromXML(XmlElement node, Game gameref, String campaignPath)
        {
            Level newLvl = new Level();
            newLvl.number = int.Parse(node.GetElementsByTagName("number")[0].FirstChild.Value);
            if(node.HasAttribute("autoProgress"))
                newLvl.autoProgress = bool.Parse(node.GetAttribute("autoProgress"));
            newLvl.name = node.GetAttribute("name");

            foreach (string singleAdj in node.GetElementsByTagName("adj")[0].FirstChild.Value.Split(','))
                newLvl.adjacent.Add(Int32.Parse(singleAdj));
            XmlNodeList list = node.GetElementsByTagName("prereq");
            if (list.Count > 0 && list[0].FirstChild != null)
                foreach (string singlePrereq in node.GetElementsByTagName("prereq")[0].FirstChild.Value.Split(','))
                    newLvl.prereq.Add(Int32.Parse(singlePrereq));
            newLvl.loc = XMLUtil.fromXMLVector2(node.GetElementsByTagName("location")[0]);
            if(node.GetElementsByTagName("horizonPath").Count > 0)
                newLvl.horizon = gameref.Content.Load<Texture2D>(campaignPath + node.GetElementsByTagName("horizonPath")[0].FirstChild.Value);
            if(node.GetElementsByTagName("items").Count > 0)
                foreach (XmlElement item in node.GetElementsByTagName("items")[0].ChildNodes)
                    newLvl.items.Add(GameItem.fromXML(item));
            if(node.GetElementsByTagName("spawns").Count > 0)
                foreach (XmlElement item in node.GetElementsByTagName("spawns")[0].ChildNodes)
                    newLvl.spawns.Add(SpawnPoint.fromXML(item));
            XmlNodeList storyNodes = node.GetElementsByTagName("story");
            if(storyNodes.Count > 0 && storyNodes[0].ChildNodes.Count > 0)
                foreach (XmlNode item in storyNodes[0].ChildNodes)
                    newLvl.storyElements.Add(StoryElement.fromXML(item, gameref));
            newLvl.levelLength = int.Parse(node.GetAttribute("length"));
            foreach (XmlElement graphic in node.GetElementsByTagName("graphic"))
                newLvl.backgroundItems.Add(BackgroundItemStruct.fromXML(graphic));
            return newLvl;
        }

        public XmlElement asXML(XmlDocument doc)
        {
            XmlElement element = doc.CreateElement("level");
            element.SetAttribute("length", levelLength.ToString());
            element.SetAttribute("name", name);
            element.SetAttribute("autoProgress", autoProgress.ToString());
            if(horizon != null)
                XMLUtil.addElementValue(doc, element, "horizonPath", horizon.Name);
            XMLUtil.addElementValue(doc, element, "adj", listToString(adjacent));
            XMLUtil.addElementValue(doc, element, "prereq", listToString(prereq));
            XMLUtil.asXMLVector2(doc, loc, "location");
            XmlElement backgroundElement = doc.CreateElement("graphic");
            foreach (BackgroundItemStruct backgroundItem in backgroundItems)
                backgroundElement.AppendChild(backgroundItem.asXML(doc));
            element.AppendChild(backgroundElement);
            XmlElement itemsElement = doc.CreateElement("items");
            foreach (GameItem gameItem in items)
                itemsElement.AppendChild(gameItem.asXML(doc));
            XmlElement spawnsElement = doc.CreateElement("spawns");
            foreach (SpawnPoint spawn in spawns)
                spawnsElement.AppendChild(spawn.asXML(doc));
            element.AppendChild(spawnsElement);
            XmlElement storyElement = doc.CreateElement("story");
            foreach (StoryElement story in storyElements)
                storyElement.AppendChild(story.asXML(doc));
            element.AppendChild(storyElement);
            return element;
        }

        private static String listToString(List<int> list)
        {
            String listString = "";
            foreach (int listItem in list)
                listString += listItem + ",";
            return listString.Length > 0 ? listString.Substring(0, listString.Length - 1) : listString;
        }

        public GameItem getGameItemAtLocation(float x, float y)
        {
            foreach (GameItem item in items)
            {
                Vector2 itemLocationScreen = item.getPositionFromScreenCoords();
                switch (item.polygonType)
                {
                    case PhysicsPolygonType.Circle:
                        if (Math.Pow(x - itemLocationScreen.X, 2) + Math.Pow(y - itemLocationScreen.Y, 2) < Math.Pow(item.radius, 2))
                            return item;
                        break;
                    default:
                        if (x < itemLocationScreen.X + item.sideLengths.X / 2 && x > itemLocationScreen.X - item.sideLengths.X / 2 &&
                            y < itemLocationScreen.Y + item.sideLengths.Y / 2 && y > itemLocationScreen.Y - item.sideLengths.Y / 2)
                            return item;
                        break;
                }
            }
            return null;
        }

        public SpawnPoint getSpawnAtLocation(float x, float y)
        {
            foreach (SpawnPoint spawn in spawns)
            {
                Vector2 dimensions = TextureManager.getDimensions(spawn.type);
                if (x < spawn.loc.X + dimensions.X && x > spawn.loc.X - dimensions.X &&
                    y < spawn.loc.Y + dimensions.Y && y > spawn.loc.Y - dimensions.Y)
                    return spawn;
            }
            return null;
        }

        public BackgroundItemStruct getBackgroundItemAtLocation(float x, float y)
        {
            foreach (BackgroundItemStruct str in backgroundItems)
            {
                Vector2 dimensions = TextureManager.getDimensions(str.texturePath);
                if (x < str.location.X + dimensions.X && x > str.location.X - dimensions.X &&
                    y < str.location.Y + dimensions.Y && y > str.location.Y - dimensions.Y)
                    return str;
            }
            return null;
        }

        public void remove(IMovableItem movingItem)
        {
            if(movingItem is BackgroundItemStruct)
                backgroundItems.Remove((BackgroundItemStruct)movingItem);
            if (movingItem is GameItem)
            {
                GameItem item = (GameItem)movingItem;
                items.Remove(item);
                if (item.guid != null && Game.instance.activeItems.ContainsKey(item.guid))
                    Game.instance.activeItems.Remove(item.guid);
            } if (movingItem is SpawnPoint)
                spawns.Remove((SpawnPoint)movingItem);
        }
    }
}
