// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ermeX.Exceptions
{
    public class ermeXComponentNotAvailableException : Exception
    {
        public ermeXComponentNotAvailableException(Guid remoteComponentId)
            : base(string.Format("The component: {0} is not available at the moment. Ensure the settings are correct",remoteComponentId)){    }
    }
}
