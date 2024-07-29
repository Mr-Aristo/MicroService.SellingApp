using System;
using System.Linq.Expressions;
using System.Security.Authentication.ExtendedProtection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EventBusUnitTest
{
    [TestClass]
    public class EventBusTest
    {
        private ServiceCollection _services;

        public EventBusTest()
        {
            this._services = new ServiceCollection();
        }
        [TestMethod]
        public void TestMethod1()
        {

        }
    }
}
