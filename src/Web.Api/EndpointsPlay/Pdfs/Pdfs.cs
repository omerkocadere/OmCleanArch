using System.Globalization;
using CleanArch.Web.Api.Extensions;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace CleanArch.Web.Api.EndpointsPlay.Pdfs;

public class Pdf : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this, "[Play]").MapGet(CreateInvoiceReport);
    }

    public async Task<IResult> CreateInvoiceReport(InvoiceFactory invoiceFactory, ILogger<Pdf> logger)
    {
        logger.LogInformation("Starting invoice PDF generation.");
        RegisterFormats();
        var invoice = invoiceFactory.Create(10);

        var templatePath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "EndpointsPlay",
            "Pdfs",
            "Views",
            "InvoiceReport.hbs"
        );

        logger.LogInformation("Loading template from {TemplatePath}.", templatePath);
        var templateContent = await File.ReadAllTextAsync(templatePath);

        var template = Handlebars.Compile(templateContent);

        var data = new
        {
            invoice.Number,
            invoice.IssuedDate,
            invoice.DueDate,
            invoice.SellerAddress,
            invoice.CustomerAddress,
            invoice.LineItems,
            Subtotal = invoice.LineItems.Sum(li => li.Price * li.Quantity),
            Total = invoice.LineItems.Sum(li => li.Price * li.Quantity), // optionally apply tax
        };

        var html = template(data);

        logger.LogInformation("Downloading headless browser for PDF rendering.");
        var browserFetcher = new BrowserFetcher();
        await browserFetcher.DownloadAsync();
        logger.LogInformation("Headless browser downloaded successfully.");

        logger.LogInformation("Launching Puppeteer browser.");
        try
        {
            using var browser = await Puppeteer.LaunchAsync(
                new LaunchOptions
                {
                    Headless = true,
                    Args = ["--no-sandbox", "--disable-setuid-sandbox", "--disable-dev-shm-usage"],
                }
            );
            using var page = await browser.NewPageAsync();

            await page.SetContentAsync(html);
            await page.EvaluateExpressionHandleAsync("document.fonts.ready");

            logger.LogInformation("Generating PDF data.");
            var pdfData = await page.PdfDataAsync(
                new PdfOptions
                {
                    HeaderTemplate = """
                    <div style='font-size: 14px; text-align: center; padding: 10px;'>
                        <span style='margin-right: 20px;'><span class='title'></span></span>
                        <span><span class='date'></span></span>
                    </div>
                    """,
                    FooterTemplate = """
                    <div style='font-size: 14px; text-align: center; padding: 10px;'>
                        <span style='margin-right: 20px;'>Generated on <span class='date'></span></span>
                        <span>Page <span class='pageNumber'></span> of <span class='totalPages'></span></span>
                    </div>
                    """,
                    DisplayHeaderFooter = true,
                    Format = PaperFormat.A4,
                    PrintBackground = true,
                    MarginOptions = new MarginOptions
                    {
                        Top = "50px",
                        Right = "20px",
                        Bottom = "50px",
                        Left = "20px",
                    },
                }
            );

            logger.LogInformation("PDF generation completed for invoice {InvoiceNumber}.", invoice.Number);
            return Results.File(pdfData, "application/pdf", $"invoice-{invoice.Number}.pdf");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Detailed Puppeteer error: {ErrorMessage}. InnerException: {InnerException}",
                ex.Message,
                ex.InnerException?.Message
            );
            throw;
        }
    }

    private static void RegisterFormats()
    {
        Handlebars.RegisterHelper(
            "formatDate",
            (context, arguments) =>
            {
                if (arguments[0] is DateOnly date)
                {
                    return date.ToString("dd/MM/yyyy");
                }
                return arguments[0]?.ToString() ?? "";
            }
        );

        Handlebars.RegisterHelper(
            "formatCurrency",
            (context, arguments) =>
            {
                if (arguments[0] is decimal value)
                {
                    return value.ToString("C", CultureInfo.CreateSpecificCulture("en-US"));
                }
                return arguments[0]?.ToString() ?? "";
            }
        );

        Handlebars.RegisterHelper(
            "formatNumber",
            (context, arguments) =>
            {
                if (arguments[0] is decimal value)
                {
                    return value.ToString("N2");
                }
                return arguments[0]?.ToString() ?? "";
            }
        );

        Handlebars.RegisterHelper(
            "multiply",
            (context, arguments) =>
            {
                if (arguments.Length >= 2 && arguments[0] is decimal a && arguments[1] is decimal b)
                {
                    return a * b;
                }
                return 0m;
            }
        );
    }
}
