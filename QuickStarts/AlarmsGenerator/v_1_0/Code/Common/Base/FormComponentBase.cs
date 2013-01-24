using System;
using System.Collections.Generic;
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
        private readonly Timer _timer = new Timer();
        private bool _connected;
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
        protected FormComponentBase(LocalComponentInfo componentInfo)
        {
            this.Activated += FormComponentBase_Activated;

            if (componentInfo == null) throw new ArgumentNullException("componentInfo");
            ComponentInfo = componentInfo;

        }

        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                this.Activated -= FormComponentBase_Activated;
                _timer.Dispose();
            }
            //resets the worldgate disconnecting the component from the network
            if (_connected)
                WorldGate.Reset();

            base.Dispose(disposing);
        }

        private void FormComponentBase_Activated(object sender, EventArgs e)
        {
            _timer.Interval = 2000;
            _timer.Tick += ClearInfo;
            _timer.Start();
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
                    _timer.Enabled=false;
                }
            }
        }

        protected void OnError(string message)
        {
            MessageBox.Show(message,
                            string.Format("An error happened in the component {0}:", ComponentInfo.FriendlyName),
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        /// Connects to the ermex network
        /// </summary>
        protected virtual void ConnectToNetwork()
        {
            lock (this)
            {
                if (!_connected)
                {
                    ShowInfo("Connecting to ermeX network...");
                    
                    Cursor current = Cursor;
                    Enabled = false;
                    Cursor = Cursors.WaitCursor;
                   
                    //basic configuration
                    var cfg =
                        Configuration.Configure(ComponentInfo.ComponentId).ListeningToTcpPort(
                            (ushort) ComponentInfo.Port);

                    //we configure the component to use an in-memory storage, it wont persist between sessions
                    cfg = cfg.SetInMemoryDb(); //this is the default mode anyway

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

                    Enabled = true;
                    Cursor = current;
                    ShowInfo("Connected");

                    _connected = true;
                }
            }
        }
    }
}