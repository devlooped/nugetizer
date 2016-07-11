using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyPageBaseTest.Mocks
{
	public class TestIPageViewSite : AbstractIPageViewSite
    {
        public Dictionary<string, string> PropertyNameValueDictionary = new Dictionary<string, string>();
        public List<string> RequestedProperties = new List<string>();
        public Dictionary<string, string> ChangedProperties = new Dictionary<string, string>();

        public override string GetValueForProperty(string propertyName)
        {
            RequestedProperties.Add(propertyName);
            return PropertyNameValueDictionary[propertyName];    
        }

        public override void PropertyChanged(string propertyName, string propertyValue)
        {
            ChangedProperties.Add(propertyName, propertyValue);
        }
    }
}
