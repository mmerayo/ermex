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
			context.VerifyStartWasCalled();
		}

		[Test]
		public void CanTransitToError_FromSubscribing()
		{
			var context = new TestContext()
				.WithExceptionOnSubscribe();

			var target = context.GetTarget();
			Assert.Throws<ermeXLocalComponentException>(target.Start);

			Assert.IsTrue(target.IsErrored());
			context.VerifyStartWasCalled();
			context.VerifySubscribeWasCalled();
		}
		[Test]
		public void CanTransitToError_FromPublishing()
		{
			var context = new TestContext()
				.WithExceptionOnPublishingServices();

			var target = context.GetTarget();
			Assert.Throws<ermeXLocalComponentException>(target.Start);

			Assert.IsTrue(target.IsErrored());
			context.VerifyStartWasCalled();
			context.VerifySubscribeWasCalled();
			context.VerifyPublishServicesWasCalled();
		}
		[Test]
		public void CanTransitToError_FromRunning()
		{
			var context = new TestContext()
				.WithExceptionOnRunning();

			var target = context.GetTarget();
			Assert.Throws<ermeXLocalComponentException>(target.Start);

			Assert.IsTrue(target.IsErrored());
			context.VerifyStartWasCalled();
			context.VerifySubscribeWasCalled();
			context.VerifyPublishServicesWasCalled();
			context.VerifyRunWasCalled();
		}
		[Test]
		public void CanTransitToError_FromStopping()
		{
			var context = new TestContext()
				.WithExceptionOnStopping();

			var target = context.GetTarget();
			target.Start();
			Assert.Throws<ermeXLocalComponentException>(target.Stop);

			Assert.IsTrue(target.IsErrored());
			context.VerifyStartWasCalled();
			context.VerifySubscribeWasCalled();
			context.VerifyPublishServicesWasCalled();
			context.VerifyRunWasCalled();
			context.VerifyStopWasCalled();
		}

		[Test]
		public void CanTransitToError_FromResetting()
		{
			var context = new TestContext()
				.WithExceptionOnResetting();

			var target = context.GetTarget();
			target.Start();
			Assert.Throws<ermeXLocalComponentException>(target.Stop);

			Assert.IsTrue(target.IsErrored());
			context.VerifyStartWasCalled();
			context.VerifySubscribeWasCalled();
			context.VerifyPublishServicesWasCalled();
			context.VerifyRunWasCalled();
			context.VerifyStopWasCalled();
			context.VerifyResetWasCalled();
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
				VerifyStartWasCalled();
				VerifySubscribeWasCalled();
				VerifyPublishServicesWasCalled();
				VerifyRunWasCalled();
			}

			public void VerifyRunWasCalled()
			{
				_payloader.Verify(x => x.Run(), Times.Exactly(1));
			}

			public void VerifyPublishServicesWasCalled()
			{
				_payloader.Verify(x => x.PublishServices(), Times.Exactly(1));
			}

			public void VerifySubscribeWasCalled()
			{
				_payloader.Verify(x => x.SubscribeToMessages(), Times.Exactly(1));
			}

			public void VerifyStartWasCalled()
			{
				_payloader.Verify(x => x.Start(), Times.Exactly(1));
			}


			public void VerifyFromRunningToStopped()
			{
				VerifyResetWasCalled();
			}

			public void VerifyResetWasCalled()
			{
				_payloader.Verify(x => x.Reset(), Times.Exactly(1));
			}

			public void VerifyStopWasCalled()
			{
				_payloader.Verify(x => x.Stop(), Times.Exactly(1));
			}

			
			public TestContext WithExceptionOnStart()
			{
				_payloader.Setup(x => x.Start()).Throws<Exception>();
				return this;
			}

			public TestContext WithExceptionOnSubscribe()
			{
				_payloader.Setup(x => x.SubscribeToMessages()).Throws<Exception>();
				return this;
			}

			public TestContext WithExceptionOnPublishingServices()
			{
				_payloader.Setup(x => x.PublishServices()).Throws<Exception>();
				return this;
			}

			public TestContext WithExceptionOnRunning()
			{
				_payloader.Setup(x => x.Run()).Throws<Exception>();
				return this;
			}


			public TestContext WithExceptionOnStopping()
			{
				_payloader.Setup(x => x.Stop()).Throws<Exception>();
				return this;
			}

			public TestContext WithExceptionOnResetting()
			{
				_payloader.Setup(x => x.Reset()).Throws<Exception>();
				return this;
			}

			
		}
	}
}
