// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Linq;

namespace ermeX.Bus.Interfaces.Attributes
{
    //TODO: STORE IN DB
    [AttributeUsage(AttributeTargets.Interface)]
    public class ServiceContractAttribute : Attribute
    {
        public ServiceContractAttribute(string guidServiceIdentifier) : this(guidServiceIdentifier, false)
        {
        }

        internal ServiceContractAttribute(string guidServiceIdentifier, bool isSystemService)
        {
            if (string.IsNullOrEmpty(guidServiceIdentifier))
                throw new ArgumentException("The serviceIdentifier is required");
            ServiceIdentifier = new Guid(guidServiceIdentifier);
            IsSystemService = isSystemService;
        }

        public Guid ServiceIdentifier { get; private set; }

        internal bool IsSystemService { get; private set; }

        internal static bool IsDefinedIn(Type interfaceType)
        {
            return interfaceType.GetCustomAttributes(typeof (ServiceContractAttribute), true).SingleOrDefault() != null;
        }

        internal static ServiceContractAttribute GetFromType(Type interfaceType)
        {
            return
                (ServiceContractAttribute)
                interfaceType.GetCustomAttributes(typeof (ServiceContractAttribute), true).Single();
        }

        internal static bool GetIsSystemService(Type interfaceType)
        {
            if (!IsDefined(interfaceType, typeof (ServiceContractAttribute)))
                throw new InvalidOperationException(
                    "The service interface type must be decorated with [ServiceContract]");

            var serviceContractAttribute =
                interfaceType.GetCustomAttributes(typeof (ServiceContractAttribute), true).Single() as
                ServiceContractAttribute;
            return serviceContractAttribute.IsSystemService;
        }
    }
}