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

namespace DrinksMachine
{
    public partial class FrmMachineEmulator : Form
    {
        private LocalComponentInfo ComponentInfo { get; set; }

        public FrmMachineEmulator()
        {
            InitializeComponent();
        }

        public FrmMachineEmulator(LocalComponentInfo componentInfo)
        {
            if (componentInfo == null) throw new ArgumentNullException("componentInfo");
            ComponentInfo = componentInfo;
        }

        private void FrmMachineEmulator_Load(object sender, EventArgs e)
        {
            try
            {
                Text = string.Format("Beverages machine with ermeX Id: {0}", ComponentInfo.ComponentId);
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
    }
}
