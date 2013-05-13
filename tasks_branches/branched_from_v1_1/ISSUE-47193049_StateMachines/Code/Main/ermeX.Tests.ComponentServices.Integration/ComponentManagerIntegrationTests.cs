using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ermeX.ComponentServices;
using ermeX.Configuration;
using ermeX.Tests.Common.Networking;

namespace ermeX.Tests.ComponentServices.Integration
{
	[TestFixture]
	public class ComponentManagerIntegrationTests
	{
		private static Configurer GetTestConfiguration()
		{
			var testPort = new TestPort(15235);

			return Configurer.Configure(Guid.NewGuid()).ListeningToTcpPort(testPort)
							 .RequestJoinTo("127.0.0.1", new TestPort(25552), Guid.NewGuid())
							 .SetInMemoryDb();
		}

		[Test]
		public void CanSetup()
		{
			Configurer settings = GetTestConfiguration();
			var target = ComponentManager.Default;
			target.Setup(settings);
			Assert.IsTrue(target.IsRunning());
		}

		[Test]
		public void CanSetUpAndReset()
		{
			Configurer settings = GetTestConfiguration();
			var target = ComponentManager.Default;
			target.Setup(settings);
			Assert.IsTrue(target.IsRunning());
			target.Reset();
			Assert.IsFalse(target.IsRunning());
		}

		[Test]
		public void CanResetAndSetUp()
		{
			Configurer settings = GetTestConfiguration();
			var target = ComponentManager.Default;
			target.Reset();
			Assert.IsFalse(target.IsRunning());
			target.Setup(settings);
			Assert.IsTrue(target.IsRunning());
		}

		[Test]
		public void CanSetupSeveralTimes()
		{
			Configurer settings = GetTestConfiguration();
			var target = ComponentManager.Default;
			for (int i = 0; i < 5; i++)
			{
				target.Setup(settings);
				Assert.IsTrue(target.IsRunning());
			}
		}

		[Test]
		public void CanSetupAndResetSeveralTimes()
		{
			Configurer settings = GetTestConfiguration();
			var target = ComponentManager.Default;
			for (int i = 0; i < 5; i++)
			{
				target.Setup(settings);
				Assert.IsTrue(target.IsRunning());
				target.Reset();
				Assert.IsFalse(target.IsRunning());
			}
		}
	}
}
