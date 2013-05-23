using System;
using Moq;
using NUnit.Framework;
using ermeX.ComponentServices.RemoteComponent;
using ermeX.ComponentServices.RemoteComponent.Commands;

namespace ermeX.Tests.ComponentServices.RemoteComponent
{
	[TestFixture]
	internal class RemoteComponentStateMachineTester
	{
		private class TestContext
		{
			private readonly Mock<IOnCreatingStepExecutor> _onCreatingStepExecutor;
			private readonly Mock<IOnErrorStepExecutor> _onErrorStepExecutor;
			private readonly Mock<IOnJoiningStepExecutor> _onJoiningStepExecutor;
			private readonly Mock<IOnPreliveExecutor> _onPreliveExecutor;
			private readonly Mock<IOnSubscriptionsReceivedStepExecutor> _onReceivedSubscriptionsStepExecutor;
			private readonly Mock<IOnRequestingServicesStepExecutor> _onRequestingServicesStepExecutor;
			private readonly Mock<IOnRequestingSubscriptionsStepExecutor> _onRequestingSubscriptionsStepExecutor;
			private readonly Mock<IOnRunningStepExecutor> _onRunningStepExecutor;
			private readonly Mock<IOnServicesReceivedStepExecutor> _onServicesReceivedStepExecutor;
			private readonly Mock<IOnStoppedStepExecutor> _onStoppedStepExecutor;

			public TestContext()
			{
				_onPreliveExecutor = new Mock<IOnPreliveExecutor>();
				_onCreatingStepExecutor = new Mock<IOnCreatingStepExecutor>();
				_onStoppedStepExecutor = new Mock<IOnStoppedStepExecutor>();
				_onJoiningStepExecutor = new Mock<IOnJoiningStepExecutor>();
				_onRunningStepExecutor = new Mock<IOnRunningStepExecutor>();
				_onErrorStepExecutor = new Mock<IOnErrorStepExecutor>();
				_onRequestingSubscriptionsStepExecutor = new Mock<IOnRequestingSubscriptionsStepExecutor>();
				_onRequestingServicesStepExecutor = new Mock<IOnRequestingServicesStepExecutor>();
				_onReceivedSubscriptionsStepExecutor = new Mock<IOnSubscriptionsReceivedStepExecutor>();
				_onServicesReceivedStepExecutor = new Mock<IOnServicesReceivedStepExecutor>();
			}

			public IRemoteComponentStateMachine Sut
			{
				get
				{
					return new RemoteComponentStateMachine(_onPreliveExecutor.Object,
					                                       _onCreatingStepExecutor.Object,
					                                       _onStoppedStepExecutor.Object,
					                                       _onJoiningStepExecutor.Object,
					                                       _onRunningStepExecutor.Object,
					                                       _onErrorStepExecutor.Object,
					                                       _onRequestingSubscriptionsStepExecutor.Object,
					                                       _onReceivedSubscriptionsStepExecutor.Object,
					                                       _onRequestingServicesStepExecutor.Object,
					                                       _onServicesReceivedStepExecutor.Object
						);
				}
			}
		}

		[Test]
		public void CanCreate()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void CanGetReady()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void CanJoin()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void CanRun()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void CanStop()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void CanTransitToError_FromCreating()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void CanTransitToError_FromJoining()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void CanTransitToError_FromPrelive()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void CanTransitToError_FromRequestServices()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void CanTransitToError_FromRequestSubscriptions()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void CanTransitToError_FromRunning()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void CanTransitToError_FromServicesReceived()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void CanTransitToError_FromSubscriptionsReceived()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void InitsInPrelive()
		{
			var context = new TestContext();
			IRemoteComponentStateMachine target = context.Sut;

			Assert.IsFalse(target.Created());
		}

		[Test]
		public void WhenRunningCanReceiveServices()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void WhenRunningCanReceiveSubscriptions()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void WhenRunningRequestsServices()
		{
			throw new NotImplementedException();
		}


		[Test]
		public void WhenRunningRequestsSubscriptions()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void WhenUnavailableGoesToStopped()
		{
			throw new NotImplementedException();
		}
	}
}