using System;
using System.Collections.Generic;
using System.Text;
using EnvDTE;

namespace PropertyPageBaseTest.Mocks
{
    abstract class AbstractConfiguration : Configuration
    {
        #region Configuration Members

        virtual public ConfigurationManager Collection
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        virtual public string ConfigurationName
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        virtual public DTE DTE
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        virtual public string ExtenderCATID
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        virtual public object ExtenderNames
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        virtual public bool IsBuildable
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        virtual public bool IsDeployable
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        virtual public bool IsRunable
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        virtual public new object Object
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        virtual public OutputGroups OutputGroups
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        virtual public object Owner
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        virtual public string PlatformName
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        virtual public Properties Properties
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        virtual public vsConfigurationType Type
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
