// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace ermeX.ConfigurationManagement.Status
{

    internal sealed class StatusManager : IStatusManager
    {
        private readonly object SyncLock = new object();

        public StatusManager()
        {

        }

        private ComponentStatus _currentStatus;

        private event StatusChangedHandler _statusChanged;

        public event StatusChangedHandler StatusChanged
        {
            add { _statusChanged += value; }
            remove { _statusChanged -= value; }
        }

        private readonly object _statusSyncLock = new object();

        public ComponentStatus CurrentStatus
        {
            get
            {
                lock (_statusSyncLock)
                    return _currentStatus;
            }
            set
            {
                lock (_statusSyncLock)
                {
                    if (!ValidateTransition(_currentStatus, value))
                    {
                        Debugger.Break();
                        throw new InvalidOperationException(
                            string.Format("The component status cannot transit from {0} to {1}", _currentStatus, value));
                    }

                    _currentStatus = value;

                    if (_currentStatus == ComponentStatus.Running)
                        _waitIsRunningEvent.Set();
                    else
                        _waitIsRunningEvent.Reset();
                }
                lock (SyncLock)
                    OnStatusChanged();
            }
        }

        private void OnStatusChanged()
        {
            if (_statusChanged != null)
                _statusChanged(this, _currentStatus);
        }

        private bool ValidateTransition(ComponentStatus currentStatus, ComponentStatus value)
        {
            switch (currentStatus)
            {
                case ComponentStatus.Stopped:
                    return value == ComponentStatus.Starting;
                case ComponentStatus.Starting:
                    return value == ComponentStatus.Running;
                case ComponentStatus.Running:
                    return value == ComponentStatus.Stopping;
                case ComponentStatus.Stopping:
                    return value == ComponentStatus.Stopped;
                default:
                    throw new ArgumentOutOfRangeException("currentStatus");
            }
        }

        private readonly ManualResetEvent _waitIsRunningEvent = new ManualResetEvent(false);

        public void WaitIsRunning()
        {
            _waitIsRunningEvent.WaitOne();
        }

        #region ExchangedDefinitionsEvent

        //TODO: MOVE and refactor

        private readonly GlobalSync _syncEvents = new GlobalSync();

        public GlobalSync SyncEvents
        {
            get { return _syncEvents; }
        }

        public class GlobalSync
        {
            public enum GlobalEvents
            {
                DefinitionsExchanged = 1
            }

            private readonly object _syncLock = new object();

            private readonly Dictionary<Guid, ManualResetEvent> _definitionsExchangedEvents =
                new Dictionary<Guid, ManualResetEvent>();

            public void CreateEvent(GlobalEvents theEvent, Guid componentId)
            {
                switch (theEvent)
                {
                    case GlobalEvents.DefinitionsExchanged:
                        CreateEvent(_definitionsExchangedEvents, componentId);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("theEvent");
                }

            }

            public void SignalEvent(GlobalEvents theEvent, Guid componentId)
            {
                switch (theEvent)
                {
                    case GlobalEvents.DefinitionsExchanged:
                        SignalEvent(_definitionsExchangedEvents, componentId);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("theEvent");
                }
            }

            public void WaitAndRemove(GlobalEvents theEvent, Guid componentId, int seconds=15)
            {
                switch (theEvent)
                {
                    case GlobalEvents.DefinitionsExchanged:
                        WaitEvent(_definitionsExchangedEvents, componentId,seconds);
                        RemoveEvent(_definitionsExchangedEvents, componentId);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("theEvent");
                }
            }

            public void Wait(GlobalEvents theEvent, Guid componentId, int seconds = 15)
            {
                switch (theEvent)
                {
                    case GlobalEvents.DefinitionsExchanged:
                        WaitEvent(_definitionsExchangedEvents, componentId, seconds);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("theEvent");
                }
            }

            private void CreateEvent(Dictionary<Guid, ManualResetEvent> dictionary, Guid componentId)
            {
                lock (_syncLock)
                {
                    if (!dictionary.ContainsKey(componentId))
                        dictionary.Add(componentId, new ManualResetEvent(false));
                    else
                        dictionary[componentId].Reset();
                }
            }



            private void SignalEvent(Dictionary<Guid, ManualResetEvent> dictionary, Guid componentId)
            {
                lock (_syncLock)
                {
                    if (!dictionary.ContainsKey(componentId))
                        CreateEvent(dictionary,componentId);

                    dictionary[componentId].Set();
                }
            }



            private void RemoveEvent(Dictionary<Guid, ManualResetEvent> dictionary, Guid componentId)
            {
                if (dictionary.ContainsKey(componentId))
                    lock (_syncLock)
                        if (dictionary.ContainsKey(componentId))
                            dictionary.Remove(componentId);
            }

            private void WaitEvent(Dictionary<Guid, ManualResetEvent> dictionary, Guid componentId,int seconds)
            {
                lock (_syncLock)
                {
                    if (!dictionary.ContainsKey(componentId)) return;
                    //CreateEvent(dictionary, componentId);
                    dictionary[componentId].WaitOne(TimeSpan.FromSeconds(seconds));
                }
            }

            
        }

        #endregion
    }
}