// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
namespace ermeX.Common.Caching
{
    public interface ICacheProvider
    {
        int SecondsToExpire { get; set; }
        void Empty();
        T Get<T>(string key);
        void Add<T>(string key, T value);
        void Add<T>(string key, T value, int cacheDurationSeconds);
        void Remove(string key);
        bool Contains(string preselectedEndPointsKey);
        void ClearCache();
    }
}