namespace SharePodLib_Sample.Forms
{
	partial class ExporterForm
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
			this.EventsListBox = new System.Windows.Forms.ListBox();
			this.StopButton = new System.Windows.Forms.Button();
			this.CloseButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// EventsListBox
			// 
			this.EventsListBox.FormattingEnabled = true;
			this.EventsListBox.Location = new System.Drawing.Point(12, 25);
			this.EventsListBox.Name = "EventsListBox";
			this.EventsListBox.Size = new System.Drawing.Size(384, 134);
			this.EventsListBox.TabIndex = 0;
			// 
			// StopButton
			// 
			this.StopButton.Location = new System.Drawing.Point(240, 165);
			this.StopButton.Name = "StopButton";
			this.StopButton.Size = new System.Drawing.Size(75, 23);
			this.StopButton.TabIndex = 1;
			this.StopButton.Text = "Stop";
			this.StopButton.UseVisualStyleBackColor = true;
			this.StopButton.Click += new System.EventHandler(this.StopButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Enabled = false;
			this.CloseButton.Location = new System.Drawing.Point(321, 165);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = new System.Drawing.Size(75, 23);
			this.CloseButton.TabIndex = 2;
			this.CloseButton.Text = "Close";
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// ExporterForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(408, 196);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.StopButton);
			this.Controls.Add(this.EventsListBox);
			this.Name = "ExporterForm";
			this.Text = "ExporterForm";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListBox EventsListBox;
		private System.Windows.Forms.Button StopButton;
		private System.Windows.Forms.Button CloseButton;
	}
}