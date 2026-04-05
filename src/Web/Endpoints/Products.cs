using OrderManagement.Application.Products.Commands.CreateProduct;
using OrderManagement.Application.Products.Commands.DeleteProduct;
using OrderManagement.Application.Products.Commands.UpdateProduct;
using Microsoft.AspNetCore.Http.HttpResults;
using OrderManagement.Application.Products.Queries.GetProducts;
using OrderManagement.Application.Products.Queries.GetProductById;

namespace OrderManagement.Web.Endpoints;

public class Products : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetProducts);
        groupBuilder.MapGet(GetProductById, "{id:int}");
        groupBuilder.MapPost(CreateProduct);
        groupBuilder.MapPut(UpdateProduct, "{id:int}");
        groupBuilder.MapDelete(DeleteProduct, "{id:int}");
    }

    [EndpointSummary("Obter todos os produtos")]
    [EndpointDescription("Retorna uma lista de todos os produtos disponíveis.")]
    public static async Task<Ok<IReadOnlyCollection<ProductDto>>> GetProducts(ISender sender)
    {        
        var products = await sender.Send(new GetProductsQuery());
        return TypedResults.Ok(products);
    }

    [EndpointSummary("Obter um produto por ID")]
    [EndpointDescription("Retorna os detalhes de um produto específico com base no ID fornecido.")]
    public static async Task<Results<Ok<ProductDto>, NotFound>> GetProductById(ISender sender, int id)
    {
        var product = await sender.Send(new GetProductByIdQuery(id));
        return TypedResults.Ok(product);
    }

    [EndpointSummary("Criar um novo produto")]
    [EndpointDescription("Cria um novo produto com as informações fornecidas no payload.")]
    public static async Task<Created<int>> CreateProduct(ISender sender, CreateProductCommand command)
    {
        var id = await sender.Send(command);

        return TypedResults.Created($"/api/{nameof(Products)}/{id}", id);
    }

    [EndpointSummary("Atualizar um produto")]
    [EndpointDescription("Atualiza o produto especificado. O ID na URL deve corresponder ao ID no payload.")]
    public static async Task<Results<NoContent, BadRequest>> UpdateProduct(ISender sender, int id, UpdateProductCommand command)
    {
        if (id != command.Id) return TypedResults.BadRequest();

        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [EndpointSummary("Deletar um produto")]
    [EndpointDescription("Deleta o produto com o ID especificado.")]
    public static async Task<NoContent> DeleteProduct(ISender sender, int id)
    {
        await sender.Send(new DeleteProductCommand(id));

        return TypedResults.NoContent();
    }
}
