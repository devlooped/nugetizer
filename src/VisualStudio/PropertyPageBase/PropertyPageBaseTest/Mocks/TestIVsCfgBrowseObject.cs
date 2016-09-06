using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace PropertyPageBaseTest.Mocks
{
    class TestIVsCfgBrowseObject : AbstractIVsCfgBrowseObject, IVsBrowseObject, IVsCfgBrowseObject
    {
        public TestIVsHierarchy Hierarchy;
        public string ConfigName;

        int IVsCfgBrowseObject.GetProjectItem(out Microsoft.VisualStudio.Shell.Interop.IVsHierarchy pHier, out uint pItemid)
        {
            pHier = Hierarchy;
            pItemid = 0;
            return VSConstants.S_OK;
        }

        int IVsBrowseObject.GetProjectItem(out IVsHierarchy pHier, out uint pItemid)
        {
            throw new InvalidCastException();
        }
        public override int GetCfg(out IVsCfg ppCfg)
        {
            ppCfg = new TestIVsCfg(ConfigName);
            return VSConstants.S_OK;
        }
    }
}
