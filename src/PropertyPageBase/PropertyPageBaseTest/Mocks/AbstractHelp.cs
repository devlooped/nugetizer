using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.VSHelp;

namespace PropertyPageBaseTest.Mocks
{
    abstract class AbstractHelp : Help
    {
        #region Help Members

        virtual public void CanShowFilterUI()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public void CanSyncContents(string bstrURL)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public void Close()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public string Collection
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        virtual public void Contents()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public void DisplayTopicFromF1Keyword(string pszKeyword)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public void DisplayTopicFromId(string bstrFile, uint Id)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public void DisplayTopicFromKeyword(string pszKeyword)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public void DisplayTopicFromURL(string pszURL)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public void DisplayTopicFromURLEx(string pszURL, IVsHelpTopicShowEvents pIVsHelpTopicShowEvents)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public void DisplayTopicFrom_OLD_Help(string bstrFile, uint Id)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public string Filter
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

        virtual public string FilterQuery
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        virtual public void FilterUI()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public string GetNextTopic(string bstrURL)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public object GetObject(string bstrMoniker, string bstrOptions)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public string GetPrevTopic(string bstrURL)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public object Help
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        virtual public IVsHelpOwner HelpOwner
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

        virtual public object HxSession
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        virtual public void Index()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public void IndexResults()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public void Search()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public void SearchResults()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public void SetCollection(string bstrCollection, string bstrFilter)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public void SyncContents(string bstrURL)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public void SyncIndex(string bstrKeyword, int fShow)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
