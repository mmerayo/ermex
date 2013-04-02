using System;
using NHibernate;

namespace ermeX.DAL.DataAccess.UoW
{
	internal static class UnitOfWork
	{
		private static IUnitOfWorkFactory _unitOfWorkFactory;

		public static IUnitOfWork Start()
		{
			if (CurrentUnitOfWork != null)
				throw new InvalidOperationException("You cannot start more than one unit of work at the same time.");

			var unitOfWork = _unitOfWorkFactory.Create();
			CurrentUnitOfWork = unitOfWork;
			return unitOfWork;
		}

		public const string CurrentUnitOfWorkKey = "ermeX.UnitOfWork";

		private static IUnitOfWork CurrentUnitOfWork
		{
			get { return Local.Data[CurrentUnitOfWorkKey] as IUnitOfWork; }
			set { Local.Data[CurrentUnitOfWorkKey] = value; }
		}

		public static IUnitOfWork Current
		{
			get
			{
				var unitOfWork = CurrentUnitOfWork;
				if (unitOfWork == null)
					throw new InvalidOperationException("You are not in a unit of work");
				return unitOfWork;
			}
		}

		public static bool IsStarted
		{
			get { return CurrentUnitOfWork != null; }
		}

		public static ISession CurrentSession
		{
			get { return _unitOfWorkFactory.CurrentSession; }
			internal set { _unitOfWorkFactory.CurrentSession = value; }
		}

		public static void DisposeUnitOfWork(UnitOfWorkImplementor adapter)
		{
			CurrentUnitOfWork = null;
		}
	}
}