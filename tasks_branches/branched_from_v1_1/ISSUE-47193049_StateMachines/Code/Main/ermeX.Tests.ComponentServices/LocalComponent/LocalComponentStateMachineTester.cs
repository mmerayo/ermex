﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using ermeX.ComponentServices.LocalComponent;
using ermeX.ComponentServices.LocalComponent.Commands;
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
			private readonly Mock<IStartablesStarterStepExecutor >_startablesStarter;
			private readonly Mock<ISubscribeToMessagesStepExecutor> _messagesSubscriber;
			private readonly Mock<IPublishServicesStepExecutor> _servicesPublisher;
			private readonly Mock<IResetStepExecutor> _resetExecutor;
			private readonly Mock<IStopComponentStepExecutor> _stopExecutor;
			private readonly Mock<IRunStepExecutor >_runExecutor;
			private readonly Mock<IErrorStepExecutor> _errorExecutor;


			public TestContext()
			{
				_startablesStarter=new Mock<IStartablesStarterStepExecutor>();
				_messagesSubscriber=new Mock<ISubscribeToMessagesStepExecutor>();
				_servicesPublisher=new Mock<IPublishServicesStepExecutor>();
				_resetExecutor=new Mock<IResetStepExecutor>();
				_stopExecutor= new Mock<IStopComponentStepExecutor>();
				_runExecutor=new Mock<IRunStepExecutor>();
				_errorExecutor=new Mock<IErrorStepExecutor>();
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
				_messagesSubscriber.Verify(x => x.Subscribe() ,Times.Exactly(1));
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
	}
}
