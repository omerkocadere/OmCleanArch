namespace CleanArch.Web.Api.Playground.Pdf.Contracts;

public sealed class LineItem
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public decimal Quantity { get; set; }
}
