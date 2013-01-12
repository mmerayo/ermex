using System;
using System.Text;

namespace ermeX.Threading.Scheduling
{
    internal sealed class Job
    {
        private Job(){}

        public DateTime FireTime { get; private set; }

        public Action DoAction { get; private set; }
        public object Param { get; private set; }


        public static Job At(DateTime fireTime, Action doAction, object param = null)
        {
            return new Job {FireTime = fireTime.ToUniversalTime(), DoAction = doAction, Param = param};
        }

        public override string ToString()
        {
            return string.Format("{0}({1}) at {2}", DoAction != null ? DoAction.Method.Name : string.Empty, Param,
                                 FireTime.ToLocalTime().ToString("o"));
        }
    }
}
