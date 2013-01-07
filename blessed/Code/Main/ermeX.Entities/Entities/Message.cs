// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using ermeX.Entities.Base;
using ermeX.LayerMessages;

namespace ermeX.Entities.Entities
{
    //TODO: THERES LOTS OF CRAP HERE THAT need to be refactored
    [Serializable]
    internal abstract class Message : ModelBase
    {

        protected Message()
        {
        }

        protected Message(BusMessageData message)
        {
            if (message == null) throw new ArgumentNullException("message");
            if(message.Id<=0) throw new ArgumentOutOfRangeException("message","Save the message before");
            //TODO: REMOVE, AS the biz message should be handled only by the bizlayer
            BusMessageId = message.Id;
            TimePublishedUtc = message.CreatedTimeUtc;
        }

        protected abstract string TableName { get; }

        public virtual int BusMessageId { get; set; }

        public virtual DateTime TimePublishedUtc { get; set; }


        public virtual Guid PublishedBy
        {
            get { return ComponentOwner; }
            set { ComponentOwner = value; }
        }

        //TODO: to compenentData object when provider specified

        public virtual Guid PublishedTo { get; set; } //TODO: to compenentData object when provider specified


        protected string GetDbFieldName(string fieldName)
        {
            return string.Format("{0}_{1}", TableName, fieldName);
        }
    }
}