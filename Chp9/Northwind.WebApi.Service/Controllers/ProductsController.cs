using Microsoft.AspNetCore.Mvc; // To use [HttpGet] and so on.
using Microsoft.Extensions.Caching.Memory;  // Add this import
using Northwind.EntityModels; // To use NorthwindContext, Product.

namespace Northwind.WebApi.Service.Controllers;

[Route("api/products")]
[ApiController]
public class ProductsController : ControllerBase
{
  private int pageSize = 10;
  private readonly ILogger<ProductsController> _logger;
  private readonly NorthwindContext _db;
  private readonly IMemoryCache _cache;
  private const string OutOfStockProductsKey = "OOSP";
  public ProductsController(ILogger<ProductsController> logger,
    NorthwindContext context,
    IMemoryCache cache)
  {
    _logger = logger;
    _db = context;
    _cache = cache;
  }

  // GET: api/products
  [HttpGet]
  [Produces(typeof(Product[]))]
  public IEnumerable<Product> Get(int? page)
  {
    string cacheKey = $"products_page_{(page ?? 1)}";

    if (!_cache.TryGetValue(cacheKey, out IEnumerable<Product> products))
    {
      products = _db.Products
        .Where(p => p.UnitsInStock > 0 && !p.Discontinued)
        .OrderBy(product => product.ProductId)
        .Skip(((page ?? 1) - 1) * pageSize)
        .Take(pageSize)
        .ToList();

      _cache.Set(cacheKey, products, TimeSpan.FromMinutes(5)); 
    }
    else
    {
      Console.WriteLine($"Found page {page} in cache!");
    }

    return products;
  }

// GET: api/products/outofstock
[HttpGet]
[Route("outofstock")]
[Produces(typeof(Product[]))]
public IEnumerable<Product> GetOutOfStockProducts()
{
    // Try to get the cached value.
    if (!_cache.TryGetValue(OutOfStockProductsKey,
      out Product[]? cachedValue))
  {
  // If the cached value is not found, get the value from the database.
  cachedValue = _db.Products
    .Where(p => p.UnitsInStock == 0 && !p.Discontinued)
    .ToArray();
  MemoryCacheEntryOptions cacheEntryOptions = new()
  {
    SlidingExpiration = TimeSpan.FromSeconds(5),
    Size = cachedValue?.Length
    };
  _cache.Set(OutOfStockProductsKey, cachedValue,
  cacheEntryOptions);
  }
    MemoryCacheStatistics? stats = _cache.GetCurrentStatistics();
    _logger.LogInformation($"Memory cache. Total hits: {stats?.TotalHits}. Estimated size: {stats?.CurrentEstimatedSize}.");
  return cachedValue ?? Enumerable.Empty<Product>(); 
}

  // GET: api/products/discontinued
  [HttpGet]
  [Route("discontinued")]
  [Produces(typeof(Product[]))]
  public IEnumerable<Product> GetDiscontinuedProducts()
  {
    string cacheKey = "products_discontinued";

    if (!_cache.TryGetValue(cacheKey, out IEnumerable<Product> products))
    {
      products = _db.Products
        .Where(product => product.Discontinued)
        .ToList();

      _cache.Set(cacheKey, products, TimeSpan.FromMinutes(5));
    }

    return products;
  }

  // GET api/products/5
  [HttpGet("{id:int}")]
  public async ValueTask<Product?> Get(int id)
  {
    string cacheKey = $"product_{id}";

    if (!_cache.TryGetValue(cacheKey, out Product? product))
    {
      product = await _db.Products.FindAsync(id);

      if (product != null)
      {
        _cache.Set(cacheKey, product, TimeSpan.FromMinutes(5));
      }
    }

    return product;
  }

  // GET api/products/cha
  [HttpGet("{name}")]
  public IEnumerable<Product> Get(string name)
  {
    string cacheKey = $"products_name_{name}";

    if (!_cache.TryGetValue(cacheKey, out IEnumerable<Product> products))
    {
      products = _db.Products
        .Where(p => p.ProductName.Contains(name))
        .ToList();

      _cache.Set(cacheKey, products, TimeSpan.FromMinutes(5));
    }

    return products;
  }

  // POST api/products
  [HttpPost]
  public async Task<IActionResult> Post([FromBody] Product product)
  {
    _cache.Remove("products_page_1");

    _db.Products.Add(product);
    await _db.SaveChangesAsync();
    return Created($"api/products/{product.ProductId}", product);
  }

  // PUT api/products/5
  [HttpPut("{id}")]
  public async Task<IActionResult> Put(int id, [FromBody] Product product)
  {
    Product? foundProduct = await _db.Products.FindAsync(id);
    if (foundProduct is null) return NotFound();
    foundProduct.ProductName = product.ProductName;
    foundProduct.CategoryId = product.CategoryId;
    foundProduct.SupplierId = product.SupplierId;
    foundProduct.QuantityPerUnit = product.QuantityPerUnit;
    foundProduct.UnitsInStock = product.UnitsInStock;
    foundProduct.UnitsOnOrder = product.UnitsOnOrder;
    foundProduct.ReorderLevel = product.ReorderLevel;
    foundProduct.UnitPrice = product.UnitPrice;
    foundProduct.Discontinued = product.Discontinued;
    await _db.SaveChangesAsync();
    return NoContent();
  }

  // DELETE api/products/5
  [HttpDelete("{id}")]
  public async Task<IActionResult> Delete(int id)
  {
    if (await _db.Products.FindAsync(id) is Product product)
    {
      _db.Products.Remove(product);
      await _db.SaveChangesAsync();
      return NoContent();
    }
    return NotFound();
  }
}