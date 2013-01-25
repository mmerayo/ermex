﻿// /*---------------------------------------------------------------------------------------*/
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
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Common.Base;
using Common.Infos;
using Common.Other;
using CommonContracts.Messages;
using CommonContracts.Services;
using StockBoyPanel.DataSources;
using StockBoyPanel.MessageHandlers;
using ermeX;

namespace StockBoyPanel
{
    /// <summary>
    /// Implementation of the panel
    /// </summary>
    public partial class FrmOperationsPanel : FormComponentBase
    {
        readonly List<MachineStatusView> _currentViewItems = new List<MachineStatusView>();

        public FrmOperationsPanel(LocalComponentInfo componentInfo) : base(componentInfo)
        {
            InitializeComponent();

            this.txtName.Text = componentInfo.FriendlyName;
            dgMachines.CellContentClick += new DataGridViewCellEventHandler(dgMachines_CellContentClick);
            dgMachines.AutoGenerateColumns = false;
            AddDgvColumns();
            ConnectionStatusChanged += FrmOperationsPanel_ConnectionStatusChanged;
            MachinesDataSource.Default.CollectionChanged += new EventHandler(Default_CollectionChanged);
        }

       
       
        #region datagrid operations

        private void AddDgvColumns()
        {
            dgMachines.ColumnCount = 1;
            dgMachines.Columns[0].Name = "MachineName";
            dgMachines.Columns[0].DataPropertyName = "ComponentName";
            
            dgMachines.Columns.Add(new DataGridViewImageColumn(true));
            dgMachines.Columns[1].Name = "Green drinks status";
            dgMachines.Columns[1].DataPropertyName = "GreenCans";

            dgMachines.Columns.Add(new DataGridViewImageColumn(true));
            dgMachines.Columns[2].Name = "Orange drinks status";
            dgMachines.Columns[2].DataPropertyName = "OrangeCans";

            dgMachines.Columns.Add(new DataGridViewImageColumn(true));
            dgMachines.Columns[3].Name = "Red drinks status";
            dgMachines.Columns[3].DataPropertyName = "RedCans";

            dgMachines.Columns.Add(new DataGridViewCheckBoxColumn());
            dgMachines.Columns[4].HeaderText = "Connected";
            dgMachines.Columns[4].DataPropertyName = "IsConnected";

            var btn = new DataGridViewButtonColumn {Text = "Add items"};
            dgMachines.Columns.Add(btn);
            dgMachines.Columns[5].Name = "Fill";
        }

        void dgMachines_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex>=0 && e.ColumnIndex == 5)
            {
                var machineData = (MachineStatusView) dgMachines.Rows[e.RowIndex].DataBoundItem;
                MachineStatus machineStatus = MachinesDataSource.Default.Get(machineData.ID);
                if(machineStatus.IsConnected)
                    FrmFill.ShowForm(this,machineStatus);
                else
                    ShowInfo("The machine is not on-line");
            }
        }

        void Default_CollectionChanged(object sender, EventArgs e)
        {
            Cursor current = Cursor;

            lock (this)
            {
                try
                {
                    Enabled = false;
                    Cursor = Cursors.WaitCursor;
                    
                    ShowInfo("Updating machines..");
                    
                    var machineStatuses = MachinesDataSource.Default.Data;
                    _currentViewItems.Clear();
                    _currentViewItems.AddRange(machineStatuses.Select(item => (MachineStatusView)item).OrderBy(x=>x.ComponentName));

                    if (dgMachines.InvokeRequired)
                    {
                        this.Invoke(new Action(BindGrid));
                    }
                    else
                    {
                        BindGrid();
                    }
                    ShowInfo("Done");

                }
                catch (Exception ex)
                {
                    OnError(ex);
                }
                finally
                {
                    Enabled = true;
                    Cursor = current;
                }
            }
        }

        private void BindGrid( )
        {
            var source = new BindingSource();
            source.ResetBindings(false);
            source.AllowNew = false;
            source.DataSource = _currentViewItems;
            dgMachines.DataSource = source;
            dgMachines.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);

        }

        #endregion datagrid operations

        #region base

        private void FrmOperationsPanel_ConnectionStatusChanged(ConnectionStatus newStatus)
        {
            mnuConnect.Enabled = newStatus == ConnectionStatus.Disconnected;
            dgMachines.Enabled = mnuDisconnect.Enabled = newStatus == ConnectionStatus.Connected;

            if (CurrentConnectionStatus == ConnectionStatus.Connected)
            {
                RequestMachinesStatus();
            }
        }

        protected override Label InfoLabel
        {
            get { return lblInfo; }
        }

        protected override void SubscribeToMessages()
        {
            base.SubscribeToMessages();
            
            //the panel needs to receive any update about the status of the machines
            WorldGate.Suscribe<MachineStatusHandler>();

        }

        protected override void RegisterServices()
        {
            base.RegisterServices();
        }


        #endregion base

        #region view event handlers

        private void mnuConnect_Click(object sender, EventArgs e)
        {
            try
            {
                ConnectToNetwork();
            }
            catch (Exception ex)
            {
                OnError(ex.ToString());
            }
        }

        private void mnuDisconnect_Click(object sender, EventArgs e)
        {
            var current = Cursor;
            try
            {
                Enabled = false;
                Cursor = Cursors.WaitCursor;

                Disconnect();
                this.dgMachines.DataSource = null;
            }
            catch (Exception ex)
            {
                OnError(ex.ToString());
            }
            finally
            {
                Enabled = true;
                Cursor = current;
            }
        }

        #endregion view event handlers

        #region private methods

        /// <summary>
        /// Invokes the service which is exposed by all the machines and as is subscribed to the message MachineStatus will receive the message
        /// </summary>
        private void RequestMachinesStatus()
        {
            var machineStatusService = WorldGate.GetServiceProxy<IMachineStatusService>();
            machineStatusService.PublishStatus();
            ShowInfo("Requested machines to publish their status");
        }

        #endregion private methods
    }
}