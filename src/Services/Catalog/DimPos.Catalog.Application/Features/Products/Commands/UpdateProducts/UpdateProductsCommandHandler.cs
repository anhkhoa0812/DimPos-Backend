using DimPos.Catalog.Application.Common.Exceptions;
using DimPos.Catalog.Application.Services.Interface;
using DimPos.Catalog.Domain.Models.Common;
using DimPos.Catalog.Infrastructure.Persistence;
using DimPos.Catalog.Infrastructure.Repositories;
using DimPos.Catalog.Infrastructure.Repositories.Interface;
using Mediator;

namespace DimPos.Catalog.Application.Features.Products.Commands.UpdateProducts;

public class UpdateProductsCommandHandler : IRequestHandler<UpdateProductsCommand, ApiResponse>
{
    private readonly IUnitOfWork<CatalogContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    
    public UpdateProductsCommandHandler(IUnitOfWork<CatalogContext> unitOfWork, ILogger logger, IClaimService claimService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _claimService = claimService;
    }
    public async ValueTask<ApiResponse> Handle(UpdateProductsCommand request, CancellationToken cancellationToken)
    {
        _logger.Information("BEGIN: UpdateProductsCommandHandler.Handle - ProductId: {ProductId}", request.ProductId);
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if(brandId == Guid.Empty)
            throw new BadHttpRequestException("Không tìm thấy thông tin thương hiệu trong yêu cầu.");
        var product = await _unitOfWork.GetRepository<Domain.Entities.Products>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.ProductId && x.BrandId == brandId
        );
        if (product == null)
            throw new NotFoundException("Không tìm thấy sản phẩm với ID đã cung cấp.");
        product.Name = request.UpdateProducts.Name ?? product.Name;
        product.Description = request.UpdateProducts.Description ?? product.Description;
        product.AlternativeCode = request.UpdateProducts.AlternativeCode ?? product.AlternativeCode;
        product.Status = request.UpdateProducts.Status ?? product.Status;
        product.IsAvailable = request.UpdateProducts.IsAvailable ?? product.IsAvailable;
        product.DisplayOrder = request.UpdateProducts.DisplayOrder ?? product.DisplayOrder;
        product.IsMenuDisplay = request.UpdateProducts.IsMenuDisplay ?? product.IsMenuDisplay;
        product.IsMostOrdered = request.UpdateProducts.IsMostOrdered ?? product.IsMostOrdered;
        product.SaleType = request.UpdateProducts.SaleType ?? product.SaleType;
        product.Note = request.UpdateProducts.Note ?? product.Note;
        
        _unitOfWork.GetRepository<Domain.Entities.Products>().UpdateAsync(product);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        _logger.Information("END: UpdateProductsCommandHandler.Handle - ProductId: {ProductId}, Success: {Success}", request.ProductId, isSuccess);
        if(!isSuccess)
            throw new Exception("Cập nhật sản phẩm không thành công.");
        return new ApiResponse()
        {
            Status = 200,
            Message = "Cập nhật sản phẩm thành công.",
            Data = null
        };
    }
}