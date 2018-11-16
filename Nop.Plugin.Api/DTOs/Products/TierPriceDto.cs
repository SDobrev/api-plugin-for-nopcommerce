using Newtonsoft.Json;
using System;

namespace Nop.Plugin.Api.DTOs.Products
{
    /// <summary>
    /// 
    /// </summary>
    public class TierPriceDto
    {
        /// <summary>
        /// Gets or sets the tierprice identifier
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the store identifier (0 - all stores)
        /// </summary>
        [JsonProperty("store_id")]
        public int StoreId { get; set; }

        /// <summary>
        /// Gets or sets the customer role identifier
        /// </summary>
        [JsonProperty("customer_role_id")]
        public int? CustomerRoleId { get; set; }

        /// <summary>
        /// Gets or sets the quantity
        /// </summary>
        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the price
        /// </summary>
        [JsonProperty("price")]
        public decimal Price { get; set; }

        /// <summary>
        /// Gets or sets the start date and time in UTC
        /// </summary>
        [JsonProperty("start_datetimeutc")]
        public DateTime? StartDateTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets the end date and time in UTC
        /// </summary>
        [JsonProperty("end_datetimeutc")]
        public DateTime? EndDateTimeUtc { get; set; }
    }
}