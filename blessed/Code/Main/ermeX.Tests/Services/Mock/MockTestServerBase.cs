// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;

namespace ermeX.Tests.Services.Mock
{
    internal abstract class MockTestServerBase<TMessage> : IDisposable
    {
        protected MockTestServerBase()
        {
            ReceivedMessages = new List<TMessage>();
        }

        public List<TMessage> ReceivedMessages { get; set; }


        #region IDisposable Members

        public virtual void Dispose()
        {
        }

        #endregion
    }
}