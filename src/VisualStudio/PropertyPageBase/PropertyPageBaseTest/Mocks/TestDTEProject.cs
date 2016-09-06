using System;
using System.Collections.Generic;
using System.Text;
using EnvDTE;

namespace PropertyPageBaseTest.Mocks
{
    class TestDTEProject : AbstractDTEProject
    {
        public TestDTEProperties ProjectProperties;
        public TestConfigurationManager ConfigManager;

        public override EnvDTE.Properties Properties
        {
            get
            {
                return ProjectProperties;
            }
        }

        public override ConfigurationManager ConfigurationManager
        {
            get
            {
                return ConfigManager;
            }
        }
    }
}
