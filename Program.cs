using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Collections.Generic;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSqlServer<ApplicationDbContext>(builder.Configuration["Database:SqlServer"]);
var app = builder.Build();
var configuration = app.Configuration;
var productRepository = new ProductRepository();
productRepository.Init(configuration);

app.MapGet("/products/{code}", ([FromRoute] string code) =>
{
    var product = productRepository.GetByCode(code);
    if (product != null)
    {
        return Results.Ok(product);
    }
    else
    {
        return Results.NotFound();
    }
});

app.MapPost("/products", (ProductRequest productRequest, ApplicationDbContext context) =>
{
    var category = context.Category.Where(c => c.Id == productRequest.CategoryId).First();
    var product = new Product
    {
        Code = productRequest.Code,
        Name = productRequest.Name,
        Description = productRequest.Description,
        Category = category
    };
        context.Products.Add(product);
        context.SaveChanges();
     return Results.Created($"/products/{product.Id}", product.Id);
});

app.MapPut("/product", (Product product) =>
{
    var productSaved = productRepository.GetByCode(product.Code);
    productSaved.Name = product.Name;
    productSaved.Code = product.Code;
    Results.Ok();
});

app.MapDelete("/products/{code}", ([FromRoute] string code) =>
{
    var productSaved = productRepository.GetByCode(code);
    productRepository.Remove(productSaved);
    return Results.Ok(productSaved);
});

app.MapGet("/configuration/database", (IConfiguration configuration) =>
{
    return Results.Ok($"{configuration["database:connection"]}/{configuration["database:Port"]}");
});


app.Run();
