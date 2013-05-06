using System;
using System.Runtime.Serialization;

namespace ermeX.Exceptions
{
	public class ermeXSetupException:ermeXException
	{
		public ermeXSetupException(string message) : base(message)
		{
		}

		public ermeXSetupException(Exception innerException)
			: this("An exception ocurred while starting ermeX", innerException){}
		public ermeXSetupException(string message, Exception innerException) : base(message, innerException)
		{
		}

		public ermeXSetupException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}