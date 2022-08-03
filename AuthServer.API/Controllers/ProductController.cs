using AuthServer.Core.Dtos;
using AuthServer.Core.Models;
using AuthServer.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthServer.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : CustomBaseController
    {
        private readonly IGenericService<Product,ProductDto> _genericService;

        public ProductController(IGenericService<Product, ProductDto> genericService)
        {
            _genericService = genericService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts() => ActionResultInstance(await _genericService.GetAllAsync());

        [HttpPost]
        public async Task<IActionResult> SaveProducts(ProductDto product) =>ActionResultInstance(await _genericService.AddAsync(product));

        [HttpPut]
        public async Task<IActionResult> UpdateProduct(ProductDto product) => ActionResultInstance(await _genericService.Update(product,product.Id));

        [HttpDelete]
        public async Task<IActionResult> DeleteProduct(DeleteProductDto delete) => ActionResultInstance(await _genericService.Remove(delete.Id));
    }
}
