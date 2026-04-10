using TagAndTrack.Backend.Utils;

namespace TagAndTrack.Tests;

public class CsvHelperTests
{
    // ===== Escape Tests =====

    [Fact]
    public void Escape_PlainValue_ReturnsUnchanged()
    {
        Assert.Equal("hello", CsvHelper.Escape("hello"));
    }

    [Fact]
    public void Escape_NullValue_ReturnsEmpty()
    {
        Assert.Equal("", CsvHelper.Escape(null));
    }

    [Fact]
    public void Escape_EmptyValue_ReturnsEmpty()
    {
        Assert.Equal("", CsvHelper.Escape(""));
    }

    [Fact]
    public void Escape_ValueWithComma_Quotes()
    {
        Assert.Equal("\"hello,world\"", CsvHelper.Escape("hello,world"));
    }

    [Fact]
    public void Escape_ValueWithQuote_EscapesAndQuotes()
    {
        Assert.Equal("\"say \"\"hi\"\"\"", CsvHelper.Escape("say \"hi\""));
    }

    [Fact]
    public void Escape_ValueWithNewline_Quotes()
    {
        Assert.Equal("\"line1\nline2\"", CsvHelper.Escape("line1\nline2"));
    }

    [Fact]
    public void Escape_ValueWithCarriageReturn_Quotes()
    {
        Assert.Equal("\"line1\rline2\"", CsvHelper.Escape("line1\rline2"));
    }

    // ===== ParseLine Tests =====

    [Fact]
    public void ParseLine_SimpleValues_SplitsCorrectly()
    {
        var fields = CsvHelper.ParseLine("a,b,c");
        Assert.Equal(new[] { "a", "b", "c" }, fields);
    }

    [Fact]
    public void ParseLine_QuotedFieldWithComma_ParsesCorrectly()
    {
        var fields = CsvHelper.ParseLine("a,\"b,c\",d");
        Assert.Equal(new[] { "a", "b,c", "d" }, fields);
    }

    [Fact]
    public void ParseLine_EscapedQuotesInField_ParsesCorrectly()
    {
        var fields = CsvHelper.ParseLine("a,\"say \"\"hi\"\"\",c");
        Assert.Equal(new[] { "a", "say \"hi\"", "c" }, fields);
    }

    [Fact]
    public void ParseLine_EmptyFields_PreservedAsEmpty()
    {
        var fields = CsvHelper.ParseLine("a,,c");
        Assert.Equal(new[] { "a", "", "c" }, fields);
    }

    [Fact]
    public void ParseLine_SingleField_ReturnsSingleElement()
    {
        var fields = CsvHelper.ParseLine("hello");
        Assert.Equal(new[] { "hello" }, fields);
    }

    [Fact]
    public void ParseLine_EmptyLine_ReturnsSingleEmptyElement()
    {
        var fields = CsvHelper.ParseLine("");
        Assert.Equal(new[] { "" }, fields);
    }

    // ===== Round-trip Tests =====

    [Fact]
    public void RoundTrip_EscapeThenParse_PreservesValue()
    {
        var original = "value with, commas and \"quotes\"";
        var escaped = CsvHelper.Escape(original);
        var line = $"before,{escaped},after";
        var fields = CsvHelper.ParseLine(line);

        Assert.Equal("before", fields[0]);
        Assert.Equal(original, fields[1]);
        Assert.Equal("after", fields[2]);
    }

    [Fact]
    public void RoundTrip_Base64Signature_PreservesData()
    {
        var signatureBytes = new byte[] { 0x00, 0xFF, 0x42, 0x13, 0x37 };
        var base64 = Convert.ToBase64String(signatureBytes);
        var escaped = CsvHelper.Escape(base64);
        var line = $"1,{escaped}";
        var fields = CsvHelper.ParseLine(line);

        var decoded = Convert.FromBase64String(fields[1]);
        Assert.Equal(signatureBytes, decoded);
    }
}
