using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using ermeX.Common;
using ermeX.Configuration;
using ermeX.ConfigurationManagement;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.Tests.AcceptanceTester.Base.Messages;
using ermeX.Tests.AcceptanceTester.Base.Services;
using ermeX.Tests.AcceptanceTester.Base.TestExecution;
using ermeX.Tests.AcceptanceTester.Helpers.Db;

namespace ermeX.Tests.AcceptanceTester.Tests.AllJoinToSameFriendComponent
{
    public class TestDirector :TesterBase,  ITestExecutor, IDirectorService
    {
        public int WaitSecondsEachComponent { get; set; }

        private class ComponentInfo
        {
            public ComponentInfo(Guid nameId)
            {
                if(nameId.IsEmpty())
                    throw new ArgumentException("nameId");
                NameId = nameId;
                FinishPublishingEvent=new AutoResetEvent(false);
                ResultsReceivedEvent=new AutoResetEvent(false);
                ReadyEvent = new AutoResetEvent(false);
            }

            public Guid NameId { get; private set; }
            public Process Runner { get; set; }
            public AutoResetEvent FinishPublishingEvent { get;private set; }
            public AutoResetEvent ResultsReceivedEvent { get; private set; }
            public AutoResetEvent ReadyEvent { get; private set; }
        }

        //TODO: THIS SHOULDNT BE static once the service instance can be set
        private static  ushort _portFrom;
        private static ushort _portTo;
        private static int _componentsNumber;
        private static int _numberOfMessagesEachComponentSends;
        private static Dictionary<Guid, ComponentInfo> _components;
        private static Dictionary<Guid, Results> _results = new Dictionary<Guid, Results>();
        private static DateTime _startTimeAbsolute;
        private static DateTime _startTimeSending;
        private static string _dbName=string.Empty;
        private static bool _watcherOn;
        const string prefix = "Dir";
        public TestDirector(ushort portFrom, ushort portTo, int componentsNumber, int numberOfMessagesEachComponentSends, bool watcherOn, int waitSecondsEachComponent)
        {
            WaitSecondsEachComponent = waitSecondsEachComponent;
            _portFrom = portFrom;
            _portTo = portTo;
            _componentsNumber = componentsNumber;
            _numberOfMessagesEachComponentSends = numberOfMessagesEachComponentSends;
            _watcherOn = watcherOn;
            _currentComponentId = Guid.NewGuid();
            _components = new Dictionary<Guid, ComponentInfo>(_componentsNumber);

        }

        public TestDirector()
        {
            
        }

        public override void Dispose()
        {
            base.Dispose();
            foreach (var key in _components.Keys)
            {
                var process = _components[key].Runner;

                if (!process.HasExited)
                    process.Kill();
            }

            if (!_watcherOn)
                SqlDbHelper.DropDatabase(_currentComponentId,prefix);
        }
        public void PerformTasks()
        {
            StartComponent();
            _startTimeAbsolute = DateTime.UtcNow;
            CreateComponents();
            StartTest();

            WaitFinish();

            CollectResults();

            PublishResults();

        }

        private void StartTest()
        {
            
            var componentsAreReady = _components.Values.Select(x => (WaitHandle)x.ReadyEvent).ToArray();
            WaitHandle.WaitAll(componentsAreReady);
            _startTimeSending = DateTime.UtcNow;
            
            foreach (var key in _components.Keys)
            {
                var testerService = WorldGate.GetServiceProxy<ITesterService>(key);
                testerService.CanStart();
                Console.WriteLine("Invoked Tester.CanStart Component:{0}", key);
            }
        }

        private void PublishResults()
        {
            bool result=true;
            int a1Sent, a2Sent, a3Sent;
            a1Sent = a2Sent = a3Sent = 0;

            foreach (var key in _results.Keys)
            {
                var results = _results[key];
                var resultDetail1 = results.GetResults<AcceptanceMessageType1>();
                var resultDetail2 = results.GetResults<AcceptanceMessageType2>();
                var resultDetail3 = results.GetResults<AcceptanceMessageType3>();
                a1Sent += resultDetail1.TotalPublished;
                a2Sent += resultDetail2.TotalPublished;
                a3Sent += resultDetail3.TotalPublished;
            }

            foreach (var key in _results.Keys)
            {
                var results = _results[key];
                var resultDetail1 = results.GetResults<AcceptanceMessageType1>();
                var resultDetail2 = results.GetResults<AcceptanceMessageType2>();
                var resultDetail3 = results.GetResults<AcceptanceMessageType3>();

                TimeSpan totalTime = results.FinishTimeUtc - _startTimeAbsolute;
                Console.WriteLine(string.Format("*****CompId: {0} OperationTime (Absolute):{1} OperationTime (Sending):{2}", key, totalTime,results.FinishTimeUtc-_startTimeSending));
                Console.WriteLine(string.Format("CompId: {1}{0} AcceptanceMessageType1 sent:{2} || received: {3}{0}" +
                                                "AcceptanceMessageType2 sent:{4} || received: {5}{0}" +
                                                "AcceptanceMessageType3 sent:{6} || received: {7}{0}"
                                                , Environment.NewLine, key, resultDetail1.TotalPublished,
                                                resultDetail1.TotalReceived, resultDetail2.TotalPublished,
                                                resultDetail2.TotalReceived, resultDetail3.TotalPublished,
                                                resultDetail3.TotalReceived));
                foreach (var key1 in _components.Keys)
                {
                    Console.WriteLine(
                        string.Format("---From {1}{0}" +
                                      " AcceptanceMessageType1 Received: {2}{0}AcceptanceMessageType2 Received: {3}{0}AcceptanceMessageType3 Received: {4}{0}",
                                      Environment.NewLine, key1, resultDetail1.TotalReceivedByComponent(key1),
                                      resultDetail2.TotalReceivedByComponent(key1),
                                      resultDetail3.TotalReceivedByComponent(key1)));
                }
                result=result & PublishCheckResults(key,a1Sent,a2Sent,a3Sent,results);

                Console.WriteLine("******************************************************************************");
            }
            ConsoleColor foregroundColor = Console.ForegroundColor;
            Console.ForegroundColor = result? ConsoleColor.Yellow : ConsoleColor.Red;
            Console.WriteLine("The final result is: {0}", result ? "PASSED" : "FAILED");
            
            Environment.ExitCode = result ? 0 : 1;
            Console.ForegroundColor = foregroundColor;
        }

        private bool PublishCheckResults(Guid componentKey, int a1Sent, int a2Sent, int a3Sent, Results results)
        {
            ConsoleColor foregroundColor = Console.ForegroundColor;
            
            int totalReceived1 = results.GetResults<AcceptanceMessageType1>().TotalReceived;
            Console.ForegroundColor = a1Sent != totalReceived1 ? ConsoleColor.Red : ConsoleColor.Green;
            Console.WriteLine(string.Format("Component:{0}, message:{1}, expected:{2}, received:{3}", componentKey,
                                            typeof(AcceptanceMessageType1).Name, a1Sent, totalReceived1));

            int totalReceived2 = results.GetResults<AcceptanceMessageType2>().TotalReceived;
            Console.ForegroundColor = a2Sent != totalReceived2 ? ConsoleColor.Red : ConsoleColor.Green;
            Console.WriteLine(string.Format("Component:{0}, message:{1}, expected:{2}, received:{3}", componentKey,
                                            typeof(AcceptanceMessageType2).Name, a2Sent, totalReceived2));

            int totalReceived3 = results.GetResults<AcceptanceMessageType3>().TotalReceived;
            Console.ForegroundColor = a3Sent != totalReceived3 ? ConsoleColor.Red : ConsoleColor.Green;
            Console.WriteLine(string.Format("Component:{0}, message:{1}, expected:{2}, received:{3}", componentKey,
                                            typeof(AcceptanceMessageType3).Name, a3Sent, totalReceived3));

            Console.ForegroundColor = foregroundColor;

            return a1Sent == totalReceived1 && a3Sent == totalReceived3 && a3Sent == totalReceived3;
        }

        
        private void CollectResults()
        {
            foreach (var key in _components.Keys)
            {
                var testerService = WorldGate.GetServiceProxy<ITesterService>(key);

                testerService.SendMeTheResults();
                Console.WriteLine(string.Format("Invoked Tester.SendMeTheResults Component:{0}", key));
            }
            WaitHandle[] waitHandles = _components.Values.Select(x => (WaitHandle) x.ResultsReceivedEvent).ToArray();
            WaitHandle.WaitAll(waitHandles, TimeSpan.FromSeconds(30));
        }

        private void WaitFinish()
        {
            WaitHandle[] waitHandles = _components.Values.Select(x => (WaitHandle) x.FinishPublishingEvent).ToArray();
            WaitHandle.WaitAll(waitHandles);
        }

        private void CreateComponents()
        {
            int startPort = _portFrom + 1;

            for (int i = 0; i < _componentsNumber; i++)
            {
                string exeFile = Path.Combine(PathUtils.GetApplicationFolderPath(), "ermeX.Tests.AcceptanceTester.exe");
                
                Guid componentCreatedId = Guid.NewGuid();
                string arguments = GetTesterArguments(componentCreatedId, i, startPort);


                var value = new ComponentInfo(componentCreatedId)
                                {
                                    Runner = StartProccess(exeFile, arguments)
                                };
                _components.Add(componentCreatedId, value);
                _results.Add(componentCreatedId, null);

                Thread.Sleep(TimeSpan.FromSeconds(WaitSecondsEachComponent));//TODO: REMOVE when fixing startup collisions
            }
        }

        private static string GetTesterArguments(Guid componentCreatedId, int i, int startPort)
        {
            return string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9}", "AllJoinToSameFriendComponent","false",
                                 Networking.GetFreePort((ushort)(startPort + i), _portTo),
                                 _portFrom,
                                 _numberOfMessagesEachComponentSends,
                                 _currentComponentId,
                                 componentCreatedId,
                                 _numberOfMessagesEachComponentSends * _componentsNumber,
                                 _dbName,
                                 _watcherOn.ToString(CultureInfo.InvariantCulture)
                );
        }

        private void StartComponent()
        {

            string connStr=null;
            if (_watcherOn)
            {
                _dbName = SqlDbHelper.CreateDatabase(_currentComponentId, prefix);
                Console.WriteLine("Db: {0}", _dbName);
                connStr = SqlDbHelper.GetConnectionString(_dbName);
                UpdateLogConnectionStringSettings(connStr);
                
            }
            else
            {
                _dbName = "In-Memory";
            }

            var cfg = Configurer.Configure(_currentComponentId)
                .DiscoverServicesToPublish(new[] {this.GetType().Assembly}, new[] {typeof (ITesterService)})
                .ListeningToTcpPort(_portFrom);
            cfg = _watcherOn ? cfg.SetSqlServerDb(connStr) : cfg.SetInMemoryDb();

            WorldGate.ConfigureAndStart(cfg);

            //WorldGate.RegisterService<IDirectorService>(GetType());
        }

        private static Process StartProccess(string exeFile, string arguments, string startAt = null, bool redirectOutput = false)
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = exeFile,
                    Arguments = arguments,
                    UseShellExecute = _watcherOn,
                    RedirectStandardOutput = redirectOutput
                }
            };
            if (!string.IsNullOrEmpty(startAt))
                process.StartInfo.WorkingDirectory = startAt;
            process.Start();
            return process;
        }

        public void ImFinished(Guid componentId)
        {
            _components[componentId].FinishPublishingEvent.Set();
        }

        public void MyResults(Guid componentName, Results myResults)
        {
            _results[componentName] = myResults;
            _components[componentName].ResultsReceivedEvent.Set();
        }

        public void ImReadyToGo(Guid componentName)
        {
            
            _components[componentName].ReadyEvent.Set();
            Console.WriteLine(string.Format("component: {0} is ready to go",componentName));
        }

       
        
    }
}