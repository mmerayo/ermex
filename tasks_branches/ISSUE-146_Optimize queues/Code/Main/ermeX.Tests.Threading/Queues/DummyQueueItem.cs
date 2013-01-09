using System;

namespace ermeX.Tests.Threading.Queues
{
    public class DummyQueueItem
    {
        private readonly int _value;
        private readonly DateTime _time;

        public DummyQueueItem(int value,DateTime time)
        {
            _value = value;
            _time = time;
        }

       

        public int Value
        {
            get { return _value; }
        }

        public DateTime Time
        {
            get { return _time; }
        }
    }
}