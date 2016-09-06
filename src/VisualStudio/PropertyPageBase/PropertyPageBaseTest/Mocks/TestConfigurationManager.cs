using System;
using System.Collections.Generic;
using System.Text;
using EnvDTE;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PropertyPageBaseTest.Mocks
{
    class TestConfigurationManager : AbstractConfigurationManager
    {
        public TestConfiguration ActiveConfig;
        public Dictionary<string, Configuration> SupportedConfigs = new Dictionary<string, Configuration>();
        public string SupportedPlatform;

        public override EnvDTE.Configuration ActiveConfiguration
        {
            get
            {
                return ActiveConfig;
            }
        }

        public override EnvDTE.Configuration Item(object index, string Platform)
        {
            if (Platform != SupportedPlatform)
            {
                Assert.Fail("Plaform passed to ConfigurationManager.Item doesn't equal expected platform");
            }
            return SupportedConfigs[index as string];
        }
    }
}
