namespace ermeX.DAL.Interfaces.UnitOfWork
{
    internal interface ILocalData
    {
        object this[object key] { get; set; }
        int Count { get; }
        void Clear();
    }
}