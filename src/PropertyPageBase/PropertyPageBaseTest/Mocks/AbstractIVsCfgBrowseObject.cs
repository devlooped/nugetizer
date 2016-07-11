using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.Shell.Interop;

namespace PropertyPageBaseTest.Mocks
{
    abstract class AbstractIVsCfgBrowseObject : IVsCfgBrowseObject
    {
        #region IVsCfgBrowseObject Members

        virtual public int GetCfg(out IVsCfg ppCfg)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public int GetProjectItem(out IVsHierarchy pHier, out uint pItemid)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
