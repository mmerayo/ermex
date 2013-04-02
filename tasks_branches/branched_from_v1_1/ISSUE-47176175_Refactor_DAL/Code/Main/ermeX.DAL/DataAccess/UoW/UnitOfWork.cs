using System;

namespace ermeX.DAL.DataAccess.UoW
{
	internal static class UnitOfWork
	{
		private static IUnitOfWorkFactory _unitOfWorkFactory;
		private static IUnitOfWork _innerUnitOfWork;

		public static IUnitOfWork Start()
		{
			if (_innerUnitOfWork != null)
				throw new InvalidOperationException("You cannot start more than one unit of work at the same time.");

			_innerUnitOfWork = _unitOfWorkFactory.Create();
			return _innerUnitOfWork;
		}

		public static IUnitOfWork Current
		{
			get
			{
				if (_innerUnitOfWork == null)
					throw new InvalidOperationException("You are not in a unit of work.");
				return _innerUnitOfWork;
			}
		}

		public static bool IsStarted
		{
			get { return _innerUnitOfWork != null; }
		}
	}
}