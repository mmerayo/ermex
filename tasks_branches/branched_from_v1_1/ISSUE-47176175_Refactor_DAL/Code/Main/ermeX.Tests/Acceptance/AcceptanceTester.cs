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
using NUnit.Framework;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.Models.Entities;
using ermeX.Tests.Acceptance.Dummy;
using ermeX.Tests.Common.DataAccess;
using ermeX.Tests.Common.SettingsProviders;


namespace ermeX.Tests.Acceptance
{
    //[TestFixture]
    internal abstract class AcceptanceTester:DataAccessTestBase
    {
        #region Setup/Teardown

        public override void OnStartUp()
        {
            CreateDatabase = false;
            base.OnStartUp();
        }

        public override void OnTearDown()
        {
           TestComponent.DisposeDomains();

           base.OnTearDown();            
        }

        public override void OnFixtureTearDown()
        {
            base.OnFixtureTearDown();
            TestSettingsProvider.DropDatabases();//TODO: REMOVE WHEN FIXED
        }

        #endregion

       

        protected void InitializeLonelyComponent(DbEngineType engineType, ushort listeningPort, TestComponent component, string dbConnString)
        {
            component.Start(engineType,dbConnString, listeningPort);
        }

        protected void InitializeConnectedComponent(DbEngineType engineType, ushort friendListeningPort, ushort localListeningPort, TestComponent friendComponent, string dbConnString, TestComponent component)
        {
            component.Start(engineType,dbConnString, localListeningPort, friendComponent.ComponentId, friendListeningPort);
        }

       
    }
}