using Application.Common.Interfaces;
using Domain.Dtos;
using MediatR;

namespace Application.Features.Products.Queries;

// Query
public sealed record GetProductByIdQuery(Guid id) : IRequest<ProductDto?>;

// Handler
public class GetProductByIdQueryHandler(IProductRepository productRepository) : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken ct)
    {
        var product = await productRepository.GetByIdAsync(request.id);

        if(product is null) return null;

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price
        };
    }
}