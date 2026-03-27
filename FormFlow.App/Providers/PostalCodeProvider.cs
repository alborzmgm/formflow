namespace FormFlow.App.Providers;

using FormFlow.Core.Providers;
using FormFlow.Core.WorkflowEngine.Models;

public sealed class PostalCodeProvider : IDataSourceProvider
{
    public string Key => "PostalCodes";

    private static readonly IReadOnlyDictionary<string, IReadOnlyList<OptionItem>> _postalCodes =
        new Dictionary<string, IReadOnlyList<OptionItem>>(StringComparer.OrdinalIgnoreCase)
        {
            // United States
            ["NYC"] = [new("10001", "10001"), new("10002", "10002"), new("10003", "10003"), new("10004", "10004")],
            ["LA"]  = [new("90001", "90001"), new("90002", "90002"), new("90210", "90210"), new("90401", "90401")],
            ["CHI"] = [new("60601", "60601"), new("60602", "60602"), new("60603", "60603"), new("60604", "60604")],
            // United Kingdom
            ["LON"] = [new("EC1A 1BB", "EC1A 1BB"), new("EC2A 2AH", "EC2A 2AH"), new("WC1A 1AA", "WC1A 1AA"), new("SW1A 1AA", "SW1A 1AA")],
            ["MAN"] = [new("M1 1AA", "M1 1AA"), new("M2 1AA", "M2 1AA"), new("M3 1AA", "M3 1AA")],
            ["BIR"] = [new("B1 1AA", "B1 1AA"), new("B2 1AA", "B2 1AA"), new("B3 1AA", "B3 1AA")],
            // Germany
            ["BER"] = [new("10115", "10115"), new("10117", "10117"), new("10119", "10119"), new("10178", "10178")],
            ["MUN"] = [new("80331", "80331"), new("80333", "80333"), new("80335", "80335"), new("80336", "80336")],
            ["HAM"] = [new("20095", "20095"), new("20097", "20097"), new("20099", "20099")],
            // France
            ["PAR"] = [new("75001", "75001"), new("75002", "75002"), new("75003", "75003"), new("75008", "75008")],
            ["LYO"] = [new("69001", "69001"), new("69002", "69002"), new("69003", "69003")],
            ["MAR"] = [new("13001", "13001"), new("13002", "13002"), new("13003", "13003")],
            // Australia
            ["SYD"] = [new("2000", "2000"), new("2010", "2010"), new("2020", "2020"), new("2060", "2060")],
            ["MEL"] = [new("3000", "3000"), new("3004", "3004"), new("3008", "3008"), new("3053", "3053")],
            ["BRI"] = [new("4000", "4000"), new("4101", "4101"), new("4102", "4102")],
        };

    public Task<IEnumerable<OptionItem>> GetOptionsAsync(FormContext context)
    {
        var cityId = context.GetValue("CityId")?.ToString();

        if (string.IsNullOrWhiteSpace(cityId) ||
            !_postalCodes.TryGetValue(cityId, out var codes))
            return Task.FromResult(Enumerable.Empty<OptionItem>());

        return Task.FromResult<IEnumerable<OptionItem>>(codes);
    }
}
