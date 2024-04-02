using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
var configuration = app.Configuration;
builder.Services.AddDbContext<ApplicationDbContext>();
var productRepository = new ProductRepository(); 
productRepository.Init(configuration);


app.MapGet("/products/{code}", ([FromRoute] string code) =>
{
    var product = productRepository.GetByCode(code);
    if(product != null)
    {
      return Results.Ok(product);
    }
    else
    {
       return Results.NotFound();
    }
});

app.MapPost("/products", (Product product) =>
{
    productRepository.Add(product);
   return Results.Created("/products/" + product.Code, product.Code);
});

app.MapPut("/product", (Product product) =>
{
    var productSaved  = productRepository.GetByCode(product.Code);
    productSaved.Name = product.Name;
    productSaved.Code = product.Code;
    Results.Ok();
});

app.MapGet("/configuration/database", (IConfiguration configuration) => {
    return Results.Ok($"{configuration["database:connection"]}/{configuration["database:Port"]}");
});

app.MapDelete("/products/{code}", ([FromRoute] string code) =>
{
    var productSaved = productRepository.GetByCode(code);
    productRepository.Remove(productSaved);
    return Results.Ok(productSaved);
});

app.Run();

public class ProductRepository // Removi a palavra-chave static
{
    public List<Product> Products { get; set; } = new List<Product>();  

    public ProductRepository()  
    {
        Products = new List<Product>();
    }

    public void Add(Product product)
    {
        Products.Add(product);
    }

    public void Init(IConfiguration configuration)
    {
        var products = configuration.GetSection("Products").Get<List<Product>>();
        Products = products;
    }
    public Product GetByCode(string code)
    {
        return Products.FirstOrDefault(p => p.Code == code);  
    }

    public  void Remove(Product product)
    {
        Products.Remove(product);
    }
}

public class Category {
    public int Id { get; set; }
    public string Name { get; set; }
}
public class Product
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; }
}

public class ApplicationDbContext: DbContext{
    public DbSet<Product> Products { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>()
            .Property(p => p.Description).HasMaxLength(500).IsRequired(false);
        modelBuilder.Entity<Product>()
            .Property(p => p.Name).HasMaxLength(120).IsRequired(true);
        modelBuilder.Entity<Product>()
            .Property(p => p.Code).HasMaxLength(20).IsRequired(true);
    }
    protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlServer(
         "Server=localhost;Database=Products;User ID=dbSQL;Password=123456;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;"
    );
}