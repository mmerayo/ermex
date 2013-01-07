// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System.Threading;

namespace ermeX.Common
{
    public sealed class SyncEvents
    {
        public const int NewItemArrived = 0;

        public const int EXIT_INDEX = 2;

        public const int NewDispatchableItem = 1;
        private readonly WaitHandle[] _eventArray;
        private readonly EventWaitHandle _exitThreadEvent;
        private readonly EventWaitHandle _newArrivedItemEvent;
        private readonly EventWaitHandle _newDispatchableItemEvent;

        public SyncEvents()
        {
            _newArrivedItemEvent = new AutoResetEvent(false);
            _newDispatchableItemEvent = new AutoResetEvent(false);
            _exitThreadEvent = new ManualResetEvent(false);
            _eventArray = new WaitHandle[3];
            _eventArray[NewItemArrived] = _newArrivedItemEvent;
            _eventArray[EXIT_INDEX] = _exitThreadEvent;
            _eventArray[NewDispatchableItem] = NewDispatchableItemEvent;
        }

        public EventWaitHandle ExitThreadEvent
        {
            get { return _exitThreadEvent; }
        }

        public EventWaitHandle NewItemArrivedEvent
        {
            get { return _newArrivedItemEvent; }
        }

        public WaitHandle[] EventArray
        {
            get { return _eventArray; }
        }

        public EventWaitHandle NewDispatchableItemEvent
        {
            get { return _newDispatchableItemEvent; }
        }
    }
}