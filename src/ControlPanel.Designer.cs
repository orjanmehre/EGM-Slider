namespace EgmSmallTest
{
    partial class ControlPanel
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
            this.trackBarHeight = new System.Windows.Forms.TrackBar();
            this.lblGlider = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblHeight = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarHeight)).BeginInit();
            this.SuspendLayout();
            // 
            // trackBarHeight
            // 
            this.trackBarHeight.Location = new System.Drawing.Point(51, 75);
            this.trackBarHeight.Maximum = 200;
            this.trackBarHeight.Minimum = 0;
            this.trackBarHeight.Name = "trackBarHeight";
            this.trackBarHeight.Size = new System.Drawing.Size(202, 45);
            this.trackBarHeight.TabIndex = 0;
            this.trackBarHeight.TickFrequency = 50;
            this.trackBarHeight.Value = 0;
            this.trackBarHeight.ValueChanged += new System.EventHandler(this.trackBarHeight_ValueChanged);
            // 
            // lblGlider
            // 
            this.lblGlider.AutoSize = true;
            this.lblGlider.Location = new System.Drawing.Point(48, 46);
            this.lblGlider.Name = "lblGlider";
            this.lblGlider.Size = new System.Drawing.Size(41, 13);
            this.lblGlider.TabIndex = 1;
            this.lblGlider.Text = "Height:";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 23);
            this.label1.TabIndex = 0;
            // 
            // lblHeight
            // 
            this.lblHeight.AutoSize = true;
            this.lblHeight.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeight.Location = new System.Drawing.Point(107, 135);
            this.lblHeight.Name = "lblHeight";
            this.lblHeight.Size = new System.Drawing.Size(0, 25);
            this.lblHeight.TabIndex = 2;
            // 
            // ControlPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(302, 207);
            this.Controls.Add(this.lblHeight);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblGlider);
            this.Controls.Add(this.trackBarHeight);
            this.Name = "ControlPanel";
            this.Text = "ControlPanel";
            ((System.ComponentModel.ISupportInitialize)(this.trackBarHeight)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TrackBar trackBarHeight;
        private System.Windows.Forms.Label lblGlider;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblHeight;
    }
}