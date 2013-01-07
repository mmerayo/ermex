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
using ermeX.Bus.Interfaces.Attributes;
using ermeX.Interfaces;

namespace ermeX.Tests.Acceptance.Dummy
{
    [ServiceContract("FD9A460C-F21F-4E50-92AE-AFE896C59A06")]
    public interface ITestService3 : IService
    {
        [ServiceOperation("E1C1C577-4FB5-48D7-B5FC-F9A5B84D31FF")]
        void EmptyMethod();

        [ServiceOperation("DD50D0A5-9E91-4E93-BCEF-75C4EB94C44F")]
        void EmptyMethodWithOneParameter(AcceptanceMessageType1 param1);

        [ServiceOperation("1E2CCCA1-D74B-4922-B82D-4E063066E119")]
        void EmptyMethodWithSeveralParameters(AcceptanceMessageType1 param1, AcceptanceMessageType2 param2);
    }
}
