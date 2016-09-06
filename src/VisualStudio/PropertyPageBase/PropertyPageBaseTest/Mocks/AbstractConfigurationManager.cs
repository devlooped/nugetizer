using System;
using System.Collections.Generic;
using System.Text;
using EnvDTE;

namespace PropertyPageBaseTest.Mocks
{
    abstract class AbstractConfigurationManager : ConfigurationManager
    {
        #region ConfigurationManager Members

        virtual public Configuration ActiveConfiguration
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        virtual public Configurations AddConfigurationRow(string NewName, string ExistingName, bool Propagate)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public Configurations AddPlatform(string NewName, string ExistingName, bool Propagate)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public Configurations ConfigurationRow(string Name)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public object ConfigurationRowNames
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

        virtual public void DeleteConfigurationRow(string Name)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public void DeletePlatform(string Name)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public System.Collections.IEnumerator GetEnumerator()
        {
            throw new Exception("The method or operation is not implemented.");
        }
        virtual public Configuration Item(object index, string Platform)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public object Parent
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        virtual public Configurations Platform(string Name)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public object PlatformNames
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        virtual public object SupportedPlatforms
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        #endregion
    }
}
