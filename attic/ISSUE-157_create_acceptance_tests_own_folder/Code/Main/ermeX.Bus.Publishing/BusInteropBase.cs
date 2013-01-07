// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using Ninject;
using ermeX.Bus.Interfaces;

namespace ermeX.Bus.Publishing
{
    internal abstract class BusInteropBase
    {
        private readonly IEsbManager _bus;

        [Inject]
        protected BusInteropBase(IEsbManager bus)
        {
            if (bus == null) throw new ArgumentNullException("bus");

            _bus = bus;
        }


        protected internal IEsbManager Bus
        {
            get { return _bus; }
        }

        public void Start()
        {
            Bus.Start();
        }
    }
}