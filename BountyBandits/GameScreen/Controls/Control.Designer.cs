namespace BountyBandits.GameScreen.Controls
{
    partial class Control
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
            this.saveButton = new System.Windows.Forms.Button();
            this.levelEditorPanel = new System.Windows.Forms.Panel();
            this.physicsEnabledBox = new System.Windows.Forms.CheckBox();
            this.autoProgressCheckBox = new System.Windows.Forms.CheckBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.enemyTab = new System.Windows.Forms.TabPage();
            this.enemyTriggerWidthTextbox = new System.Windows.Forms.TextBox();
            this.enemyTriggerWidthLabel = new System.Windows.Forms.Label();
            this.enemyTriggerLocationTextbox = new System.Windows.Forms.TextBox();
            this.enemyTriggerLocationLabel = new System.Windows.Forms.Label();
            this.enemyCountLabel = new System.Windows.Forms.Label();
            this.enemyCountBox = new System.Windows.Forms.TextBox();
            this.enemySpawnButton = new System.Windows.Forms.Button();
            this.enemyTypeText = new System.Windows.Forms.TextBox();
            this.enemyTypeLabel = new System.Windows.Forms.Label();
            this.enemyLevelTextfield = new System.Windows.Forms.TextBox();
            this.enemyLevelLabel = new System.Windows.Forms.Label();
            this.itemTab = new System.Windows.Forms.TabPage();
            this.itemTextureBox = new System.Windows.Forms.ComboBox();
            this.itemWidthLabel = new System.Windows.Forms.Label();
            this.itemWidthSlider = new System.Windows.Forms.TrackBar();
            this.rotationLabel = new System.Windows.Forms.Label();
            this.itemRotationTextBox = new System.Windows.Forms.TextBox();
            this.itemDepthSlider = new System.Windows.Forms.TrackBar();
            this.itemSpawnButton = new System.Windows.Forms.Button();
            this.itemSpawnLabel = new System.Windows.Forms.Label();
            this.depthLabel = new System.Windows.Forms.Label();
            this.weightLabel = new System.Windows.Forms.Label();
            this.itemWeightBox = new System.Windows.Forms.TextBox();
            this.itemRadiusText = new System.Windows.Forms.TextBox();
            this.itemRadiusLabel = new System.Windows.Forms.Label();
            this.itemImmovableBox = new System.Windows.Forms.CheckBox();
            this.polygonTypeLabel = new System.Windows.Forms.Label();
            this.itemPolygonType = new System.Windows.Forms.ComboBox();
            this.backgroundTab = new System.Windows.Forms.TabPage();
            this.backgroundSpawnButton = new System.Windows.Forms.Button();
            this.backgroundScaleLabel = new System.Windows.Forms.Label();
            this.backgroundScaleField = new System.Windows.Forms.TextBox();
            this.backgroundRotationText = new System.Windows.Forms.TextBox();
            this.backgroundTextureField = new System.Windows.Forms.TextBox();
            this.backgroundRotationLabel = new System.Windows.Forms.Label();
            this.backgroundTextureLabel = new System.Windows.Forms.Label();
            this.levelLengthBox = new System.Windows.Forms.TextBox();
            this.levelLengthLabel = new System.Windows.Forms.Label();
            this.currentPosTextLabel = new System.Windows.Forms.Label();
            this.backgroundPictureLabel = new System.Windows.Forms.Label();
            this.backgroundButton = new System.Windows.Forms.Button();
            this.currentPosLabel = new System.Windows.Forms.Label();
            this.leveEditorTitleLabel = new System.Windows.Forms.Label();
            this.mapLevelTabControl = new System.Windows.Forms.TabControl();
            this.mapTab = new System.Windows.Forms.TabPage();
            this.levelInfoPanel = new BountyBandits.GameScreen.Controls.LevelInfoPanel();
            this.levelTab = new System.Windows.Forms.TabPage();
            this.cancelButton = new System.Windows.Forms.Button();
            this.levelEditorPanel.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.enemyTab.SuspendLayout();
            this.itemTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.itemWidthSlider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.itemDepthSlider)).BeginInit();
            this.backgroundTab.SuspendLayout();
            this.mapLevelTabControl.SuspendLayout();
            this.mapTab.SuspendLayout();
            this.levelTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(12, 12);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(75, 23);
            this.saveButton.TabIndex = 1;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // levelEditorPanel
            // 
            this.levelEditorPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.levelEditorPanel.Controls.Add(this.physicsEnabledBox);
            this.levelEditorPanel.Controls.Add(this.autoProgressCheckBox);
            this.levelEditorPanel.Controls.Add(this.tabControl1);
            this.levelEditorPanel.Controls.Add(this.levelLengthBox);
            this.levelEditorPanel.Controls.Add(this.levelLengthLabel);
            this.levelEditorPanel.Controls.Add(this.currentPosTextLabel);
            this.levelEditorPanel.Controls.Add(this.backgroundPictureLabel);
            this.levelEditorPanel.Controls.Add(this.backgroundButton);
            this.levelEditorPanel.Controls.Add(this.currentPosLabel);
            this.levelEditorPanel.Controls.Add(this.leveEditorTitleLabel);
            this.levelEditorPanel.Location = new System.Drawing.Point(6, 6);
            this.levelEditorPanel.Name = "levelEditorPanel";
            this.levelEditorPanel.Size = new System.Drawing.Size(314, 451);
            this.levelEditorPanel.TabIndex = 3;
            // 
            // physicsEnabledBox
            // 
            this.physicsEnabledBox.AutoSize = true;
            this.physicsEnabledBox.Location = new System.Drawing.Point(140, 84);
            this.physicsEnabledBox.Name = "physicsEnabledBox";
            this.physicsEnabledBox.Size = new System.Drawing.Size(104, 17);
            this.physicsEnabledBox.TabIndex = 19;
            this.physicsEnabledBox.Text = "Physics Enabled";
            this.physicsEnabledBox.UseVisualStyleBackColor = true;
            this.physicsEnabledBox.CheckedChanged += new System.EventHandler(this.physicsEnabledBox_CheckedChanged);
            // 
            // autoProgressCheckBox
            // 
            this.autoProgressCheckBox.AutoSize = true;
            this.autoProgressCheckBox.Location = new System.Drawing.Point(140, 4);
            this.autoProgressCheckBox.Name = "autoProgressCheckBox";
            this.autoProgressCheckBox.Size = new System.Drawing.Size(92, 17);
            this.autoProgressCheckBox.TabIndex = 18;
            this.autoProgressCheckBox.Text = "Auto Progress";
            this.autoProgressCheckBox.UseVisualStyleBackColor = true;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.enemyTab);
            this.tabControl1.Controls.Add(this.itemTab);
            this.tabControl1.Controls.Add(this.backgroundTab);
            this.tabControl1.Location = new System.Drawing.Point(18, 108);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(279, 312);
            this.tabControl1.TabIndex = 17;
            // 
            // enemyTab
            // 
            this.enemyTab.Controls.Add(this.enemyTriggerWidthTextbox);
            this.enemyTab.Controls.Add(this.enemyTriggerWidthLabel);
            this.enemyTab.Controls.Add(this.enemyTriggerLocationTextbox);
            this.enemyTab.Controls.Add(this.enemyTriggerLocationLabel);
            this.enemyTab.Controls.Add(this.enemyCountLabel);
            this.enemyTab.Controls.Add(this.enemyCountBox);
            this.enemyTab.Controls.Add(this.enemySpawnButton);
            this.enemyTab.Controls.Add(this.enemyTypeText);
            this.enemyTab.Controls.Add(this.enemyTypeLabel);
            this.enemyTab.Controls.Add(this.enemyLevelTextfield);
            this.enemyTab.Controls.Add(this.enemyLevelLabel);
            this.enemyTab.Location = new System.Drawing.Point(4, 22);
            this.enemyTab.Name = "enemyTab";
            this.enemyTab.Padding = new System.Windows.Forms.Padding(3);
            this.enemyTab.Size = new System.Drawing.Size(271, 286);
            this.enemyTab.TabIndex = 0;
            this.enemyTab.Text = "Enemies";
            this.enemyTab.UseVisualStyleBackColor = true;
            // 
            // enemyTriggerWidthTextbox
            // 
            this.enemyTriggerWidthTextbox.Location = new System.Drawing.Point(98, 113);
            this.enemyTriggerWidthTextbox.Name = "enemyTriggerWidthTextbox";
            this.enemyTriggerWidthTextbox.Size = new System.Drawing.Size(92, 20);
            this.enemyTriggerWidthTextbox.TabIndex = 47;
            this.enemyTriggerWidthTextbox.Text = "64";
            // 
            // enemyTriggerWidthLabel
            // 
            this.enemyTriggerWidthLabel.AutoSize = true;
            this.enemyTriggerWidthLabel.Location = new System.Drawing.Point(6, 112);
            this.enemyTriggerWidthLabel.Name = "enemyTriggerWidthLabel";
            this.enemyTriggerWidthLabel.Size = new System.Drawing.Size(74, 13);
            this.enemyTriggerWidthLabel.TabIndex = 46;
            this.enemyTriggerWidthLabel.Text = "Trigger Width:";
            // 
            // enemyTriggerLocationTextbox
            // 
            this.enemyTriggerLocationTextbox.Location = new System.Drawing.Point(98, 86);
            this.enemyTriggerLocationTextbox.Name = "enemyTriggerLocationTextbox";
            this.enemyTriggerLocationTextbox.Size = new System.Drawing.Size(92, 20);
            this.enemyTriggerLocationTextbox.TabIndex = 45;
            this.enemyTriggerLocationTextbox.Text = "100,1";
            // 
            // enemyTriggerLocationLabel
            // 
            this.enemyTriggerLocationLabel.AutoSize = true;
            this.enemyTriggerLocationLabel.Location = new System.Drawing.Point(6, 86);
            this.enemyTriggerLocationLabel.Name = "enemyTriggerLocationLabel";
            this.enemyTriggerLocationLabel.Size = new System.Drawing.Size(87, 13);
            this.enemyTriggerLocationLabel.TabIndex = 44;
            this.enemyTriggerLocationLabel.Text = "Trigger Location:";
            // 
            // enemyCountLabel
            // 
            this.enemyCountLabel.AutoSize = true;
            this.enemyCountLabel.Location = new System.Drawing.Point(6, 59);
            this.enemyCountLabel.Name = "enemyCountLabel";
            this.enemyCountLabel.Size = new System.Drawing.Size(38, 13);
            this.enemyCountLabel.TabIndex = 43;
            this.enemyCountLabel.Text = "Count:";
            // 
            // enemyCountBox
            // 
            this.enemyCountBox.Location = new System.Drawing.Point(98, 59);
            this.enemyCountBox.Name = "enemyCountBox";
            this.enemyCountBox.Size = new System.Drawing.Size(92, 20);
            this.enemyCountBox.TabIndex = 42;
            this.enemyCountBox.Text = "1";
            // 
            // enemySpawnButton
            // 
            this.enemySpawnButton.Location = new System.Drawing.Point(98, 139);
            this.enemySpawnButton.Name = "enemySpawnButton";
            this.enemySpawnButton.Size = new System.Drawing.Size(92, 23);
            this.enemySpawnButton.TabIndex = 41;
            this.enemySpawnButton.Text = "Spawn";
            this.enemySpawnButton.UseVisualStyleBackColor = true;
            this.enemySpawnButton.Click += new System.EventHandler(this.enemySpawnButton_Click);
            // 
            // enemyTypeText
            // 
            this.enemyTypeText.Location = new System.Drawing.Point(98, 6);
            this.enemyTypeText.Name = "enemyTypeText";
            this.enemyTypeText.Size = new System.Drawing.Size(92, 20);
            this.enemyTypeText.TabIndex = 29;
            this.enemyTypeText.Text = "governator";
            // 
            // enemyTypeLabel
            // 
            this.enemyTypeLabel.AutoSize = true;
            this.enemyTypeLabel.Location = new System.Drawing.Point(6, 9);
            this.enemyTypeLabel.Name = "enemyTypeLabel";
            this.enemyTypeLabel.Size = new System.Drawing.Size(37, 13);
            this.enemyTypeLabel.TabIndex = 29;
            this.enemyTypeLabel.Text = "Type: ";
            // 
            // enemyLevelTextfield
            // 
            this.enemyLevelTextfield.Location = new System.Drawing.Point(98, 32);
            this.enemyLevelTextfield.Name = "enemyLevelTextfield";
            this.enemyLevelTextfield.Size = new System.Drawing.Size(92, 20);
            this.enemyLevelTextfield.TabIndex = 29;
            this.enemyLevelTextfield.Text = "10";
            // 
            // enemyLevelLabel
            // 
            this.enemyLevelLabel.AutoSize = true;
            this.enemyLevelLabel.Location = new System.Drawing.Point(6, 32);
            this.enemyLevelLabel.Name = "enemyLevelLabel";
            this.enemyLevelLabel.Size = new System.Drawing.Size(36, 13);
            this.enemyLevelLabel.TabIndex = 26;
            this.enemyLevelLabel.Text = "Level:";
            // 
            // itemTab
            // 
            this.itemTab.Controls.Add(this.itemTextureBox);
            this.itemTab.Controls.Add(this.itemWidthLabel);
            this.itemTab.Controls.Add(this.itemWidthSlider);
            this.itemTab.Controls.Add(this.rotationLabel);
            this.itemTab.Controls.Add(this.itemRotationTextBox);
            this.itemTab.Controls.Add(this.itemDepthSlider);
            this.itemTab.Controls.Add(this.itemSpawnButton);
            this.itemTab.Controls.Add(this.itemSpawnLabel);
            this.itemTab.Controls.Add(this.depthLabel);
            this.itemTab.Controls.Add(this.weightLabel);
            this.itemTab.Controls.Add(this.itemWeightBox);
            this.itemTab.Controls.Add(this.itemRadiusText);
            this.itemTab.Controls.Add(this.itemRadiusLabel);
            this.itemTab.Controls.Add(this.itemImmovableBox);
            this.itemTab.Controls.Add(this.polygonTypeLabel);
            this.itemTab.Controls.Add(this.itemPolygonType);
            this.itemTab.Location = new System.Drawing.Point(4, 22);
            this.itemTab.Name = "itemTab";
            this.itemTab.Padding = new System.Windows.Forms.Padding(3);
            this.itemTab.Size = new System.Drawing.Size(271, 286);
            this.itemTab.TabIndex = 1;
            this.itemTab.Text = "Items";
            this.itemTab.UseVisualStyleBackColor = true;
            // 
            // itemTextureBox
            // 
            this.itemTextureBox.FormattingEnabled = true;
            this.itemTextureBox.Location = new System.Drawing.Point(142, 37);
            this.itemTextureBox.Name = "itemTextureBox";
            this.itemTextureBox.Size = new System.Drawing.Size(123, 21);
            this.itemTextureBox.Sorted = true;
            this.itemTextureBox.TabIndex = 46;
            this.itemTextureBox.Text = "log";
            // 
            // itemWidthLabel
            // 
            this.itemWidthLabel.AutoSize = true;
            this.itemWidthLabel.Location = new System.Drawing.Point(9, 200);
            this.itemWidthLabel.Name = "itemWidthLabel";
            this.itemWidthLabel.Size = new System.Drawing.Size(38, 13);
            this.itemWidthLabel.TabIndex = 45;
            this.itemWidthLabel.Text = "Width:";
            // 
            // itemWidthSlider
            // 
            this.itemWidthSlider.Location = new System.Drawing.Point(142, 200);
            this.itemWidthSlider.Maximum = 4;
            this.itemWidthSlider.Minimum = 1;
            this.itemWidthSlider.Name = "itemWidthSlider";
            this.itemWidthSlider.Size = new System.Drawing.Size(123, 45);
            this.itemWidthSlider.TabIndex = 44;
            this.itemWidthSlider.Value = 1;
            // 
            // rotationLabel
            // 
            this.rotationLabel.AutoSize = true;
            this.rotationLabel.Location = new System.Drawing.Point(7, 116);
            this.rotationLabel.Name = "rotationLabel";
            this.rotationLabel.Size = new System.Drawing.Size(50, 13);
            this.rotationLabel.TabIndex = 43;
            this.rotationLabel.Text = "Rotation:";
            // 
            // itemRotationTextBox
            // 
            this.itemRotationTextBox.Location = new System.Drawing.Point(142, 116);
            this.itemRotationTextBox.Name = "itemRotationTextBox";
            this.itemRotationTextBox.Size = new System.Drawing.Size(123, 20);
            this.itemRotationTextBox.TabIndex = 42;
            this.itemRotationTextBox.Text = "0";
            // 
            // itemDepthSlider
            // 
            this.itemDepthSlider.Location = new System.Drawing.Point(142, 151);
            this.itemDepthSlider.Maximum = 3;
            this.itemDepthSlider.Name = "itemDepthSlider";
            this.itemDepthSlider.Size = new System.Drawing.Size(123, 45);
            this.itemDepthSlider.TabIndex = 41;
            // 
            // itemSpawnButton
            // 
            this.itemSpawnButton.Location = new System.Drawing.Point(142, 251);
            this.itemSpawnButton.Name = "itemSpawnButton";
            this.itemSpawnButton.Size = new System.Drawing.Size(84, 23);
            this.itemSpawnButton.TabIndex = 40;
            this.itemSpawnButton.Text = "Spawn";
            this.itemSpawnButton.UseVisualStyleBackColor = true;
            this.itemSpawnButton.Click += new System.EventHandler(this.itemSpawnButton_Click);
            // 
            // itemSpawnLabel
            // 
            this.itemSpawnLabel.AutoSize = true;
            this.itemSpawnLabel.Location = new System.Drawing.Point(6, 37);
            this.itemSpawnLabel.Name = "itemSpawnLabel";
            this.itemSpawnLabel.Size = new System.Drawing.Size(69, 13);
            this.itemSpawnLabel.TabIndex = 38;
            this.itemSpawnLabel.Text = "Item Texture:";
            // 
            // depthLabel
            // 
            this.depthLabel.AutoSize = true;
            this.depthLabel.Location = new System.Drawing.Point(6, 151);
            this.depthLabel.Name = "depthLabel";
            this.depthLabel.Size = new System.Drawing.Size(64, 13);
            this.depthLabel.TabIndex = 36;
            this.depthLabel.Text = "Start Depth:";
            // 
            // weightLabel
            // 
            this.weightLabel.AutoSize = true;
            this.weightLabel.Location = new System.Drawing.Point(6, 63);
            this.weightLabel.Name = "weightLabel";
            this.weightLabel.Size = new System.Drawing.Size(44, 13);
            this.weightLabel.TabIndex = 34;
            this.weightLabel.Text = "Weight:";
            // 
            // itemWeightBox
            // 
            this.itemWeightBox.Location = new System.Drawing.Point(142, 63);
            this.itemWeightBox.Name = "itemWeightBox";
            this.itemWeightBox.Size = new System.Drawing.Size(123, 20);
            this.itemWeightBox.TabIndex = 33;
            this.itemWeightBox.Text = "10";
            // 
            // itemRadiusText
            // 
            this.itemRadiusText.Location = new System.Drawing.Point(142, 89);
            this.itemRadiusText.Name = "itemRadiusText";
            this.itemRadiusText.Size = new System.Drawing.Size(123, 20);
            this.itemRadiusText.TabIndex = 26;
            this.itemRadiusText.Text = "30";
            // 
            // itemRadiusLabel
            // 
            this.itemRadiusLabel.AutoSize = true;
            this.itemRadiusLabel.Location = new System.Drawing.Point(6, 89);
            this.itemRadiusLabel.Name = "itemRadiusLabel";
            this.itemRadiusLabel.Size = new System.Drawing.Size(127, 13);
            this.itemRadiusLabel.TabIndex = 32;
            this.itemRadiusLabel.Text = "Radius / side length (x,y):";
            // 
            // itemImmovableBox
            // 
            this.itemImmovableBox.AutoSize = true;
            this.itemImmovableBox.Location = new System.Drawing.Point(9, 251);
            this.itemImmovableBox.Name = "itemImmovableBox";
            this.itemImmovableBox.Size = new System.Drawing.Size(77, 17);
            this.itemImmovableBox.TabIndex = 31;
            this.itemImmovableBox.Text = "Immovable";
            this.itemImmovableBox.UseVisualStyleBackColor = true;
            // 
            // polygonTypeLabel
            // 
            this.polygonTypeLabel.AutoSize = true;
            this.polygonTypeLabel.Location = new System.Drawing.Point(6, 10);
            this.polygonTypeLabel.Name = "polygonTypeLabel";
            this.polygonTypeLabel.Size = new System.Drawing.Size(75, 13);
            this.polygonTypeLabel.TabIndex = 30;
            this.polygonTypeLabel.Text = "Polygon Type:";
            // 
            // itemPolygonType
            // 
            this.itemPolygonType.FormattingEnabled = true;
            this.itemPolygonType.Items.AddRange(new object[] {
            "Rectangle",
            "Circle",
            "Polygon"});
            this.itemPolygonType.Location = new System.Drawing.Point(142, 10);
            this.itemPolygonType.Name = "itemPolygonType";
            this.itemPolygonType.Size = new System.Drawing.Size(123, 21);
            this.itemPolygonType.TabIndex = 29;
            this.itemPolygonType.Text = "Circle";
            this.itemPolygonType.SelectedIndexChanged += new System.EventHandler(this.itemPolygonType_SelectedIndexChanged);
            // 
            // backgroundTab
            // 
            this.backgroundTab.Controls.Add(this.backgroundSpawnButton);
            this.backgroundTab.Controls.Add(this.backgroundScaleLabel);
            this.backgroundTab.Controls.Add(this.backgroundScaleField);
            this.backgroundTab.Controls.Add(this.backgroundRotationText);
            this.backgroundTab.Controls.Add(this.backgroundTextureField);
            this.backgroundTab.Controls.Add(this.backgroundRotationLabel);
            this.backgroundTab.Controls.Add(this.backgroundTextureLabel);
            this.backgroundTab.Location = new System.Drawing.Point(4, 22);
            this.backgroundTab.Name = "backgroundTab";
            this.backgroundTab.Size = new System.Drawing.Size(271, 286);
            this.backgroundTab.TabIndex = 2;
            this.backgroundTab.Text = "Background";
            this.backgroundTab.UseVisualStyleBackColor = true;
            // 
            // backgroundSpawnButton
            // 
            this.backgroundSpawnButton.Location = new System.Drawing.Point(60, 82);
            this.backgroundSpawnButton.Name = "backgroundSpawnButton";
            this.backgroundSpawnButton.Size = new System.Drawing.Size(100, 23);
            this.backgroundSpawnButton.TabIndex = 42;
            this.backgroundSpawnButton.Text = "Spawn";
            this.backgroundSpawnButton.UseVisualStyleBackColor = true;
            this.backgroundSpawnButton.Click += new System.EventHandler(this.backgroundSpawnButton_Click);
            // 
            // backgroundScaleLabel
            // 
            this.backgroundScaleLabel.AutoSize = true;
            this.backgroundScaleLabel.Location = new System.Drawing.Point(3, 56);
            this.backgroundScaleLabel.Name = "backgroundScaleLabel";
            this.backgroundScaleLabel.Size = new System.Drawing.Size(37, 13);
            this.backgroundScaleLabel.TabIndex = 5;
            this.backgroundScaleLabel.Text = "Scale:";
            // 
            // backgroundScaleField
            // 
            this.backgroundScaleField.Location = new System.Drawing.Point(60, 56);
            this.backgroundScaleField.Name = "backgroundScaleField";
            this.backgroundScaleField.Size = new System.Drawing.Size(100, 20);
            this.backgroundScaleField.TabIndex = 4;
            this.backgroundScaleField.Text = "1";
            // 
            // backgroundRotationText
            // 
            this.backgroundRotationText.Location = new System.Drawing.Point(60, 30);
            this.backgroundRotationText.Name = "backgroundRotationText";
            this.backgroundRotationText.Size = new System.Drawing.Size(100, 20);
            this.backgroundRotationText.TabIndex = 3;
            this.backgroundRotationText.Text = "0";
            // 
            // backgroundTextureField
            // 
            this.backgroundTextureField.Location = new System.Drawing.Point(60, 4);
            this.backgroundTextureField.Name = "backgroundTextureField";
            this.backgroundTextureField.Size = new System.Drawing.Size(100, 20);
            this.backgroundTextureField.TabIndex = 2;
            this.backgroundTextureField.Text = "road";
            // 
            // backgroundRotationLabel
            // 
            this.backgroundRotationLabel.AutoSize = true;
            this.backgroundRotationLabel.Location = new System.Drawing.Point(3, 30);
            this.backgroundRotationLabel.Name = "backgroundRotationLabel";
            this.backgroundRotationLabel.Size = new System.Drawing.Size(50, 13);
            this.backgroundRotationLabel.TabIndex = 1;
            this.backgroundRotationLabel.Text = "Rotation:";
            // 
            // backgroundTextureLabel
            // 
            this.backgroundTextureLabel.AutoSize = true;
            this.backgroundTextureLabel.Location = new System.Drawing.Point(3, 4);
            this.backgroundTextureLabel.Name = "backgroundTextureLabel";
            this.backgroundTextureLabel.Size = new System.Drawing.Size(46, 13);
            this.backgroundTextureLabel.TabIndex = 0;
            this.backgroundTextureLabel.Text = "Texture:";
            // 
            // levelLengthBox
            // 
            this.levelLengthBox.Location = new System.Drawing.Point(48, 426);
            this.levelLengthBox.Name = "levelLengthBox";
            this.levelLengthBox.Size = new System.Drawing.Size(117, 20);
            this.levelLengthBox.TabIndex = 16;
            this.levelLengthBox.Text = "1000";
            // 
            // levelLengthLabel
            // 
            this.levelLengthLabel.AutoSize = true;
            this.levelLengthLabel.Location = new System.Drawing.Point(-1, 429);
            this.levelLengthLabel.Name = "levelLengthLabel";
            this.levelLengthLabel.Size = new System.Drawing.Size(43, 13);
            this.levelLengthLabel.TabIndex = 14;
            this.levelLengthLabel.Text = "Length:";
            // 
            // currentPosTextLabel
            // 
            this.currentPosTextLabel.AutoSize = true;
            this.currentPosTextLabel.Location = new System.Drawing.Point(137, 30);
            this.currentPosTextLabel.Name = "currentPosTextLabel";
            this.currentPosTextLabel.Size = new System.Drawing.Size(163, 13);
            this.currentPosTextLabel.TabIndex = 9;
            this.currentPosTextLabel.Text = "<Right click on map to populate>";
            // 
            // backgroundPictureLabel
            // 
            this.backgroundPictureLabel.AutoSize = true;
            this.backgroundPictureLabel.Location = new System.Drawing.Point(4, 59);
            this.backgroundPictureLabel.Name = "backgroundPictureLabel";
            this.backgroundPictureLabel.Size = new System.Drawing.Size(46, 13);
            this.backgroundPictureLabel.TabIndex = 5;
            this.backgroundPictureLabel.Text = "Horizon:";
            // 
            // backgroundButton
            // 
            this.backgroundButton.Location = new System.Drawing.Point(140, 54);
            this.backgroundButton.Name = "backgroundButton";
            this.backgroundButton.Size = new System.Drawing.Size(80, 23);
            this.backgroundButton.TabIndex = 4;
            this.backgroundButton.Text = "Browse...";
            this.backgroundButton.UseVisualStyleBackColor = true;
            this.backgroundButton.Click += new System.EventHandler(this.backgroundButton_Click);
            // 
            // currentPosLabel
            // 
            this.currentPosLabel.AutoSize = true;
            this.currentPosLabel.Location = new System.Drawing.Point(4, 30);
            this.currentPosLabel.Name = "currentPosLabel";
            this.currentPosLabel.Size = new System.Drawing.Size(84, 13);
            this.currentPosLabel.TabIndex = 1;
            this.currentPosLabel.Text = "Current Position:";
            // 
            // leveEditorTitleLabel
            // 
            this.leveEditorTitleLabel.AutoSize = true;
            this.leveEditorTitleLabel.Location = new System.Drawing.Point(4, 4);
            this.leveEditorTitleLabel.Name = "leveEditorTitleLabel";
            this.leveEditorTitleLabel.Size = new System.Drawing.Size(63, 13);
            this.leveEditorTitleLabel.TabIndex = 0;
            this.leveEditorTitleLabel.Text = "Level Editor";
            // 
            // mapLevelTabControl
            // 
            this.mapLevelTabControl.Controls.Add(this.mapTab);
            this.mapLevelTabControl.Controls.Add(this.levelTab);
            this.mapLevelTabControl.Location = new System.Drawing.Point(12, 42);
            this.mapLevelTabControl.Name = "mapLevelTabControl";
            this.mapLevelTabControl.SelectedIndex = 0;
            this.mapLevelTabControl.Size = new System.Drawing.Size(335, 489);
            this.mapLevelTabControl.TabIndex = 6;
            // 
            // mapTab
            // 
            this.mapTab.Controls.Add(this.levelInfoPanel);
            this.mapTab.Location = new System.Drawing.Point(4, 22);
            this.mapTab.Name = "mapTab";
            this.mapTab.Padding = new System.Windows.Forms.Padding(3);
            this.mapTab.Size = new System.Drawing.Size(327, 463);
            this.mapTab.TabIndex = 1;
            this.mapTab.Text = "Map";
            this.mapTab.UseVisualStyleBackColor = true;
            // 
            // levelInfoPanel
            // 
            this.levelInfoPanel.Location = new System.Drawing.Point(24, 6);
            this.levelInfoPanel.Name = "levelInfoPanel";
            this.levelInfoPanel.Size = new System.Drawing.Size(274, 139);
            this.levelInfoPanel.TabIndex = 0;
            // 
            // levelTab
            // 
            this.levelTab.Controls.Add(this.levelEditorPanel);
            this.levelTab.Location = new System.Drawing.Point(4, 22);
            this.levelTab.Name = "levelTab";
            this.levelTab.Padding = new System.Windows.Forms.Padding(3);
            this.levelTab.Size = new System.Drawing.Size(327, 463);
            this.levelTab.TabIndex = 0;
            this.levelTab.Text = "Level";
            this.levelTab.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(94, 12);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 7;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // Control
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(359, 543);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.mapLevelTabControl);
            this.Controls.Add(this.saveButton);
            this.Name = "Control";
            this.Text = "Control";
            this.levelEditorPanel.ResumeLayout(false);
            this.levelEditorPanel.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.enemyTab.ResumeLayout(false);
            this.enemyTab.PerformLayout();
            this.itemTab.ResumeLayout(false);
            this.itemTab.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.itemWidthSlider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.itemDepthSlider)).EndInit();
            this.backgroundTab.ResumeLayout(false);
            this.backgroundTab.PerformLayout();
            this.mapLevelTabControl.ResumeLayout(false);
            this.mapTab.ResumeLayout(false);
            this.levelTab.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Panel levelEditorPanel;
        private System.Windows.Forms.Label leveEditorTitleLabel;
        private System.Windows.Forms.Label currentPosLabel;
        private System.Windows.Forms.Label backgroundPictureLabel;
        private System.Windows.Forms.Button backgroundButton;
        private System.Windows.Forms.Label currentPosTextLabel;
        private System.Windows.Forms.Label levelLengthLabel;
        private System.Windows.Forms.TextBox levelLengthBox;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage enemyTab;
        private System.Windows.Forms.TextBox enemyTypeText;
        private System.Windows.Forms.Label enemyTypeLabel;
        private System.Windows.Forms.TextBox enemyLevelTextfield;
        private System.Windows.Forms.Label enemyLevelLabel;
        private System.Windows.Forms.TabPage itemTab;
        private System.Windows.Forms.TrackBar itemDepthSlider;
        private System.Windows.Forms.Button itemSpawnButton;
        private System.Windows.Forms.Label itemSpawnLabel;
        private System.Windows.Forms.Label depthLabel;
        private System.Windows.Forms.Label weightLabel;
        private System.Windows.Forms.TextBox itemWeightBox;
        private System.Windows.Forms.TextBox itemRadiusText;
        private System.Windows.Forms.Label itemRadiusLabel;
        private System.Windows.Forms.CheckBox itemImmovableBox;
        private System.Windows.Forms.Label polygonTypeLabel;
        private System.Windows.Forms.ComboBox itemPolygonType;
        private System.Windows.Forms.Button enemySpawnButton;
        private System.Windows.Forms.Label enemyCountLabel;
        private System.Windows.Forms.TextBox enemyCountBox;
        private System.Windows.Forms.TabPage backgroundTab;
        private System.Windows.Forms.TextBox backgroundTextureField;
        private System.Windows.Forms.Label backgroundRotationLabel;
        private System.Windows.Forms.Label backgroundTextureLabel;
        private System.Windows.Forms.TextBox backgroundRotationText;
        private System.Windows.Forms.Label backgroundScaleLabel;
        private System.Windows.Forms.TextBox backgroundScaleField;
        private System.Windows.Forms.Button backgroundSpawnButton;
        private System.Windows.Forms.TabControl mapLevelTabControl;
        private System.Windows.Forms.TabPage mapTab;
        private System.Windows.Forms.TabPage levelTab;
        private System.Windows.Forms.Label rotationLabel;
        private System.Windows.Forms.TextBox itemRotationTextBox;
        private System.Windows.Forms.Label itemWidthLabel;
        private System.Windows.Forms.TrackBar itemWidthSlider;
        private System.Windows.Forms.CheckBox autoProgressCheckBox;
        private System.Windows.Forms.TextBox enemyTriggerWidthTextbox;
        private System.Windows.Forms.Label enemyTriggerWidthLabel;
        private System.Windows.Forms.TextBox enemyTriggerLocationTextbox;
        private System.Windows.Forms.Label enemyTriggerLocationLabel;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.CheckBox physicsEnabledBox;
        private System.Windows.Forms.ComboBox itemTextureBox;
        private Controls.LevelInfoPanel levelInfoPanel;
    }
}