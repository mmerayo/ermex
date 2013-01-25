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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Common.Infos;
using Common.Other;
using ermeX;

namespace Common.Base
{
     

    //so we can use the designer
#if DEBUG
    public class FormComponentBase : Form
#else
    public abstract class FormComponentBase : Form

#endif
    {

        protected enum ConnectionStatus
        {
            Disconnected=0,
            Connecting,
            Connected,
            Disconnecting
        }

        protected delegate void ConnectionStatusChangedHandler(ConnectionStatus newStatus);

        private readonly Timer _timer = new Timer();
        private ConnectionStatus _connectionStatus=ConnectionStatus.Disconnected;
        private string _lastInfoMessage = string.Empty;

        /// <summary>
        /// Gets the local ComponentInfo
        /// </summary>
        protected LocalComponentInfo ComponentInfo { get; private set; }

        /// <summary>
        /// gets the label where to show the info
        /// </summary>
#if DEBUG
        protected virtual Label InfoLabel { get { throw new InvalidOperationException("Override this"); } }
        
#else
        protected abstract Label InfoLabel { get ; }
#endif
        /// <summary>
        /// Indicates if the component is connected
        /// </summary>
        protected ConnectionStatus CurrentConnectionStatus
        {
            get
            {
                lock(this)
                    return _connectionStatus;
            }
            private set
            {
                lock(this)
                {
                    if (_connectionStatus == value)
                        return;
                    if (!ValidateStatusTransition(value))
                    {
                        throw new InvalidOperationException(
                            string.Format("The component is being set to a wrong status from {0} to {1}",
                                          Enum.GetName(typeof (ConnectionStatus), _connectionStatus),
                                          Enum.GetName(typeof (ConnectionStatus), value)));
                    }
                    _connectionStatus = value;
                    OnConnectionStatusChanged(_connectionStatus);
                }
            }
        }

        private bool ValidateStatusTransition(ConnectionStatus newStatus)
        {
            switch (newStatus)
            {
                case ConnectionStatus.Disconnected:
                    return true;
                case ConnectionStatus.Connecting:
                    return _connectionStatus == ConnectionStatus.Disconnected || _connectionStatus == ConnectionStatus.Connecting;
                case ConnectionStatus.Connected:
                    return _connectionStatus == ConnectionStatus.Connecting;
                case ConnectionStatus.Disconnecting:
                    return _connectionStatus == ConnectionStatus.Connected || _connectionStatus==ConnectionStatus.Connecting;
                default:
                    throw new ArgumentOutOfRangeException("newStatus");
            }
        }

        protected event ConnectionStatusChangedHandler ConnectionStatusChanged;

        protected void OnConnectionStatusChanged(ConnectionStatus newstatus)
        {
            ConnectionStatusChangedHandler handler = ConnectionStatusChanged;
            if (handler != null) handler(newstatus);
        }

#if DEBUG
        public FormComponentBase()
        {
            
        }
#endif

        protected FormComponentBase(LocalComponentInfo componentInfo)
        {
            this.Activated += FormComponentBase_Activated;
            this.Load += FormComponentBase_Load;
            if (componentInfo == null) throw new ArgumentNullException("componentInfo");
            ComponentInfo = componentInfo;

        }

       

        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                _timer.Dispose();
            }
            //resets the worldgate disconnecting the component from the network
            Disconnect();

            base.Dispose(disposing);
        }

        protected void Disconnect()
        {
            lock (this)
            {
                if (CurrentConnectionStatus==ConnectionStatus.Connected || CurrentConnectionStatus==ConnectionStatus.Connecting)
                {
                    CurrentConnectionStatus=ConnectionStatus.Disconnecting;
                    
                    WorldGate.Reset();
                    
                    CurrentConnectionStatus = ConnectionStatus.Disconnected;
                }
            }
        }

        private void FormComponentBase_Activated(object sender, EventArgs e)
        {
            _timer.Interval = 2000;
            _timer.Tick += ClearInfo;
            _timer.Start();
        }
        private void FormComponentBase_Load(object sender, EventArgs e)
        {
            Text = string.Format("Component: {0} Id: {1}", ComponentInfo.FriendlyName,ComponentInfo.ComponentId);
        }

        /// <summary>
        /// Shows a message for 2 seconds.<seealso cref="_timer"/>
        /// </summary>
        /// <param name="messageToShow">The message to show</param>
        protected void ShowInfo(string messageToShow)
        {
            lock (InfoLabel)
            {
                _lastInfoMessage = messageToShow;
                InfoLabel.Text = messageToShow;
                InfoLabel.Invalidate();
                _timer.Enabled = true;
            }
        }

        /// <summary>
        /// This event is raised by the timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearInfo(object sender, EventArgs e)
        {
            lock (InfoLabel)
            {
                if (InfoLabel.Text == _lastInfoMessage)
                {
                    InfoLabel.Text = _lastInfoMessage = string.Empty;
                    _timer.Enabled = false;
                }
            }
        }

        protected void OnError(Exception ex)
        {
            if (ex == null) throw new ArgumentNullException("ex");
            OnError(ex.ToString());
        }

        protected void OnError(string message)
        {
            if (string.IsNullOrEmpty(message)) 
                throw new ArgumentException("message");

            MessageBox.Show(message,
                            string.Format("An error happened in the component {0}:", ComponentInfo.FriendlyName),
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
        }


        /// <summary>
        /// Connects to the ermex network
        /// </summary>
        protected void ConnectToNetwork()
        {
            lock (this)
            {
                if (CurrentConnectionStatus == ConnectionStatus.Disconnected)
                {
                    ShowInfo("Connecting to your ermeX network...");

                    Cursor current = Cursor;
                    Enabled = false;
                    Cursor = Cursors.WaitCursor;

                    CurrentConnectionStatus = ConnectionStatus.Connecting;

                    try
                    {
                        //basic configuration
                        var cfg =
                            Configuration.Configure(ComponentInfo.ComponentId).ListeningToTcpPort(
                                (ushort) ComponentInfo.Port);

                        //we configure the component to use an in-memory storage, it wont persist between sessions
                        cfg = cfg.SetInMemoryDb(); //this is the default mode anyway
                        //cfg =cfg.SetSqlServerDb(@"Server=localhost\SQLEXPRESS;Database=QS;User Id=ubiqUser;Password=sqlsql;"); //using sql server

                        //If is not the network creator(the first) then set up the component to join to
                        if (ComponentInfo.FriendComponent != null)
                        {
                            string localhostIp = Networking.GetLocalhostIp();
                            cfg = cfg.RequestJoinTo(localhostIp,
                                                    ComponentInfo.FriendComponent.Port,
                                                    ComponentInfo.FriendComponent.ComponentId);
                        }

                        //now lets connect
                        WorldGate.ConfigureAndStart(cfg);

                        SubscribeToMessages();
                        RegisterServices();

                    }
                    catch
                    {
                        CurrentConnectionStatus = ConnectionStatus.Disconnected;
                        throw;
                    }
                    finally
                    {
                        Enabled = true;
                        Cursor = current;
                    }
                    ShowInfo("Connected to your ermeX network");

                    CurrentConnectionStatus = ConnectionStatus.Connected;
                }
            }
        }

        /// <summary>
        /// Override to register services
        /// </summary>
        /// <remarks>The component supports autodiscovery but this is an illustrative example</remarks>
        protected virtual void RegisterServices(){}

        /// <summary>
        /// Override to subscribe to messages
        /// </summary>
        /// <remarks>The component supports autodiscovery but this is an illustrative example</remarks>
        protected virtual void SubscribeToMessages(){}

        protected void PublishMessage(object message)
        {
            if (message == null) throw new ArgumentNullException("message");
            
            Cursor current = Cursor;
            Enabled = false;
            Cursor = Cursors.WaitCursor;

            try
            {
                //publish machine status
                WorldGate.Publish(message);
            }
            finally
            {
                Enabled = true;
                Cursor = current;
            }
        }
    }
}