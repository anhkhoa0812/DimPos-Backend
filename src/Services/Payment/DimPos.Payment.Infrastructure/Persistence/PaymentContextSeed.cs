using DimPos.Payment.Domain.Entities;
using DimPos.Payment.Domain.Enums;
using DimPos.Payment.Infrastructure.Utils;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Payment.Infrastructure.Persistence;

public class PaymentContextSeed
{
    private readonly ILogger _logger;
    private readonly PaymentContext _context;
    
    public PaymentContextSeed(ILogger logger, PaymentContext context)
    {
        _logger = logger;
        _context = context;
    }
    public async Task InitializeAsync()
    {
        try
        {
            if (_context.Database.IsSqlServer())
            {
                await _context.Database.MigrateAsync();
            }
        }
        catch (Exception e)
        {
            _logger.Error(e, "An error occurred while migrating the database");
            throw;
        }
    }
    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.Error(e, "An error occurred while seeding the database");
            throw;
        }
    }
    public async Task TrySeedAsync()
    {
        if (!_context.SystemPaymentMethodTypes.Any())
        {
            await _context.SystemPaymentMethodTypes.AddRangeAsync(
                new SystemPaymentMethods()
                {
                    Id = Guid.Parse("0197cb0e-ee76-73db-ae8c-eaedab6707c1"),
                    Code = "CASH",
                    Name = "Tiền mặt",
                    Description = "Phương thức thanh toán bằng tiền mặt",
                    Type = ESystemPaymentMethod.CASH,
                    ConfigurationSchema = "",
                    IsGloballyActive = true,
                    LogoUrl = "",
                    CreatedDate = TimeUtil.GetCurrentSEATime()
                },
                new SystemPaymentMethods()
                {
                    Id = Guid.Parse("0197cb14-4053-71cf-ae7e-3fa5504130ed"),
                    Code = "QRVIETQR",
                    Name = "QR VietQR",
                    Description = "Phương thức thanh toán bằng mã QR VietQR",
                    Type = ESystemPaymentMethod.QR_VIETQR,
                    ConfigurationSchema = "",
                    IsGloballyActive = true,
                    LogoUrl = "",
                    CreatedDate = TimeUtil.GetCurrentSEATime()
                },
                new SystemPaymentMethods()
                {
                    Id = Guid.Parse("0197cb15-7e22-704f-8ecc-06345de987f3"),
                    Code = "QREDC",
                    Name = "QR EDC",
                    Description = "Phương thức thanh toán bằng mã QR EDC",
                    Type = ESystemPaymentMethod.QR_EDC,
                    ConfigurationSchema = "",
                    IsGloballyActive = true,
                    LogoUrl = "",
                    CreatedDate = TimeUtil.GetCurrentSEATime()
                },
                new SystemPaymentMethods()
                {
                    Id = Guid.Parse("0197cb16-1ac4-701d-86fc-5bf82b4700b4"),
                    Code = "CARDEDC",
                    Name = "Thẻ EDC",
                    Description = "Phương thức thanh toán bằng thẻ EDC",
                    Type = ESystemPaymentMethod.CARD_EDC,
                    ConfigurationSchema = "",
                    IsGloballyActive = true,
                    LogoUrl = "",
                    CreatedDate = TimeUtil.GetCurrentSEATime()
                }
            );
        }
    }
}