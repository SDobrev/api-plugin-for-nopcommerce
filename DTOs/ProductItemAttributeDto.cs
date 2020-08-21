using Newtonsoft.Json;
using Nop.Plugin.Api.DTO.Base;

namespace Nop.Plugin.Api.DTO
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
