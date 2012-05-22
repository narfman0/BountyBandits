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
    }

    public struct BackgroundItemStruct
    {
        public string texturePath;
        public Vector2 location;
        public float rotation, scale, layer;
        public static BackgroundItemStruct fromXML(XmlElement element)
        {
            BackgroundItemStruct str = new BackgroundItemStruct();
            str.location = XMLUtil.fromXMLVector2(element.GetElementsByTagName("location")[0]);
            str.texturePath = element.GetElementsByTagName("path")[0].FirstChild.Value;
            str.rotation = element.HasAttribute("rotation") ? float.Parse(element.GetAttribute("rotation")) : 0f;
            str.scale = element.HasAttribute("scale") ? float.Parse(element.GetAttribute("scale")) : 1f;
            str.layer = element.HasAttribute("layer") ? float.Parse(element.GetAttribute("layer")) : 1f;
            return str;
        }
    }
}
