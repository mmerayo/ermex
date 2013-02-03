using System;
using System.Runtime.InteropServices;
using ermeX.Tests.AcceptanceTester.Base.TestExecution;

namespace ermeX.Tests.AcceptanceTester.Base.Loaders
{
    public class TestLoaderBase
    {
        protected ITestExecutor _executor = null;
        private static bool isclosing = false;

        protected bool ConsoleCtrlCheck(CtrlTypes ctrlType)
        {
            if (!isclosing)
            {
                switch (ctrlType)
                {
                    case CtrlTypes.CTRL_C_EVENT:
                        isclosing = true;
                        Console.WriteLine("CTRL+C received!");
                        break;

                    case CtrlTypes.CTRL_BREAK_EVENT:
                        isclosing = true;
                        Console.WriteLine("CTRL+BREAK received!");
                        break;

                    case CtrlTypes.CTRL_CLOSE_EVENT:
                        isclosing = true;
                        Console.WriteLine("Program being closed!");
                        break;

                    case CtrlTypes.CTRL_LOGOFF_EVENT:
                    case CtrlTypes.CTRL_SHUTDOWN_EVENT:
                        isclosing = true;
                        Console.WriteLine("User is logging off!");
                        break;

                }
                if (isclosing && _executor != null)
                    _executor.Dispose();
            }
            return true;
        }

        [DllImport("Kernel32")]
        public static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);

        public delegate bool HandlerRoutine(CtrlTypes CtrlType);

        public enum CtrlTypes
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }
    }
}