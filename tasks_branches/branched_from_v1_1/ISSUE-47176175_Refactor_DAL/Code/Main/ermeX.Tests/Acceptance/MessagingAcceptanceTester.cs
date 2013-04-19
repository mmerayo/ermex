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
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using ermeX.Common;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.Entities.Entities;
using ermeX.Tests.Acceptance.Dummy;
using ermeX.Tests.Common.Networking;
using ermeX.Tests.Common.SettingsProviders;


namespace ermeX.Tests.Acceptance
{
    //TODO: Create high reusable acceptance tests funcionallity

    //TODO: STRUCTURE THESE TESTS USING MESSAGE SEND/LISTEN, MESSAGE SUBSCRIPTIONS, SERVICES PUBLISH, SERVICES REQUESTS

    //These are the acceptance tests for message sending

    internal sealed class MessagingAcceptanceTester : AcceptanceTester
    {
        
        [Test,TestCaseSource(typeof(TestCaseSources), TestCaseSources.DbInMemory)]
        public void SendMessageAndReception(DbEngineType engineType)
        {
            //arrange
             var senderListeningPort = new TestPort(9000);
             var receiverListeningPort = new TestPort(9101);

            string dbConnString = TestSettingsProvider.GetConnString(engineType);

            var expected = new AcceptanceMessageType1(true);
            var messagesToSend = new List<object> { expected };


            using (var sender = TestComponent.GetComponent())
            {
                sender.Start(engineType,dbConnString, senderListeningPort);
                using (var receiver = TestComponent.GetComponent())
                {
                    //arrange subscription handling
                    AutoResetEvent finishedEvent;
                    var handler = InitializeConnectedComponentAndHandler(engineType, senderListeningPort, receiverListeningPort,
                                                                        sender, dbConnString, receiver, 1,
                                                                        out finishedEvent);

                    //act
                    foreach (var o in messagesToSend)
                    {
                        sender.Publish(o);
                    }

                    finishedEvent.WaitOne(AppComponent.DefaultLatencyMilliseconds *2);

                    Assert.IsTrue(handler.ReceivedMessages.Count == 1);

                    var actual = handler.ReceivedMessages[0];
                    Assert.IsNotNull(actual);
                    Assert.AreEqual(expected, actual);

                }
            }
        }

        [Test,TestCaseSource(typeof(TestCaseSources), TestCaseSources.DbInMemory)]
        public void SeveralReceiversSendMessageAndReception(DbEngineType engineType)
        {

            //arrange
            ushort senderListeningPort = new TestPort(9000);
            ushort receiver1ListeningPort = new TestPort(9000);
            ushort receiver2ListeningPort = new TestPort(9000);
            ushort receiver3ListeningPort = new TestPort(9000);
            ushort receiver4ListeningPort = new TestPort(9000);

            string dbConnString = TestSettingsProvider.GetConnString(engineType);

            var expected1 = new AcceptanceMessageType1(true);
            var expected2 = new AcceptanceMessageType2(true);
            var expected3 = new AcceptanceMessageType2(true);
            var messagesToSend = new List<object> { expected1, expected2, expected3 };


            using (var sender = TestComponent.GetComponent())
            {
                sender.Start(engineType, dbConnString, senderListeningPort);
                using (var receiver1 = TestComponent.GetComponent())
                using (var receiver2 = TestComponent.GetComponent())
                using (var receiver3 = TestComponent.GetComponent())
                using (var receiver4 = TestComponent.GetComponent())
                {
                    //arrange subscription handling
                    AutoResetEvent finishedEvent1;
                    var handler1 = InitializeConnectedComponentAndHandler(engineType, senderListeningPort, receiver1ListeningPort,
                                                                         sender, dbConnString, receiver1, 1,
                                                                         out finishedEvent1);

                    AutoResetEvent finishedEvent2;
                    var handler2 = InitializeConnectedComponentAndHandler(engineType, senderListeningPort, receiver2ListeningPort,
                                                                         sender, dbConnString, receiver2, 1,
                                                                         out finishedEvent2);

                    AutoResetEvent finishedEvent3;
                    var handler3 = InitializeConnectedComponentAndHandler(engineType, senderListeningPort, receiver3ListeningPort,
                                                                         sender, dbConnString, receiver3, 1,
                                                                         out finishedEvent3);

                    AutoResetEvent finishedEvent4;
                    var handler4 = InitializeConnectedComponentAndHandler2(engineType, senderListeningPort, receiver4ListeningPort,
                                                                         sender, dbConnString, receiver4, 2,
                                                                         out finishedEvent4);

                    AutoResetEvent finishedEvent5 = new AutoResetEvent(false);
                    var handler5 = sender.Suscribe<DummyMessagesHandler3>(typeof(DummyMessagesHandler3));
                    handler5.NotifyWhenReceive(1, finishedEvent5);

                    //act
                    foreach (var o in messagesToSend)
                    {
                        sender.Publish(o);
                    }

                    var acceptanceMessageType3 = new AcceptanceMessageType3();
                    receiver4.Publish(acceptanceMessageType3);

                    WaitHandle.WaitAll(new[] { finishedEvent1, finishedEvent2, finishedEvent3, finishedEvent4, finishedEvent5 },
                                       TimeSpan.FromSeconds(30));


                    //assert
                    Assert.IsTrue(handler1.ReceivedMessages.Count == 1);
                    Assert.IsTrue(handler2.ReceivedMessages.Count == 1);
                    Assert.IsTrue(handler3.ReceivedMessages.Count == 1);
                    Assert.IsTrue(handler4.ReceivedMessages.Count == 2);
                    Assert.IsTrue(handler5.ReceivedMessages.Count == 1);


                    var actual1 = handler1.ReceivedMessages[0];
                    Assert.IsNotNull(actual1);
                    Assert.AreEqual(expected1, actual1);

                    var actual2 = handler2.ReceivedMessages[0];
                    Assert.IsNotNull(actual2);
                    Assert.AreEqual(expected1, actual2);

                    var actual3 = handler3.ReceivedMessages[0];
                    Assert.IsNotNull(actual3);
                    Assert.AreEqual(expected1, actual3);

                    Assert.IsNotNull(handler4.ReceivedMessages[0]);
                    Assert.AreEqual(expected2, handler4.ReceivedMessages[0]);
                    Assert.IsNotNull(handler4.ReceivedMessages[1]);
                    Assert.AreEqual(expected3, handler4.ReceivedMessages[1]);

                    Assert.IsNotNull(handler5.ReceivedMessages[0]);
                    Assert.AreEqual(acceptanceMessageType3, handler5.ReceivedMessages[0]);


                }
            }
        }



        [Ignore("TODO: FIX OR DEVELOP")]
        [Test,TestCaseSource(typeof(TestCaseSources), TestCaseSources.DbInMemory)]
        public void CanSubscribeBeforeStartUp(DbEngineType engineType)
        {
            throw new NotImplementedException();
        }


        [Test,TestCaseSource(typeof(TestCaseSources), TestCaseSources.DbInMemory)]
        public void SuscriptorCanReceiveMessagesAfterBeingOffline(DbEngineType engineType)
        {
            //arrange
            var senderListeningPort = new TestPort(9000);
            var receiverListeningPort = new TestPort(9000); 

            string dbConnString = TestSettingsProvider.GetConnString(engineType);

            var expected = new AcceptanceMessageType1(true);
            var messagesToSend = new List<object> { expected };


            using (var sender = TestComponent.GetComponent())
            {
                sender.Start(engineType, dbConnString, senderListeningPort);
                using (var receiver = TestComponent.GetComponent())
                {
                    //arrange subscription handling
                    AutoResetEvent finishedEvent;
                    var handler1 = InitializeConnectedComponentAndHandler(engineType, senderListeningPort, receiverListeningPort,
                                                                        sender, dbConnString, receiver, 1,
                                                                        out finishedEvent);

                    receiver.Reset();
                    //act
                    foreach (var o in messagesToSend)
                    {
                        sender.Publish(o);
                    }

                    var handler2 = InitializeConnectedComponentAndHandler(engineType, senderListeningPort, receiverListeningPort,
                                                                        sender, dbConnString, receiver, 1,
                                                                        out finishedEvent);

                    finishedEvent.WaitOne(TimeSpan.FromSeconds(50));

                    Assert.IsTrue(handler1.ReceivedMessages.Count == 0, string.Format("handler1: {0}", handler1.ReceivedMessages.Count.ToString()));
                    Assert.IsTrue(handler2.ReceivedMessages.Count == 1, string.Format("handler2: {0}", handler2.ReceivedMessages.Count.ToString()));

                    var actual = handler2.ReceivedMessages[0];
                    Assert.IsNotNull(actual);
                    Assert.AreEqual(expected, actual);

                }
            }
        }

        [Test,TestCaseSource(typeof(TestCaseSources), TestCaseSources.DbInMemory)]
        public void TwoComponentsCanReceiveMessagesAfterBeingOffLine(DbEngineType engineType)
        {
            //arrange
            ushort senderListeningPort = new TestPort(9000);
            ushort receiverListeningPort = new TestPort(9000); 

            string dbConnString = TestSettingsProvider.GetConnString(engineType);

            var expected = new AcceptanceMessageType1(true);
            var messagesToSend = new List<object> { expected };

            Guid receiverComponentId;
            Guid serverComponentId;

            using (var sender = TestComponent.GetComponent())
            {
                sender.Start(engineType, dbConnString, senderListeningPort);
                using (var receiver = TestComponent.GetComponent())
                {
                    //arrange subscription handling
                    AutoResetEvent finishedEvent;
                    var handler1 = InitializeConnectedComponentAndHandler(engineType, senderListeningPort, receiverListeningPort,
                                                                          sender, dbConnString, receiver, 1,
                                                                          out finishedEvent);
                    receiverComponentId = receiver.ComponentId;
                    receiver.Reset();
                }
                foreach (var o in messagesToSend)
                {
                    sender.Publish(o);
                }

                serverComponentId = sender.ComponentId;
            }
            using (var sender = TestComponent.GetComponent())
            {

                sender.Start(engineType, serverComponentId, dbConnString, senderListeningPort);
                using (var receiver = TestComponent.GetComponent())
                {
                    AutoResetEvent finishedEvent;
                    var handler2 = InitializeConnectedComponentAndHandler(engineType, receiverComponentId, senderListeningPort,
                                                                          receiverListeningPort,
                                                                          sender, dbConnString, receiver, 1,
                                                                          out finishedEvent);

                    finishedEvent.WaitOne(AppComponent.DefaultLatencyMilliseconds*2);

                    Assert.IsTrue(handler2.ReceivedMessages.Count == 1,
                                  string.Format("handler2: {0}", handler2.ReceivedMessages.Count.ToString()));

                    var actual = handler2.ReceivedMessages[0];
                    Assert.IsNotNull(actual);
                    Assert.AreEqual(expected, actual);

                }
            }
        }

        [Test,TestCaseSource(typeof(TestCaseSources), TestCaseSources.DbInMemory)]
        public void SuscriptorCanReceiveMessagesAfterBeing_Disconnected(DbEngineType engineType)
        {
            //arrange
            var senderListeningPort = new TestPort(9000);
            var receiverListeningPort = new TestPort(9000); 

            string dbConnString = TestSettingsProvider.GetConnString(engineType);

            var expected = new AcceptanceMessageType1(true);
            var messagesToSend = new List<object> { expected };


            using (var sender = TestComponent.GetComponent())
            {
                sender.Start(engineType, dbConnString, senderListeningPort);
                Guid componentId;
                using (var receiver = TestComponent.GetComponent())
                {
                    //arrange subscription handling
                    AutoResetEvent finishedEvent;
                    var handler1 = InitializeConnectedComponentAndHandler(engineType, senderListeningPort, receiverListeningPort,
                                                                          sender, dbConnString, receiver, 1,
                                                                          out finishedEvent);
                    componentId = receiver.ComponentId;
                    //act
                }
                foreach (var o in messagesToSend)
                {
                    sender.Publish(o);
                }


                using (var receiver = TestComponent.GetComponent())
                {
                    AutoResetEvent finishedEvent;
                    var handler2 = InitializeConnectedComponentAndHandler(engineType, componentId, senderListeningPort,
                                                                          receiverListeningPort,
                                                                          sender, dbConnString, receiver, 1,
                                                                          out finishedEvent);

                    finishedEvent.WaitOne(TimeSpan.FromSeconds(20));

                    Assert.IsTrue(handler2.ReceivedMessages.Count == 1,
                                  string.Format("handler2: {0}", handler2.ReceivedMessages.Count.ToString()));

                    var actual = handler2.ReceivedMessages[0];
                    Assert.IsNotNull(actual);
                    Assert.AreEqual(expected, actual);

                }
            }
        }

        [Test,TestCaseSource(typeof(TestCaseSources), TestCaseSources.DbInMemory)]
        public void SuscriptorCanReceiveMessagesAfterBeing_Disconnected_And_Add_Subscriptions(DbEngineType engineType)
        {
            //arrange
            ushort senderListeningPort = new TestPort(9000);
            ushort receiverListeningPort = new TestPort(9000);

            string dbConnString = TestSettingsProvider.GetConnString(engineType);

            var expected = new AcceptanceMessageType1(true);
            var messagesToSend = new List<object> { expected };


            using (var sender = TestComponent.GetComponent())
            {
                sender.Start(engineType, dbConnString, senderListeningPort);
                Guid componentId;
                using (var receiver = TestComponent.GetComponent())
                {
                    //arrange subscription handling
                    AutoResetEvent finishedEvent;
                    var handler1 = InitializeConnectedComponentAndHandler(engineType, senderListeningPort, receiverListeningPort,
                                                                          sender, dbConnString, receiver, 1,
                                                                          out finishedEvent);
                    componentId = receiver.ComponentId;
                    receiver.Reset();
                    //act
                }
                foreach (var o in messagesToSend)
                {
                    sender.Publish(o);
                }


                using (var receiver = TestComponent.GetComponent())
                {

                    AutoResetEvent finishedEvent2;
                    var handler2 = InitializeConnectedComponentAndHandler(engineType, componentId, senderListeningPort,
                                                                          receiverListeningPort,
                                                                          sender, dbConnString, receiver, 1,
                                                                          out finishedEvent2);

                    var handler1 = receiver.Suscribe<DummyMessagesHandler3>(typeof(DummyMessagesHandler3));
                    AutoResetEvent finishedEvent1 = new AutoResetEvent(false);
                    handler1.NotifyWhenReceive(1, finishedEvent1);

                    var handler3 = sender.Suscribe<DummyMessagesHandler2>(typeof(DummyMessagesHandler2));
                    AutoResetEvent finishedEvent3 = new AutoResetEvent(false);
                    handler3.NotifyWhenReceive(1, finishedEvent3);

                    var acceptanceMessageType2 = new AcceptanceMessageType2();
                    receiver.Publish(acceptanceMessageType2);

                    var acceptanceMessageType3 = new AcceptanceMessageType3();
                    sender.Publish(acceptanceMessageType3);

                    WaitHandle.WaitAll(new[] { finishedEvent1, finishedEvent2, finishedEvent3 });

                    finishedEvent2.WaitOne(TimeSpan.FromSeconds(30));

                    Assert.IsTrue(handler1.ReceivedMessages.Count == 1,
                                 string.Format("handler1: {0}", handler1.ReceivedMessages.Count.ToString()));

                    Assert.IsTrue(handler3.ReceivedMessages.Count == 1,
                                string.Format("handler3: {0}", handler3.ReceivedMessages.Count.ToString()));

                    Assert.IsTrue(handler2.ReceivedMessages.Count == 1,
                                 string.Format("handler2: {0}", handler2.ReceivedMessages.Count.ToString()));



                    Assert.IsNotNull(handler1.ReceivedMessages[0]);
                    Assert.AreEqual(acceptanceMessageType3, handler1.ReceivedMessages[0]);

                    Assert.IsNotNull(handler2.ReceivedMessages[0]);
                    Assert.AreEqual(expected, handler2.ReceivedMessages[0]);

                    Assert.IsNotNull(handler3.ReceivedMessages[0]);
                    Assert.AreEqual(acceptanceMessageType2, handler3.ReceivedMessages[0]);

                }
            }
        }


        [Test,TestCaseSource(typeof(TestCaseSources), TestCaseSources.DbInMemory)]
        public void TwoSuscriptions_Of_The_SameHandler_Are_HandledBy_TheSame_ObjectHandler(DbEngineType engineType)
        {
            //arrange
            ushort senderListeningPort = new TestPort(9000); ;
            ushort receiverListeningPort = new TestPort(9000); ;

            string dbConnString = TestSettingsProvider.GetConnString(engineType);

            var expected = new AcceptanceMessageType1(true);
            var messagesToSend = new List<object> { expected };


            using (var sender = TestComponent.GetComponent())
            {
                sender.Start(engineType, dbConnString, senderListeningPort);
                using (var receiver = TestComponent.GetComponent())
                {
                    //arrange subscription handling
                    AutoResetEvent finishedEvent;
                    var handler1 = InitializeConnectedComponentAndHandler(engineType, senderListeningPort, receiverListeningPort,
                                                                        sender, dbConnString, receiver, 1,
                                                                        out finishedEvent);
                    var handler2 = receiver.Suscribe<DummyMessagesHandler>(typeof(DummyMessagesHandler));
                    handler2.NotifyWhenReceive(1, finishedEvent);
                    //act
                    foreach (var o in messagesToSend)
                    {
                        sender.Publish(o);
                    }

                    finishedEvent.WaitOne(AppComponent.DefaultLatencyMilliseconds*2);

                    Assert.AreSame(handler1, handler2);
                    Assert.IsTrue(handler1.ReceivedMessages.Count == 1, string.Format("handler1: {0}", handler1.ReceivedMessages.Count.ToString()));
                    Assert.IsTrue(handler2.ReceivedMessages.Count == 1, string.Format("handler2: {0}", handler2.ReceivedMessages.Count.ToString()));

                    var actual = handler2.ReceivedMessages[0];
                    Assert.IsNotNull(actual);
                    Assert.AreEqual(expected, actual);

                }
            }
        }

        [Test,TestCaseSource(typeof(TestCaseSources), TestCaseSources.DbInMemory)]
        public void TwoComponentsCanExchangeMessages(DbEngineType engineType)
        {
            //arrange
            var senderListeningPort = new TestPort(9000);
            var receiverListeningPort = new TestPort(9000); 

            string dbConnString = TestSettingsProvider.GetConnString(engineType);

            var expectedReceiver = new AcceptanceMessageType1(true);
            var expectedSender = new AcceptanceMessageType2(true);


            using (var sender = TestComponent.GetComponent())
            {
                AutoResetEvent finishedEvent1;
                var handler1 = InitializeLonelyComponentAndHandler2(engineType,senderListeningPort, sender, dbConnString, 1, out finishedEvent1);

                using (var receiver = TestComponent.GetComponent())
                {
                    //arrange subscription handling
                    AutoResetEvent finishedEvent2;
                    var handler2 = InitializeConnectedComponentAndHandler(engineType, senderListeningPort, receiverListeningPort,
                                                                         sender, dbConnString, receiver, 1,
                                                                         out finishedEvent2);

                    AutoResetEvent finishedEvent3 = new AutoResetEvent(false);
                    var handler3 = sender.Suscribe<DummyMessagesHandler3>(typeof(DummyMessagesHandler3));
                    handler3.NotifyWhenReceive(1, finishedEvent3);

                    //act
                    sender.Publish(expectedReceiver);
                    receiver.Publish(expectedSender);
                    var acceptanceMessageType3 = new AcceptanceMessageType3();
                    receiver.Publish(acceptanceMessageType3);


                    WaitHandle.WaitAll(new[] { finishedEvent3, finishedEvent2, finishedEvent1 }, AppComponent.DefaultLatencyMilliseconds*3);

                    //the receiver's side
                    Assert.IsTrue(handler2.ReceivedMessages.Count == 1, "handler2.ReceivedMessages.Count:" + handler2.ReceivedMessages.Count);
                    var actualReceiver = handler2.ReceivedMessages[0];
                    Assert.IsNotNull(actualReceiver);
                    Assert.AreEqual(expectedReceiver, actualReceiver);

                    //the senders side
                    Assert.IsTrue(handler1.ReceivedMessages.Count == 1);
                    var actualSender = handler1.ReceivedMessages[0];
                    Assert.IsNotNull(actualSender);
                    Assert.AreEqual(expectedSender, actualSender);

                    Assert.IsTrue(handler3.ReceivedMessages.Count == 1);
                    var actualSender2 = handler3.ReceivedMessages[0];
                    Assert.IsNotNull(actualSender2);
                    Assert.AreEqual(acceptanceMessageType3, actualSender2);


                }
            }
        }

        [Test,TestCaseSource(typeof(TestCaseSources), TestCaseSources.DbInMemory)]
        public void OneComponentCanSubscribeToItsOwnMessages(DbEngineType engineType)
        {
            //arrange
            var listeningPort = new TestPort(9000);

            string dbConnString = TestSettingsProvider.GetConnString(engineType);

            var expectedReceiver = new AcceptanceMessageType1(true);

            using (var sender = TestComponent.GetComponent())
            {
                AutoResetEvent finishedEvent1;
                var handler1 = InitializeLonelyComponentAndHandler(engineType,listeningPort, sender, dbConnString, 1,
                                                                    out finishedEvent1);

                //act
                sender.Publish(expectedReceiver);

                WaitHandle.WaitAll(new[] {finishedEvent1}, AppComponent.DefaultLatencyMilliseconds*2);

                //the receiver's side
                Assert.IsTrue(handler1.ReceivedMessages.Count == 1,
                              string.Format("handler1.ReceivedMessages.Count:{0}", handler1.ReceivedMessages.Count));
                var actualReceiver = handler1.ReceivedMessages[0];
                Assert.IsNotNull(actualReceiver);
                Assert.AreEqual(expectedReceiver, actualReceiver);
            }
        }



        [Test,TestCaseSource(typeof(TestCaseSources), TestCaseSources.DbInMemory)]
        public void SeveralComponentsCanExchangeMessages(DbEngineType engineType) //the server subscribes to messages that ar sent from the recivers and each receiver gets one message from the server
        {
            //arrange
            ushort senderListeningPort = new TestPort(9000);
            ushort receiver1ListeningPort = new TestPort(9000);
            ushort receiver2ListeningPort = new TestPort(9000);

            string dbConnString = TestSettingsProvider.GetConnString(engineType);

            var expected1 = new AcceptanceMessageType1(true);
            var expected2 = new AcceptanceMessageType1(true);
            var expected3 = new AcceptanceMessageType3(true);
            var expected4 = new AcceptanceMessageType2(true);



            using (var sender = TestComponent.GetComponent())
            {
                AutoResetEvent serverFinished;
                var serverHandler = InitializeLonelyComponentAndHandler(engineType,senderListeningPort, sender, dbConnString, 2, out serverFinished);
                using (var receiver1 = TestComponent.GetComponent())
                using (var receiver2 = TestComponent.GetComponent())
                {
                    //arrange subscription handling
                    AutoResetEvent finishedEvent1;
                    var handler1 = InitializeConnectedComponentAndHandler2(engineType, senderListeningPort, receiver1ListeningPort,
                                                                         sender, dbConnString, receiver1, 1,
                                                                         out finishedEvent1);

                    AutoResetEvent finishedEvent2;
                    var handler2 = InitializeConnectedComponentAndHandler3(engineType, senderListeningPort, receiver2ListeningPort,
                                                                         sender, dbConnString, receiver2, 1,
                                                                         out finishedEvent2);


                    //act
                    receiver1.Publish(expected1);
                    receiver2.Publish(expected2);
                    sender.Publish(expected3);
                    sender.Publish(expected4);

                    WaitHandle.WaitAll(new[] { serverFinished, finishedEvent1, finishedEvent2 },
                                       AppComponent.DefaultLatencyMilliseconds * 4);


                    //assert
                    Assert.IsTrue(handler1.ReceivedMessages.Count == 1);
                    Assert.IsTrue(handler2.ReceivedMessages.Count == 1);
                    Assert.IsTrue(serverHandler.ReceivedMessages.Count == 2);


                    Assert.IsTrue(expected1 == serverHandler.ReceivedMessages[0] || expected1 == serverHandler.ReceivedMessages[1]);
                    Assert.IsTrue(expected2 == serverHandler.ReceivedMessages[0] || expected2 == serverHandler.ReceivedMessages[1]);

                    var actual1 = handler1.ReceivedMessages[0];
                    Assert.IsNotNull(actual1);
                    Assert.AreEqual(expected4, actual1);

                    var actual2 = handler2.ReceivedMessages[0];
                    Assert.IsNotNull(actual2);
                    Assert.AreEqual(expected3, actual2);


                }
            }
        }

        [Ignore("TODO: FIX OR DEVELOP")]
        [Test,TestCaseSource(typeof(TestCaseSources), TestCaseSources.DbInMemory)]
        public void Several_Components_SuscriptorsOnlyReceives_Its_Own_Suscriptions(DbEngineType engineType)
        {
            throw new NotImplementedException();


        }

        [Test,TestCaseSource(typeof(TestCaseSources), TestCaseSources.DbInMemory)]
        public void MessagesAreDeliveredFIFO(DbEngineType engineType)
        {
            const int numberOfMessages = 10;    //arrange
            var senderListeningPort = new TestPort(9000);
             var receiverListeningPort = new TestPort(9000); ; 

            string dbConnString = TestSettingsProvider.GetConnString(engineType);

            using (var sender = TestComponent.GetComponent())
            {
                sender.Start(engineType, dbConnString, senderListeningPort);
                using (var receiver = TestComponent.GetComponent())
                {
                    //arrange subscription handling
                    AutoResetEvent finishedEvent;
                    var handler = InitializeConnectedComponentAndHandler(engineType, senderListeningPort, receiverListeningPort,
                                                                        sender, dbConnString, receiver, numberOfMessages,
                                                                        out finishedEvent);

                    //act
                    for (int i = 0; i < numberOfMessages; i++)
                    {
                        Console.WriteLine("msg #:"+ i);
                        Console.Out.Flush();
                        sender.Publish(new AcceptanceMessageType1() { TheDateTime = DateTime.Now });
                        Thread.Sleep(1);
                    }

                    TimeSpan fromSeconds = TimeSpan.FromSeconds(AppComponent.DefaultLatencyMilliseconds/1000 + numberOfMessages*3);
                    finishedEvent.WaitOne(fromSeconds);

                    Assert.IsTrue(handler.ReceivedMessages.Count == numberOfMessages, string.Format("Received messages {0}", handler.ReceivedMessages.Count));

                    List<AcceptanceMessageType1> receivedMessages = handler.ReceivedMessages;
                    DateTime currDt = DateTime.MinValue;
                    foreach (var msg in receivedMessages)
                    {
                        Assert.IsTrue(msg.TheDateTime.Ticks >= currDt.Ticks);
                        currDt = msg.TheDateTime;
                    }
                }
            }
        }

        [Test,TestCaseSource(typeof(TestCaseSources), TestCaseSources.DbInMemory)]
        public void MessagesAreDeliveredFIFO_SeveralComponents(DbEngineType engineType)
        {
            const int numberOfMessages = 26;
            //arrange
            ushort senderListeningPort = new TestPort(9000);
            ushort receiverListeningPort = new TestPort(9000);
            ushort component2ListeningPort = new TestPort(9000);

            string dbConnString = TestSettingsProvider.GetConnString(engineType);



            using (var sender = TestComponent.GetComponent())
            {
                sender.Start(engineType, dbConnString, senderListeningPort);
                using (var receiver = TestComponent.GetComponent())
                using (var component2 = TestComponent.GetComponent())
                {
                    //arrange subscription handling
                    AutoResetEvent finishedEvent1;
                    var handler = InitializeConnectedComponentAndHandler(engineType, senderListeningPort, receiverListeningPort,
                                                                        sender, dbConnString, receiver, numberOfMessages,
                                                                        out finishedEvent1);
                    InitializeConnectedComponent(engineType, senderListeningPort, component2ListeningPort, sender, dbConnString,
                                                 component2);

                    //act
                    for (int i = 0; i < numberOfMessages / 2; i++)
                    {
                        Console.WriteLine("msg #:{0}", i);
                        Console.Out.Flush();
                        sender.Publish(new AcceptanceMessageType1() { TheDateTime = DateTime.Now });
                        Thread.Sleep(1);
                        component2.Publish(new AcceptanceMessageType1() { TheDateTime = DateTime.Now });
                        Thread.Sleep(1);

                    }

                    finishedEvent1.WaitOne(TimeSpan.FromSeconds(AppComponent.DefaultLatencyMilliseconds/1000 + numberOfMessages * 3));

                    Assert.IsTrue(handler.ReceivedMessages.Count == numberOfMessages, string.Format("Received messages {0}", handler.ReceivedMessages.Count));

                    List<AcceptanceMessageType1> receivedMessages = handler.ReceivedMessages;
                    DateTime currDt = DateTime.MinValue;
                    foreach (var msg in receivedMessages)
                    {
                        Assert.IsTrue(msg.TheDateTime.Ticks >= currDt.Ticks);
                        currDt = msg.TheDateTime;
                    }
                }
            }
        }


        [Ignore("TODO: FIX OR DEVELOP")]
        [Test,TestCaseSource(typeof(TestCaseSources), TestCaseSources.DbInMemory)]
        public void FiveComponentsCanReceiveMessagesAfterBeingOffLine(DbEngineType engineType)
        {
            throw new NotImplementedException();
        }

        [Test,TestCaseSource(typeof(TestCaseSources), TestCaseSources.DbInMemory)]
        public void Components_CanPublish_Any_Message(DbEngineType engineType)
        {
            string dbConnString = TestSettingsProvider.GetConnString(engineType);

            using (var component = TestComponent.GetComponent())
            {
                var listeningPort = new TestPort(9000);
                InitializeLonelyComponent(engineType, listeningPort, component, dbConnString);

                Assert.DoesNotThrow(() => { if (component != null) component.Publish(new Int32()); });
                Assert.DoesNotThrow(() => { if (component != null) component.Publish("this is a value"); });

                Assert.DoesNotThrow(() => { if (component != null) component.Publish(new AcceptanceMessageType1()); });

            }
        }


        private DummyMessagesHandler InitializeLonelyComponentAndHandler(DbEngineType engineType, ushort listeningPort,
                                                                          TestComponent component,
                                                                          string dbConnString, int numOfExpectedMessages,
                                                                          out AutoResetEvent finishedEvent)
        {
            finishedEvent = new AutoResetEvent(false);
            component.Start(engineType, dbConnString, listeningPort);
            var handler = component.Suscribe<DummyMessagesHandler>(typeof(DummyMessagesHandler));
            handler.NotifyWhenReceive(numOfExpectedMessages, finishedEvent);

            return handler;
        }

        private DummyMessagesHandler2 InitializeLonelyComponentAndHandler2(DbEngineType engineType, ushort listeningPort,
                                                                          TestComponent component,
                                                                          string dbConnString, int numOfExpectedMessages,
                                                                          out AutoResetEvent finishedEvent)
        {

            finishedEvent = new AutoResetEvent(false);
            component.Start(engineType,dbConnString, listeningPort);
            var handler = component.Suscribe<DummyMessagesHandler2>(typeof(DummyMessagesHandler2));
            handler.NotifyWhenReceive(numOfExpectedMessages, finishedEvent);

            return handler;
        }

        private DummyMessagesHandler InitializeConnectedComponentAndHandler(DbEngineType engineType, ushort senderListeningPort,
                                                                          ushort receiverListeningPort, TestComponent sender,
                                                                          string dbConnString, TestComponent receiver, int numOfExpectedMessages,
                                                                          out AutoResetEvent finishedEvent)
        {

            finishedEvent = new AutoResetEvent(false);
            receiver.Start(engineType,dbConnString, receiverListeningPort,
                            sender.ComponentId, senderListeningPort);
            var handler = receiver.Suscribe<DummyMessagesHandler>(typeof(DummyMessagesHandler));
            handler.NotifyWhenReceive(numOfExpectedMessages, finishedEvent);

            return handler;
        }




        private DummyMessagesHandler InitializeConnectedComponentAndHandler(DbEngineType engineType, Guid componentId, ushort senderListeningPort,
                                                                          ushort receiverListeningPort, TestComponent sender,
                                                                          string dbConnString, TestComponent receiver, int numOfExpectedMessages,
                                                                          out AutoResetEvent finishedEvent)
        {

            finishedEvent = new AutoResetEvent(false);
            receiver.Start(engineType,componentId, dbConnString, receiverListeningPort,
                            sender.ComponentId, senderListeningPort);
            var handler = receiver.Suscribe<DummyMessagesHandler>(typeof(DummyMessagesHandler));
            handler.NotifyWhenReceive(numOfExpectedMessages, finishedEvent);

            return handler;
        }

        private DummyMessagesHandler2 InitializeConnectedComponentAndHandler2(DbEngineType engineType, ushort senderListeningPort,
                                                                         ushort receiverListeningPort, TestComponent sender,
                                                                         string dbConnString, TestComponent receiver, int numOfExpectedMessages,
                                                                         out AutoResetEvent finishedEvent)
        {
            finishedEvent = new AutoResetEvent(false);
            receiver.Start(engineType,dbConnString, receiverListeningPort,
                            sender.ComponentId, senderListeningPort);
            var handler = receiver.Suscribe<DummyMessagesHandler2>(typeof(DummyMessagesHandler2));
            handler.NotifyWhenReceive(numOfExpectedMessages, finishedEvent);

            return handler;
        }
        private DummyMessagesHandler3 InitializeConnectedComponentAndHandler3(DbEngineType engineType, ushort senderListeningPort,
                                                                        ushort receiverListeningPort, TestComponent sender,
                                                                        string dbConnString, TestComponent receiver, int numOfExpectedMessages,
                                                                        out AutoResetEvent finishedEvent)
        {

            finishedEvent = new AutoResetEvent(false);
            receiver.Start(engineType,dbConnString, receiverListeningPort,
                            sender.ComponentId, senderListeningPort);
            var handler = receiver.Suscribe<DummyMessagesHandler3>(typeof(DummyMessagesHandler3));
            handler.NotifyWhenReceive(numOfExpectedMessages, finishedEvent);

            return handler;
        }
    }
}