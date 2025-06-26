using System.Net;
using DimPos.Order.Domain.Models.Common;
using FluentValidation;
using FluentValidation.Results;

namespace DimPos.Order.Application.Common.Utils;

public class ValidationUtil<T> where T : class
{
    private readonly IValidator<T> _validator;

    public ValidationUtil(IValidator<T> validator)
    {
        _validator = validator;
    }

    public async Task<(bool IsValid, ApiResponse Response)> ValidateAsync(T model)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(model);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
            var response = new ApiResponse()
            {
                Status = (int)HttpStatusCode.BadRequest,
                Message = "Lỗi kiểm tra dữ liệu",
                Data = errors
            };
            return (false, response);
        }

        return (true, null);
    }
}