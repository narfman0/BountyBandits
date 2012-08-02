using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace BountyBandits.Story
{
    public class AudioElement
    {
        /// <summary>
        /// Path to sound to play
        /// </summary>
        public String audioPath;

        /// <summary>
        /// Start time in ms when to play this audio
        /// </summary>
        public int startTime;

        public bool executed = false;

        public static AudioElement fromXML(XmlNode audioSegment)
        {
            AudioElement element = new AudioElement();
            foreach (XmlNode subnode in audioSegment.ChildNodes)
                if (subnode.Name.Equals("audioPath"))
                    element.audioPath = subnode.FirstChild.Value;
                else if (subnode.Name.Equals("startTime"))
                    element.startTime = int.Parse(subnode.FirstChild.Value);
            return element;
        }

        public XmlElement asXML(XmlDocument doc)
        {
            XmlElement element = doc.CreateElement("audio");
            XMLUtil.addElementValue(doc, element, "audioPath", audioPath.ToString());
            XMLUtil.addElementValue(doc, element, "startTime", startTime.ToString());
            return element;
        }
    }
}
