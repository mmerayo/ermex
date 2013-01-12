using System;

using System.Configuration;
using System.Reflection;
using System.Runtime.Remoting;
using System.Threading;
using ermeX.Tests.AcceptanceTester.Base.Loaders;


namespace ermeX.Tests.AcceptanceTester
{
    internal class Program
    {

        private static void Main(string[] args)
        {
            //TODO: use all the tests created in the CI
            bool isDirector = bool.Parse(args[1]);

            string[] testParameters;
            var testLoader = CreateTestLoader(args, out testParameters);

            bool watcherOn = testLoader.StartTest(isDirector, testParameters);

#if DEBUG
            if (isDirector &&  watcherOn)
                Console.ReadKey();
            else
            {
                Thread.Sleep(TimeSpan.FromSeconds(5));
            }
#endif
        }

        private static ITestLoader CreateTestLoader(string[] args, out string[] testParameters)
        {
            string testLoaderTypeName = ConfigurationManager.AppSettings[args[0]];
            
            //Type type = Type.GetType(testLoaderTypeName,true);
            ObjectHandle objectHandle = Activator.CreateInstance("ermeX.Tests.AcceptanceTester.Tests", testLoaderTypeName);
            var testLoader = (ITestLoader)objectHandle.Unwrap();

            int length = args.Length - 2;
            testParameters = new string[length];
            Array.Copy(args, 2, testParameters, 0, length);
            return testLoader;
        }
    }


}
