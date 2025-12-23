using FluentResults;
using Nubrio.Application.Interfaces;
using Nubrio.Application.Interfaces.Repository;

namespace Nubrio.Application.Services;

public class StatsService : IStatsService
{
    private readonly IStatsRepository _statsRepository;

    private const int MaxSkip = 10000;

    public StatsService(IStatsRepository statsRepository)
    {
        _statsRepository = statsRepository;
    }

    public async Task<Result<TopCitiesResult>> GetTopCitiesAsync(DateOnly fromDate, DateOnly toDate, int limit,
        CancellationToken ct)
    {
        var validationResult = ValidateTopCitiesInput(fromDate, toDate, limit);
        if (validationResult.IsFailed) return Result.Fail(validationResult.Errors);


        var fromUtc =
            new DateTimeOffset(fromDate.ToDateTime(TimeOnly.MinValue),
                TimeSpan.Zero); // from - начало дня (00:00:00) UTC
        var toExclusiveUtc =
            new DateTimeOffset(toDate.AddDays(1).ToDateTime(TimeOnly.MinValue),
                TimeSpan.Zero); // to - конец периода как следующий день 00:00


        var repoResult = await _statsRepository.GetTopCitiesAsync(fromUtc, toExclusiveUtc, limit, ct);
        if (repoResult.IsFailed) return Result.Fail(repoResult.Errors);

        var topCities = repoResult.Value;

        var topCitiesResult = new TopCitiesResult(
            From: fromDate,
            To: toDate,
            Limit: limit,
            TopCities: topCities);

        return Result.Ok(topCitiesResult);
    }

    public async Task<Result<RequestsPageResult>> GetRequestsAsync(DateOnly fromDate, DateOnly toDate, int page,
        int pageSize, CancellationToken ct)
    {
        var validationResult = ValidateRequestsInput(fromDate, toDate, page, pageSize);
        if (validationResult.IsFailed) return Result.Fail(validationResult.Errors);

        var fromUtc =
            new DateTimeOffset(fromDate.ToDateTime(TimeOnly.MinValue),
                TimeSpan.Zero); // from - начало дня (00:00:00) UTC
        var toExclusiveUtc =
            new DateTimeOffset(toDate.AddDays(1).ToDateTime(TimeOnly.MinValue),
                TimeSpan.Zero); // to - конец периода как следующий день 00:00

        var skip = (page - 1) * pageSize;

        var totalResult = await _statsRepository.CountRequestsAsync(fromUtc, toExclusiveUtc, ct);
        if (totalResult.IsFailed) return Result.Fail<RequestsPageResult>(totalResult.Errors);

        var total = totalResult.Value;

        var repoResult = await _statsRepository.GetRequestsPageAsync(fromUtc, toExclusiveUtc, skip, pageSize, ct);
        if (repoResult.IsFailed) return Result.Fail(repoResult.Errors);

        var requestEntries = repoResult.Value;

        var requestsPageResult = new RequestsPageResult(
            From: fromDate,
            To: toDate,
            Page: page,
            PageSize: pageSize,
            Total: total,
            Entries: requestEntries);

        return Result.Ok(requestsPageResult);
    }

    private Result ValidateTopCitiesInput(DateOnly fromDate, DateOnly toDate, int limit)
    {
        var errors = new List<IError>();

        var dateValidationResult = ValidateDate(fromDate, toDate);

        if (dateValidationResult.IsFailed) errors.AddRange(dateValidationResult.Errors);

        if (limit < 1 || limit > 50)
            errors.Add(
                new Error("Limit must be between 1 and 50")
                    .WithMetadata("Code", "Stats.Validation")
                    .WithMetadata("Field", "limit")
                    .WithMetadata("Reason", "OutOfRange")
                    .WithMetadata("Min", 1)
                    .WithMetadata("Max", 50)
                    .WithMetadata("Actual", limit)
            );

        return errors.Count != 0
            ? Result.Fail(errors)
            : Result.Ok();
    }

    private Result ValidateRequestsInput(DateOnly fromDate, DateOnly toDate, int page, int pageSize)
    {
        var errors = new List<IError>();

        var dateValidationResult = ValidateDate(fromDate, toDate);

        if (dateValidationResult.IsFailed) errors.AddRange(dateValidationResult.Errors);


        if (pageSize < 1 || pageSize > 50)
        {
            errors.Add(
                new Error("PageSize must be between 1 and 50")
                    .WithMetadata("Code", "Stats.Validation")
                    .WithMetadata("Field", "pageSize")
                    .WithMetadata("Reason", "OutOfRange")
                    .WithMetadata("Min", 1)
                    .WithMetadata("Max", 50)
                    .WithMetadata("Actual", pageSize));
            
            return Result.Fail(errors);
        }


        var maxPage = (MaxSkip / pageSize) + 1;
        
        if (page < 1 || page > maxPage)
            errors.Add(
                new Error("Page is out of allowed range")
                    .WithMetadata("Code", "Stats.Validation")
                    .WithMetadata("Field", "page")
                    .WithMetadata("Reason", "OutOfRange")
                    .WithMetadata("Min", 1)
                    .WithMetadata("Max", maxPage)
                    .WithMetadata("Actual", page));


        return errors.Count != 0
            ? Result.Fail(errors)
            : Result.Ok();
    }

    private Result ValidateDate(DateOnly fromDate, DateOnly toDate)
    {
        if (toDate < fromDate)
            return Result.Fail(new Error("TO date must be after or equal to FROM date")
                .WithMetadata("Code", "Stats.Validation")
                .WithMetadata("Field", "to")
                .WithMetadata("Reason", "RangeInvalid")
                .WithMetadata("From", fromDate.ToString("yyyy-MM-dd"))
                .WithMetadata("To", toDate.ToString("yyyy-MM-dd")));

        return Result.Ok();
    }
}