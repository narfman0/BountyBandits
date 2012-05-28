using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using BountyBandits.Stats;
using Microsoft.Xna.Framework;

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

        //hiding constructor because it is only fromXML
        private AnimationController() { }

        public Vector2 getFrameDimensions(int frame)
        {
            if (frames.Count > frame)
                return new Vector2(frames[frame].Width, frames[frame].Height);
            return new Vector2(128, 128);
        }

        public int PermutationCount { get {
            //int i = 1;
            //foreach (Color[] colors in permutations)
            //    i *= colors.Length;
            int i = 0;
            for (int permutationCount = 0; permutationCount < permutations.Count; permutationCount++)
                for (int dest = 0; dest < permutations[permutationCount].Length; dest++)
                    i++;
            return i;
        }}

        public static AnimationController fromXML(ContentManager content, string name)
        {
            AnimationController controller = new AnimationController();
            controller.name = name;
            string folderPath = @"Content\Beings\" + name + @"\";
            bool isUnitTest = !Directory.Exists(folderPath);
            if (isUnitTest)
                folderPath = @"..\..\..\BountyBandits\" + folderPath;
            string xmlPath = folderPath + name + ".xml";
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
                                else if (subnode.Name.Equals("slowIfTouchingGeom"))
                                    anim.slowIfTouchingGeom = bool.Parse(subnode.FirstChild.Value);
                                else if (subnode.Name.Equals("aoe"))
                                    anim.aoe = bool.Parse(subnode.FirstChild.Value);
                                else if (subnode.Name.Equals("force"))
                                {
                                    XmlElement forceEle = (XmlElement)subnode;
                                    bool isEnemy = forceEle.HasAttribute("isEnemy") && bool.Parse(forceEle.GetAttribute("isEnemy"));
                                    bool isSelf = forceEle.HasAttribute("isSelf") && bool.Parse(forceEle.GetAttribute("isSelf"));
                                    anim.forces.Add(new ForceFrame(
                                        int.Parse(forceEle.GetAttribute("frame")),
                                        XMLUtil.fromXMLVector2(subnode), isEnemy, isSelf));
                                }
                                else if (subnode.Name.Equals("targets"))
                                    anim.targets = int.Parse(subnode.FirstChild.Value);
                                else if (subnode.Name.Equals("stunDuration"))
                                    anim.stunDuration = int.Parse(subnode.FirstChild.Value);
                                else if (subnode.Name.Equals("projectileTexture"))
                                    anim.projectileTexture = subnode.FirstChild.Value;
                                else if (subnode.Name.Equals("projectileWeight"))
                                    anim.projectileWeight = float.Parse(subnode.FirstChild.Value);
                                else if (subnode.Name.Equals("dmgMultiplier"))
                                    anim.dmgMultiplier = float.Parse(subnode.FirstChild.Value);
                                else if (subnode.Name.Equals("teleport"))
                                    anim.teleport = XMLUtil.fromXMLVector2(subnode);
                            controller.animations.Add(anim);
                        }
                        if (!isUnitTest)
                        {
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
                                controller.frames.Add(newFrame);
                            }
                        }
                    } else if (monsterchild.Name == "stats")
                        foreach (XmlNode node in monsterchild.ChildNodes)
                        {
                            StatType statType = (StatType)Enum.Parse(typeof(StatType), node.Attributes.GetNamedItem("type").Value);
                            int value = int.Parse(node.Attributes.GetNamedItem("value").Value);
                            controller.statRatios.addStatValue(statType, value);
                        }
                    else if (monsterchild.Name == "colorPermutations")
                        foreach (XmlNode permutationNode in monsterchild.ChildNodes)
                        {
                            Color[] colors = new Color[permutationNode.ChildNodes.Count];
                            for (int colorIndex = 0; colorIndex < colors.Length; colorIndex++)
                                colors[colorIndex] = XMLUtil.fromXMLColor(permutationNode.ChildNodes.Item(colorIndex));
                            controller.permutations.Add(colors);
                        }
                }
                foreach (string str in Directory.GetFiles(folderPath))
                    if (str.Contains("portrait") && !isUnitTest)
                    {
                        controller.portrait = content.Load<Texture2D>(@"Beings\" + name + @"\portrait");
                        AnimationController.getAlphaFromTex(ref controller.portrait);
                        break;
                    }
            }
            return controller;
        }

        /// <summary>
        /// Replace magenta with transparent black (alphad)
        /// </summary>
        /// <param name="tex"></param>
        public static void getAlphaFromTex(ref Texture2D tex)
        {
            replaceColor(ref tex, Color.Magenta, Color.Transparent);
        }

        public static void replaceColor(ref Texture2D tex, Color src, Color dst)
        {
            Color[] pixels = new Color[tex.Width * tex.Height];
            tex.GetData<Color>(pixels);
            for (int i = 0; i < pixels.Length; i++)
                if (pixels[i] == src)
                    pixels[i] = dst;
            tex.SetData<Color>(pixels);
        }

        public AnimationInfo getAnimationInfo(string name)
        {
            foreach (AnimationInfo animInf in animations)
                if (animInf.name.Equals(name))
                    return animInf;
            return null;
        }
    }

    public class AnimationInfo
    {
        public string name, projectileTexture;
        public int start, end, keyframe, targets = 5, stunDuration;
        public bool slowIfTouchingGeom = true, aoe = false;
        public float dmgMultiplier = 1f, projectileWeight;
        public Vector2 teleport;
        public List<ForceFrame> forces = new List<ForceFrame>();

    }

    public class ForceFrame
    {
        public readonly int frame;
        public readonly Vector2 force;
        public readonly bool isEnemy, isSelf;

        public ForceFrame(int frame, Vector2 force, bool isEnemy, bool isSelf)
        {
            this.frame = frame;
            this.force = force;
            this.isEnemy = isEnemy;
            this.isSelf = isSelf;
        }
        public ForceFrame clone()
        {
            return new ForceFrame(frame, new Vector2(force.X, force.Y), isEnemy, isSelf);
        }
    }
}
