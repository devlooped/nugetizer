using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.Shell.Interop;

namespace PropertyPageBaseTest.Mocks
{
    abstract class AbstractIVsBrowseObject : IVsBrowseObject
    {
        #region IVsBrowseObject Members

        virtual public int GetProjectItem(out IVsHierarchy pHier, out uint pItemid)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
