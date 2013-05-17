using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using ermeX.ComponentServices.LocalComponent;
using ermeX.Exceptions;

namespace ermeX.Tests.ComponentServices.LocalComponent
{
	[TestFixture]
	class LocalComponentStateMachineTester
	{
		[Test]
		public void IsStoppedWhenInitialized()
		{
			var context = new TestContext();
			var target = context.GetTarget();
			Assert.IsTrue(target.IsStopped());
		}

		[Test]
		public void CanRun()
		{
			var context = new TestContext();
			var target = context.GetTarget();
			target.Start();
			Assert.IsTrue(target.IsRunning());
			context.VerifyFromStoppedToRunning();
		}

		[Test]
		public void CanStop()
		{
			var context = new TestContext();
			var target = context.GetTarget();
			target.Start();

			target.Stop();

			Assert.IsTrue(target.IsStopped());
			context.VerifyFromRunningToStopped();
		}

		[Test]
		public void CanTransitToError_FromStarting()
		{
			var context = new TestContext()
				.WithExceptionOnStart();

			var target = context.GetTarget();
			Assert.Throws<ermeXLocalComponentException>(target.Start);

			Assert.IsTrue(target.IsErrored());
		}

		[Test]
		public void CanRestartFromErrored()
		{
			throw new NotImplementedException();
		}]

		[Test]
		public void CanTransitToError_FromSubscribing()
		{
			throw new NotImplementedException();
		}
		[Test]
		public void CanTransitToError_FromPublishing()
		{
			throw new NotImplementedException();
		}
		[Test]
		public void CanTransitToError_FromRunning()
		{
			throw new NotImplementedException();
		}
		[Test]
		public void CanTransitToError_FromStopping()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void CanTransitToError_FromResetting()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void CanTransitTo_ResetErrorResetStopped()
		{
			throw new NotImplementedException();
		}

		private class TestContext
		{
			private readonly Mock<ILocalStateMachinePayloader> _payloader;

			public TestContext()
			{
				_payloader = new Mock<ILocalStateMachinePayloader>();
			}

			public LocalComponentStateMachine GetTarget()
			{
				return new LocalComponentStateMachine(Payloader);
			}

			public ILocalStateMachinePayloader Payloader
			{
				get { return _payloader.Object; }
			}

			public void VerifyFromStoppedToRunning()
			{
				_payloader.Verify(x=>x.Start(),Times.Exactly(1));
				_payloader.Verify(x => x.SubscribeToMessages(), Times.Exactly(1));
				_payloader.Verify(x => x.PublishServices(), Times.Exactly(1));
				_payloader.Verify(x => x.Run(), Times.Exactly(1));
			}


			public void VerifyFromRunningToStopped()
			{
				VerifyResetWasCalled();
			}

			private void VerifyResetWasCalled()
			{
				_payloader.Verify(x => x.Reset(), Times.Exactly(1));
			}

			public TestContext WithExceptionOnStart()
			{
				_payloader.Setup(x => x.Start()).Throws<Exception>();
				return this;
			}

		}
	}
}
