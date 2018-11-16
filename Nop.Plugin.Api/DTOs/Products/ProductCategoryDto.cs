using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTOs.Products
{
    public class ProductCategoryDto
    {
        [JsonProperty("product_id")]
        public int ProductId { get; set; }

        [JsonProperty("categori_id")]
        public int CategoriId { get; set; }
    }
}