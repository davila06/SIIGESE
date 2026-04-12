using Application.DTOs;
using System.Collections.Concurrent;

namespace WebApi.Services;

public interface IEndpointPerformanceStore
{
    void Track(string endpoint, double durationMs);
    SistemaRendimientoDto BuildReport(int top = 12);
}

public sealed class EndpointPerformanceStore : IEndpointPerformanceStore
{
    private const int MaxSamplesPerEndpoint = 2000;
    private readonly ConcurrentDictionary<string, ConcurrentQueue<double>> _durations = new(StringComparer.OrdinalIgnoreCase);

    public void Track(string endpoint, double durationMs)
    {
        if (string.IsNullOrWhiteSpace(endpoint) || durationMs < 0)
        {
            return;
        }

        var queue = _durations.GetOrAdd(endpoint, _ => new ConcurrentQueue<double>());
        queue.Enqueue(durationMs);

        while (queue.Count > MaxSamplesPerEndpoint && queue.TryDequeue(out _))
        {
        }
    }

    public SistemaRendimientoDto BuildReport(int top = 12)
    {
        var endpoints = _durations
            .Select(kvp =>
            {
                var snapshot = kvp.Value.ToArray();
                if (snapshot.Length == 0)
                {
                    return null;
                }

                Array.Sort(snapshot);

                return new EndpointPercentilDto
                {
                    Endpoint = kvp.Key,
                    P50Ms = Percentile(snapshot, 0.50),
                    P90Ms = Percentile(snapshot, 0.90),
                    P99Ms = Percentile(snapshot, 0.99),
                    Samples = snapshot.Length
                };
            })
            .Where(x => x is not null)
            .Select(x => x!)
            .OrderByDescending(x => x.P90Ms)
            .ThenByDescending(x => x.Samples)
            .Take(top)
            .ToList();

        return new SistemaRendimientoDto
        {
            Disponible = endpoints.Count > 0,
            Endpoints = endpoints
        };
    }

    private static double Percentile(double[] sortedValues, double percentile)
    {
        if (sortedValues.Length == 0)
        {
            return 0;
        }

        if (sortedValues.Length == 1)
        {
            return Math.Round(sortedValues[0], 1);
        }

        var rank = percentile * (sortedValues.Length - 1);
        var lower = (int)Math.Floor(rank);
        var upper = (int)Math.Ceiling(rank);

        if (lower == upper)
        {
            return Math.Round(sortedValues[lower], 1);
        }

        var weight = rank - lower;
        var value = sortedValues[lower] + (sortedValues[upper] - sortedValues[lower]) * weight;
        return Math.Round(value, 1);
    }
}
