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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace AlarmGeneratorSampleRunner
{
    //NOTE: this assembly is not the focus of the QuickStart
    public partial class FrmRunner : Form
    {

        /// <summary>
        /// Enumerates the status of the view
        /// </summary>
        private enum Status
        {
            /// <summary>
            /// The emulation is running
            /// </summary>
            Running = 1,

            /// <summary>
            /// the emulation is stopped
            /// </summary>
            Stopped = 0
        }

        private sealed class ProcessInfo
        {

            public Process TheProcess { get; set; }
            public Guid TheComponentId { get; set; }
            public int ThePort { get; set; }

        }

        private readonly Dictionary<int, ProcessInfo> _currentProcesses = new Dictionary<int, ProcessInfo>();

        private Status CurrentStatus { get; set; }

        public FrmRunner()
        {
            InitializeComponent();

            SetStatus(Status.Stopped);
        }

        private void FrmRunner_Load(object sender, EventArgs e)
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
        }

        void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            try
            {
                FinishProcesses();
            }
            catch (Exception ex)
            {
                OnError(ex.ToString());
            }
        }


        private void SetStatus(Status newStatus)
        {
            if (CurrentStatus == newStatus)
                return;

            CurrentStatus = newStatus;

            btnStart.Enabled = CurrentStatus == Status.Stopped;
            btnNewPanel.Enabled = btnNewMachine.Enabled = btnStop.Enabled = CurrentStatus == Status.Running;
        }


        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                //performs the default start, one vendor machine and one stock alarm panel
                StartNewMachine();
                StartNewPanel();

                //changes the status
                SetStatus(Status.Running);
            }
            catch (Exception ex)
            {
                OnError(ex.ToString());
            }
        }


        private void btnStop_Click(object sender, EventArgs e)
        {
            try
            {
                //Finish the processes
                FinishProcesses();

                //changes the status
                SetStatus(Status.Stopped);
            }
            catch (Exception ex)
            {
                OnError(ex.ToString());
            }
        }



        private void btnNewMachine_Click(object sender, EventArgs e)
        {
            try
            {
                //Adds a new machine by creating a process that emulates the device
                StartNewMachine();

                //changes the status
                SetStatus(Status.Stopped);
            }
            catch (Exception ex)
            {
                OnError(ex.ToString());
            }
        }

        private void btnNewPanel_Click(object sender, EventArgs e)
        {
            try
            {
                //Adds a new machine by creating a process that emulates the device
                StartNewPanel();

                //changes the status
                SetStatus(Status.Stopped);
            }
            catch (Exception ex)
            {
                OnError(ex.ToString());
            }
        }

        private void StartNewPanel()
        {
            //Gets the new component id, in a real-world application this should be always the same
            Guid componentId = Guid.NewGuid();

            //Gets the new component port
            int port = GetFreePort(2000, 50000);

            //starts the Panel process
            StartProccess("StockBoyPanel.exe", "Panel_Name", componentId, port);
        }

        private void StartNewMachine()
        {
            //Gets the new component id, in a real-world application this should be always the same
            Guid componentId = Guid.NewGuid();

            //Gets the new component port
            int port = GetFreePort(2000, 50000);

            //starts the Panel process
            StartProccess("DrinksMachine.exe","Machine_Name", componentId, port);
        }

        /// <summary>
        /// Starts one process
        /// </summary>
        /// <param name="exeFile"></param>
        /// <param name="friendlyName"> </param>
        /// <param name="componentId"> </param>
        /// <param name="port"> </param>
        private int StartProccess(string exeFile, string friendlyName, Guid componentId, int port)
        {
            friendlyName = friendlyName.Replace(' ', '_'); 

            //Build the arguments, if its the first we dont need to tel the process to join another component otherwise will join a random one
            string arguments = string.Format("{0} {1} {2}", friendlyName, componentId, port);

            if (_currentProcesses.Count > 0)
            {
                //the friend component is one of the created so the new component will join the ermeX network through any of the existing
                int index = GetRandomInt(_currentProcesses.Count - 1);

                var friend = _currentProcesses.ElementAt(index).Value;
                Debug.Assert(friend != null);

                arguments += string.Format(" {0} {1}", friend.TheComponentId, friend.ThePort);
                //so the the process will configure ermeX to join the network through the friend
            }

            //start the process
            string fileName = Path.Combine(GetApplicationFolderPath(), exeFile);
            var process = new Process
                {
                    StartInfo =
                        {
                            FileName = fileName,
                            Arguments = arguments,
                        }
                };
            process.EnableRaisingEvents = true;
            process.Exited += new EventHandler(process_Exited);
            process.Start();

            var result = process.Id;
            var processInfo = new ProcessInfo()
                {
                    TheProcess = process,
                    TheComponentId = componentId,
                    ThePort = port
                };


            _currentProcesses.Add(result, processInfo);

            Debug.Assert(result > 0);
            return result;
        }

        private void process_Exited(object sender, EventArgs e)
        {
            var process = (Process) sender;
            Debug.Assert(process != null); //TODO: REMOVE THIS

            var pId = process.Id;
            _currentProcesses.Remove(pId); //it must contain it
        }

        private void FinishProcesses()
        {
            var processes = new List<ProcessInfo>(_currentProcesses.Values);
            foreach (var p in processes)
            {
                Process theProcess = p.TheProcess;
                theProcess.Kill();
                theProcess.WaitForExit(5000);
            }
        }

        private void OnError(string message)
        {
            MessageBox.Show(message, "An error happened:", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }


        private bool PortIsBusy(ushort port)
        {
            if (_currentProcesses.Values.Any(x => x.ThePort == port))
                return true;

            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] endpoints = ipGlobalProperties.GetActiveTcpListeners();

            if (endpoints == null || endpoints.Length == 0) return false;
            return endpoints.Any(t => t.Port == port);
        }

        private ushort GetFreePort(ushort bottomRange, ushort topRange)
        {
            var x = bottomRange;
            var y = topRange;
            if (bottomRange > topRange)
            {
                x = topRange;
                y = bottomRange;
            }

            for (var i = x; i <= y; i++)
            {
                if (!PortIsBusy(i))
                    return i;
            }

            throw new ApplicationException(
                string.Format("All the ports from {0} to {1} are busy. You may extend the range", x, y));
        }

        public static int GetRandomInt(int maxValue)
        {
            var random = new Random((int) DateTime.Now.Ticks);
            return random.Next(0, maxValue < int.MaxValue ? maxValue + 1 : int.MaxValue);
        }

        private string _applicationFolderPath = null;

        public string GetApplicationFolderPath()
        {
            if (_applicationFolderPath == null)
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                _applicationFolderPath = Path.GetDirectoryName(path);
            }
            return _applicationFolderPath;
        }
    }
}