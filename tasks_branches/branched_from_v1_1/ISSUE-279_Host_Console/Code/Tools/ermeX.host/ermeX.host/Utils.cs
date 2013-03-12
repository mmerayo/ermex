using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ermeX.Host
{
    /// <summary>
    /// Utility class.
    /// </summary>
    internal static class Utils
    {
        internal static Command GetCommand(string arg)
        {
            string[] components = arg.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

            if (components.Length < 1)
            {
                return null;
            }
            else
            {
                Command command = new Command();
                command.CommandString = components[0];
                command.ParameterString = components[1];
                return command;
            }

        }
    }
}
