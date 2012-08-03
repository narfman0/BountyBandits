using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using BountyBandits.Map;

namespace BountyBandits.GameScreen.Controls
{
    public partial class LevelInfoPanel : UserControl
    {
        public LevelInfoPanel()
        {
            InitializeComponent();
        }

        public void setLocation(Vector2 location)
        {
            this.locXBox.Text = location.X.ToString();
            this.locYBox.Text = location.Y.ToString();
        }

        public void setLevelInfo(Level level)
        {
            setLocation(level.loc);
            this.levelIndexBox.Text = level.number.ToString();
            this.adjacentLevelsBox.Clear();
            this.prereqLevelsBox.Clear();
            this.levelNameBox.Text = level.name;
            for (int index = 0; index < level.prereq.Count; index++)
            {
                this.prereqLevelsBox.Text = this.prereqLevelsBox.Text + level.prereq[index];
                if (index + 1 != level.prereq.Count)
                    this.prereqLevelsBox.Text = this.prereqLevelsBox.Text + ",";
            }
            for (int index = 0; index < level.adjacent.Count; index++)
            {
                this.adjacentLevelsBox.Text = this.adjacentLevelsBox.Text + level.adjacent[index];
                if (index + 1 != level.adjacent.Count)
                    this.adjacentLevelsBox.Text = this.adjacentLevelsBox.Text + ",";
            }
        }

        public void setUIInfo(Level level)
        {
            List<int> adjacent = new List<int>();
            string[] adjStrings = this.adjacentLevelsBox.Text.Split(',');
            foreach (string adjstr in adjStrings)
                if (adjstr != "")
                    adjacent.Add(int.Parse(adjstr));

            List<int> prereq = new List<int>();
            string[] prereqStrings = this.prereqLevelsBox.Text.Split(',');
            foreach (string prereqstr in prereqStrings)
                if (prereqstr != "")
                    prereq.Add(int.Parse(prereqstr));

            level.adjacent = adjacent;
            level.loc = new Vector2(float.Parse(this.locXBox.Text), float.Parse(this.locYBox.Text));
            level.prereq = prereq;
            level.name = levelNameBox.Text;
            level.number = int.Parse(levelIndexBox.Text);
        }

        public void setEnabled(bool enabled)
        {
            prereqLevelsBox.Enabled = levelNameBox.Enabled = adjacentLevelsBox.Enabled =
                   locXBox.Enabled = locYBox.Enabled = levelIndexBox.Enabled = enabled;
        }

        public int getLevelIndex()
        {
            return int.Parse(levelIndexBox.Text);
        }
    }
}
