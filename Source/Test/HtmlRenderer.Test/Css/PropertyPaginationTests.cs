using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

using static HtmlRenderer.Test.CssEngineSupport.CssConstructionFunctions;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/Property.cs (the Orphans/Widows/UnicodeBidirectional cases).
///
/// orphans (StyleProperties/OrphansProperty.cs) uses <c>Converters.NaturalIntegerConverter</c>
/// (<c>ValueExtensions.ToNaturalInteger</c>, Model/Converters.cs lines 36-37) - a non-negative integer
/// only, rejecting both negative values and non-integers.
///
/// widows (StyleProperties/WidowsProperty.cs) uses the plain <c>Converters.IntegerConverter</c>
/// (<c>ValueExtensions.ToInteger</c>) rather than the natural-number variant - it still rejects a value
/// carrying a length unit like "5px" since that isn't a bare integer token at all.
///
/// unicode-bidi (StyleProperties/UnicodeBidirectionalProperty.cs) uses
/// <c>Converters.UnicodeModeConverter</c> (backed by <c>Map.UnicodeModes</c>, Model/Map.cs lines 310-319),
/// which defines normal/embed/isolate/isolate-override/bidirectional-override/plaintext - "bidi-override"
/// and "isolate"/"plaintext"/"embed" are all present, confirming this port's value set matches the source
/// file's expectations unchanged; PeachPDF's own <c>UnicodeMode</c>-typed assertions are commented out in
/// the source (the concrete <c>State</c> member does not exist on this port's <c>Property</c>, mirroring
/// how the source itself already leaves them commented rather than asserting them).
/// </summary>
[TestClass]
public sealed class PropertyPaginationTests
{
    [TestMethod]
    public void CssOrphansZeroLegal()
    {
        var snippet = "orphans : 0 ";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("orphans", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<OrphansProperty>(property);
        var concrete = (OrphansProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("0", concrete.Value);
        Assert.AreEqual("0", concrete.Original);
    }

    [TestMethod]
    public void CssOrphansTwoLegal()
    {
        var snippet = "orphans : 2 ";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("orphans", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<OrphansProperty>(property);
        var concrete = (OrphansProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("2", concrete.Value);
        Assert.AreEqual("2", concrete.Original);
    }

    [TestMethod]
    public void CssOrphansNegativeIllegal()
    {
        var snippet = "orphans : -2 ";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("orphans", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<OrphansProperty>(property);
        var concrete = (OrphansProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void CssOrphansFloatingIllegal()
    {
        var snippet = "orphans : 1.5 ";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("orphans", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<OrphansProperty>(property);
        var concrete = (OrphansProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void CssWidowsZeroLegal()
    {
        var snippet = "widows: 0";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("widows", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<WidowsProperty>(property);
        var concrete = (WidowsProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("0", concrete.Value);
        Assert.AreEqual("0", concrete.Original);
    }

    [TestMethod]
    public void CssWidowsThreeLegal()
    {
        var snippet = "widows: 3";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("widows", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<WidowsProperty>(property);
        var concrete = (WidowsProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("3", concrete.Value);
        Assert.AreEqual("3", concrete.Original);
    }

    [TestMethod]
    public void CssWidowsLengthIllegal()
    {
        var snippet = "widows: 5px";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("widows", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<WidowsProperty>(property);
        var concrete = (WidowsProperty)property;
        Assert.IsTrue(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void CssUnicodeBidiEmbedLegal()
    {
        var snippet = "unicode-BIDI: Embed";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("unicode-bidi", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<UnicodeBidirectionalProperty>(property);
        var concrete = (UnicodeBidirectionalProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("embed", concrete.Value);
        Assert.AreEqual("Embed", concrete.Original);
    }

    [TestMethod]
    public void CssUnicodeBidiIsolateLegal()
    {
        var snippet = "unicode-Bidi: isolate";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("unicode-bidi", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<UnicodeBidirectionalProperty>(property);
        var concrete = (UnicodeBidirectionalProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("isolate", concrete.Value);
        Assert.AreEqual("isolate", concrete.Original);
    }

    [TestMethod]
    public void CssUnicodeBidiBidiOverrideLegal()
    {
        var snippet = "unicode-Bidi: Bidi-Override";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("unicode-bidi", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<UnicodeBidirectionalProperty>(property);
        var concrete = (UnicodeBidirectionalProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("bidi-override", concrete.Value);
        Assert.AreEqual("Bidi-Override", concrete.Original);
    }

    [TestMethod]
    public void CssUnicodeBidiPlaintextLegal()
    {
        var snippet = "unicode-Bidi: PLAINTEXT";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("unicode-bidi", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<UnicodeBidirectionalProperty>(property);
        var concrete = (UnicodeBidirectionalProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("plaintext", concrete.Value);
        Assert.AreEqual("PLAINTEXT", concrete.Original);
    }

    [TestMethod]
    public void CssUnicodeBidiIllegal()
    {
        var snippet = "unicode-bidi: none";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("unicode-bidi", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<UnicodeBidirectionalProperty>(property);
        var concrete = (UnicodeBidirectionalProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }
}
