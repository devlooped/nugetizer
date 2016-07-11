using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace PropertyPageBaseTest.Mocks
{
    class TestIVsHierarchy : AbstractIVsHierarchy
    {
        public TestDTEProject dteProject;

        public override int GetProperty(uint itemid, int propid, out object pvar)
        {
            if (itemid == VSConstants.VSITEMID_ROOT && propid == (int)__VSHPROPID.VSHPROPID_ExtObject)
            {
                pvar = dteProject;
                return VSConstants.S_OK;
            }
            throw new InvalidOperationException();
        }
    }
}
