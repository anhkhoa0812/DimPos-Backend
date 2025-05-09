using DimPos.Brand.Domain.Entities;
using DimPos.Brand.Infrastructure.Persistence;
using DimPos.Brand.Infrastructure.Repositories.Interface;
using MassTransit;
using SharedProject.Events.Account;

namespace DimPos.Brand.Application.Consumers;

public class RollbackBrandAccountConsumer : IConsumer<RollbackBrandAccountModel>
{
    private readonly ILogger _logger;
    private readonly IUnitOfWork<BrandContext> _unitOfWork;
    
    public RollbackBrandAccountConsumer(ILogger logger, IUnitOfWork<BrandContext> unitOfWork)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }
    
    public async Task Consume(ConsumeContext<RollbackBrandAccountModel> context)
    {
        _logger.Information("RollbackBrandAccountConsumer: {CorrelationId}", context.Message.CorrelationId);
        var brand = await _unitOfWork.GetRepository<Brands>().SingleOrDefaultAsync(
            predicate: x => x.Id == context.Message.BrandId
        );
        if (brand != null)
        {
            _unitOfWork.GetRepository<Brands>().DeleteAsync(brand);
        }
        var brandAccount = await _unitOfWork.GetRepository<BrandAccounts>().SingleOrDefaultAsync(
            predicate: x => x.BrandId == context.Message.BrandId && x.AccountId == context.Message.AccountId
        );
        if (brandAccount != null)
        {
            _unitOfWork.GetRepository<BrandAccounts>().DeleteAsync(brandAccount);
        }
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (isSuccess)
        {
            _logger.Information("RollbackBrandAccountConsumer: {CorrelationId} - Success", context.Message.CorrelationId);
        }
        else
        {
            _logger.Error("RollbackBrandAccountConsumer: {CorrelationId} - Failed", context.Message.CorrelationId);
        }
    }
}