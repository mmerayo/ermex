using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Ploeh.AutoFixture.Kernel;

namespace ermeX.Tests.Common.AutoFixture
{
	public class IpAddressSpecimenBuilder : ISpecimenBuilder
	{
		private static readonly Random _random = new Random();

		public object Create(object request, ISpecimenContext context)
		{
			if (Equals(request, typeof(IPAddress)))
			{
				var bytes = new[] { (byte)_random.Next(0, 255), (byte)_random.Next(0, 255), (byte)_random.Next(0, 255), (byte)_random.Next(0, 255) };

				return new IPAddress(bytes);
			}

			return new NoSpecimen(request);
		}
	}
}
