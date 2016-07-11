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
using System.Windows.Forms;
using System.Drawing;
using Microsoft.VisualStudio;

namespace PropertyPageBaseTest
{
    public class TestIPageView : IPageView
    {
        public Control MyParent;
        public Rectangle MyLocation;
        public bool IsVisible = false;
        
        public IntPtr sampleChildHwnd;
        public IntPtr outWParam;
        public IntPtr outLParam;

        public bool PropertiesRefreshed = false;

        #region IPageView Members

        public void Initialize(System.Windows.Forms.Control parentControl, System.Drawing.Rectangle rectangle)
        {
            this.MyParent = parentControl;
            this.MyLocation = rectangle;
        }

        public void MoveView(System.Drawing.Rectangle rectangle)
        {
            this.MyLocation = rectangle;
        }

        public void ShowView()
        {
            IsVisible = true;
        }

        public void HideView()
        {
            IsVisible = false;
        }

        public void Dispose()
        {
            IsVisible = false;
            MyParent = null;
            MyLocation = Rectangle.FromLTRB(0, 0, 0, 0);
        }

        public int ProcessAccelerator(ref Message keyboardMessage)
        {
            if (keyboardMessage.HWnd == sampleChildHwnd)
            {
                keyboardMessage.LParam = outLParam;
                keyboardMessage.WParam = outWParam;
                return VSConstants.S_OK;
            }
            return VSConstants.S_FALSE;
        }

        #endregion

        #region IPageView Members


        public Size ViewSize
        {
            get { return new Size(100, 100); }
        }

        #endregion


        #region IPageView Members


        public void RefreshPropertyValues()
        {
            PropertiesRefreshed = true;
        }

        #endregion
    }
}
