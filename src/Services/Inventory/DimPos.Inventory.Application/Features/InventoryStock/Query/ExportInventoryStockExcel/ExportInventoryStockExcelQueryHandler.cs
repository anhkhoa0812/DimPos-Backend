using ClosedXML.Excel;
using DimPos.Catalog.Application.Common.Protos;
using DimPos.Inventory.Application.Services.Interface;
using DimPos.Inventory.Domain.Models.Common;
using DimPos.Inventory.Domain.Models.Response;
using DimPos.Inventory.Infrastructure.Persistence;
using DimPos.Inventory.Infrastructure.Repositories.Interface;
using DimPos.Inventory.Infrastructure.Utils;
using DimPos.Media.Application.Common.Protos;
using DimPos.Store.Application.Common.Protos;
using Google.Protobuf;
using Mediator;

namespace DimPos.Inventory.Application.Features.InventoryStock.Query.ExportInventoryStockExcel;

public class ExportInventoryStockExcelQueryHandler : IRequestHandler<ExportInventoryStockExcelQuery, ApiResponse>
{
    private readonly IUnitOfWork<InventoryContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly StoreGrpcService.StoreGrpcServiceClient _storeGrpcService;
    private readonly CatalogGrpcService.CatalogGrpcServiceClient _catalogGrpcService;
    private readonly HttpClient _httpClient;
    private readonly MediaGrpcService.MediaGrpcServiceClient _mediaGrpcService;
    public ExportInventoryStockExcelQueryHandler(IUnitOfWork<InventoryContext> unitOfWork, ILogger logger, IClaimService claimService,
        StoreGrpcService.StoreGrpcServiceClient storeGrpcService,
        CatalogGrpcService.CatalogGrpcServiceClient catalogGrpcService,
        HttpClient httpClient,
        MediaGrpcService.MediaGrpcServiceClient mediaGrpcService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _storeGrpcService = storeGrpcService ?? throw new ArgumentNullException(nameof(storeGrpcService));
        _catalogGrpcService = catalogGrpcService ?? throw new ArgumentNullException(nameof(catalogGrpcService));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _mediaGrpcService = mediaGrpcService ?? throw new ArgumentNullException(nameof(mediaGrpcService));
    }
    
    public async ValueTask<ApiResponse> Handle(ExportInventoryStockExcelQuery request, CancellationToken cancellationToken)
    {
        var storeId = _claimService.GetStoreId ?? Guid.Empty;
        if (storeId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy id của cửa hàng");
        }
        
        var inventoryStocks = await _unitOfWork.GetRepository<Domain.Entities.InventoryStock>().GetListAsync(
            predicate: x => x.StoreId == storeId
        );
        
        var storeDetail = await _storeGrpcService.GetStoreDetailByIdAsync(new GetStoreDetailByIdRequest()
        {
            StoreId = storeId.ToString()
        });
        if (storeDetail == null)
        {
            throw new BadHttpRequestException("Không tìm thấy thông tin cửa hàng");
        }

        var ingredientIds = inventoryStocks.Select(x => x.IngredientId.ToString()).Distinct().ToList();
        
        var ingredients = await _catalogGrpcService.GetIngredientsByIngredientIdsAsync(new GetIngredientsByIngredientIdsRequest()
        {
            IngredientIds = { ingredientIds }
        });

        var excelData = new List<GetInventoryStockByIdResponse>();

        foreach (var inventoryStock in inventoryStocks)
        {
            var ingredient = ingredients.Ingredients.First(x => x.Id == inventoryStock.IngredientId.ToString());
            excelData.Add(new GetInventoryStockByIdResponse()
            {
                Id = inventoryStock.Id,
                Quantity = inventoryStock.Quantity,
                ReOrderLevel = inventoryStock.ReOrderLevel,
                CreatedDate = inventoryStock.CreatedDate,
                LastModifiedDate = inventoryStock.LastModifiedDate,
                Ingredient = new IngredientsForGetInventoryStockByIdResponse()
                {
                    Id = Guid.Parse(ingredient.Id),
                    Name = ingredient.Name,
                    Sku = ingredient.Sku,
                    Code = ingredient.Code,
                    MeasureUnit = ingredient.MeasureUnit,
                    Description = ingredient.Description
                }
            });
        }
        excelData = excelData.OrderBy(x => x.Ingredient.Name).ToList();
        
        using var workbook = new XLWorkbook();
        
        var worksheet = workbook.Worksheets.Add("Inventory Stock");

        
        CreateHeader(worksheet, storeDetail);
            
        CreateDataTable(worksheet, excelData);
            
        FormatWorksheet(worksheet);
            
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

    private void CreateHeader(IXLWorksheet worksheet, GetStoreDetailByIdResponse store)
    {
        worksheet.Range("A1:A6").Merge();
    
        worksheet.Cell("B1").Value = store.Name ?? "N/A";
        worksheet.Cell("B1").Style.Font.Bold = true;
        worksheet.Cell("B1").Style.Font.FontSize = 16;
        worksheet.Cell("B1").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
    
        worksheet.Cell("B2").Value = $"Mã cửa hàng : {store.Code}";
        worksheet.Cell("B2").Style.Font.Bold = true;
        worksheet.Cell("B2").Style.Font.FontSize = 12;
        worksheet.Cell("B2").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        
        worksheet.Cell("B3").Value = $"Cửa Hàng Trưởng : {store.ManagerName ?? "N/A"}";
        worksheet.Cell("B3").Style.Font.Bold = true;
        worksheet.Cell("B3").Style.Font.FontSize = 12;
        worksheet.Cell("B3").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
    
        worksheet.Cell("B4").Value = $"Địa Chỉ : {store.Address ?? "N/A"}";
        worksheet.Cell("B4").Style.Font.Bold = true;
        worksheet.Cell("B4").Style.Font.FontSize = 12;
        worksheet.Cell("B4").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

        var datetime = TimeUtil.GetCurrentSEATime();
        string formattedDate = datetime.ToString("dd/MM/yyyy HH:mm:ss");

        worksheet.Cell("B5").Value = $"Thời gian xuất báo cáo : {formattedDate}";
        worksheet.Cell("B5").Style.Font.Bold = true;
        worksheet.Cell("B5").Style.Font.FontSize = 12;
        worksheet.Cell("B5").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        
        worksheet.Range("B1:E1").Merge();
        worksheet.Range("B2:E2").Merge();
        worksheet.Range("B3:E3").Merge();
        worksheet.Range("B4:E4").Merge();
        worksheet.Range("B5:E5").Merge();
        
        worksheet.Range("A1:E6").Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
    
        // Set minimum height for header area
        for (int i = 1; i <= 6; i++)
        {
            if (worksheet.Row(i).Height < 20)
                worksheet.Row(i).Height = 20;
        }
    }
    
    private void CreateDataTable(IXLWorksheet worksheet, List<GetInventoryStockByIdResponse> inventoryData)
    {
        int startRow = 8;
            
        // Create table headers
        worksheet.Cell(startRow, 1).Value = "STT";
        worksheet.Cell(startRow, 2).Value = "Tên Nguyên Vật Liệu";
        worksheet.Cell(startRow, 3).Value = "ĐƠN VỊ CỦA HÀNG\nKIỂM KÊ & NHẬP FILE";
        worksheet.Cell(startRow, 4).Value = "Khối Lượng";
        worksheet.Cell(startRow, 5).Value = "Ghi Chú";
            
        // Style headers
        var headerRange = worksheet.Range(startRow, 1, startRow, 5);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
        headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            
        // Add data rows
        int currentRow = startRow + 1;
        for (int i = 0; i < inventoryData.Count; i++)
        {
            var item = inventoryData[i];
                
            worksheet.Cell(currentRow, 1).Value = i + 1;
            worksheet.Cell(currentRow, 2).Value = item.Ingredient.Name;
            worksheet.Cell(currentRow, 3).Value = item.Ingredient.MeasureUnit;
            worksheet.Cell(currentRow, 4).Value = item.Quantity;
            worksheet.Cell(currentRow, 5).Value = "";
                
            // Style data rows
            var dataRange = worksheet.Range(currentRow, 1, currentRow, 5);
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                
            // Center align STT and Unit columns
            worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(currentRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                
            // Right align quantity
            worksheet.Cell(currentRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                
            currentRow++;
        }
    }
    
    private void FormatWorksheet(IXLWorksheet worksheet)
    {
        // Auto-fit columns
        worksheet.Columns().AdjustToContents();
            
        // Set specific column widths
        worksheet.Column(1).Width = 8;  // STT
        worksheet.Column(2).Width = 25; // Tên Nguyên Vật Liệu
        worksheet.Column(3).Width = 15; // Đơn vị
        worksheet.Column(4).Width = 12; // Khối lượng
        worksheet.Column(5).Width = 15; // Ghi chú
            
        // Set row height for header
        worksheet.Row(8).Height = 40;
            
        // Enable text wrapping for header
        worksheet.Range("A8:E8").Style.Alignment.WrapText = true;
    }
}