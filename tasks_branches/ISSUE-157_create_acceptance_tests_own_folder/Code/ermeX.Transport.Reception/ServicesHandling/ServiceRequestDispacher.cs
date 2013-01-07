// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Common.Logging;
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.Interfaces;

using ermeX.Interfaces;
using ermeX.Transport.Interfaces;
using ermeX.Transport.Interfaces.Messages;

namespace ermeX.Transport.Reception.ServicesHandling
{
    internal class ServiceRequestDispacher : IServiceHandler
    {
        private readonly Dictionary<Guid, MethodInfo> Operations = new Dictionary<Guid, MethodInfo>();

        public ServiceRequestDispacher(IService realHandlerInstance, IServiceDetailsDataSource dataSource)
        {
            if (realHandlerInstance == null) throw new ArgumentNullException("realHandlerInstance");
            if (dataSource == null) throw new ArgumentNullException("dataSource");

            RealHandlerInstance = realHandlerInstance;
            DataSource = dataSource;
        }

        private readonly object SyncLock=new object(); 


        private IService RealHandlerInstance { get; set; }
        private IServiceDetailsDataSource DataSource { get; set; }
        private readonly ILog Logger=LogManager.GetLogger(StaticSettings.LoggerName);

        #region IServiceHandler Members

        public void Dispose()
        {
            var toDispose = RealHandlerInstance as IDisposable;
            if (toDispose != null)
                toDispose.Dispose();
        }

        public object Handle(object message)
        {
            Logger.Debug(x=>x("Handling {0}",((ServiceRequestMessage)message).Operation));
            var request = EnsureParametersTypeAfterSerialization((ServiceRequestMessage) message);
            if (!Operations.ContainsKey(request.Operation))
                lock (SyncLock)
                    if (!Operations.ContainsKey(request.Operation))
                    {
                        var svc = DataSource.GetByOperationId(request.Operation);
                        var method = TypesHelper.GetPublicInstanceMethod(svc.ServiceInterfaceTypeName,
                                                                         svc.ServiceImplementationMethodName);
                        if (method == null)
                            throw new EntryPointNotFoundException("The method was not found in the type");
                        Operations.Add(request.Operation, method);
                    }

            //TODO: REMOVE THIS Create tests con parametros de distintos tipos ver que falla y aplicar convertidor aqui

            object[] parameters = request.Parameters != null
                                      ? request.Parameters.Values.Select(x => x.ParameterValue).ToArray()
                                      : new object[0];
            var result =TypesHelper.InvokeFast(Operations[request.Operation],RealHandlerInstance, parameters);
            Logger.Debug(x=>x("Handled {0} with result:{1}", ((ServiceRequestMessage)message).Operation, result));
            return result;
        }

        //TODO: WE NEED TO CHECK IF THIS IS STILL NEEDED WHEN communication SERIALiZATION CHANGES. tEH REASON THIS IS HERE IS BECAUSe SOME PARAMETER TYPES LIKE (gUID) ARE SERIALZIED AS STRING
        private ServiceRequestMessage EnsureParametersTypeAfterSerialization(ServiceRequestMessage message)
        {
            string stringTypeFullName = typeof(string).FullName;

            if (message.Parameters != null)
                foreach (var requestParameter in message.Parameters.Values)
                {
                    if (requestParameter.ParameterValue is string &&
                        requestParameter.PTypeName != stringTypeFullName)
                        requestParameter.ParameterValue = TypesHelper.ConvertFrom(requestParameter.PTypeName,
                                                                                  requestParameter.ParameterValue);
                    else if (TypesHelper.GetTypeFromDomain(requestParameter.PTypeName).IsEnum)
                        requestParameter.ParameterValue = TypesHelper.ConvertFrom(requestParameter.PTypeName,
                                                                                  requestParameter.ParameterValue.ToString());
                    
                }
            return message;
        }

        #endregion
    }
}