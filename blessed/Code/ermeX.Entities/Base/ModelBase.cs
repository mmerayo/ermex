// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;

namespace ermeX.Entities.Base
{
    [Serializable]
    public abstract class ModelBase
    {
        public virtual int Id { get; set; }

        //prevents several compoenents using same db
        public virtual Guid ComponentOwner { get; set; }

        public virtual long Version { get; set; }
    }
}