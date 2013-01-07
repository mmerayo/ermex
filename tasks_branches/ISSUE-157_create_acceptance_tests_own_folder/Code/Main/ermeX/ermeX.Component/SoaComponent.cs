// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Diagnostics;
using System.Linq;
using Common.Logging;
using Common.Logging.Simple;
using Ninject;
using ermeX.Common;
using ermeX.ConfigurationManagement.IoC;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Status;



namespace ermeX.ermeX.Component
{
    /// <summary>
    ///   Base class for each component
    /// </summary>
    public abstract class SoaComponent:IDisposable
    {
        static SoaComponent()
        {
#if DEBUG
            if(LogManager.Adapter is NoOpLoggerFactoryAdapter)
                LogManager.Adapter = new ConsoleOutLoggerFactoryAdapter(LogLevel.All, true, true, true, "yyyy/MM/dd HH:mm:ss:fff");
#endif
        }

        protected static readonly ILog Logger = LogManager.GetLogger(StaticSettings.LoggerName); 

        [Inject]
        internal IStatusManager StatusManager { get; set; }

        internal bool IsStarted
        {
            get { return IoCManager.Kernel != null && StatusManager.CurrentStatus==ComponentStatus.Running; }
        }

        internal void ResetAll()
        {
            if (IoCManager.Kernel == null) return;
            
            if (StatusManager.CurrentStatus != ComponentStatus.Running)
                return;
            try
            {
                try
                {
                    StatusManager.CurrentStatus = ComponentStatus.Stopping;
                }catch(Exception ex)
                {
                    IoCManager.Reset();
                    throw ex;
                }
                IoCManager.Reset();
                StatusManager.CurrentStatus=ComponentStatus.Stopped;
            }
            catch (Exception ex)
            {
                Debugger.Launch();

                Logger.Warn(x=>x("ResetAll: Exception:{0} ", ex));
                throw ex;
            }
        }

        internal static TService Get<TService>()
        {
            try
            {
                return IoCManager.Kernel.Get<TService>();
            }
            catch (Exception ex)
            {
                if (!IoCManager.Kernel.GetBindings(typeof (TService)).Any())
                    return default(TService);
                    Logger.Error(x=>x("Error while trying to get from the container the service:{0} Exception:{1}",
                                               typeof (TService).FullName, ex));
                throw ex;
            }
        }

        protected bool _disposed = false;
        public void Dispose()
        {
            if(!_disposed)
            {
                ResetAll();
                _disposed = true;
            }
        }
    }
}