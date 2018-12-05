using Newtonsoft.Json;
using Nop.Plugin.Api.DTOs.Base;

namespace Nop.Plugin.Api.DTOs
{
    [JsonObject(Title = "attribute")]
    public class ProductItemAttributeDto : BaseDto
    {
        [JsonProperty("id")]
        public string IdAsString { get => Id.ToString(); set => Id = int.Parse(value); }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
