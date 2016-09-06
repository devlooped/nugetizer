namespace PropertyPageBaseTest.Mocks
{
    partial class TestPageViewControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.CheckBox1 = new System.Windows.Forms.CheckBox();
            this.TextBox1 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // CheckBox1
            // 
            this.CheckBox1.AutoSize = true;
            this.CheckBox1.Location = new System.Drawing.Point(3, 3);
            this.CheckBox1.Name = "CheckBox1";
            this.CheckBox1.Size = new System.Drawing.Size(80, 17);
            this.CheckBox1.TabIndex = 0;
            this.CheckBox1.Text = "checkBox1";
            this.CheckBox1.UseVisualStyleBackColor = true;
            // 
            // TextBox1
            // 
            this.TextBox1.Location = new System.Drawing.Point(3, 26);
            this.TextBox1.Name = "TextBox1";
            this.TextBox1.Size = new System.Drawing.Size(100, 20);
            this.TextBox1.TabIndex = 1;
            // 
            // TestPageViewControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.TextBox1);
            this.Controls.Add(this.CheckBox1);
            this.Name = "TestPageViewControl";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox CheckBox1;
        private System.Windows.Forms.TextBox TextBox1;
    }
}
