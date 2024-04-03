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
