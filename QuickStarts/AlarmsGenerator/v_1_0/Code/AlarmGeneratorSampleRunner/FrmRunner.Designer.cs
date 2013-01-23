namespace AlarmGeneratorSampleRunner
{
    partial class FrmRunner
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmRunner));
            this.btnStart = new System.Windows.Forms.Button();
            this.btnNewMachine = new System.Windows.Forms.Button();
            this.btnNewPanel = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(34, 12);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(106, 55);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "&Start Emulation";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnNewMachine
            // 
            this.btnNewMachine.Enabled = false;
            this.btnNewMachine.Location = new System.Drawing.Point(324, 12);
            this.btnNewMachine.Name = "btnNewMachine";
            this.btnNewMachine.Size = new System.Drawing.Size(106, 55);
            this.btnNewMachine.TabIndex = 1;
            this.btnNewMachine.Text = "Add &machine";
            this.btnNewMachine.UseVisualStyleBackColor = true;
            this.btnNewMachine.Click += new System.EventHandler(this.btnNewMachine_Click);
            // 
            // btnNewPanel
            // 
            this.btnNewPanel.Enabled = false;
            this.btnNewPanel.Location = new System.Drawing.Point(466, 12);
            this.btnNewPanel.Name = "btnNewPanel";
            this.btnNewPanel.Size = new System.Drawing.Size(106, 55);
            this.btnNewPanel.TabIndex = 2;
            this.btnNewPanel.Text = "Add &panel";
            this.btnNewPanel.UseVisualStyleBackColor = true;
            this.btnNewPanel.Click += new System.EventHandler(this.btnNewPanel_Click);
            // 
            // btnStop
            // 
            this.btnStop.Enabled = false;
            this.btnStop.Location = new System.Drawing.Point(174, 12);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(106, 55);
            this.btnStop.TabIndex = 3;
            this.btnStop.Text = "S&top Emulation";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // FrmRunner
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(611, 79);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnNewPanel);
            this.Controls.Add(this.btnNewMachine);
            this.Controls.Add(this.btnStart);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmRunner";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "QuickStart Runner";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.FrmRunner_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnNewMachine;
        private System.Windows.Forms.Button btnNewPanel;
        private System.Windows.Forms.Button btnStop;
    }
}

