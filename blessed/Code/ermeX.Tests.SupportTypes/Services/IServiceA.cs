// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ermeX.Bus.Interfaces.Attributes;
using ermeX.Interfaces;

namespace ermeX.Tests.SupportTypes.Services
{
    [ServiceContract("9E05015E-5E92-4F97-B70E-F0F89DE37AE8")]
    public interface IServiceA: IService
    {
        [ServiceOperation("96FC8562-3600-4AC6-88F5-0E2D39810767")]
        void EmptyMethod();
    }
}
