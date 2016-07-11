using System;
using System.Collections.Generic;
using System.Text;
using PropertyPageBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PropertyPageBaseTest.Mocks
{
	public class TestIPropertyPageUI : AbstractIPropertyPageUI
    {
        public PropertyControlMap registeringMap;
        public Dictionary<string, string> controlValues = new Dictionary<string, string>();
        public UserEditCompleteHandler editCompleteDelegate;

        public override event PropertyPageBase.UserEditCompleteHandler UserEditComplete
        {
            add 
            {
                registeringMap = (PropertyControlMap)value.Target;
                editCompleteDelegate = value;
            }
            remove
            {
                registeringMap = null;
                editCompleteDelegate = null;
            }
        }

        public override void SetControlValue(string controlName, string value)
        {
            controlValues[controlName] = value;
            if (registeringMap != null)
                Assert.Fail("Setting a control value when hooked up to the edit event will cause errors");
        }

        public void SimulateUserEdit(string controlName, string newValue)
        {
            editCompleteDelegate(controlName, newValue);
        }
    }
}
