﻿using EZNEW.Develop.CQuery;
using EZNEW.Develop.DataAccess;
using EZNEW.Develop.Domain.Aggregation;
using EZNEW.Develop.Domain.Repository.Event;
using EZNEW.Develop.Domain.Repository.Warehouse;
using EZNEW.Develop.Entity;
using EZNEW.Develop.UnitOfWork;
using EZNEW.Framework.Extension;
using EZNEW.Framework.IoC;
using EZNEW.Framework.Paging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZNEW.Develop.Domain.Repository
{
    public abstract class DefaultRelationRepository<First, Second, ET, DAI> : BaseRelationRepository<First, Second> where Second : IAggregationRoot<Second> where First : IAggregationRoot<First> where ET : BaseEntity<ET> where DAI : IDataAccess<ET>
    {
        IRepositoryWarehouse<ET, DAI> repositoryWarehouse = ContainerManager.Resolve<IRepositoryWarehouse<ET, DAI>>();

        static DefaultRelationRepository()
        {
            if (!ContainerManager.IsRegister<IRepositoryWarehouse<ET, DAI>>())
            {
                ContainerManager.Register<IRepositoryWarehouse<ET, DAI>, DefaultRepositoryWarehouse<ET, DAI>>();
            }
        }

        #region save

        /// <summary>
        /// save
        /// </summary>
        /// <param name="datas">datas</param>
        public sealed override void Save(IEnumerable<Tuple<First, Second>> datas)
        {
            SaveAsync(datas).Wait();
        }

        /// <summary>
        /// save async
        /// </summary>
        /// <param name="datas">datas</param>
        public sealed override async Task SaveAsync(IEnumerable<Tuple<First, Second>> datas)
        {
            var records = await ExecuteSaveAsync(datas).ConfigureAwait(false);
            if (records.IsNullOrEmpty())
            {
                return;
            }
            RepositoryEventBus.PublishSave<Tuple<First, Second>>(GetType(), datas);
            WorkFactory.RegisterActivationRecord(records.ToArray());
        }

        /// <summary>
        /// save by first type datas
        /// </summary>
        /// <param name="datas">datas</param>
        public sealed override void SaveByFirst(IEnumerable<First> datas)
        {
            var saveRecords = ExecuteSaveByFirstAsync(datas).Result;
            if (saveRecords.IsNullOrEmpty())
            {
                return;
            }
            RepositoryEventBus.PublishSave<First>(GetType(), datas);
            WorkFactory.RegisterActivationRecord(saveRecords.ToArray());
        }

        /// <summary>
        /// save by second type datas
        /// </summary>
        /// <param name="datas">datas</param>
        public sealed override void SaveBySecond(IEnumerable<Second> datas)
        {
            var saveRecords = ExecuteSaveBySecondAsync(datas).Result;
            if (saveRecords.IsNullOrEmpty())
            {
                return;
            }
            RepositoryEventBus.PublishSave<Second>(GetType(), datas);
            WorkFactory.RegisterActivationRecord(saveRecords.ToArray());
        }

        #endregion

        #region remove

        /// <summary>
        /// remove
        /// </summary>
        /// <param name="datas">datas</param>
        public sealed override void Remove(IEnumerable<Tuple<First, Second>> datas)
        {
            RemoveAsync(datas).Wait();
        }

        /// <summary>
        /// save async
        /// </summary>
        /// <param name="datas">datas</param>
        public sealed override async Task RemoveAsync(IEnumerable<Tuple<First, Second>> datas)
        {
            var records = await ExecuteRemoveAsync(datas).ConfigureAwait(false);
            if (records.IsNullOrEmpty())
            {
                return;
            }
            RepositoryEventBus.PublishRemove<Tuple<First, Second>>(GetType(), datas);
            WorkFactory.RegisterActivationRecord(records.ToArray());
        }

        /// <summary>
        /// remove by condition
        /// </summary>
        /// <param name="query">query</param>
        public sealed override void Remove(IQuery query)
        {
            RemoveAsync(query).Wait();
        }

        /// <summary>
        /// remove by condition
        /// </summary>
        /// <param name="query">query</param>
        public sealed override async Task RemoveAsync(IQuery query)
        {
            var record = await ExecuteRemoveAsync(query).ConfigureAwait(false);
            if (record == null)
            {
                return;
            }
            RepositoryEventBus.PublishRemove<Tuple<First, Second>>(GetType(), query);
            WorkFactory.RegisterActivationRecord(record);
        }

        /// <summary>
        /// remove by first datas
        /// </summary>
        /// <param name="datas">datas</param>
        public sealed override void RemoveByFirst(IEnumerable<First> datas)
        {
            var removeRecord = ExecuteRemoveByFirst(datas);
            if (removeRecord == null)
            {
                return;
            }
            RepositoryEventBus.PublishRemove<First>(GetType(), datas);
            WorkFactory.RegisterActivationRecord(removeRecord);
        }

        /// <summary>
        /// remove by first
        /// </summary>
        /// <param name="query">query</param>
        public sealed override void RemoveByFirst(IQuery query)
        {
            var removeQuery = CreateQueryByFirst(query);
            Remove(removeQuery);
        }

        /// <summary>
        /// remove by second datas
        /// </summary>
        /// <param name="datas">datas</param>
        public sealed override void RemoveBySecond(IEnumerable<Second> datas)
        {
            var removeRecord = ExecuteRemoveBySecond(datas);
            if (removeRecord == null)
            {
                return;
            }
            RepositoryEventBus.PublishRemove<Second>(GetType(), datas);
            WorkFactory.RegisterActivationRecord(removeRecord);
        }

        /// <summary>
        /// remove by first
        /// </summary>
        /// <param name="query">query</param>
        public sealed override void RemoveBySecond(IQuery query)
        {
            var removeQuery = CreateQueryBySecond(query);
            Remove(removeQuery);
        }

        #endregion

        #region query

        /// <summary>
        /// get relation data
        /// </summary>
        /// <param name="query">query</param>
        /// <returns></returns>
        public sealed override Tuple<First, Second> Get(IQuery query)
        {
            return GetAsync(query).Result;
        }

        /// <summary>
        /// get relation data
        /// </summary>
        /// <param name="query">query</param>
        /// <returns></returns>
        public sealed override async Task<Tuple<First, Second>> GetAsync(IQuery query)
        {
            return await ExecuteGetAsync(query).ConfigureAwait(false);
        }

        /// <summary>
        /// get relation data list
        /// </summary>
        /// <param name="query">query</param>
        /// <returns></returns>
        public sealed override List<Tuple<First, Second>> GetList(IQuery query)
        {
            return GetListAsync(query).Result;
        }

        /// <summary>
        /// get relation data list
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public sealed override async Task<List<Tuple<First, Second>>> GetListAsync(IQuery query)
        {
            return await ExecuteGetListAsync(query).ConfigureAwait(false);
        }

        /// <summary>
        /// get relation paging
        /// </summary>
        /// <param name="query">query</param>
        /// <returns></returns>
        public sealed override IPaging<Tuple<First, Second>> GetPaging(IQuery query)
        {
            return GetPagingAsync(query).Result;
        }

        /// <summary>
        /// get relation paging
        /// </summary>
        /// <param name="query">query</param>
        /// <returns></returns>
        public sealed override async Task<IPaging<Tuple<First, Second>>> GetPagingAsync(IQuery query)
        {
            return await ExecuteGetPagingAsync(query).ConfigureAwait(false);
        }

        /// <summary>
        /// get First by Second
        /// </summary>
        /// <param name="datas">second datas</param>
        /// <returns></returns>
        public sealed override List<First> GetFirstListBySecond(IEnumerable<Second> datas)
        {
            return GetFirstListBySecondAsync(datas).Result;
        }

        /// <summary>
        /// get First by Second
        /// </summary>
        /// <param name="datas">datas</param>
        /// <returns></returns>
        public sealed override async Task<List<First>> GetFirstListBySecondAsync(IEnumerable<Second> datas)
        {
            return await ExecuteGetFirstBySecondListAsync(datas).ConfigureAwait(false);
        }

        /// <summary>
        /// get Second by First
        /// </summary>
        /// <param name="datas">datas</param>
        /// <returns></returns>
        public sealed override List<Second> GetSecondListByFirst(IEnumerable<First> datas)
        {
            return GetSecondListByFirstAsync(datas).Result;
        }

        /// <summary>
        /// get Second by First
        /// </summary>
        /// <param name="datas">datas</param>
        /// <returns></returns>
        public sealed override async Task<List<Second>> GetSecondListByFirstAsync(IEnumerable<First> datas)
        {
            return await ExecuteGetSecondByFirstListAsync(datas).ConfigureAwait(false);
        }

        #endregion

        #region functions

        /// <summary>
        /// execute save
        /// </summary>
        /// <param name="datas">datas</param>
        /// <returns></returns>
        public virtual async Task<List<IActivationRecord>> ExecuteSaveAsync(IEnumerable<Tuple<First, Second>> datas)
        {
            if (datas.IsNullOrEmpty())
            {
                return null;
            }
            var entitys = datas.Select(c => CreateEntityByRelationData(c)).ToList();
            List<IActivationRecord> records = new List<IActivationRecord>(entitys.Count);
            foreach (var entity in entitys)
            {
                records.Add(await repositoryWarehouse.SaveAsync(entity).ConfigureAwait(false));
            }
            return records;
        }

        /// <summary>
        /// execute save by first datas
        /// </summary>
        /// <param name="datas">datas</param>
        /// <returns></returns>
        public virtual async Task<List<IActivationRecord>> ExecuteSaveByFirstAsync(IEnumerable<First> datas)
        {
            if (datas.IsNullOrEmpty())
            {
                return null;
            }
            var entitys = datas.Select(c => CreateEntityByFirst(c)).ToList();
            List<IActivationRecord> records = new List<IActivationRecord>(entitys.Count);
            foreach (var entity in entitys)
            {
                records.Add(await repositoryWarehouse.SaveAsync(entity).ConfigureAwait(false));
            }
            return records;
        }

        /// <summary>
        /// execute save by second datas
        /// </summary>
        /// <param name="datas">datas</param>
        /// <returns></returns>
        public virtual async Task<List<IActivationRecord>> ExecuteSaveBySecondAsync(IEnumerable<Second> datas)
        {
            if (datas.IsNullOrEmpty())
            {
                return null;
            }
            var entitys = datas.Select(c => CreateEntityBySecond(c)).ToList();
            List<IActivationRecord> records = new List<IActivationRecord>(entitys.Count);
            foreach (var entity in entitys)
            {
                records.Add(await repositoryWarehouse.SaveAsync(entity).ConfigureAwait(false));
            }
            return records;
        }

        /// <summary>
        /// execute remove
        /// </summary>
        /// <param name="datas">datas</param>
        /// <returns></returns>
        public virtual async Task<List<IActivationRecord>> ExecuteRemoveAsync(IEnumerable<Tuple<First, Second>> datas)
        {
            if (datas.IsNullOrEmpty())
            {
                return null;
            }
            var entitys = datas.Select(c => CreateEntityByRelationData(c)).ToList();
            List<IActivationRecord> records = new List<IActivationRecord>(entitys.Count);
            foreach (var entity in entitys)
            {
                records.Add(await repositoryWarehouse.RemoveAsync(entity).ConfigureAwait(false));
            }
            return records;
        }

        /// <summary>
        /// execute remove
        /// </summary>
        /// <param name="query">query</param>
        /// <returns></returns>
        public virtual async Task<IActivationRecord> ExecuteRemoveAsync(IQuery query)
        {
            return await repositoryWarehouse.RemoveAsync(query);
        }

        /// <summary>
        /// remove by first datas
        /// </summary>
        /// <param name="datas">datas</param>
        /// <returns></returns>
        public virtual IActivationRecord ExecuteRemoveByFirst(IEnumerable<First> datas)
        {
            if (datas.IsNullOrEmpty())
            {
                return null;
            }
            var query = CreateQueryByFirst(datas);
            var removeRecord = repositoryWarehouse.RemoveAsync(query).Result;
            return removeRecord;
        }

        /// <summary>
        /// remove by second datas
        /// </summary>
        /// <param name="datas">datas</param>
        /// <returns></returns>
        public virtual IActivationRecord ExecuteRemoveBySecond(IEnumerable<Second> datas)
        {
            if (datas.IsNullOrEmpty())
            {
                return null;
            }
            var query = CreateQueryBySecond(datas);
            var removeRecord = repositoryWarehouse.RemoveAsync(query).Result;
            return removeRecord;
        }

        /// <summary>
        /// create data by first
        /// </summary>
        /// <param name="data">data</param>
        /// <returns></returns>
        public virtual ET CreateEntityByFirst(First data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// create data by second
        /// </summary>
        /// <param name="datas">data</param>
        /// <returns></returns>
        public virtual ET CreateEntityBySecond(Second datas)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// get relation data
        /// </summary>
        /// <param name="query">query</param>
        /// <returns></returns>
        public virtual async Task<Tuple<First, Second>> ExecuteGetAsync(IQuery query)
        {
            var entity = await repositoryWarehouse.GetAsync(query).ConfigureAwait(false);
            return CreateRelationDataByEntity(entity);
        }

        /// <summary>
        /// get relation data list
        /// </summary>
        /// <param name="query">query</param>
        /// <returns></returns>
        public virtual async Task<List<Tuple<First, Second>>> ExecuteGetListAsync(IQuery query)
        {
            var entityList = await repositoryWarehouse.GetListAsync(query).ConfigureAwait(false);
            if (entityList.IsNullOrEmpty())
            {
                return new List<Tuple<First, Second>>(0);
            }
            return entityList.Select(c => CreateRelationDataByEntity(c)).ToList();
        }

        /// <summary>
        /// get relation data paging
        /// </summary>
        /// <param name="query">query</param>
        /// <returns></returns>
        public virtual async Task<IPaging<Tuple<First, Second>>> ExecuteGetPagingAsync(IQuery query)
        {
            var entityPaging = await repositoryWarehouse.GetPagingAsync(query).ConfigureAwait(false);
            if (entityPaging.IsNullOrEmpty())
            {
                return Paging<Tuple<First, Second>>.EmptyPaging();
            }
            var datas = entityPaging.Select(c => CreateRelationDataByEntity(c));
            return new Paging<Tuple<First, Second>>(entityPaging.Page, entityPaging.PageSize, entityPaging.TotalCount, datas);
        }

        /// <summary>
        /// get First by Second
        /// </summary>
        /// <param name="datas">datas</param>
        /// <returns></returns>
        public virtual async Task<List<First>> ExecuteGetFirstBySecondListAsync(IEnumerable<Second> datas)
        {
            if (datas.IsNullOrEmpty())
            {
                return new List<First>(0);
            }
            var query = CreateQueryBySecond(datas);
            var entitys = await repositoryWarehouse.GetListAsync(query).ConfigureAwait(false);
            return entitys.Select(c => CreateRelationDataByEntity(c).Item1).ToList();
        }

        /// <summary>
        /// get Second by First
        /// </summary>
        /// <param name="datas">datas</param>
        /// <returns></returns>
        public virtual async Task<List<Second>> ExecuteGetSecondByFirstListAsync(IEnumerable<First> datas)
        {
            if (datas.IsNullOrEmpty())
            {
                return new List<Second>(0);
            }
            var query = CreateQueryByFirst(datas);
            var entitys = await repositoryWarehouse.GetListAsync(query).ConfigureAwait(false);
            return entitys.Select(c => CreateRelationDataByEntity(c).Item2).ToList();
        }

        /// <summary>
        /// create query by first type datas
        /// </summary>
        /// <param name="datas">datas</param>
        /// <returns></returns>
        public abstract IQuery CreateQueryByFirst(IEnumerable<First> datas);

        /// <summary>
        /// create query by first type datas query object
        /// </summary>
        /// <param name="query">query</param>
        /// <returns></returns>
        public abstract IQuery CreateQueryByFirst(IQuery query);

        /// <summary>
        /// create query by second type datas
        /// </summary>
        /// <param name="datas">datas</param>
        /// <returns></returns>
        public abstract IQuery CreateQueryBySecond(IEnumerable<Second> datas);

        /// <summary>
        /// create query by second type datas query object
        /// </summary>
        /// <param name="query">query</param>
        /// <returns></returns>
        public abstract IQuery CreateQueryBySecond(IQuery query);

        /// <summary>
        /// create entity by relation data
        /// </summary>
        /// <param name="data">data</param>
        /// <returns></returns>
        public abstract ET CreateEntityByRelationData(Tuple<First, Second> data);

        /// <summary>
        /// create relation data by entity
        /// </summary>
        /// <param name="entity">entity</param>
        /// <returns></returns>
        public abstract Tuple<First, Second> CreateRelationDataByEntity(ET entity);

        #endregion
    }
}