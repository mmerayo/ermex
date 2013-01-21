using System;
using ermeX;
using ermeX.Tests.AcceptanceTester.Base.Messages;

namespace ermeX.Tests.AcceptanceTester.Base.Services
{
    [ServiceContract("F8489FCF-44C8-4C37-9D47-68035C60F9DA")]
    public interface IDirectorService : IService
    {
        [ServiceOperation("6ADA3B97-A8DB-4BF6-9B70-1A9A6C2CC71F")]
        void ImFinished(Guid componentName);

        [ServiceOperation("3ECF2120-384D-476A-9B90-11CF49928CFD")]
        void MyResults(Guid componentName, Results myResults);

        [ServiceOperation("A0443307-7A8F-4CBC-AAD1-517F8C90D2B4")]
        void ImReadyToGo(Guid componentName);
    }
}