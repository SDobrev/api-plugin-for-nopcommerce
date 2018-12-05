using Newtonsoft.Json;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Plugin.Api.DTOs.Base;

namespace Nop.Plugin.Api.DTOs.Manufacturers
{
    [JsonObject(Title = "discount")]
    //[Validator(typeof(ProductDtoValidator))]
    public class DiscountManufacturerMappingDto : BaseDto
    {
        [JsonProperty("id")]
        public string IdAsString { get => Id.ToString(); set => Id = int.Parse(value); }

        /// <summary>
        /// Gets or sets the discount identifier
        /// </summary>
        [JsonProperty("discount_id")]
        public int DiscountId { get; set; }


        [JsonProperty("discount_name")]
        public string DiscountName { get; set; }
    }
}
