using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace BountyBandits
{
    public class TextureManager
    {
        readonly String[] textureDirectories = {@"Content\Textures\", @"Content\Campaigns\"};
        Dictionary<String, Texture2D> textures = new Dictionary<String, Texture2D>();
        private const float DEFAULT_DIMENSION = 64f;

        public TextureManager(ContentManager content)
        {
            foreach(String textureDirectory in textureDirectories)
                addTextureDirectory(content, textureDirectory);
        }
        private void addTextureDirectory(ContentManager content, string path)
        {
            string[] fileEntries = Directory.GetFiles(path);
            foreach (string fileName in fileEntries)
            {
                string name = fileName.Split('\\')[fileName.Split('\\').Length - 1].Split('.')[0];
                try
                {
                    textures.Add(name, content.Load<Texture2D>(path.Substring(8) + name));
                }
                catch (Exception e) { 
                    Console.WriteLine(e.StackTrace); 
                }
            }
            string[] dirs = Directory.GetDirectories(path);
            foreach (string dir in Directory.GetDirectories(path))
                addTextureDirectory(content, dir+@"\");
        }
        public Texture2D getTex(string name)
        {
            return textures[name];
        }

        public Texture2D getTexColored(string name, Color primary, Color secondary, GraphicsDevice gdv)
        {
            if (textures.ContainsKey(name + primary.GetHashCode().ToString() + secondary.GetHashCode().ToString()))
                return textures[name + primary.GetHashCode().ToString() + secondary.GetHashCode().ToString()];
            //create new texture by copying data, then changing black -> secondary and white -> primary
            Color[] colors = new Color[textures[name].Width * textures[name].Height];
            textures[name].GetData(colors);
            for (int texel = 0; texel < colors.Length; texel++)
            {
                if (colors[texel] == Color.White)
                    colors[texel] = primary;
                else if (colors[texel] == Color.Black)
                    colors[texel] = secondary;
            }
            Texture2D newTex = new Texture2D(gdv, textures[name].Width, textures[name].Height);
            newTex.SetData(colors);
            textures.Add(name + primary.GetHashCode().ToString() + secondary.GetHashCode().ToString(), newTex);
            return newTex;
        }

        public string[] getSortedTextureNames()
        {
            string[] names = new List<string>(textures.Keys).ToArray();
            Array.Sort<string>(names);
            return names;
        }

        public static Vector2 getDimensions(string texName)
        {
            Texture2D tex = Game.instance.texMan.getTex(texName);
            return new Vector2(tex != null ? tex.Width : DEFAULT_DIMENSION, tex != null ? tex.Height : DEFAULT_DIMENSION);
        }
    }
}
