namespace LogFlowPipeline;

using System.Text.RegularExpressions;

public enum ErrorPolicyKind { FailFast, Skip, DeadLetter, Retry }

public record ErrorPolicy(ErrorPolicyKind Kind, int RetryCount = 0)
{
    // YAML'da onError yazılmazsa en güvenli davranış: dur.
    public static ErrorPolicy Default => new(ErrorPolicyKind.FailFast);

    public static ErrorPolicy Parse(string? text, string where)
    {
        if (string.IsNullOrWhiteSpace(text)) return Default;

        var t = text.Trim().ToUpperInvariant();
        if (t == "FAIL_FAST") return new(ErrorPolicyKind.FailFast);
        if (t == "SKIP") return new(ErrorPolicyKind.Skip);
        if (t == "DEAD_LETTER") return new(ErrorPolicyKind.DeadLetter);

        var m = Regex.Match(t, @"^RETRY\((\d+)\)$");
        if (m.Success && int.TryParse(m.Groups[1].Value, out var n) && n is >= 1 and <= 10)
            return new(ErrorPolicyKind.Retry, n);

        throw new ConfigException(
            $"{where}: geçersiz onError '{text}'. Geçerli değerler: FAIL_FAST, SKIP, DEAD_LETTER, RETRY(n) (n: 1-10)");
    }

    public override string ToString() => Kind switch
    {
        ErrorPolicyKind.FailFast => "FAIL_FAST",
        ErrorPolicyKind.Skip => "SKIP",
        ErrorPolicyKind.DeadLetter => "DEAD_LETTER",
        _ => $"RETRY({RetryCount})",
    };
}