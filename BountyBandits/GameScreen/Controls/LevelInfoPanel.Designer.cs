namespace BountyBandits.GameScreen.Controls
{
    partial class LevelInfoPanel
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.levelIndexLabel = new System.Windows.Forms.Label();
            this.levelIndexBox = new System.Windows.Forms.TextBox();
            this.locationLabel = new System.Windows.Forms.Label();
            this.locXBox = new System.Windows.Forms.TextBox();
            this.locYBox = new System.Windows.Forms.TextBox();
            this.prereqLevelsLabel = new System.Windows.Forms.Label();
            this.prereqLevelsBox = new System.Windows.Forms.TextBox();
            this.adjacentLevelsBox = new System.Windows.Forms.TextBox();
            this.adjacentLevelsLabel = new System.Windows.Forms.Label();
            this.levelNameBox = new System.Windows.Forms.TextBox();
            this.selectedLevelLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // levelIndexLabel
            // 
            this.levelIndexLabel.AutoSize = true;
            this.levelIndexLabel.Location = new System.Drawing.Point(6, 113);
            this.levelIndexLabel.Name = "levelIndexLabel";
            this.levelIndexLabel.Size = new System.Drawing.Size(65, 13);
            this.levelIndexLabel.TabIndex = 52;
            this.levelIndexLabel.Text = "Level Index:";
            // 
            // levelIndexBox
            // 
            this.levelIndexBox.Enabled = false;
            this.levelIndexBox.Location = new System.Drawing.Point(97, 110);
            this.levelIndexBox.Name = "levelIndexBox";
            this.levelIndexBox.Size = new System.Drawing.Size(84, 20);
            this.levelIndexBox.TabIndex = 51;
            // 
            // locationLabel
            // 
            this.locationLabel.AutoSize = true;
            this.locationLabel.Location = new System.Drawing.Point(6, 87);
            this.locationLabel.Name = "locationLabel";
            this.locationLabel.Size = new System.Drawing.Size(51, 13);
            this.locationLabel.TabIndex = 50;
            this.locationLabel.Text = "Location:";
            // 
            // locXBox
            // 
            this.locXBox.Enabled = false;
            this.locXBox.Location = new System.Drawing.Point(97, 84);
            this.locXBox.Name = "locXBox";
            this.locXBox.Size = new System.Drawing.Size(84, 20);
            this.locXBox.TabIndex = 49;
            // 
            // locYBox
            // 
            this.locYBox.Enabled = false;
            this.locYBox.Location = new System.Drawing.Point(187, 84);
            this.locYBox.Name = "locYBox";
            this.locYBox.Size = new System.Drawing.Size(79, 20);
            this.locYBox.TabIndex = 48;
            // 
            // prereqLevelsLabel
            // 
            this.prereqLevelsLabel.AutoSize = true;
            this.prereqLevelsLabel.Location = new System.Drawing.Point(6, 61);
            this.prereqLevelsLabel.Name = "prereqLevelsLabel";
            this.prereqLevelsLabel.Size = new System.Drawing.Size(75, 13);
            this.prereqLevelsLabel.TabIndex = 47;
            this.prereqLevelsLabel.Text = "Prereq Levels:";
            // 
            // prereqLevelsBox
            // 
            this.prereqLevelsBox.Enabled = false;
            this.prereqLevelsBox.Location = new System.Drawing.Point(97, 58);
            this.prereqLevelsBox.Name = "prereqLevelsBox";
            this.prereqLevelsBox.Size = new System.Drawing.Size(169, 20);
            this.prereqLevelsBox.TabIndex = 46;
            // 
            // adjacentLevelsBox
            // 
            this.adjacentLevelsBox.Enabled = false;
            this.adjacentLevelsBox.Location = new System.Drawing.Point(97, 32);
            this.adjacentLevelsBox.Name = "adjacentLevelsBox";
            this.adjacentLevelsBox.Size = new System.Drawing.Size(169, 20);
            this.adjacentLevelsBox.TabIndex = 45;
            // 
            // adjacentLevelsLabel
            // 
            this.adjacentLevelsLabel.AutoSize = true;
            this.adjacentLevelsLabel.Location = new System.Drawing.Point(6, 35);
            this.adjacentLevelsLabel.Name = "adjacentLevelsLabel";
            this.adjacentLevelsLabel.Size = new System.Drawing.Size(86, 13);
            this.adjacentLevelsLabel.TabIndex = 44;
            this.adjacentLevelsLabel.Text = "Adjacent Levels:";
            // 
            // levelNameBox
            // 
            this.levelNameBox.Enabled = false;
            this.levelNameBox.Location = new System.Drawing.Point(97, 6);
            this.levelNameBox.Name = "levelNameBox";
            this.levelNameBox.Size = new System.Drawing.Size(169, 20);
            this.levelNameBox.TabIndex = 43;
            // 
            // selectedLevelLabel
            // 
            this.selectedLevelLabel.AutoSize = true;
            this.selectedLevelLabel.Location = new System.Drawing.Point(6, 9);
            this.selectedLevelLabel.Name = "selectedLevelLabel";
            this.selectedLevelLabel.Size = new System.Drawing.Size(81, 13);
            this.selectedLevelLabel.TabIndex = 42;
            this.selectedLevelLabel.Text = "Selected Level:";
            // 
            // LevelInfoPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.levelIndexLabel);
            this.Controls.Add(this.levelIndexBox);
            this.Controls.Add(this.locationLabel);
            this.Controls.Add(this.locXBox);
            this.Controls.Add(this.locYBox);
            this.Controls.Add(this.prereqLevelsLabel);
            this.Controls.Add(this.prereqLevelsBox);
            this.Controls.Add(this.adjacentLevelsBox);
            this.Controls.Add(this.adjacentLevelsLabel);
            this.Controls.Add(this.levelNameBox);
            this.Controls.Add(this.selectedLevelLabel);
            this.Name = "LevelInfoPanel";
            this.Size = new System.Drawing.Size(274, 139);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label levelIndexLabel;
        private System.Windows.Forms.TextBox levelIndexBox;
        private System.Windows.Forms.Label locationLabel;
        private System.Windows.Forms.TextBox locXBox;
        private System.Windows.Forms.TextBox locYBox;
        private System.Windows.Forms.Label prereqLevelsLabel;
        private System.Windows.Forms.TextBox prereqLevelsBox;
        private System.Windows.Forms.TextBox adjacentLevelsBox;
        private System.Windows.Forms.Label adjacentLevelsLabel;
        private System.Windows.Forms.TextBox levelNameBox;
        private System.Windows.Forms.Label selectedLevelLabel;
    }
}
