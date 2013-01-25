using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CommonContracts.Messages;
using CommonContracts.Services;
using CommonContracts.enums;
using StockBoyPanel.DataSources;
using ermeX;

namespace StockBoyPanel
{
    public partial class FrmFill : Form
    {
        private static readonly FrmFill Instance=new FrmFill();
        private MachineStatus _machine;

        private FrmFill()
        {
            InitializeComponent();

            MachinesDataSource.Default.CollectionChanged += new EventHandler(Default_CollectionChanged);
        }

        void Default_CollectionChanged(object sender, EventArgs e)
        {
            lock(this)
            {
                if (Machine == null) return;
                if(!MachinesDataSource.Default.Get(Machine.Id).IsConnected)
                    HideForm();
            }
        }

        public static void ShowForm(FrmOperationsPanel caller, MachineStatus machineStatus)
        {
            if (caller == null) throw new ArgumentNullException("caller");
            if (machineStatus == null) throw new ArgumentNullException("machineStatus");
            Instance.Machine = machineStatus;
            Instance.udGreen.Value = Instance.udRed.Value = Instance.udOrange.Value = 0;
            Instance.ShowDialog(caller);
        }


        protected MachineStatus Machine
        {
            get { return _machine; }
            set
            {
                this.Text = string.Format("Filling Drinks. Machine: {0}", value.Name);
                _machine = value;
            }
        }

        private void btnCommit_Click(object sender, EventArgs e)
        {
            try
            {
                Enabled = false;
                Cursor = Cursors.WaitCursor;
                if (udGreen.Value == 0 && udOrange.Value == 0 && udRed.Value == 0)
                    OnError("Must add at least one item");
                //Gets the proxy, BUT ONLY FOR THE DESTINATION MACHINE
                var svc = WorldGate.GetServiceProxy<IMachineStatusService>(Machine.Id);
                if(udGreen.Value > 0)
                {
                    var numItemsToAdd = (int) udGreen.Value;
                    svc.AddItems(DrinkType.Green,numItemsToAdd);
                }
                if (udOrange.Value > 0)
                {
                    var itemsToAdd = (int) udOrange.Value;
                    svc.AddItems(DrinkType.Orange, itemsToAdd);
                }
                if (udRed.Value > 0)
                {
                    var toAdd = (int) udRed.Value;
                    svc.AddItems(DrinkType.Red, toAdd);
                }

                HideForm();
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            finally
            {
                Enabled = true;
                Cursor = Cursors.Default;
            }
        }

        private void HideForm()
        {
            this.Hide();
            _machine = null;
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
                            "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
