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
			var target = context.GetTarget();
			target.Setup();

			Assert.IsTrue(target.IsReady());
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
			var target = context.GetTarget();
			Assert.Throws<ermeXSetupException>(target.Setup);

			Assert.IsTrue(target.SetupFailed());
			context.Verify();
		}

		private class TestContext
		{
			public TestContext()
			{
				_injector=new Mock<ISetupServiceInjector>();
				_upgrader=new Mock<ISetupVersionUpgradeRunner>();
			}

			public SetupMachine GetTarget()
			{
				return new SetupMachine(Injector,UpgradeRunner);
			}

			public void WithNormalWorkflow()
			{
				_injector.Setup(x=>x.InjectServices()).Verifiable();
				_upgrader.Setup(x => x.RunUpgrades()).Verifiable();
			}

			public void WithInjectingFailure()
			{
				_injector.Setup(x=>x.InjectServices()).Throws(new Exception("Test")).Verifiable();
			}

			public void WithUpgradingFailure()
			{
				_upgrader.Setup(x => x.RunUpgrades()).Throws(new Exception("Test")).Verifiable();
			}

			private readonly Mock<ISetupServiceInjector> _injector;
			public ISetupServiceInjector Injector
			{
				get { return _injector.Object; }
			}

			private readonly Mock<ISetupVersionUpgradeRunner> _upgrader;
			public ISetupVersionUpgradeRunner UpgradeRunner
			{
				get { return _upgrader.Object; }
			}

			public void Verify()
			{
				_injector.Verify();
				_upgrader.Verify();
			}
		}
	}
}
