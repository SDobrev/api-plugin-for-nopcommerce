﻿using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Plugin.Api.DataStructures;
using Nop.Services.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using Nop.Plugin.Api.DataStructures;
using Nop.Services.Stores;
using static Nop.Plugin.Api.Infrastructure.Constants;

namespace Nop.Plugin.Api.Services
{
    public class CategoryApiService : ICategoryApiService
    {
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<ProductCategory> _productCategoryMappingRepository;
        private readonly IRepository<LocalizedProperty> _localizedPropertyRepository;

        public CategoryApiService(IRepository<Category> categoryRepository,
            IRepository<ProductCategory> productCategoryMappingRepository,
            IRepository<LocalizedProperty> localizedPropertyRepository)
        {
            _categoryRepository = categoryRepository;
            _productCategoryMappingRepository = productCategoryMappingRepository;
            _localizedPropertyRepository = localizedPropertyRepository;
        }

        public IList<Category> GetCategories(IList<int> ids = null,
            DateTime? createdAtMin = null, DateTime? createdAtMax = null, DateTime? updatedAtMin = null, DateTime? updatedAtMax = null,
            int limit = Configurations.DefaultLimit, int page = Configurations.DefaultPageValue, int sinceId = Configurations.DefaultSinceId,
            int? productId = null, bool? publishedStatus = null, int? languageId = null)
        {
            var query = GetCategoriesQuery(createdAtMin, createdAtMax, updatedAtMin, updatedAtMax, publishedStatus, productId, ids);


            if (sinceId > 0)
            {
                query = query.Where(c => c.Id > sinceId);
            }
            var list = new ApiList<Category>(query, page - 1, limit);

            if (languageId.HasValue)
            {
                var localizedNames = _localizedPropertyRepository.Table.Where(x => x.LocaleKeyGroup == "Category" && languageId == languageId.Value);
                foreach (var cat in list)
                {
                    var localizedName = localizedNames.FirstOrDefault(x => x.EntityId == cat.Id);
                    if (localizedName != null)
                        cat.Name = localizedName.LocaleValue;
                }
            }

            return list;
            //return new ApiList<Category>(query, page - 1, limit);
        }

        public Category GetCategoryById(int id)
        {
            if (id <= 0)
                return null;

            var category = _categoryRepository.Table.FirstOrDefault(cat => cat.Id == id && !cat.Deleted);

            return category;
        }

        public int GetCategoriesCount(DateTime? createdAtMin = null, DateTime? createdAtMax = null,
            DateTime? updatedAtMin = null, DateTime? updatedAtMax = null,
            bool? publishedStatus = null, int? productId = null)
        {
            var query = GetCategoriesQuery(createdAtMin, createdAtMax, updatedAtMin, updatedAtMax,
                                           publishedStatus, productId);

            return query.Count();
        }

        private IQueryable<Category> GetCategoriesQuery(
            DateTime? createdAtMin = null, DateTime? createdAtMax = null, DateTime? updatedAtMin = null, DateTime? updatedAtMax = null,
            bool? publishedStatus = null, int? productId = null, IList<int> ids = null)
        {
            var query = _categoryRepository.Table;

            if (ids != null && ids.Count > 0)
            {
                query = query.Where(c => ids.Contains(c.Id));
            }

            if (publishedStatus != null)
            {
                query = query.Where(c => c.Published == publishedStatus.Value);
            }

            query = query.Where(c => !c.Deleted);

            if (createdAtMin != null)
            {
                query = query.Where(c => c.CreatedOnUtc > createdAtMin.Value);
            }

            if (createdAtMax != null)
            {

                query = query.Where(c => c.CreatedOnUtc < createdAtMax.Value);
            }

            if (updatedAtMin != null)
            {
                query = query.Where(c => c.UpdatedOnUtc > updatedAtMin.Value);
            }

            if (updatedAtMax != null)
            {
                query = query.Where(c => c.UpdatedOnUtc < updatedAtMax.Value);
            }

            if (productId != null)
            {
                var categoryMappingsForProduct = from productCategoryMapping in _productCategoryMappingRepository.Table
                                                 where productCategoryMapping.ProductId == productId
                                                 select productCategoryMapping;

                query = from category in query
                        join productCategoryMapping in categoryMappingsForProduct on category.Id equals productCategoryMapping.CategoryId
                        select category;
            }

            query = query.OrderBy(category => category.Id);

            return query;
        }
    }
}