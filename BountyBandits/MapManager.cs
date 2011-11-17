using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BountyBandits
{
    public class MapManager
    {
        const string MAP_PATH = @"Content\Map\map.xml";
        List<Level> levels;
        public int currentLevelIndex;
        public MapManager(Game gameref){
            levels = new List<Level>();
            FileStream fs = new FileStream(MAP_PATH, FileMode.Open, FileAccess.Read);
            XmlDocument mapdoc = new XmlDocument();
            mapdoc.Load(fs);
            XmlNodeList xmlnode = mapdoc.GetElementsByTagName("level");
            for(int i=0;i<xmlnode.Count;i++)
            {
                Level newLvl = new Level();
                foreach (XmlNode node in xmlnode[i].ChildNodes)
                    if (node.FirstChild != null)
                        if (node.Name.Equals("number"))
                            newLvl.number = Int32.Parse(node.FirstChild.Value);
                        else if (node.Name.Equals("name"))
                            newLvl.name = node.FirstChild.Value;
                        else if (node.Name.Equals("adj"))
                        {
                            string[] adjacent = node.FirstChild.Value.Split(',');
                            foreach (string singleAdj in adjacent)
                                newLvl.adjacent.Add(Int32.Parse(singleAdj));
                        }
                        else if (node.Name.Equals("prereq"))
                        {
                            string[] prereq = node.FirstChild.Value.Split(',');
                            foreach (string singlePrereq in prereq)
                                newLvl.prereq.Add(Int32.Parse(singlePrereq));
                        }
                        else if (node.Name.Equals("location"))
                        {
                            string[] loc = node.FirstChild.Value.Split(',');
                            foreach (string singlePrereq in loc)
                                newLvl.loc = new Vector2(Int32.Parse(loc[0]), Int32.Parse(loc[1]));
                        }
                        else if (node.Name.Equals("backgroundPath"))
                            newLvl.background = gameref.Content.Load<Texture2D>(node.FirstChild.Value);
                        else if (node.Name.Equals("horizonPath"))
                            newLvl.horizon = gameref.Content.Load<Texture2D>(node.FirstChild.Value);
                        else if (node.Name.Equals("items"))
                        {
                            foreach (XmlNode item in node.ChildNodes)
                            {
                                string name = "";
                                foreach (XmlNode itemChild in item.ChildNodes)
                                    if (itemChild.Name.Equals("name"))
                                        name = itemChild.FirstChild.Value;
                                if (name.Equals("enemies"))
                                    newLvl.spawns.Add(new SpawnPoint(item));
                                else
                                    newLvl.items.Add(new GameItem(item));
                            }
                        }
                if (newLvl.background == null)
                    newLvl.background = gameref.easyLevel;
                levels.Add(newLvl);
            }
        }
        public List<Level> getLevels() { return levels; }
        public Level getCurrentLevel() { return getLevelByNumber(currentLevelIndex); }
        public Level getLevelByNumber(int number)
        {
            foreach (Level level in levels)
                if (level.number.Equals(number))
                    return level;
            return null;
        }
    }
}
