using Microsoft.Extensions.Options;
using Nubrio.Application.Interfaces;

namespace Nubrio.Infrastructure.Services;

public sealed class LanguageResolver : ILanguageResolver
{
    private readonly string _defaultLanguage;

    public LanguageResolver(IOptions<LanguageResolverOptions> options)
    {
        _defaultLanguage = options.Value.DefaultLanguage;
    }

    public string Resolve(string city)
    {
        if (ContainsCyrillic(city)) return "ru";
        
        return _defaultLanguage;
    }

    private static bool ContainsCyrillic(string cityString)
    {
        foreach (var ch in cityString)
        {
            // Базовая кириллица + доп. диапазоны + буква Ё/ё
            if ((ch >= '\u0400' && ch <= '\u04FF') || // Cyrillic
                (ch >= '\u0500' && ch <= '\u052F') || // Cyrillic Supplement
                (ch >= '\u2DE0' && ch <= '\u2DFF') || // Cyrillic Extended-A (диакритики)
                (ch >= '\uA640' && ch <= '\uA69F') || // Cyrillic Extended-B
                ch == '\u0401' || ch == '\u0451')     // Ё и ё на всякий случай
            {
                return true;
            }
        }
        return false;
    }
}

public sealed class LanguageResolverOptions
{
    public string DefaultLanguage { get; init; } = "en";
}