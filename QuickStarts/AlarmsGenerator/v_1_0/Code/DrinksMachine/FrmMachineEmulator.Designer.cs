namespace DrinksMachine
{
    partial class FrmMachineEmulator
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMachineEmulator));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblOrangeStock = new System.Windows.Forms.Label();
            this.pbOrange = new System.Windows.Forms.PictureBox();
            this.lblRedStock = new System.Windows.Forms.Label();
            this.pbRed = new System.Windows.Forms.PictureBox();
            this.lblGreenStock = new System.Windows.Forms.Label();
            this.pbGreen = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnBuyOrange = new System.Windows.Forms.Button();
            this.btnBuyRed = new System.Windows.Forms.Button();
            this.btnBuyGreen = new System.Windows.Forms.Button();
            this.tips = new System.Windows.Forms.ToolTip(this.components);
            this.lblInfo = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.button1 = new System.Windows.Forms.Button();
            this.txtName = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbOrange)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbGreen)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lblOrangeStock);
            this.groupBox1.Controls.Add(this.pbOrange);
            this.groupBox1.Controls.Add(this.lblRedStock);
            this.groupBox1.Controls.Add(this.pbRed);
            this.groupBox1.Controls.Add(this.lblGreenStock);
            this.groupBox1.Controls.Add(this.pbGreen);
            this.groupBox1.Location = new System.Drawing.Point(160, 63);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(109, 168);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Stock";
            // 
            // lblOrangeStock
            // 
            this.lblOrangeStock.AutoSize = true;
            this.lblOrangeStock.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOrangeStock.Location = new System.Drawing.Point(57, 125);
            this.lblOrangeStock.Name = "lblOrangeStock";
            this.lblOrangeStock.Size = new System.Drawing.Size(51, 16);
            this.lblOrangeStock.TabIndex = 7;
            this.lblOrangeStock.Text = "label1";
            // 
            // pbOrange
            // 
            this.pbOrange.Image = global::DrinksMachine.Properties.Resources.orange;
            this.pbOrange.InitialImage = null;
            this.pbOrange.Location = new System.Drawing.Point(15, 113);
            this.pbOrange.Name = "pbOrange";
            this.pbOrange.Size = new System.Drawing.Size(36, 41);
            this.pbOrange.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbOrange.TabIndex = 6;
            this.pbOrange.TabStop = false;
            // 
            // lblRedStock
            // 
            this.lblRedStock.AutoSize = true;
            this.lblRedStock.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRedStock.Location = new System.Drawing.Point(57, 78);
            this.lblRedStock.Name = "lblRedStock";
            this.lblRedStock.Size = new System.Drawing.Size(51, 16);
            this.lblRedStock.TabIndex = 5;
            this.lblRedStock.Text = "label1";
            // 
            // pbRed
            // 
            this.pbRed.Image = global::DrinksMachine.Properties.Resources.red;
            this.pbRed.InitialImage = null;
            this.pbRed.Location = new System.Drawing.Point(15, 66);
            this.pbRed.Name = "pbRed";
            this.pbRed.Size = new System.Drawing.Size(36, 41);
            this.pbRed.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbRed.TabIndex = 4;
            this.pbRed.TabStop = false;
            // 
            // lblGreenStock
            // 
            this.lblGreenStock.AutoSize = true;
            this.lblGreenStock.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGreenStock.Location = new System.Drawing.Point(57, 31);
            this.lblGreenStock.Name = "lblGreenStock";
            this.lblGreenStock.Size = new System.Drawing.Size(51, 16);
            this.lblGreenStock.TabIndex = 3;
            this.lblGreenStock.Text = "label1";
            // 
            // pbGreen
            // 
            this.pbGreen.Image = global::DrinksMachine.Properties.Resources.green;
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
            this.pictureBox1.Image = global::DrinksMachine.Properties.Resources.Machine;
            this.pictureBox1.Location = new System.Drawing.Point(13, 6);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(128, 232);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnBuyOrange);
            this.groupBox2.Controls.Add(this.btnBuyRed);
            this.groupBox2.Controls.Add(this.btnBuyGreen);
            this.groupBox2.Location = new System.Drawing.Point(288, 63);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(109, 168);
            this.groupBox2.TabIndex = 8;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Buy drink";
            // 
            // btnBuyOrange
            // 
            this.btnBuyOrange.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBuyOrange.Location = new System.Drawing.Point(29, 110);
            this.btnBuyOrange.Name = "btnBuyOrange";
            this.btnBuyOrange.Size = new System.Drawing.Size(50, 42);
            this.btnBuyOrange.TabIndex = 2;
            this.tips.SetToolTip(this.btnBuyOrange, "Click to buy drink and decrease the stock");
            this.btnBuyOrange.UseVisualStyleBackColor = true;
            this.btnBuyOrange.Click += new System.EventHandler(this.Buy_Drink_Click);
            // 
            // btnBuyRed
            // 
            this.btnBuyRed.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBuyRed.Location = new System.Drawing.Point(29, 65);
            this.btnBuyRed.Name = "btnBuyRed";
            this.btnBuyRed.Size = new System.Drawing.Size(50, 42);
            this.btnBuyRed.TabIndex = 1;
            this.tips.SetToolTip(this.btnBuyRed, "Click to buy drink and decrease the stock");
            this.btnBuyRed.UseVisualStyleBackColor = true;
            this.btnBuyRed.Click += new System.EventHandler(this.Buy_Drink_Click);
            // 
            // btnBuyGreen
            // 
            this.btnBuyGreen.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBuyGreen.Location = new System.Drawing.Point(29, 19);
            this.btnBuyGreen.Name = "btnBuyGreen";
            this.btnBuyGreen.Size = new System.Drawing.Size(50, 42);
            this.btnBuyGreen.TabIndex = 0;
            this.tips.SetToolTip(this.btnBuyGreen, "Click to buy drink and decrease the stock");
            this.btnBuyGreen.UseVisualStyleBackColor = true;
            this.btnBuyGreen.Click += new System.EventHandler(this.Buy_Drink_Click);
            // 
            // lblInfo
            // 
            this.lblInfo.AutoSize = true;
            this.lblInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInfo.ForeColor = System.Drawing.Color.Navy;
            this.lblInfo.Location = new System.Drawing.Point(157, 237);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(51, 16);
            this.lblInfo.TabIndex = 9;
            this.lblInfo.Text = "label1";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.button1);
            this.groupBox3.Controls.Add(this.txtName);
            this.groupBox3.Location = new System.Drawing.Point(160, 6);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(237, 51);
            this.groupBox3.TabIndex = 10;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Machine name:";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(157, 19);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(61, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Update";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.UpdateName_Click);
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(15, 19);
            this.txtName.MaxLength = 15;
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(122, 20);
            this.txtName.TabIndex = 0;
            // 
            // FrmMachineEmulator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(640, 262);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.pictureBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmMachineEmulator";
            this.Text = "Form1";
            this.Activated += new System.EventHandler(this.FrmMachineEmulator_Activated);
            this.Load += new System.EventHandler(this.FrmMachineEmulator_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbOrange)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbRed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbGreen)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.PictureBox pbGreen;
        private System.Windows.Forms.Label lblOrangeStock;
        private System.Windows.Forms.PictureBox pbOrange;
        private System.Windows.Forms.Label lblRedStock;
        private System.Windows.Forms.PictureBox pbRed;
        private System.Windows.Forms.Label lblGreenStock;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnBuyOrange;
        private System.Windows.Forms.Button btnBuyRed;
        private System.Windows.Forms.Button btnBuyGreen;
        private System.Windows.Forms.ToolTip tips;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox txtName;
    }
}

