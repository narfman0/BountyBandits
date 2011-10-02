using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using BountyBandits.Inventory;

namespace BountyBandits.Graphics
{
    static class GraphicsHelper
    {
        public const float DEPTH_MULTIPLE = 42, DEPTH_X_OFFSET = 12;

        public static void drawGameItem(ref SpriteBatch spriteBatch, ref TextureManager texMan,
            GraphicsDevice gdv, ref Resolution res, ref float aveX, GameItem gameItem, 
            int currentDepth)
        {
            Vector2 pos = new Vector2(gameItem.body.Position.X - aveX + res.ScreenWidth / 2, gameItem.body.Position.Y);
            if (gameItem.name.Contains("log") || gameItem.name.Contains("circle"))
            {
                Texture2D tex = texMan.getTex("log");
                Vector2 scale = new Vector2((float)gameItem.radius * 2 / (float)tex.Width, (float)gameItem.radius * 2 / (float)tex.Width);
                Vector2 origin = new Vector2(tex.Width / 2, tex.Height / 2);
                drawItem(ref spriteBatch, ref res, tex, pos, -gameItem.body.Rotation, currentDepth, scale, SpriteEffects.None, origin);
            }
            else if (! (gameItem is DropItem) )
            {
                Texture2D tex =  texMan.getTex(gameItem.name);
                Vector2 scale = new Vector2((float)gameItem.radius / (float)tex.Width, (float)gameItem.radius / (float)tex.Width);
                Vector2 origin = new Vector2(tex.Width / 2, tex.Height / 2);
                drawItem(ref spriteBatch, ref res, tex, pos, -gameItem.body.Rotation, currentDepth, scale, SpriteEffects.None, origin);
            }
            if (gameItem is DropItem)
            {
                Item item = ((DropItem)gameItem).getItem();
                Texture2D texColored = texMan.getTexColored(item.getTextureName(), item.getPrimaryColor(), item.getSecondaryColor(), gdv);
                Vector2 origin = new Vector2(texMan.getTex(item.getTextureName()).Width / 2, texMan.getTex(item.getTextureName()).Height / 2);
                drawItem(ref spriteBatch, ref res, texColored, pos, gameItem.body.Rotation, currentDepth, Vector2.One, SpriteEffects.None, origin);
            }
        }

        public static void drawItem(ref SpriteBatch spriteBatch, ref Resolution res, 
            Texture2D tex, Vector2 pos, float rot, int depth, Vector2 scale, 
            SpriteEffects effects, Vector2 origin)
        {
            Vector2 relPos = new Vector2(pos.X - DEPTH_X_OFFSET * depth, res.ScreenHeight - pos.Y - (DEPTH_MULTIPLE * (3 - depth)));
            spriteBatch.Draw(tex, relPos, null, Color.White, rot, origin, scale, effects, ((float)depth) / 10.0f);
        }
    }
}