﻿using Microsoft.AspNetCore.Http;
using Nop.Plugin.Api.DTO.Products;
using Nop.Plugin.Api.Helpers;
using System.Collections.Generic;

namespace Nop.Plugin.Api.Validators
{
    public class ProductAttributeCombinationDtoValidator : BaseDtoValidator<ProductAttributeCombinationDto>
    {

        #region Constructors

        public ProductAttributeCombinationDtoValidator(IHttpContextAccessor httpContextAccessor, IJsonHelper jsonHelper, Dictionary<string, object> requestJsonDictionary) : base(httpContextAccessor, jsonHelper, requestJsonDictionary)
        {
            SetAttributesXmlRule();
            SetProductIdRule();
        }

        #endregion

        #region Private Methods

        private void SetAttributesXmlRule()
        {
            // Removed the validation to prevent retuning errors when creating products from AdminD
            // SetNotNullOrEmptyCreateOrUpdateRule(p => p.AttributesXml, "invalid attributes xml", "attributes_xml");
        }

        private void SetProductIdRule()
        {
            // Removed the validation to prevent retuning errors when creating products from AdminD
            //SetGreaterThanZeroCreateOrUpdateRule(p => p.ProductId, "invalid product id", "product_id");
        }

        #endregion

    }
}