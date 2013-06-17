using System;
using System.Runtime.Serialization;

namespace ermeX.Exceptions
{
	[Serializable]
	public class ermeXLocalComponentException : ermeXException
	{
		public ermeXLocalComponentException(string message)
			: base(message)
		{
		}

		public ermeXLocalComponentException(Exception innerException)
			: this("An exception ocurred while starting the ermeX local component ", innerException) { }
		public ermeXLocalComponentException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		public ermeXLocalComponentException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}