using System.Net;
using DimPos.Catalog.Application.Common.Mapper;
using DimPos.Catalog.Domain.Entities;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Catalog.Application.Features.Products.Commands.CreateProducts;

public class CreateProductsCommandHandler : IRequestHandler<CreateProductsCommand, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;

    public CreateProductsCommandHandler(IUnitOfWork<CatalogContext> unitOfWork,
        ILogger logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    public async ValueTask<ApiResponse> Handle(CreateProductsCommand request, CancellationToken cancellationToken)
    {
        _logger.Information($"BEGIN: {nameof(CreateProductsCommandHandler)} - {DateTime.UtcNow}");
        
        var product = ProductMapper.ToProducts(request);
        product.Id = Guid.NewGuid();
        product.IsMenuDisplay = false;
        product.IsMostOrdered = false;
        product.ProductVariants = new List<ProductVariants>();
        if (request.ProductVariants != null)
        {
            foreach (var productVariant in request.ProductVariants)
            {
                var productVariants = ProductVariantMapper.ToPoProductVariants(productVariant);
                productVariants.Id = Guid.NewGuid();
                productVariants.ProductId = product.Id;
                productVariants.IsActive = false;
                productVariants.IsMenuDisplay = false;
                product.ProductVariants.Add(productVariants);
            }
            product.IsHasVariants = true;
        }
        else
        {
            if (request.Price == null)
            {
                return new ApiResponse()
                {
                    Status = HttpStatusCode.BadRequest,
                    Message = "Giá sản phẩm không được để trống",
                };
            }
            product.IsHasVariants = false;
            //Chưa set Status
            var productVariant = new ProductVariants()
            {
                Id = Guid.NewGuid(),
                IsMenuDisplay = false,
                Code = request.Code,
                Name = request.Name,
                AlternativeCode = request.AlternativeCode,
                ProductId = product.Id,
                Price = request.Price,
                PriceCOGS = request.PriceCOGS,
                IsActive = false,
                DiscountPercent = request.DiscountPercent,
                DiscountPrice = request.DiscountPrice,
                DisplayOrder = request.DisplayOrder,
            };
            // await _unitOfWork.GetRepository<ProductVariants>().InsertAsync(productVariant);
            product.ProductVariants.Add(productVariant);
        }

        if (request.ModifierGroupIds != null)
        {
            foreach (var modifierGroupId in request.ModifierGroupIds)
            {
                var modifierGroup = await _unitOfWork.GetRepository<ModifierGroups>().SingleOrDefaultAsync(
                    predicate: x => x.Id == modifierGroupId
                );
                if (modifierGroup != null)
                {
                    var productModifierGroup = new ProductModifierGroups()
                    {
                        Id = Guid.NewGuid(),
                        ProductId = product.Id,
                        ModifierGroupId = modifierGroup.Id
                    };
                    await _unitOfWork.GetRepository<ProductModifierGroups>().InsertAsync(productModifierGroup);
                }
            }
        }
        await _unitOfWork.GetRepository<Domain.Entities.Products>().InsertAsync(product);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        _logger.Information($"END: {nameof(CreateProductsCommandHandler)} - {DateTime.UtcNow}");
        if (isSuccess)
        {
            return new ApiResponse()
            {
                Status = HttpStatusCode.Created,
                Message = "Create product successfully",
                Data = ProductMapper.ToProductResponse(product)
            };
        }
        return new ApiResponse()
        {
            Status = HttpStatusCode.InternalServerError,
            Message = "Create product failed",
        };
    }
}