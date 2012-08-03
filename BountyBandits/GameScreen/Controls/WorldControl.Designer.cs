namespace BountyBandits.GameScreen.Controls
{
    partial class WorldControl
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.backgroundLabel = new System.Windows.Forms.Label();
            this.backgroundTextBox = new System.Windows.Forms.TextBox();
            this.newButton = new System.Windows.Forms.Button();
            this.openButton = new System.Windows.Forms.Button();
            this.saveButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.levelInfoPanel = new BountyBandits.GameScreen.Controls.LevelInfoPanel();
            this.acceptButton = new System.Windows.Forms.Button();
            this.createButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // backgroundLabel
            // 
            this.backgroundLabel.AutoSize = true;
            this.backgroundLabel.Location = new System.Drawing.Point(12, 212);
            this.backgroundLabel.Name = "backgroundLabel";
            this.backgroundLabel.Size = new System.Drawing.Size(68, 13);
            this.backgroundLabel.TabIndex = 23;
            this.backgroundLabel.Text = "Background:";
            // 
            // backgroundTextBox
            // 
            this.backgroundTextBox.Location = new System.Drawing.Point(106, 212);
            this.backgroundTextBox.Name = "backgroundTextBox";
            this.backgroundTextBox.Size = new System.Drawing.Size(169, 20);
            this.backgroundTextBox.TabIndex = 24;
            this.backgroundTextBox.Text = "<select background>";
            // 
            // newButton
            // 
            this.newButton.Location = new System.Drawing.Point(13, 13);
            this.newButton.Name = "newButton";
            this.newButton.Size = new System.Drawing.Size(60, 23);
            this.newButton.TabIndex = 25;
            this.newButton.Text = "New...";
            this.newButton.UseVisualStyleBackColor = true;
            this.newButton.Click += new System.EventHandler(this.newButton_Click);
            // 
            // openButton
            // 
            this.openButton.Location = new System.Drawing.Point(79, 12);
            this.openButton.Name = "openButton";
            this.openButton.Size = new System.Drawing.Size(60, 23);
            this.openButton.TabIndex = 26;
            this.openButton.Text = "Open...";
            this.openButton.UseVisualStyleBackColor = true;
            this.openButton.Click += new System.EventHandler(this.openButton_Click);
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(145, 13);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(60, 23);
            this.saveButton.TabIndex = 27;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(211, 12);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(60, 23);
            this.cancelButton.TabIndex = 28;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // levelInfoPanel
            // 
            this.levelInfoPanel.Location = new System.Drawing.Point(1, 42);
            this.levelInfoPanel.Name = "levelInfoPanel";
            this.levelInfoPanel.Size = new System.Drawing.Size(274, 139);
            this.levelInfoPanel.TabIndex = 44;
            // 
            // acceptButton
            // 
            this.acceptButton.Location = new System.Drawing.Point(196, 183);
            this.acceptButton.Name = "acceptButton";
            this.acceptButton.Size = new System.Drawing.Size(75, 23);
            this.acceptButton.TabIndex = 46;
            this.acceptButton.Text = "Accept";
            this.acceptButton.UseVisualStyleBackColor = true;
            this.acceptButton.Click += new System.EventHandler(this.acceptButton_Click);
            // 
            // createButton
            // 
            this.createButton.Location = new System.Drawing.Point(106, 183);
            this.createButton.Name = "createButton";
            this.createButton.Size = new System.Drawing.Size(83, 23);
            this.createButton.TabIndex = 45;
            this.createButton.Text = "Create Level";
            this.createButton.UseVisualStyleBackColor = true;
            this.createButton.Click += new System.EventHandler(this.createButton_Click);
            // 
            // WorldControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 241);
            this.Controls.Add(this.acceptButton);
            this.Controls.Add(this.createButton);
            this.Controls.Add(this.levelInfoPanel);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.openButton);
            this.Controls.Add(this.newButton);
            this.Controls.Add(this.backgroundTextBox);
            this.Controls.Add(this.backgroundLabel);
            this.Name = "WorldControl";
            this.Text = "WorldControl";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label backgroundLabel;
        private System.Windows.Forms.TextBox backgroundTextBox;
        private System.Windows.Forms.Button newButton;
        private System.Windows.Forms.Button openButton;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Button cancelButton;
        private LevelInfoPanel levelInfoPanel;
        private System.Windows.Forms.Button acceptButton;
        private System.Windows.Forms.Button createButton;
    }
}