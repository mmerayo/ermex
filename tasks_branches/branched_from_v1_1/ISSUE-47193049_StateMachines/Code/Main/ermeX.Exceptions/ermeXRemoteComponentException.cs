using System;
using System.Runtime.Serialization;

namespace ermeX.Exceptions
{
	[Serializable]
	public class ermeXRemoteComponentException : ermeXException
	{
		public ermeXRemoteComponentException(string message)
			: base(message)
		{
		}

		public ermeXRemoteComponentException(Exception innerException)
			: this("An exception ocurred while starting an ermeX remote component ", innerException) { }
		public ermeXRemoteComponentException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		public ermeXRemoteComponentException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}