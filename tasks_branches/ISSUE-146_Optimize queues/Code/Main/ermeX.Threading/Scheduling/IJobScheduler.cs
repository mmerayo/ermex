using System;
using System.Collections.Generic;

namespace ermeX.Threading.Scheduling
{
    internal interface IJobScheduler:IDisposable
    {
        int Length { get; }
        bool IsWorking();
        IEnumerable<Job> GetJobs();
        void ScheduleJob(Job job);
    }
}