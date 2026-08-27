using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

using static HtmlRenderer.Test.CssEngineSupport.CssConstructionFunctions;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/Property.cs (the break-*/page-break-* cases).
/// Exercises the modern css-break-3 <c>break-before</c>/<c>break-after</c>/<c>break-inside</c> properties
/// (<see cref="BreakAfterProperty"/>/<see cref="BreakBeforeProperty"/>/<see cref="BreakInsideProperty"/>)
/// against the legacy <c>page-break-before</c>/<c>page-break-after</c>/<c>page-break-inside</c> aliases
/// (<see cref="PageBreakAfterProperty"/>/<see cref="PageBreakBeforeProperty"/>/<see cref="PageBreakInsideProperty"/>),
/// confirming they really do use two distinct converters/keyword sets as the port task described:
/// Source/HtmlRenderer/Core/CssEngine/Model/Converters.cs lines 279-281 define
/// <c>Converters.BreakModeConverter</c> (break-before/break-after), <c>Converters.BreakInsideModeConverter</c>
/// (break-inside) and <c>Converters.PageBreakModeConverter</c> (page-break-before/page-break-after) as three
/// separate maps, and page-break-inside (Break/PageBreakInsideProperty.cs) uses a fourth, hand-rolled
/// auto/avoid-only converter rather than any of the three maps.
///
/// One real divergence from PeachPDF's own grammar was found while verifying this port (Model/Map.cs
/// lines 265-277) and is documented in place below rather than assumed away:
/// <c>Map.BreakModes</c> (break-before/break-after) implements only 9 of the 12 css-break-3 §3.1 values -
/// it is missing "recto"/"verso"/"avoid-region"/"region" entirely (no such keywords exist anywhere in
/// Enumerations/Keywords.cs), and unlike PeachPDF it treats "always" as a supported value on the modern
/// properties (PeachPDF scopes "always" to the legacy page-break-* aliases only, per css-break-4).
/// <c>Map.BreakInsideModes</c> (break-inside) and <c>Map.PageBreakModes</c> (legacy page-break-before/after),
/// by contrast, match their respective spec value sets exactly.
/// </summary>
[TestClass]
public sealed class PropertyBreakTests
{
    [TestMethod]
    public void CssBreakAfterLegalAvoid()
    {
        var snippet = "break-after:avoid";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("break-after", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BreakAfterProperty>(property);
        var concrete = (BreakAfterProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.AreEqual("avoid", concrete.Value);
        Assert.AreEqual("avoid", concrete.Original);
    }

    [TestMethod]
    public void CssPageBreakAfterLegalAvoid()
    {
        var snippet = "page-break-after:avoid";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("page-break-after", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<PageBreakAfterProperty>(property);
        var concrete = (PageBreakAfterProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.AreEqual("avoid", concrete.Value);
        Assert.AreEqual("avoid", concrete.Original);
    }

    [TestMethod]
    public void CssBreakAfterLegalPageCapital()
    {
        var snippet = "break-after:Page";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("break-after", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BreakAfterProperty>(property);
        var concrete = (BreakAfterProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.AreEqual("page", concrete.Value);
        Assert.AreEqual("Page", concrete.Original);
    }

    [TestMethod]
    public void CssPageBreakAfterIllegalAvoidColumn()
    {
        var snippet = "page-break-after:avoid-column";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("page-break-after", property.Name);
        Assert.IsFalse(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<PageBreakAfterProperty>(property);
        var concrete = (PageBreakAfterProperty)property;
        Assert.IsFalse(concrete.IsInherited);
    }

    [TestMethod]
    public void CssBreakAfterLegalAvoidColumn()
    {
        var snippet = "break-after:avoid-column";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("break-after", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BreakAfterProperty>(property);
        var concrete = (BreakAfterProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.AreEqual("avoid-column", concrete.Value);
        Assert.AreEqual("avoid-column", concrete.Original);
    }

    [TestMethod]
    public void CssBreakBeforeLegalAuto()
    {
        var snippet = "break-before:AUTO";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("break-before", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BreakBeforeProperty>(property);
        var concrete = (BreakBeforeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.AreEqual("auto", concrete.Value);
        Assert.AreEqual("AUTO", concrete.Original);
    }

    [TestMethod]
    public void CssPageBreakBeforeLegalAuto()
    {
        var snippet = "page-break-before:AUTO";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("page-break-before", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<PageBreakBeforeProperty>(property);
        var concrete = (PageBreakBeforeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.AreEqual("auto", concrete.Value);
        Assert.AreEqual("AUTO", concrete.Original);
    }

    [TestMethod]
    public void CssPageBreakBeforeLegalLeft()
    {
        var snippet = "page-break-before:left";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("page-break-before", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<PageBreakBeforeProperty>(property);
        var concrete = (PageBreakBeforeProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.AreEqual("left", concrete.Value);
        Assert.AreEqual("left", concrete.Original);
    }

    [TestMethod]
    public void CssBreakBeforeIllegalValue()
    {
        var snippet = "break-before:whatever";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("break-before", property.Name);
        Assert.IsFalse(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BreakBeforeProperty>(property);
        var concrete = (BreakBeforeProperty)property;
        Assert.IsNotNull(concrete);
    }

    [TestMethod]
    public void CssBreakInsideIllegalPage()
    {
        var snippet = "break-inside:page";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("break-inside", property.Name);
        Assert.IsFalse(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BreakInsideProperty>(property);
        var concrete = (BreakInsideProperty)property;
        Assert.IsNotNull(concrete);
    }

    [TestMethod]
    public void CssBreakInsideLegalAvoidRegionUppercase()
    {
        var snippet = "break-inside:avoid-REGION";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("break-inside", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<BreakInsideProperty>(property);
        var concrete = (BreakInsideProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.AreEqual("avoid-region", concrete.Value);
        Assert.AreEqual("avoid-REGION", concrete.Original);
    }

    [TestMethod]
    public void CssPageBreakInsideLegalAvoid()
    {
        var snippet = "page-break-inside:avoid";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("page-break-inside", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<PageBreakInsideProperty>(property);
        var concrete = (PageBreakInsideProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.AreEqual("avoid", concrete.Value);
        Assert.AreEqual("avoid", concrete.Original);
    }

    [TestMethod]
    public void CssPageBreakInsideLegalAutoUppercase()
    {
        var snippet = "page-break-inside:AUTO";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("page-break-inside", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<PageBreakInsideProperty>(property);
        var concrete = (PageBreakInsideProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.AreEqual("auto", concrete.Value);
        Assert.AreEqual("AUTO", concrete.Original);
    }

    // The css-break-3 §3.1 break-before/break-after value set, restricted to the subset this engine's
    // Map.BreakModes (Converters.cs line 279) actually implements. "recto"/"verso"/"avoid-region"/"region"
    // are asserted as rejected separately below, rather than folded in here as PeachPDF does, because they
    // are genuinely unsupported (no matching keywords exist in Enumerations/Keywords.cs).
    [TestMethod]
    [DataRow("break-before")]
    [DataRow("break-after")]
    public void BreakBeforeAfter_AcceptsSupportedSpecValues(string name)
    {
        string[] supportedValues =
        [
            "auto", "avoid", "avoid-page", "page", "left", "right", "avoid-column", "column"
        ];

        foreach (var value in supportedValues)
        {
            var property = ParseDeclaration($"{name}:{value}");
            Assert.IsTrue(property.HasValue, $"{name}:{value} should be valid");
            Assert.AreEqual(value, property.Value);
        }
    }

    // Documents the gap noted in the class header: these four css-break-3 values have no keyword defined
    // anywhere in this engine (Enumerations/Keywords.cs) and are absent from Map.BreakModes
    // (Model/Map.cs lines 265-277), so break-before/break-after reject them outright.
    [TestMethod]
    [DataRow("break-before", "recto")]
    [DataRow("break-before", "verso")]
    [DataRow("break-before", "avoid-region")]
    [DataRow("break-before", "region")]
    [DataRow("break-after", "recto")]
    [DataRow("break-after", "verso")]
    [DataRow("break-after", "avoid-region")]
    [DataRow("break-after", "region")]
    public void BreakBeforeAfter_RejectsUnimplementedSpecValues(string name, string value)
    {
        var property = ParseDeclaration($"{name}:{value}");
        Assert.IsFalse(property.HasValue, $"{name}:{value} is not implemented by Map.BreakModes and should be rejected");
    }

    [TestMethod]
    [DataRow("break-before")]
    [DataRow("break-after")]
    public void BreakBeforeAfter_IsCaseInsensitiveAndKeepsAuthoredText(string name)
    {
        // "avoid-page" replaces PeachPDF's "verso" probe value here, since "verso" is one of the
        // unimplemented values documented above and would not exercise case-insensitivity at all.
        var property = ParseDeclaration($"{name}:AvOiD-PaGe");
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("avoid-page", property.Value);
        Assert.AreEqual("AvOiD-PaGe", property.Original);
    }

    // "all" and "avoid-nothing" are not part of any value set this engine defines for break-before/
    // break-after and are correctly rejected. "always" is asserted separately below since - unlike
    // PeachPDF, which scopes "always" to the legacy page-break-* aliases only - this engine's
    // Map.BreakModes (Model/Map.cs line 269) includes it as a supported break-before/break-after value.
    [TestMethod]
    [DataRow("break-before", "all")]
    [DataRow("break-after", "all")]
    [DataRow("break-before", "avoid-nothing")]
    public void BreakBeforeAfter_RejectsNonSpecValue(string name, string value)
    {
        var property = ParseDeclaration($"{name}:{value}");
        Assert.IsFalse(property.HasValue);
    }

    // Divergence from PeachPDF (see class header): this engine's Map.BreakModes treats "always" as a
    // legal break-before/break-after value, not just a legacy page-break-* one.
    [TestMethod]
    [DataRow("break-before")]
    [DataRow("break-after")]
    public void BreakBeforeAfter_AcceptsAlwaysUnlikePeachPdf(string name)
    {
        var property = ParseDeclaration($"{name}:always");
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual("always", property.Value);
    }

    [TestMethod]
    [DataRow("auto")]
    [DataRow("avoid")]
    [DataRow("avoid-page")]
    [DataRow("avoid-column")]
    [DataRow("avoid-region")]
    public void BreakInside_AcceptsSpecValue(string value)
    {
        var property = ParseDeclaration($"break-inside:{value}");
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual(value, property.Value);
    }

    [TestMethod]
    [DataRow("page")]
    [DataRow("column")]
    [DataRow("region")]
    [DataRow("left")]
    [DataRow("recto")]
    [DataRow("always")]
    public void BreakInside_RejectsNonSpecValue(string value)
    {
        var property = ParseDeclaration($"break-inside:{value}");
        Assert.IsFalse(property.HasValue);
    }

    [TestMethod]
    [DataRow("page-break-before", "auto")]
    [DataRow("page-break-before", "always")]
    [DataRow("page-break-before", "avoid")]
    [DataRow("page-break-before", "left")]
    [DataRow("page-break-before", "right")]
    [DataRow("page-break-after", "auto")]
    [DataRow("page-break-after", "always")]
    [DataRow("page-break-after", "avoid")]
    [DataRow("page-break-after", "left")]
    [DataRow("page-break-after", "right")]
    public void PageBreakBeforeAfter_AcceptsLegacyValue(string name, string value)
    {
        var property = ParseDeclaration($"{name}:{value}");
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual(value, property.Value);
    }

    [TestMethod]
    [DataRow("page-break-before", "page")]
    [DataRow("page-break-before", "recto")]
    [DataRow("page-break-before", "verso")]
    [DataRow("page-break-before", "column")]
    [DataRow("page-break-before", "region")]
    [DataRow("page-break-after", "page")]
    [DataRow("page-break-after", "avoid-page")]
    [DataRow("page-break-after", "verso")]
    public void PageBreakBeforeAfter_RejectsModernOnlyValue(string name, string value)
    {
        var property = ParseDeclaration($"{name}:{value}");
        Assert.IsFalse(property.HasValue);
    }

    [TestMethod]
    [DataRow("auto")]
    [DataRow("avoid")]
    public void PageBreakInside_AcceptsLegacyValue(string value)
    {
        var property = ParseDeclaration($"page-break-inside:{value}");
        Assert.IsTrue(property.HasValue);
        Assert.AreEqual(value, property.Value);
    }

    [TestMethod]
    [DataRow("avoid-page")]
    [DataRow("avoid-column")]
    [DataRow("avoid-region")]
    [DataRow("page")]
    public void PageBreakInside_RejectsNonLegacyValue(string value)
    {
        var property = ParseDeclaration($"page-break-inside:{value}");
        Assert.IsFalse(property.HasValue);
    }
}
