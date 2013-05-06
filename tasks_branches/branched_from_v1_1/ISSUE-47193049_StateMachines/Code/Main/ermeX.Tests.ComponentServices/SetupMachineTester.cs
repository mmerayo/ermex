using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using ermeX.ComponentServices.ComponentSetup;
using ermeX.Exceptions;

namespace ermeX.Tests.ComponentServices
{
	[TestFixture]
	public class SetupMachineTester
	{
		[Test]
		public void CanSetup()
		{
			var context=new TestContext();
			context.WithNormalWorkflow();
			var target = new SetupMachine(context.Payloader);
			target.Setup();

			Assert.AreEqual(SetupMachine.SetupProcessState.Ready, target.State);

			context.Verify();
		}

		[Test]
		public void OnErrorInjecting_GoesToError()
		{
			var context = new TestContext();
			context.WithInjectingFailure();
			VerifyTransitionToError(context);
		}

		[Test]
		public void OnErrorUpgrading_GoesToError()
		{
			var context = new TestContext();
			context.WithUpgradingFailure();
			VerifyTransitionToError(context);
		}

		private static void VerifyTransitionToError(TestContext context)
		{
			var target = new SetupMachine(context.Payloader);
			Assert.Throws<ermeXSetupException>(target.Setup);

			Assert.AreEqual(SetupMachine.SetupProcessState.Error, target.State);

			context.Verify();
		}

		private class TestContext
		{
			public TestContext()
			{
				_payloader=new Mock<ISetupPayloader>();
			}

			public void WithNormalWorkflow()
			{
				_payloader.Setup(x=>x.InjectServices()).Verifiable();
				_payloader.Setup(x => x.RunUpgrades()).Verifiable();
			}

			public void WithInjectingFailure()
			{
				_payloader.Setup(x=>x.InjectServices()).Throws(new Exception("Test")).Verifiable();
			}

			public void WithUpgradingFailure()
			{
				_payloader.Setup(x => x.RunUpgrades()).Throws(new Exception("Test")).Verifiable();
			}

			private readonly Mock<ISetupPayloader> _payloader;
			public ISetupPayloader Payloader
			{
				get { return _payloader.Object; }
			}

			public void Verify()
			{
				_payloader.Verify();
			}
		}
	}
}
