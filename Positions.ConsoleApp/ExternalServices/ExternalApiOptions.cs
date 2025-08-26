namespace Positions.ConsoleApp.ExternalServices
{
    public sealed class ExternalApiOptions
    {
        public string Url { get; set; } = "https://api.andbank.com.br/candidate/positions";
        public string? Key { get; set; }
        public bool UseMock { get; set; } = true;
        public int BatchSize { get; set; } = 5000;
        public bool TruncateBeforeImport { get; set; } = false;
        public bool UseUpsert { get; set; } = false;
        public int? MaxItems { get; set; } = null;
    }
}
