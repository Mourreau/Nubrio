using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nubrio.Application.Common.Errors;

namespace Nubrio.Presentation.Filters;

public class ResultToActionResultFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is not ObjectResult objectResult 
            || objectResult.Value is not IResultBase fluentResult 
            || fluentResult.IsSuccess)
        {
            await next();
            return;
        }

        var error = fluentResult.Errors[0];

        if (!error.TryGetAppErrorCode(out var appCode))
        {
            context.Result = BuildProblemResult(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Unknown external error",
                detail: error.Message,
                providerCode: null,
                serviceCode: null);

            await next();  
            return;
        }

        var (statusCode, clientMessage) = MapProviderCodeToHttp(appCode);
        
        error.TryGetProviderCode(out var providerCode);
        
        context.Result = BuildProblemResult(
            statusCode,
            clientMessage,
            detail: error.Message,
            providerCode,
            appCode.ToString());
        
        await next();  
    }

    private static (int StatusCode, string ClientMessage) MapProviderCodeToHttp(AppErrorCode code)
        => code switch
        {
            AppErrorCode.LocationNotFound =>
                (StatusCodes.Status404NotFound, "Location data not found for this city"),
            AppErrorCode.ForecastNotFound =>
                (StatusCodes.Status404NotFound, "Weather data not found for this location"),
            AppErrorCode.TooManyRequests =>
                (StatusCodes.Status429TooManyRequests, "External provider rate limit exceeded"),
            AppErrorCode.Timeout =>
                (StatusCodes.Status504GatewayTimeout, "External provider did not respond in time"),
            AppErrorCode.ExternalClientError =>
                (StatusCodes.Status502BadGateway, "External provider rejected our request"),
            AppErrorCode.ExternalServerError =>
                (StatusCodes.Status502BadGateway, "External provider is unavailable"),
            AppErrorCode.ProviderBadResponse =>
                (StatusCodes.Status502BadGateway, "External provider returned malformed response"),
            AppErrorCode.DateOutOfRange =>
                (StatusCodes.Status400BadRequest, "Date is out of range"),
            AppErrorCode.EmptyCity =>
                (StatusCodes.Status400BadRequest, "City is required"),
            _ =>
                (StatusCodes.Status500InternalServerError, "Unknown external error")
        };

    private static ObjectResult BuildProblemResult(
        int statusCode,
        string title,
        string detail,
        string? providerCode,
        string? serviceCode)
    {
        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail
        };

        if (providerCode is not null)
            problem.Extensions["providerCode"] = providerCode;

        if (serviceCode is not null)
            problem.Extensions["serviceCode"] = serviceCode;

        return new ObjectResult(problem)
        {
            StatusCode = statusCode
        };
    }
}