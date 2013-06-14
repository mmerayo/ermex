using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using ermeX.ComponentServices.ComponentSetup;
using ermeX.ComponentServices.Interfaces.ComponentSetup;
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
			context.Verify(1);
		}

		[Test]
		public void CanSetupSeveralTimes()
		{
			var context = new TestContext();
			context.WithNormalWorkflow();
			var target = context.GetTarget();
			for (int i = 0; i < 4; i++)
			{
				//if(i>0)
				//    target.Reset();
				//else
					target.Setup();
			}
			Assert.IsTrue(target.IsReady());
			context.Verify(4);
		}

		[Test]
		public void OnErrorInjecting_GoesToError()
		{
			var context = new TestContext();
			context.WithInjectingFailure();
			VerifyTransitionToError(context);
			context.VerifyInjectServices(1);
		}

		[Test]
		public void OnErrorUpgrading_GoesToError()
		{
			var context = new TestContext();
			context.WithUpgradingFailure();
			VerifyTransitionToError(context);
			context.VerifyInjectServices(1);
			context.VerifyUpgrader(1);
		}

		private static void VerifyTransitionToError(TestContext context)
		{
			var target = context.GetTarget();
			Assert.Throws<ermeXSetupException>(target.Setup);
			
			Assert.IsTrue(target.SetupFailed());
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

			public void Verify(int times=1)
			{
				VerifyInjectServices(times);
				VerifyUpgrader(times);
			}

			public void VerifyUpgrader(int times)
			{
				_upgrader.Verify(x => x.RunUpgrades(), Times.Exactly(times));
			}

			public void VerifyInjectServices(int times)
			{
				_injector.Verify(x => x.InjectServices(), Times.Exactly(times));
			}
		}
	}
}
