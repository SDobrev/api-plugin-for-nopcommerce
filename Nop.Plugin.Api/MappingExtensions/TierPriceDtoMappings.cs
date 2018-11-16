using Nop.Core.Domain.Catalog;
using Nop.Plugin.Api.AutoMapper;
using Nop.Plugin.Api.DTOs.Products;

namespace Nop.Plugin.Api.MappingExtensions
{
    public static class TierPriceDtoMappings
    {
        public static TierPriceDto ToDto(this TierPrice tierPrice)
        {
            return tierPrice.MapTo<TierPrice, TierPriceDto>();
        }
    }
}