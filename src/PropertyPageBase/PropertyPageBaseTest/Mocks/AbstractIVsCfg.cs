using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.Shell.Interop;

namespace PropertyPageBaseTest.Mocks
{
    abstract class AbstractIVsCfg : IVsCfg
    {
        #region IVsCfg Members

        virtual public int get_DisplayName(out string pbstrDisplayName)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public int get_IsDebugOnly(out int pfIsDebugOnly)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public int get_IsReleaseOnly(out int pfIsReleaseOnly)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
