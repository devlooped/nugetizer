/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using PropertyPageBase;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.OLE.Interop;

namespace PropertyPageBaseTest
{
    [Guid("C986CB77-C1A1-42ed-8ECC-A96EFE9A1FBE")]
    [ComVisible(true)]
    public class TestPage : PropertyPage
    {
        public bool GetRealStore = false;
        protected override IPageView GetNewPageView()
        {
            return new TestIPageView();
        }

        protected override IPropertyStore GetNewPropertyStore()
        {
            if (GetRealStore)
                return base.GetNewPropertyStore();
            else
                return new Mocks.TestIPropertyStore();
        }

        public override string Title
        {
            get { return "TestPage"; }
        }

        protected override string HelpKeyword
        {
            get { return "TestPageHelp"; }
        }
    }
}
