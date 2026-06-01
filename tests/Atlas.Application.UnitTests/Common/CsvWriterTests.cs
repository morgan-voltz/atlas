using System.Text;
using Atlas.Application.Common;
using FluentAssertions;

namespace Atlas.Application.UnitTests.Common;

public class CsvWriterTests
{
    private sealed record Row(string Name, int Count, DateOnly Date);

    private static readonly IReadOnlyList<CsvColumn<Row>> RowColumns =
    [
        new("Nom", x => x.Name),
        new("Quantité", x => x.Count),
        new("Date", x => x.Date),
    ];

    [Fact]
    public void Writes_BOM_then_header_then_rows_in_CRLF()
    {
        byte[] bytes = CsvWriter.WriteToBytes(
            new[] { new Row("Alice", 3, new DateOnly(2026, 5, 29)) },
            RowColumns);

        // BOM UTF-8.
        bytes[0].Should().Be(0xEF);
        bytes[1].Should().Be(0xBB);
        bytes[2].Should().Be(0xBF);

        string text = Encoding.UTF8.GetString(bytes[3..]);
        text.Should().Be("Nom,Quantité,Date\r\nAlice,3,2026-05-29\r\n");
    }

    [Fact]
    public void Escapes_fields_containing_comma_or_quote_or_newline()
    {
        var items = new[]
        {
            new Row("Doe, Jane", 1, new DateOnly(2024, 1, 1)),     // virgule
            new Row("\"Bob\"", 2, new DateOnly(2024, 2, 1)),       // guillemets
            new Row("line1\nline2", 3, new DateOnly(2024, 3, 1)),  // newline
        };

        byte[] bytes = CsvWriter.WriteToBytes(items, RowColumns);
        string text = Encoding.UTF8.GetString(bytes[3..]); // skip BOM

        text.Should().Contain("\"Doe, Jane\",1,2024-01-01");
        text.Should().Contain("\"\"\"Bob\"\"\",2,2024-02-01");
        text.Should().Contain("\"line1\nline2\",3,2024-03-01");
    }

    [Theory]
    [InlineData("=1+1")]
    [InlineData("+1+1")]
    [InlineData("-2+3")]
    [InlineData("@SUM(A1)")]
    public void Neutralizes_csv_formula_injection_in_values(string payload)
    {
        // Audit Lot 4 — F2 : les cellules débutant par un caractère de formule sont préfixées d'une
        // apostrophe pour ne pas être exécutées par Excel/LibreOffice (Name/Title viennent du client).
        byte[] bytes = CsvWriter.WriteToBytes(
            new[] { new Row(payload, 1, new DateOnly(2026, 1, 1)) },
            RowColumns);
        string text = Encoding.UTF8.GetString(bytes[3..]); // skip BOM

        text.Should().Contain("'" + payload);
    }

    [Fact]
    public void Empty_items_writes_only_header()
    {
        byte[] bytes = CsvWriter.WriteToBytes(Array.Empty<Row>(), RowColumns);
        string text = Encoding.UTF8.GetString(bytes[3..]); // skip BOM

        text.Should().Be("Nom,Quantité,Date\r\n");
    }

    [Fact]
    public void Null_values_become_empty_fields()
    {
        var columns = new List<CsvColumn<Row>>
        {
            new("Optional", _ => null),
        };

        byte[] bytes = CsvWriter.WriteToBytes(new[] { new Row("x", 0, default) }, columns);
        string text = Encoding.UTF8.GetString(bytes[3..]);

        text.Should().Be("Optional\r\n\r\n");
    }

    [Fact]
    public void DateTimeOffset_serialized_as_ISO_round_trip()
    {
        var columns = new List<CsvColumn<DateTimeOffset>>
        {
            new("When", x => x),
        };
        var when = new DateTimeOffset(2026, 5, 29, 12, 0, 0, TimeSpan.FromHours(2));

        byte[] bytes = CsvWriter.WriteToBytes(new[] { when }, columns);
        string text = Encoding.UTF8.GetString(bytes[3..]);

        // Format "o" → 2026-05-29T12:00:00.0000000+02:00
        text.Should().Contain("2026-05-29T12:00:00");
        text.Should().Contain("+02:00");
    }
}
