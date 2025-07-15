using System.Text.Json.Serialization;

public class BusinessProblemResponse
{
    [JsonPropertyName("problem")]
    public string Problem { get; init; } = default!;

    [JsonPropertyName("message")]
    public string Message { get; init; } = default!;

    [JsonPropertyName("details")]
    public string? Details { get; init; }
}
