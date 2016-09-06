using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace PropertyPageBaseTest.Mocks
{
	public class TestTextBox : TextBox
    {
        public void ValidateNow()
        {
            EventArgs eventArgs = new EventArgs();
            OnValidated(eventArgs);
        }
    }
}
