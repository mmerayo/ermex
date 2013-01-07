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

namespace ermeX.DAL.Interfaces
{
    internal interface IDataSource<TEntity> where TEntity : ModelBase
    {
        Guid LocalComponentId { get; }

        /// <summary>
        ///   Saves an entity from other component
        /// </summary>
        /// <param name="entity"> </param>
        /// <param name="deterministicFilter"> </param>
        bool SaveFromOtherComponent(TEntity entity, Tuple<string, object> deterministicFilter);

        bool SaveFromOtherComponent(TEntity entity, Tuple<string, object>[] deterministicFilter);


        /// <summary>
        ///   Saves an entity
        /// </summary>
        /// <param name="entity"> </param>
        void Save(TEntity entity);

        /// <summary>
        ///   Saves a set of entities
        /// </summary>
        /// <param name="entities"> </param>
        void Save(IEnumerable<TEntity> entities);

        /// <summary>
        ///   Removes all entities from the datasource
        /// </summary>
        void RemoveAll();

        /// <summary>
        ///   Removes a list of entitities from the datasource
        /// </summary>
        /// <param name="entities"> </param>
        void Remove(IList<TEntity> entities);

        /// <summary>
        ///   Gets all the entitites
        /// </summary>
        /// <param name="sortByParams"> tuple name and srot ascending </param>
        /// <returns> </returns>
        IList<TEntity> GetAll(params Tuple<string, bool>[] sortByParams);

        /// <summary>
        ///   Gets all the entitites from any component
        /// </summary>
        /// <param name="sortByParams"> tuple name and srot ascending </param>
        /// <returns> </returns>
        IList<TEntity> GetAllAbsolute(params Tuple<string, bool>[] sortByParams);

        /// <summary>
        ///   Removes an entity
        /// </summary>
        /// <param name="entity"> </param>
        void Remove(TEntity entity);

        /// <summary>
        ///   Removes an entity by a filter
        /// </summary>
        /// <param name="propertyName"> </param>
        /// <param name="propertyValue"> </param>
        void RemoveByProperty(string propertyName, string propertyValue);

        /// <summary>
        ///   Gets an item by a field value or null
        /// </summary>
        /// <typeparam name="TFieldType"> </typeparam>
        /// <param name="propertyName"> </param>
        /// <param name="propertyValue"> </param>
        /// <returns> </returns>
        TEntity GetItemByField<TFieldType>(string propertyName, TFieldType propertyValue);

        /// <summary>
        ///   Gets a set of items that meets the filter condition
        /// </summary>
        /// <typeparam name="TFieldType"> </typeparam>
        /// <param name="propertyName"> </param>
        /// <param name="propertyValue"> </param>
        /// <returns> </returns>
        IList<TEntity> GetItemsByField<TFieldType>(string propertyName, TFieldType propertyValue);

        /// <summary>
        ///   Gets an item that meets the filter condition
        /// </summary>
        /// <param name="arguments"> </param>
        /// <returns> </returns>
        TEntity GetItemByFields(Tuple<string, object>[] arguments);

        /// <summary>
        ///   Gets a set of items that meets the filter condition
        /// </summary>
        /// <param name="arguments"> </param>
        /// <returns> </returns>
        IList<TEntity> GetItemsByFields(params Tuple<string, object>[] arguments);

        /// <summary>
        ///   Gets an item by its id
        /// </summary>
        /// <param name="id"> </param>
        /// <returns> </returns>
        TEntity GetById(int id);

        /// <summary>
        ///   counts items that meets the conditions
        /// </summary>
        /// <param name="propertyName"> </param>
        /// <param name="propertyValue"> </param>
        /// <returns> </returns>
        int CountItems(string propertyName, object propertyValue);

        int CountItems(params Tuple<string, object>[] arguments);

        /// <summary>
        ///   gets the max value of one property from a set of items
        /// </summary>
        /// <param name="propertyName"> </param>
        /// <returns> </returns>
        int GetMax(string propertyName);

        // bool HasRowAlready(TEntity entity);
        
    }
}