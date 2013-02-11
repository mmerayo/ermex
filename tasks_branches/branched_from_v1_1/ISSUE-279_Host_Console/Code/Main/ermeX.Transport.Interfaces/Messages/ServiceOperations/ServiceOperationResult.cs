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
using ermeX.Transport.Interfaces.ServiceOperations;

namespace ermeX.Transport.Interfaces.Messages.ServiceOperations
{
    public class ServiceOperationResult<TDomain> : IServiceOperationResult<TDomain>
    {
        internal ServiceOperationResult(ServiceResult partialResult)
        {
            OperationResult = partialResult.Ok ? OperationResultType.Success : OperationResultType.Failed;
            ResultValue = (TDomain) partialResult.ResultData;
            InvocationMethod = OperationInvocationMethodType.Synchronous; //TODO:Asynchronous
            if (partialResult.ServerMessages!=null && partialResult.ServerMessages.Count > 0)
                InnerException =
                    new ApplicationException(string.Format("Exception message: {0}{1}", Environment.NewLine,
                                                           string.Join(Environment.NewLine, partialResult.ServerMessages)));
        }

        #region IServiceOperationResult<TDomain> Members

        public OperationResultType OperationResult { get; private set; }

        public TDomain ResultValue { get;  set; }


        public OperationInvocationMethodType InvocationMethod { get; private set; }

        public Exception InnerException { get; private set; }

        #endregion
    }
}