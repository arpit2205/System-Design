using Application.Common.Interfaces;
using Domain.Dtos;
using Domain.Entites;
using MediatR;

namespace Application.Features.Products.Commands;

// Command
public sealed record CreateProductCommand(CreateProductDto createProductDto) : IRequest<Guid>;

// Handler
public class CreateProductCommandHandler(IProductRepository productRepository) : IRequestHandler<CreateProductCommand, Guid>
{
    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var product = new Product { Id = Guid.NewGuid(), Name = request.createProductDto.Name, Price = request.createProductDto.Price };

        await productRepository.AddAsync(product);

        return product.Id;
    }
}

