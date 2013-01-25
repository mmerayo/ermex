namespace StockBoyPanel
{
    partial class FrmFill
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmFill));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.pbOrange = new System.Windows.Forms.PictureBox();
            this.pbRed = new System.Windows.Forms.PictureBox();
            this.pbGreen = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.btnCommit = new System.Windows.Forms.Button();
            this.udRed = new System.Windows.Forms.NumericUpDown();
            this.udOrange = new System.Windows.Forms.NumericUpDown();
            this.udGreen = new System.Windows.Forms.NumericUpDown();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbOrange)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbGreen)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udRed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udOrange)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udGreen)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.udGreen);
            this.groupBox1.Controls.Add(this.udOrange);
            this.groupBox1.Controls.Add(this.udRed);
            this.groupBox1.Controls.Add(this.pbOrange);
            this.groupBox1.Controls.Add(this.pbRed);
            this.groupBox1.Controls.Add(this.pbGreen);
            this.groupBox1.Location = new System.Drawing.Point(125, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(109, 168);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Add Items";
            // 
            // pbOrange
            // 
            this.pbOrange.Image = global::StockBoyPanel.Properties.Resources.green;
            this.pbOrange.InitialImage = null;
            this.pbOrange.Location = new System.Drawing.Point(15, 113);
            this.pbOrange.Name = "pbOrange";
            this.pbOrange.Size = new System.Drawing.Size(36, 41);
            this.pbOrange.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbOrange.TabIndex = 6;
            this.pbOrange.TabStop = false;
            // 
            // pbRed
            // 
            this.pbRed.Image = global::StockBoyPanel.Properties.Resources.orange;
            this.pbRed.InitialImage = null;
            this.pbRed.Location = new System.Drawing.Point(15, 66);
            this.pbRed.Name = "pbRed";
            this.pbRed.Size = new System.Drawing.Size(36, 41);
            this.pbRed.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbRed.TabIndex = 4;
            this.pbRed.TabStop = false;
            // 
            // pbGreen
            // 
            this.pbGreen.Image = global::StockBoyPanel.Properties.Resources.red;
            this.pbGreen.InitialImage = null;
            this.pbGreen.Location = new System.Drawing.Point(15, 19);
            this.pbGreen.Name = "pbGreen";
            this.pbGreen.Size = new System.Drawing.Size(36, 41);
            this.pbGreen.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbGreen.TabIndex = 2;
            this.pbGreen.TabStop = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(12, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(107, 168);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            // 
            // btnCommit
            // 
            this.btnCommit.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCommit.Location = new System.Drawing.Point(131, 187);
            this.btnCommit.Name = "btnCommit";
            this.btnCommit.Size = new System.Drawing.Size(102, 28);
            this.btnCommit.TabIndex = 4;
            this.btnCommit.Text = "Accept";
            this.btnCommit.UseVisualStyleBackColor = true;
            this.btnCommit.Click += new System.EventHandler(this.btnCommit_Click);
            // 
            // udRed
            // 
            this.udRed.Location = new System.Drawing.Point(57, 30);
            this.udRed.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.udRed.Name = "udRed";
            this.udRed.ReadOnly = true;
            this.udRed.Size = new System.Drawing.Size(37, 20);
            this.udRed.TabIndex = 7;
            // 
            // udOrange
            // 
            this.udOrange.Location = new System.Drawing.Point(57, 76);
            this.udOrange.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.udOrange.Name = "udOrange";
            this.udOrange.ReadOnly = true;
            this.udOrange.Size = new System.Drawing.Size(37, 20);
            this.udOrange.TabIndex = 8;
            // 
            // udGreen
            // 
            this.udGreen.Location = new System.Drawing.Point(57, 122);
            this.udGreen.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.udGreen.Name = "udGreen";
            this.udGreen.ReadOnly = true;
            this.udGreen.Size = new System.Drawing.Size(37, 20);
            this.udGreen.TabIndex = 9;
            // 
            // FrmFill
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(263, 227);
            this.Controls.Add(this.btnCommit);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmFill";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Fill drinks";
            this.TopMost = true;
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbOrange)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbGreen)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udRed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udOrange)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udGreen)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.PictureBox pbOrange;
        private System.Windows.Forms.PictureBox pbRed;
        private System.Windows.Forms.PictureBox pbGreen;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button btnCommit;
        private System.Windows.Forms.NumericUpDown udGreen;
        private System.Windows.Forms.NumericUpDown udOrange;
        private System.Windows.Forms.NumericUpDown udRed;
    }
}