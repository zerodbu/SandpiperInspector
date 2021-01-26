namespace SandpiperInspector
{
    partial class FormSelectSlice
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
            this.buttonOk = new System.Windows.Forms.Button();
            this.listBoxSlices = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBoxApplyToAll = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // buttonOk
            // 
            this.buttonOk.Location = new System.Drawing.Point(527, 245);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 29);
            this.buttonOk.TabIndex = 0;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // listBoxSlices
            // 
            this.listBoxSlices.FormattingEnabled = true;
            this.listBoxSlices.ItemHeight = 16;
            this.listBoxSlices.Location = new System.Drawing.Point(12, 43);
            this.listBoxSlices.Name = "listBoxSlices";
            this.listBoxSlices.Size = new System.Drawing.Size(590, 196);
            this.listBoxSlices.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(464, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "New files we found in the local grain cache folder. Assign them to a slice.";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // checkBoxApplyToAll
            // 
            this.checkBoxApplyToAll.AutoSize = true;
            this.checkBoxApplyToAll.Location = new System.Drawing.Point(12, 250);
            this.checkBoxApplyToAll.Name = "checkBoxApplyToAll";
            this.checkBoxApplyToAll.Size = new System.Drawing.Size(262, 21);
            this.checkBoxApplyToAll.TabIndex = 3;
            this.checkBoxApplyToAll.Text = "Put all unassigned grains in this slice";
            this.checkBoxApplyToAll.UseVisualStyleBackColor = true;
            // 
            // FormSelectSlice
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(611, 285);
            this.Controls.Add(this.checkBoxApplyToAll);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.listBoxSlices);
            this.Controls.Add(this.buttonOk);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(400, 250);
            this.Name = "FormSelectSlice";
            this.Text = "Select Slice";
            this.Load += new System.EventHandler(this.FormSelectSlice_Load);
            this.Resize += new System.EventHandler(this.FormSelectSlice_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.ListBox listBoxSlices;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBoxApplyToAll;
    }
}