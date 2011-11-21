using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Xml;
using Microsoft.Xna.Framework.Graphics;

namespace BountyBandits
{
    public static class XMLUtil
    {
        public static Vector2 fromXML(XmlNode node)
        {
            Vector2 vec = Vector2.Zero;
            foreach (XmlNode subnode in node.ChildNodes)
                if (subnode.Name.Equals("x"))
                    vec.X = float.Parse(subnode.FirstChild.Value);
                else if (subnode.Name.Equals("y"))
                    vec.Y = float.Parse(subnode.FirstChild.Value);
            return vec;
        }

        public static Color colorFromXML(XmlNode xmlNode)
        {
            byte r = 0, g = 0, b = 0;
            foreach (XmlNode subnode in xmlNode.ChildNodes)
                if (subnode.Name.Equals("r"))
                    r = byte.Parse(subnode.FirstChild.Value);
                else if (subnode.Name.Equals("g"))
                    g = byte.Parse(subnode.FirstChild.Value);
                else if (subnode.Name.Equals("b"))
                    b = byte.Parse(subnode.FirstChild.Value);
            return new Color(r,g,b);
        }
    }
}
