namespace SoftRenderer
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
            this.PictureBox = new System.Windows.Forms.PictureBox();
            this.PropertyGrid = new System.Windows.Forms.PropertyGrid();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.FpsLabel1 = new System.Windows.Forms.Label();
            this.PauseBtn = new System.Windows.Forms.Button();
            this.SelectCameraBtn = new System.Windows.Forms.Button();
            this.SelectRendererBtn = new System.Windows.Forms.Button();
            this.SelectFormBtn = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // PictureBox
            // 
            this.PictureBox.BackColor = System.Drawing.SystemColors.ActiveCaption;
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
            this.PropertyGrid.Location = new System.Drawing.Point(377, 12);
            this.PropertyGrid.Name = "PropertyGrid";
            this.PropertyGrid.Size = new System.Drawing.Size(169, 358);
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
            this.FpsLabel1.Location = new System.Drawing.Point(12, 325);
            this.FpsLabel1.Name = "FpsLabel1";
            this.FpsLabel1.Size = new System.Drawing.Size(31, 15);
            this.FpsLabel1.TabIndex = 7;
            this.FpsLabel1.Text = "FPS";
            // 
            // PauseBtn
            // 
            this.PauseBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.PauseBtn.Location = new System.Drawing.Point(12, 343);
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
            this.SelectCameraBtn.Location = new System.Drawing.Point(106, 343);
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
            this.SelectRendererBtn.Location = new System.Drawing.Point(230, 343);
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
            this.SelectFormBtn.Location = new System.Drawing.Point(230, 310);
            this.SelectFormBtn.Name = "SelectFormBtn";
            this.SelectFormBtn.Size = new System.Drawing.Size(141, 27);
            this.SelectFormBtn.TabIndex = 11;
            this.SelectFormBtn.Text = "SelectForm";
            this.SelectFormBtn.UseVisualStyleBackColor = true;
            this.SelectFormBtn.Click += new System.EventHandler(this.SelectFormBtn_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(558, 382);
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
    }
}

