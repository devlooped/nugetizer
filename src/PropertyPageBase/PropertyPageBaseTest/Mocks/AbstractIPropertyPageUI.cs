using System;
using System.Collections.Generic;
using System.Text;
using PropertyPageBase;

namespace PropertyPageBaseTest.Mocks
{
	public abstract class AbstractIPropertyPageUI : IPropertyPageUI
    {
        #region IPropertyPageUI Members

        virtual public event UserEditCompleteHandler UserEditComplete;

        virtual public string GetControlValue(string controlName)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public void SetControlValue(string controlName, string value)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
