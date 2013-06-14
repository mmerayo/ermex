using System;
using Moq;
using NUnit.Framework;
using Ninject.Modules;
using ermeX.ComponentServices.Interfaces.LocalComponent.Commands;
using ermeX.ComponentServices.LocalComponent;
using ermeX.ComponentServices.LocalComponent.Commands;
using ermeX.ConfigurationManagement.IoC;
using ermeX.Exceptions;

namespace ermeX.Tests.ComponentServices.LocalComponent
{
	[TestFixture]
	internal class LocalComponentStateMachineTester
	{
		private class TestContext
		{
			private class TestInjectionsModule:NinjectModule
			{
				private readonly IOnStartStepExecutor _onStartStepExecutor;
				private readonly IOnSubscribeToMessagesStepExecutor _onSubscribeToMessagesStepExecutor;
				private readonly IOnPublishServicesStepExecutor _onPublishServicesStepExecutor;
				private readonly IOnResetStepExecutor _onResetStepExecutor;
				private readonly IOnStopStepExecutor _onStopStepExecutor;
				private readonly IOnRunStepExecutor _onRunStepExecutor;
				private readonly IOnErrorStepExecutor _onErrorStepExecutor;

				public TestInjectionsModule(IOnStartStepExecutor onStartStepExecutor, 
					IOnSubscribeToMessagesStepExecutor onSubscribeToMessagesStepExecutor, 
					IOnPublishServicesStepExecutor onPublishServicesStepExecutor, 
					IOnResetStepExecutor onResetStepExecutor, 
					IOnStopStepExecutor onStopStepExecutor,
					IOnRunStepExecutor onRunStepExecutor, 
					IOnErrorStepExecutor onErrorStepExecutor)
				{
					_onStartStepExecutor = onStartStepExecutor;
					_onSubscribeToMessagesStepExecutor = onSubscribeToMessagesStepExecutor;
					_onPublishServicesStepExecutor = onPublishServicesStepExecutor;
					_onResetStepExecutor = onResetStepExecutor;
					_onStopStepExecutor = onStopStepExecutor;
					_onRunStepExecutor = onRunStepExecutor;
					_onErrorStepExecutor = onErrorStepExecutor;
				}

				public override void Load()
				{
					Bind<IOnStartStepExecutor>().ToConstant(_onStartStepExecutor);

					Bind<IOnSubscribeToMessagesStepExecutor>().ToConstant(_onSubscribeToMessagesStepExecutor);
					Bind<IOnPublishServicesStepExecutor>().ToConstant(_onPublishServicesStepExecutor);
					Bind<IOnResetStepExecutor>().ToConstant(_onResetStepExecutor);
					Bind<IOnStopStepExecutor>().ToConstant(_onStopStepExecutor);
					Bind<IOnRunStepExecutor>().ToConstant(_onRunStepExecutor);
					Bind<IOnErrorStepExecutor>().ToConstant(_onErrorStepExecutor);
					
				}
			}

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

				IoCManager.SetCurrentInjections(new INinjectModule[]{ new TestInjectionsModule(_startablesStarter.Object,
													  _messagesSubscriber.Object,
													  _servicesPublisher.Object,
													  _resetExecutor.Object,
													  _stopExecutor.Object,
													  _runExecutor.Object,
													  _errorExecutor.Object)});
			}

			public LocalComponentStateMachine GetTarget()
			{
				return new LocalComponentStateMachine();
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