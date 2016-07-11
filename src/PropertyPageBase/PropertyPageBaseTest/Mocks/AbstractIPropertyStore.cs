using System;
using System.Collections.Generic;
using System.Text;
using PropertyPageBase;

namespace PropertyPageBaseTest.Mocks
{
    abstract class AbstractIPropertyStore : IPropertyStore
    {

        #region IPropertyStore Members

        virtual public void Dispose()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public void Initialize(object dataObject)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public void Persist(string propertyName, string propertyValue)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public string PropertyValue(string propertyName)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        #region IPropertyStore Members


        public event StoreChangedDelegate StoreChanged;

        #endregion
    }
}
