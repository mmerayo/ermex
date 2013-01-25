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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Windows.Forms;

namespace AlarmGeneratorSampleRunner
{
    //NOTE: this assembly is not the focus of the QuickStart
    public partial class FrmRunner : Form
    {
        private readonly Dictionary<int, ProcessInfo> _currentProcesses = new Dictionary<int, ProcessInfo>();
        private string _applicationFolderPath;
        private readonly Timer _timer=new Timer();

        public FrmRunner()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            SetStatus(Status.Stopped);
        }

        private Status CurrentStatus { get; set; }

        private void FrmRunner_Load(object sender, EventArgs e)
        {
            _timer.Interval = 5000;
            _timer.Tick += new EventHandler(_timer_Tick);
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

        }

        void _timer_Tick(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Default;
            this.Enabled = true;
            _timer.Stop();
        }
        private void SleepInteraction()
        {
            this.Enabled = false;
            this.Cursor = Cursors.WaitCursor;
            _timer.Start();
        }

        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
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

            int count = this._currentProcesses.Values.Count(x => !x.IsMachine) + 1;

            //starts the Panel process
            StartProccess("StockBoyPanel.exe", "UserName_"+count, componentId, port, false);
        }

        private void StartNewMachine()
        {
            //Gets the new component id, in a real-world application this should be always the same
            Guid componentId = Guid.NewGuid();

            //Gets the new component port
            int port = GetFreePort(2000, 50000);

            int count = this._currentProcesses.Values.Count(x => x.IsMachine)+1;

            //starts the Panel process
            StartProccess("DrinksMachine.exe", "MachineName_"+count, componentId, port, true);
        }

        /// <summary>
        /// Starts one process
        /// </summary>
        /// <param name="exeFile"></param>
        /// <param name="friendlyName"> </param>
        /// <param name="componentId"> </param>
        /// <param name="port"> </param>
        private void StartProccess(string exeFile, string friendlyName, Guid componentId, int port, bool isMachine)
        {
            friendlyName = friendlyName.Replace(' ', '_');

            //Build the arguments, if its the first we dont need to tel the process to join another component otherwise will join a random one
            string arguments = string.Format("{0} {1} {2}", friendlyName, componentId, port);

            if (_currentProcesses.Count > 0)
            {
                //the friend component is one of the created so the new component will join the ermeX network through any of the existing
                ProcessInfo friend;
                do
                {
                    int index = GetRandomInt(_currentProcesses.Count - 1);
                    friend = _currentProcesses.ElementAt(index).Value;
                } while (!friend.IsMachine);

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
            process.Exited += process_Exited;
            process.Start();

            int key = process.Id;
            var processInfo = new ProcessInfo
                {
                    TheProcess = process,
                    TheComponentId = componentId,
                    ThePort = port,
                    IsMachine = isMachine
                };


            _currentProcesses.Add(key, processInfo);

            Debug.Assert(key > 0);

            SleepInteraction();
        }

       

        private void process_Exited(object sender, EventArgs e)
        {
            try
            {
                lock (this)
                {
                    this.Enabled = false;
                    var process = (Process) sender;
                    Debug.Assert(process != null); //TODO: REMOVE THIS

                    int pId = process.Id;
                    _currentProcesses.Remove(pId); //it must contain it

                    if (!_finishingProcesses && !_currentProcesses.Values.Any(x => x.IsMachine) &&
                        _currentProcesses.Count > 0)
                    {
                        FinishProcesses();
                        OnError(
                            "Finished all processes as there were not machines on and the network of this sample is started by a machine with a random configuration");
                    }
                    this.Enabled = true;
                }
            }catch(Exception ex)
            {
                OnError(ex.ToString());
            }
        }

        private bool _finishingProcesses = false;
        private void FinishProcesses()
        {
            _finishingProcesses = true;
            var processes = new List<ProcessInfo>(_currentProcesses.Values);
            foreach (ProcessInfo p in processes)
            {
                Process theProcess = p.TheProcess;
                theProcess.Kill();
                theProcess.WaitForExit(5000);
            }
            _finishingProcesses = false;
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
            ushort x = bottomRange;
            ushort y = topRange;
            if (bottomRange > topRange)
            {
                x = topRange;
                y = bottomRange;
            }

            for (ushort i = x; i <= y; i++)
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

        

        #region Nested type: ProcessInfo

        private sealed class ProcessInfo
        {
            public Process TheProcess { get; set; }
            public Guid TheComponentId { get; set; }
            public int ThePort { get; set; }

            /// <summary>
            /// The machines are connected since they start
            /// </summary>
            public bool IsMachine { get; set; }
        }

        #endregion

        #region Nested type: Status

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

        #endregion

        private void FrmRunner_FormClosed(object sender, FormClosedEventArgs e)
        {
            _timer.Dispose();
        }
    }
}