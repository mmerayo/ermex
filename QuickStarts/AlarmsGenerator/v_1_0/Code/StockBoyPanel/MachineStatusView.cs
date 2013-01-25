using System;
using System.ComponentModel;
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

        public int GreenCans { get; set; }
        public int OrangeCans { get; set; }
        public int RedCans { get; set; }

        public static implicit operator MachineStatusView(MachineStatus source)
        {
            if (source == null) return null;
            return new MachineStatusView
                {
                    ID = source.Id,
                    ComponentName = source.Name,
                    GreenCans = source.CurrentStock[DrinkType.Green],
                    OrangeCans = source.CurrentStock[DrinkType.Orange],
                    RedCans = source.CurrentStock[DrinkType.Red]
                };
        }
    }
}