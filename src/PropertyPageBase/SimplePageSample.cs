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
using Microsoft.VisualStudio.OLE.Interop;
using System.Runtime.InteropServices;

namespace PropertyPageBase
{
	[ComVisible(true)]
	[Guid("C986CB77-C1A1-42ed-8ECC-A96EFE9A1FBE")]
	public class SimplePageSample : PropertyPage
	{

		protected override PropertyPageBase.IPageView GetNewPageView()
		{
			return new SimplePageView();
		}
	}
}
