using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BountyBandits.Map;
using Microsoft.Xna.Framework;

namespace BountyBandits.GameScreen.Controls
{
    public partial class WorldControl : Form
    {
        private WorldEditorScreen worldEditorScreen;

        public WorldControl(WorldEditorScreen worldEditorScreen)
        {
            this.worldEditorScreen = worldEditorScreen;
            InitializeComponent();
        }

        private void newButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.RootFolder = Environment.SpecialFolder.MyComputer;
            dlg.SelectedPath = Game.instance.Content.RootDirectory;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Game.instance.mapManager.currentCampaignPath = dlg.SelectedPath;
                Game.instance.mapManager.getLevels().Clear();
            }
        }

        private void openButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.RootFolder = Environment.SpecialFolder.MyComputer;
            dlg.SelectedPath = Game.instance.Content.RootDirectory;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Game.instance.mapManager.currentCampaignPath = dlg.SelectedPath;
                Game.instance.mapManager.loadCampaign(dlg.SelectedPath);
            }
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            Game.instance.mapManager.saveCampaign(Game.instance.mapManager.currentCampaignPath);
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            worldEditorScreen.exitMapEditor();
        }

        private void createButton_Click(object sender, EventArgs e)
        {
            levelInfoPanel.setEnabled(true);
            Level level  = new Level();
            level.number = Game.instance.mapManager.currentLevelIndex = Game.instance.mapManager.getNextUnusedLevelIndex();
            Game.instance.mapManager.addLevel(level);
            levelInfoPanel.setLevelInfo(level);
        }

        private void acceptButton_Click(object sender, EventArgs e)
        {
            Level level = Game.instance.mapManager.getCurrentLevel();
            levelInfoPanel.setUIInfo(level);
            levelInfoPanel.setEnabled(false);
        }

        private void levelNameBox_TextChanged(object sender, EventArgs e)
        {
            int newIndex = levelInfoPanel.getLevelIndex();
            Game.instance.mapManager.currentLevelIndex = newIndex;
            levelInfoPanel.setLevelInfo(Game.instance.mapManager.getCurrentLevel());
        }

        public void setLocation(Vector2 location)
        {
            levelInfoPanel.setLocation(location);
        }
    }
}
