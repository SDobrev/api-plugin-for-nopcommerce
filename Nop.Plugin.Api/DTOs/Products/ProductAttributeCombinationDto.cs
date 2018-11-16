using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Api.DTOs.Products
{
    public class ProductAttributeCombinationDto
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the stock quantity
        /// </summary>
        [JsonProperty("stock_quantity")]
        public int StockQuantity { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to allow orders when out of stock
        /// </summary>
        [JsonProperty("allow_out_of_stock_orders")]
        public bool AllowOutOfStockOrders { get; set; }

        /// <summary>
        /// Gets or sets the SKU
        /// </summary>
        [JsonProperty("sku")]
        public string Sku { get; set; }

        /// <summary>
        /// Gets or sets the manufacturer part number
        /// </summary>
        [JsonProperty("manufacturer_part_number")]
        public string ManufacturerPartNumber { get; set; }

        /// <summary>
        /// Gets or sets the Global Trade Item Number (GTIN). These identifiers include UPC (in North America), EAN (in Europe), JAN (in Japan), and ISBN (for books).
        /// </summary>
        [JsonProperty("gtin")]
        public string Gtin { get; set; }

        /// <summary>
        /// Gets or sets the attribute combination price. This way a store owner can override the default product price when this attribute combination is added to the cart. For example, you can give a discount this way.
        /// </summary>
        [JsonProperty("overridden_price")]
        public decimal? OverriddenPrice { get; set; }

        /// <summary>
        /// Gets or sets the quantity when admin should be notified
        /// </summary>
        [JsonProperty("notify_admin_for_quantity_below")]
        public int NotifyAdminForQuantityBelow { get; set; }

        /// <summary>
        /// Gets or sets the records
        /// </summary>
        [JsonProperty("records")]
        public List<int> Records { get; set; }

        /// <summary>
        /// Admind's Combination ID!
        /// </summary>
        [JsonProperty("admind_combination_id")]
        public int AdmindCombinationId { get; set; }
    }
}