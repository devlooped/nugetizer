using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.Shell.Interop;

namespace PropertyPageBaseTest.Mocks
{
    abstract class AbstractIVsHierarchy : IVsHierarchy
    {
        #region IVsHierarchy Members

        virtual public int AdviseHierarchyEvents(IVsHierarchyEvents pEventSink, out uint pdwCookie)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public int Close()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public int GetCanonicalName(uint itemid, out string pbstrName)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public int GetGuidProperty(uint itemid, int propid, out Guid pguid)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public int GetNestedHierarchy(uint itemid, ref Guid iidHierarchyNested, out IntPtr ppHierarchyNested, out uint pitemidNested)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public int GetProperty(uint itemid, int propid, out object pvar)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public int GetSite(out Microsoft.VisualStudio.OLE.Interop.IServiceProvider ppSP)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public int ParseCanonicalName(string pszName, out uint pitemid)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public int QueryClose(out int pfCanClose)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public int SetGuidProperty(uint itemid, int propid, ref Guid rguid)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public int SetProperty(uint itemid, int propid, object var)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public int SetSite(Microsoft.VisualStudio.OLE.Interop.IServiceProvider psp)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public int UnadviseHierarchyEvents(uint dwCookie)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public int Unused0()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public int Unused1()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public int Unused2()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public int Unused3()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        virtual public int Unused4()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
