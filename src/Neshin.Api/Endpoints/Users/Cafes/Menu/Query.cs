using FastEndpoints;
using Neshin.Application.Common;

namespace Neshin.Api.Endpoints.Users.Cafes.Menu;

internal sealed record Query : ICommand<Query.Response>
{
    public Guid CafeId { get; init; }

    internal sealed record Response
    {
        public Guid CafeId { get; init; }
        public string CafeName { get; init; } = string.Empty;
        public IReadOnlyList<MenuResponse> Menus { get; init; } = [];
    }

    internal sealed record MenuResponse
    {
        public Guid MenuId { get; init; }
        public string Title { get; init; } = string.Empty;
        public IReadOnlyList<ItemResponse> Items { get; init; } = [];
    }

    internal sealed record ItemResponse(
        Guid ItemId,
        string Title,
        string? Caption,
        string? Category,
        string? PhotoUrl,
        decimal Price);

    private sealed class Handler(IUserExperienceRepository repository)
        : ICommandHandler<Query, Response>
    {
        public async Task<Response> ExecuteAsync(Query query, CancellationToken ct)
        {
            var result = await repository.GetCafeMenuAsync(query.CafeId, ct)
                ?? throw new ResourceNotFoundException("The cafe was not found.");
            return new Response
            {
                CafeId = result.Cafe.Id,
                CafeName = result.Cafe.Name,
                Menus = result.Menus.Select(menu => new MenuResponse
                {
                    MenuId = menu.Id,
                    Title = menu.Name,
                    Items = result.Items.Where(item => item.MenuId == menu.Id).Select(item =>
                        new ItemResponse(
                            item.Id,
                            item.Name,
                            item.Description,
                            item.CategoryName,
                            item.ImageUrl,
                            item.Price)).ToList()
                }).ToList()
            };
        }
    }
}
