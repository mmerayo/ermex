using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Moq;
using NUnit.Framework;
using ermeX.DAL.DataAccess.UoW;

namespace ermeX.Tests.DAL
{
	[TestFixture]
	public class UnitOfWorkUnitTests
	{
		private Mock<IUnitOfWorkFactory> _mockFactory;
		private Mock<IUnitOfWork> _mockUnitOfWork;

		[SetUp]
		public void SetupContext()
		{
			_mockFactory = new Mock<IUnitOfWorkFactory>();
			_mockUnitOfWork = new Mock<IUnitOfWork>();

			SetNonPublicVariable("_unitOfWorkFactory", _mockFactory.Object);
			_mockFactory.Setup(x => x.Create()).Returns(_mockUnitOfWork.Object).Verifiable();
		}

		[TearDown]
		public void TearDownContext()
		{
			_mockFactory.VerifyAll();
			_mockUnitOfWork.VerifyAll();

			SetNonPublicVariable("_innerUnitOfWork", null);
		}

		private static void SetNonPublicVariable(string vbleName, object newValue)
		{
			var fieldInfo = typeof (UnitOfWork).GetField(vbleName,
			                                             BindingFlags.Static | BindingFlags.SetField | BindingFlags.NonPublic);
			fieldInfo.SetValue(null, newValue);
		}

		[Test]
		public void CanStart_WhenStartedAlready()
		{
			UnitOfWork.Start();
			Assert.Throws<InvalidOperationException>(() => UnitOfWork.Start());
		}

		[Test]
		public void CanAccessCurrent()
		{
			IUnitOfWork uow = UnitOfWork.Start();
			var current = UnitOfWork.Current;
			Assert.AreSame(uow, current);
		}

		[Test]
		public void NotStartedUnitOfWork_throws()
		{
			Assert.Throws<InvalidOperationException>(() =>
				{
					var a=UnitOfWork.Current;
				});
		}

		[Test]
		public void IsStarted_IsAssigned()
		{
			Assert.IsFalse(UnitOfWork.IsStarted);

			var uow = UnitOfWork.Start();
			Assert.IsTrue(UnitOfWork.IsStarted);
		}
	}
}
