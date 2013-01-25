using System;
using System.ComponentModel;
using System.Drawing;
using CommonContracts.Messages;
using CommonContracts.enums;

namespace StockBoyPanel
{
    /// <summary>
    /// View data of the machine status
    /// </summary>
    public class MachineStatusView
    {
        [Browsable(false)]
        public Guid ID { get; set; }

        public string ComponentName { get; set; }
        public bool IsConnected { get; set; }

        public Icon GreenCans { get; set; }
        public Icon OrangeCans { get; set; }
        public Icon RedCans { get; set; }

        private static readonly Icon StatusOkIcon=new Icon(Properties.Resources.GreenAlarm, new Size(32,32));
        private static readonly Icon StatusAwareIcon = new Icon(Properties.Resources.OrangeAlarm, new Size(32, 32));
        private static readonly Icon StatusEmptyIcon = new Icon(Properties.Resources.RedAlarm, new Size(32, 32));

        public static implicit operator MachineStatusView(MachineStatus source)
        {
            if (source == null) return null;
            var result = new MachineStatusView
                {
                    ID = source.Id,
                    ComponentName = source.Name,
                    IsConnected=source.IsConnected
                };

            int greenCans = source.CurrentStock[DrinkType.Green];
            if (greenCans > 5)
                result.GreenCans = StatusOkIcon;
            else if (greenCans > 0)
                result.GreenCans = StatusAwareIcon;
            else
                result.GreenCans = StatusEmptyIcon;

            int orangeCans = source.CurrentStock[DrinkType.Orange];
            if (orangeCans > 5)
                result.OrangeCans = StatusOkIcon;
            else if (orangeCans > 0)
                result.OrangeCans = StatusAwareIcon;
            else
                result.OrangeCans = StatusEmptyIcon;

            int redCans = source.CurrentStock[DrinkType.Red];
            if (redCans > 5)
                result.RedCans = StatusOkIcon;
            else if (redCans > 0)
                result.RedCans = StatusAwareIcon;
            else
                result.RedCans = StatusEmptyIcon;


            return result;
        }
    }
}