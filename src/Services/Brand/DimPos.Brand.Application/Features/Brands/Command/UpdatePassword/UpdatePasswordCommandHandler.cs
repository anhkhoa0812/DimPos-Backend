using Confluent.Kafka;
using DimPos.Brand.Application.Common.Utils;
using DimPos.Brand.Domain.Models.Common;
using DimPos.Brand.Infrastructure.Persistence;
using DimPos.Brand.Infrastructure.Repositories.Interface;
using MassTransit;
using Mediator;
using Microsoft.EntityFrameworkCore;
using SharedProject.Events.UpdateBrandPassword;

namespace DimPos.Brand.Application.Features.Brands.Command.UpdatePassword;

public class UpdatePasswordCommandHandler : IRequestHandler<UpdatePasswordCommand, ApiResponse>
{
    private readonly IUnitOfWork<BrandContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly ITopicProducer<Null, UpdateBrandPasswordRequestModel> _topicProducer;
    public UpdatePasswordCommandHandler(IUnitOfWork<BrandContext> unitOfWork, ILogger logger,
        ITopicProducer<Null, UpdateBrandPasswordRequestModel> topicProducer)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _topicProducer = topicProducer ?? throw new ArgumentNullException(nameof(topicProducer));
    }
    
    public async ValueTask<ApiResponse> Handle(UpdatePasswordCommand request, CancellationToken cancellationToken)
    {
        var brand = await _unitOfWork.GetRepository<Domain.Entities.Brands>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.BrandId,
            include: x => x.Include(x => x.BrandAccounts)
        );

        if (brand == null)
        {
            throw new BadHttpRequestException("Không tìm thấy thương hiệu");
        }

        var account = brand.BrandAccounts.FirstOrDefault();
        if (account == null)
        {
            throw new BadHttpRequestException("Không tìm thấy tài khoản của thương hiệu");
        }
        var (hashPassword, saltPassword) = PasswordUtil.HashPassword(request.Password);
        var updateBrandPasswordRequestModel = new UpdateBrandPasswordRequestModel()
        {
            CorrelationId = Guid.CreateVersion7(),
            AccountId = account.AccountId,
            HashPassword = hashPassword,
            SaltPassword = saltPassword
        };

        await _topicProducer.Produce(
            null,
            updateBrandPasswordRequestModel,
            cancellationToken: cancellationToken
        ).ConfigureAwait(false);
        _logger.Information("UpdateBrandPasswordCommandHandler.Handle called with request: {@Request}", request);
        return new ApiResponse()
        {
            Status = 200,
            Message = "Cập nhật mật khẩu thành công.",
            Data = brand.Id
        };
    }
}