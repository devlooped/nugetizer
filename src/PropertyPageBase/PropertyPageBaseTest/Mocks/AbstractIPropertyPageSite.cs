using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.OLE.Interop;

namespace PropertyPageBaseTest.Mocks
{
    abstract class AbstractIPropertyPageSite : IPropertyPageSite, System.IServiceProvider
    {
        #region IPropertyPageSite Members

        virtual public void GetLocaleID(out uint pLocaleID)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public void GetPageContainer(out object ppunk)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public void OnStatusChange(uint dwFlags)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public int TranslateAccelerator(MSG[] pMsg)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        #region IServiceProvider Members

        virtual public object GetService(Type serviceType)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
