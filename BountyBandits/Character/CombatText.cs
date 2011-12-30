using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BountyBandits.Character
{
    public enum CombatTextType { HealthTaken, HealthAdded, StatusChanged }
    public struct CombatText
    {
        public long time, yOffset;
        public string text;
        public CombatTextType type;
        public CombatText(long time, string text, CombatTextType type)
        {
            this.time = time;
            this.text = text;
            this.type = type;
            yOffset = 0;
        }
    }

    public class CombatTextManager
    {
        private static long COMBAT_TEXT_DURATION = 2000;
        private List<CombatText> items = new List<CombatText>();
        
        public void update()
        {
            while (items.Count > 0 && Environment.TickCount - items[0].time > COMBAT_TEXT_DURATION)
                items.RemoveAt(0);
        }

        public void draw(Game gameref, Vector2 drawPos, int depth)
        {
            for(int i=0; i<items.Count; i++)
            {
                CombatText text = items[i];
                Color color = Color.Black;
                switch (text.type)
                {
                    case CombatTextType.HealthAdded:
                        color = Color.Green;
                        break;
                    case CombatTextType.HealthTaken:
                        color = Color.Red;
                        break;
                    case CombatTextType.StatusChanged:
                        color = Color.LightGray;
                        break;
                }
                text.yOffset += (Environment.TickCount - text.time) / 20;
                gameref.drawTextBorder(gameref.vademecumFont12, text.text, drawPos + new Vector2(0, text.yOffset), color, Color.Black, depth);
            }
        }

        public void add(Guid guid, string text, CombatTextType type)
        {
            items.Add(new CombatText(Environment.TickCount, text, type));
            Game.instance.network.sendNewCombatText(guid, text, type);
        }
    }
}
