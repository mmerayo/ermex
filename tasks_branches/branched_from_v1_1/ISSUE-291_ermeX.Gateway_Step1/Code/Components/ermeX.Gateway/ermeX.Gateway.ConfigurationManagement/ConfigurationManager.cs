using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ermeX.Gateway.Configuration;

namespace ermeX.Gateway.ConfigurationManagement
{
    public class ConfigurationManager
    {
        public static RestfulServiceDefinition GetConfiguration()
        {
            var config = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.None);
            var target = (ermeXGatewayConfiguration)config.GetSection("ermeXGatewayConfiguration");
            return target.RestfulServiceDefinition;
        }
    }
}
