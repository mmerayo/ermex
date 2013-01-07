// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Castle.DynamicProxy;
using ermeX.Common.ILGeneration;

namespace ermeX.Common
{
    public static class ObjectBuilder
    {
        public static TResult FromTypeName<TResult>(string fullName)
        {
            var type = TypesHelper.GetTypeFromDomain(fullName);
            return FromType<TResult>(type);
        }


        public static TResult FromType<TResult>(Type typeToBuild, params object[] args)
        {
            return ILHelper.CreateInstance<TResult>(typeToBuild, args);
        }

        public static TInterfaceResult CreateProxy<TInterfaceResult>(IInterceptor implementation)
        {
            if (implementation == null) throw new ArgumentNullException("implementation");
            var generator = new ProxyGenerator();
            var result = (TInterfaceResult) generator.CreateInterfaceProxyWithoutTarget(typeof (TInterfaceResult), implementation);

            if(result==null)
                throw new ApplicationException("Could not create interface of type " + typeof(TInterfaceResult).FullName);

            return result;
        }

        public static Type CreateProxyType<TInterfaceResult>(IInterceptor implementation)
        {
            if (implementation == null) throw new ArgumentNullException("implementation");
            var generator = new ProxyGenerator();
            return generator.CreateInterfaceProxyWithTarget(typeof (TInterfaceResult), implementation);
        }
    }
}