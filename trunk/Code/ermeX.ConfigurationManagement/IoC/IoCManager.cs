// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Diagnostics;
using Ninject;
using Ninject.Modules;

namespace ermeX.ConfigurationManagement.IoC
{
    /// <summary>
    ///   IoCMAnager
    /// </summary>
    internal static class IoCManager
    {
        private static readonly object syncRoot = new object();
        public static IKernel Kernel { get; private set; }
        public static event EventHandler IoCKernelChanged;

        public static void SetCurrentInjections(INinjectModule[] injectionModules)
            //TODO:There should be a discovering process for this definitions in case is extended
        {
            INinjectSettings settings = new NinjectSettings {InjectNonPublic = true};
            if (injectionModules != null)
            {
                SetKernel(new StandardKernel(settings, injectionModules));
            }
        }

        public static void Reset()
        {
            SetKernel(null);
        }

        private static void SetKernel(IKernel newKernel)
        {            
            if (Kernel != null)
            {
                lock (syncRoot)
                    if (Kernel != null)
                    {
                        Kernel.Dispose();
                        Kernel = null;
                    }
            }
            if (newKernel != null)
            {
                lock (syncRoot)
                    if (Kernel == null)
                    {
                        Kernel = newKernel;
                    }
                OnKernelChanged();
            }
        }

        private static void OnKernelChanged()
        {
            if (IoCKernelChanged != null)
                IoCKernelChanged(null, null);
        }

        public static void SetCurrentInjections(IKernel newKernel)
        {
            SetKernel(newKernel);
        }

        public static void InjectObject<TInterface>(TInterface implementor)
        {
            Kernel.Bind<TInterface>().ToConstant(implementor);
        }


        public static void InjectType<TInterface>(Type implementor)
        {
            Kernel.Bind<TInterface>().To(implementor);
        }
    }
}