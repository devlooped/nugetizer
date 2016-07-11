using System;
using System.Collections.Generic;
using System.Text;
using EnvDTE;

namespace PropertyPageBaseTest.Mocks
{
    abstract class AbstractDTEProperty : Property
    {
        #region Property Members

        virtual public object Application
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        virtual public Properties Collection
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        virtual public DTE DTE
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        virtual public string Name
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        virtual public short NumIndices
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        virtual public new object Object
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        virtual public Properties Parent
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        virtual public object Value
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        virtual public object get_IndexedValue(object Index1, object Index2, object Index3, object Index4)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public void let_Value(object lppvReturn)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public void set_IndexedValue(object Index1, object Index2, object Index3, object Index4, object Val)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
