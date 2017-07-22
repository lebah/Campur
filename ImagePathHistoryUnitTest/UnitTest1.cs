using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ImagePathHistory;
using System.Collections.Generic;

namespace ImagePathHistoryUnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
        }

        [TestMethod]
        public void SaveToXMLFileTest()
        {
            List<ImagePathHistoryClass> listIph = new List<ImagePathHistoryClass>();
            listIph.Add(new ImagePathHistoryClass() { ImagePath = "D:\tempfile1.png" });
            listIph.Add(new ImagePathHistoryClass() { ImagePath = "D:\tempfile2.png" });

            XmlHelper.ToXmlFile(listIph, @"D:\Temp\ImagePathHistory.xml");

            bool actual = XmlHelper.ValidatePath(@"D:\Temp\");
            bool expected = true;
            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void OpenToXMLFileTest()
        {
            List<ImagePathHistoryClass> listIph = XmlHelper.FromXmlFile<List<ImagePathHistoryClass>>(@"D:\Temp\ImagePathHistory.xml");

            int actual = listIph.Count;
            int expected = 2;
            Assert.AreEqual(expected, actual);

        }
    }


}
