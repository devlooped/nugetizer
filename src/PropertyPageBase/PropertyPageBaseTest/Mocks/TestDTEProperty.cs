using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyPageBaseTest.Mocks
{
    class TestDTEProperty : AbstractDTEProperty
    {
        public string PropertyValue;
        public override object Value
        {
            get
            {
                return PropertyValue;
            }
            set
            {
                PropertyValue = value as string;
            }
        }
    }
}
