using System;
using Ninject;
using ermeX.DAL.Interfaces;

namespace ermeX.Domain.AppComponent.Impl
{
    internal sealed class ComponentsRegistrator : IRegisterComponents
    {
        private IAutoRegistration Repository { get; set; }

        [Inject]
        public ComponentsRegistrator(IAutoRegistration repository) //TODO: THIS REPOSITORY MIGHT BE REMOVED?
        {
            Repository = repository;
        }

        public bool CreateRemoteComponent(Guid remoteComponentId, string ip, int port)
        {
            return Repository.CreateRemoteComponentInitialSetOfData(remoteComponentId, ip, port);//TODO: BRING LOGIC HERE
        }

        public void CreateLocalComponent(int port)
        {
            Repository.CreateLocalSetOfData(port);
        }
    }
}