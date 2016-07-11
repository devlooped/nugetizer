using System;
using System.Collections.Generic;
using System.Text;
using EnvDTE;

namespace PropertyPageBaseTest.Mocks
{
    abstract class AbstractDTEProperties : Properties
    {
        #region Properties Members

        virtual public object Application
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        virtual public int Count
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        virtual public DTE DTE
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        virtual public System.Collections.IEnumerator GetEnumerator()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public Property Item(object index)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public object Parent
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        #endregion
    }
}
