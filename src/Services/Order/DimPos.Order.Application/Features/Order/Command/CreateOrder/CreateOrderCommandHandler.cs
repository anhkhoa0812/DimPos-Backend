using Confluent.Kafka;
using DimPos.Catalog.Application.Common.Protos;
using DimPos.Inventory.Application.Common.Protos;
using DimPos.Order.Application.Services.Interface;
using DimPos.Order.Domain.Entities;
using DimPos.Order.Domain.Enums;
using DimPos.Order.Domain.Models.Common;
using DimPos.Order.Domain.Models.Response;
using DimPos.Order.Infrastructure.Persistence;
using DimPos.Order.Infrastructure.Repositories.Interface;
using DimPos.Payment.Application.Common.Protos;
using DimPos.Promotion.Application.Common.Protos;
using DimPos.Store.Application.Common.Protos;
using MassTransit;
using Mediator;
using SharedProject.Events.Order.UpdateInventoryForSuccessOrder;

namespace DimPos.Order.Application.Features.Order.Command.CreateOrder;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, ApiResponse>
{
    private readonly IUnitOfWork<OrderContext> _unitOfWork;
    private readonly ILogger _logger;
    private readonly IClaimService _claimService;
    private readonly CatalogGrpcService.CatalogGrpcServiceClient _catalogGrpcService;
    private readonly StoreGrpcService.StoreGrpcServiceClient _storeGrpcService;
    private readonly PromotionGrpcService.PromotionGrpcServiceClient _promotionGrpcService;
    private readonly PaymentGrpcService.PaymentGrpcServiceClient _paymentGrpcService;
    private readonly InventoryGrpcService.InventoryGrpcServiceClient _inventoryGrpcService;
    private readonly ITopicProducer<Null, CreateOrderResponseModel> _topicProducer;
    public CreateOrderCommandHandler(IUnitOfWork<OrderContext> unitOfWork, ILogger logger, IClaimService claimService,
        CatalogGrpcService.CatalogGrpcServiceClient catalogGrpcService,
        StoreGrpcService.StoreGrpcServiceClient storeGrpcService,
        PromotionGrpcService.PromotionGrpcServiceClient promotionGrpcService,
        PaymentGrpcService.PaymentGrpcServiceClient paymentGrpcService,
        InventoryGrpcService.InventoryGrpcServiceClient inventoryGrpcService,
        ITopicProducer<Null, CreateOrderResponseModel> topicProducer)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
        _catalogGrpcService = catalogGrpcService ?? throw new ArgumentNullException(nameof(catalogGrpcService));
        _storeGrpcService = storeGrpcService ?? throw new ArgumentNullException(nameof(storeGrpcService));
        _promotionGrpcService = promotionGrpcService ?? throw new ArgumentNullException(nameof(promotionGrpcService));
        _paymentGrpcService = paymentGrpcService ?? throw new ArgumentNullException(nameof(paymentGrpcService));
        _inventoryGrpcService = inventoryGrpcService ?? throw new ArgumentNullException(nameof(inventoryGrpcService));
        _topicProducer = topicProducer ?? throw new ArgumentNullException(nameof(topicProducer));
    }
    
    public async ValueTask<ApiResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var storeId = _claimService.GetStoreId ?? Guid.Empty;
        if (storeId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy cửa hàng");
        }

        if (request.TableNumberDineIn != null)
        {
            var existingTableNumberDineInOrder = await _unitOfWork.GetRepository<Orders>().SingleOrDefaultAsync(
                predicate: x => x.StoreId == storeId &&
                                x.BrandId == request.BrandId &&
                                x.TableNumberDineIn == request.TableNumberDineIn &&
                                x.Status == EOrderStatus.PendingPayment &&
                                x.Type == EOrderType.DineIn
            );
            if (existingTableNumberDineInOrder != null)
            {
                throw new BadHttpRequestException($"Bàn {request.TableNumberDineIn} đã có đơn hàng đang chờ thanh toán");
            }
        }
        var accountId = _claimService.GetCurrentUserId;
        if (accountId == Guid.Empty)
        {
            throw new BadHttpRequestException("Không tìm thấy tài khoản người dùng");
        }
        var order = new Orders()
        {
            Id = Guid.CreateVersion7(),
            StoreId = storeId,
            BrandId = request.BrandId,
            Status = EOrderStatus.PendingPayment,
            TableNumberDineIn = request.TableNumberDineIn,
            PickupTime = request.PickupTime,
            CreatedByAccountId = accountId,
            Type = EOrderType.DineIn,
            IsNeedToUpdateInventory = false
        };
        if (request.CustomerId != null && request.CustomerId != Guid.Empty)
        {
            
        }
        
        var storeDetailGrpcResponse = await _storeGrpcService.GetTaxRateAndPaymentMethodConfigAsync(new GetTaxRateAndPaymentMethodConfigRequest()
        {
            StoreId = storeId.ToString(),
            BrandId = request.BrandId.ToString(),
            StorePaymentMethodConfigId = request.StorePaymentMethodConfigId.ToString()
        });
        if (storeDetailGrpcResponse.IsSuccess == false)
        {
            throw new BadHttpRequestException(storeDetailGrpcResponse.ErrorMessage);
        }
        order.FinancialShiftId = Guid.Parse(storeDetailGrpcResponse.FinancialShiftId);
        if (storeDetailGrpcResponse.Rate != 0)
        {
            order.AppliedTax = new AppliedTaxes()
            {
                Id = Guid.CreateVersion7(),
                OrderId = order.Id,
                TaxRateId = Guid.Parse(storeDetailGrpcResponse.TaxRateId),
                TaxNameSnapshot = storeDetailGrpcResponse.TaxRateName,
                TaxRateSnapshot = (decimal)storeDetailGrpcResponse.Rate,
            };
        }
        
        
        var orderItemsFromGrpc = await _catalogGrpcService.GetProductForOrderAsync(new GetProductForOrderRequest()
        {
            StoreId = storeId.ToString(),
            BrandId = request.BrandId.ToString(),
            ProductForOrders =
            {
                request.OrderItems.Select(x => new ProductForOrderRequest()
                {
                    Id = x.ProductVariantId.ToString(),
                    Quantity = x.Quantity,
                    Note = x.Note ?? string.Empty,
                    ModifierOptionIds =
                    {
                        x.ModifierOptionIds?.Select(m => m.ToString()) ?? new  List<string>()
                    }
                })
            }
        });
        var checkInventoryResponse = await _inventoryGrpcService.CheckInventoryForOrderAsync(new CheckInventoryForOrderRequest()
        {
            StoreId = storeId.ToString(),
            IngredientInventory =
            {
                // orderItemsFromGrpc.ProductForOrders.SelectMany(x => x.RecipeItems)
                //     .GroupBy(y => y.Ingredient.Id)
                //     .Select(group => new IngredientInventory()
                // {
                //     IngredientId = group.Key,
                //     Quantity = group.Sum(item => item.Quantity)
                // })
                orderItemsFromGrpc.ProductForOrders
                    .SelectMany(product => product.RecipeItems.Select(recipe => new
                    {
                        IngredientId = recipe.Ingredient.Id,
                        Quantity = recipe.Quantity * product.Quantity
                    }))
                    .GroupBy(x => x.IngredientId)
                    .Select(group => new IngredientInventory()
                    {
                        IngredientId = group.Key,
                        Quantity = group.Sum(item => item.Quantity)
                    })
            }
        });

        if (!checkInventoryResponse.IsValid)
        {
            var ingredientNames = orderItemsFromGrpc.ProductForOrders.SelectMany(x => x.RecipeItems)
                .Where(x => checkInventoryResponse.InsufficientIngredientIds.Contains(x.Ingredient.Id))
                .Select(x => x.Ingredient.Name).ToList();
            throw new BadHttpRequestException("Không đủ nguyên liệu trong kho cho đơn hàng. Nguyên liệu không đủ: " + string.Join(", ", ingredientNames));
        }
        var orderItems = orderItemsFromGrpc.ProductForOrders.Select(x => new OrderItems()
        {
            Id = Guid.CreateVersion7(),
            ProductVariantId = Guid.Parse(x.Id),
            Quantity = x.Quantity,
            Note = x.Note,
            ProductNameSnapshot = x.ProductName,
            ProductVariantNameSnapshot = x.ProductVariantName,
            UnitPriceSnapshot = (decimal)x.UnitPrice,
            OrderId = order.Id,
            TotalPriceBeforeItemDiscount = x.Quantity * ((decimal)x.UnitPrice + x.ModifierOptions.Sum(mo => (decimal)mo.DeltaPrice)),
            OrderItemSelectedOptions = x.ModifierOptions.Select(x => new OrderItemSelectedOptions()
            {
                Id = Guid.CreateVersion7(),
                ModifierOptionId = Guid.Parse(x.Id),
                ModifierGroupId = Guid.Parse(x.ModifierGroupId),
                ModifierOptionSnapshot = x.ModifierOptionName,
                ModifierGroupSnapshot = x.ModifierGroupName,
                PriceDeltaOptionSnapshot = (decimal) x.DeltaPrice
            }).ToList()
        }).ToList();
        order.OrderItems = orderItems;
        order.SubTotalAmount = order.OrderItems.Sum(x => x.TotalPriceBeforeItemDiscount);
        
        if (request.PromotionRuleIds != null && request.PromotionRuleIds.Any())
        {
            var promotionRules = await _promotionGrpcService.GetPromotionForOrderAsync(new GetPromotionForOrderRequest()
            {
                StoreId = storeId.ToString(),
                BrandId = request.BrandId.ToString(),
                SubtotalAmount = (float) order.SubTotalAmount,
                PromotionRuleIds = { request.PromotionRuleIds.Select(x => x.ToString()) },
                OrderItems =
                {
                    orderItems.Select(x => new OrderItemRequest()
                    {
                        ProductVariantId = x.ProductVariantId.ToString(),
                        Quantity = x.Quantity,
                        UnitPrice = (float) x.UnitPriceSnapshot,
                    })
                }
            });
            foreach (var promotionRule in promotionRules.Promotions)
            {
                var appliedOrderPromotion = new AppliedOrderPromotions()
                {
                    Id = Guid.CreateVersion7(),
                    OrderId = order.Id,
                    PromotionRuleId = Guid.Parse(promotionRule.Id),
                    DiscountAmountApplied = (decimal)promotionRule.DiscountAmountApplied,
                    PromotionNameSnapshot = promotionRule.Name,
                    PromotionDescriptionSnapshot = promotionRule.Description,
                    PromotionTypeSnapshot = ""
                };
                if (promotionRule.IsGiveFreeItemSku)
                {
                    var targetOrderItem = orderItems.First(x =>
                        x.Id == Guid.Parse(promotionRule.OrderItemFree.ProductVariantId));
                    targetOrderItem.Quantity = promotionRule.OrderItemFree.Quantity;
                }
                order.AppliedOrderPromotions.Add(appliedOrderPromotion);
            }
        }
        order.DiscountAmount = order.AppliedOrderPromotions.Sum(x => x.DiscountAmountApplied);
        order.TaxAmount = (order.SubTotalAmount - order.DiscountAmount) * ((decimal) storeDetailGrpcResponse.Rate / 100);
        order.TotalAmount = order.TaxAmount + (order.SubTotalAmount - order.DiscountAmount);
        var paymentGrpcResponse = await _paymentGrpcService.CreatePaymentTransactionAsync(
            new CreatePaymentTransactionRequest()
            {
                OrderId = order.Id.ToString(),
                BrandId = request.BrandId.ToString(),
                StoreId = storeId.ToString(),
                Amount = (float)order.TotalAmount,
                CredentialsConfig = storeDetailGrpcResponse.CredentialsConfigAtStore,
                SystemPaymentMethodId = storeDetailGrpcResponse.SystemPaymentMethodId,
                StorePaymentMethodConfigId = request.StorePaymentMethodConfigId.ToString(),
                AccountId = accountId.ToString(),
                CustomerId = request.CustomerId != null ? request.CustomerId.ToString() : String.Empty
            });
        order.SystemPaymentMethodNameSnapshot = paymentGrpcResponse.SystemPaymentMethodName;
        order.SystemPaymentMethodId = Guid.Parse(storeDetailGrpcResponse.SystemPaymentMethodId);
        await _unitOfWork.GetRepository<Orders>().InsertAsync(order);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;
        
        if (!isSuccess)
        {
            _logger.Error("Tạo đơn hàng thất bại");
            throw new Exception("Tạo đơn hàng thất bại");
        }
        _logger.Information("Tạo đơn hàng thành công với ID: {OrderId}", order.Id);
        var createOrderResponseModel = new CreateOrderResponseModel()
        {
            CorrelationId = Guid.CreateVersion7(),
            AccountId = accountId,
            OrderId = order.Id,
            StoreId = storeId,
            Ingredients = orderItemsFromGrpc.ProductForOrders
                .SelectMany(product => product.RecipeItems.Select(recipe => new
                {
                    IngredientId = recipe.Ingredient.Id,
                    Quantity = recipe.Quantity * product.Quantity
                }))
                .GroupBy(x => x.IngredientId)
                .Select(group => new IngredientForUpdateInventoryModel()
                {
                    IngredientId = Guid.Parse(group.Key),
                    Quantity = (decimal) group.Sum(item => item.Quantity)
                }).ToList()
        };
        await _topicProducer.Produce(
            null,
            createOrderResponseModel,
            cancellationToken: cancellationToken
        ).ConfigureAwait(false);
        
        return new ApiResponse()
        {
            Status = StatusCodes.Status201Created,
            Message = "Tạo đơn hàng thành công",
            Data = new CreateOrderResponse()
            {
                OrderId = order.Id,
                PaymentUrl = paymentGrpcResponse.QrLink != String.Empty ? paymentGrpcResponse.QrLink : null
            }
        };
    }
}