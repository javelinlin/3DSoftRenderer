﻿namespace SoftRenderer
{
    partial class MainForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.PictureBox = new System.Windows.Forms.PictureBox();
            this.PropertyGrid = new System.Windows.Forms.PropertyGrid();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.FpsLabel1 = new System.Windows.Forms.Label();
            this.PauseBtn = new System.Windows.Forms.Button();
            this.SelectCameraBtn = new System.Windows.Forms.Button();
            this.SelectRendererBtn = new System.Windows.Forms.Button();
            this.SelectFormBtn = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.focusCtrlLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.resetTRS = new System.Windows.Forms.Button();
            this.TimeScaleLabel = new System.Windows.Forms.Label();
            this.TimeScaleSlider = new System.Windows.Forms.TrackBar();
            this.TimeScaleValueLabel = new System.Windows.Forms.Label();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.ResetCamera = new System.Windows.Forms.Button();
            this.AutoRotateCheckBox = new System.Windows.Forms.CheckBox();
            this.DepthPictureBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox)).BeginInit();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TimeScaleSlider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DepthPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // PictureBox
            // 
            this.PictureBox.BackColor = System.Drawing.Color.Transparent;
            this.PictureBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.PictureBox.Location = new System.Drawing.Point(12, 12);
            this.PictureBox.Name = "PictureBox";
            this.PictureBox.Size = new System.Drawing.Size(300, 300);
            this.PictureBox.TabIndex = 2;
            this.PictureBox.TabStop = false;
            // 
            // PropertyGrid
            // 
            this.PropertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PropertyGrid.Location = new System.Drawing.Point(675, 12);
            this.PropertyGrid.Name = "PropertyGrid";
            this.PropertyGrid.Size = new System.Drawing.Size(281, 556);
            this.PropertyGrid.TabIndex = 5;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 15;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // FpsLabel1
            // 
            this.FpsLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.FpsLabel1.AutoSize = true;
            this.FpsLabel1.BackColor = System.Drawing.Color.Transparent;
            this.FpsLabel1.Location = new System.Drawing.Point(12, 522);
            this.FpsLabel1.Name = "FpsLabel1";
            this.FpsLabel1.Size = new System.Drawing.Size(31, 15);
            this.FpsLabel1.TabIndex = 7;
            this.FpsLabel1.Text = "FPS";
            // 
            // PauseBtn
            // 
            this.PauseBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.PauseBtn.Location = new System.Drawing.Point(12, 540);
            this.PauseBtn.Name = "PauseBtn";
            this.PauseBtn.Size = new System.Drawing.Size(88, 27);
            this.PauseBtn.TabIndex = 8;
            this.PauseBtn.Text = "Pause";
            this.PauseBtn.UseVisualStyleBackColor = true;
            this.PauseBtn.Click += new System.EventHandler(this.PauseBtn_Click);
            // 
            // SelectCameraBtn
            // 
            this.SelectCameraBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.SelectCameraBtn.Location = new System.Drawing.Point(106, 540);
            this.SelectCameraBtn.Name = "SelectCameraBtn";
            this.SelectCameraBtn.Size = new System.Drawing.Size(118, 27);
            this.SelectCameraBtn.TabIndex = 9;
            this.SelectCameraBtn.Text = "SelectCam";
            this.SelectCameraBtn.UseVisualStyleBackColor = true;
            this.SelectCameraBtn.Click += new System.EventHandler(this.SelectCameraBtn_Click);
            // 
            // SelectRendererBtn
            // 
            this.SelectRendererBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.SelectRendererBtn.Location = new System.Drawing.Point(230, 540);
            this.SelectRendererBtn.Name = "SelectRendererBtn";
            this.SelectRendererBtn.Size = new System.Drawing.Size(141, 27);
            this.SelectRendererBtn.TabIndex = 10;
            this.SelectRendererBtn.Text = "SelectRenderer";
            this.SelectRendererBtn.UseVisualStyleBackColor = true;
            this.SelectRendererBtn.Click += new System.EventHandler(this.SelectRendererBtn_Click);
            // 
            // SelectFormBtn
            // 
            this.SelectFormBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.SelectFormBtn.Location = new System.Drawing.Point(230, 507);
            this.SelectFormBtn.Name = "SelectFormBtn";
            this.SelectFormBtn.Size = new System.Drawing.Size(141, 27);
            this.SelectFormBtn.TabIndex = 11;
            this.SelectFormBtn.Text = "SelectForm";
            this.SelectFormBtn.UseVisualStyleBackColor = true;
            this.SelectFormBtn.Click += new System.EventHandler(this.SelectFormBtn_Click);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button1.Location = new System.Drawing.Point(106, 507);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(118, 27);
            this.button1.TabIndex = 12;
            this.button1.Text = "SnapshotPic";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel,
            this.focusCtrlLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 568);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(968, 25);
            this.statusStrip1.TabIndex = 13;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(31, 20);
            this.statusLabel.Text = "OK";
            // 
            // focusCtrlLabel
            // 
            this.focusCtrlLabel.Name = "focusCtrlLabel";
            this.focusCtrlLabel.Size = new System.Drawing.Size(0, 20);
            // 
            // resetTRS
            // 
            this.resetTRS.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.resetTRS.Location = new System.Drawing.Point(263, 474);
            this.resetTRS.Name = "resetTRS";
            this.resetTRS.Size = new System.Drawing.Size(108, 27);
            this.resetTRS.TabIndex = 14;
            this.resetTRS.Text = "ResetObjTRS";
            this.resetTRS.UseVisualStyleBackColor = true;
            this.resetTRS.Click += new System.EventHandler(this.resetTRS_Click);
            // 
            // TimeScaleLabel
            // 
            this.TimeScaleLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.TimeScaleLabel.AutoSize = true;
            this.TimeScaleLabel.Location = new System.Drawing.Point(12, 414);
            this.TimeScaleLabel.Name = "TimeScaleLabel";
            this.TimeScaleLabel.Size = new System.Drawing.Size(151, 15);
            this.TimeScaleLabel.TabIndex = 15;
            this.TimeScaleLabel.Text = "TimeScale[0,1000]:";
            // 
            // TimeScaleSlider
            // 
            this.TimeScaleSlider.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.TimeScaleSlider.Location = new System.Drawing.Point(161, 413);
            this.TimeScaleSlider.Maximum = 1000;
            this.TimeScaleSlider.Name = "TimeScaleSlider";
            this.TimeScaleSlider.Size = new System.Drawing.Size(151, 56);
            this.TimeScaleSlider.TabIndex = 16;
            this.TimeScaleSlider.TickStyle = System.Windows.Forms.TickStyle.None;
            this.TimeScaleSlider.Value = 500;
            this.TimeScaleSlider.ValueChanged += new System.EventHandler(this.TimeScaleSlider_ValueChanged);
            // 
            // TimeScaleValueLabel
            // 
            this.TimeScaleValueLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.TimeScaleValueLabel.AutoSize = true;
            this.TimeScaleValueLabel.Location = new System.Drawing.Point(318, 414);
            this.TimeScaleValueLabel.Name = "TimeScaleValueLabel";
            this.TimeScaleValueLabel.Size = new System.Drawing.Size(31, 15);
            this.TimeScaleValueLabel.TabIndex = 17;
            this.TimeScaleValueLabel.Text = "500";
            // 
            // richTextBox1
            // 
            this.richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox1.Location = new System.Drawing.Point(12, 324);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(359, 87);
            this.richTextBox1.TabIndex = 20;
            this.richTextBox1.Text = resources.GetString("richTextBox1.Text");
            // 
            // ResetCamera
            // 
            this.ResetCamera.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ResetCamera.Location = new System.Drawing.Point(149, 475);
            this.ResetCamera.Name = "ResetCamera";
            this.ResetCamera.Size = new System.Drawing.Size(108, 27);
            this.ResetCamera.TabIndex = 21;
            this.ResetCamera.Text = "ResetCamera";
            this.ResetCamera.UseVisualStyleBackColor = true;
            this.ResetCamera.Click += new System.EventHandler(this.ResetCamera_Click);
            // 
            // AutoRotateCheckBox
            // 
            this.AutoRotateCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.AutoRotateCheckBox.AutoSize = true;
            this.AutoRotateCheckBox.Checked = true;
            this.AutoRotateCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.AutoRotateCheckBox.Location = new System.Drawing.Point(15, 479);
            this.AutoRotateCheckBox.Name = "AutoRotateCheckBox";
            this.AutoRotateCheckBox.Size = new System.Drawing.Size(109, 19);
            this.AutoRotateCheckBox.TabIndex = 22;
            this.AutoRotateCheckBox.Text = "AutoRotate";
            this.AutoRotateCheckBox.UseVisualStyleBackColor = true;
            this.AutoRotateCheckBox.CheckedChanged += new System.EventHandler(this.AutoRotateCheckBox_CheckedChanged);
            // 
            // DepthPictureBox
            // 
            this.DepthPictureBox.BackColor = System.Drawing.Color.Transparent;
            this.DepthPictureBox.Location = new System.Drawing.Point(340, 12);
            this.DepthPictureBox.Name = "DepthPictureBox";
            this.DepthPictureBox.Size = new System.Drawing.Size(300, 300);
            this.DepthPictureBox.TabIndex = 23;
            this.DepthPictureBox.TabStop = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(968, 593);
            this.Controls.Add(this.DepthPictureBox);
            this.Controls.Add(this.AutoRotateCheckBox);
            this.Controls.Add(this.ResetCamera);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.TimeScaleValueLabel);
            this.Controls.Add(this.TimeScaleSlider);
            this.Controls.Add(this.TimeScaleLabel);
            this.Controls.Add(this.resetTRS);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.SelectFormBtn);
            this.Controls.Add(this.SelectRendererBtn);
            this.Controls.Add(this.SelectCameraBtn);
            this.Controls.Add(this.PauseBtn);
            this.Controls.Add(this.FpsLabel1);
            this.Controls.Add(this.PropertyGrid);
            this.Controls.Add(this.PictureBox);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.Text = "Jave.Lin-SoftRenderer";
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TimeScaleSlider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DepthPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.PictureBox PictureBox;
        private System.Windows.Forms.PropertyGrid PropertyGrid;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label FpsLabel1;
        private System.Windows.Forms.Button PauseBtn;
        private System.Windows.Forms.Button SelectCameraBtn;
        private System.Windows.Forms.Button SelectRendererBtn;
        private System.Windows.Forms.Button SelectFormBtn;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.Button resetTRS;
        private System.Windows.Forms.Label TimeScaleLabel;
        private System.Windows.Forms.TrackBar TimeScaleSlider;
        private System.Windows.Forms.Label TimeScaleValueLabel;
        private System.Windows.Forms.ToolStripStatusLabel focusCtrlLabel;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button ResetCamera;
        private System.Windows.Forms.CheckBox AutoRotateCheckBox;
        private System.Windows.Forms.PictureBox DepthPictureBox;
    }
}

