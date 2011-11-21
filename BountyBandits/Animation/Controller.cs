using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using BountyBandits.Stats;

namespace BountyBandits.Animation
{
    public class AnimationController
    {
        public string name;
        public List<Texture2D> frames = new List<Texture2D>();
        public Texture2D portrait;
        public List<AnimationInfo> animations = new List<AnimationInfo>();
        public StatSet statRatios = new StatSet();
        public List<Color[]> permutations = new List<Color[]>();

        public void fromXML(ContentManager content, string name)
        {
            this.name = name;
            string xmlPath = @"Content\Beings\" + name + @"\" + name + ".xml";
            FileStream fs = new FileStream(xmlPath, FileMode.Open, FileAccess.Read);
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.Load(fs);

            XmlNodeList xmlnode = xmldoc.GetElementsByTagName("monster");
            for (int i = 0; i < xmlnode.Count; i++)
            {
                foreach (XmlNode monsterchild in xmlnode[i].ChildNodes)
                {
                    if (monsterchild.Name == "animations")
                    {
                        foreach (XmlNode node in monsterchild.ChildNodes)
                        {
                            AnimationInfo anim = new AnimationInfo();
                            foreach (XmlNode subnode in node.ChildNodes)
                                if (subnode.Name.Equals("name"))
                                    anim.name = subnode.FirstChild.Value;
                                else if (subnode.Name.Equals("start"))
                                    anim.start = int.Parse(subnode.FirstChild.Value);
                                else if (subnode.Name.Equals("end"))
                                    anim.end = int.Parse(subnode.FirstChild.Value);
                                else if (subnode.Name.Equals("keyframe"))
                                    anim.keyframe = int.Parse(subnode.FirstChild.Value);
                            animations.Add(anim);
                        }

                        int highestActiveFrame = 0;
                        foreach (XmlNode node in monsterchild.ChildNodes)
                            foreach (XmlNode subnode in node.ChildNodes)
                                if (subnode.Name.Equals("end"))
                                    if (int.Parse(subnode.FirstChild.Value) > highestActiveFrame)
                                        highestActiveFrame = int.Parse(subnode.FirstChild.Value);
                        for (int frameIndex = 1; frameIndex <= highestActiveFrame; ++frameIndex)
                        {
                            string frameIndexOffset = "";
                            if (frameIndex < 10) frameIndexOffset = "000";
                            else if (frameIndex < 100) frameIndexOffset = "00";
                            else if (frameIndex < 1000) frameIndexOffset = "0";
                            string newFramePath = @"Beings\" + name + @"\" + name + frameIndexOffset + frameIndex.ToString();
                            Texture2D newFrame = content.Load<Texture2D>(newFramePath);
                            getAlphaFromTex(ref newFrame);
                            frames.Add(newFrame);
                        }
                    } else if (monsterchild.Name == "stats")
                        foreach (XmlNode node in monsterchild.ChildNodes)
                        {
                            StatType statType = (StatType)Enum.Parse(typeof(StatType), node.Attributes.GetNamedItem("type").Value);
                            int value = int.Parse(node.Attributes.GetNamedItem("value").Value);
                            statRatios.addStatValue(statType, value);
                        }
                    else if (monsterchild.Name == "colorPermutations")
                        foreach (XmlNode permutationNode in monsterchild.ChildNodes)
                        {
                            Color[] colors = new Color[permutationNode.ChildNodes.Count];
                            for (int colorIndex = 0; colorIndex < colors.Length; colorIndex++)
                                colors[colorIndex] = XMLUtil.colorFromXML(permutationNode.ChildNodes.Item(colorIndex));
                            permutations.Add(colors);
                        }
                }
                foreach(string str in Directory.GetFiles(@"Content\Beings\" + name))
                    if (str.Contains("portrait"))
                    {
                        portrait = content.Load<Texture2D>(@"Beings\" + name + @"\portrait");
                        break;
                    }
            }
        }
        private void getAlphaFromTex(ref Texture2D tex)
        {
            Color[] pixels = new Color[tex.Width * tex.Height];
            tex.GetData<Color>(pixels);
            for (int i = 0; i < pixels.Length; i++)
                if (pixels[i] == Color.Magenta)
                    pixels[i] = Color.TransparentBlack;
            tex.SetData<Color>(pixels);
        }
        public AnimationInfo getAnimationInfo(string name)
        {
            foreach (AnimationInfo animInf in animations)
                if (animInf.name.Equals(name))
                    return animInf;
            return new AnimationInfo();
        }
    }
    public struct AnimationInfo
    {
        public string name;
        public int start, end, keyframe;
    }
}
