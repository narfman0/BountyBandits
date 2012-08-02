﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using BountyBandits.Map;

namespace BountyBandits.GameScreen.MapEditor
{
    public partial class Control : Form
    {
        private MapEditorScreen screen;
        private Vector2 currentLocation;

        public Control(MapEditorScreen screen)
        {
            currentLocation = new Vector2();
            this.screen = screen;
            InitializeComponent();
            setLevelInfo(screen.level);
        }

        public void setLevelInfo(Level level)
        {
            this.levelIndexBox.Text = level.number.ToString();
            this.locXBox.Text = level.loc.X.ToString();
            this.locYBox.Text = level.loc.Y.ToString();
            this.adjacentLevelsBox.Clear();
            this.prereqLevelsBox.Clear();
            this.levelNameBox.Text = level.name;
            this.autoProgressCheckBox.Checked = level.autoProgress;
            this.levelLengthBox.Text = screen.level.levelLength.ToString();
            for (int index = 0; index < level.prereq.Count; index++)
            {
                this.prereqLevelsBox.Text = this.prereqLevelsBox.Text + level.prereq[index];
                if (index + 1 != level.prereq.Count)
                    this.prereqLevelsBox.Text = this.prereqLevelsBox.Text+",";
            }
            for (int index = 0; index < level.adjacent.Count; index++)
            {
                this.adjacentLevelsBox.Text = this.adjacentLevelsBox.Text + level.adjacent[index];
                if (index + 1 != level.adjacent.Count)
                    this.adjacentLevelsBox.Text = this.adjacentLevelsBox.Text + ",";
            }
        }

        public void setCurrentPosition(Vector2 currentLocation)
        {
            this.currentLocation = currentLocation;
            this.currentPosTextLabel.Text = currentLocation.X + "x   " + currentLocation.Y + "y";
        }
        
        private string filechooserPicture(){
            OpenFileDialog openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            openFileDialog1.DefaultExt = "jpg";
            openFileDialog1.Filter = "picture files (*.jpg)|*.jpg";
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                return openFileDialog1.SafeFileName;
            }
            return null;
        }

        private void saveButton_Click(object sender, EventArgs e)
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

            screen.level.adjacent = adjacent;
            screen.level.loc = new Vector2(float.Parse(this.locXBox.Text), float.Parse(this.locYBox.Text));
            screen.level.prereq = prereq;
            screen.level.name = levelNameBox.Text;
            screen.level.number = int.Parse(levelIndexBox.Text);
            screen.level.autoProgress = autoProgressCheckBox.Checked;

            Game.instance.mapManager.removeLevel(Game.instance.mapManager.currentLevelIndex);
            Game.instance.mapManager.addLevel(screen.level);
            Game.instance.mapManager.saveCampaign(Game.instance.mapManager.currentCampaignPath);
        }

        private void enemySpawnButton_Click(object sender, EventArgs e)
        {
            try
            {
                string[] locStr = enemyTriggerLocationTextbox.Text.Split(',');
                SpawnPoint spawn = new SpawnPoint();
                spawn.count = uint.Parse(enemyCountBox.Text);
                spawn.type = enemyTypeText.Text;
                spawn.weight = uint.Parse(enemyLevelTextfield.Text);
                spawn.loc = currentLocation;
                spawn.triggerLocation = new Vector2(float.Parse(locStr[0]), float.Parse(locStr[1]));
                spawn.triggerWidth = uint.Parse(enemyTriggerWidthTextbox.Text);
                screen.level.spawns.Add(spawn);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception from spawn point:\n" + ex.StackTrace);
            }
        }

        private void itemSpawnButton_Click(object sender, EventArgs e)
        {
            try
            {
                GameItem item = new GameItem();
                item.name = this.itemTextureText.Text;
                item.polygonType = (PhysicsPolygonType)Enum.Parse(typeof(PhysicsPolygonType), this.itemPolygonType.Text);
                if (item.polygonType == PhysicsPolygonType.Circle)
                    item.radius = uint.Parse(this.itemRadiusText.Text);
                else if(item.polygonType == PhysicsPolygonType.Rectangle)
                {
                    String[] sideLengths = this.itemRadiusText.Text.Split(',');
                    item.sideLengths = new Vector2(int.Parse(sideLengths[0]), int.Parse(sideLengths[1]));
                }
                item.immovable = this.itemImmovableBox.Checked;
                item.startdepth = (uint)this.itemDepthSlider.Value;
                item.weight = uint.Parse(this.itemWeightBox.Text);
                item.width = (uint)this.itemWidthSlider.Value;
                item.rotation = float.Parse(this.itemRotationTextBox.Text);
                item.loc = currentLocation;
                screen.level.items.Add(item);
                Game.instance.addGameItem(item);
            }
            catch (Exception exceptionSpawnItem)
            {
                MessageBox.Show("Exception from game item:\n" + exceptionSpawnItem.StackTrace);
            }
        }

        private void itemPolygonType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (itemPolygonType.Text == "Rectangle")
            {
                itemRadiusLabel.Text = "Side Lengths (x,y):";
                itemRadiusText.Text = "10,10";
            }
            else
            {
                itemRadiusLabel.Text = "Radius:";
                itemRadiusText.Text = "10";
            }
        }

        private void backgroundSpawnButton_Click(object sender, EventArgs e)
        {
            BackgroundItemStruct str = new BackgroundItemStruct();
            str.texturePath = backgroundTextureLabel.Text;
            str.location = currentLocation;
            str.rotation = float.Parse(backgroundRotationText.Text);
            str.scale = float.Parse(backgroundScaleField.Text);
            screen.level.backgroundItems.Add(str);
        }

        public void setGuiControls(GameItem item)
        {
            itemPolygonType.SelectedItem = item.polygonType.ToString();
            itemTextureText.Text = item.name;
            itemWeightBox.Text = item.weight.ToString();
            itemRadiusText.Text = item.polygonType == PhysicsPolygonType.Circle ? item.radius.ToString() : 
                item.sideLengths.X.ToString() + "," + item.sideLengths.Y.ToString();
            itemRotationTextBox.Text = item.rotation.ToString();
            itemDepthSlider.Value = (int)item.startdepth;
            itemWidthSlider.Value = (int)item.width;
            itemImmovableBox.Checked = item.immovable;
        }

        public void setGuiControls(SpawnPoint spawn)
        {
            enemyCountBox.Text = spawn.count.ToString();
            enemyTypeText.Text = spawn.type.ToString();
            enemyLevelTextfield.Text = spawn.weight.ToString();
        }

        public void setGuiControls(BackgroundItemStruct backgroundItemStruct)
        {
            backgroundRotationText.Text = backgroundItemStruct.rotation.ToString();
            backgroundScaleField.Text = backgroundItemStruct.scale.ToString();
            backgroundTextureField.Text = backgroundItemStruct.texturePath;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            screen.exitMapEditor();
        }

        private void backgroundButton_Click(object sender, EventArgs e)
        {
            screen.level.horizon = Game.instance.texMan.getTex(filechooserPicture());
        }

        public int getLevelLength()
        {
            return int.Parse(levelLengthBox.Text);
        }
    }
}
