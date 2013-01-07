// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using ermeX.Bus.Interfaces.Attributes;
using ermeX.Interfaces;
using ermeX.Tests.Common.Dummies;

namespace ermeX.Tests.WorldGateTests.Mock
{
    [ServiceContract("D21D9D92-3394-40B7-8E12-14625AF73C49")]
    public interface ITestService2 : IService
    {
        [ServiceOperation("AA86C421-1684-4A60-A39C-DC0535A320A8")]
        void EmptyMethod();

        [ServiceOperation("33015B8B-0BDE-45EE-9858-746EAC691BCF")]
        void EmptyMethodWithOneParameter(DummyDomainEntity param1);

        [ServiceOperation("789F7B3E-AD50-4A51-BACA-77994577C7F0")]
        void EmptyMethodWithSeveralParameters(DummyDomainEntity param1, DummyDomainEntity param2);
    }
}