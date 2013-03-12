using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace ermeX.Host
{
    internal class ConfigHandler
    {
        private string dllPath;
        internal ConfigHandler(string dll)
        {
            dllPath = dll;
        }

        internal void LaunchConfigurationMethod(string[] args)
        {
            Command typeCommand = Utils.GetCommand(args[1]);
            Command methodCommand = Utils.GetCommand(args[2]);

            if (typeCommand.CommandString.Equals("-startType", StringComparison.InvariantCultureIgnoreCase)
                && methodCommand.CommandString.Equals("-startMethod", StringComparison.InvariantCultureIgnoreCase))
            {
                // Load the Assembly.
                Assembly assembly = Assembly.LoadFrom(dllPath);

                Type type = assembly.GetType(typeCommand.ParameterString);
                object instance = Activator.CreateInstance(type);

                MethodInfo method = type.GetMethod(methodCommand.ParameterString);
                method.Invoke(instance, null);

            }
        }

    }
}
