using System;

namespace ermeX.Tests.AcceptanceTester.Base.TestExecution
{
    public interface ITestExecutor:IDisposable
    {
        void PerformTasks();
    }
}