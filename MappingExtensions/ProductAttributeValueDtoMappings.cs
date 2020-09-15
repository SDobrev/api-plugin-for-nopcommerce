using Nop.Core.Domain.Catalog;
using Nop.Plugin.Api.AutoMapper;
using Nop.Plugin.Api.DTO.Products;

namespace Nop.Plugin.Api.MappingExtensions
{
    public static class ProductAttributeValueDtoMappings
    {
        public static ProductAttributeValue ToEntity(this ProductAttributeValueDto productAttributeDto)
        {
            return productAttributeDto.MapTo<ProductAttributeValueDto, ProductAttributeValue>();
        }
    }
}
