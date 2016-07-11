using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyPageBaseTest.Mocks
{
    class TestIPropertyStore : AbstractIPropertyStore
    {
        public object ObjectSet;

        public Dictionary<string, string> Properties = new Dictionary<string, string>();

        #region IPropertyStore Members

        override public void Dispose()
        {
            ObjectSet = null;
        }

        override public void Initialize(object dataObject)
        {
            ObjectSet = dataObject;
        }

        override public void Persist(string propertyName, string propertyValue)
        {
            Properties[propertyName] = propertyValue;
        }

        public override string PropertyValue(string propertyName)
        {
            return Properties[propertyName];
        }

        #endregion
    }
}
