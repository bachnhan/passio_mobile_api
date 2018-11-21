using AutoMapper;
using AutoMapper.QueryableExtensions;
using HmsService.Models;
using HmsService.Models.Entities.Services;
using SkyWeb.DatVM.Data;
using SkyWeb.DatVM.Mvc;
using SkyWeb.DatVM.Mvc.Autofac;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{

    public abstract class BaseSdkApi
    {
        
        public IConfigurationProvider AutoMapperConfig
        {
            get
            {
                return this.Service<IConfigurationProvider>();
            }
        }

        public T Service<T>() where T : class
        {
            return DependencyUtils.Resolve<T>();
        }

    }

    public abstract class BaseSdkApi<TEntity, TViewModel, TService> : BaseSdkApi
        where TEntity : class, IEntity, new()
        where TViewModel : BaseEntityViewModel<TEntity>, new()
        where TService : class, IBaseService<TEntity>
    {

        public TService BaseService
        {
            get
            {
                return this.Service<TService>();
            }
        }
        
        public virtual List<TViewModel> Get()
        {
            return this.BaseService.Get()
                .ProjectTo<TViewModel>(this.AutoMapperConfig)
                .ToList();
        }

        public virtual List<TViewModel> GetActive()
        {
            return this.BaseService.GetActive()
                .ProjectTo<TViewModel>(this.AutoMapperConfig)
                .ToList();
        }

        public virtual TViewModel Get<TKey>(TKey keyValue)
        {
            return this.BaseService.Get<TKey>(keyValue)
                .ToViewModel<TEntity, TViewModel>();
        }
        
        public virtual void Create(TViewModel model)
        {
            var entity = model.ToEntity();
            this.BaseService.Create(entity);
            this.BaseService.Save();
            model.CopyFromEntity(entity);
        }

        public virtual void Create(TViewModel model, string keyName)
        {
            var entity = model.ToEntity();
            this.BaseService.Create(entity);

            var entityKeyProperty = entity.GetType().GetProperty(keyName);
            var modelKeyProperty = model.GetType().GetProperty(keyName);

            modelKeyProperty.SetValue(model, entityKeyProperty.GetValue(entity));
        }

        public virtual void Edit<TKey>(TKey keyValue, TViewModel model)
        {
            var entity = this.BaseService.Get(keyValue);
            model.CopyToEntity(entity);
            this.BaseService.Update(entity);
        }

        public virtual void Delete<TKey>(TKey keyValue)
        {
            var entity = this.BaseService.Get(keyValue);
            this.BaseService.Delete(entity);
        }

        public virtual void DeleteByEntity(TEntity entity)
        {
            this.BaseService.Delete(entity);
        }

        public virtual void Activate<TKey>(TKey keyValue)
        {
            var entity = this.BaseService.Get(keyValue);
            this.BaseService.Activate(entity);
        }

        public virtual void Deactivate<TKey>(TKey keyValue)
        {
            var entity = this.BaseService.Get(keyValue);
            this.BaseService.Deactivate(entity);
        }

        #region Async Equivalent

        public async virtual Task<List<TViewModel>> GetAsync()
        {
            return await this.BaseService.Get()
                .ProjectTo<TViewModel>(this.AutoMapperConfig)
                .ToListAsync();
        }

        public async virtual Task<List<TViewModel>> GetActiveAsync()
        {
            return await this.BaseService.GetActive()
                .ProjectTo<TViewModel>(this.AutoMapperConfig)
                .ToListAsync();
        }

        public async virtual Task<TViewModel> GetAsync<TKey>(TKey keyValue)
        {
            return (await this.BaseService.GetAsync(keyValue))
                ?.ToViewModel<TEntity, TViewModel>();
        }
        
        public async virtual Task CreateAsync(TViewModel model)
        {
            var entity = model.ToEntity();
            await this.BaseService.CreateAsync(entity);
        }

        public async virtual Task CreateAsync(TViewModel model, string keyName)
        {
            var entity = model.ToEntity();
            await this.BaseService.CreateAsync(entity);

            var entityKeyProperty = entity.GetType().GetProperty(keyName);
            var modelKeyProperty = model.GetType().GetProperty(keyName);

            modelKeyProperty.SetValue(model, entityKeyProperty.GetValue(entity));
        }

        public async virtual Task EditAsync<TKey>(TKey keyValue, TViewModel model)
        {
            var entity = await this.BaseService.GetAsync(keyValue);
            model.CopyToEntity(entity);
            await this.BaseService.UpdateAsync(entity);
        }

        public async virtual Task DeleteAsync<TKey>(TKey keyValue)
        {
            var entity = await this.BaseService.GetAsync(keyValue);
            await this.BaseService.DeleteAsync(entity);
        }

        public virtual async Task ActivateAsync<TKey>(TKey keyValue)
        {
            var entity = this.BaseService.Get(keyValue);
            await this.BaseService.ActivateAsync(entity);
        }

        public virtual async Task DeactivateAsync<TKey>(TKey keyValue)
        {
            var entity = await this.BaseService.GetAsync(keyValue);
            await this.BaseService.DeactivateAsync(entity);
        }

        #endregion

    }

}