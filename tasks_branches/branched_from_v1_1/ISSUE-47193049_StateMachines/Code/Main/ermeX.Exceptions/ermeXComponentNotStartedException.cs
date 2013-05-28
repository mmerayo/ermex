using System;

namespace ermeX.Exceptions
{
	public class ermeXComponentNotStartedException : ermeXComponentNotAvailableException
	{
		public ermeXComponentNotStartedException(Guid remoteComponentId)
			: base(remoteComponentId) { }
	}
}