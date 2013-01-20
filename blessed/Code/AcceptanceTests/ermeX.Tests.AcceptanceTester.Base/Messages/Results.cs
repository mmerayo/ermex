using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ermeX.Tests.AcceptanceTester.Base.Messages
{

    public class Results
    {
        private int _expectedMessages;
        private readonly AutoResetEvent _receivedAllEvent;
        private readonly object SyncLock = new object();

        public Results() : this(0, null)
        {
        }

        public Results(int expectedMessages, AutoResetEvent receivedAllEvent)
        {
            _expectedMessages = expectedMessages;
            _receivedAllEvent = receivedAllEvent;
            Data = new Dictionary<string, ResultDetail>();
        }

        public Dictionary<string, ResultDetail> Data { get; set; }
        public DateTime FinishTimeUtc { get; set; }

        public void DefineCollectable<TMessage>()
        {
            string typeName = typeof (TMessage).FullName;
            if (!Data.Keys.Contains(typeName))
                lock (SyncLock)
                    if (!Data.Keys.Contains(typeName))
                    {
                        var resultDetail = new ResultDetail();
                        resultDetail.MessageReceived += OnMessageReceived;
                        Data.Add(typeName, resultDetail);
                    }
        }

        private void OnMessageReceived(object sender, EventArgs args)
        {
            if (--_expectedMessages == 0)
            {
                FinishTimeUtc = DateTime.UtcNow;
                _receivedAllEvent.Set();
            }
        }

        public ResultDetail GetResults<TMessage>()
        {
            return Data[typeof (TMessage).FullName];
        }

        public int TotalPublished<TMessage>()
        {
            return GetResults<TMessage>().TotalPublished;
        }

        public int TotalReceived<TMessage>()
        {
            return GetResults<TMessage>().TotalReceived;
        }

        public int TotalReceivedByComponent<TMessage>(Guid senderId)
        {
            return GetResults<TMessage>().TotalReceivedByComponent(senderId);
        }

        public void AddReceived<TMessage>(Guid senderId)
        {
            GetResults<TMessage>().AddReceived(senderId);
        }

        public void AddPublished<TMessage>()
        {
            GetResults<TMessage>().AddPublished();
        }

        public class ResultDetail
        {
            private readonly object _syncLock = new object();

            public event EventHandler MessageReceived;

            public ResultDetail()
            {
                ReceivedData = new Dictionary<Guid, int>();
            }


            public Dictionary<Guid, int> ReceivedData { get; private set; }

            public int TotalPublished { get; private set; }

            public void AddReceived(Guid senderId)
            {
                lock (_syncLock)
                {
                    if (!ReceivedData.ContainsKey(senderId))
                        ReceivedData.Add(senderId, 0);

                    ReceivedData[senderId]++;
                    OnMessageReceived();
                }
            }

            public int TotalReceivedByComponent(Guid senderId)
            {
                return ReceivedData.ContainsKey(senderId) ? ReceivedData[senderId] : 0;
            }

            public int TotalReceived
            {
                get { return ReceivedData.Values.Sum(x => x); }
            }

            private void OnMessageReceived()
            {
                if (MessageReceived != null)
                    MessageReceived(this, null);
            }

            public void AddPublished()
            {
                lock (_syncLock)
                    TotalPublished++;
            }
        }
    }
}
