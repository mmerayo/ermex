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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Common;

namespace DrinksMachine
{
    public partial class FrmMachineEmulator : Form
    {
        private const int InitialDrinks = 7;

        /// <summary>
        /// When this amount is reached an alarm is raised
        /// </summary>
        private const int AlarmItems = 5;

        private enum Drinks
        {
            Green=1,
            Orange,
            Red
        }

        private readonly Dictionary<Drinks, int> _stock = new Dictionary<Drinks, int>(3)
            {
                {Drinks.Green, InitialDrinks},
                {Drinks.Orange, InitialDrinks},
                {Drinks.Red, InitialDrinks}
            };

        private LocalComponentInfo ComponentInfo { get; set; }

        public FrmMachineEmulator(LocalComponentInfo componentInfo)
        {
            InitializeComponent();

            if (componentInfo == null) throw new ArgumentNullException("componentInfo");
            ComponentInfo = componentInfo;
        }

        private void FrmMachineEmulator_Load(object sender, EventArgs e)
        {
            try
            {
                ShowInfo("Loading form..");
                Text = string.Format("Beverages machine with ermeX Id: {0}", ComponentInfo.ComponentId);

                btnBuyGreen.Image= new Bitmap(pbGreen.Image, btnBuyGreen.Width, btnBuyGreen.Height);
                btnBuyGreen.Tag = Drinks.Green;

                btnBuyOrange.Image = new Bitmap(pbOrange.Image, btnBuyOrange.Width, btnBuyOrange.Height);
                btnBuyOrange.Tag = Drinks.Orange;

                btnBuyRed.Image = new Bitmap(pbRed.Image, btnBuyRed.Width, btnBuyRed.Height);
                btnBuyRed.Tag = Drinks.Red;

                //Show current stock
                RefreshStock();

                ShowInfo(string.Empty);

            }
            catch (Exception ex)
            {
                OnError(ex.ToString());
            }
        }

        private void OnError(string message)
        {
            MessageBox.Show(message,
                            string.Format("An error happened in the machine emulator {0}:", ComponentInfo.ComponentId),
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void Buy_Drink_Click(object sender, EventArgs e)
        {
            try
            {
                var drinkType = (Drinks) ((Button) sender).Tag;

                if(_stock[drinkType]==0)
                {
                    MessageBox.Show("We apologise, there are no more drinks until they are replaced", "No more drinks",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                //decreases the current stock
                _stock[drinkType]--;

                //Shows the new stock
                RefreshStock();

                //Check status
            }
            catch (Exception ex)
            {
                OnError(ex.ToString());
            }
        }

        private void RefreshStock()
        {
            lblGreenStock.Text = _stock[Drinks.Green].ToString(CultureInfo.InvariantCulture);
            lblOrangeStock.Text = _stock[Drinks.Orange].ToString(CultureInfo.InvariantCulture);
            lblRedStock.Text = _stock[Drinks.Red].ToString(CultureInfo.InvariantCulture);
            
        }

        private void ShowInfo(string messageToShow)
        {
            lblInfo.Text = messageToShow;
            lblInfo.Invalidate();
        }
    }
}
