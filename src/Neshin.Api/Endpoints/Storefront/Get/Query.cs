using FastEndpoints;

namespace Neshin.Api.Endpoints.Storefront.Get;

internal sealed record Query : ICommand<Query.Response>
{
    public Guid BranchId { get; init; }

    private sealed class Handler(
        IPublicExperienceRepository repository,
        IRequestContext requestContext)
        : ICommandHandler<Query, Response>
    {
        public async Task<Response> ExecuteAsync(Query query, CancellationToken ct)
        {
            var data = await repository.GetStorefrontAsync(query.BranchId, ct)
                ?? throw new Neshin.Application.Common.ResourceNotFoundException(
                    "The storefront was not found.");
            requestContext.SetPublicStorefrontCache();
            return new Response
            {
                BranchId = data.Branch.Id,
                ClientId = data.ClientId,
                Name = data.Branch.Name,
                Description = data.Branch.Description,
                Address = data.Branch.Address,
                PublicPhoneNumber = data.Branch.PublicPhoneNumber,
                LogoUrl = data.Branch.LogoUrl,
                CoverImageUrl = data.Branch.CoverImageUrl,
                AcceptsAppOrders = data.Branch.AcceptsAppOrders,
                AllowsPayAtVenue = data.Branch.AllowsPayAtVenue,
                Menus = data.Menus.Select(menu => new MenuResponse
                {
                    Id = menu.Id,
                    Name = menu.Name,
                    Items = data.MenuItems
                        .Where(item => item.MenuId == menu.Id)
                        .Select(item => new MenuItemResponse
                        {
                            Id = item.Id,
                            Name = item.Name,
                            Description = item.Description,
                            CategoryName = item.CategoryName,
                            ImageUrl = item.ImageUrl,
                            Price = item.Price,
                            DisplayOrder = item.DisplayOrder
                        }).ToList()
                }).ToList(),
                Events = data.Events.Select(venueEvent => new EventResponse
                {
                    Id = venueEvent.Id,
                    Title = venueEvent.Title,
                    Description = venueEvent.Description,
                    ImageUrl = venueEvent.ImageUrl,
                    StartsAtUtc = venueEvent.StartsAtUtc,
                    EndsAtUtc = venueEvent.EndsAtUtc
                }).ToList()
            };
        }
    }

    internal sealed record Response
    {
        public Guid BranchId { get; init; }
        public Guid ClientId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string? Address { get; init; }
        public string? PublicPhoneNumber { get; init; }
        public string? LogoUrl { get; init; }
        public string? CoverImageUrl { get; init; }
        public bool AcceptsAppOrders { get; init; }
        public bool AllowsPayAtVenue { get; init; }
        public IReadOnlyList<MenuResponse> Menus { get; init; } = [];
        public IReadOnlyList<EventResponse> Events { get; init; } = [];
    }

    internal sealed record MenuResponse
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public IReadOnlyList<MenuItemResponse> Items { get; init; } = [];
    }

    internal sealed record MenuItemResponse
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string? CategoryName { get; init; }
        public string? ImageUrl { get; init; }
        public decimal Price { get; init; }
        public int DisplayOrder { get; init; }
    }

    internal sealed record EventResponse
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string? ImageUrl { get; init; }
        public DateTime StartsAtUtc { get; init; }
        public DateTime EndsAtUtc { get; init; }
    }
}
