using System;

namespace ermeX.Exceptions
{
	[Serializable]
	public class ermeXComponentNotStartedException : ermeXComponentNotAvailableException
	{
		public ermeXComponentNotStartedException(Guid remoteComponentId)
			: base(remoteComponentId) { }
	}
}