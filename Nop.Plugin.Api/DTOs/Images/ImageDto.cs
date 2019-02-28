using Newtonsoft.Json;
using Nop.Plugin.Api.Attributes;

namespace Nop.Plugin.Api.DTOs.Images
{
    [ImageValidation]
    public class ImageDto
    {
        [JsonProperty("src")]
        public string Src { get; set; }

        [JsonProperty("attachment")]
        public string Attachment { get; set; }

        [JsonIgnore]
        public byte[] Binary { get; set; }

        [JsonIgnore]
        public string MimeType { get; set; }
        
        [JsonProperty("alt")]
        public string Alt { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("record_id")]
        public int RecordId { get; set; }
    }
}