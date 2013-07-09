using System;
using System.Net;
using Moq;
using NUnit.Framework;
using Ninject.Modules;
using ermeX.ComponentServices.Interfaces.RemoteComponent;
using ermeX.ComponentServices.Interfaces.RemoteComponent.Commands;
using ermeX.ComponentServices.RemoteComponent;
using ermeX.ComponentServices.RemoteComponent.Commands;
using ermeX.ConfigurationManagement.IoC;
using ermeX.Exceptions;
using ermeX.Tests.Common.RandomValues;

namespace ermeX.Tests.ComponentServices.RemoteComponent
{
	[TestFixture]
	internal class RemoteComponentStateMachineTester
	{
		private class TestContext
		{
			private class TestInjectionsModule : NinjectModule
			{
				private readonly IOnCreatingStepExecutor _onCreatingStepExecutor;
				private readonly IOnErrorStepExecutor _onErrorStepExecutor;
				private readonly IOnJoiningStepExecutor _onJoiningStepExecutor;
				private readonly IOnPreliveExecutor _onPreliveExecutor;
				private readonly IOnSubscriptionsReceivedStepExecutor _onReceivedSubscriptionsStepExecutor;
				private readonly IOnRequestingServicesStepExecutor _onRequestingServicesStepExecutor;
				private readonly IOnRequestingSubscriptionsStepExecutor _onRequestingSubscriptionsStepExecutor;
				private readonly IOnRunningStepExecutor _onRunningStepExecutor;
				private readonly IOnServicesReceivedStepExecutor _onServicesReceivedStepExecutor;
				private readonly IOnStoppedStepExecutor _onStoppedStepExecutor;


				public TestInjectionsModule(IOnCreatingStepExecutor onCreatingStepExecutor,
				                            IOnErrorStepExecutor onErrorStepExecutor,
				                            IOnJoiningStepExecutor onJoiningStepExecutor,
				                            IOnPreliveExecutor onPreliveExecutor,
				                            IOnSubscriptionsReceivedStepExecutor onReceivedSubscriptionsStepExecutor,
				                            IOnRequestingServicesStepExecutor onRequestingServicesStepExecutor,
				                            IOnRequestingSubscriptionsStepExecutor onRequestingSubscriptionsStepExecutor,
				                            IOnRunningStepExecutor onRunningStepExecutor,
				                            IOnServicesReceivedStepExecutor onServicesReceivedStepExecutor,
				                            IOnStoppedStepExecutor onStoppedStepExecutor)
				{
					_onCreatingStepExecutor = onCreatingStepExecutor;
					_onErrorStepExecutor = onErrorStepExecutor;
					_onJoiningStepExecutor = onJoiningStepExecutor;
					_onPreliveExecutor = onPreliveExecutor;
					_onReceivedSubscriptionsStepExecutor = onReceivedSubscriptionsStepExecutor;
					_onRequestingServicesStepExecutor = onRequestingServicesStepExecutor;
					_onRequestingSubscriptionsStepExecutor = onRequestingSubscriptionsStepExecutor;
					_onRunningStepExecutor = onRunningStepExecutor;
					_onServicesReceivedStepExecutor = onServicesReceivedStepExecutor;
					_onStoppedStepExecutor = onStoppedStepExecutor;
				}

				public override void Load()
				{
					Bind<IOnCreatingStepExecutor>().ToConstant(_onCreatingStepExecutor);
					Bind<IOnErrorStepExecutor>().ToConstant(_onErrorStepExecutor);
					Bind<IOnJoiningStepExecutor>().ToConstant(_onJoiningStepExecutor);
					Bind<IOnPreliveExecutor>().ToConstant(_onPreliveExecutor);
					Bind<IOnSubscriptionsReceivedStepExecutor>().ToConstant(_onReceivedSubscriptionsStepExecutor);
					Bind<IOnRequestingServicesStepExecutor>().ToConstant(_onRequestingServicesStepExecutor);
					Bind<IOnRequestingSubscriptionsStepExecutor>().ToConstant(_onRequestingSubscriptionsStepExecutor);
					Bind<IOnRunningStepExecutor>().ToConstant(_onRunningStepExecutor);
					Bind<IOnServicesReceivedStepExecutor>().ToConstant(_onServicesReceivedStepExecutor);
					Bind<IOnStoppedStepExecutor>().ToConstant(_onStoppedStepExecutor);
				}
			}


			//do the injections as in the local component tester
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

				//TODO: AUTOFIXTURE FOR THIS
				ComponentId = Guid.NewGuid();
				this.IPAddress=IPAddress.Parse("123.123.123.123");
				this.Port = RandomHelper.GetRandomUShort();

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

				DataContext=new RemoteComponentStateMachineContext();

				IoCManager.SetCurrentInjections(new INinjectModule[]
					{
						new TestInjectionsModule(_onCreatingStepExecutor.Object,
						                         _onErrorStepExecutor.Object,
						                         _onJoiningStepExecutor.Object,
						                         _onPreliveExecutor.Object, _onReceivedSubscriptionsStepExecutor.Object,
						                         _onRequestingServicesStepExecutor.Object, _onRequestingSubscriptionsStepExecutor.Object,
						                         _onRunningStepExecutor.Object, _onServicesReceivedStepExecutor.Object,
						                         _onStoppedStepExecutor.Object)
					});
			}

			public IRemoteComponentStateMachineContext DataContext { get; private set; }

			public IRemoteComponentStateMachine Sut
			{
				get
				{
					return new RemoteComponentStateMachine(DataContext);
				}
			}

			public Guid ComponentId { get; private set; }

			public IPAddress IPAddress { get; private set; }

			public ushort Port { get; private set; }

			public void VerifyCreatedHandlerWasCalled(int times=1)
			{
				_onCreatingStepExecutor.Verify(x=>x.Create(DataContext),Times.Exactly(times));
			}

			public void VerifyPreliveHandlerWasCalled(int times=1)
			{
				_onPreliveExecutor.Verify(x=>x.OnPrelive(),Times.Exactly(times));
			}

			public void VerifyJoiningHandlerWasCalled(int times=1)
			{
				_onJoiningStepExecutor.Verify(x => x.Join(DataContext), Times.Exactly(times));
			}

			public void VerifyStoppedHandlerWasCalled(int times=1)
			{
				_onStoppedStepExecutor.Verify(x=>x.Stop(),Times.Exactly(times));
			}

			public void VerifyRunningHandlerWasCalled(int times=1)
			{
				_onRunningStepExecutor.Verify(x => x.OnRunning(DataContext), Times.Exactly(times));
			}

			public void VerifyRunningSubstatesHandlersWereCalled(int times=1)
			{
				VerifyRequestingServicesHandlerWasCalled(times);
				VerifyServicesReceptionExecutor(times);
				VerifyRequestingSubscriptionsHandlerWasCalled(times);
				VerifySubscriptionsReceptionExecutor(times);
			}

			public void VerifySubscriptionsReceptionExecutor(int times=1)
			{
				_onReceivedSubscriptionsStepExecutor.Verify(x => x.SubscriptionsReceived(TODO), Times.Exactly(times));
			}

			public void VerifyServicesReceptionExecutor(int times=1)
			{
				_onServicesReceivedStepExecutor.Verify(x => x.ServicesReceived(DataContext), Times.Exactly(times));
			}

			public void VerifyRequestingSubscriptionsHandlerWasCalled(int times=1)
			{
				_onRequestingSubscriptionsStepExecutor.Verify(x => x.Request(DataContext), Times.Exactly(times));
			}

			public void VerifyRequestingServicesHandlerWasCalled(int times = 1)
			{
				_onRequestingServicesStepExecutor.Verify(x => x.Request(DataContext), Times.Exactly(times));
			}

			public void VerifyErrorHandlerWasCalled(int times = 1)
			{
				_onErrorStepExecutor.Verify(x=>x.OnError(),Times.Exactly(times));
			}

			public TestContext WithExceptionOnCreating()
			{
				_onCreatingStepExecutor.Setup(x => x.Create(DataContext)).Throws<Exception>();
				return this;
			}


			public TestContext WithExceptionOnJoining()
			{
				_onJoiningStepExecutor.Setup(x => x.Join(DataContext)).Throws<Exception>();
				return this;
			}

			public TestContext WithExceptionOnRequestingServices()
			{
				_onRequestingServicesStepExecutor.Setup(x => x.Request(DataContext)).Throws<Exception>();
				return this;
			}


			public TestContext WithExceptionOnRequestingSubscriptions()
			{
				_onRequestingSubscriptionsStepExecutor.Setup(x => x.Request(DataContext)).Throws<Exception>();
				return this;
			}

			public TestContext WithExceptionOnSubscriptionsReception()
			{
				_onReceivedSubscriptionsStepExecutor.Setup(x => x.SubscriptionsReceived(DataContext)).Throws<Exception>();
				return this;
			}
			public TestContext WithExceptionOnServicesReception()
			{
				_onServicesReceivedStepExecutor.Setup(x => x.ServicesReceived(DataContext)).Throws<Exception>();
				return this;
			}

			public TestContext WithExceptionOnRunning()
			{
				_onRunningStepExecutor.Setup(x => x.OnRunning(DataContext)).Throws<Exception>();
				return this;
			}

			public TestContext WithExceptionOnStopped()
			{
				_onStoppedStepExecutor.Setup(x => x.Stop()).Throws<Exception>();
				return this;
			}
		}

		TestContext _context;

		[SetUp]
		public void OnSetup()
		{
			_context = new TestContext();
		}

		[Test]
		public void CanCreate()
		{
			var target = _context.Sut;
			target.Create(_context.ComponentId,_context.IPAddress,_context.Port);

			Assert.IsTrue(target.WasCreated());
			Assert.IsTrue(target.IsStopped());
			_context.VerifyCreatedHandlerWasCalled();
			_context.VerifyStoppedHandlerWasCalled();
			
		}

		[Test]
		public void CanJoin()
		{
			var target = _context.Sut;
			target.Create(_context.ComponentId,_context.IPAddress,_context.Port);
			target.Join();

			Assert.IsTrue(target.WasCreated());
			Assert.IsTrue(target.IsJoining());
			Assert.IsFalse(target.IsStopped());
			_context.VerifyJoiningHandlerWasCalled();
		}

		[Test]
		public void CanRun()
		{
			var target = _context.Sut;
			target.Create(_context.ComponentId,_context.IPAddress,_context.Port);
			target.Join();

			target.Joined(TODO);
			target.ServicesReceived();
			target.SubscriptionsReceived();
			
			Assert.IsFalse(target.IsJoining());
			Assert.IsTrue(target.IsRunning());
			
			_context.VerifyRunningHandlerWasCalled();
			_context.VerifyRunningSubstatesHandlersWereCalled();
		}

		[Test]
		public void WhenRunningCanReceiveServices()
		{
			var target = _context.Sut;
			target.Create(_context.ComponentId,_context.IPAddress,_context.Port);
			target.Join();

			target.Joined(TODO);
			Assert.IsTrue(target.IsRunning());
			_context.VerifyRunningHandlerWasCalled();
			Assert.IsTrue(target.IsRequestingServices());

			target.ServicesReceived();
			_context.VerifyServicesReceptionExecutor();
		}

		[Test]
		public void WhenRunningCanReceiveSubscriptions()
		{
			var target = _context.Sut;
			target.Create(_context.ComponentId,_context.IPAddress,_context.Port);
			target.Join();

			target.Joined(TODO);
			Assert.IsTrue(target.IsRunning());
			_context.VerifyRunningHandlerWasCalled();

			target.ServicesReceived();
			Assert.IsTrue(target.IsRequestingSubscriptions());

			target.SubscriptionsReceived();
			_context.VerifySubscriptionsReceptionExecutor();
		}

		[Test]
		public void WhenRunningRequestsServices()
		{
			var target = _context.Sut;
			target.Create(_context.ComponentId,_context.IPAddress,_context.Port);
			target.Join();
			target.Joined(TODO);

			Assert.IsTrue(target.IsRequestingServices());
			_context.VerifyRequestingServicesHandlerWasCalled();
		}


		[Test]
		public void WhenServicesReceivedRequestsSubscriptions()
		{
			var target = _context.Sut;
			target.Create(_context.ComponentId,_context.IPAddress,_context.Port);
			target.Join();
			target.Joined(TODO);
			target.ServicesReceived();
			Assert.IsTrue(target.IsRequestingSubscriptions());
			_context.VerifyRequestingSubscriptionsHandlerWasCalled();
		}

		[Test]
		public void CanStop()
		{
			var target = _context.Sut;
			target.Create(_context.ComponentId,_context.IPAddress,_context.Port);
			target.Join();
			target.Joined(TODO);

			_context.VerifyStoppedHandlerWasCalled(1);

			target.Stop();
			Assert.IsTrue(target.IsStopped());
			_context.VerifyStoppedHandlerWasCalled(2);

		}

		[Test]
		public void CanTransitFromStopped_ToError()
		{
			_context.WithExceptionOnStopped();
			var target = _context.Sut;
			Assert.Throws<ermeXRemoteComponentException>(()=>target.Create(_context.ComponentId,_context.IPAddress,_context.Port));
			Assert.IsTrue(target.IsErrored());
			Assert.IsTrue(target.WasCreated());

			_context.VerifyStoppedHandlerWasCalled();
			_context.VerifyErrorHandlerWasCalled();
		}

		[Test]
		public void CanTransitToError_FromCreating()
		{
			_context.WithExceptionOnCreating();
			var target = _context.Sut;
			Assert.Throws<ermeXRemoteComponentException>(() => target.Create(_context.ComponentId, _context.IPAddress, _context.Port));
			Assert.IsTrue(target.IsErrored());
			Assert.IsFalse(target.WasCreated());

			_context.VerifyCreatedHandlerWasCalled();
			_context.VerifyErrorHandlerWasCalled();
		}

		[Test]
		public void CanTransitToStopped_FromJoining()
		{
			_context.WithExceptionOnJoining();
			var target = _context.Sut;
			target.Create(_context.ComponentId,_context.IPAddress,_context.Port);

			target.Join();
			Assert.IsTrue(target.IsStopped());
			Assert.IsFalse(target.IsErrored());
			Assert.IsTrue(target.WasCreated());
			
			_context.VerifyJoiningHandlerWasCalled();
			_context.VerifyStoppedHandlerWasCalled(2);
		}

		[Test]
		public void CanTransitToStopped_FromRequestServices()
		{
			_context.WithExceptionOnRequestingServices();
			var target = _context.Sut;
			target.Create(_context.ComponentId,_context.IPAddress,_context.Port);
			target.Join();

			target.Joined(TODO);
			Assert.IsTrue(target.IsStopped());
			Assert.IsFalse(target.IsErrored());
			Assert.IsTrue(target.WasCreated());

			_context.VerifyRequestingServicesHandlerWasCalled();
			_context.VerifyStoppedHandlerWasCalled(2);
		}

		[Test]
		public void CanTransitToStopped_FromRequestSubscriptions()
		{
			_context.WithExceptionOnRequestingSubscriptions();
			var target = _context.Sut;
			target.Create(_context.ComponentId,_context.IPAddress,_context.Port);
			target.Join();
			target.Joined(TODO);
			target.ServicesReceived();
			Assert.IsTrue(target.IsStopped());
			Assert.IsFalse(target.IsErrored());
			Assert.IsTrue(target.WasCreated());

			_context.VerifyRequestingSubscriptionsHandlerWasCalled();
			_context.VerifyStoppedHandlerWasCalled(2);
		}

		[Test]
		public void CanTransitToStopped_FromRunning()
		{
			_context.WithExceptionOnRunning();
			var target = _context.Sut;
			target.Create(_context.ComponentId,_context.IPAddress,_context.Port);
			target.Join();

			target.Joined(TODO);
			Assert.IsTrue(target.IsStopped());
			Assert.IsFalse(target.IsErrored());
			Assert.IsTrue(target.WasCreated());

			_context.VerifyRunningHandlerWasCalled();
			_context.VerifyStoppedHandlerWasCalled(2);
		}

		[Test]
		public void CanTransitToStopped_FromServicesReceived()
		{
			_context.WithExceptionOnServicesReception();
			var target = _context.Sut;
			target.Create(_context.ComponentId,_context.IPAddress,_context.Port);
			target.Join();
			target.Joined(TODO);
			
			target.ServicesReceived();

			Assert.IsTrue(target.IsStopped());
			Assert.IsFalse(target.IsErrored());
			Assert.IsTrue(target.WasCreated());

			_context.VerifyServicesReceptionExecutor();
			_context.VerifyStoppedHandlerWasCalled(2);
		}

		[Test]
		public void CanTransitToStopped_FromSubscriptionsReceived()
		{
			_context.WithExceptionOnSubscriptionsReception();
			var target = _context.Sut;
			target.Create(_context.ComponentId,_context.IPAddress,_context.Port);
			target.Join();

			target.Joined(TODO);
			target.ServicesReceived();
			target.SubscriptionsReceived();
			Assert.IsTrue(target.IsStopped());
			Assert.IsFalse(target.IsErrored());
			Assert.IsTrue(target.WasCreated());

			_context.VerifySubscriptionsReceptionExecutor();
			_context.VerifyStoppedHandlerWasCalled(2);
		}

		[Test]
		public void InitsInPrelive()
		{
			IRemoteComponentStateMachine target = _context.Sut;

			Assert.IsFalse(target.WasCreated());
			_context.VerifyPreliveHandlerWasCalled(1);
		}
		
	}
}