/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PropertyPageBaseTest;
using PropertyPageBase;
using Microsoft.VisualStudio.OLE.Interop;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.VisualStudio;
using PropertyPageBaseTest.Mocks;

namespace PropertyPageBaseTest
{
    /// <summary>
    /// Summary description for PropertyPageTest
    /// </summary>
    [TestClass]
    public class PropertyPageTest
    {
        [TestMethod]
        public void ConstructorTest()
        {
            TestPage samplePage = new TestPage();
            Assert.IsNotNull(samplePage);
        }

        [TestMethod]
        public void ImplementsIPropertyPage2()
        {
            Type samplePageType = typeof(TestPage);
            Assert.IsNotNull(samplePageType.GetInterface("IPropertyPage2"));
        }

        [TestMethod]
        public void IsComVisible()
        {
            Type samplePageType = typeof(TestPage);
            Assert.IsTrue(Marshal.IsTypeVisibleFromCom(samplePageType));
        }

        [TestMethod]
        public void HasGuid()
        {
            Type samplePageType = typeof(TestPage);
            object[] samplePageAttributes = samplePageType.GetCustomAttributes(typeof(GuidAttribute), false);
            Assert.AreEqual(1, samplePageAttributes.Length);
            GuidAttribute pageGuidAttribute = samplePageAttributes[0] as GuidAttribute;
            Assert.IsNotNull(pageGuidAttribute);
            string expectedPageGuid = "C986CB77-C1A1-42ed-8ECC-A96EFE9A1FBE";
            Assert.AreEqual(expectedPageGuid, pageGuidAttribute.Value);
        }

        [TestMethod]
        public void ActivateTest()
        {
            TestPage samplePage = new TestPage();
            Mocks.TestHostingForm hostingForm = new PropertyPageBaseTest.Mocks.TestHostingForm();
            using (hostingForm)
            {
                hostingForm.CreateControl();
                RECT hostingRectangle;
                hostingRectangle.left = 0;
                hostingRectangle.top = 0;
                hostingRectangle.right = 100;
                hostingRectangle.bottom = 100;
                samplePage.Activate(hostingForm.Handle, new RECT[] { hostingRectangle } , 0);
                TestIPageView testPageView = samplePage.MyPageView as TestIPageView;
                Assert.AreEqual(hostingForm, testPageView.MyParent);
                Assert.AreEqual(Rectangle.FromLTRB(0, 0, 100, 100), testPageView.MyLocation);
                Assert.AreEqual(false, testPageView.IsVisible);
            }
        }

        [TestMethod]
        public void MoveTest()
        {
            TestPage samplePage = new TestPage();
            TestIPropertyPageSite testSite = new TestIPropertyPageSite();

            samplePage.SetPageSite(testSite);
            Mocks.TestHostingForm hostingForm = new PropertyPageBaseTest.Mocks.TestHostingForm();
            using (hostingForm)
            {
                hostingForm.CreateControl();
                RECT hostingRectangle;
                hostingRectangle.left = 0;
                hostingRectangle.top = 0;
                hostingRectangle.right = 100;
                hostingRectangle.bottom = 100;
                samplePage.Activate(hostingForm.Handle, new RECT[] { hostingRectangle } , 0);
                hostingRectangle.left = 0;
                hostingRectangle.top = 0;
                hostingRectangle.right = 90;
                hostingRectangle.bottom = 90;
                samplePage.Move(new RECT[] { hostingRectangle });
                TestIPageView pageView = samplePage.MyPageView as TestIPageView;
                Assert.AreEqual(Rectangle.FromLTRB(0, 0, 90, 90), pageView.MyLocation);
            }
        }

        [TestMethod]
        public void ShowTest()
        {
            TestPage testPage = new TestPage();
            Mocks.TestHostingForm hostingForm = new PropertyPageBaseTest.Mocks.TestHostingForm();
            using (hostingForm)
            {
                hostingForm.CreateControl();
                RECT hostingRectangle;
                hostingRectangle.left = 0;
                hostingRectangle.top = 0;
                hostingRectangle.right = 100;
                hostingRectangle.bottom = 100;
                testPage.Activate(hostingForm.Handle, new RECT[] { hostingRectangle } , 0);
                TestIPageView pageView = testPage.MyPageView as TestIPageView;
                Assert.IsFalse(pageView.IsVisible);
                testPage.Show(Constants.SW_SHOW);
                Assert.IsTrue(pageView.IsVisible);
                testPage.Show(Constants.SW_HIDE);
                Assert.IsFalse(pageView.IsVisible);
                testPage.Show(Constants.SW_SHOWNORMAL);
                Assert.IsTrue(pageView.IsVisible);
                testPage.Show(7);
                Assert.IsTrue(pageView.IsVisible);
            }
        }

        [TestMethod]
        public void DeactivateTest()
        {
            TestPage testPage = new TestPage();
            TestIPropertyPageSite testSite = new TestIPropertyPageSite();
            testPage.SetPageSite(testSite);
            Mocks.TestHostingForm hostingForm = new PropertyPageBaseTest.Mocks.TestHostingForm();
            using (hostingForm)
            {
                hostingForm.CreateControl();
                RECT hostingRectangle;
                hostingRectangle.left = 0;
                hostingRectangle.top = 0;
                hostingRectangle.right = 100;
                hostingRectangle.bottom = 100;
                testPage.Activate(hostingForm.Handle, new RECT[] { hostingRectangle } , 0);
                TestIPageView pageView = testPage.MyPageView as TestIPageView;
                Assert.AreEqual(hostingForm, pageView.MyParent);
                testPage.Deactivate();
                Assert.IsNull(pageView.MyParent);
            }
        }

        [TestMethod]
        public void TranslateAcceleratorTest()
        {
            TestPage testPage = new TestPage();
            Mocks.TestHostingForm hostingForm = new PropertyPageBaseTest.Mocks.TestHostingForm();
            using (hostingForm)
            {
                hostingForm.CreateControl();
                RECT hostingRectangle;
                hostingRectangle.left = 0;
                hostingRectangle.top = 0;
                hostingRectangle.right = 100;
                hostingRectangle.bottom = 100;
                testPage.Activate(hostingForm.Handle, new RECT[] { hostingRectangle } , 0);
                TestIPageView pageView = testPage.MyPageView as TestIPageView;
                pageView.sampleChildHwnd = new IntPtr(10);
                pageView.outLParam = new IntPtr(11);
                pageView.outWParam = new IntPtr(12);

                MSG[] recognizedAcceleratorMessage = new MSG[] { new MSG() };
                recognizedAcceleratorMessage[0].hwnd = pageView.sampleChildHwnd;
                recognizedAcceleratorMessage[0].message = 1;
                recognizedAcceleratorMessage[0].wParam = new IntPtr(2);
                recognizedAcceleratorMessage[0].lParam = new IntPtr(3);

                int hr = testPage.TranslateAccelerator(recognizedAcceleratorMessage);
                Assert.AreEqual(VSConstants.S_OK, hr);
                Assert.AreEqual(pageView.outLParam, recognizedAcceleratorMessage[0].lParam);
                Assert.AreEqual(pageView.outWParam, recognizedAcceleratorMessage[0].wParam);

                MSG[] unrecognizedAcceleratorMessage = new MSG[] { new MSG() };
                unrecognizedAcceleratorMessage[0].hwnd = new IntPtr(100);
                unrecognizedAcceleratorMessage[0].message = 1;
                unrecognizedAcceleratorMessage[0].wParam = new IntPtr(2);
                unrecognizedAcceleratorMessage[0].lParam = new IntPtr(3);

                hr = testPage.TranslateAccelerator(unrecognizedAcceleratorMessage);
                Assert.AreEqual(VSConstants.S_FALSE, hr);
                Assert.AreEqual(new IntPtr(2), unrecognizedAcceleratorMessage[0].wParam);
                Assert.AreEqual(new IntPtr(3), unrecognizedAcceleratorMessage[0].lParam);
            }
        }

        [TestMethod]
        public void SetPageSiteTest()
        {
            TestPage testPage = new TestPage();
            Mocks.MockIPropertyPageSite mockSite = new PropertyPageBaseTest.Mocks.MockIPropertyPageSite();
            testPage.SetPageSite(mockSite);
            Assert.AreEqual(mockSite, testPage.IPropertyPageSite);
            Assert.IsNotNull(testPage.MyPageView);
        }

        [TestMethod]
        public void SetObjectsTest()
        {
            TestPage testPage = new TestPage();

            string dummyObject = "dummy";

            testPage.SetObjects(1, new object[] { dummyObject });
            TestIPropertyStore testPropertyStore = testPage.IPropertyStore as TestIPropertyStore;
            Assert.AreEqual(dummyObject, testPropertyStore.ObjectSet);
        }

        [TestMethod]
        public void GetValueForPropertyTest()
        {
            TestPage testPage = new TestPage();

            string dummyObject = "dummy";
            string propertyName = "Property1";
            string expectedValue = "Value1";

            testPage.SetObjects(1, new object[] { dummyObject });
            TestIPropertyStore testPropertyStore = testPage.IPropertyStore as TestIPropertyStore;
            testPropertyStore.Properties.Add(propertyName, expectedValue);

            string actualValue = testPage.GetValueForProperty(propertyName);
            Assert.AreEqual(expectedValue, actualValue);
        }

        [TestMethod]
        public void PropertyChangedTest()
        {
            TestPage testPage = new TestPage();

            string dummyObject = "dummy";
            string propertyName = "Property1";
            string expectedValue = "Value1";
            string newValue = "Value2";

            TestIPropertyPageSite site = new TestIPropertyPageSite();
            site.page = testPage;

            testPage.SetPageSite(site);
            testPage.SetObjects(1, new object[] { dummyObject });
            TestIPropertyStore testPropertyStore = testPage.IPropertyStore as TestIPropertyStore;
            testPropertyStore.Properties.Add(propertyName, expectedValue);

            testPage.PropertyChanged(propertyName, newValue);
            Assert.AreEqual((uint)(PROPPAGESTATUS.PROPPAGESTATUS_VALIDATE | PROPPAGESTATUS.PROPPAGESTATUS_DIRTY), site.StatusChangeFlags);
            Assert.AreEqual(newValue, testPropertyStore.Properties[propertyName]);
        }

        [TestMethod]
        public void ApplyWithNoUserChangeTest()
        {
            TestPage testPage = new TestPage();

            IPropertyPage page = testPage as IPropertyPage2;
            int actual = page.Apply();
            Assert.AreEqual(VSConstants.S_OK, actual);
        }

        [TestMethod]
        public void GetPropertyStoreTest()
        {
            TestPage testPage = new TestPage();
            testPage.GetRealStore = true;

            string propertyName = "Property1";
            string propertyValue = "Value1";

            TestIVsBrowseObject dataObject = new TestIVsBrowseObject();
            TestIVsHierarchy hierarchy = new TestIVsHierarchy();
            dataObject.Hierarchy = hierarchy;
            TestDTEProject dteProject = new TestDTEProject();
            hierarchy.dteProject = dteProject;
            TestDTEProperties dteProjectProperties = new TestDTEProperties();
            dteProject.ProjectProperties = dteProjectProperties;
            TestDTEProperty property1 = new TestDTEProperty();
            property1.PropertyValue = propertyValue;
            dteProjectProperties.Properties.Add(propertyName, property1);

            testPage.SetObjects(1, new object[] { dataObject });
            Assert.AreEqual(propertyValue, testPage.GetValueForProperty(propertyName));
        }

        [TestMethod]
        public void IsPageDirtyTest()
        {
            TestPage testPage = new TestPage();

            TestIPropertyPageSite site = new TestIPropertyPageSite();
            site.ImmediateApply = false;
            site.page = testPage;

            string propertyName = "PropertyName";
            string propertyValue = "PropertyValue";
            string newValue = "PropertyValueNew";
            string dummyObject = "DummyObject";

            testPage.SetPageSite(site);
            testPage.SetObjects(1, new object[] { dummyObject });
            TestIPropertyStore testPropertyStore = testPage.IPropertyStore as TestIPropertyStore;
            testPropertyStore.Properties.Add(propertyName, propertyValue);

            bool actual = testPage.IsPageDirty() == VSConstants.S_OK;
            Assert.AreEqual(false, actual);

            testPage.PropertyChanged(propertyName, newValue);
            actual = testPage.IsPageDirty() == VSConstants.S_OK;
            Assert.AreEqual(true, actual);

            testPage.Apply();
            actual = testPage.IsPageDirty() == VSConstants.S_OK;
            Assert.AreEqual(false, actual);
        }

        [TestMethod]
        public void GetPageInfoTest()
        {
            TestPage testPage = new TestPage();

            PROPPAGEINFO[] pageInfoList = new PROPPAGEINFO[1];
            testPage.GetPageInfo(pageInfoList);

            PROPPAGEINFO testPageInfo = pageInfoList[0];

            Assert.AreEqual((uint)Marshal.SizeOf(typeof(PROPPAGEINFO)), testPageInfo.cb);
            Assert.AreEqual((uint)0, testPageInfo.dwHelpContext);
            Assert.IsNull(testPageInfo.pszHelpFile);
            Assert.IsNull(testPageInfo.pszDocString);
            Assert.AreEqual(testPage.MyPageView.ViewSize, new Size(testPageInfo.SIZE.cx, testPageInfo.SIZE.cy));
            Assert.AreEqual(testPage.Title, testPageInfo.pszTitle);
        }

        [TestMethod]
        public void HelpTest()
        {
            TestPage testPage = new TestPage();
            TestIPropertyPageSite testSite = new TestIPropertyPageSite();
            TestHelp testHelp = new TestHelp();
            testSite.ServiceObjects.Add(typeof(Microsoft.VisualStudio.VSHelp.Help), testHelp);

            testPage.SetPageSite(testSite);
            testPage.Help("");
            Assert.AreEqual("TestPageHelp", testHelp.KeywordsDisplayed[0]);
        }

        [TestMethod]
        public void EditPropertyTest()
        {
            TestPage testPage = new TestPage();

            testPage.EditProperty(0);
            // all we have to do is make sure this doesn't throw
        }

        [TestMethod]
        public void NullSetObjectsTest()
        {
            TestPage testPage = new TestPage();

            string dummyObject = "dummy";

            testPage.SetObjects(1, new object[] { dummyObject });
            TestIPropertyStore testPropertyStore = testPage.IPropertyStore as TestIPropertyStore;
            Assert.AreEqual(dummyObject, testPropertyStore.ObjectSet);
            testPage.SetObjects(0, null);
            Assert.IsNull(testPropertyStore.ObjectSet);
        }

        [TestMethod]
        public void MultipleSetObjectsCallsTest()
        {
            TestPage testPage = new TestPage();

            string dummyObject = "dummy";
            string dummyObject2 = "dummy2";

            testPage.SetObjects(1, new object[] { dummyObject });
            TestIPropertyStore testPropertyStore = testPage.IPropertyStore as TestIPropertyStore;
            Assert.AreEqual(dummyObject, testPropertyStore.ObjectSet);
            testPage.SetObjects(1, new object[] { dummyObject2 });

            testPropertyStore = testPage.IPropertyStore as TestIPropertyStore;
            Assert.AreEqual(dummyObject2, testPropertyStore.ObjectSet);
            Assert.IsTrue((testPage.MyPageView as TestIPageView).PropertiesRefreshed);
        }

    }
}
