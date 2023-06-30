namespace YoloV8Net.Segment
{
    partial class FrmSegment
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            menuStrip1 = new MenuStrip();
            tsmFile = new ToolStripMenuItem();
            btnOpenImg = new ToolStripMenuItem();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            lblResult = new ToolStripStatusLabel();
            statusStrip1 = new StatusStrip();
            toolStripStatusLabel2 = new ToolStripStatusLabel();
            pictureBox1 = new PictureBox();
            menuStrip1.SuspendLayout();
            statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(40, 40);
            menuStrip1.Items.AddRange(new ToolStripItem[] { tsmFile });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new Padding(5, 2, 0, 2);
            menuStrip1.Size = new Size(1467, 39);
            menuStrip1.TabIndex = 8;
            menuStrip1.Text = "menuStrip1";
            // 
            // tsmFile
            // 
            tsmFile.DropDownItems.AddRange(new ToolStripItem[] { btnOpenImg });
            tsmFile.Name = "tsmFile";
            tsmFile.Size = new Size(73, 35);
            tsmFile.Text = "File";
            // 
            // btnOpenImg
            // 
            btnOpenImg.Name = "btnOpenImg";
            btnOpenImg.Size = new Size(289, 44);
            btnOpenImg.Text = "Open Image";
            btnOpenImg.Click += btnOpenImg_Click;
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(332, 31);
            toolStripStatusLabel1.Text = "https://docs.ultralytics.com/";
            // 
            // lblResult
            // 
            lblResult.Name = "lblResult";
            lblResult.Size = new Size(44, 31);
            lblResult.Text = "---";
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new Size(32, 32);
            statusStrip1.Items.AddRange(new ToolStripItem[] { lblResult, toolStripStatusLabel1, toolStripStatusLabel2 });
            statusStrip1.Location = new Point(0, 911);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(1467, 41);
            statusStrip1.TabIndex = 10;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel2
            // 
            toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            toolStripStatusLabel2.Size = new Size(235, 31);
            toolStripStatusLabel2.Text = "Net Version By Zdy";
            // 
            // pictureBox1
            // 
            pictureBox1.Dock = DockStyle.Fill;
            pictureBox1.Location = new Point(0, 39);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(1467, 872);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 11;
            pictureBox1.TabStop = false;
            // 
            // FrmSegment
            // 
            AutoScaleDimensions = new SizeF(14F, 31F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1467, 952);
            Controls.Add(pictureBox1);
            Controls.Add(menuStrip1);
            Controls.Add(statusStrip1);
            Name = "FrmSegment";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Yolo V8 Net Segment";
            Load += FrmSegment_Load;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem tsmFile;
        private ToolStripMenuItem btnOpenImg;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private ToolStripStatusLabel lblResult;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel toolStripStatusLabel2;
        private PictureBox pictureBox1;
    }
}