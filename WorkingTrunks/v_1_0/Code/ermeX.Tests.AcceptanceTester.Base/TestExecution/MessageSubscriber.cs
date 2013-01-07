using System;
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

        public void HandleMessage(AcceptanceMessageType1 message)
        {
            LogReception(message);
            _currentResults.AddReceived<AcceptanceMessageType1>(message.SenderId);
        }

        public void HandleMessage(AcceptanceMessageType2 message)
        {
            LogReception(message);
            _currentResults.AddReceived<AcceptanceMessageType2>(message.SenderId);
        }

        public void HandleMessage(AcceptanceMessageType3 message)
        {
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