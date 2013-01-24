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
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Common.Base;
using Common.Infos;
using Common.Other;
using CommonContracts.Messages;
using CommonContracts.enums;
using DrinksMachine.ServiceImplementations;
using ermeX;

namespace DrinksMachine
{
    public partial class FrmMachineEmulator : FormComponentBase, IStatusPublisher
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

        public FrmMachineEmulator(LocalComponentInfo componentInfo):base(componentInfo)
        {
            InitializeComponent();
           
            txtName.Text = componentInfo.FriendlyName;

            //indicates the StatusService that hte current instance is the publisher
            StatusService.SetStatusPublisher(this);
        }

        #region base class overrides

        protected override Label InfoLabel
        {
            get { return lblInfo; }
        }

        protected override void ConnectToNetwork()
        {
            base.ConnectToNetwork();
            
            //updates the status in the currently connected components
            PublishStatus();
        }

        #endregion base class overrides

        #region IStatusPublisher Members

        public void PublishStatus()
        {
            lock (this)
            {
                Cursor current = Cursor;
                Enabled = false;
                Cursor = Cursors.WaitCursor;

                //Get the status
                MachineStatus message = GetStatus();

                //publish machine status
                WorldGate.Publish(message);

                Enabled = true;
                Cursor = current;
            }
        }

        #endregion

        #region view event handlers

        private void FrmMachineEmulator_Load(object sender, EventArgs e)
        {
            try
            {
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

        private void FrmMachineEmulator_Activated(object sender, EventArgs e)
        {
            //connects to the network when is activated
            ConnectToNetwork();
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

        private void UpdateName_Click(object sender, EventArgs e)
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


        #endregion view event handlers

        #region private methods

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

        #endregion private methods
    }
}