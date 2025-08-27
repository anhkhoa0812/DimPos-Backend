using ClosedXML.Excel;
using DimPos.Brand.Application.Common.Protos;
using DimPos.Media.Application.Common.Protos;
using DimPos.Order.Application.Services.Interface;
using DimPos.Order.Domain.Entities;
using DimPos.Order.Domain.Enums;
using DimPos.Order.Domain.Models.Common;
using DimPos.Order.Domain.Models.Response;
using DimPos.Order.Infrastructure.Persistence;
using DimPos.Order.Infrastructure.Repositories.Interface;
using DimPos.Payment.Application.Common.Protos;
using DimPos.Store.Application.Common.Protos;
using Google.Protobuf;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace DimPos.Order.Application.Features.Dashboards.Query.ExportExcelForDashboardBrand;

public class ExportExcelForDashboardBrandQueryHandler : IRequestHandler<ExportExcelForDashboardBrandQuery, ApiResponse>
{
    private readonly IUnitOfWork<OrderContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly BrandGrpcService.BrandGrpcServiceClient _brandGrpcService;
    private readonly StoreGrpcService.StoreGrpcServiceClient _storeGrpcService;
    private readonly PaymentGrpcService.PaymentGrpcServiceClient _paymentGrpcService;
    private readonly MediaGrpcService.MediaGrpcServiceClient _mediaGrpcService;
    
    public ExportExcelForDashboardBrandQueryHandler(IUnitOfWork<OrderContext> unitOfWork, ILogger logger, IClaimService claimService,
        BrandGrpcService.BrandGrpcServiceClient brandGrpcService,
        StoreGrpcService.StoreGrpcServiceClient storeGrpcService,
        PaymentGrpcService.PaymentGrpcServiceClient paymentGrpcService,
        MediaGrpcService.MediaGrpcServiceClient mediaGrpcService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _brandGrpcService = brandGrpcService ?? throw new ArgumentNullException(nameof(brandGrpcService));
        _storeGrpcService = storeGrpcService ?? throw new ArgumentNullException(nameof(storeGrpcService));
        _paymentGrpcService = paymentGrpcService ?? throw new ArgumentNullException(nameof(paymentGrpcService));
        _mediaGrpcService = mediaGrpcService ?? throw new ArgumentNullException(nameof(mediaGrpcService));
    }
    
    public async ValueTask<ApiResponse> Handle(ExportExcelForDashboardBrandQuery request, CancellationToken cancellationToken)
    {
        var brandId = _claimService.GetBrandId ?? Guid.Empty;
        if (brandId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy thông tin thương hiệu");
        }

        var brand = await _brandGrpcService.GetBrandDetailByIdAsync(new GetBrandDetailByIdRequest()
        {
            BrandId = brandId.ToString()
        });
        
        var orders = await _unitOfWork.GetRepository<Orders>().GetListAsync(
            predicate: x => x.BrandId == brandId 
                            && DateOnly.FromDateTime(x.CreatedDate) >= request.FromDate
                            && DateOnly.FromDateTime(x.CreatedDate) <= request.ToDate
                            && x.Status == EOrderStatus.Completed,
            include: x => x.Include(x => x.OrderItems)
        );

        var storeIds = orders.Select(x => x.StoreId.ToString()).Distinct().ToList();
        
        var stores = await _storeGrpcService.GetListStoreByStoreIdsAsync(new GetListStoreByStoreIdsRequest()
        {
            StoreIds = { storeIds }
        });
        
        var systemPaymentMethods = await _paymentGrpcService.GetAllSystemPaymentMethodsAsync(new GetAllSystemPaymentMethodsRequest());
        var systemPaymentMethodDict = systemPaymentMethods.SystemPaymentMethods.ToDictionary(x => x.PaymentMethod, x => Guid.Parse(x.SystemPaymentMethodId));
        
        var storeOrdersMapping = orders
            .GroupBy(order => order.StoreId)
            .ToDictionary(
                group => group.Key,
                group => group.ToList()
            );
        var excelResponse = new List<ExportExcelForDashboardBrandResponse>();
        foreach (var store in stores.Stores)
        {
            var storeOrders = storeOrdersMapping[Guid.Parse(store.Id)];
            var response = new ExportExcelForDashboardBrandResponse()
            {
                TotalDineInRevenue = 0,
                TotalTakeAwayRevenue = 0,
                TotalSubTotalRevenue = 0,
                TotalRevenue = 0,
                TotalDiscountRevenue = 0,
                TotalDineInOrders = 0,
                TotalTakeAwayOrders = 0,
                TotalOrders = 0,
                AverageOrderValue = 0,
                AverageDineInOrderValue = 0,
                AverageTakeAwayOrderValue = 0,
                AverageItemsPerOrder = 0,
                AverageDineInItemsPerOrder = 0,
                AverageTakeAwayItemsPerOrder = 0
            };
            response.TotalDineInRevenue = storeOrders.Where(x => x.Type == EOrderType.DineIn).Sum(x => x.TotalAmount); 
            response.TotalTakeAwayRevenue = storeOrders.Where(x => x.Type == EOrderType.TakeAway).Sum(x => x.TotalAmount); 
            response.TotalRevenue = storeOrders.Sum(x => x.TotalAmount); 
            response.TotalDiscountRevenue = storeOrders.Sum(x => x.DiscountAmount); 
            response.TotalSubTotalRevenue = storeOrders.Sum(x => x.SubTotalAmount); 
            response.TotalDineInOrders = storeOrders.Count(x => x.Type == EOrderType.DineIn);
            response.TotalTakeAwayOrders = storeOrders.Count(x => x.Type == EOrderType.TakeAway);
            response.TotalOrders = storeOrders.Count;
            response.AverageOrderValue = response.TotalOrders > 0 ? Math.Round(response.TotalRevenue / response.TotalOrders, 2) : 0;
            response.AverageDineInOrderValue = response.TotalDineInOrders > 0 ? Math.Round(response.TotalDineInRevenue / response.TotalDineInOrders, 2) : 0;
            response.AverageTakeAwayOrderValue = response.TotalTakeAwayOrders > 0 ? Math.Round(response.TotalTakeAwayRevenue / response.TotalTakeAwayOrders, 2) : 0;
            response.AverageItemsPerOrder = (decimal) (storeOrders.Count > 0 ? Math.Round(orders.Average(x => x.OrderItems.Count), 2) : 0);
            response.AverageDineInItemsPerOrder = response.TotalDineInOrders > 0 ? (decimal)Math.Round(orders.Where(x => x.Type == EOrderType.DineIn).Average(x => x.OrderItems.Count)) : 0;
            response.AverageTakeAwayItemsPerOrder = response.TotalTakeAwayOrders > 0 ? (decimal)Math.Round(orders.Where(x => x.Type == EOrderType.TakeAway).Average(x => x.OrderItems.Count)) : 0; 
            response.TotalCashRevenue = orders.Where(x => x.SystemPaymentMethodId == systemPaymentMethodDict[PaymentMethod.Cash]).Sum(x => x.TotalAmount); 
            response.TotalQrCodeRevenue = orders.Where(x => x.SystemPaymentMethodId == systemPaymentMethodDict[PaymentMethod.QrVietqr]).Sum(x => x.TotalAmount); 
            response.TotalQrEdcRevenue = orders.Where(x => x.SystemPaymentMethodId == systemPaymentMethodDict[PaymentMethod.QrEdc]).Sum(x => x.TotalAmount); 
            response.TotalCardEdcRevenue = orders.Where(x => x.SystemPaymentMethodId == systemPaymentMethodDict[PaymentMethod.CardEdc]).Sum(x => x.TotalAmount);
            
            response.StoreName = store.Name;
            excelResponse.Add(response);
        }

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Báo cáo tổng quan");
        
        SetupHeader(worksheet, brand.Name, brand.Code, brand.Address, request.FromDate, request.ToDate);
        
        SetupDataTable(worksheet, excelResponse);
        
        worksheet.Columns().AdjustToContents();
        
        using var memoryStream = new MemoryStream();
        
        workbook.SaveAs(memoryStream);
        
        var byteString = ByteString.CopyFrom(memoryStream.ToArray());
            
        using var call = _mediaGrpcService.UploadExcel(cancellationToken: cancellationToken);
        await call.RequestStream.WriteAsync(new UpdateExcelRequest()
        {
            ChunkData = byteString
        });
        await call.RequestStream.CompleteAsync();
            
        var uploadExcelGrpcResponse = await call.ResponseAsync;
        var imageResponse = uploadExcelGrpcResponse.Url;
        if (string.IsNullOrEmpty(imageResponse))
            throw new Exception("Lỗi khi tải ảnh lên");

        return new ApiResponse()
        {
            Status = StatusCodes.Status200OK,
            Message = "Xuất file excel thành công",
            Data = imageResponse
        };
    }

    private void SetupHeader(IXLWorksheet worksheet, string brandName, string brandCode, string address,
        DateOnly fromDate, DateOnly toDate)
    {
        worksheet.Cell("A1").Value = brandName;
        worksheet.Cell("A1").Style.Font.Bold = true;
        worksheet.Cell("A1").Style.Font.FontSize = 14;
        worksheet.Range("A1:E1").Merge();
        worksheet.Cell("A1").Style.Border.OutsideBorder = XLBorderStyleValues.Thick;

        worksheet.Cell("A2").Value = $"Mã thương hiệu : {brandCode}";
        worksheet.Cell("A2").Style.Font.Bold = true;
        worksheet.Range("A2:E2").Merge();
        worksheet.Cell("A2").Style.Border.OutsideBorder = XLBorderStyleValues.Thick;

        worksheet.Cell("A3").Value = $"Địa Chỉ : {address}";
        worksheet.Cell("A3").Style.Font.Bold = true;
        worksheet.Range("A3:E3").Merge();
        worksheet.Cell("A3").Style.Border.OutsideBorder = XLBorderStyleValues.Thick;

        worksheet.Cell("A4").Value = $"Thời gian thống kê: {fromDate:dd/MM/yyyy} - {toDate:dd/MM/yyyy}";
        worksheet.Cell("A4").Style.Font.Bold = true;
        worksheet.Range("A4:E4").Merge();
        worksheet.Cell("A4").Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
        
        worksheet.Cell("A5").Value = $"Thời gian xuất báo cáo : {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
        worksheet.Cell("A5").Style.Font.Bold = true;
        worksheet.Range("A5:E5").Merge();
        worksheet.Cell("A5").Style.Border.OutsideBorder = XLBorderStyleValues.Thick;

        worksheet.Row(6).Height = 10;
    }

    private void SetupDataTable(IXLWorksheet worksheet, List<ExportExcelForDashboardBrandResponse> data)
    {
        int currentRow = 7;
        
        foreach (var storeData in data)
        {
            // Add Store Name Header
            worksheet.Cell(currentRow, 1).Value = $"CỬA HÀNG: {storeData.StoreName}";
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 1).Style.Font.FontSize = 12;
            worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
            worksheet.Range(currentRow, 1, currentRow, 5).Merge();
            worksheet.Cell(currentRow, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
            worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            currentRow++;
            
            // Add Table Headers
            var headers = new[]
            {
                "STT",
                "Tên Chỉ Số",
                "Dine In",
                "Take Away", 
                "Tổng Cộng"
            };
            
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(currentRow, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
            }
            currentRow++;

            int stt = 1;

            // Add Store Data Rows
            AddDataRow(worksheet, currentRow++, stt++, "Doanh Thu (VNĐ)", 
                storeData.TotalDineInRevenue.ToString("N0"), 
                storeData.TotalTakeAwayRevenue.ToString("N0"), 
                storeData.TotalRevenue.ToString("N0"));

            AddDataRow(worksheet, currentRow++, stt++, "Doanh Thu Trước Chiết Khấu (VNĐ)", 
                "", "", storeData.TotalSubTotalRevenue.ToString("N0"));

            AddDataRow(worksheet, currentRow++, stt++, "Chiết Khấu (VNĐ)", 
                "", "", storeData.TotalDiscountRevenue.ToString("N0"));

            AddDataRow(worksheet, currentRow++, stt++, "Số Đơn Hàng", 
                storeData.TotalDineInOrders.ToString(), 
                storeData.TotalTakeAwayOrders.ToString(), 
                storeData.TotalOrders.ToString());

            AddDataRow(worksheet, currentRow++, stt++, "Giá Trị Đơn Hàng Trung Bình (VNĐ)", 
                storeData.AverageDineInOrderValue.ToString("N0"), 
                storeData.AverageTakeAwayOrderValue.ToString("N0"), 
                storeData.AverageOrderValue.ToString("N0"));

            AddDataRow(worksheet, currentRow++, stt++, "Số Sản Phẩm Trung Bình/Đơn", 
                storeData.AverageDineInItemsPerOrder.ToString("N2"), 
                storeData.AverageTakeAwayItemsPerOrder.ToString("N2"), 
                storeData.AverageItemsPerOrder.ToString("N2"));

            AddDataRow(worksheet, currentRow++, stt++, "Doanh Thu Tiền Mặt (VNĐ)", 
                "", "", storeData.TotalCashRevenue.ToString("N0"));

            AddDataRow(worksheet, currentRow++, stt++, "Doanh Thu QR Code (VNĐ)", 
                "", "", storeData.TotalQrCodeRevenue.ToString("N0"));

            AddDataRow(worksheet, currentRow++, stt++, "Doanh Thu QR EDC (VNĐ)", 
                "", "", storeData.TotalQrEdcRevenue.ToString("N0"));

            AddDataRow(worksheet, currentRow++, stt++, "Doanh Thu Thẻ EDC (VNĐ)", 
                "", "", storeData.TotalCardEdcRevenue.ToString("N0"));

            // Add separator row between stores
            if (data.Count > 1 && storeData != data.Last())
            {
                currentRow += 2; // Add extra space between stores
            }
        }
    }
    private void AddDataRow(IXLWorksheet worksheet, int row, int stt, string metricName, 
        string dineInValue, string takeAwayValue, string totalValue)
    {
        worksheet.Cell(row, 1).Value = stt;
        worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        worksheet.Cell(row, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

        worksheet.Cell(row, 2).Value = metricName;
        worksheet.Cell(row, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

        worksheet.Cell(row, 3).Value = dineInValue;
        worksheet.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        worksheet.Cell(row, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

        worksheet.Cell(row, 4).Value = takeAwayValue;
        worksheet.Cell(row, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        worksheet.Cell(row, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

        worksheet.Cell(row, 5).Value = totalValue;
        worksheet.Cell(row, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        worksheet.Cell(row, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
    }
    
}