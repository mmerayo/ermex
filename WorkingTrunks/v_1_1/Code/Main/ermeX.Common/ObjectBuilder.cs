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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Castle.DynamicProxy;
using ermeX.Common.ILGeneration;

namespace ermeX.Common
{
    public static class ObjectBuilder
    {
        public static TResult FromTypeName<TResult>(string fullName)
        {
            var type = TypesHelper.GetTypeFromDomain(fullName,true,false);
            return FromType<TResult>(type);
        }

        public static TResult FromType<TResult>(Type typeToBuild, params object[] args)
        {
            return ILHelper.CreateInstance<TResult>(typeToBuild, args);
        }

        public static TInterfaceResult CreateProxy<TInterfaceResult>(IInterceptor implementation)
        {
            if (implementation == null) throw new ArgumentNullException("implementation");
            var generator = new ProxyGenerator();
            var result = (TInterfaceResult) generator.CreateInterfaceProxyWithoutTarget(typeof (TInterfaceResult), implementation);

            if(result==null)
                throw new ApplicationException("Could not create interface of type " + typeof(TInterfaceResult).FullName);

            return result;
        }

        public static Type CreateProxyType<TInterfaceResult>(IInterceptor implementation)
        {
            if (implementation == null) throw new ArgumentNullException("implementation");
            var generator = new ProxyGenerator();
            return generator.CreateInterfaceProxyWithTarget(typeof (TInterfaceResult), implementation);
        }
    }
}