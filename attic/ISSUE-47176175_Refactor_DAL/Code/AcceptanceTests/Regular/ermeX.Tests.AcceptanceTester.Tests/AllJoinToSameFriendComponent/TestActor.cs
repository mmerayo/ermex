using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using ermeX.Common;
using ermeX.Configuration;
using ermeX.ConfigurationManagement;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.Tests.AcceptanceTester.Base.Messages;
using ermeX.Tests.AcceptanceTester.Base.Services;
using ermeX.Tests.AcceptanceTester.Base.TestExecution;
using ermeX.Tests.AcceptanceTester.Helpers.Db;
using ermeX.Tests.Common;
using ermeX.Tests.Common.RandomValues;

namespace ermeX.Tests.AcceptanceTester.Tests.AllJoinToSameFriendComponent
{
    public class TestActor : TesterBase, ITestExecutor, ITesterService
    {
        //TODO: ENABLE to specify a concrete instance os the static is not needed
        private static ushort _port;
        private static  ushort _friendPort;
        private static int _numberOfMessagesToSend;
        private static int _expectToReceive;
        private static Guid _friendComponentId;
        private static  string _dbName;
        private static  bool _watcherOn;
        private static readonly AutoResetEvent CanFinishEvent = new AutoResetEvent(false);
        private static readonly AutoResetEvent CanStartEvent = new AutoResetEvent(false);
        const string prefix = "Component";
        public TestActor(ushort port, ushort friendPort, int numberOfMessagesToSend, int expectToReceive,
            Guid friendComponentId, Guid currentComponentId, string dbName, bool watcherOn)
        {
            _port = port;
            _friendPort = friendPort;
            _numberOfMessagesToSend = numberOfMessagesToSend;
            _expectToReceive = expectToReceive;
            _friendComponentId = friendComponentId;
            _dbName = dbName;
            _watcherOn = watcherOn;
            _currentComponentId = currentComponentId;
            LogComponentDetails();
            StartComponent();
            Console.WriteLine("TesterComponent-->Started");
            SubscribeToMessages();
            Console.WriteLine("TesterComponent-->Subscribed to messages");
            

        }

        public override void Dispose()
        {
            base.Dispose();
            try
            {
                if (!_watcherOn)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(10));
                    SqlDbHelper.DropDatabase(_currentComponentId, prefix);
                }
              
            }
            catch
            {
            }
        }

        private void LogComponentDetails()
        {
            ConsoleColor foregroundColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("name:{1}{0}port: {2}{0}",Environment.NewLine,_currentComponentId,_port);
            Console.ForegroundColor = foregroundColor;
        }

        public TestActor(){}

        public void SendMeTheResults()
        {
            var directorService = WorldGate.GetServiceProxy<IDirectorService>();
            Results currentResults = MessageSubscriber.CurrentResults;
            Debug.Assert(currentResults!=null);
            directorService.MyResults(_currentComponentId, currentResults);
            Console.WriteLine("Invoked director. MyResults");
            CanFinishEvent.Set();
        }
        public void CanStart()
        {
            CanStartEvent.Set();
        }

        public void PerformTasks()
        {

            var testerService = WorldGate.GetServiceProxy<IDirectorService>();
            testerService.ImReadyToGo(_currentComponentId);
            Console.WriteLine("Invoked director. ImReadyToGo");

            PublishMessages();

            WaitMessagesReception();

            NotifyDirectorImDone();
        }

        private static void NotifyDirectorImDone()
        {
            var directorService = WorldGate.GetServiceProxy<IDirectorService>();
            directorService.ImFinished(_currentComponentId);
            Console.WriteLine("Invoked director. ImFinished");
            CanFinishEvent.WaitOne(TimeSpan.FromMinutes(2));
        }

        private readonly AutoResetEvent AllMessagesReceived=new AutoResetEvent(false);
        private void WaitMessagesReception()
        {
            AllMessagesReceived.WaitOne();
        }

        private void PublishMessages()
        {
            CanStartEvent.WaitOne();
            while (_numberOfMessagesToSend-- > 0)
            {
                PublishMessage();
            }
        }

        private void SubscribeToMessages()
        {
            MessageSubscriber.Reset(_expectToReceive,AllMessagesReceived);
            WorldGate.Suscribe(typeof(MessageSubscriber));
        }

        private void PublishMessage()
        {
            int msgtype;
            var message = CreateRandomMessage(out msgtype);

            WorldGate.Publish(message);

            Console.WriteLine(string.Format("Component:{0} Published message:{1}",_currentComponentId,message.GetType().Name));

            UpdatePublishedDetails(msgtype);
        }

        private void UpdatePublishedDetails(int msgtype)
        {
            Results.ResultDetail detail = null;
            switch (msgtype)
            {
                case 1:
                    detail = MessageSubscriber.GetResults<AcceptanceMessageType1>();
                    break;
                case 2:
                    detail = MessageSubscriber.GetResults<AcceptanceMessageType2>();
                    break;
                case 3:
                    detail = MessageSubscriber.GetResults<AcceptanceMessageType3>();
                    break;
            }
            detail.AddPublished();
        }

        private static AcceptanceMessageType CreateRandomMessage(out int msgtype)
        {
            AcceptanceMessageType message = null;
            msgtype = RandomHelper.GetRandomInt(1, 3);
            switch (msgtype)
            {
                case 1:
                    message = new AcceptanceMessageType1(true);
                    break;
                case 2:
                    message = new AcceptanceMessageType2(true);
                    break;
                case 3:
                    message = new AcceptanceMessageType3(true);
                    break;
            }
            message.SenderId = _currentComponentId;
            return message;
        }


        private void StartComponent()
        {
            string connStr=string.Empty;

            if (_watcherOn)
            {
                _dbName = SqlDbHelper.CreateDatabase(_currentComponentId, prefix);
                connStr = SqlDbHelper.GetConnectionString(_dbName);
            }

            var cfg = Configurer.Configure(_currentComponentId)
                .ListeningToTcpPort(_port)
                .DiscoverServicesToPublish(new[] {this.GetType().Assembly}, new[] {typeof (IDirectorService)})
                .DiscoverSubscriptors(new[] {this.GetType().Assembly}, null)
                .RequestJoinTo(Networking.GetLocalhostIp(), _friendPort, _friendComponentId);
                

            cfg = _watcherOn ? cfg.SetSqlServerDb(connStr) : cfg.SetInMemoryDb();

            try
            {
                WorldGate.ConfigureAndStart(cfg);
            }catch(Exception ex)
            {
                Console.WriteLine(ex);
                //Debugger.Launch();
            }
            //WorldGate.RegisterService<ITesterService>(GetType());
        }

       
    }
}