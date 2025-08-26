using Microsoft.Extensions.Options;
using Positions.ConsoleApp.Contracts;
using Positions.ConsoleApp.ExternalServices;
using System.Runtime.CompilerServices;
using System.Text.Json;

#nullable disable
namespace Positions.ConsoleApp.Imports
{
    public sealed class ApiPositionsSource : IPositionsSource
    {
        private readonly HttpClient _http;
        private readonly string _url;

        private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

        public ApiPositionsSource(HttpClient http, IOptions<ExternalApiOptions> opts)
        {
            _http = http;
            _url = opts.Value.Url;

            if (!string.IsNullOrWhiteSpace(opts.Value.Key))
                _http.DefaultRequestHeaders.Add("X-Test-Key", opts.Value.Key);

            _http.Timeout = TimeSpan.FromMinutes(5);
        }

        public async IAsyncEnumerable<PositionDto> StreamAsync([EnumeratorCancellation] CancellationToken stoppingToken)
        {
            using var response = await _http.GetAsync(_url, HttpCompletionOption.ResponseHeadersRead, stoppingToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(stoppingToken);
            await foreach (var item in JsonSerializer.DeserializeAsyncEnumerable<PositionDto>(stream, JsonOpts, stoppingToken))
            {
                if (item is not null) yield return item;
            }
        }
    }
}
