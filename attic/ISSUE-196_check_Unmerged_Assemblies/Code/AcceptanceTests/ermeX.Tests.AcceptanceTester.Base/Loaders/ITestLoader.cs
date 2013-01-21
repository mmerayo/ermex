namespace ermeX.Tests.AcceptanceTester.Base.Loaders
{
    public interface ITestLoader
    {
        /// <summary>
        /// Starts the test and returns a value indicating whether the watcher is on or not
        /// </summary>
        /// <param name="isTestDirector"> </param>
        /// <param name="args"></param>
        /// <returns></returns>
        bool StartTest(bool isTestDirector, string[] args);
    }
}
