// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;

namespace ermeX.Bus.Synchronisation.Messages
{
    internal sealed class JoinRequestMessage : DialogMessageBase
    {
        //public JoinRequestMessage():base() //TODO: resolve restriction on deserializer when there is not an empty constructor
        //{}
        public JoinRequestMessage(Guid sourceComponentId, string sourceIp, int sourcePort)
            : base(sourceComponentId)
        {
            SourceIp = sourceIp;
            SourcePort = sourcePort;
        }

        public string SourceIp { get; private set; }
        public int SourcePort { get; set; }
    }
}