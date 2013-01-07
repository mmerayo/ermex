// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System.Collections.Generic;
using ermeX.Transport.Interfaces;
using ermeX.Transport.Interfaces.Messages;

namespace ermeX.Bus.Listening.Handlers
{
    internal abstract class ServiceHandlerBase : IServiceHandler
    {
        #region IServiceHandler Members

        public object Handle(object message)
        {
            //TODO: DUE TO ServiceStack library issue
            var requestParameters = ((ServiceRequestMessage) message).Parameters;
            return Handle(requestParameters);
        }

        //private static void FixDueToIssueServiceStack(IDictionary<string, ServiceRequestMessage.RequestParameter> requestParameters)
        //{
        //    foreach (var key in requestParameters.Keys)
        //    {
        //        if (requestParameters[key].ParameterValue != null)
        //        {
        //            string toDeserialize = (string) requestParameters[key].ParameterValue;

        //            toDeserialize = toDeserialize.Substring(1, toDeserialize.Length - 2);//remove '[' and ']'

        //            if (toDeserialize.Contains("__type"))
        //            {
        //                var strings = toDeserialize.Split(',');
        //                var split = strings[0].Split('"');
        //                string typeName = split[3];
        //                strings[0] = split[0];

        //                var aux = new List<string>(strings);
        //                aux.RemoveAt(1);
        //                var finalString = string.Join(",", aux).Remove(1,1);

        //                requestParameters[key].ParameterValue =
        //                    ObjectSerializer.DeserializeObjectFromJson(finalString, typeName);
        //            }
        //            else
        //            {
        //                requestParameters[key].ParameterValue = toDeserialize;
        //            }
        //        }
        //    }
        //}

        public abstract void Dispose();

        #endregion

        public abstract object Handle(IDictionary<string, ServiceRequestMessage.RequestParameter> message);
    }
}