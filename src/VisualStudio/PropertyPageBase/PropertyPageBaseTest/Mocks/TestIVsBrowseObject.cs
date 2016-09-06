using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio;

namespace PropertyPageBaseTest.Mocks
{
    class TestIVsBrowseObject : AbstractIVsBrowseObject
    {
        public TestIVsHierarchy Hierarchy;

        public override int GetProjectItem(out Microsoft.VisualStudio.Shell.Interop.IVsHierarchy pHier, out uint pItemid)
        {
            pHier = Hierarchy;
            pItemid = 0;
            return VSConstants.S_OK;
        }
    }
}
