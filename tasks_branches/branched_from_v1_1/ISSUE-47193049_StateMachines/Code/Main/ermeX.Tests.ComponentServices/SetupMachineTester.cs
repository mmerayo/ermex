using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using ermeX.ComponentServices.ComponentSetup;

namespace ermeX.Tests.ComponentServices
{
	//this is tests the setupmachine integrated with the payloader
	[TestFixture]
	public class SetupMachineTester
	{

		[Test]
		public void CanSetup()
		{
			var context=new TestContext();
			var target = new SetupMachine(context.Payloader);
			target.Setup();

			Assert.AreEqual(SetupMachine.SetupProcessState.Ready, target.State);
		}

		[Test]
		public void OnErrorThrows()
		{
			throw new NotImplementedException("Do for all states");
		}

		private class TestContext
		{
			public TestContext()
			{
				Payloader=new SetupPayloader();
			}

			public ISetupPayloader Payloader { get; private set; }
		}
	}
}
