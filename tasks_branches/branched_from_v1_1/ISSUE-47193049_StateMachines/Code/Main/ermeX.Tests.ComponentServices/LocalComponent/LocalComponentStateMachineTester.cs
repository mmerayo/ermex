using System;
using Moq;
using NUnit.Framework;
using ermeX.ComponentServices.LocalComponent;
using ermeX.ComponentServices.LocalComponent.Commands;
using ermeX.Exceptions;

namespace ermeX.Tests.ComponentServices.LocalComponent
{
	[TestFixture]
	internal class LocalComponentStateMachineTester
	{
		private class TestContext
		{
			private readonly Mock<IOnErrorStepExecutor> _errorExecutor;
			private readonly Mock<IOnSubscribeToMessagesStepExecutor> _messagesSubscriber;
			private readonly Mock<IOnResetStepExecutor> _resetExecutor;
			private readonly Mock<IOnRunStepExecutor> _runExecutor;
			private readonly Mock<IOnPublishServicesStepExecutor> _servicesPublisher;
			private readonly Mock<IOnStartStepExecutor> _startablesStarter;
			private readonly Mock<IOnStopStepExecutor> _stopExecutor;


			public TestContext()
			{
				_startablesStarter = new Mock<IOnStartStepExecutor>();
				_messagesSubscriber = new Mock<IOnSubscribeToMessagesStepExecutor>();
				_servicesPublisher = new Mock<IOnPublishServicesStepExecutor>();
				_resetExecutor = new Mock<IOnResetStepExecutor>();
				_stopExecutor = new Mock<IOnStopStepExecutor>();
				_runExecutor = new Mock<IOnRunStepExecutor>();
				_errorExecutor = new Mock<IOnErrorStepExecutor>();
			}

			public LocalComponentStateMachine GetTarget()
			{
				return new LocalComponentStateMachine(_startablesStarter.Object,
				                                      _messagesSubscriber.Object,
				                                      _servicesPublisher.Object,
				                                      _resetExecutor.Object,
				                                      _stopExecutor.Object,
				                                      _runExecutor.Object,
				                                      _errorExecutor.Object);
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
				_runExecutor.Verify(x => x.Run(), Times.Exactly(1));
			}

			public void VerifyPublishServicesWasCalled()
			{
				_servicesPublisher.Verify(x => x.Publish(), Times.Exactly(1));
			}

			public void VerifySubscribeWasCalled()
			{
				_messagesSubscriber.Verify(x => x.Subscribe(), Times.Exactly(1));
			}

			public void VerifyStartWasCalled()
			{
				_startablesStarter.Verify(x => x.DoStart(), Times.Exactly(1));
			}


			public void VerifyFromRunningToStopped()
			{
				VerifyResetWasCalled();
			}

			public void VerifyResetWasCalled()
			{
				_resetExecutor.Verify(x => x.Reset(), Times.Exactly(1));
			}

			public void VerifyStopWasCalled()
			{
				_stopExecutor.Verify(x => x.Stop(), Times.Exactly(1));
			}


			public TestContext WithExceptionOnStart()
			{
				_startablesStarter.Setup(x => x.DoStart()).Throws<Exception>();
				return this;
			}

			public TestContext WithExceptionOnSubscribe()
			{
				_messagesSubscriber.Setup(x => x.Subscribe()).Throws<Exception>();
				return this;
			}

			public TestContext WithExceptionOnPublishingServices()
			{
				_servicesPublisher.Setup(x => x.Publish()).Throws<Exception>();
				return this;
			}

			public TestContext WithExceptionOnRunning()
			{
				_runExecutor.Setup(x => x.Run()).Throws<Exception>();
				return this;
			}


			public TestContext WithExceptionOnStopping()
			{
				_stopExecutor.Setup(x => x.Stop()).Throws<Exception>();
				return this;
			}

			public TestContext WithExceptionOnResetting()
			{
				_resetExecutor.Setup(x => x.Reset()).Throws<Exception>();
				return this;
			}
		}

		[Test]
		public void CanRun()
		{
			var context = new TestContext();
			LocalComponentStateMachine target = context.GetTarget();
			target.Start();
			Assert.IsTrue(target.IsRunning());
			context.VerifyFromStoppedToRunning();
		}

		[Test]
		public void CanStop()
		{
			var context = new TestContext();
			LocalComponentStateMachine target = context.GetTarget();
			target.Start();

			target.Stop();

			Assert.IsTrue(target.IsStopped());
			context.VerifyFromRunningToStopped();
		}

		[Test]
		public void CanTransitToError_FromPublishing()
		{
			TestContext context = new TestContext()
				.WithExceptionOnPublishingServices();

			LocalComponentStateMachine target = context.GetTarget();
			Assert.Throws<ermeXLocalComponentException>(target.Start);

			Assert.IsTrue(target.IsErrored());
			context.VerifyStartWasCalled();
			context.VerifySubscribeWasCalled();
			context.VerifyPublishServicesWasCalled();
		}

		[Test]
		public void CanTransitToError_FromResetting()
		{
			TestContext context = new TestContext()
				.WithExceptionOnResetting();

			LocalComponentStateMachine target = context.GetTarget();
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

		[Test]
		public void CanTransitToError_FromRunning()
		{
			TestContext context = new TestContext()
				.WithExceptionOnRunning();

			LocalComponentStateMachine target = context.GetTarget();
			Assert.Throws<ermeXLocalComponentException>(target.Start);

			Assert.IsTrue(target.IsErrored());
			context.VerifyStartWasCalled();
			context.VerifySubscribeWasCalled();
			context.VerifyPublishServicesWasCalled();
			context.VerifyRunWasCalled();
		}

		[Test]
		public void CanTransitToError_FromStarting()
		{
			TestContext context = new TestContext()
				.WithExceptionOnStart();

			LocalComponentStateMachine target = context.GetTarget();
			Assert.Throws<ermeXLocalComponentException>(target.Start);

			Assert.IsTrue(target.IsErrored());
			context.VerifyStartWasCalled();
		}

		[Test]
		public void CanTransitToError_FromStopping()
		{
			TestContext context = new TestContext()
				.WithExceptionOnStopping();

			LocalComponentStateMachine target = context.GetTarget();
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
		public void CanTransitToError_FromSubscribing()
		{
			TestContext context = new TestContext()
				.WithExceptionOnSubscribe();

			LocalComponentStateMachine target = context.GetTarget();
			Assert.Throws<ermeXLocalComponentException>(target.Start);

			Assert.IsTrue(target.IsErrored());
			context.VerifyStartWasCalled();
			context.VerifySubscribeWasCalled();
		}

		[Test]
		public void IsStoppedWhenInitialized()
		{
			var context = new TestContext();
			LocalComponentStateMachine target = context.GetTarget();
			Assert.IsTrue(target.IsStopped());
		}
	}
}