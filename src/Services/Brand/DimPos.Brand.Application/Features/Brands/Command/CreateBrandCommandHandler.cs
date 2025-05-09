using Confluent.Kafka;
using DimPos.Brand.Application.Common.Mapper;
using DimPos.Brand.Application.Common.Utils;
using DimPos.Brand.Domain.Entities;
using DimPos.Brand.Domain.Enums;
using DimPos.Brand.Domain.Models.Common;
using DimPos.Brand.Infrastructure.Persistence;
using DimPos.Brand.Infrastructure.Repositories.Interface;
using MassTransit;
using Mediator;
using SharedProject.Events.Account;

namespace DimPos.Brand.Application.Features.Brands.Command;

public class CreateBrandCommandHandler : IRequestHandler<CreateBrandCommand, ApiResponse>
{
    private readonly IUnitOfWork<BrandContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly ITopicProducer<Null, CreateBrandAccountModel> _producer;
    public CreateBrandCommandHandler(IUnitOfWork<BrandContext> unitOfWork, ILogger logger,
        ITopicProducer<Null, CreateBrandAccountModel> producer)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _producer = producer ?? throw new ArgumentNullException(nameof(producer));
    }
    public async ValueTask<ApiResponse> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = BrandMapper.ToBrands(request);
        brand.Id = Guid.CreateVersion7();
        brand.Status = EBrandStatus.Active;
        await _unitOfWork.GetRepository<Domain.Entities.Brands>().InsertAsync(brand);

        var accountId = Guid.CreateVersion7();
        var brandAccount = new BrandAccounts()
        {
            Id = Guid.CreateVersion7(),
            BrandId = brand.Id,
            AccountId = accountId
        };
        await _unitOfWork.GetRepository<BrandAccounts>().InsertAsync(brandAccount);
        
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        if (isSuccess)
        {
            var (hashPassword, saltPassword) = PasswordUtil.HashPassword(request.Password);
            var createBrandAccountModel = new CreateBrandAccountModel()
            {
                CorrelationId = Guid.CreateVersion7(),
                BrandId = brand.Id,
                AccountId = accountId,
                Code = brand.Code,
                Email = brand.Email,
                Username = request.Username,
                HashPassword = hashPassword,
                SaltPassword = saltPassword,
            };
            var correlationId = Guid.CreateVersion7();
            await _producer.Produce(
                key: null,
                createBrandAccountModel,
                cancellationToken: cancellationToken
            );
            return new ApiResponse()
            {
                Status = StatusCodes.Status201Created,
                Message = "Tạo mới thương hiệu thành công",
            };
        }
        return new ApiResponse()
        {
            Status = StatusCodes.Status500InternalServerError,
            Message = "Tạo mới thương hiệu thất bại",
        };
    }
}