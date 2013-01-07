// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using ermeX.Bus.Interfaces.Attributes;
using ermeX.Interfaces;

namespace ermeX.Tests.WorldGateTests.Mock
{
    [ServiceContract("4F8518F2-80E7-4CDF-A195-14F1AA6D184E")]
    public interface ITestService4<TTestType> : IService
    {
        [ServiceOperation("8658A023-8ABB-4D96-B49C-E5EE2FB196CE")]
        TTestType Echo(TTestType param1);
    }
}