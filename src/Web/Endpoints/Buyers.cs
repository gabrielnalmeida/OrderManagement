using OrderManagement.Application.Buyers.Commands.CreateBuyer;
using OrderManagement.Application.Buyers.Commands.DeleteBuyer;
using OrderManagement.Application.Buyers.Commands.UpdateBuyer;
using Microsoft.AspNetCore.Http.HttpResults;
using OrderManagement.Application.Buyers.Queries.GetBuyers;

namespace OrderManagement.Web.Endpoints;

public class Buyers : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetBuyers);
        groupBuilder.MapPost(CreateBuyer);
        groupBuilder.MapPut(UpdateBuyer, "{id:int}");
        groupBuilder.MapDelete(DeleteBuyer, "{id:int}");
    }

    [EndpointSummary("Obter todos os compradores")]
    [EndpointDescription("Retorna uma lista de todos os compradores disponíveis.")]
    public static async Task<Ok<IReadOnlyCollection<BuyerDto>>> GetBuyers(ISender sender)
    {        
        var buyers = await sender.Send(new GetBuyersQuery());
        return TypedResults.Ok(buyers);
    }

    [EndpointSummary("Criar um novo comprador")]
    [EndpointDescription("Cria um novo comprador com as informações fornecidas no payload.")]
    public static async Task<Created<int>> CreateBuyer(ISender sender, CreateBuyerCommand command)
    {
        var id = await sender.Send(command);

        return TypedResults.Created($"/api/{nameof(Buyers)}/{id}", id);
    }

    [EndpointSummary("Atualizar um comprador")]
    [EndpointDescription("Atualiza o comprador especificado. O ID na URL deve corresponder ao ID no payload.")]
    public static async Task<Results<NoContent, BadRequest>> UpdateBuyer(ISender sender, int id, UpdateBuyerCommand command)
    {
        if (id != command.Id) return TypedResults.BadRequest();

        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [EndpointSummary("Deletar um comprador")]
    [EndpointDescription("Deleta o comprador com o ID especificado.")]
    public static async Task<NoContent> DeleteBuyer(ISender sender, int id)
    {
        await sender.Send(new DeleteBuyerCommand(id));

        return TypedResults.NoContent();
    }
}
