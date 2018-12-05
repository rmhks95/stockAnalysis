namespace stockAnalysis
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.openFile = new System.Windows.Forms.Button();
            this.uxOpenFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.AddCriteria = new System.Windows.Forms.Button();
            this.openCriteriaDialog = new System.Windows.Forms.OpenFileDialog();
            this.SuspendLayout();
            // 
            // openFile
            // 
            this.openFile.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.openFile.FlatAppearance.BorderSize = 0;
            this.openFile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.openFile.Font = new System.Drawing.Font("Lato Semibold", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.openFile.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.openFile.Location = new System.Drawing.Point(25, 25);
            this.openFile.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.openFile.Name = "openFile";
            this.openFile.Size = new System.Drawing.Size(380, 85);
            this.openFile.TabIndex = 0;
            this.openFile.Text = "Aggregate File(s)";
            this.openFile.UseVisualStyleBackColor = false;
            this.openFile.Click += new System.EventHandler(this.openFile_Click);
            // 
            // uxOpenFileDialog
            // 
            this.uxOpenFileDialog.FileName = "openFileDialog1";
            this.uxOpenFileDialog.Filter = "CSV Files (*.csv)|*csv";
            this.uxOpenFileDialog.Multiselect = true;
            // 
            // AddCriteria
            // 
            this.AddCriteria.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.AddCriteria.FlatAppearance.BorderSize = 0;
            this.AddCriteria.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.AddCriteria.Font = new System.Drawing.Font("Lato Semibold", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AddCriteria.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.AddCriteria.Location = new System.Drawing.Point(25, 135);
            this.AddCriteria.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.AddCriteria.Name = "AddCriteria";
            this.AddCriteria.Size = new System.Drawing.Size(380, 85);
            this.AddCriteria.TabIndex = 1;
            this.AddCriteria.Text = "Add Criteria Set(s)";
            this.AddCriteria.UseVisualStyleBackColor = false;
            this.AddCriteria.Click += new System.EventHandler(this.AddCriteria_Click);
            // 
            // openCriteriaDialog
            // 
            this.openCriteriaDialog.FileName = "openFileDialog1";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLight;
            this.ClientSize = new System.Drawing.Size(429, 244);
            this.Controls.Add(this.AddCriteria);
            this.Controls.Add(this.openFile);
            this.Font = new System.Drawing.Font("Lato Semibold", 7.875F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Name = "Form1";
            this.RightToLeftLayout = true;
            this.Text = "Stock Analysis";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button openFile;
        private System.Windows.Forms.OpenFileDialog uxOpenFileDialog;
        private System.Windows.Forms.Button AddCriteria;
        private System.Windows.Forms.OpenFileDialog openCriteriaDialog;
    }
}

