using System;
using System.Runtime.Serialization;

namespace ermeX.Exceptions
{
    public class ermeXException:Exception
    {
        public ermeXException(string message):base(message){}
        public ermeXException(string message, Exception innerException):base(message,innerException){}
        public ermeXException(SerializationInfo info, StreamingContext context) : base(info,context) { }
    }
}