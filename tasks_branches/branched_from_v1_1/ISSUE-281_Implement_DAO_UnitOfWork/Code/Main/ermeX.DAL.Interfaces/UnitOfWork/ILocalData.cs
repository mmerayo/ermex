namespace ermeX.DAL.Interfaces.UnitOfWork
{
    public interface ILocalData
    {
        object this[object key] { get; set; }
        int Count { get; }
        void Clear();
    }
}