using System;
using System.Collections.Generic;
using System.Text;
using PropertyPageBase;

namespace PropertyPageBaseTest.Mocks
{
	public class TestPageView : PageView
    {
        private System.Windows.Forms.CheckBox Control2;
        private System.Windows.Forms.TextBox Control1;
        private System.Windows.Forms.Label label1;
        private TestTextBox Control3;
        public PropertyControlTable Table;

        public TestPageView()
            : base()
        {
            InitializeComponent();
        }

        public TestPageView(IPageViewSite site)
            : base(site)
        {
            InitializeComponent();
        }

        protected override PropertyControlTable PropertyControlTable
        {
            get {
                Table = new PropertyControlTable();
                Table.Add("Property1", "Control1");
                Table.Add("Property2", "Control2");
                Table.Add("Property3", "Control3");
                return Table;
            }
        }

        private void InitializeComponent()
        {
            this.Control2 = new System.Windows.Forms.CheckBox();
            this.Control1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.Control3 = new PropertyPageBaseTest.Mocks.TestTextBox();
            this.SuspendLayout();
            // 
            // Control2
            // 
            this.Control2.AutoSize = true;
            this.Control2.Location = new System.Drawing.Point(3, 29);
            this.Control2.Name = "Control2";
            this.Control2.Size = new System.Drawing.Size(80, 17);
            this.Control2.TabIndex = 0;
            this.Control2.Text = "checkBox1";
            this.Control2.UseVisualStyleBackColor = true;
            // 
            // Control1
            // 
            this.Control1.Location = new System.Drawing.Point(3, 3);
            this.Control1.Name = "Control1";
            this.Control1.Size = new System.Drawing.Size(100, 20);
            this.Control1.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 53);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "label1";
            // 
            // Control3
            // 
            this.Control3.Location = new System.Drawing.Point(4, 70);
            this.Control3.Name = "Control3";
            this.Control3.Size = new System.Drawing.Size(100, 20);
            this.Control3.TabIndex = 3;
            // 
            // TestPageView
            // 
            this.Controls.Add(this.Control3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Control1);
            this.Controls.Add(this.Control2);
            this.Name = "TestPageView";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
