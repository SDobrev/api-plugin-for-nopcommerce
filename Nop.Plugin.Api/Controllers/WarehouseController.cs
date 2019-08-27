using Nop.Plugin.Api.APIAuth;
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.DTOs.Warehouses;
using Nop.Plugin.Api.Helpers;
using Nop.Plugin.Api.JSON.ActionResults;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using System.Collections.Generic;
using System.Net;

namespace Nop.Plugin.Api.Controllers
{
    using DTOs.Errors;
    using JSON.Serializers;
    using Microsoft.AspNetCore.Mvc;

    [BasicAuthentication]
    public class WarehouseController : BaseApiController
    {
        private readonly IShippingService _shippingService;
        private readonly IDTOHelper _dtoHelper;

        public WarehouseController(IJsonFieldsSerializer jsonFieldsSerializer,
            IAclService aclService,
            ICustomerService customerService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IDiscountService discountService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            IPictureService pictureService,
            IDTOHelper dtoHelper,
            IShippingService shippingService)
            : base(jsonFieldsSerializer,
                  aclService,
                  customerService,
                  storeMappingService,
                  storeService,
                  discountService,
                  customerActivityService,
                  localizationService,
                  pictureService)
        {
            _dtoHelper = dtoHelper;
            _shippingService = shippingService;
        }

        /// <summary>
        /// Retrieve all warehouses
        /// </summary>
        /// <param name="fields">Fields from the warehouse you want your json to contain</param>
        /// <response code="200">OK</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [Route("/api/warehouses")]
        [ProducesResponseType(typeof(WarehousesRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public IActionResult GetAllWarehouses(string fields = "")
        {
            var allWarehouses = _shippingService.GetAllWarehouses();
        
            var warehousesAsDto = new List<WarehouseDto>();

            foreach (var warehouse in allWarehouses)
            {
                var warehouseDto = _dtoHelper.PrepareWarehouseDto(warehouse);

                warehousesAsDto.Add(warehouseDto);
            }

            var warehousesRootObject = new WarehousesRootObject()
            {
                Warehouses = warehousesAsDto
            };

            var json = JsonFieldsSerializer.Serialize(warehousesRootObject, fields);

            return new RawJsonActionResult(json);
        }
    }
}
