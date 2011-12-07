﻿using System;
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
            byte[] byteArr = new byte[textures[name].Width * textures[name].Height * 4];
            textures[name].GetData(byteArr);
            for (int texel = 0; texel < byteArr.Length; texel++)
            {
                //if it is all alpha, quit
                if (byteArr[texel + 3] == 255)
                {
                    if (byteArr[texel] == 255)
                    {
                        byteArr[texel++] = primary.R;
                        byteArr[texel++] = primary.G;
                        byteArr[texel++] = primary.B;
                        byteArr[texel] = primary.A;
                    }
                    else if (byteArr[texel] == 0)
                    {
                        byteArr[texel++] = secondary.R;
                        byteArr[texel++] = secondary.G;
                        byteArr[texel++] = secondary.B;
                        byteArr[texel] = secondary.A;
                    }
                }
                else
                    texel += 3;
            }
            Texture2D newTex = new Texture2D(gdv, textures[name].Width, textures[name].Height);
            newTex.SetData(byteArr);
            textures.Add(name + primary.GetHashCode().ToString() + secondary.GetHashCode().ToString(), newTex);
            return newTex;
        }
    }
}
