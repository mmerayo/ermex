// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using NHibernate;
using ermeX.Entities.Base;
using ermeX.Entities.Entities;

namespace ermeX.DAL.Interfaces
{
    /// <summary>
    /// Allow to exchange sessions between datasources
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <remarks>its meant for internal use only</remarks>
    internal interface IDataAccessUsable<TEntity> where TEntity : ModelBase
    {
        DataAccessOperationResult<TEntity> GetById(ISession session, int id);
        DataAccessOperationResult<IList<TEntity>> GetAll(ISession session, params Tuple<string, bool>[] sortByParams);
        int CountItems(ISession session,string propertyName, object propertyValue);
        int CountItems(ISession session,Tuple<string, object>[] arguments );
        DataAccessOperationResult<bool> Save(ISession session, TEntity entity);
        DataAccessOperationResult<bool> Save(ISession session, IEnumerable<TEntity> entities);

        TEntity GetItemByField<TFieldType>(ISession session, string propertyName, TFieldType propertyValue);
        TEntity GetItemByFields(ISession session, Tuple<string, object>[] arguments);
        IList<TEntity> GetItemsByFields(ISession session, params Tuple<string, object>[] arguments);
        IList<TEntity> GetItemsByField<TFieldType>(ISession session, string propertyName, TFieldType propertyValue);

        DataAccessOperationResult<bool> SaveFromOtherComponent(ISession session, TEntity entity,
                                                               Tuple<string, object>[] deterministicFilter);

        DataAccessOperationResult<bool> RemoveAll(ISession session);
    }

    internal interface IDataAccessUsableConnectivityDetails:IDataAccessUsable<ConnectivityDetails>
    {
        ConnectivityDetails GetByComponentId(ISession session,Guid componentId);
    }
}