using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Xml;

namespace BountyBandits.Story
{
    /// <summary>
    /// Represents a line segment for the camera to pan
    /// </summary>
    public class CameraPathSegment
    {
        public Vector2 begin, end;

        /// <summary>
        /// Time to go from begin to end in ms
        /// </summary>
        public int msSpan;

        /// <summary>
        /// Represents when this path should start overall in cutscene
        /// </summary>
        public int msStart;

        public Vector2 getCurrent(GameTime gameTime)
        {
            float percentComplete = ((float)gameTime.TotalGameTime.TotalMilliseconds - msStart) / msSpan;
            return (end - begin) * percentComplete + begin;
        }

        public static CameraPathSegment fromXML(XmlNode pathSegment)
        {
            CameraPathSegment element = new CameraPathSegment();
            foreach (XmlNode subnode in pathSegment.ChildNodes)
                if (subnode.Name.Equals("begin"))
                    element.begin = XMLUtil.fromXMLVector2(subnode.FirstChild);
                else if (subnode.Name.Equals("end"))
                    element.end = XMLUtil.fromXMLVector2(subnode.FirstChild);
                else if (subnode.Name.Equals("msSpan"))
                    element.msSpan = int.Parse(subnode.FirstChild.Value);
                else if (subnode.Name.Equals("msStart"))
                    element.msStart = int.Parse(subnode.FirstChild.Value);
            return element;
        }

        public XmlElement asXML(XmlDocument doc)
        {
            XmlElement element = doc.CreateElement("segment");
            XMLUtil.addElementValue(doc, element, "begin", begin.ToString());
            XMLUtil.addElementValue(doc, element, "end", end.ToString());
            XMLUtil.addElementValue(doc, element, "msSpan", msSpan.ToString());
            XMLUtil.addElementValue(doc, element, "msStart", msStart.ToString());
            return element;
        }
    }
}
