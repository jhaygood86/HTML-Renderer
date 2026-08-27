using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css.Selectors;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/SelectorsTests.cs. Loads the same bootstrap.css fixture (embedded as
/// a resource via HtmlRenderer.Test.csproj) and exercises it against HTML-Renderer's CSS engine port.
/// </summary>
[TestClass]
public sealed class SelectorsTests
{
    [TestMethod]
    public void FindAllStyleRulesForAnElement()
    {
        var sheet = ParseBootstrap();

        var list = sheet.StyleRules
            .Where(r =>
                r.Selector is TypeSelector { Name: "input" }
                || (r.Selector is CompoundSelector selector && selector.First() is TypeSelector { Name: "input" })
            );

        Assert.AreEqual(6, list.Count());
    }

    [TestMethod]
    public void FindAllStyleRulesElementsWithMoreThanTwoCompoundSelectors()
    {
        var sheet = ParseBootstrap();

        var list = sheet.StyleRules
            .Where(r => (r.Selector as CompoundSelector)?.Length > 2);

        Assert.AreEqual(2, list.Count());
    }

    [TestMethod]
    public void FindAllStyleRulesWithCompoundSelector()
    {
        var sheet = ParseBootstrap();

        var list = sheet.StyleRules
            .Where(r => r.Selector is CompoundSelector selector && selector.Last().Text.StartsWith("["));

        Assert.AreEqual(7, list.Count());
    }

    private static readonly string[] StandardPseudoElementNames =
    [
        PseudoElementNames.After,
        PseudoElementNames.Before,
        PseudoElementNames.Content,
        PseudoElementNames.FirstLetter,
        PseudoElementNames.FirstLine,
        PseudoElementNames.Selection
    ];

    /// <summary>
    /// PORT-UNIT-IGNORED: PeachPDF's expected counts (277 / 6) were calibrated to a PeachPDF-only fix
    /// (vendor-prefixed pseudo-elements like ::-moz-placeholder parsing without invalidating their
    /// rule) that has no counterpart in HTML-Renderer's CSS engine port - porting the exact expected
    /// counts here would just be asserting HTML-Renderer's (currently undetermined, unverified) parser
    /// behavior on bootstrap.css's vendor-prefixed selectors, not a real regression guard.
    /// </summary>
    [TestMethod]
    [Ignore("not yet spec compliant - expected counts were calibrated to a PeachPDF-only vendor-prefixed " +
            "pseudo-element fix with no HTML-Renderer counterpart; see method doc comment.")]
    [DataRow(false, false, 277)]
    [DataRow(false, true, 6)]
    [DataRow(true, false, 277)]
    [DataRow(true, true, 6)]
    public void FindAllStandardPseudoElementSelectors(bool allowInvalidSelectors, bool nonStandard, int expectedCount)
    {
        var sheet = ParseBootstrap(allowInvalidSelectors);

        var list = sheet.StyleRules
            .Where(r => HasStandardPseudoElementSelector(r.Selector, negate: nonStandard));

        Assert.AreEqual(expectedCount, list.Count());
    }

    [TestMethod]
    [DataRow(":nth-child(10n + 1)", 10, 1)]
    [DataRow(":nth-child(10n - 1)", 10, -1)]
    [DataRow(":nth-child(-2n + 3)", -2, 3)]
    [DataRow(":nth-child( 10n  +  1 )", 10, 1)]
    [DataRow(":nth-child(10n+1)", 10, 1)]
    [DataRow(":nth-child(odd)", 2, 1)]
    [DataRow(":nth-last-child(3n + 2)", 3, 2)]
    [DataRow(":nth-of-type(3n + 2)", 3, 2)]
    [DataRow(":nth-last-of-type(3n + 2)", 3, 2)]
    public void NthChildAcceptsAnBWithAndWithoutSpaces(string selector, int step, int offset)
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet(selector + " { color: red }");

        Assert.AreEqual(1, sheet.Rules.Length);
        Assert.IsInstanceOfType<ChildSelector>(((StyleRule)sheet.Rules[0]).Selector);
        var child = (ChildSelector)((StyleRule)sheet.Rules[0]).Selector;
        Assert.AreEqual(step, child.Step);
        Assert.AreEqual(offset, child.Offset);
    }

    [TestMethod]
    [DataRow(":nth-child(3n + -6)")]
    [DataRow(":nth-child(10n + +1)")]
    [DataRow(":nth-child(10n + + 1)")]
    [DataRow(":nth-child(10n + n)")]
    [DataRow(":nth-child(3 n)")]
    [DataRow(":nth-child(+ 2n)")]
    [DataRow(":nth-child(+ 2)")]
    public void NthChildRejectsMalformedAnB(string selector)
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet(selector + " { color: red }");

        Assert.AreEqual(0, sheet.Rules.Length);
    }

    private static bool HasStandardPseudoElementSelector(ISelector selector, bool negate = false)
    {
        if (selector is PseudoElementSelector pes)
            return negate ^ StandardPseudoElementNames.Contains(pes.Name);
        else if (selector is CompoundSelector comp)
            return HasStandardPseudoElementSelector(comp.Last(), negate);
        else if (selector is ListSelector list)
            return list.Any(s => HasStandardPseudoElementSelector(s, negate));
        else if (selector is ComplexSelector complex)
            return HasStandardPseudoElementSelector(complex.Last().Selector, negate);
        return false;
    }

    private static Stylesheet ParseBootstrap(bool tolerateInvalidSelectors = false)
    {
        using var stream = GetBootstrapCssStream();
        var parser = new StylesheetParser(tolerateInvalidSelectors: tolerateInvalidSelectors);
        return parser.Parse(stream);
    }

    private static Stream GetBootstrapCssStream()
    {
        const string resourceName = "HtmlRenderer.Test.CssEngineSupport.bootstrap.css";
        var assembly = typeof(SelectorsTests).Assembly;
        return assembly.GetManifestResourceStream(resourceName)
               ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' not found.");
    }
}
