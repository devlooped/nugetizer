using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.OLE.Interop;
using System.ComponentModel.Design;

namespace PropertyPageBaseTest.Mocks
{
    class TestIPropertyPageSite : AbstractIPropertyPageSite
    {
        public uint StatusChangeFlags;
        public bool ImmediateApply = true;

        public IPropertyPage2 page;

        public override void OnStatusChange(uint dwFlags)
        {
            StatusChangeFlags = dwFlags;
            if(ImmediateApply)
                page.Apply();
        }


        public Dictionary<Type, object> ServiceObjects = new Dictionary<Type, object>();
        public override object GetService(Type serviceType)
        {
            if (serviceType == typeof(Microsoft.VisualStudio.VSHelp.Help))
            {
                if (!ServiceObjects.ContainsKey(serviceType))
                    ServiceObjects.Add(serviceType, new TestHelp());
                return ServiceObjects[serviceType];
            }
            return null;
        }
    }
}
