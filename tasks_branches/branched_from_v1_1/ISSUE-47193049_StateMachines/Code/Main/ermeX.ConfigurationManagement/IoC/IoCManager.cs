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
using Ninject;
using Ninject.Modules;

namespace ermeX.ConfigurationManagement.IoC
{
    /// <summary>
    ///   IoCMAnager
    /// </summary>
    internal static class IoCManager
    {
        private static readonly object syncRoot = new object();
        public static IKernel Kernel { get; private set; }//TODO: dont expose this and expose methods where needed 
        public static event EventHandler IoCKernelChanged;

        public static void SetCurrentInjections(INinjectModule[] injectionModules)
            //TODO:There should be a discovering process for this definitions in case is extended
        {
            INinjectSettings settings = new NinjectSettings {InjectNonPublic = true};
            if (injectionModules != null)
            {
                SetKernel(new StandardKernel(settings, injectionModules));
            }
        }

        public static void Reset()
        {
            SetKernel(null);
        }

        private static void SetKernel(IKernel newKernel)
        {            
            if (Kernel != null)
            {
                lock (syncRoot)
                    if (Kernel != null)
                    {
                        Kernel.Dispose();
                        Kernel = null;
                    }
            }
            if (newKernel != null)
            {
                lock (syncRoot)
                    if (Kernel == null)
                    {
                        Kernel = newKernel;
                    }
                OnKernelChanged();
            }
        }

        private static void OnKernelChanged()
        {
            if (IoCKernelChanged != null)
                IoCKernelChanged(null, null);
        }

        public static void SetCurrentInjections(IKernel newKernel)
        {
            SetKernel(newKernel);
        }

        public static void InjectObject<TInterface>(TInterface implementor)
        {
            Kernel.Bind<TInterface>().ToConstant(implementor);
        }


        public static void InjectType<TInterface>(Type implementor)
        {
            Kernel.Bind<TInterface>().To(implementor);
        }
    }
}