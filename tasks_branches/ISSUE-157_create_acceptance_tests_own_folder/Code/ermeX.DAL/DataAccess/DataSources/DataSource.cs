// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using Remotion.Linq.Utilities;
using ermeX.ConfigurationManagement.Settings;
using ermeX.DAL.DataAccess.Helpers;
using ermeX.DAL.Interfaces;
using ermeX.Entities.Base;

namespace ermeX.DAL.DataAccess.DataSources
{
    /// <summary>
    ///   Base class for each data source
    /// </summary>
    /// <typeparam name="TEntity"> </typeparam>
    internal abstract class DataSource<TEntity> : IDataSource<TEntity> where TEntity : ModelBase
    {
        /// <summary>
        /// </summary>
        /// <param name="settings"> </param>
        /// <param name="ownerComponentId"> prevents several components running on the same db </param>
        protected DataSource(IDalSettings settings, Guid ownerComponentId,IDataAccessExecutor dataAccessExecutor)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            if (dataAccessExecutor == null) throw new ArgumentNullException("dataAccessExecutor");
            DataAccessSettings = settings;

            LocalComponentId = ownerComponentId;
            DataAccessExecutor = dataAccessExecutor;
        }

        protected internal IDalSettings DataAccessSettings { get; private set; }

        protected abstract string TableName { get; }

        #region IDataSource<TEntity> Members

        public Guid LocalComponentId { get; protected set; }
        public IDataAccessExecutor DataAccessExecutor { get; set; }



        /// <summary>
        ///   Saves an entity from other component
        /// </summary>
        /// <param name="entity"> </param>
        /// <param name="deterministicFilter"> </param>
        public virtual bool SaveFromOtherComponent(TEntity entity, Tuple<string, object> deterministicFilter)
        {
            return SaveFromOtherComponent(entity, new[] { deterministicFilter });
        }

        public virtual bool SaveFromOtherComponent(TEntity entity, Tuple<string, object>[] deterministicFilter)
        {
            DataAccessOperationResult<bool> dataAccessOperationResult = DataAccessExecutor.Perform(session => SaveFromOtherComponent(session, entity, deterministicFilter));
            if(!dataAccessOperationResult.Success)
                throw new DataException("Could not commit the data access operation SaveFromOtherComponent");
            return dataAccessOperationResult.ResultValue;
        }

        public  DataAccessOperationResult<bool> SaveFromOtherComponent(ISession session, TEntity entity, Tuple<string, object>[] deterministicFilter)
        {
            CleanExternalData(entity);

            entity.ComponentOwner = LocalComponentId;
            var isNew = CountItems(session,deterministicFilter) == 0;
            if (isNew)
            {
                entity.Id = 0;
            }
            else
            {
                TEntity existingEntity = GetItemByFields(session, deterministicFilter);
                UpdateWhenExternal(entity, existingEntity);
                session.Evict(existingEntity);
            }
            Save(session,new []{entity});
            return new DataAccessOperationResult<bool>(){ResultValue=isNew, Success=true};
        }

        

        /// <summary>
        /// Cleans the data that comes form an external entity
        /// </summary>
        /// <param name="externalEntity"></param>
        /// <returns></returns>
        protected virtual void CleanExternalData(TEntity externalEntity)
        {
        }

        protected virtual void UpdateWhenExternal(TEntity entity, TEntity existingEntity)
        {
            entity.Id = existingEntity.Id;
        }

        /// <summary>
        ///   Saves an entity
        /// </summary>
        /// <param name="entity"> </param>
        public virtual void Save(TEntity entity)
        {
            Save(new[] {entity});
        }

        /// <summary>
        ///   Saves a set of entities
        /// </summary>
        /// <param name="entities"> </param>
        public virtual void Save(IEnumerable<TEntity> entities)
        {
            DataAccessOperationResult<bool> dataAccessOperationResult = DataAccessExecutor.Perform(session => Save(session, entities));
            //if (!dataAccessOperationResult.Success)
            //    throw new DataException("Could not commit the data access operation Save");
        }


        /// <summary>
        ///   Removes all entities from the datasource
        /// </summary>
        public virtual void RemoveAll()
        {
           if(! DataAccessExecutor.Perform(RemoveAll).Success) 
               throw new DataException("could not perform RemoveAll");
        }

        public DataAccessOperationResult<bool> RemoveAll(ISession session)
        {
            var result = GetAll(session);
            Remove(session, result.ResultValue);
            return new DataAccessOperationResult<bool>() {Success = true};
        }


        /// <summary>
        ///   Removes a list of entitities from the datasource
        /// </summary>
        /// <param name="entities"> </param>
        public virtual void Remove(IList<TEntity> entities)
        {
            if (!DataAccessExecutor.Perform(session => this.Remove(session, entities)).Success)
                throw new DataException("could not perform Remove");
        }

        /// <summary>
        ///   Removes an entity
        /// </summary>
        /// <param name="entity"> </param>
        public virtual void Remove(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
#if DEBUG
            if (entity.ComponentOwner != LocalComponentId)
                throw new InvalidOperationException("Attempt to remove an entity from a different component: " +
                                                    entity.ComponentOwner + " local component: " + LocalComponentId);
#endif
            Remove(new[] {entity});
        }
        /// <summary>
        ///   Removes a list of entitities from the datasource
        /// </summary>
        /// <param name="session"> </param>
        /// <param name="entities"> </param>
        protected DataAccessOperationResult<IList<TEntity>> Remove(ISession session, IList<TEntity> entities)
        {
            if (entities == null) throw new ArgumentNullException("entities");
         
            var result=new DataAccessOperationResult<IList<TEntity>>();
   
            foreach (var entity in entities) //TODO: Transaction here
                session.Delete(entity);
            result.ResultValue = entities;
            result.Success = true;
            return result;
        }

        

        /// <summary>
        ///   Removes an entity by a filter
        /// </summary>
        /// <param name="propertyName"> </param>
        /// <param name="propertyValue"> </param>
        public virtual void RemoveByProperty(string propertyName, string propertyValue)
        {
            if (string.IsNullOrEmpty(propertyName)) throw new ArgumentNullException("propertyName");
            if (propertyValue == null) throw new ArgumentNullException("propertyValue");

            if (!DataAccessExecutor.Perform(session =>
                {
                    session.Delete(string.Format("from {0} e where {1}='{2}' and ComponentOwner='{3}'",
                                                 typeof(TEntity).Name, propertyName,
                                                 propertyValue, LocalComponentId));
                    return new DataAccessOperationResult<bool>() {Success = true};
                }).Success)
                throw new DataException("could not perform Remove");
        }


        /// <summary>
        ///   Gets all the entitites
        /// </summary>
        /// <param name="sortByParams"> tuple name and sort ascending </param>
        /// <returns> </returns>
        public IList<TEntity> GetAll(params Tuple<string, bool>[] sortByParams)
        {
            var result = DataAccessExecutor.Perform(session => GetAll(session, sortByParams));
            if(!result.Success)
                throw new DataException("Couldnt perform the operation GetAll");

            return result.ResultValue;
        }

        public virtual DataAccessOperationResult<IList<TEntity>> GetAll(ISession session, params Tuple<string, bool>[] sortByParams)
        {
            var result = new DataAccessOperationResult<IList<TEntity>>();

            var criteria = session.CreateCriteria(typeof (TEntity));
            criteria.Add(Restrictions.Eq("ComponentOwner", LocalComponentId));
            foreach (var p in sortByParams)
            {
                var order = p.Item2 ? Order.Asc(p.Item1) : Order.Desc(p.Item1);
                criteria.AddOrder(order);
            }
            criteria.SetCacheable(false);
            result.ResultValue = criteria.List<TEntity>();
            result.Success = true;
            return result;
        }

        /// <summary>
        ///   Gets all the entitites from all the components
        /// </summary>
        /// <param name="sortByParams"> tuple name and srot ascending </param>
        /// <returns> </returns>
        public virtual IList<TEntity> GetAllAbsolute(params Tuple<string, bool>[] sortByParams)
        {
            var result = DataAccessExecutor.Perform(session =>
                {
                    var criteria = session.CreateCriteria(typeof(TEntity));
                    foreach (var p in sortByParams)
                    {
                        var order = p.Item2 ? Order.Asc(p.Item1) : Order.Desc(p.Item1);
                        criteria.AddOrder(order);
                    }
                    criteria.SetCacheable(false);
                    return new DataAccessOperationResult<IList<TEntity>>()
                        {ResultValue = criteria.List<TEntity>(), Success = true};
                });
            if (!result.Success)
                throw new DataException("Couldnt perform the operation GetAll");

            return result.ResultValue;
        }

       
        /// <summary>
        ///   Gets an item by a field value or null
        /// </summary>
        /// <typeparam name="TFieldType"> </typeparam>
        /// <param name="propertyName"> </param>
        /// <param name="propertyValue"> </param>
        /// <returns> </returns>
        public virtual TEntity GetItemByField<TFieldType>(string propertyName, TFieldType propertyValue)
        {
            return GetItemsByField(propertyName, propertyValue).SingleOrDefault();
        }

        /// <summary>
        ///   Gets a set of items that meets the filter condition
        /// </summary>
        /// <typeparam name="TFieldType"> </typeparam>
        /// <param name="propertyName"> </param>
        /// <param name="propertyValue"> </param>
        /// <returns> </returns>
        public virtual IList<TEntity> GetItemsByField<TFieldType>(string propertyName, TFieldType propertyValue)
        {
            return GetItemsByFields(new Tuple<string, object>(propertyName, propertyValue));
        }

        public virtual IList<TEntity> GetItemsByField<TFieldType>(ISession session, string propertyName, TFieldType propertyValue)
        {
            return GetItemsByFields(session,new Tuple<string, object>(propertyName, propertyValue));
        }

        public virtual TEntity GetItemByField<TFieldType>(ISession session, string propertyName, TFieldType propertyValue)
        {
            return GetItemsByFields(session,new Tuple<string, object>(propertyName, propertyValue)).SingleOrDefault();
        }

        /// <summary>
        ///   Gets an item that meets the filter condition
        /// </summary>
        /// <param name="arguments"> </param>
        /// <returns> </returns>
        public virtual TEntity GetItemByFields(Tuple<string, object>[] arguments)
        {
            return GetItemsByFields(arguments).SingleOrDefault();
        }

        /// <summary>
        ///   Gets a set of items that meets the filter condition
        /// </summary>
        /// <param name="arguments"> </param>
        /// <returns> </returns>
        public virtual IList<TEntity> GetItemsByFields(params Tuple<string, object>[] arguments)
        {
            var result = DataAccessExecutor.Perform(session =>
                {
                    IList<TEntity> itemsByFields = GetItemsByFields(session, arguments);
                    return new DataAccessOperationResult<IList<TEntity>>()
                        {ResultValue = itemsByFields, Success = true};
                });
            if (!result.Success)
                throw new DataException("Could not perform the operation GetAll");

            return result.ResultValue;
        }

        public TEntity GetItemByFields(ISession session, Tuple<string, object>[] arguments)
        {
            IList<TEntity> itemsByFields = GetItemsByFields(session, arguments);
            return itemsByFields.SingleOrDefault();
        }

        public IList<TEntity> GetItemsByFields(ISession session, params Tuple<string, object>[] arguments)
        {
            return GetItemsByFields(session, arguments, null);

        }

        public IList<TEntity> GetItemsByFields(ISession session, Tuple<string, object>[] equalArguments, Tuple<string, object>[] differentArguments)
        {
            if (session == null) throw new ArgumentNullException("session");
            ICriteria criteria = session.CreateCriteria(typeof(TEntity));

            criteria.Add(Restrictions.Eq("ComponentOwner", LocalComponentId));
            if (equalArguments != null)
                foreach (var arg in equalArguments)
                    criteria.Add(Restrictions.Eq(arg.Item1, arg.Item2));
            if (differentArguments != null)
                foreach (var arg in differentArguments)
                    criteria.Add(Expression.Not(Restrictions.Eq(arg.Item1, arg.Item2)));
            criteria.SetCacheable(false);

            IList<TEntity> result = criteria.List<TEntity>();

            return result;
        }
       


        /// <summary>
        ///   Gets an item by its id
        /// </summary>
        /// <param name="id"> </param>
        /// <returns> </returns>
        public virtual TEntity GetById(int id)
        {
            var result = DataAccessExecutor.Perform(session =>GetById(session,id));
            if (!result.Success)
                throw new DataException("Couldnt perform the operation GetById");

            return result.ResultValue;
        }

        public virtual DataAccessOperationResult<TEntity> GetById(ISession session, int id)
        {
            var resultValue = session.Get<TEntity>(id);
            
            if (resultValue!=null && resultValue.ComponentOwner != LocalComponentId)
                throw new InvalidOperationException(
                    "Attempt to retrieve one row from another component. Local component: " + LocalComponentId);
            return new DataAccessOperationResult<TEntity>()
            {
                Success = true,
                ResultValue = resultValue
            };
        }

        /// <summary>
        ///   counts items that meets the conditions
        /// </summary>
        /// <param name="propertyName"> </param>
        /// <param name="propertyValue"> </param>
        /// <returns> </returns>
        public virtual int CountItems(string propertyName, object propertyValue)
        {
            return CountItems(new[] {new Tuple<string, object>(propertyName, propertyValue)});
        }

        public virtual int CountItems(params Tuple<string, object>[] arguments)
        {
            var result = DataAccessExecutor.Perform(session =>
            {
                var resultValue = CountItems(session, arguments);
                return new DataAccessOperationResult<int>()
                {
                    Success = true,
                    ResultValue = resultValue
                };
            });
            if (!result.Success)
                throw new DataException("Couldnt perform the operation CountItems");

            return result.ResultValue;
        }

        /// <summary>
        ///   gets the max value of one property from a set of items
        /// </summary>
        /// <param name="propertyName"> </param>
        /// <returns> </returns>
        public int GetMax(string propertyName)
        {
            var result = DataAccessExecutor.Perform(session =>
            {
                var criteria = session.CreateCriteria(typeof(TEntity))
                   .Add(Restrictions.Eq("ComponentOwner", LocalComponentId))
                   .SetProjection(Projections.Max(propertyName));
                var max = criteria.List()[0] == null ? 0 : (Int32)criteria.UniqueResult();
                criteria.SetCacheable(false);

                return new DataAccessOperationResult<int>()
                {
                    Success = true,
                    ResultValue = max
                };
            });
            if (!result.Success)
                throw new DataException("Couldnt perform the operation GetAll");

            return result.ResultValue;

        }

        #endregion
        public DataAccessOperationResult<bool> Save( ISession session,TEntity entity)
        {
            return Save(session, new[] {entity});
        }
        public DataAccessOperationResult<bool> Save( ISession session,IEnumerable<TEntity> entities)
        {
            if (session == null) throw new ArgumentNullException("session");

            var result=new DataAccessOperationResult<bool>();

            foreach (var entity in entities)
            {
                if (entity.ComponentOwner == Guid.Empty)
                    throw new ArgumentEmptyException("entity.ComponentOwner");

                if (!BeforeUpdating(entity, session))
                    return result;
                
                if (entity.ComponentOwner == LocalComponentId)
                    entity.Version = DateTime.UtcNow.Ticks;


                entity.ComponentOwner = LocalComponentId;

                session.SaveOrUpdate(entity);
            }
            result.Success = true;
            return result;
        }

        public int CountItems(ISession session, string propertyName, object propertyValue)
        {
            return CountItems(session, new[] { new Tuple<string, object>(propertyName, propertyValue) });
        }

        public int CountItems( ISession session,Tuple<string, object>[] arguments)
        {
            if (session == null) throw new ArgumentNullException("session");
            var criteria = session.CreateCriteria(typeof(TEntity));
            foreach (var arg in arguments)
                criteria.Add(Restrictions.Eq(arg.Item1, arg.Item2));
            criteria.Add(Restrictions.Eq("ComponentOwner", LocalComponentId))
                .SetProjection(Projections.Count(arguments[0].Item1));
            var count = (Int32)criteria.UniqueResult();
            criteria.SetCacheable(false);
            return count;
        }

        /// <summary>
        ///   Implement to handle what happens before updating
        /// </summary>
        /// <param name="entity"> </param>
        /// <param name="session"> </param>
        /// <returns> false to cancel the operation </returns>
        protected virtual bool BeforeUpdating(TEntity entity, ISession session)
        {
            return true;
        }

       
    }
}