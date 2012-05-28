using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BountyBandits.Character
{
    public enum CombatTextType { HealthTaken, HealthAdded, StatusChanged, XPAdded }
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
        private static readonly long COMBAT_TEXT_DURATION = 1500, FADE_DURATION = 1000;
        private List<CombatText> items = new List<CombatText>();
        
        public void update()
        {
            while (items.Count > 0 && Environment.TickCount - items[0].time > COMBAT_TEXT_DURATION)
                items.RemoveAt(0);
        }

        public void draw(Vector2 drawPos, int depth)
        {
            for(int i=0; i<items.Count; i++)
            {
                CombatText text = items[i];
                Color color = Color.Black, borderColor = Color.Black;
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
                    case CombatTextType.XPAdded:
                        color = Color.Blue;
                        break;
                }
                float timeTilTextRemoved = COMBAT_TEXT_DURATION - (Environment.TickCount - items[0].time);
                if (timeTilTextRemoved < FADE_DURATION)
                {
                    byte alpha = (byte)((timeTilTextRemoved / (float)FADE_DURATION) * (float)byte.MaxValue);
                    color.A = alpha;
                    borderColor.A = alpha;
                }
                text.yOffset += (Environment.TickCount - text.time) / 20;
                Game.instance.currentState.getScreen().drawTextBorder(Game.instance.vademecumFont12, text.text, drawPos + new Vector2(0, text.yOffset), color, borderColor, depth);
            }
        }

        public void add(Guid guid, string text, CombatTextType type)
        {
            items.Add(new CombatText(Environment.TickCount, text, type));
            if(Game.instance.network != null)
                Game.instance.network.sendNewCombatText(guid, text, type);
        }
    }
}
