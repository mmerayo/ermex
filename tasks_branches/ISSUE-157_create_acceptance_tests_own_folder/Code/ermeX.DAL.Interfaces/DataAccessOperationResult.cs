// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
namespace ermeX.DAL.Interfaces
{
    internal class DataAccessOperationResult<TResult>
    {
        public DataAccessOperationResult(){}

        public DataAccessOperationResult(bool success, TResult resultValue=default(TResult))
        {
            Success = success;
            ResultValue = resultValue;
        }

        public bool Success { get; set; }
        public TResult ResultValue { get; set; }
    }
}