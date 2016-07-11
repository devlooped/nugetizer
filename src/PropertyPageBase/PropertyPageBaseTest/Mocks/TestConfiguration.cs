using System;
using System.Collections.Generic;
using System.Text;
using EnvDTE;

namespace PropertyPageBaseTest.Mocks
{
    class TestConfiguration : AbstractConfiguration
    {
        public Properties ConfigProperties;
        public string ConfigPlatformName;

        public override EnvDTE.Properties Properties
        {
            get
            {
                return ConfigProperties;
            }
        }
        public override string PlatformName
        {
            get
            {
                return ConfigPlatformName;
            }
        }
    }
}
