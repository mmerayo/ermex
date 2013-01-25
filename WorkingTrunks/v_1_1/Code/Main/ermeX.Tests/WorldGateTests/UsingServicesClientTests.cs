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
using ermeX.Common;
using ermeX.ConfigurationManagement;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.ConfigurationManagement.Settings.Data.Schemas;
using ermeX.Exceptions;
using ermeX.Tests.Common.DataAccess;
using ermeX.Tests.Common.Dummies;
using ermeX.Tests.Common.RandomValues;
using ermeX.Tests.Common.SettingsProviders;
using ermeX.Tests.WorldGateTests.Mock;

namespace ermeX.Tests.WorldGateTests
{
    [Category(TestCategories.CoreFunctionalTest)]
    [TestFixture]
    internal class UsingServicesClientTests : DataAccessTestBase
    {
        #region Setup/Teardown

        public override void OnStartUp()
        {
            CreateDatabase = false;
            base.OnStartUp();
        }

        public override void OnTearDown()
        {
            WorldGate.Reset();
            TestService.Reset();

            base.OnTearDown();
        }

        public override void OnFixtureTearDown()
        {
            base.OnFixtureTearDown();
            TestSettingsProvider.DropDatabases();//TODO: EXTRANGE BEHAVIOR TO REMOVE
        }

        #endregion


        [Test,TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void Can_GetServiceProxy( DbEngineType dbEngine)
        {
            var cfg = TestSettingsProvider.GetServiceLayerSettingsSource(LocalComponentId, dbEngine);
            WorldGate.ConfigureAndStart(cfg);

            WorldGate.RegisterService<ITestService>(typeof (TestService));

            var actual = WorldGate.GetServiceProxy<ITestService>();
            Assert.IsNotNull(actual);
        }

        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void Can_InvokeProxyEmptyMethod(DbEngineType dbEngine)
        {
            var cfg = TestSettingsProvider.GetServiceLayerSettingsSource(LocalComponentId, dbEngine);
            WorldGate.ConfigureAndStart(cfg);

            WorldGate.RegisterService<ITestService>(typeof (TestService));

            var actual = WorldGate.GetServiceProxy<ITestService>();
            try
            {
                actual.EmptyMethod();
            }catch(ermeXServiceRequestReturnedErrors ex)
            {
                Assert.Fail(ex.ToString());
            }
            catch
            {
                throw;
            }
            TestService.Refresh();
            Assert.IsTrue(TestService.Tracker.EmptyMethodCalled == 1);
            Assert.IsTrue(TestService.Tracker.ParametersLastCall.Count == 0);
        }

        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void Can_InvokeProxyReturnMethod(DbEngineType dbEngine)
        {
            var cfg = TestSettingsProvider.GetServiceLayerSettingsSource(LocalComponentId, dbEngine);
            WorldGate.ConfigureAndStart(cfg);

            WorldGate.RegisterService<ITestService>(typeof (TestService));

            var target = WorldGate.GetServiceProxy<ITestService>();

            DummyDomainEntity actual = target.ReturnMethod();
            Assert.IsNotNull(actual);
            TestService.Refresh();
            Assert.IsTrue(TestService.Tracker.ReturnMethodCalled == 1);
            Assert.IsTrue(TestService.Tracker.ParametersLastCall.Count == 0);
        }
        
        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void Can_InvokeProxy_OneParameterMethod(DbEngineType dbEngine)
        {
            var cfg = TestSettingsProvider.GetServiceLayerSettingsSource(LocalComponentId, dbEngine);
            WorldGate.ConfigureAndStart(cfg);

            WorldGate.RegisterService<ITestService>(typeof (TestService));

            var target = WorldGate.GetServiceProxy<ITestService>();

            var dummyDomainEntity = new DummyDomainEntity {Id = Guid.NewGuid()};
            target.EmptyMethodWithOneParameter(dummyDomainEntity);
            TestService.Refresh();
            Assert.IsTrue(TestService.Tracker.EmptyMethodWithOneParameterCalled == 1);
            Assert.IsTrue(TestService.Tracker.ParametersLastCall.Count == 1);
            Assert.IsTrue(((DummyDomainEntity)TestService.Tracker.ParametersLastCall[0]).Id == dummyDomainEntity.Id);
        }

        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void Can_InvokeProxy_ReturnMethodOneParam(DbEngineType dbEngine)
        {
            var cfg = TestSettingsProvider.GetServiceLayerSettingsSource(LocalComponentId, dbEngine);
            WorldGate.ConfigureAndStart(cfg);

            WorldGate.RegisterService<ITestService>(typeof (TestService));

            var target = WorldGate.GetServiceProxy<ITestService>();

            var dummyDomainEntity = new DummyDomainEntity {Id = Guid.NewGuid()};
            DummyDomainEntity actual = target.ReturnMethodWithOneParameter(dummyDomainEntity);

            Assert.IsNotNull(actual);
            TestService.Refresh();
            Assert.IsTrue(TestService.Tracker.ReturnMethodWithOneParameterCalled == 1);
            Assert.IsTrue(TestService.Tracker.ParametersLastCall.Count == 1);
            Assert.IsTrue(((DummyDomainEntity)TestService.Tracker.ParametersLastCall[0]).Id == dummyDomainEntity.Id);
        }

        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void Can_InvokeProxy_SeveralParametersMethod(DbEngineType dbEngine)
        {
            var cfg = TestSettingsProvider.GetServiceLayerSettingsSource(LocalComponentId, dbEngine);
            WorldGate.ConfigureAndStart(cfg);

            WorldGate.RegisterService<ITestService>(typeof (TestService));

            var target = WorldGate.GetServiceProxy<ITestService>();

            var dummyDomainEntity = new DummyDomainEntity {Id = Guid.NewGuid()};
            var domainEntity = new DummyDomainEntity {Id = Guid.NewGuid()};
            target.EmptyMethodWithSeveralParameters(dummyDomainEntity, domainEntity);
            TestService.Refresh();
            Assert.IsTrue(TestService.Tracker.EmptyMethodWithSeveralParametersCalled == 1);
            Assert.IsTrue(TestService.Tracker.ParametersLastCall.Count == 2);
            Assert.IsTrue(((DummyDomainEntity)TestService.Tracker.ParametersLastCall[0]).Id == dummyDomainEntity.Id);
            Assert.IsTrue(((DummyDomainEntity)TestService.Tracker.ParametersLastCall[1]).Id == domainEntity.Id);

        }

        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void Can_InvokeProxy_Several_ValueTypes_ParametersMethod(DbEngineType dbEngine)
        {
            var cfg = TestSettingsProvider.GetServiceLayerSettingsSource(LocalComponentId, dbEngine);
            WorldGate.ConfigureAndStart(cfg);

            WorldGate.RegisterService<ITestService3>(typeof(TestService));

            var target = WorldGate.GetServiceProxy<ITestService3>();

            DateTime theDateTime=RandomHelper.GetRandomDateTime();
            Guid theGuid=Guid.NewGuid();
            int theInt32 = 3;
            short theInt16 = 222;
            ushort theUshort = 352;
            long theInt64 = 123;
            ulong theUlong = 9999;
            uint theUint = 654654;
            float theFloat = (float)55.222;
            double theDouble = 654654.3232;
            decimal theDecimal = new decimal(999999999999999999.99);
            var actual= target.ReturnMethodWithSeveralParametersValueTypes(theGuid, theDateTime,theInt32,
                theUint,theInt16,theUshort,theInt64,theUlong,theFloat,theDouble,theDecimal);
            Assert.IsFalse(actual.IsEmpty());

            TestService.Refresh();
            Assert.IsTrue(TestService.Tracker.ReturnMethodWithSeveralParametersValueTypesCalled == 1);
            Assert.IsTrue(TestService.Tracker.ParametersLastCall.Count == 11);
            Assert.IsTrue(Guid.Parse(TestService.Tracker.ParametersLastCall[0].ToString()) == theGuid);//TODO: REMOVE WHEN CHANGED SERIALIZER
            Assert.IsTrue((DateTime)TestService.Tracker.ParametersLastCall[1]== theDateTime);
            Assert.IsTrue(Convert.ToInt32(TestService.Tracker.ParametersLastCall[2]) == theInt32);
            Assert.IsTrue(Convert.ToUInt32(TestService.Tracker.ParametersLastCall[3]) == theUint);
            Assert.IsTrue(Convert.ToInt16(TestService.Tracker.ParametersLastCall[4]) == theInt16);
            Assert.IsTrue(Convert.ToUInt16(TestService.Tracker.ParametersLastCall[5]) == theUshort);
            Assert.IsTrue(Convert.ToInt64(TestService.Tracker.ParametersLastCall[6]) == theInt64);
            Assert.IsTrue(Convert.ToUInt64(TestService.Tracker.ParametersLastCall[7]) == theUlong);
            Assert.IsTrue(Convert.ToSingle(TestService.Tracker.ParametersLastCall[8]) == theFloat);
            Assert.IsTrue(Convert.ToDouble(TestService.Tracker.ParametersLastCall[9]) == theDouble);
            Assert.IsTrue(Convert.ToDecimal(TestService.Tracker.ParametersLastCall[10]) == theDecimal);
        }



        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void Can_InvokeProxy_CustomValueType_ParameterMethod(DbEngineType dbEngine)
        {
            var cfg = TestSettingsProvider.GetServiceLayerSettingsSource(LocalComponentId, dbEngine);
            WorldGate.ConfigureAndStart(cfg);

            WorldGate.RegisterService<ITestService3>(typeof(TestService));

            var target = WorldGate.GetServiceProxy<ITestService3>();


            MyCustomStruct theStruct=new MyCustomStruct()
                                         {
                                             DateTime=DateTime.UtcNow,
                                             Guid=Guid.NewGuid(),
                                             TheDecimal = new decimal(2.6633355),
                                             TheLong = 99999999999,
                                             TheString = "this is the test data",
                                             TimeSpan = TimeSpan.FromTicks(55555555)
                                         };
            var actual = target.ReturnCustomStructMethod(theStruct);
            Assert.IsNotNull(actual);

            TestService.Refresh();
            Assert.IsTrue(TestService.Tracker.ReturnMethodWithCustomStructParametersCalled == 1);
            Assert.IsTrue(TestService.Tracker.ParametersLastCall.Count == 1);
            var parameter = (MyCustomStruct)TestService.Tracker.ParametersLastCall[0];
            Assert.IsTrue(theStruct.AreEqual(parameter));

        }

        [Test, TestCaseSource(typeof(TestCaseSources), "InMemoryDb")]
        public void Can_InvokeProxy_EnumType_ParameterMethod(DbEngineType dbEngine)
        {
            var cfg = TestSettingsProvider.GetServiceLayerSettingsSource(LocalComponentId, dbEngine);
            WorldGate.ConfigureAndStart(cfg);

            WorldGate.RegisterService<ITestService3>(typeof(TestService));

            var target = WorldGate.GetServiceProxy<ITestService3>();


            EnumerationType enumeration=EnumerationType.Value1;
            var actual = target.ReturnEnumMethod(enumeration);
            Assert.IsNotNull(actual);

            TestService.Refresh();
            Assert.IsTrue(TestService.Tracker.ReturnMethodWithEnumParametersCalled== 1);
            Assert.IsTrue(TestService.Tracker.ParametersLastCall.Count == 1);
            var parameter = (EnumerationType)Enum.Parse(typeof(EnumerationType), TestService.Tracker.ParametersLastCall[0].ToString());
            Assert.AreEqual(enumeration, parameter);
            Assert.AreEqual(EnumerationType.Value2, actual);

        }
    }
}