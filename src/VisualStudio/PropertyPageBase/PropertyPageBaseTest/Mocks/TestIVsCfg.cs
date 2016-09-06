using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio;

namespace PropertyPageBaseTest.Mocks
{
    class TestIVsCfg : AbstractIVsCfg
    {
        private string m_configName;
        public TestIVsCfg(string configName)
        {
            m_configName = configName;
        }

        public override int get_DisplayName(out string pbstrDisplayName)
        {
            pbstrDisplayName = m_configName;
            return VSConstants.S_OK;
        }
    }
}
