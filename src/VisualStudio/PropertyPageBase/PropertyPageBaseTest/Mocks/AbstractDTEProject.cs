using System;
using System.Collections.Generic;
using System.Text;
using EnvDTE;

namespace PropertyPageBaseTest.Mocks
{
    abstract class AbstractDTEProject : Project
    {
        #region Project Members

        virtual public CodeModel CodeModel
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        virtual public Projects Collection
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        virtual public ConfigurationManager ConfigurationManager
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        virtual public DTE DTE
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        virtual public void Delete()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public string ExtenderCATID
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        virtual public object ExtenderNames
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        virtual public string FileName
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        virtual public string FullName
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        virtual public Globals Globals
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        virtual public bool IsDirty
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

        virtual public string Kind
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        virtual public string Name
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

        virtual public new object Object
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        virtual public ProjectItem ParentProjectItem
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        virtual public ProjectItems ProjectItems
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        virtual public Properties Properties
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        virtual public void Save(string FileName)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public void SaveAs(string NewFileName)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public bool Saved
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

        virtual public string UniqueName
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        virtual public object get_Extender(string ExtenderName)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
