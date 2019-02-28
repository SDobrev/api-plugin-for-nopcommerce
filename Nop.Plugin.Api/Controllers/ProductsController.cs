using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Discounts;
using Nop.Plugin.Api.APIAuth;
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.Constants;
using Nop.Plugin.Api.Delta;
using Nop.Plugin.Api.DTOs.Images;
using Nop.Plugin.Api.DTOs.Products;
using Nop.Plugin.Api.Factories;
using Nop.Plugin.Api.Helpers;
using Nop.Plugin.Api.JSON.ActionResults;
using Nop.Plugin.Api.ModelBinders;
using Nop.Plugin.Api.Models.ProductsParameters;
using Nop.Plugin.Api.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Nop.Plugin.Api.Controllers
{
    using DTOs.Errors;
    using JSON.Serializers;
    using Microsoft.AspNetCore.Mvc;

    [BasicAuthentication]
    public class ProductsController : BaseApiController
    {
        private readonly IProductApiService _productApiService;
        private readonly IProductService _productService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IFactory<Product> _factory;
        private readonly IProductTagService _productTagService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IDTOHelper _dtoHelper;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IRepository<GenericAttribute> _genericAttributeRepository;
        private readonly IProductAttributeParser _productAttributeParser;

        public ProductsController(IProductApiService productApiService,
                                  IJsonFieldsSerializer jsonFieldsSerializer,
                                  IProductService productService,
                                  IUrlRecordService urlRecordService,
                                  ICustomerActivityService customerActivityService,
                                  ILocalizationService localizationService,
                                  IFactory<Product> factory,
                                  IAclService aclService,
                                  IStoreMappingService storeMappingService,
                                  IStoreService storeService,
                                  ICustomerService customerService,
                                  IDiscountService discountService,
                                  IPictureService pictureService,
                                  IManufacturerService manufacturerService,
                                  IProductTagService productTagService,
                                  IProductAttributeService productAttributeService,
                                  IDTOHelper dtoHelper, 
                                  IGenericAttributeService genericAttributeService,
                                  IRepository<GenericAttribute> genericAttributeRepository,
                                  IProductAttributeParser productAttributeParser)
            : base(jsonFieldsSerializer, aclService, customerService, storeMappingService, storeService, discountService, customerActivityService, localizationService, pictureService)
        {
            _productApiService = productApiService;
            _factory = factory;
            _manufacturerService = manufacturerService;
            _productTagService = productTagService;
            _urlRecordService = urlRecordService;
            _productService = productService;
            _productAttributeService = productAttributeService;
            _dtoHelper = dtoHelper;
            _genericAttributeService = genericAttributeService;
            _genericAttributeRepository = genericAttributeRepository;
            _productAttributeParser = productAttributeParser;
        }

        /// <summary>
        /// Receive a list of all products
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [Route("/api/products")]
        [ProducesResponseType(typeof(ProductsRootObjectDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public IActionResult GetProducts(ProductsParametersModel parameters)
        {
            if (parameters.Limit < Configurations.MinLimit || parameters.Limit > Configurations.MaxLimit)
            {
                return Error(HttpStatusCode.BadRequest, "limit", "invalid limit parameter");
            }

            if (parameters.Page < Configurations.DefaultPageValue)
            {
                return Error(HttpStatusCode.BadRequest, "page", "invalid page parameter");
            }

            var allProducts = _productApiService.GetProducts(parameters.Ids, parameters.CreatedAtMin, parameters.CreatedAtMax, parameters.UpdatedAtMin,
                                                                        parameters.UpdatedAtMax, parameters.Limit, parameters.Page, parameters.SinceId, parameters.CategoryId,
                                                                        parameters.VendorName, parameters.PublishedStatus);
            
            IList<ProductDto> productsAsDtos = allProducts.Select(product => _dtoHelper.PrepareProductDTO(product)).ToList();

            var productsRootObject = new ProductsRootObjectDto()
            {
                Products = productsAsDtos
            };

            var json = JsonFieldsSerializer.Serialize(productsRootObject, parameters.Fields);

            return new RawJsonActionResult(json);
        }

        /// <summary>
        /// Receive a count of all products
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [Route("/api/products/count")]
        [ProducesResponseType(typeof(ProductsCountRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public IActionResult GetProductsCount(ProductsCountParametersModel parameters)
        {
            var allProductsCount = _productApiService.GetProductsCount(parameters.CreatedAtMin, parameters.CreatedAtMax, parameters.UpdatedAtMin,
                                                                       parameters.UpdatedAtMax, parameters.PublishedStatus, parameters.VendorName,
                                                                       parameters.CategoryId);

            var productsCountRootObject = new ProductsCountRootObject()
            {
                Count = allProductsCount
            };

            return Ok(productsCountRootObject);
        }

        /// <summary>
        /// Retrieve product by spcified id
        /// </summary>
        /// <param name="id">Id of the product</param>
        /// <param name="fields">Fields from the product you want your json to contain</param>
        /// <response code="200">OK</response>
        /// <response code="404">Not Found</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [Route("/api/products/{id}")]
        [ProducesResponseType(typeof(ProductsRootObjectDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [GetRequestsErrorInterceptorActionFilter]
        public IActionResult GetProductById(int id, string fields = "")
        {
            if (id <= 0)
            {
                return Error(HttpStatusCode.BadRequest, "id", "invalid id");
            }

            var product = _productApiService.GetProductById(id);

            if (product == null)
            {
                return Error(HttpStatusCode.NotFound, "product", "not found");
            }

            var productDto = _dtoHelper.PrepareProductDTO(product);
            productDto.AdmindId = _genericAttributeService.GetAttribute<int>(product, "nop.product.admindid");

            var productsRootObject = new ProductsRootObjectDto();

            productsRootObject.Products.Add(productDto);

            var json = JsonFieldsSerializer.Serialize(productsRootObject, fields);

            return new RawJsonActionResult(json);
        }

        [HttpPost]
        [Route("/api/products")]
        [ProducesResponseType(typeof(ProductsRootObjectDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        public IActionResult CreateProduct([ModelBinder(typeof(JsonModelBinder<ProductDto>))] Delta<ProductDto> productDelta)
        {
            var genricAttribute = _genericAttributeRepository.Table.Where(g => g.Key == "nop.product.admindid" && g.Value == productDelta.Dto.AdmindId.ToString()).FirstOrDefault();

            Product product = null;
            if (genricAttribute == null)
            {
                // Here we display the errors if the validation has failed at some point.
                if (!ModelState.IsValid)
            {
                return Error();
            }

            // Inserting the new product
            product = _factory.Initialize();

            productDelta.Merge(product);

            _productService.InsertProduct(product);

            UpdateProductPictures(product, productDelta.Dto.Images);

            UpdateProductTags(product, productDelta.Dto.Tags);

            UpdateProductManufacturers(product, productDelta.Dto.ManufacturerIds);

            UpdateAssociatedProducts(product, productDelta.Dto.AssociatedProductIds);
                /*EXTRA*/
                UpdateProductAttributes(product, productDelta);

                UpdateProductAttributeCombinations(product, productDelta.Dto.ProductAttributeCombinations);

                UpdateProductTirePrices(product, productDelta.Dto.DtoTierPrices);

                UpdateProductGenericAttributes(product, productDelta.Dto.DtoGenericAttributes);

                _genericAttributeService.SaveAttribute<int>(product, "nop.product.admindid", productDelta.Dto.AdmindId);
                /*EXTRA*/

                //search engine name
                var seName = _urlRecordService.ValidateSeName(product, productDelta.Dto.SeName, product.Name, true);
            _urlRecordService.SaveSlug(product, seName, 0);

            UpdateAclRoles(product, productDelta.Dto.RoleIds);

            UpdateDiscountMappings(product, productDelta.Dto.DiscountIds);

            UpdateStoreMappings(product, productDelta.Dto.StoreIds);

            _productService.UpdateProduct(product);

            CustomerActivityService.InsertActivity("AddNewProduct",
                LocalizationService.GetResource("ActivityLog.AddNewProduct"), product);
            }
            else
            {
                product = _productService.GetProductById(genricAttribute.EntityId);
            }

            if (product == null)
                return Error(HttpStatusCode.Conflict, "product", "could not find product!");

            // Preparing the result dto of the new product
            var productDto = _dtoHelper.PrepareProductDTO(product);

            /*EXTRA*/
            productDto.AdmindId = _genericAttributeService.GetAttribute<int>(product, "nop.product.admindid");
            /*EXTRA*/

            var productsRootObject = new ProductsRootObjectDto();

            productsRootObject.Products.Add(productDto);

            var json = JsonFieldsSerializer.Serialize(productsRootObject, string.Empty);

            return new RawJsonActionResult(json);
        }

        [HttpPut]
        [Route("/api/products/{id}")]
        [ProducesResponseType(typeof(ProductsRootObjectDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        public IActionResult UpdateProduct([ModelBinder(typeof(JsonModelBinder<ProductDto>))] Delta<ProductDto> productDelta)
        {
            // Here we display the errors if the validation has failed at some point.
            if (!ModelState.IsValid)
            {
                return Error();
            }

            var product = _productApiService.GetProductById(productDelta.Dto.Id);

            if (product == null)
            {
                return Error(HttpStatusCode.NotFound, "product", "not found");
            }

            /*EXTRA*/
            /*
            var admindid = product.GetAttribute<int>("nop.product.admindid");
            if (admindid != productDelta.Dto.AdmindId)
                return Error(HttpStatusCode.Conflict, "admind", "does not match!");
            */
            UpdateProductTirePrices(product, productDelta.Dto.DtoTierPrices);
            UpdateProductGenericAttributes(product, productDelta.Dto.DtoGenericAttributes);
            /*EXTRA*/

            productDelta.Merge(product);

            product.UpdatedOnUtc = DateTime.UtcNow;
            _productService.UpdateProduct(product);

            UpdateProductAttributes(product, productDelta);

            UpdateProductAttributeCombinations(product, productDelta.Dto.ProductAttributeCombinations);

            UpdateProductPictures(product, productDelta.Dto.Images);

            UpdateProductTags(product, productDelta.Dto.Tags);

            UpdateProductManufacturers(product, productDelta.Dto.ManufacturerIds);

            UpdateAssociatedProducts(product, productDelta.Dto.AssociatedProductIds);

            // Update the SeName if specified
            if (productDelta.Dto.SeName != null)
            {
                var seName = _urlRecordService.ValidateSeName(product, productDelta.Dto.SeName, product.Name, true);
                _urlRecordService.SaveSlug(product, seName, 0);
            }

            UpdateDiscountMappings(product, productDelta.Dto.DiscountIds);

            UpdateStoreMappings(product, productDelta.Dto.StoreIds);

            UpdateAclRoles(product, productDelta.Dto.RoleIds);

            _productService.UpdateProduct(product);

            CustomerActivityService.InsertActivity("UpdateProduct",
               LocalizationService.GetResource("ActivityLog.UpdateProduct"), product);

            // Preparing the result dto of the new product
            var productDto = _dtoHelper.PrepareProductDTO(product);

            var productsRootObject = new ProductsRootObjectDto();

            productsRootObject.Products.Add(productDto);

            var json = JsonFieldsSerializer.Serialize(productsRootObject, string.Empty);

            return new RawJsonActionResult(json);
        }

        [HttpDelete]
        [Route("/api/products/{id}")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public IActionResult DeleteProduct(int id)
        {
            if (id <= 0)
            {
                return Error(HttpStatusCode.BadRequest, "id", "invalid id");
            }

            var product = _productApiService.GetProductById(id);

            if (product == null)
            {
                return Error(HttpStatusCode.NotFound, "product", "not found");
            }

            var genericAttribute = _genericAttributeService.GetAttributesForEntity(product.Id, "Product").FirstOrDefault();
            if (genericAttribute != null)
                _genericAttributeService.DeleteAttribute(genericAttribute);

            _productService.DeleteProduct(product);

            //activity log
            CustomerActivityService.InsertActivity("DeleteProduct",
                string.Format(LocalizationService.GetResource("ActivityLog.DeleteProduct"), product.Name), product);

            return new RawJsonActionResult("{}");
        }

        private void UpdateProductGenericAttributes(Product entityToUpdate, List<ProductGenericAttributeDto> dtoGenericAttributes)
        {
            if (dtoGenericAttributes == null)
                return;

            var attributes = new List<GenericAttribute>();
            // IF LIST IS EMPTY DELETE ALL GENERIC ATTRIBUTES!
            if (dtoGenericAttributes.Count == 0)
            {
                attributes = _genericAttributeService.GetAttributesForEntity(entityToUpdate.Id, "Product").ToList();
                attributes = attributes.Where(a => a.Key != "nop.product.admindid" && a.Key != "nop.product.attributevalue.recordid" && a.Key != "nop.product.attribute.combination.records" && a.Key != "nop.product.attribute.combination.admind_id").ToList();
                _genericAttributeService.DeleteAttributes(attributes);
            }

            var attribute = new GenericAttribute();
            foreach (var ga in dtoGenericAttributes)
            {
                var key = string.Format("nop.product.{0}", ga.Name);
                attribute = attributes.Where(a => a.Key == key).FirstOrDefault();
                if (attribute == null)
                {
                    _genericAttributeService.SaveAttribute<string>(entityToUpdate, key, ga.Value);
                }
                else
                {
                    attribute.Value = ga.Value;
                    _genericAttributeService.UpdateAttribute(attribute);
                }
            }
        }

        private void UpdateProductPictures(Product entityToUpdate, List<ImageMappingDto> setPictures)
        {
            // If no pictures are specified means we don't have to update anything
            if (setPictures == null)
                return;

            // delete unused product pictures
            var unusedProductPictures = entityToUpdate.ProductPictures.Where(x => setPictures.All(y => y.Id != x.Id)).ToList();
            foreach (var unusedProductPicture in unusedProductPictures)
            {
                var picture = PictureService.GetPictureById(unusedProductPicture.PictureId);
                if (picture == null)
                    throw new ArgumentException("No picture found with the specified id");
                PictureService.DeletePicture(picture);
            }

            foreach (var imageDto in setPictures)
            {
                if (imageDto.Id > 0)
                {
                    // update existing product picture
                    var productPictureToUpdate = entityToUpdate.ProductPictures.FirstOrDefault(x => x.Id == imageDto.Id);
                    if (productPictureToUpdate != null && imageDto.Position > 0)
                    {
                        productPictureToUpdate.DisplayOrder = imageDto.Position;
                        _productService.UpdateProductPicture(productPictureToUpdate);
                    }
                }
                else
                {
                    // add new product picture
                    var newPicture = PictureService.InsertPicture(imageDto.Binary, imageDto.MimeType, string.Empty, imageDto.Alt, imageDto.Title);
                    _productService.InsertProductPicture(new ProductPicture()
                    {
                        PictureId = newPicture.Id,
                        ProductId = entityToUpdate.Id,
                        DisplayOrder = imageDto.Position
                    });

                    _genericAttributeService.SaveAttribute(newPicture, "nop.product.image.recordid", imageDto.RecordId);
                }
            }
        }

        private void UpdateProductAttributes(Product entityToUpdate, Delta<ProductDto> productDtoDelta)
        {
            // If no product attribute mappings are specified means we don't have to update anything
            if (productDtoDelta.Dto.ProductAttributeMappings == null)
                return;

            //If it has attributes, then stock should be managed by them
            entityToUpdate.ManageInventoryMethod = ManageInventoryMethod.ManageStockByAttributes;
            entityToUpdate.AllowAddingOnlyExistingAttributeCombinations = true;
            entityToUpdate.DisplayStockAvailability = true;

            // delete unused product attribute mappings
            var toBeUpdatedIds = new List<int>();
            var useRecordMappings = false;
            foreach (var entityToUpdateMapping in entityToUpdate.ProductAttributeMappings)
            {
                var entityRecords = entityToUpdateMapping.ProductAttributeValues.Select(v => _genericAttributeService.GetAttribute<int>(v, "nop.product.attributevalue.recordid"));

                if (entityRecords.Count() > 0)
                {
                    useRecordMappings = true;

                    foreach (var deltaMapping in productDtoDelta.Dto.ProductAttributeMappings)
                    {
                        var values = deltaMapping.ProductAttributeValues.Where(v => entityRecords.Contains(v.RecordId));
                        if (values != null && values.Count() != 0 && deltaMapping.Id == 0)
                        {
                            toBeUpdatedIds.Add(entityToUpdateMapping.Id);
                            deltaMapping.Id = entityToUpdateMapping.Id;

                            foreach (var entityValue in entityToUpdateMapping.ProductAttributeValues)
                            {
                                var deltaValue = deltaMapping.ProductAttributeValues.Where(dv => dv.RecordId == _genericAttributeService.GetAttribute<int>(entityValue, "nop.product.attributevalue.recordid")).FirstOrDefault();
                                if (deltaValue != null)
                                    deltaValue.Id = entityValue.Id;
                            }
                        }
                    }
                }
            }

            if (!useRecordMappings)
            {
                toBeUpdatedIds = productDtoDelta.Dto.ProductAttributeMappings.Where(y => y.Id != 0).Select(x => x.Id).ToList();
            }

            var unusedProductAttributeMappings = entityToUpdate.ProductAttributeMappings.Where(x => !toBeUpdatedIds.Contains(x.Id)).ToList();

            foreach (var unusedProductAttributeMapping in unusedProductAttributeMappings)
            {
                _productAttributeService.DeleteProductAttributeMapping(unusedProductAttributeMapping);
            }

            // delete unused product attribute mappings
            foreach (var productAttributeMappingDto in productDtoDelta.Dto.ProductAttributeMappings)
            {
                if (productAttributeMappingDto.Id > 0)
                {
                    // update existing product attribute mapping
                    var productAttributeMappingToUpdate = entityToUpdate.ProductAttributeMappings.FirstOrDefault(x => x.Id == productAttributeMappingDto.Id);
                    productAttributeMappingToUpdate.AttributeControlTypeId = 1;
                    if (productAttributeMappingToUpdate != null)
                    {
                        productDtoDelta.Merge(productAttributeMappingDto,productAttributeMappingToUpdate,false);
                       
                        _productAttributeService.UpdateProductAttributeMapping(productAttributeMappingToUpdate);

                        UpdateProductAttributeValues(productAttributeMappingDto, productDtoDelta);
                    }
                }
                else
                {
                    var newProductAttributeMapping = new ProductAttributeMapping
                    {
                        ProductId = entityToUpdate.Id,
                        AttributeControlTypeId = 1
                    };

                    productDtoDelta.Merge(productAttributeMappingDto, newProductAttributeMapping);

                    // add new product attribute
                    _productAttributeService.InsertProductAttributeMapping(newProductAttributeMapping);
                    productAttributeMappingDto.Id = newProductAttributeMapping.Id;

                    for (int i = 0; i < newProductAttributeMapping.ProductAttributeValues.Count; i++)
                    {
                        var attributeValue = newProductAttributeMapping.ProductAttributeValues.ElementAt(i);
                        var dtoAttributeValue = productAttributeMappingDto.ProductAttributeValues.ElementAt(i);
                        if (dtoAttributeValue.ProductPictureId.HasValue)
                            attributeValue.PictureId = dtoAttributeValue.ProductPictureId.Value;
                        else
                            attributeValue.PictureId = 0;

                        _genericAttributeService.SaveAttribute(attributeValue, "nop.product.attributevalue.recordid", dtoAttributeValue.RecordId);
                    }
                }
            }
        }

        private void UpdateProductAttributeValues(ProductAttributeMappingDto productAttributeMappingDto, Delta<ProductDto> productDtoDelta)
        {
            // If no product attribute values are specified means we don't have to update anything
            if (productAttributeMappingDto.ProductAttributeValues == null)
                return;

            // delete unused product attribute values
            var toBeUpdatedIds = productAttributeMappingDto.ProductAttributeValues.Where(y => y.Id != 0).Select(x => x.Id);

            var unusedProductAttributeValues =
                _productAttributeService.GetProductAttributeValues(productAttributeMappingDto.Id).Where(x => !toBeUpdatedIds.Contains(x.Id)).ToList();

            foreach (var unusedProductAttributeValue in unusedProductAttributeValues)
            {
                _productAttributeService.DeleteProductAttributeValue(unusedProductAttributeValue);
            }

            foreach (var productAttributeValueDto in productAttributeMappingDto.ProductAttributeValues)
            {
                if (productAttributeValueDto.Id > 0)
                {
                    // update existing product attribute mapping
                    var productAttributeValueToUpdate = _productAttributeService.GetProductAttributeValueById(productAttributeValueDto.Id);
                    //var recordId = productAttributeValueToUpdate.GetAttribute<int>("nop.product.attributevalue.recordid");
                    if (productAttributeValueToUpdate != null)
                    {
                        productDtoDelta.Merge(productAttributeValueDto, productAttributeValueToUpdate, false);
                        if (productAttributeValueDto.ProductPictureId.HasValue)
                            productAttributeValueToUpdate.PictureId = productAttributeValueDto.ProductPictureId.Value;
                        else
                            productAttributeValueToUpdate.PictureId = 0;

                        _productAttributeService.UpdateProductAttributeValue(productAttributeValueToUpdate);
                    }
                }
                else
                {
                    var newProductAttributeValue = new ProductAttributeValue();
                    productDtoDelta.Merge(productAttributeValueDto, newProductAttributeValue);

                    newProductAttributeValue.ProductAttributeMappingId = productAttributeMappingDto.Id;
                    // add new product attribute value
                    _productAttributeService.InsertProductAttributeValue(newProductAttributeValue);
                    _genericAttributeService.SaveAttribute<int>(newProductAttributeValue, "nop.product.attributevalue.recordid", productAttributeValueDto.RecordId);
                }
            }
        }

        private void UpdateProductAttributeCombinations(Product product, List<ProductAttributeCombinationDto> productAttributeCombinations)
        {
            if (productAttributeCombinations == null)
                return;

            // REMOVE OLD ProductAttributeCombinations! // IF ID IS NOT IN LIST THEN REMOVE!
            if (product.ProductAttributeCombinations.Count > 0)
            {
                var ids = productAttributeCombinations.Select(c => c.Id).ToList();
                var dataCombinations = new ProductAttributeCombination[product.ProductAttributeCombinations.Count];
                product.ProductAttributeCombinations.CopyTo(dataCombinations, 0);

                foreach (var combi in dataCombinations.Where(t => !ids.Contains(t.Id)))
                {
                    _productAttributeService.DeleteProductAttributeCombination(combi);
                }
            }

            var attributesXml = "";
            foreach (var combi in productAttributeCombinations)
            {
                attributesXml = string.Empty;
                if (combi.Id == 0)
                {
                    for (int i = 0; i < combi.Records.Count; i++)
                    {
                        foreach (var mapping in product.ProductAttributeMappings)
                        {
                            if (mapping.ProductAttributeValues.Count == 0)
                            {
                                var productAttributeValues = _productAttributeService.GetProductAttributeValues(mapping.Id);
                            }

                            var mappingValue = mapping.ProductAttributeValues.Where(v => _genericAttributeService.GetAttribute<int>(v, "nop.product.attributevalue.recordid") == combi.Records[i]).FirstOrDefault();
                            if (mappingValue != null)
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml, mapping, mappingValue.Id.ToString());
                        }
                    }
                    // OPRET NY COMBINATION
                    var attributeCombi = new ProductAttributeCombination()
                    {
                        ProductId = product.Id,
                        AttributesXml = attributesXml,
                        Sku = combi.Sku,
                        Gtin = combi.Gtin,
                        StockQuantity = combi.StockQuantity,
                        AllowOutOfStockOrders = combi.AllowOutOfStockOrders,
                        ManufacturerPartNumber = combi.ManufacturerPartNumber,
                        NotifyAdminForQuantityBelow = combi.NotifyAdminForQuantityBelow,
                        OverriddenPrice = combi.OverriddenPrice
                    };

                    _productAttributeService.InsertProductAttributeCombination(attributeCombi);
                    _genericAttributeService.SaveAttribute(attributeCombi, "nop.product.attribute.combination.records", combi.Records);
                    _genericAttributeService.SaveAttribute(attributeCombi, "nop.product.attribute.combination.admind_id", combi.AdmindCombinationId);
                }
                else
                {
                    // OPDATERE COMBINATION
                    var currentCombi = _productAttributeService.GetProductAttributeCombinationById(combi.Id);
                    currentCombi.Gtin = combi.Gtin != currentCombi.Gtin ? combi.Gtin : currentCombi.Gtin;
                    currentCombi.Sku = combi.Sku != currentCombi.Sku ? combi.Sku : currentCombi.Sku;
                    currentCombi.StockQuantity = combi.StockQuantity != currentCombi.StockQuantity ? combi.StockQuantity : currentCombi.StockQuantity;
                    currentCombi.AllowOutOfStockOrders = combi.AllowOutOfStockOrders != currentCombi.AllowOutOfStockOrders ? combi.AllowOutOfStockOrders : currentCombi.AllowOutOfStockOrders;
                    currentCombi.ManufacturerPartNumber = combi.ManufacturerPartNumber != currentCombi.ManufacturerPartNumber ? combi.ManufacturerPartNumber : currentCombi.ManufacturerPartNumber;
                    currentCombi.NotifyAdminForQuantityBelow = combi.NotifyAdminForQuantityBelow != currentCombi.NotifyAdminForQuantityBelow ? combi.NotifyAdminForQuantityBelow : currentCombi.NotifyAdminForQuantityBelow;
                    currentCombi.OverriddenPrice = combi.OverriddenPrice != currentCombi.OverriddenPrice ? combi.OverriddenPrice : currentCombi.OverriddenPrice;

                    _productAttributeService.UpdateProductAttributeCombination(currentCombi);
                }
            }
        }

        private void UpdateProductTags(Product product, IReadOnlyCollection<string> productTags)
        {
            if (productTags == null)
                return;

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            //Copied from UpdateProductTags method of ProductTagService
            //product tags
            var existingProductTags = _productTagService.GetAllProductTagsByProductId(product.Id);
            var productTagsToRemove = new List<ProductTag>();
            foreach (var existingProductTag in existingProductTags)
            {
                var found = false;
                foreach (var newProductTag in productTags)
                {
                    if (!existingProductTag.Name.Equals(newProductTag, StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    found = true;
                    break;
                }

                if (!found)
                {
                    productTagsToRemove.Add(existingProductTag);
                }
            }

            foreach (var productTag in productTagsToRemove)
            {
                //product.ProductTags.Remove(productTag);
                product.ProductProductTagMappings
                    .Remove(product.ProductProductTagMappings.FirstOrDefault(mapping => mapping.ProductTagId == productTag.Id));
                _productService.UpdateProduct(product);
            }

            foreach (var productTagName in productTags)
            {
                ProductTag productTag;
                var productTag2 = _productTagService.GetProductTagByName(productTagName);
                if (productTag2 == null)
                {
                    //add new product tag
                    productTag = new ProductTag
                    {
                        Name = productTagName
                    };
                    _productTagService.InsertProductTag(productTag);
                }
                else
                {
                    productTag = productTag2;
                }

                if (!_productService.ProductTagExists(product, productTag.Id))
                {
                    product.ProductProductTagMappings.Add(new ProductProductTagMapping { ProductTag = productTag });
                    _productService.UpdateProduct(product);
                }

                var seName = _urlRecordService.ValidateSeName(productTag, string.Empty, productTag.Name, true);
                _urlRecordService.SaveSlug(productTag, seName, 0);
            }
        }

        private void UpdateDiscountMappings(Product product, List<int> passedDiscountIds)
        {
            if (passedDiscountIds == null)
                return;

            var allDiscounts = DiscountService.GetAllDiscounts(DiscountType.AssignedToSkus, showHidden: true);

            foreach (var discount in allDiscounts)
            {
                if (passedDiscountIds.Contains(discount.Id))
                {
                    //new discount
                    if (product.AppliedDiscounts.Count(d => d.Id == discount.Id) == 0)
                        product.AppliedDiscounts.Add(discount);
                }
                else
                {
                    //remove discount
                    if (product.AppliedDiscounts.Count(d => d.Id == discount.Id) > 0)
                        product.AppliedDiscounts.Remove(discount);
                }
            }

            _productService.UpdateProduct(product);
            _productService.UpdateHasDiscountsApplied(product);
        }
        
        private void UpdateProductManufacturers(Product product, List<int> passedManufacturerIds)
        {
            // If no manufacturers specified then there is nothing to map 
            if (passedManufacturerIds == null)
                return;

            var unusedProductManufacturers = product.ProductManufacturers.Where(x => !passedManufacturerIds.Contains(x.ManufacturerId)).ToList();

            // remove all manufacturers that are not passed
            foreach (var unusedProductManufacturer in unusedProductManufacturers)
            {
                _manufacturerService.DeleteProductManufacturer(unusedProductManufacturer);
            }

            foreach (var passedManufacturerId in passedManufacturerIds)
            {
                // not part of existing manufacturers so we will create a new one
                if (product.ProductManufacturers.All(x => x.ManufacturerId != passedManufacturerId))
                {
                    // if manufacturer does not exist we simply ignore it, otherwise add it to the product
                    var manufacturer = _manufacturerService.GetManufacturerById(passedManufacturerId);
                    if (manufacturer != null)
                    {
                        _manufacturerService.InsertProductManufacturer(new ProductManufacturer()
                        { ProductId = product.Id, ManufacturerId = manufacturer.Id });
                    }
                }
            }
        }

        private void UpdateAssociatedProducts(Product product, List<int> passedAssociatedProductIds)
        {
            // If no associated products specified then there is nothing to map 
            if (passedAssociatedProductIds == null)
                return;

            var noLongerAssociatedProducts =
                _productService.GetAssociatedProducts(product.Id, showHidden: true)
                    .Where(p => !passedAssociatedProductIds.Contains(p.Id));

            // update all products that are no longer associated with our product
            foreach (var noLongerAssocuatedProduct in noLongerAssociatedProducts)
            {
                noLongerAssocuatedProduct.ParentGroupedProductId = 0;
                _productService.UpdateProduct(noLongerAssocuatedProduct);
            }

            var newAssociatedProducts = _productService.GetProductsByIds(passedAssociatedProductIds.ToArray());
            foreach (var newAssociatedProduct in newAssociatedProducts)
            {
                newAssociatedProduct.ParentGroupedProductId = product.Id;
                _productService.UpdateProduct(newAssociatedProduct);
            }
        }

        private void UpdateProductTirePrices(Product product, List<TierPriceDto> tierPrices)
        {
            if (tierPrices == null)
                return;

            if (tierPrices.Count == 0 && product.TierPrices.Count == 0)
                return;

            // REMOVE OLD TIERPRICES! IF [] IS EMPTY!
            if (tierPrices.Count == 0)
            {
                var dataTierPrices = new TierPrice[product.TierPrices.Count];
                product.TierPrices.CopyTo(dataTierPrices, 0);
                foreach (var tier in dataTierPrices)
                {
                    _productService.DeleteTierPrice(tier);
                }
                product.TierPrices.Clear();
                return;
            }

            // REMOVE OLD TIERPRICES! // IF ID IS NOT IN LIST THEN REMOVE!
            if (product.TierPrices.Count > 0)
            {
                var ids = tierPrices.Select(t => t.Id).ToList();
                var dataTierPrices = new TierPrice[product.TierPrices.Count];
                product.TierPrices.CopyTo(dataTierPrices, 0);

                foreach (var tier in dataTierPrices.Where(t => !ids.Contains(t.Id)))
                {
                    _productService.DeleteTierPrice(tier);
                }
            }

            foreach (var tp in tierPrices)
            {
                var tier = product.TierPrices.Where(t => t.Id == tp.Id).FirstOrDefault();
                if (tier == null)
                {
                    _productService.InsertTierPrice(new TierPrice()
                    {
                        ProductId = product.Id,
                        StoreId = tp.StoreId,
                        CustomerRoleId = tp.CustomerRoleId,
                        Price = tp.Price,
                        Quantity = tp.Quantity,
                        StartDateTimeUtc = tp.StartDateTimeUtc.HasValue ? tp.StartDateTimeUtc.Value : (DateTime?)null,
                        EndDateTimeUtc = tp.EndDateTimeUtc.HasValue ? tp.EndDateTimeUtc.Value : (DateTime?)null
                    });
                }
                else
                {
                    if ((tp.Price != tier.Price && tp.Price > 0) || tp.StoreId != tier.StoreId || tp.CustomerRoleId.HasValue || tp.Quantity != tier.Quantity || tp.StartDateTimeUtc.HasValue || tp.EndDateTimeUtc.HasValue)
                    {
                        tier.Price = tp.Price;
                        tier.Quantity = tp.Quantity;
                        tier.StoreId = tp.StoreId;
                        tier.CustomerRoleId = tp.CustomerRoleId.HasValue ? tp.CustomerRoleId.Value : tier.CustomerRoleId;
                        tier.StartDateTimeUtc = tp.StartDateTimeUtc.HasValue ? tp.StartDateTimeUtc.Value : tier.StartDateTimeUtc;
                        tier.EndDateTimeUtc = tp.EndDateTimeUtc.HasValue ? tp.EndDateTimeUtc.Value : tier.EndDateTimeUtc;

                        _productService.UpdateTierPrice(tier);
                    }
                }
            }
            _productService.UpdateHasTierPricesProperty(product);
        }
    }
}