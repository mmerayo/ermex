using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AlarmGeneratorSampleRunner
{
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

        private Status CurrentStatus { get; set; }

        public FrmRunner()
        {
            InitializeComponent();

            SetStatus(Status.Stopped);
        }

        private void SetStatus(Status newStatus)
        {
            if (CurrentStatus == newStatus)
                return;

            CurrentStatus = newStatus;

            btnStart.Enabled = CurrentStatus == Status.Stopped;
            btnNewMachine.Enabled = CurrentStatus == Status.Running;
            btnNewPanel.Enabled = CurrentStatus == Status.Running;
            btnStop.Enabled = CurrentStatus == Status.Running;
        }


        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                //performs the default start, one vendor machine and one stock alarm panel

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

                //changes the status
                SetStatus(Status.Stopped);
            }
            catch (Exception ex)
            {
                OnError(ex.ToString());
            }
        }

        private void OnError(string message)
        {
            MessageBox.Show(message, "An error happened:", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
