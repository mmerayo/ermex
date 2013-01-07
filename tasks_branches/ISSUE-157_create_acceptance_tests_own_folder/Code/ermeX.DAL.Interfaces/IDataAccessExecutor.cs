// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using NHibernate;

namespace ermeX.DAL.Interfaces
{
    /// <summary>
    /// Performs database operations
    /// </summary>
    internal interface IDataAccessExecutor
    {
        DataAccessOperationResult<TResult> Perform<TResult>(Func<ISession, DataAccessOperationResult<TResult>> innerOperation);
        DataAccessOperationResult<TResult> Perform<TResult>(IEnumerable<Func<ISession, DataAccessOperationResult<TResult>>> innerChainOfOperations, 
            Func<ISession, DataAccessOperationResult<TResult>> operationExtractsResult = null);


    }
}