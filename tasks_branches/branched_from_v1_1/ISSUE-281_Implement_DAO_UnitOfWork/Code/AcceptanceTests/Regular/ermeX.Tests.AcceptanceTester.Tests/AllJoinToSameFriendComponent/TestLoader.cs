using System;
using ermeX.Tests.AcceptanceTester.Base.Loaders;

namespace ermeX.Tests.AcceptanceTester.Tests.AllJoinToSameFriendComponent
{
    class TestLoader : TestLoaderBase, ITestLoader
    {
        /// <summary>
        /// Starts the test and returns a value indicating whether the watcher is on or not
        /// </summary>
        /// <param name="isTestDirector"> </param>
        /// <param name="args"></param>
        /// <returns></returns>
        public bool StartTest(bool isTestDirector, string[] args)
        {
            bool result;

            if (isTestDirector)
            {
                SetConsoleCtrlHandler(new HandlerRoutine(ConsoleCtrlCheck), true);

                var portFrom = int.Parse(args[0]);
                var portTo = int.Parse(args[1]);
                int componentsNumber = int.Parse(args[2]);
                int numberOfMessagesEachComponentSends = int.Parse(args[3]);
                result = bool.Parse(args[4]);
                var waitSecondsEachComponent = int.Parse(args[5]);


                _executor = new TestDirector((ushort)portFrom, (ushort)portTo, componentsNumber, numberOfMessagesEachComponentSends, result, waitSecondsEachComponent);
            }
            else
            {
                // Debugger.Launch();
                ushort port = ushort.Parse(args[0]);
                ushort friendPort = ushort.Parse(args[1]);
                int numberOfMessagesToSend = int.Parse(args[2]);
                var friendComponentId = Guid.Parse(args[3]);
                var currentComponentId = Guid.Parse(args[4]);
                int numberOfMessagesToReceive = int.Parse(args[5]);
                var dbName = args[6];
                result = bool.Parse(args[7]);
                _executor = new TestActor(port, friendPort, numberOfMessagesToSend, numberOfMessagesToReceive,
                                                friendComponentId, currentComponentId, dbName, result);
            }

            try
            {
                _executor.PerformTasks();

                _executor.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error happened. {0}", ex.ToString());
                Console.ReadKey();
            }

            return result;
        }

        #region control app Exit

        // Declare the SetConsoleCtrlHandler function
        // as external and receiving a delegate.

        // A delegate type to be used as the handler routine
        // for SetConsoleCtrlHandler.

        // An enumerated type for the control messages
        // sent to the handler routine.

        #endregion
    }
}
