﻿using Newtonsoft.Json;
using Nop.Plugin.Api.Attributes;

namespace Nop.Plugin.Api.DTO.Images
{
    [ImageValidation]
    public class ImageMappingDto : ImageDto
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("product_id")]
        public int ProductId { get; set; }

        [JsonProperty("position")]
        public int Position { get; set; }

        [JsonProperty("picture_id")]
        public int PictureId { get; set; }
    }
}