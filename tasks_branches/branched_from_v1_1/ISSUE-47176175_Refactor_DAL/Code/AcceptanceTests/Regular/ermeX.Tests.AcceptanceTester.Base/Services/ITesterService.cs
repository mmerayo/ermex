using ermeX;

namespace ermeX.Tests.AcceptanceTester.Base.Services
{
    [ServiceContract("8D371CA5-70E2-4700-914A-432A74C63243")]
    public interface ITesterService : IService
    {
        [ServiceOperation("FE4C34D0-04E3-4932-B163-6FF28FFD12AE")]
        void SendMeTheResults();

        [ServiceOperation("52D01BD8-8BCC-4F4C-A0D6-8F49C0C8012F")]
        void CanStart();
        
    }
}
