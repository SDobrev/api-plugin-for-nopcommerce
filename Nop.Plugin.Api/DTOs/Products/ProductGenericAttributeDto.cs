using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTOs.Products
{
    /// <summary>
    /// 
    /// </summary>
    public class ProductGenericAttributeDto
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("value")]
        public string Value { get; set; }
    }
}