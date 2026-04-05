using OrderManagement.Application.Orders.Commands.CreateOrder;
using OrderManagement.Application.Orders.Commands.UpdateOrder;
using OrderManagement.Application.Orders.Commands.ProcessOrder;
using OrderManagement.Application.Orders.Commands.ShipOrder;
using OrderManagement.Application.Orders.Commands.CancelOrder;
using OrderManagement.Application.Orders.Queries.GetOrders;
using OrderManagement.Application.Orders.Queries.GetOrderById;
using Microsoft.AspNetCore.Http.HttpResults;
using OrderManagement.Web.Filters.Orders;

namespace OrderManagement.Web.Endpoints;

public class Orders : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetOrders);
        groupBuilder.MapGet(GetOrderById, "{id:int}");
        groupBuilder.MapPost(CreateOrder);
        groupBuilder.MapPut(UpdateOrder, "{id:int}");
        groupBuilder.MapPut(ProcessOrder, "{id:int}/process");
        groupBuilder.MapPut(ShipOrder, "{id:int}/ship");
        groupBuilder.MapDelete(CancelOrder, "{id:int}/cancel");
    }

    [EndpointSummary("Obter todos os pedidos")]
    [EndpointDescription("Retorna uma lista de todos os pedidos disponíveis.")]
    public static async Task<Ok<IReadOnlyCollection<OrderSummaryDto>>> GetOrders(
        ISender sender,
        [AsParameters] GetOrdersRequest query
    )
    {        
        var orders = await sender.Send(new GetOrdersQuery
        {
            BuyerId = query.BuyerId,
            Status = query.Status,
            CreatedFrom = query.CreatedFrom,
            CreatedTo = query.CreatedTo
        });

        return TypedResults.Ok(orders);
    }

    [EndpointSummary("Obter um pedido por ID")]
    [EndpointDescription("Retorna um pedido específico com base no ID fornecido.")]
    public static async Task<Ok<OrderDetailsDto>> GetOrderById(ISender sender, int id)
    {
        var order = await sender.Send(new GetOrderByIdQuery(id));
        return TypedResults.Ok(order);
    }

    [EndpointSummary("Criar um novo pedido")]
    [EndpointDescription("Cria um novo pedido com as informações fornecidas no payload.")]
    public static async Task<Created<int>> CreateOrder(ISender sender, CreateOrderCommand command)
    {
        var id = await sender.Send(command);

        return TypedResults.Created($"/api/{nameof(Orders)}/{id}", id);
    }

    [EndpointSummary("Atualizar um pedido")]
    [EndpointDescription("Atualiza o pedido especificado. O ID na URL deve corresponder ao ID no payload.")]
    public static async Task<Results<NoContent, BadRequest>> UpdateOrder(ISender sender, int id, UpdateOrderCommand command)
    {
        if (id != command.Id) return TypedResults.BadRequest();

        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [EndpointSummary("Processar um pedido")]
    [EndpointDescription("Processa o pedido especificado, alterando seu status para 'Processing'.")]
    public static async Task<NoContent> ProcessOrder(ISender sender, int id)
    {
        await sender.Send(new ProcessOrderCommand(id));
        return TypedResults.NoContent();
    }

    [EndpointSummary("Enviar um pedido")]
    [EndpointDescription("Envia o pedido especificado, alterando seu status para 'Shipped'.")]
    public static async Task<NoContent> ShipOrder(ISender sender, int id)
    {
        await sender.Send(new ShipOrderCommand(id));
        return TypedResults.NoContent();
    }

    [EndpointSummary("Cancelar um pedido")]
    [EndpointDescription("Cancela o pedido especificado, alterando seu status para 'Cancelled'.")]
    public static async Task<NoContent> CancelOrder(ISender sender, int id)
    {
        await sender.Send(new CancelOrderCommand(id));
        return TypedResults.NoContent();    
    }
}

