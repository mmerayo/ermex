// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;

namespace ermeX.Tests.Common.Dummies
{
    [Serializable]
    public class DummyDomainEntity
    {
        public bool Flag { get; set; }

        public Guid Id { get; set; }

        public byte[] FileBytes { get; set; }

        public List<DummyDomainEntity> Dummies { get; set; }
    }
}