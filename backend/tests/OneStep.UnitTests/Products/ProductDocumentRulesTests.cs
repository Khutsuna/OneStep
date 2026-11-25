using OneStep.Application.Features.Products;

namespace OneStep.UnitTests.Products;

public class ProductDocumentRulesTests
{
    [Fact]
    public void A_real_pdf_header_is_accepted_and_an_html_page_named_pdf_is_not()
    {
        Assert.True(ProductDocumentRules.LooksLikePdf("%PDF-1.7\n"u8));
        Assert.False(ProductDocumentRules.LooksLikePdf("<html><script>"u8));
        Assert.False(ProductDocumentRules.LooksLikePdf("%PD"u8));            // too short
    }

    [Theory]
    [InlineData("manual.pdf", true)]
    [InlineData("Manual.PDF", true)]
    [InlineData("manual.pdf.exe", false)]
    [InlineData("manual", false)]
    public void Only_the_pdf_extension_is_allowed(string fileName, bool expected)
    {
        Assert.Equal(expected, ProductDocumentRules.HasPdfExtension(fileName));
    }

    [Fact]
    public void Folders_and_odd_characters_are_removed_from_the_file_name()
    {
        Assert.Equal("Sony manual (EN).pdf", ProductDocumentRules.SafeFileName(@"..\..\secret/Sony manual (EN).pdf"));
        Assert.Equal("script.pdf", ProductDocumentRules.SafeFileName("<script>.pdf"));
        Assert.Equal("ინსტრუქცია.pdf", ProductDocumentRules.SafeFileName("ინსტრუქცია.pdf"));
        Assert.Equal("document.pdf", ProductDocumentRules.SafeFileName("???.pdf"));
    }

    [Fact]
    public void The_title_is_what_staff_typed_or_else_the_file_name()
    {
        Assert.Equal("User manual", ProductDocumentRules.MakeTitle("  User manual ", "a.pdf"));
        Assert.Equal("WH-1000XM5 guide", ProductDocumentRules.MakeTitle(null, "WH-1000XM5 guide.pdf"));
        Assert.Equal("WH-1000XM5 guide", ProductDocumentRules.MakeTitle("   ", "WH-1000XM5 guide.pdf"));
    }

    [Fact]
    public void A_long_title_is_cut_to_150_characters()
    {
        var title = ProductDocumentRules.MakeTitle(new string('a', 300), "a.pdf");

        Assert.Equal(ProductDocumentRules.MaxTitleLength, title.Length);
    }
}
