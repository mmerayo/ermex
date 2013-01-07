// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;

namespace ermeX.Exceptions
{
    public class ermeXServiceRequestReturnedErrors : Exception
    {
        public ermeXServiceRequestReturnedErrors(string interfaceName, string methodName) : this(interfaceName, methodName, null) { }
        public ermeXServiceRequestReturnedErrors(string interfaceName, string methodName, Exception innerException) : this(interfaceName, methodName, innerException, null) { }

        public ermeXServiceRequestReturnedErrors(string interfaceName, string methodName, Exception innerException, Guid? destinationComponent)
            : base(string.Format("Component:{2} Service: {0}.{1} is not defined locally", interfaceName, methodName,
            destinationComponent.HasValue ? destinationComponent.Value.ToString() : "Unspecified"),innerException)
        {
        }
    }
}