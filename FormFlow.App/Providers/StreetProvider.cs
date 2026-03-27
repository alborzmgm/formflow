namespace FormFlow.App.Providers;

using FormFlow.Core.Providers;
using FormFlow.Core.WorkflowEngine.Models;

public sealed class StreetProvider : IDataSourceProvider
{
    public string Key => "Streets";

    private static readonly IReadOnlyDictionary<string, IReadOnlyList<OptionItem>> _streets =
        new Dictionary<string, IReadOnlyList<OptionItem>>(StringComparer.OrdinalIgnoreCase)
        {
            // United States
            ["NYC"] = [new("broadway", "Broadway"), new("5th-ave", "5th Avenue"), new("wall-st", "Wall Street"), new("park-ave", "Park Avenue"), new("lexington-ave", "Lexington Avenue")],
            ["LA"]  = [new("sunset-blvd", "Sunset Boulevard"), new("hollywood-blvd", "Hollywood Boulevard"), new("wilshire-blvd", "Wilshire Boulevard"), new("rodeo-dr", "Rodeo Drive")],
            ["CHI"] = [new("michigan-ave", "Michigan Avenue"), new("lake-shore-dr", "Lake Shore Drive"), new("wacker-dr", "Wacker Drive"), new("state-st", "State Street")],
            // United Kingdom
            ["LON"] = [new("oxford-st", "Oxford Street"), new("regent-st", "Regent Street"), new("baker-st", "Baker Street"), new("kings-road", "King's Road"), new("strand", "The Strand")],
            ["MAN"] = [new("deansgate", "Deansgate"), new("oxford-road", "Oxford Road"), new("market-st", "Market Street"), new("peter-st", "Peter Street")],
            ["BIR"] = [new("new-st", "New Street"), new("broad-st", "Broad Street"), new("corporation-st", "Corporation Street")],
            // Germany
            ["BER"] = [new("unter-den-linden", "Unter den Linden"), new("kurfuerstendamm", "Kurfürstendamm"), new("friedrichstrasse", "Friedrichstraße"), new("potsdamer-platz", "Potsdamer Platz")],
            ["MUN"] = [new("maximilianstrasse", "Maximilianstraße"), new("ludwigstrasse", "Ludwigstraße"), new("kaufingerstrasse", "Kaufingerstraße")],
            ["HAM"] = [new("moenckebergstrasse", "Mönckebergstraße"), new("jungfernstieg", "Jungfernstieg"), new("reeperbahn", "Reeperbahn")],
            // France
            ["PAR"] = [new("champs-elysees", "Champs-Élysées"), new("rue-de-rivoli", "Rue de Rivoli"), new("boulevard-haussmann", "Boulevard Haussmann"), new("rue-saint-honore", "Rue Saint-Honoré")],
            ["LYO"] = [new("rue-de-la-republique", "Rue de la République"), new("rue-victor-hugo", "Rue Victor Hugo"), new("cours-de-verdun", "Cours de Verdun")],
            ["MAR"] = [new("la-canebiere", "La Canebière"), new("rue-saint-ferreol", "Rue Saint-Ferréol"), new("cours-julien", "Cours Julien")],
            // Australia
            ["SYD"] = [new("george-st", "George Street"), new("pitt-st", "Pitt Street"), new("market-st", "Market Street"), new("macquarie-st", "Macquarie Street")],
            ["MEL"] = [new("flinders-st", "Flinders Street"), new("collins-st", "Collins Street"), new("bourke-st", "Bourke Street"), new("swanston-st", "Swanston Street")],
            ["BRI"] = [new("queen-st", "Queen Street"), new("ann-st", "Ann Street"), new("george-st", "George Street")],
        };

    public Task<IEnumerable<OptionItem>> GetOptionsAsync(FormContext context)
    {
        var cityId = context.GetValue("CityId")?.ToString();

        if (string.IsNullOrWhiteSpace(cityId) ||
            !_streets.TryGetValue(cityId, out var streets))
            return Task.FromResult(Enumerable.Empty<OptionItem>());

        return Task.FromResult<IEnumerable<OptionItem>>(streets);
    }
}
