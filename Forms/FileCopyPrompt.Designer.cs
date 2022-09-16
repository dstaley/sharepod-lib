namespace SharePodLib.Forms
{
    partial class FileCopyPrompt
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
            this.FileNameLabel = new System.Windows.Forms.Label();
            this.ApplyToAllFilesCheckBox = new System.Windows.Forms.CheckBox();
            this.RenameFileButton = new System.Windows.Forms.Button();
            this.SkipFileButton = new System.Windows.Forms.Button();
            this.FolderName = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.SuspendLayout();
            // 
            // FileNameLabel
            // 
            this.FileNameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.FileNameLabel.BackColor = System.Drawing.Color.White;
            this.FileNameLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FileNameLabel.Location = new System.Drawing.Point(19, 43);
            this.FileNameLabel.Name = "FileNameLabel";
            this.FileNameLabel.Size = new System.Drawing.Size(427, 15);
            this.FileNameLabel.TabIndex = 1;
            this.FileNameLabel.Text = "<filename>";
            // 
            // ApplyToAllFilesCheckBox
            // 
            this.ApplyToAllFilesCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ApplyToAllFilesCheckBox.AutoSize = true;
            this.ApplyToAllFilesCheckBox.Location = new System.Drawing.Point(68, 120);
            this.ApplyToAllFilesCheckBox.Name = "ApplyToAllFilesCheckBox";
            this.ApplyToAllFilesCheckBox.Size = new System.Drawing.Size(157, 17);
            this.ApplyToAllFilesCheckBox.TabIndex = 8;
            this.ApplyToAllFilesCheckBox.Text = "Do this for all duplicate files";
            this.ApplyToAllFilesCheckBox.UseVisualStyleBackColor = true;
            // 
            // RenameFileButton
            // 
            this.RenameFileButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.RenameFileButton.Location = new System.Drawing.Point(318, 114);
            this.RenameFileButton.Name = "RenameFileButton";
            this.RenameFileButton.Size = new System.Drawing.Size(121, 23);
            this.RenameFileButton.TabIndex = 7;
            this.RenameFileButton.Text = "Copy with new name";
            this.RenameFileButton.UseVisualStyleBackColor = true;
            this.RenameFileButton.Click += new System.EventHandler(this.RenameFileButton_Click);
            // 
            // SkipFileButton
            // 
            this.SkipFileButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.SkipFileButton.Location = new System.Drawing.Point(231, 114);
            this.SkipFileButton.Name = "SkipFileButton";
            this.SkipFileButton.Size = new System.Drawing.Size(81, 23);
            this.SkipFileButton.TabIndex = 6;
            this.SkipFileButton.Text = "Don\'t copy";
            this.SkipFileButton.UseVisualStyleBackColor = true;
            this.SkipFileButton.Click += new System.EventHandler(this.SkipFileButton_Click);
            // 
            // FolderName
            // 
            this.FolderName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.FolderName.BackColor = System.Drawing.Color.White;
            this.FolderName.Location = new System.Drawing.Point(19, 62);
            this.FolderName.Name = "FolderName";
            this.FolderName.Size = new System.Drawing.Size(427, 31);
            this.FolderName.TabIndex = 7;
            this.FolderName.Text = "<filename>";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.BackColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(1, 2);
            this.label1.Name = "label1";
            this.label1.Padding = new System.Windows.Forms.Padding(15, 15, 0, 0);
            this.label1.Size = new System.Drawing.Size(450, 94);
            this.label1.TabIndex = 8;
            this.label1.Text = "There is already a file with this name in this location.";
            // 
            // groupBox1
            // 
            this.groupBox1.Location = new System.Drawing.Point(-6, 94);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(510, 2);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            // 
            // FileCopyPrompt
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(451, 149);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.ApplyToAllFilesCheckBox);
            this.Controls.Add(this.RenameFileButton);
            this.Controls.Add(this.FolderName);
            this.Controls.Add(this.SkipFileButton);
            this.Controls.Add(this.FileNameLabel);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FileCopyPrompt";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Copy File";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label FileNameLabel;
        private System.Windows.Forms.CheckBox ApplyToAllFilesCheckBox;
        private System.Windows.Forms.Button RenameFileButton;
        private System.Windows.Forms.Button SkipFileButton;
        private System.Windows.Forms.Label FolderName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}