using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace PropertyPageBaseTest.Mocks
{
	public class TestControl : UserControl
    {
        public List<Message> MessagesAskedToProcess = new List<Message>();

        public List<int> MessagesIRecognize = new List<int>();

        public override bool PreProcessMessage(ref Message msg)
        {
            if (MessagesIRecognize.Contains(msg.Msg))
            {
                MessagesAskedToProcess.Add(msg);
                return true;
            }
            else
                return base.PreProcessMessage(ref msg);
        }
    }
}
