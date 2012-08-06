using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using BountyBandits.Map;

namespace BountyBandits.GameScreen.Controls
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
            levelInfoPanel.setEnabled(true);
            itemTextureBox.Items.AddRange(Game.instance.texMan.getSortedTextureNames());
            itemTextureBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            itemTextureBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
            itemTextureBox.AutoCompleteCustomSource.AddRange(Game.instance.texMan.getSortedTextureNames());
            levelInfoPanel.setLevelInfo(screen.level);
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
            levelInfoPanel.setUIInfo(screen.level);
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
                item.name = this.itemTextureBox.Text;
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

        public void setGuiControls(IMovableItem movableItem)
        {
            if (movableItem is BackgroundItemStruct)
            {
                BackgroundItemStruct backgroundItemStruct = (BackgroundItemStruct)movableItem;
                backgroundRotationText.Text = backgroundItemStruct.rotation.ToString();
                backgroundScaleField.Text = backgroundItemStruct.scale.ToString();
                backgroundTextureField.Text = backgroundItemStruct.texturePath;
            }
            if (movableItem is GameItem)
            {
                GameItem item = (GameItem)movableItem;
                itemPolygonType.SelectedItem = item.polygonType.ToString();
                itemTextureBox.Text = item.name;
                itemWeightBox.Text = item.weight.ToString();
                itemRadiusText.Text = item.polygonType == PhysicsPolygonType.Circle ? item.radius.ToString() :
                    item.sideLengths.X.ToString() + "," + item.sideLengths.Y.ToString();
                itemRotationTextBox.Text = item.rotation.ToString();
                itemDepthSlider.Value = (int)item.startdepth;
                itemWidthSlider.Value = (int)item.width;
                itemImmovableBox.Checked = item.immovable;
            }
            if (movableItem is SpawnPoint)
            {
                SpawnPoint spawn = (SpawnPoint)movableItem;
                enemyCountBox.Text = spawn.count.ToString();
                enemyTypeText.Text = spawn.type.ToString();
                enemyLevelTextfield.Text = spawn.weight.ToString();
            }
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

        private void physicsEnabledBox_CheckedChanged(object sender, EventArgs e)
        {
            screen.physicsEnabled = physicsEnabledBox.Checked;
        }

        public void setPhysicsEnabled(bool enabled)
        {
            physicsEnabledBox.Checked = enabled;
        }
    }
}
