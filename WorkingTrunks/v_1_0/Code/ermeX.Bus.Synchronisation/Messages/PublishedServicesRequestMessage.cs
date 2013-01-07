// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;

namespace ermeX.Bus.Synchronisation.Messages
{
    internal sealed class PublishedServicesRequestMessage : DialogMessageBase
    {
        public string InterfaceName { get; set; }
        public string MethodName { get; set; }

        public bool IsSingleResult
        {
            get { return !string.IsNullOrEmpty(InterfaceName) && !string.IsNullOrEmpty(MethodName); }
        }

        private PublishedServicesRequestMessage()
        {}

        public PublishedServicesRequestMessage(Guid sourceComponentId) : base(sourceComponentId)
        {
        }

        /// <summary>
        /// Use to request just one service definition
        /// </summary>
        /// <param name="sourceComponentId"></param>
        /// <param name="interfaceName"></param>
        /// <param name="methodName"></param>
        public PublishedServicesRequestMessage(Guid sourceComponentId, string interfaceName, string methodName) : this(sourceComponentId)
        {
            if (string.IsNullOrEmpty(interfaceName)) throw new ArgumentException("interfaceName");
            if (string.IsNullOrEmpty(methodName)) throw new ArgumentException("methodName");
            
            InterfaceName = interfaceName;
            MethodName = methodName;
            


        }
    }
}