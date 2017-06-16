using Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
        }
    }

    [TestClass]
    public class PersonTest
    {
        [TestMethod]
        public void HasAllProperty()
        {
            var obj = new Person();
            Assert.AreEqual(typeof(int), obj.Id.GetType());
        }
    }
}
