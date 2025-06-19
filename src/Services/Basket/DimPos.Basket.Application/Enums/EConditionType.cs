namespace DimPos.Basket.Application.Enums;

public enum EConditionType
{
    MinCartValue = 0,
    CartContainsSku = 1,
    CartContainsCategory = 2,
    CustomerIsInSegment = 3,
    CustomerIsInLoyaltyTier = 4,
    CurrentTimeIsInRange = 5,
    CurrentDayIsInList = 6,
    NthOrderOfCustomer = 7
}