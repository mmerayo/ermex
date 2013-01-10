using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ermeX.Tests.Common.DataAccess;
using ermeX.Tests.Common.SettingsProviders;

namespace ermeX.Tests.Bus.Publishing.Dispatching.Messages
{

    class MessageCollectorTester : DataAccessTestBase
    {
        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void CanDispatchMessage()
        {
            throw new NotImplementedException();
        }

        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void RemovesExpiredItems()
        {
            throw new NotImplementedException();
        }

        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void SendsExistingItemsOnStart()
        {
            throw new NotImplementedException();
        }

    }
}
