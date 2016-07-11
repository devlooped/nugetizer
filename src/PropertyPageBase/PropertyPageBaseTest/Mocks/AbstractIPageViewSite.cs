using System;
using System.Collections.Generic;
using System.Text;
using PropertyPageBase;

namespace PropertyPageBaseTest.Mocks
{
    public abstract class AbstractIPageViewSite : IPageViewSite
    {
        #region IPageViewSite Members

        virtual public void PropertyChanged(string propertyName, string propertyValue)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public string GetValueForProperty(string propertyName)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
