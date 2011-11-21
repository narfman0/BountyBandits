﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework;

namespace BountyBandits
{
    public class SpawnPoint
    {
        public string name = "enemy";
        public Vector2 loc = Vector2.Zero;
        public uint count = 1;
        public uint bosses = 0;
        public string type;
        public uint weight = 1;
        public bool isSpawned = false;
        XmlNode fromNode;
        public SpawnPoint(XmlNode spawnnode)
        {
            fromXML(spawnnode);
        }
        public SpawnPoint Clone()
        {
            return new SpawnPoint(fromNode);
        }
        void fromXML(XmlNode node)
        {
            fromNode = node;
            foreach (XmlNode itemChild in node)
                if (itemChild.Name.Equals("name"))
                    name = itemChild.FirstChild.Value;
                else if (itemChild.Name.Equals("loc"))
                {
                    string[] locStr = itemChild.FirstChild.Value.Split(',');
                    loc = new Vector2(float.Parse(locStr[0]), float.Parse(locStr[1]));
                }
                else if (itemChild.Name.Equals("count"))
                    count = uint.Parse(itemChild.FirstChild.Value);
                else if (itemChild.Name.Equals("bosses"))
                    bosses = uint.Parse(itemChild.FirstChild.Value);
                else if (itemChild.Name.Equals("weight"))
                    weight = uint.Parse(itemChild.FirstChild.Value);
                else if (itemChild.Name.Equals("type"))
                    type = itemChild.FirstChild.Value;
        }
    }
}