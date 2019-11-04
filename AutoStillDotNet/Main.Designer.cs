namespace AutoStillDotNet
{
    partial class Main
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.lblTemp1 = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblTemp3 = new System.Windows.Forms.Label();
            this.lblTemp4 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.lblPressure = new System.Windows.Forms.Label();
            this.chartRun = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pinSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sensorCalibrationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.costSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewHistoricalRunsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.efficencyReportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.productionCostsReportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.manualToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnRescan = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.lblStillLowSwitch = new System.Windows.Forms.Label();
            this.lblStillHighSwitch = new System.Windows.Forms.Label();
            this.lblRVLowSwitch = new System.Windows.Forms.Label();
            this.lblRVHighSwtich = new System.Windows.Forms.Label();
            this.lblTemp2 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.lblTheoretical = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartRun)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(338, 30);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(493, 588);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(408, 87);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(134, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Column Head Temperature";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(348, 524);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(85, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "System Pressure";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 435);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(74, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Energy Usage";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(408, 272);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(139, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Reflux Coolant Temperature";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(12, 592);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(320, 23);
            this.progressBar1.TabIndex = 5;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 566);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(171, 13);
            this.label5.TabIndex = 6;
            this.label5.Text = "Estimated Progress Of Current Run";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(671, 293);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(160, 13);
            this.label6.TabIndex = 7;
            this.label6.Text = "Condenser Coolant Temperature";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(14, 36);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(154, 26);
            this.label7.TabIndex = 8;
            this.label7.Text = "System Status";
            // 
            // lblTemp1
            // 
            this.lblTemp1.AutoSize = true;
            this.lblTemp1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTemp1.Location = new System.Drawing.Point(407, 100);
            this.lblTemp1.Name = "lblTemp1";
            this.lblTemp1.Size = new System.Drawing.Size(70, 24);
            this.lblTemp1.TabIndex = 9;
            this.lblTemp1.Text = "Temp1";
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatus.Location = new System.Drawing.Point(12, 62);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(108, 37);
            this.lblStatus.TabIndex = 10;
            this.lblStatus.Text = "Status";
            // 
            // lblTemp3
            // 
            this.lblTemp3.AutoSize = true;
            this.lblTemp3.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTemp3.Location = new System.Drawing.Point(407, 285);
            this.lblTemp3.Name = "lblTemp3";
            this.lblTemp3.Size = new System.Drawing.Size(70, 24);
            this.lblTemp3.TabIndex = 11;
            this.lblTemp3.Text = "Temp3";
            // 
            // lblTemp4
            // 
            this.lblTemp4.AutoSize = true;
            this.lblTemp4.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTemp4.Location = new System.Drawing.Point(673, 306);
            this.lblTemp4.Name = "lblTemp4";
            this.lblTemp4.Size = new System.Drawing.Size(70, 24);
            this.lblTemp4.TabIndex = 12;
            this.lblTemp4.Text = "Temp4";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(12, 448);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(74, 13);
            this.label12.TabIndex = 13;
            this.label12.Text = "Current: 0 KW";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(12, 463);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(89, 13);
            this.label13.TabIndex = 14;
            this.label13.Text = "This Run: 0 KWh";
            // 
            // lblPressure
            // 
            this.lblPressure.AutoSize = true;
            this.lblPressure.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPressure.Location = new System.Drawing.Point(347, 537);
            this.lblPressure.Name = "lblPressure";
            this.lblPressure.Size = new System.Drawing.Size(85, 24);
            this.lblPressure.TabIndex = 15;
            this.lblPressure.Text = "Pressure";
            // 
            // chartRun
            // 
            chartArea1.Name = "ChartArea1";
            this.chartRun.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            this.chartRun.Legends.Add(legend1);
            this.chartRun.Location = new System.Drawing.Point(856, 39);
            this.chartRun.Name = "chartRun";
            this.chartRun.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.SemiTransparent;
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            this.chartRun.Series.Add(series1);
            this.chartRun.Size = new System.Drawing.Size(681, 578);
            this.chartRun.TabIndex = 16;
            this.chartRun.Text = "Current Run Temperature vs Time";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsToolStripMenuItem,
            this.dataToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1549, 24);
            this.menuStrip1.TabIndex = 18;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pinSettingsToolStripMenuItem,
            this.sensorCalibrationToolStripMenuItem,
            this.costSettingsToolStripMenuItem});
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.settingsToolStripMenuItem.Text = "Settings";
            // 
            // pinSettingsToolStripMenuItem
            // 
            this.pinSettingsToolStripMenuItem.Name = "pinSettingsToolStripMenuItem";
            this.pinSettingsToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.pinSettingsToolStripMenuItem.Text = "Pin Settings";
            this.pinSettingsToolStripMenuItem.Click += new System.EventHandler(this.PinSettingsToolStripMenuItem_Click);
            // 
            // sensorCalibrationToolStripMenuItem
            // 
            this.sensorCalibrationToolStripMenuItem.Name = "sensorCalibrationToolStripMenuItem";
            this.sensorCalibrationToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.sensorCalibrationToolStripMenuItem.Text = "Sensor Calibration";
            // 
            // costSettingsToolStripMenuItem
            // 
            this.costSettingsToolStripMenuItem.Name = "costSettingsToolStripMenuItem";
            this.costSettingsToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.costSettingsToolStripMenuItem.Text = "Cost Settings";
            // 
            // dataToolStripMenuItem
            // 
            this.dataToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewHistoricalRunsToolStripMenuItem,
            this.efficencyReportToolStripMenuItem,
            this.productionCostsReportToolStripMenuItem});
            this.dataToolStripMenuItem.Name = "dataToolStripMenuItem";
            this.dataToolStripMenuItem.Size = new System.Drawing.Size(43, 20);
            this.dataToolStripMenuItem.Text = "Data";
            // 
            // viewHistoricalRunsToolStripMenuItem
            // 
            this.viewHistoricalRunsToolStripMenuItem.Name = "viewHistoricalRunsToolStripMenuItem";
            this.viewHistoricalRunsToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.viewHistoricalRunsToolStripMenuItem.Text = "View Historical Runs";
            // 
            // efficencyReportToolStripMenuItem
            // 
            this.efficencyReportToolStripMenuItem.Name = "efficencyReportToolStripMenuItem";
            this.efficencyReportToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.efficencyReportToolStripMenuItem.Text = "Efficency Report";
            // 
            // productionCostsReportToolStripMenuItem
            // 
            this.productionCostsReportToolStripMenuItem.Name = "productionCostsReportToolStripMenuItem";
            this.productionCostsReportToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.productionCostsReportToolStripMenuItem.Text = "Production Costs Report";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.manualToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // manualToolStripMenuItem
            // 
            this.manualToolStripMenuItem.Name = "manualToolStripMenuItem";
            this.manualToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            this.manualToolStripMenuItem.Text = "Manual";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            this.aboutToolStripMenuItem.Text = "About";
            // 
            // btnRescan
            // 
            this.btnRescan.Location = new System.Drawing.Point(19, 103);
            this.btnRescan.Name = "btnRescan";
            this.btnRescan.Size = new System.Drawing.Size(131, 23);
            this.btnRescan.TabIndex = 19;
            this.btnRescan.Text = "Scan for controller";
            this.btnRescan.UseVisualStyleBackColor = true;
            this.btnRescan.Click += new System.EventHandler(this.btnScan_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(434, 448);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(87, 13);
            this.label8.TabIndex = 20;
            this.label8.Text = "Still Low Switch: ";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(434, 382);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(89, 13);
            this.label9.TabIndex = 21;
            this.label9.Text = "Still High Switch: ";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(506, 592);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(153, 13);
            this.label10.TabIndex = 22;
            this.label10.Text = "Receiving Vessel Low Switch: ";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(542, 498);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(155, 13);
            this.label11.TabIndex = 23;
            this.label11.Text = "Receiving Vessel High Switch: ";
            // 
            // lblStillLowSwitch
            // 
            this.lblStillLowSwitch.AutoSize = true;
            this.lblStillLowSwitch.Location = new System.Drawing.Point(518, 448);
            this.lblStillLowSwitch.Name = "lblStillLowSwitch";
            this.lblStillLowSwitch.Size = new System.Drawing.Size(32, 13);
            this.lblStillLowSwitch.TabIndex = 24;
            this.lblStillLowSwitch.Text = "State";
            // 
            // lblStillHighSwitch
            // 
            this.lblStillHighSwitch.AutoSize = true;
            this.lblStillHighSwitch.Location = new System.Drawing.Point(518, 382);
            this.lblStillHighSwitch.Name = "lblStillHighSwitch";
            this.lblStillHighSwitch.Size = new System.Drawing.Size(32, 13);
            this.lblStillHighSwitch.TabIndex = 25;
            this.lblStillHighSwitch.Text = "State";
            // 
            // lblRVLowSwitch
            // 
            this.lblRVLowSwitch.AutoSize = true;
            this.lblRVLowSwitch.Location = new System.Drawing.Point(665, 592);
            this.lblRVLowSwitch.Name = "lblRVLowSwitch";
            this.lblRVLowSwitch.Size = new System.Drawing.Size(32, 13);
            this.lblRVLowSwitch.TabIndex = 26;
            this.lblRVLowSwitch.Text = "State";
            // 
            // lblRVHighSwtich
            // 
            this.lblRVHighSwtich.AutoSize = true;
            this.lblRVHighSwtich.Location = new System.Drawing.Point(701, 498);
            this.lblRVHighSwtich.Name = "lblRVHighSwtich";
            this.lblRVHighSwtich.Size = new System.Drawing.Size(32, 13);
            this.lblRVHighSwtich.TabIndex = 27;
            this.lblRVHighSwtich.Text = "State";
            // 
            // lblTemp2
            // 
            this.lblTemp2.AutoSize = true;
            this.lblTemp2.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTemp2.Location = new System.Drawing.Point(347, 498);
            this.lblTemp2.Name = "lblTemp2";
            this.lblTemp2.Size = new System.Drawing.Size(70, 24);
            this.lblTemp2.TabIndex = 29;
            this.lblTemp2.Text = "Temp2";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(348, 485);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(135, 13);
            this.label15.TabIndex = 28;
            this.label15.Text = "Boiling Vessel Temperature";
            // 
            // lblTheoretical
            // 
            this.lblTheoretical.AutoSize = true;
            this.lblTheoretical.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTheoretical.Location = new System.Drawing.Point(407, 62);
            this.lblTheoretical.Name = "lblTheoretical";
            this.lblTheoretical.Size = new System.Drawing.Size(109, 24);
            this.lblTheoretical.TabIndex = 31;
            this.lblTheoretical.Text = "BoilingPoint";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(408, 49);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(121, 13);
            this.label16.TabIndex = 30;
            this.label16.Text = "Theoretical Boiling Point";
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ClientSize = new System.Drawing.Size(1549, 629);
            this.Controls.Add(this.lblTheoretical);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.lblTemp2);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.lblRVHighSwtich);
            this.Controls.Add(this.lblRVLowSwitch);
            this.Controls.Add(this.lblStillHighSwitch);
            this.Controls.Add(this.lblStillLowSwitch);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.btnRescan);
            this.Controls.Add(this.chartRun);
            this.Controls.Add(this.lblPressure);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.lblTemp4);
            this.Controls.Add(this.lblTemp3);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lblTemp1);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.menuStrip1);
            this.DoubleBuffered = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Main";
            this.Text = "AutoStill";
            this.Load += new System.EventHandler(this.Main_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartRun)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label lblTemp1;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblTemp3;
        private System.Windows.Forms.Label lblTemp4;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label lblPressure;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartRun;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pinSettingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sensorCalibrationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem costSettingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dataToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewHistoricalRunsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem efficencyReportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem productionCostsReportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem manualToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.Button btnRescan;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label lblStillLowSwitch;
        private System.Windows.Forms.Label lblStillHighSwitch;
        private System.Windows.Forms.Label lblRVLowSwitch;
        private System.Windows.Forms.Label lblRVHighSwtich;
        private System.Windows.Forms.Label lblTemp2;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label lblTheoretical;
        private System.Windows.Forms.Label label16;
    }
}

