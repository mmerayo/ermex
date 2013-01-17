using System;
using System.Collections.Generic;
using System.Threading;
using ermeX.Bus.Interfaces;
using ermeX.Tests.AcceptanceTester.Base.Messages;

namespace ermeX.Tests.AcceptanceTester.Base.TestExecution
{
    public class MessageSubscriber :
        IHandleMessages<AcceptanceMessageType1>, IHandleMessages<AcceptanceMessageType2>, IHandleMessages<AcceptanceMessageType3>
    {
        private static Results _currentResults;

        public static Results CurrentResults
        {
            get { return _currentResults; }
        }

        public static void Reset(int expectedMessages, AutoResetEvent allReceived)
        {
            _currentResults = new Results(expectedMessages,allReceived);
            _currentResults.DefineCollectable<AcceptanceMessageType1>();
            _currentResults.DefineCollectable<AcceptanceMessageType2>();
            _currentResults.DefineCollectable<AcceptanceMessageType3>();

        }

        private static DateTime _lastReception = DateTime.MinValue;

        private static readonly object _locker = new object();
        private void CheckOrder(AcceptanceMessageType message)
        {
            lock (_locker)
            {
                //we admit an error of 500ms.
                if (!(message.CreationUtc >= _lastReception) && Math.Abs(message.CreationUtc.Subtract(_lastReception).TotalMilliseconds)>500)
                {
                    string msg = string.Format("The order of the messages is not correct message.CreationUtc: {0} _lastReception:{1}", message.CreationUtc.ToString("MM/dd/yyyy hh:mm:ss.fff tt"), _lastReception.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
                    Console.WriteLine(msg);
                    Environment.Exit(1);
                }
                _lastReception = message.CreationUtc;
            }
        }
        public void HandleMessage(AcceptanceMessageType1 message)
        {
            CheckOrder(message);
                
            LogReception(message);
            _currentResults.AddReceived<AcceptanceMessageType1>(message.SenderId);
        }


        public void HandleMessage(AcceptanceMessageType2 message)
        {
            CheckOrder(message);
            LogReception(message);
            _currentResults.AddReceived<AcceptanceMessageType2>(message.SenderId);
        }

        public void HandleMessage(AcceptanceMessageType3 message)
        {
            CheckOrder(message);
            LogReception(message);
            _currentResults.AddReceived<AcceptanceMessageType3>(message.SenderId);
        }

        public static Results.ResultDetail GetResults<TMessage>()
        {
            return _currentResults.GetResults<TMessage>();
        }
        private static void LogReception(object message)
        {
            Console.WriteLine("Received message: {0} ", message.GetType().Name);
        }
    }
}