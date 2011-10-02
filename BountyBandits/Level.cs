using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BountyBandits
{
    public class Level
    {
        #region Map editor relevant
        public int number = -1;
        public string name = "default";
        public List<int> adjacent = new List<int>();
        public List<int> prereq = new List<int>();
        public Vector2 loc = Vector2.Zero;
        #endregion

        #region In game specific
        public Texture2D background, horizon;
        public List<GameItem> items = new List<GameItem>();
        public List<SpawnPoint> spawns = new List<SpawnPoint>();
        #endregion

    }
}
