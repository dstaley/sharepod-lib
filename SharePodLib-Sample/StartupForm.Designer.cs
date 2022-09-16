namespace SharePodLib_Sample
{
    partial class StartupForm
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
            this.button1 = new System.Windows.Forms.Button();
            this.ConnectEventsButton = new System.Windows.Forms.Button();
            this.SharePodCloneButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(45, 108);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(194, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Find all connected iPods";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // ConnectEventsButton
            // 
            this.ConnectEventsButton.Location = new System.Drawing.Point(46, 67);
            this.ConnectEventsButton.Name = "ConnectEventsButton";
            this.ConnectEventsButton.Size = new System.Drawing.Size(194, 23);
            this.ConnectEventsButton.TabIndex = 1;
            this.ConnectEventsButton.Text = "iPod connect/disconnect events";
            this.ConnectEventsButton.UseVisualStyleBackColor = true;
            this.ConnectEventsButton.Click += new System.EventHandler(this.ConnectEventsButton_Click);
            // 
            // SharePodCloneButton
            // 
            this.SharePodCloneButton.Location = new System.Drawing.Point(47, 25);
            this.SharePodCloneButton.Name = "SharePodCloneButton";
            this.SharePodCloneButton.Size = new System.Drawing.Size(194, 23);
            this.SharePodCloneButton.TabIndex = 2;
            this.SharePodCloneButton.Text = "SharePod clone";
            this.SharePodCloneButton.UseVisualStyleBackColor = true;
            this.SharePodCloneButton.Click += new System.EventHandler(this.SharePodCloneButton_Click);
            // 
            // StartupForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(287, 157);
            this.Controls.Add(this.SharePodCloneButton);
            this.Controls.Add(this.ConnectEventsButton);
            this.Controls.Add(this.button1);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "StartupForm";
            this.Text = "SharePodLib Sample application";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button ConnectEventsButton;
        private System.Windows.Forms.Button SharePodCloneButton;
    }
}