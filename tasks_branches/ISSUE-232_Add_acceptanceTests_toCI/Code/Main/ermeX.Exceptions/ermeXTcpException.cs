using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ermeX.Exceptions
{
    public class ermeXTcpException:ermeXException
    {
        public ermeXTcpException(string message) : base(message)
        {
        }

        public ermeXTcpException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
