using System;
using System.Net.Sockets;
using Ninject;
using ermeX.Common;
using ermeX.DAL.Interfaces;
using ermeX.Domain.Connectivity;
using ermeX.Entities.Entities;

namespace ermeX.Domain.Implementations.Connectivity
{
	internal sealed class ConnectivityDetailsWritter : ICanUpdateConnectivityDetails
	{
		private IConnectivityDetailsDataSource Repository { get; set; }

		private Guid LocalComponentId { get { return Repository.LocalComponentId; } } //TODO: ISSUE-281 --> THE LOCALCOMPONENT TO BE INJECTED

		[Inject]
		public ConnectivityDetailsWritter(IConnectivityDetailsDataSource repository)
		{
			Repository = repository;
		}

		public void RemoveComponentDetails(Guid componentId)
		{
			Repository.RemoveByProperty("ServerId", componentId.ToString());
		}

		public void RemoveLocalComponentDetails()
		{
			Repository.RemoveByProperty("ServerId", LocalComponentId.ToString()); 
		}

		public ConnectivityDetails CreateLocalComponentConnectivityDetails(ushort port, bool asLocal = true)
		{
			var connectivityDetails = new ConnectivityDetails
			{
				ServerId = LocalComponentId,
				ComponentOwner = LocalComponentId,
				Ip = Networking.GetLocalhostIp(AddressFamily.InterNetwork),
				IsLocal = asLocal, //TODO: ISSUE-281: --> review this is valid
				Port = port
			};
			Repository.Save(connectivityDetails);

			return connectivityDetails;
		}
	}
}