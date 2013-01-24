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
using System.Text;
using CommonContracts.Messages;
using StockBoyPanel.DataSources;
using ermeX;

namespace StockBoyPanel.MessageHandlers
{
    /// <summary>
    /// This class is an ermeX message handler
    /// </summary>
    public class MachineStatusHandler:IHandleMessages<MachineStatus>
    {
        /// <summary>
        /// This method is invoked everytime the component receives a message of type MachineStatus or inheritors
        /// and updates the MachinesDataSource collection
        /// </summary>
        /// <param name="message"></param>
        public void HandleMessage(MachineStatus message)
        {
            try
            {
                MachinesDataSource.Default.Save(message);
            }
            catch (Exception ex)
            {
                //log exception, rethrow if you want to retry the message handling as it remains on top of the delivery queue

                throw;
            }
        }
    }
}
