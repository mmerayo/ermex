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
using Ninject.Modules;
using ermeX.Bus.Interfaces;
using ermeX.Bus.Synchronisation.Dialogs.Anarquik.IoCLoader;
using ermeX.ConfigurationManagement.Settings;
using ermeX.ConfigurationManagement.Settings.Component;

namespace ermeX.Bus.Synchronisation.DependencyInjectionModules
{
    internal class DialogInjections : NinjectModule
    {
        private readonly IBusSettings _settings;

        public DialogInjections(IBusSettings settings)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            _settings = settings;
        }

        public override void Load()
        {

            //TODO: DIALOGS FOR ANARQUIK OR GOVERNED

            switch (_settings.NetworkingMode)
            {
                case NetworkingMode.Anarquik:
                    IoCLoader.PerformInjections(this);
                    break;
                case NetworkingMode.Governed:
                    throw new NotSupportedException(
                        "The Governed networking mode is not supported yet. Keep an eye on the updates");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}