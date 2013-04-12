namespace ermeX.DAL.DataAccess.UoW
{
	public interface ILocalData
	{
		object this[object key] { get; set; }
		int Count { get; }
		void Clear();
	}
}