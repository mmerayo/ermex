// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using ermeX.Bus.Interfaces.Attributes;
using ermeX.Interfaces;

namespace ermeX.Tests.WorldGateTests.Mock
{
    public enum EnumerationType
    {
        Value1,Value2
    }

    [ServiceContract("7DF58873-8822-4527-B67A-8014A0FC7CA5")]
    public interface ITestService3 : IService
    {
        [ServiceOperation("84E88870-4457-444D-839B-B3A2BBCB7C33")]
        Guid ReturnMethodWithSeveralParametersValueTypes(Guid param1, DateTime param2);

        [ServiceOperation("13CAFB26-D17C-4CE7-8667-AF7787E41172")]
        MyCustomStruct ReturnCustomStructMethod(MyCustomStruct data);

        [ServiceOperation("A647070F-A071-4E2A-B101-180EF63147C6")]
        EnumerationType ReturnEnumMethod(EnumerationType data);
        
    }
}