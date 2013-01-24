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
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Common;
using CommonContracts;
using CommonContracts.Messages;
using CommonContracts.Services;
using CommonContracts.enums;
using DrinksMachine.ServiceImplementations;
using ermeX;

namespace DrinksMachine
{

    public partial class FrmMachineEmulator : Form,IStatusPublisher
    {
        private const int InitialDrinks = 7;

        /// <summary>
        /// When this amount is reached an alarm is raised
        /// </summary>
        private const int FewItemsAlarm = 5;

        private readonly Dictionary<DrinkType, int> _stock = new Dictionary<DrinkType, int>(3)
            {
                {DrinkType.Green, InitialDrinks},
                {DrinkType.Orange, InitialDrinks},
                {DrinkType.Red, InitialDrinks}
            };

        //used to show the information for a while
        private string _lastInfoMessage = string.Empty;
        private readonly Timer _timer = new Timer();

        //stores a value indicating if the component was connected, we use this since we connected in the activate event
        private bool _connected = false;
        private LocalComponentInfo ComponentInfo { get; set; }

        public FrmMachineEmulator(LocalComponentInfo componentInfo)
        {
            InitializeComponent();

            if (componentInfo == null) throw new ArgumentNullException("componentInfo");
            ComponentInfo = componentInfo;
            txtName.Text = componentInfo.FriendlyName;
            StatusService.SetStatusPublisher(this);
        }

        private void FrmMachineEmulator_Load(object sender, EventArgs e)
        {
            try
            {
                _timer.Interval = 2000;
                _timer.Tick += new EventHandler(ClearInfo);
                Text = string.Format("Beverages machine: {0}", ComponentInfo.FriendlyName);

                btnBuyGreen.Image = new Bitmap(pbGreen.Image, btnBuyGreen.Width, btnBuyGreen.Height);
                btnBuyGreen.Tag = DrinkType.Green;

                btnBuyOrange.Image = new Bitmap(pbOrange.Image, btnBuyOrange.Width, btnBuyOrange.Height);
                btnBuyOrange.Tag = DrinkType.Orange;

                btnBuyRed.Image = new Bitmap(pbRed.Image, btnBuyRed.Width, btnBuyRed.Height);
                btnBuyRed.Tag = DrinkType.Red;

                //Show current stock
                RefreshStock();

            }
            catch (Exception ex)
            {
                OnError(ex.ToString());
            }
        }

        private void OnError(string message)
        {
            MessageBox.Show(message,
                            string.Format("An error happened in the machine emulator {0}:", ComponentInfo.FriendlyName),
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void Buy_Drink_Click(object sender, EventArgs e)
        {
            try
            {
                var drinkType = (DrinkType) ((Button) sender).Tag;

                lock (_stock)
                {
                    if (_stock[drinkType] == 0)
                    {
                        MessageBox.Show("We apologise, there are no more drinks until they are replaced",
                                        "No more drinks",
                                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }


                    //decreases the current stock
                    _stock[drinkType]--;

                    //Check status
                    CheckAlarms(drinkType);
                }
                //Shows the new stock
                RefreshStock();

            }
            catch (Exception ex)
            {
                OnError(ex.ToString());
            }
        }

        private void CheckAlarms(DrinkType drinkType)
        {
            if (_stock[drinkType] == FewItemsAlarm)
            {
                ShowInfo(string.Format(@"Sending alarm ""Few Items"" for {0}",
                                       Enum.GetName(typeof (DrinkType), drinkType)));

                //send alarm
                PublishStatus();
                return;
            }

            if (_stock[drinkType] == 0)
            {
                ShowInfo(string.Format(@"Sending alarm ""No Items"" for {0}",
                                       Enum.GetName(typeof (DrinkType), drinkType)));

                //send alarm
                PublishStatus();
                return;
            }

        }

        /// <summary>
        /// shows the current stock
        /// </summary>
        private void RefreshStock()
        {
            lblGreenStock.Text = _stock[DrinkType.Green].ToString(CultureInfo.InvariantCulture);
            lblOrangeStock.Text = _stock[DrinkType.Orange].ToString(CultureInfo.InvariantCulture);
            lblRedStock.Text = _stock[DrinkType.Red].ToString(CultureInfo.InvariantCulture);

        }


        /// <summary>
        /// Shows a message
        /// </summary>
        /// <param name="messageToShow"></param>
        private void ShowInfo(string messageToShow)
        {
            lock (lblInfo)
            {
                _lastInfoMessage = messageToShow;
                lblInfo.Text = messageToShow;
                lblInfo.Invalidate();
            }
            _timer.Start();

        }

        /// <summary>
        /// This event is raised by the timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearInfo(object sender, EventArgs e)
        {
            lock (lblInfo)
            {
                if (lblInfo.Text == _lastInfoMessage)
                {
                    lblInfo.Text = _lastInfoMessage = string.Empty;
                    _timer.Stop();
                }
            }
        }

        /// <summary>
        /// Connects to the ermex network
        /// </summary>
        private void ConnectToNetwork()
        {
            lock (this)
            {
                if (!_connected)
                {
                    ShowInfo("Connecting to ermeX network...");
                    var current = this.Cursor;
                    this.Enabled = false;
                    this.Cursor = Cursors.WaitCursor;
                    //basic onfiguration
                    var cfg =
                        Configuration.Configure(ComponentInfo.ComponentId).ListeningToTcpPort(
                            (ushort) ComponentInfo.Port);

                    //we configure the component to use an in-memory storage, it wont persist between sessions
                    cfg = cfg.SetInMemoryDb(); //this is the default mode anyway

                    //If is not the network creator(the first) then set up the component to join to
                    if (ComponentInfo.FriendComponent != null)
                    {
                        string localhostIp = Utils.GetLocalhostIp();
                        cfg = cfg.RequestJoinTo(localhostIp,
                                                ComponentInfo.FriendComponent.Port,
                                                ComponentInfo.FriendComponent.ComponentId);
                    }

                    //now lets connect
                    WorldGate.ConfigureAndStart(cfg);
                    this.Enabled = true;
                    this.Cursor = current;
                    ShowInfo("Connected");
                    _connected = true;
                }
            }

            //updates the status in the currently connected components
            PublishStatus();
        }

        private void FrmMachineEmulator_FormClosing(object sender, FormClosingEventArgs e)
        {
            _timer.Dispose();

            //resets the worldgate disconnecting from the network
            WorldGate.Reset();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string machineName = txtName.Text.Trim();
                if (string.IsNullOrEmpty(machineName))
                    throw new ArgumentException("The name cannot be an empty value");

                if (machineName == ComponentInfo.FriendlyName)
                {
                    ShowInfo("The name was not changed");
                    return;
                }
                ComponentInfo.FriendlyName = machineName;
                ShowInfo("Publishing machine status");

                PublishStatus();

                ShowInfo("Status published");
            }
            catch (Exception ex)
            {
                OnError(ex.ToString());
            }
        }

        public void PublishStatus()
        {
            lock (this)
            {
                var current = this.Cursor;
                this.Enabled = false;
                this.Cursor = Cursors.WaitCursor;

                //Get the status
                var message = GetStatus();

                //publish machine status
                WorldGate.Publish(message);

                this.Enabled = true;
                this.Cursor = current;
            }
        }

        private MachineStatus GetStatus()
        {
            var machineStatus = new MachineStatus
                {
                    Id = ComponentInfo.ComponentId,
                    Name = ComponentInfo.FriendlyName,
                    CurrentStock = _stock
                };
            return machineStatus;
        }

        private void FrmMachineEmulator_Activated(object sender, EventArgs e)
        {
            //connect
            ConnectToNetwork();
        }

       
    }
}
