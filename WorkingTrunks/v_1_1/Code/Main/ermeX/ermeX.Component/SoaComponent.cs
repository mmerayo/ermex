// /*---------------------------------------------------------------------------------------*/
//        Licensed to the Apache Software Foundation (ASF) under one
//        or more contributor license agreements.  See the NOTICE file
//        distributed with this work for additional information
//        regarding copyright ownership.  The ASF licenses this file
//        to you under the Apache License, Version 2.0 (the
//        "License"); you may not use this file except in compliance
//        with the License.  You may obtain a copy of the License at
// 
//          http://www.apache.org/licenses/LICENSE-2.0
// 
//        Unless required by applicable law or agreed to in writing,
//        software distributed under the License is distributed on an
//        "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//        KIND, either express or implied.  See the License for the
//        specific language governing permissions and limitations
//        under the License.
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
using ermeX.NonMerged;
using ermeX.Threading.Queues;


namespace ermeX.ermeX.Component
{
    /// <summary>
    ///   Base class for each component
    /// </summary>
    public abstract class SoaComponent:IDisposable
    {
        static SoaComponent()
        {
            ResolveUnmerged.Prepare();
#if DEBUG //TODO: MOVE TO THE TESTFIXTURESETUPS
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
                SystemTaskQueue.Reset();
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