using Application.Common.Interfaces;
using Domain.Dtos;
using MediatR;

namespace Application.Features.Products.Queries;

// Query
public sealed record GetAllProductsQuery : IRequest<List<ProductDto>>;

// Handler
public class GetAllProductsQueryHandler(IProductRepository productRepository) : IRequestHandler<GetAllProductsQuery, List<ProductDto>>
{
    public async Task<List<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken ct)
    {
        var products = await productRepository.GetAllAsync();

        var productDtos = products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price
        }).ToList();

        return productDtos;
    }
}

