namespace Chummer
{
    partial class frmSelectVehicle
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
            if (disposing)
            {
                components?.Dispose();
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
            this.components = new System.ComponentModel.Container();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblSearchLabel = new System.Windows.Forms.Label();
            this.lblVehiclePilot = new System.Windows.Forms.Label();
            this.lblVehiclePilotLabel = new System.Windows.Forms.Label();
            this.lblVehicleArmor = new System.Windows.Forms.Label();
            this.lblVehicleArmorLabel = new System.Windows.Forms.Label();
            this.lblVehicleBody = new System.Windows.Forms.Label();
            this.lblVehicleBodyLabel = new System.Windows.Forms.Label();
            this.lblVehicleSpeed = new System.Windows.Forms.Label();
            this.lblVehicleSpeedLabel = new System.Windows.Forms.Label();
            this.lblVehicleCost = new System.Windows.Forms.Label();
            this.lblVehicleCostLabel = new System.Windows.Forms.Label();
            this.lblVehicleAvail = new System.Windows.Forms.Label();
            this.lblVehicleAvailLabel = new System.Windows.Forms.Label();
            this.lblVehicleAccel = new System.Windows.Forms.Label();
            this.lblVehicleAccelLabel = new System.Windows.Forms.Label();
            this.lblVehicleHandling = new System.Windows.Forms.Label();
            this.lblVehicleHandlingLabel = new System.Windows.Forms.Label();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdOK = new System.Windows.Forms.Button();
            this.lstVehicle = new System.Windows.Forms.ListBox();
            this.lblCategory = new System.Windows.Forms.Label();
            this.cboCategory = new Chummer.ElasticComboBox();
            this.lblVehicleSensor = new System.Windows.Forms.Label();
            this.lblVehicleSensorLabel = new System.Windows.Forms.Label();
            this.cmdOKAdd = new System.Windows.Forms.Button();
            this.lblSource = new System.Windows.Forms.Label();
            this.lblSourceLabel = new System.Windows.Forms.Label();
            this.chkFreeItem = new Chummer.ColorableCheckBox(this.components);
            this.chkUsedVehicle = new Chummer.ColorableCheckBox(this.components);
            this.lblUsedVehicleDiscountLabel = new System.Windows.Forms.Label();
            this.nudUsedVehicleDiscount = new Chummer.NumericUpDownEx();
            this.lblUsedVehicleDiscountPercentLabel = new System.Windows.Forms.Label();
            this.nudMarkup = new Chummer.NumericUpDownEx();
            this.lblMarkupLabel = new System.Windows.Forms.Label();
            this.lblMarkupPercentLabel = new System.Windows.Forms.Label();
            this.lblTest = new System.Windows.Forms.Label();
            this.lblTestLabel = new System.Windows.Forms.Label();
            this.chkBlackMarketDiscount = new Chummer.ColorableCheckBox(this.components);
            this.lblVehicleSeatsLabel = new System.Windows.Forms.Label();
            this.lblVehicleSeats = new System.Windows.Forms.Label();
            this.chkHideOverAvailLimit = new Chummer.ColorableCheckBox(this.components);
            this.tlpMain = new Chummer.BufferedTableLayoutPanel(this.components);
            this.chkShowOnlyAffordItems = new Chummer.ColorableCheckBox(this.components);
            this.tableLayoutPanel2 = new Chummer.BufferedTableLayoutPanel(this.components);
            this.tlpButtons = new Chummer.BufferedTableLayoutPanel(this.components);
            this.tlpRight = new Chummer.BufferedTableLayoutPanel(this.components);
            this.flpDiscount = new System.Windows.Forms.FlowLayoutPanel();
            this.flpCheckBoxes = new System.Windows.Forms.FlowLayoutPanel();
            this.flpMarkup = new System.Windows.Forms.FlowLayoutPanel();
            this.tlpTopRight = new Chummer.BufferedTableLayoutPanel(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.nudUsedVehicleDiscount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMarkup)).BeginInit();
            this.tlpMain.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tlpButtons.SuspendLayout();
            this.tlpRight.SuspendLayout();
            this.flpDiscount.SuspendLayout();
            this.flpCheckBoxes.SuspendLayout();
            this.flpMarkup.SuspendLayout();
            this.tlpTopRight.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtSearch
            // 
            this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSearch.Location = new System.Drawing.Point(53, 3);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(404, 20);
            this.txtSearch.TabIndex = 1;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            this.txtSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyDown);
            this.txtSearch.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyUp);
            // 
            // lblSearchLabel
            // 
            this.lblSearchLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSearchLabel.AutoSize = true;
            this.lblSearchLabel.Location = new System.Drawing.Point(3, 6);
            this.lblSearchLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSearchLabel.Name = "lblSearchLabel";
            this.lblSearchLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSearchLabel.TabIndex = 0;
            this.lblSearchLabel.Tag = "Label_Search";
            this.lblSearchLabel.Text = "&Search:";
            // 
            // lblVehiclePilot
            // 
            this.lblVehiclePilot.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblVehiclePilot.AutoSize = true;
            this.lblVehiclePilot.Location = new System.Drawing.Point(288, 31);
            this.lblVehiclePilot.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehiclePilot.Name = "lblVehiclePilot";
            this.lblVehiclePilot.Size = new System.Drawing.Size(33, 13);
            this.lblVehiclePilot.TabIndex = 9;
            this.lblVehiclePilot.Text = "[Pilot]";
            // 
            // lblVehiclePilotLabel
            // 
            this.lblVehiclePilotLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblVehiclePilotLabel.AutoSize = true;
            this.lblVehiclePilotLabel.Location = new System.Drawing.Point(252, 31);
            this.lblVehiclePilotLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehiclePilotLabel.Name = "lblVehiclePilotLabel";
            this.lblVehiclePilotLabel.Size = new System.Drawing.Size(30, 13);
            this.lblVehiclePilotLabel.TabIndex = 8;
            this.lblVehiclePilotLabel.Tag = "Label_Pilot";
            this.lblVehiclePilotLabel.Text = "Pilot:";
            // 
            // lblVehicleArmor
            // 
            this.lblVehicleArmor.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblVehicleArmor.AutoSize = true;
            this.lblVehicleArmor.Location = new System.Drawing.Point(288, 56);
            this.lblVehicleArmor.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleArmor.Name = "lblVehicleArmor";
            this.lblVehicleArmor.Size = new System.Drawing.Size(40, 13);
            this.lblVehicleArmor.TabIndex = 13;
            this.lblVehicleArmor.Text = "[Armor]";
            // 
            // lblVehicleArmorLabel
            // 
            this.lblVehicleArmorLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblVehicleArmorLabel.AutoSize = true;
            this.lblVehicleArmorLabel.Location = new System.Drawing.Point(245, 56);
            this.lblVehicleArmorLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleArmorLabel.Name = "lblVehicleArmorLabel";
            this.lblVehicleArmorLabel.Size = new System.Drawing.Size(37, 13);
            this.lblVehicleArmorLabel.TabIndex = 12;
            this.lblVehicleArmorLabel.Tag = "Label_Armor";
            this.lblVehicleArmorLabel.Text = "Armor:";
            // 
            // lblVehicleBody
            // 
            this.lblVehicleBody.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblVehicleBody.AutoSize = true;
            this.lblVehicleBody.Location = new System.Drawing.Point(61, 56);
            this.lblVehicleBody.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleBody.Name = "lblVehicleBody";
            this.lblVehicleBody.Size = new System.Drawing.Size(37, 13);
            this.lblVehicleBody.TabIndex = 11;
            this.lblVehicleBody.Text = "[Body]";
            // 
            // lblVehicleBodyLabel
            // 
            this.lblVehicleBodyLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblVehicleBodyLabel.AutoSize = true;
            this.lblVehicleBodyLabel.Location = new System.Drawing.Point(21, 56);
            this.lblVehicleBodyLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleBodyLabel.Name = "lblVehicleBodyLabel";
            this.lblVehicleBodyLabel.Size = new System.Drawing.Size(34, 13);
            this.lblVehicleBodyLabel.TabIndex = 10;
            this.lblVehicleBodyLabel.Tag = "Label_Body";
            this.lblVehicleBodyLabel.Text = "Body:";
            // 
            // lblVehicleSpeed
            // 
            this.lblVehicleSpeed.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblVehicleSpeed.AutoSize = true;
            this.lblVehicleSpeed.Location = new System.Drawing.Point(61, 31);
            this.lblVehicleSpeed.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleSpeed.Name = "lblVehicleSpeed";
            this.lblVehicleSpeed.Size = new System.Drawing.Size(44, 13);
            this.lblVehicleSpeed.TabIndex = 7;
            this.lblVehicleSpeed.Text = "[Speed]";
            // 
            // lblVehicleSpeedLabel
            // 
            this.lblVehicleSpeedLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblVehicleSpeedLabel.AutoSize = true;
            this.lblVehicleSpeedLabel.Location = new System.Drawing.Point(14, 31);
            this.lblVehicleSpeedLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleSpeedLabel.Name = "lblVehicleSpeedLabel";
            this.lblVehicleSpeedLabel.Size = new System.Drawing.Size(41, 13);
            this.lblVehicleSpeedLabel.TabIndex = 6;
            this.lblVehicleSpeedLabel.Tag = "Label_Speed";
            this.lblVehicleSpeedLabel.Text = "Speed:";
            // 
            // lblVehicleCost
            // 
            this.lblVehicleCost.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblVehicleCost.AutoSize = true;
            this.lblVehicleCost.Location = new System.Drawing.Point(61, 131);
            this.lblVehicleCost.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleCost.Name = "lblVehicleCost";
            this.lblVehicleCost.Size = new System.Drawing.Size(34, 13);
            this.lblVehicleCost.TabIndex = 21;
            this.lblVehicleCost.Text = "[Cost]";
            // 
            // lblVehicleCostLabel
            // 
            this.lblVehicleCostLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblVehicleCostLabel.AutoSize = true;
            this.lblVehicleCostLabel.Location = new System.Drawing.Point(24, 131);
            this.lblVehicleCostLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleCostLabel.Name = "lblVehicleCostLabel";
            this.lblVehicleCostLabel.Size = new System.Drawing.Size(31, 13);
            this.lblVehicleCostLabel.TabIndex = 20;
            this.lblVehicleCostLabel.Tag = "Label_Cost";
            this.lblVehicleCostLabel.Text = "Cost:";
            // 
            // lblVehicleAvail
            // 
            this.lblVehicleAvail.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblVehicleAvail.AutoSize = true;
            this.lblVehicleAvail.Location = new System.Drawing.Point(61, 106);
            this.lblVehicleAvail.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleAvail.Name = "lblVehicleAvail";
            this.lblVehicleAvail.Size = new System.Drawing.Size(36, 13);
            this.lblVehicleAvail.TabIndex = 17;
            this.lblVehicleAvail.Text = "[Avail]";
            // 
            // lblVehicleAvailLabel
            // 
            this.lblVehicleAvailLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblVehicleAvailLabel.AutoSize = true;
            this.lblVehicleAvailLabel.Location = new System.Drawing.Point(22, 106);
            this.lblVehicleAvailLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleAvailLabel.Name = "lblVehicleAvailLabel";
            this.lblVehicleAvailLabel.Size = new System.Drawing.Size(33, 13);
            this.lblVehicleAvailLabel.TabIndex = 16;
            this.lblVehicleAvailLabel.Tag = "Label_Avail";
            this.lblVehicleAvailLabel.Text = "Avail:";
            // 
            // lblVehicleAccel
            // 
            this.lblVehicleAccel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblVehicleAccel.AutoSize = true;
            this.lblVehicleAccel.Location = new System.Drawing.Point(288, 6);
            this.lblVehicleAccel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleAccel.Name = "lblVehicleAccel";
            this.lblVehicleAccel.Size = new System.Drawing.Size(40, 13);
            this.lblVehicleAccel.TabIndex = 5;
            this.lblVehicleAccel.Text = "[Accel]";
            // 
            // lblVehicleAccelLabel
            // 
            this.lblVehicleAccelLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblVehicleAccelLabel.AutoSize = true;
            this.lblVehicleAccelLabel.Location = new System.Drawing.Point(245, 6);
            this.lblVehicleAccelLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleAccelLabel.Name = "lblVehicleAccelLabel";
            this.lblVehicleAccelLabel.Size = new System.Drawing.Size(37, 13);
            this.lblVehicleAccelLabel.TabIndex = 4;
            this.lblVehicleAccelLabel.Tag = "Label_Accel";
            this.lblVehicleAccelLabel.Text = "Accel:";
            // 
            // lblVehicleHandling
            // 
            this.lblVehicleHandling.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblVehicleHandling.AutoSize = true;
            this.lblVehicleHandling.Location = new System.Drawing.Point(61, 6);
            this.lblVehicleHandling.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleHandling.Name = "lblVehicleHandling";
            this.lblVehicleHandling.Size = new System.Drawing.Size(55, 13);
            this.lblVehicleHandling.TabIndex = 3;
            this.lblVehicleHandling.Text = "[Handling]";
            // 
            // lblVehicleHandlingLabel
            // 
            this.lblVehicleHandlingLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblVehicleHandlingLabel.AutoSize = true;
            this.lblVehicleHandlingLabel.Location = new System.Drawing.Point(3, 6);
            this.lblVehicleHandlingLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleHandlingLabel.Name = "lblVehicleHandlingLabel";
            this.lblVehicleHandlingLabel.Size = new System.Drawing.Size(52, 13);
            this.lblVehicleHandlingLabel.TabIndex = 2;
            this.lblVehicleHandlingLabel.Tag = "Label_Handling";
            this.lblVehicleHandlingLabel.Text = "Handling:";
            // 
            // cmdCancel
            // 
            this.cmdCancel.AutoSize = true;
            this.cmdCancel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdCancel.Location = new System.Drawing.Point(3, 3);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(72, 23);
            this.cmdCancel.TabIndex = 37;
            this.cmdCancel.Tag = "String_Cancel";
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // cmdOK
            // 
            this.cmdOK.AutoSize = true;
            this.cmdOK.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdOK.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdOK.Location = new System.Drawing.Point(159, 3);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(72, 23);
            this.cmdOK.TabIndex = 35;
            this.cmdOK.Tag = "String_OK";
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // lstVehicle
            // 
            this.tableLayoutPanel2.SetColumnSpan(this.lstVehicle, 2);
            this.lstVehicle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstVehicle.FormattingEnabled = true;
            this.lstVehicle.Location = new System.Drawing.Point(3, 30);
            this.lstVehicle.Name = "lstVehicle";
            this.lstVehicle.Size = new System.Drawing.Size(300, 510);
            this.lstVehicle.TabIndex = 32;
            this.lstVehicle.SelectedIndexChanged += new System.EventHandler(this.lstVehicle_SelectedIndexChanged);
            this.lstVehicle.DoubleClick += new System.EventHandler(this.cmdOK_Click);
            // 
            // lblCategory
            // 
            this.lblCategory.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblCategory.AutoSize = true;
            this.lblCategory.Location = new System.Drawing.Point(3, 7);
            this.lblCategory.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(52, 13);
            this.lblCategory.TabIndex = 33;
            this.lblCategory.Tag = "Label_Category";
            this.lblCategory.Text = "Category:";
            // 
            // cboCategory
            // 
            this.cboCategory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCategory.FormattingEnabled = true;
            this.cboCategory.Location = new System.Drawing.Point(61, 3);
            this.cboCategory.Name = "cboCategory";
            this.cboCategory.Size = new System.Drawing.Size(242, 21);
            this.cboCategory.TabIndex = 34;
            this.cboCategory.TooltipText = "";
            this.cboCategory.SelectedIndexChanged += new System.EventHandler(this.RefreshCurrentList);
            // 
            // lblVehicleSensor
            // 
            this.lblVehicleSensor.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblVehicleSensor.AutoSize = true;
            this.lblVehicleSensor.Location = new System.Drawing.Point(61, 81);
            this.lblVehicleSensor.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleSensor.Name = "lblVehicleSensor";
            this.lblVehicleSensor.Size = new System.Drawing.Size(46, 13);
            this.lblVehicleSensor.TabIndex = 15;
            this.lblVehicleSensor.Text = "[Sensor]";
            // 
            // lblVehicleSensorLabel
            // 
            this.lblVehicleSensorLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblVehicleSensorLabel.AutoSize = true;
            this.lblVehicleSensorLabel.Location = new System.Drawing.Point(12, 81);
            this.lblVehicleSensorLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleSensorLabel.Name = "lblVehicleSensorLabel";
            this.lblVehicleSensorLabel.Size = new System.Drawing.Size(43, 13);
            this.lblVehicleSensorLabel.TabIndex = 14;
            this.lblVehicleSensorLabel.Tag = "Label_Sensor";
            this.lblVehicleSensorLabel.Text = "Sensor:";
            // 
            // cmdOKAdd
            // 
            this.cmdOKAdd.AutoSize = true;
            this.cmdOKAdd.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdOKAdd.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdOKAdd.Location = new System.Drawing.Point(81, 3);
            this.cmdOKAdd.Name = "cmdOKAdd";
            this.cmdOKAdd.Size = new System.Drawing.Size(72, 23);
            this.cmdOKAdd.TabIndex = 36;
            this.cmdOKAdd.Tag = "String_AddMore";
            this.cmdOKAdd.Text = "&Add && More";
            this.cmdOKAdd.UseVisualStyleBackColor = true;
            this.cmdOKAdd.Click += new System.EventHandler(this.cmdOKAdd_Click);
            // 
            // lblSource
            // 
            this.lblSource.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSource.AutoSize = true;
            this.tlpRight.SetColumnSpan(this.lblSource, 3);
            this.lblSource.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblSource.Location = new System.Drawing.Point(61, 208);
            this.lblSource.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new System.Drawing.Size(47, 13);
            this.lblSource.TabIndex = 31;
            this.lblSource.Text = "[Source]";
            this.lblSource.Click += new System.EventHandler(this.OpenSourceFromLabel);
            // 
            // lblSourceLabel
            // 
            this.lblSourceLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSourceLabel.AutoSize = true;
            this.lblSourceLabel.Location = new System.Drawing.Point(11, 208);
            this.lblSourceLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblSourceLabel.Name = "lblSourceLabel";
            this.lblSourceLabel.Size = new System.Drawing.Size(44, 13);
            this.lblSourceLabel.TabIndex = 30;
            this.lblSourceLabel.Tag = "Label_Source";
            this.lblSourceLabel.Text = "Source:";
            // 
            // chkFreeItem
            // 
            this.chkFreeItem.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkFreeItem.AutoSize = true;
            this.chkFreeItem.DefaultColorScheme = true;
            this.chkFreeItem.Location = new System.Drawing.Point(3, 4);
            this.chkFreeItem.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkFreeItem.Name = "chkFreeItem";
            this.chkFreeItem.Size = new System.Drawing.Size(50, 17);
            this.chkFreeItem.TabIndex = 26;
            this.chkFreeItem.Tag = "Checkbox_Free";
            this.chkFreeItem.Text = "Free!";
            this.chkFreeItem.UseVisualStyleBackColor = true;
            this.chkFreeItem.CheckedChanged += new System.EventHandler(this.chkFreeItem_CheckedChanged);
            // 
            // chkUsedVehicle
            // 
            this.chkUsedVehicle.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkUsedVehicle.AutoSize = true;
            this.chkUsedVehicle.DefaultColorScheme = true;
            this.chkUsedVehicle.Location = new System.Drawing.Point(3, 4);
            this.chkUsedVehicle.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkUsedVehicle.Name = "chkUsedVehicle";
            this.chkUsedVehicle.Size = new System.Drawing.Size(89, 17);
            this.chkUsedVehicle.TabIndex = 22;
            this.chkUsedVehicle.Tag = "Checkbox_SelectVehicle_UsedVehicle";
            this.chkUsedVehicle.Text = "Used Vehicle";
            this.chkUsedVehicle.UseVisualStyleBackColor = true;
            this.chkUsedVehicle.Visible = false;
            this.chkUsedVehicle.CheckedChanged += new System.EventHandler(this.chkUsedVehicle_CheckedChanged);
            // 
            // lblUsedVehicleDiscountLabel
            // 
            this.lblUsedVehicleDiscountLabel.AutoSize = true;
            this.lblUsedVehicleDiscountLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblUsedVehicleDiscountLabel.Location = new System.Drawing.Point(98, 6);
            this.lblUsedVehicleDiscountLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblUsedVehicleDiscountLabel.Name = "lblUsedVehicleDiscountLabel";
            this.lblUsedVehicleDiscountLabel.Size = new System.Drawing.Size(52, 14);
            this.lblUsedVehicleDiscountLabel.TabIndex = 23;
            this.lblUsedVehicleDiscountLabel.Tag = "Label_SelectVehicle_Discount";
            this.lblUsedVehicleDiscountLabel.Text = "Discount:";
            this.lblUsedVehicleDiscountLabel.Visible = false;
            // 
            // nudUsedVehicleDiscount
            // 
            this.nudUsedVehicleDiscount.AutoSize = true;
            this.nudUsedVehicleDiscount.DecimalPlaces = 2;
            this.nudUsedVehicleDiscount.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudUsedVehicleDiscount.Location = new System.Drawing.Point(156, 3);
            this.nudUsedVehicleDiscount.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.nudUsedVehicleDiscount.Minimum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.nudUsedVehicleDiscount.Name = "nudUsedVehicleDiscount";
            this.nudUsedVehicleDiscount.Size = new System.Drawing.Size(50, 20);
            this.nudUsedVehicleDiscount.TabIndex = 24;
            this.nudUsedVehicleDiscount.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.nudUsedVehicleDiscount.Visible = false;
            this.nudUsedVehicleDiscount.ValueChanged += new System.EventHandler(this.nudUsedVehicleDiscount_ValueChanged);
            // 
            // lblUsedVehicleDiscountPercentLabel
            // 
            this.lblUsedVehicleDiscountPercentLabel.AutoSize = true;
            this.lblUsedVehicleDiscountPercentLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblUsedVehicleDiscountPercentLabel.Location = new System.Drawing.Point(212, 6);
            this.lblUsedVehicleDiscountPercentLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblUsedVehicleDiscountPercentLabel.Name = "lblUsedVehicleDiscountPercentLabel";
            this.lblUsedVehicleDiscountPercentLabel.Size = new System.Drawing.Size(15, 14);
            this.lblUsedVehicleDiscountPercentLabel.TabIndex = 25;
            this.lblUsedVehicleDiscountPercentLabel.Text = "%";
            this.lblUsedVehicleDiscountPercentLabel.Visible = false;
            // 
            // nudMarkup
            // 
            this.nudMarkup.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudMarkup.AutoSize = true;
            this.nudMarkup.DecimalPlaces = 2;
            this.nudMarkup.Location = new System.Drawing.Point(3, 3);
            this.nudMarkup.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudMarkup.Minimum = new decimal(new int[] {
            9999,
            0,
            0,
            -2147352576});
            this.nudMarkup.Name = "nudMarkup";
            this.nudMarkup.Size = new System.Drawing.Size(56, 20);
            this.nudMarkup.TabIndex = 28;
            this.nudMarkup.ValueChanged += new System.EventHandler(this.nudMarkup_ValueChanged);
            // 
            // lblMarkupLabel
            // 
            this.lblMarkupLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblMarkupLabel.AutoSize = true;
            this.lblMarkupLabel.Location = new System.Drawing.Point(236, 131);
            this.lblMarkupLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMarkupLabel.Name = "lblMarkupLabel";
            this.lblMarkupLabel.Size = new System.Drawing.Size(46, 13);
            this.lblMarkupLabel.TabIndex = 27;
            this.lblMarkupLabel.Tag = "Label_SelectGear_Markup";
            this.lblMarkupLabel.Text = "Markup:";
            // 
            // lblMarkupPercentLabel
            // 
            this.lblMarkupPercentLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblMarkupPercentLabel.AutoSize = true;
            this.lblMarkupPercentLabel.Location = new System.Drawing.Point(65, 6);
            this.lblMarkupPercentLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMarkupPercentLabel.Name = "lblMarkupPercentLabel";
            this.lblMarkupPercentLabel.Size = new System.Drawing.Size(15, 13);
            this.lblMarkupPercentLabel.TabIndex = 29;
            this.lblMarkupPercentLabel.Text = "%";
            // 
            // lblTest
            // 
            this.lblTest.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblTest.AutoSize = true;
            this.lblTest.Location = new System.Drawing.Point(288, 106);
            this.lblTest.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblTest.Name = "lblTest";
            this.lblTest.Size = new System.Drawing.Size(19, 13);
            this.lblTest.TabIndex = 19;
            this.lblTest.Text = "[0]";
            // 
            // lblTestLabel
            // 
            this.lblTestLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblTestLabel.AutoSize = true;
            this.lblTestLabel.Location = new System.Drawing.Point(251, 106);
            this.lblTestLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblTestLabel.Name = "lblTestLabel";
            this.lblTestLabel.Size = new System.Drawing.Size(31, 13);
            this.lblTestLabel.TabIndex = 18;
            this.lblTestLabel.Tag = "Label_Test";
            this.lblTestLabel.Text = "Test:";
            // 
            // chkBlackMarketDiscount
            // 
            this.chkBlackMarketDiscount.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkBlackMarketDiscount.AutoSize = true;
            this.chkBlackMarketDiscount.DefaultColorScheme = true;
            this.chkBlackMarketDiscount.Location = new System.Drawing.Point(59, 4);
            this.chkBlackMarketDiscount.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkBlackMarketDiscount.Name = "chkBlackMarketDiscount";
            this.chkBlackMarketDiscount.Size = new System.Drawing.Size(163, 17);
            this.chkBlackMarketDiscount.TabIndex = 38;
            this.chkBlackMarketDiscount.Tag = "Checkbox_BlackMarketDiscount";
            this.chkBlackMarketDiscount.Text = "Black Market Discount (10%)";
            this.chkBlackMarketDiscount.UseVisualStyleBackColor = true;
            this.chkBlackMarketDiscount.Visible = false;
            this.chkBlackMarketDiscount.CheckedChanged += new System.EventHandler(this.chkBlackMarketDiscount_CheckedChanged);
            // 
            // lblVehicleSeatsLabel
            // 
            this.lblVehicleSeatsLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblVehicleSeatsLabel.AutoSize = true;
            this.lblVehicleSeatsLabel.Location = new System.Drawing.Point(245, 81);
            this.lblVehicleSeatsLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleSeatsLabel.Name = "lblVehicleSeatsLabel";
            this.lblVehicleSeatsLabel.Size = new System.Drawing.Size(37, 13);
            this.lblVehicleSeatsLabel.TabIndex = 39;
            this.lblVehicleSeatsLabel.Tag = "Label_Seats";
            this.lblVehicleSeatsLabel.Text = "Seats:";
            // 
            // lblVehicleSeats
            // 
            this.lblVehicleSeats.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblVehicleSeats.AutoSize = true;
            this.lblVehicleSeats.Location = new System.Drawing.Point(288, 81);
            this.lblVehicleSeats.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblVehicleSeats.Name = "lblVehicleSeats";
            this.lblVehicleSeats.Size = new System.Drawing.Size(40, 13);
            this.lblVehicleSeats.TabIndex = 40;
            this.lblVehicleSeats.Text = "[Seats]";
            // 
            // chkHideOverAvailLimit
            // 
            this.chkHideOverAvailLimit.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkHideOverAvailLimit.AutoSize = true;
            this.chkHideOverAvailLimit.DefaultColorScheme = true;
            this.chkHideOverAvailLimit.Location = new System.Drawing.Point(309, 468);
            this.chkHideOverAvailLimit.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkHideOverAvailLimit.Name = "chkHideOverAvailLimit";
            this.chkHideOverAvailLimit.Size = new System.Drawing.Size(175, 17);
            this.chkHideOverAvailLimit.TabIndex = 65;
            this.chkHideOverAvailLimit.Tag = "Checkbox_HideOverAvailLimit";
            this.chkHideOverAvailLimit.Text = "Hide Items Over Avail Limit ({0})";
            this.chkHideOverAvailLimit.UseVisualStyleBackColor = true;
            this.chkHideOverAvailLimit.CheckedChanged += new System.EventHandler(this.RefreshCurrentList);
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 2;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tlpMain.Controls.Add(this.chkHideOverAvailLimit, 1, 3);
            this.tlpMain.Controls.Add(this.chkShowOnlyAffordItems, 1, 4);
            this.tlpMain.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tlpMain.Controls.Add(this.tlpButtons, 1, 5);
            this.tlpMain.Controls.Add(this.tlpRight, 1, 1);
            this.tlpMain.Controls.Add(this.tlpTopRight, 1, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(9, 9);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 6;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(766, 543);
            this.tlpMain.TabIndex = 66;
            // 
            // chkShowOnlyAffordItems
            // 
            this.chkShowOnlyAffordItems.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkShowOnlyAffordItems.AutoSize = true;
            this.chkShowOnlyAffordItems.DefaultColorScheme = true;
            this.chkShowOnlyAffordItems.Location = new System.Drawing.Point(309, 493);
            this.chkShowOnlyAffordItems.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkShowOnlyAffordItems.Name = "chkShowOnlyAffordItems";
            this.chkShowOnlyAffordItems.Size = new System.Drawing.Size(164, 17);
            this.chkShowOnlyAffordItems.TabIndex = 67;
            this.chkShowOnlyAffordItems.Tag = "Checkbox_ShowOnlyAffordItems";
            this.chkShowOnlyAffordItems.Text = "Show Only Items I Can Afford";
            this.chkShowOnlyAffordItems.UseVisualStyleBackColor = true;
            this.chkShowOnlyAffordItems.CheckedChanged += new System.EventHandler(this.RefreshCurrentList);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.lblCategory, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.cboCategory, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.lstVehicle, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tlpMain.SetRowSpan(this.tableLayoutPanel2, 6);
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(306, 543);
            this.tableLayoutPanel2.TabIndex = 69;
            // 
            // tlpButtons
            // 
            this.tlpButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpButtons.AutoSize = true;
            this.tlpButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpButtons.ColumnCount = 3;
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpButtons.Controls.Add(this.cmdCancel, 0, 0);
            this.tlpButtons.Controls.Add(this.cmdOKAdd, 1, 0);
            this.tlpButtons.Controls.Add(this.cmdOK, 2, 0);
            this.tlpButtons.Location = new System.Drawing.Point(532, 514);
            this.tlpButtons.Margin = new System.Windows.Forms.Padding(0);
            this.tlpButtons.Name = "tlpButtons";
            this.tlpButtons.RowCount = 1;
            this.tlpButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpButtons.Size = new System.Drawing.Size(234, 29);
            this.tlpButtons.TabIndex = 73;
            // 
            // tlpRight
            // 
            this.tlpRight.AutoSize = true;
            this.tlpRight.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpRight.ColumnCount = 4;
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpRight.Controls.Add(this.lblSource, 1, 8);
            this.tlpRight.Controls.Add(this.lblVehicleHandlingLabel, 0, 0);
            this.tlpRight.Controls.Add(this.lblVehicleCost, 1, 5);
            this.tlpRight.Controls.Add(this.lblVehicleSeats, 3, 3);
            this.tlpRight.Controls.Add(this.lblSourceLabel, 0, 8);
            this.tlpRight.Controls.Add(this.flpDiscount, 0, 7);
            this.tlpRight.Controls.Add(this.lblVehicleCostLabel, 0, 5);
            this.tlpRight.Controls.Add(this.flpCheckBoxes, 0, 6);
            this.tlpRight.Controls.Add(this.flpMarkup, 3, 5);
            this.tlpRight.Controls.Add(this.lblMarkupLabel, 2, 5);
            this.tlpRight.Controls.Add(this.lblVehicleSeatsLabel, 2, 3);
            this.tlpRight.Controls.Add(this.lblTest, 3, 4);
            this.tlpRight.Controls.Add(this.lblVehicleHandling, 1, 0);
            this.tlpRight.Controls.Add(this.lblVehicleArmorLabel, 2, 2);
            this.tlpRight.Controls.Add(this.lblVehicleArmor, 3, 2);
            this.tlpRight.Controls.Add(this.lblTestLabel, 2, 4);
            this.tlpRight.Controls.Add(this.lblVehicleAccelLabel, 2, 0);
            this.tlpRight.Controls.Add(this.lblVehicleAccel, 3, 0);
            this.tlpRight.Controls.Add(this.lblVehicleBody, 1, 2);
            this.tlpRight.Controls.Add(this.lblVehicleBodyLabel, 0, 2);
            this.tlpRight.Controls.Add(this.lblVehicleSpeed, 1, 1);
            this.tlpRight.Controls.Add(this.lblVehicleSpeedLabel, 0, 1);
            this.tlpRight.Controls.Add(this.lblVehicleAvail, 1, 4);
            this.tlpRight.Controls.Add(this.lblVehicleAvailLabel, 0, 4);
            this.tlpRight.Controls.Add(this.lblVehicleSensor, 1, 3);
            this.tlpRight.Controls.Add(this.lblVehicleSensorLabel, 0, 3);
            this.tlpRight.Controls.Add(this.lblVehiclePilotLabel, 2, 1);
            this.tlpRight.Controls.Add(this.lblVehiclePilot, 3, 1);
            this.tlpRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRight.Location = new System.Drawing.Point(306, 26);
            this.tlpRight.Margin = new System.Windows.Forms.Padding(0);
            this.tlpRight.Name = "tlpRight";
            this.tlpRight.RowCount = 9;
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpRight.Size = new System.Drawing.Size(460, 227);
            this.tlpRight.TabIndex = 74;
            // 
            // flpDiscount
            // 
            this.flpDiscount.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.flpDiscount.AutoSize = true;
            this.flpDiscount.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpRight.SetColumnSpan(this.flpDiscount, 4);
            this.flpDiscount.Controls.Add(this.chkUsedVehicle);
            this.flpDiscount.Controls.Add(this.lblUsedVehicleDiscountLabel);
            this.flpDiscount.Controls.Add(this.nudUsedVehicleDiscount);
            this.flpDiscount.Controls.Add(this.lblUsedVehicleDiscountPercentLabel);
            this.flpDiscount.Location = new System.Drawing.Point(0, 176);
            this.flpDiscount.Margin = new System.Windows.Forms.Padding(0);
            this.flpDiscount.Name = "flpDiscount";
            this.flpDiscount.Size = new System.Drawing.Size(230, 26);
            this.flpDiscount.TabIndex = 70;
            // 
            // flpCheckBoxes
            // 
            this.flpCheckBoxes.AutoSize = true;
            this.tlpRight.SetColumnSpan(this.flpCheckBoxes, 4);
            this.flpCheckBoxes.Controls.Add(this.chkFreeItem);
            this.flpCheckBoxes.Controls.Add(this.chkBlackMarketDiscount);
            this.flpCheckBoxes.Location = new System.Drawing.Point(0, 151);
            this.flpCheckBoxes.Margin = new System.Windows.Forms.Padding(0);
            this.flpCheckBoxes.Name = "flpCheckBoxes";
            this.flpCheckBoxes.Size = new System.Drawing.Size(225, 25);
            this.flpCheckBoxes.TabIndex = 72;
            // 
            // flpMarkup
            // 
            this.flpMarkup.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.flpMarkup.AutoSize = true;
            this.flpMarkup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpMarkup.Controls.Add(this.nudMarkup);
            this.flpMarkup.Controls.Add(this.lblMarkupPercentLabel);
            this.flpMarkup.Location = new System.Drawing.Point(285, 125);
            this.flpMarkup.Margin = new System.Windows.Forms.Padding(0);
            this.flpMarkup.Name = "flpMarkup";
            this.flpMarkup.Size = new System.Drawing.Size(83, 26);
            this.flpMarkup.TabIndex = 71;
            this.flpMarkup.WrapContents = false;
            // 
            // tlpTopRight
            // 
            this.tlpTopRight.AutoSize = true;
            this.tlpTopRight.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpTopRight.ColumnCount = 2;
            this.tlpTopRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpTopRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTopRight.Controls.Add(this.lblSearchLabel, 0, 0);
            this.tlpTopRight.Controls.Add(this.txtSearch, 1, 0);
            this.tlpTopRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpTopRight.Location = new System.Drawing.Point(306, 0);
            this.tlpTopRight.Margin = new System.Windows.Forms.Padding(0);
            this.tlpTopRight.Name = "tlpTopRight";
            this.tlpTopRight.RowCount = 1;
            this.tlpTopRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTopRight.Size = new System.Drawing.Size(460, 26);
            this.tlpTopRight.TabIndex = 75;
            // 
            // frmSelectVehicle
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.tlpMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSelectVehicle";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Tag = "Title_SelectVehicle";
            this.Text = "Select a Vehicle";
            this.Load += new System.EventHandler(this.frmSelectVehicle_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudUsedVehicleDiscount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMarkup)).EndInit();
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.tlpButtons.ResumeLayout(false);
            this.tlpButtons.PerformLayout();
            this.tlpRight.ResumeLayout(false);
            this.tlpRight.PerformLayout();
            this.flpDiscount.ResumeLayout(false);
            this.flpDiscount.PerformLayout();
            this.flpCheckBoxes.ResumeLayout(false);
            this.flpCheckBoxes.PerformLayout();
            this.flpMarkup.ResumeLayout(false);
            this.flpMarkup.PerformLayout();
            this.tlpTopRight.ResumeLayout(false);
            this.tlpTopRight.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblSearchLabel;
        private System.Windows.Forms.Label lblVehiclePilot;
        private System.Windows.Forms.Label lblVehiclePilotLabel;
        private System.Windows.Forms.Label lblVehicleArmor;
        private System.Windows.Forms.Label lblVehicleArmorLabel;
        private System.Windows.Forms.Label lblVehicleBody;
        private System.Windows.Forms.Label lblVehicleBodyLabel;
        private System.Windows.Forms.Label lblVehicleSpeed;
        private System.Windows.Forms.Label lblVehicleSpeedLabel;
        private System.Windows.Forms.Label lblVehicleCost;
        private System.Windows.Forms.Label lblVehicleCostLabel;
        private System.Windows.Forms.Label lblVehicleAvail;
        private System.Windows.Forms.Label lblVehicleAvailLabel;
        private System.Windows.Forms.Label lblVehicleAccel;
        private System.Windows.Forms.Label lblVehicleAccelLabel;
        private System.Windows.Forms.Label lblVehicleHandling;
        private System.Windows.Forms.Label lblVehicleHandlingLabel;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.ListBox lstVehicle;
        private System.Windows.Forms.Label lblCategory;
        private ElasticComboBox cboCategory;
        private System.Windows.Forms.Label lblVehicleSensor;
        private System.Windows.Forms.Label lblVehicleSensorLabel;
        private System.Windows.Forms.Button cmdOKAdd;
        private System.Windows.Forms.Label lblSource;
        private System.Windows.Forms.Label lblSourceLabel;
        private Chummer.ColorableCheckBox chkFreeItem;
        private Chummer.ColorableCheckBox chkUsedVehicle;
        private System.Windows.Forms.Label lblUsedVehicleDiscountLabel;
        private Chummer.NumericUpDownEx nudUsedVehicleDiscount;
        private System.Windows.Forms.Label lblUsedVehicleDiscountPercentLabel;
        private Chummer.NumericUpDownEx nudMarkup;
        private System.Windows.Forms.Label lblMarkupLabel;
        private System.Windows.Forms.Label lblMarkupPercentLabel;
        private System.Windows.Forms.Label lblTest;
        private System.Windows.Forms.Label lblTestLabel;
        private Chummer.ColorableCheckBox chkBlackMarketDiscount;
        private System.Windows.Forms.Label lblVehicleSeatsLabel;
        private System.Windows.Forms.Label lblVehicleSeats;
        private Chummer.ColorableCheckBox chkHideOverAvailLimit;
        private Chummer.BufferedTableLayoutPanel tlpMain;
        private Chummer.ColorableCheckBox chkShowOnlyAffordItems;
        private System.Windows.Forms.FlowLayoutPanel flpMarkup;
        private System.Windows.Forms.FlowLayoutPanel flpCheckBoxes;
        private BufferedTableLayoutPanel tlpButtons;
        private System.Windows.Forms.FlowLayoutPanel flpDiscount;
        private BufferedTableLayoutPanel tlpRight;
        private BufferedTableLayoutPanel tableLayoutPanel2;
        private BufferedTableLayoutPanel tlpTopRight;
    }
}
