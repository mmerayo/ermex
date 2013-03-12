using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ermeX.Host
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Invalid Args...");
                // TODO: Print usage
                return;
            }

            Command firstCommand = Utils.GetCommand(args[0]);

            switch (firstCommand.CommandString.ToLower())
            {
                case "-entryassembly":
                    {
                        if (args.Length < 3)
                        {
                            Console.WriteLine("Invalid Args...");
                            // TODO: Print usage
                            return;
                        }

                        ConfigHandler handler = new ConfigHandler(firstCommand.ParameterString);
                        handler.LaunchConfigurationMethod(args);
                    }
                    break;
                case "-installservice":
                    { 
                      // ToDo: Install..
                    }
                    break;
                case "-uninstallservice":
                    {
                        // ToDo: Uninstall..
                    }
                    break;
         
                default:
                    return ;
            }


        }


    }
}
