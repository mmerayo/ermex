// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using ermeX.Bus.Interfaces.Attributes;
using ermeX.Interfaces;

namespace ermeX.Tests.SupportTypes.Services
{
    [ServiceContract("37319FE8-8785-4483-9598-11AB898A21CA")]
    public interface IServiceB: IService
    {
        [ServiceOperation("501C662B-71A0-49C7-BAAA-AAD91C1C5894")]
        void EmptyMethod();
    }
}