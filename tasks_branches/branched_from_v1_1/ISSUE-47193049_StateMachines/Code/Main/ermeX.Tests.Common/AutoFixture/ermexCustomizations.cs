using Ploeh.AutoFixture;

namespace ermeX.Tests.Common.AutoFixture
{
	public class ErmeXCustomizations : ICustomization
	{
		public void Customize(IFixture fixture)
		{
			fixture.Customize(new MultipleCustomization());

			fixture.Customizations.Add(new IpAddressSpecimenBuilder());
		}
	}
}