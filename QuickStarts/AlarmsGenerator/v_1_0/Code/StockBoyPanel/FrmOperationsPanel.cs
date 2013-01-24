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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Common;
using ermeX;

namespace StockBoyPanel
{
    public partial class FrmOperationsPanel : Form
    {
        public LocalComponentInfo ComponentInfo { get; set; }

        public FrmOperationsPanel(LocalComponentInfo componentInfo)
        {
            if (componentInfo == null) throw new ArgumentNullException("componentInfo");
            ComponentInfo = componentInfo;
            InitializeComponent();
        }

        private void FrmOperationsPanel_Load(object sender, EventArgs e)
        {
            try
            {
                Text = string.Format("Stockman panel: {0}", ComponentInfo.FriendlyName);
            }
            catch (Exception ex)
            {
                OnError(ex.ToString());
            }
        }

        private void OnError(string message)
        {
            MessageBox.Show(message,
                            string.Format("An error happened in the panel {0}:", ComponentInfo.FriendlyName),
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Connects to the ermex network
        /// </summary>
        private void ConnectToNetwork()
        {
            //basic onfiguration
            var cfg = Configuration.Configure(ComponentInfo.ComponentId).ListeningToTcpPort((ushort)ComponentInfo.Port);

            //we configure the component to use an in-memory storage, it wont persist between sessions
            cfg = cfg.SetInMemoryDb(); //this is the default value anyway

            //If is not the network creator(the first) then set up the component to join to
            if (ComponentInfo.FriendComponent != null)
            {
                string localhostIp = Utils.GetLocalhostIp();
                cfg = cfg.RequestJoinTo(localhostIp,
                                        ComponentInfo.FriendComponent.Port, ComponentInfo.FriendComponent.ComponentId);
            }

            //now lets connect
            WorldGate.ConfigureAndStart(cfg);
        }

    }
}
