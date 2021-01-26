namespace SandpiperInspector
{
    partial class FormNewSlice
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxSliceDescription = new System.Windows.Forms.TextBox();
            this.buttonNewSliceCancel = new System.Windows.Forms.Button();
            this.buttonNewSliceCreate = new System.Windows.Forms.Button();
            this.textBoxSliceType = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxMetadata = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Description";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 38);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 17);
            this.label2.TabIndex = 1;
            this.label2.Text = "Slice Type";
            // 
            // textBoxSliceDescription
            // 
            this.textBoxSliceDescription.Location = new System.Drawing.Point(97, 6);
            this.textBoxSliceDescription.Name = "textBoxSliceDescription";
            this.textBoxSliceDescription.Size = new System.Drawing.Size(378, 22);
            this.textBoxSliceDescription.TabIndex = 2;
            // 
            // buttonNewSliceCancel
            // 
            this.buttonNewSliceCancel.Location = new System.Drawing.Point(307, 176);
            this.buttonNewSliceCancel.Name = "buttonNewSliceCancel";
            this.buttonNewSliceCancel.Size = new System.Drawing.Size(87, 26);
            this.buttonNewSliceCancel.TabIndex = 5;
            this.buttonNewSliceCancel.Text = "Cancel";
            this.buttonNewSliceCancel.UseVisualStyleBackColor = true;
            this.buttonNewSliceCancel.Click += new System.EventHandler(this.buttonNewSliceCancel_Click);
            // 
            // buttonNewSliceCreate
            // 
            this.buttonNewSliceCreate.Location = new System.Drawing.Point(400, 176);
            this.buttonNewSliceCreate.Name = "buttonNewSliceCreate";
            this.buttonNewSliceCreate.Size = new System.Drawing.Size(75, 26);
            this.buttonNewSliceCreate.TabIndex = 6;
            this.buttonNewSliceCreate.Text = "Create";
            this.buttonNewSliceCreate.UseVisualStyleBackColor = true;
            this.buttonNewSliceCreate.Click += new System.EventHandler(this.buttonNewSliceCreate_Click);
            // 
            // textBoxSliceType
            // 
            this.textBoxSliceType.Location = new System.Drawing.Point(97, 35);
            this.textBoxSliceType.Name = "textBoxSliceType";
            this.textBoxSliceType.Size = new System.Drawing.Size(378, 22);
            this.textBoxSliceType.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 68);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(67, 17);
            this.label3.TabIndex = 6;
            this.label3.Text = "Metadata";
            // 
            // textBoxMetadata
            // 
            this.textBoxMetadata.Location = new System.Drawing.Point(97, 68);
            this.textBoxMetadata.Multiline = true;
            this.textBoxMetadata.Name = "textBoxMetadata";
            this.textBoxMetadata.Size = new System.Drawing.Size(378, 102);
            this.textBoxMetadata.TabIndex = 4;
            // 
            // FormNewSlice
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(487, 214);
            this.Controls.Add(this.textBoxMetadata);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBoxSliceType);
            this.Controls.Add(this.buttonNewSliceCreate);
            this.Controls.Add(this.buttonNewSliceCancel);
            this.Controls.Add(this.textBoxSliceDescription);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormNewSlice";
            this.Text = "Create New Slice";
            this.Load += new System.EventHandler(this.FormNewSlice_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxSliceDescription;
        private System.Windows.Forms.Button buttonNewSliceCancel;
        private System.Windows.Forms.Button buttonNewSliceCreate;
        private System.Windows.Forms.TextBox textBoxSliceType;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxMetadata;
    }
}