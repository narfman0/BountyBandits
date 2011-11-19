using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Xml;

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
    }
}
