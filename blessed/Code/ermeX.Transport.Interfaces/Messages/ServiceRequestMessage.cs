// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using ProtoBuf;
using ermeX.Common;
using ermeX.LayerMessages;

namespace ermeX.Transport.Interfaces.Messages
{
    [ProtoContract(SkipConstructor = true)]
    internal class ServiceRequestMessage : IMessageServiceRequestData, IOperationServiceRequestData
    {
        internal ServiceRequestMessage()
        {
            //TODO: this is here due to restrictions in json.net library FIX
        }

        private ServiceRequestMessage(Guid operationIdentifier, TransportMessage message)
        {
            if (operationIdentifier == Guid.Empty) throw new ArgumentNullException("operationIdentifier");
            if (message == null) throw new ArgumentNullException("message");
            Operation = operationIdentifier;
            Data = message;
        }

        private ServiceRequestMessage(Guid serverId, Guid operationIdentifier, string resultType, Guid callingContextId,
                                      IDictionary<string, RequestParameter> serviceParameters)
        {
            if (string.IsNullOrEmpty(resultType)) throw new ArgumentNullException("resultType");
            if (operationIdentifier == Guid.Empty) throw new ArgumentNullException("operationIdentifier");
            ServerId = serverId;
            Operation = operationIdentifier;
            ResultType = resultType;
            CallingContextId = callingContextId;
            Parameters = serviceParameters;
        }

        #region IMessageServiceRequestData Members
        
        [ProtoMember(1)]
        public Guid Operation { get; set; }
        
        [ProtoMember(7)]
        public TransportMessage Data { get; set; }

        #endregion

        #region IOperationServiceRequestData Members
        [ProtoMember(3)]
        public Guid ServerId { get; set; }

        [ProtoMember(4)]
        public Guid CallingContextId { get; set; }
        //TODO: ENSURE DATA HAS PUBLIC GETTER SETTERS OR DOCUMENT CONSTRAINT

        [ProtoMember(5)]
        public IDictionary<string, RequestParameter> Parameters { get; set; }

        [ProtoMember(6)]
        public string ResultType { get; set; }

        #endregion

        public static ServiceRequestMessage GetForMessagePublishing(TransportMessage message)
        {
            return new ServiceRequestMessage(OperationIdentifiers.InternalMessagesOperationIdentifier, message);
        }

        public static ServiceRequestMessage GetForServiceRequest<TResult>(Guid serverId, Guid operationIdentifier,
                                                                          Guid callingContextId)
        {
            return GetForServiceRequest<TResult>(serverId, operationIdentifier, callingContextId, null);
        }

        public static ServiceRequestMessage GetForServiceRequest<TResult>(Guid serverId, Guid operationIdentifier,
                                                                          Guid callingContextId,
                                                                          IDictionary<string, object> serviceParameters)
        {
            IDictionary<string, RequestParameter> pars = null;
            if (serviceParameters != null)
            {
                pars = new Dictionary<string, RequestParameter>(serviceParameters.Count);
                foreach (var serviceParameter in serviceParameters)
                {
                    pars.Add(serviceParameter.Key, new RequestParameter(serviceParameter.Key, serviceParameter.Value));
                }
            }
            return new ServiceRequestMessage(serverId, operationIdentifier, typeof (TResult).FullName, callingContextId,
                                             pars);
        }

        #region Nested type: RequestParameter

        [ProtoContract(SkipConstructor = true)]
        public class RequestParameter
        {
            private string _jsonParameterValue;
            private object _parameterValue;
            public RequestParameter(string parameterName, object value)
            {
                ParameterName = parameterName;
                if (value != null)
                {
                    PTypeName = value.GetType().FullName ;
                    ParameterValue = value;
                }
            }
            
            [ProtoMember(1)]
            public string PTypeName { get; set; }
            
            [ProtoMember(2)]
            public string ParameterName { get; set; }

            [ProtoMember(3)]
            public string JsonParameterValue
            {
                get
                {
                    if (string.IsNullOrEmpty(_jsonParameterValue))
                    {
                        if (_parameterValue == null)
                            throw new ApplicationException(
                                "One of both, the parameter value or the serialized json message must have a value");
                        _jsonParameterValue= JsonSerializer.SerializeObjectToJson(_parameterValue);
                    }
                    
                    return _jsonParameterValue;
                }
                private set { _jsonParameterValue = value; }
            }

            public object ParameterValue
            {
                get
                {

                    if (_parameterValue == null)
                    {
                        if (string.IsNullOrEmpty(_jsonParameterValue))
                        
                            throw new ApplicationException(
                                "One of both, the parameter value or the serialized json message must have a value");
                            _parameterValue= JsonSerializer.DeserializeObjectFromJson<object>(_jsonParameterValue);
                        
                    }
                    return _parameterValue;
                }
                set { _parameterValue = value; }
            }
            
        }

        #endregion
    }

    //TODO: force usage of the interfaces, is not done yet
}