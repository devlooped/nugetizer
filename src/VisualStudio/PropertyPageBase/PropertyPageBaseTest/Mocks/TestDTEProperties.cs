using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyPageBaseTest.Mocks
{
    class TestDTEProperties : AbstractDTEProperties
    {
        public Dictionary<string, TestDTEProperty> Properties = new Dictionary<string,TestDTEProperty>();
        public override EnvDTE.Property Item(object index)
        {
            string propertyName = index as string;
            return Properties[propertyName];
        }
    }
}
