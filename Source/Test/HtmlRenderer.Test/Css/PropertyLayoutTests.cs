using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

using static HtmlRenderer.Test.CssEngineSupport.CssConstructionFunctions;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/Property.cs (the Clear/Position/Display/Visibility/Overflow/TableLayout
/// cases). Each property here is a plain single-keyword map lookup
/// (Source/HtmlRenderer/Core/CssEngine/StyleProperties/Flow/ClearProperty.cs,
/// PositionProperty.cs, DisplayProperty.cs; StyleProperties/Visibility/VisibilityProperty.cs;
/// StyleProperties/Flow/OverflowProperty.cs; StyleProperties/TableLayoutProperty.cs), backed respectively
/// by <c>Map.ClearModes</c>, <c>Map.PositionModes</c>, <c>Map.DisplayModes</c>, <c>Map.Visibilities</c>,
/// <c>Map.OverflowModes</c> (Model/Map.cs) and the <c>Toggle(Keywords.Fixed, Keywords.Auto)</c> converter
/// for table-layout (Model/Converters.cs line 538) - all verified to contain every keyword this port
/// exercises, so this file is a faithful, unmodified port of the source cases.
/// </summary>
[TestClass]
public sealed class PropertyLayoutTests
{
    [TestMethod]
    public void CssClearLegalLeft()
    {
        var snippet = "clear:left";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("clear", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ClearProperty>(property);
        var concrete = (ClearProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.AreEqual("left", concrete.Value);
        Assert.AreEqual("left", concrete.Original);
    }

    [TestMethod]
    public void CssClearLegalBoth()
    {
        var snippet = "clear:both";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("clear", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ClearProperty>(property);
        var concrete = (ClearProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.AreEqual("both", concrete.Value);
        Assert.AreEqual("both", concrete.Original);
    }

    [TestMethod]
    public void CssClearInherited()
    {
        var snippet = "clear:inherit";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("clear", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsTrue(property.IsInherited);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ClearProperty>(property);
        var concrete = (ClearProperty)property;
        Assert.IsNotNull(concrete);
    }

    [TestMethod]
    public void CssClearIllegal()
    {
        var snippet = "clear:yes";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("clear", property.Name);
        Assert.IsFalse(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ClearProperty>(property);
        var concrete = (ClearProperty)property;
        Assert.IsNotNull(concrete);
    }

    [TestMethod]
    public void CssPositionLegalAbsolute()
    {
        var snippet = "position:absolute";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("position", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<PositionProperty>(property);
        var concrete = (PositionProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.AreEqual("absolute", concrete.Value);
        Assert.AreEqual("absolute", concrete.Original);
    }

    [TestMethod]
    public void CssDisplayLegalBlock()
    {
        var snippet = "display:   block ";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("display", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<DisplayProperty>(property);
        var concrete = (DisplayProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.AreEqual("block", concrete.Value);
        Assert.AreEqual("block", concrete.Original);
    }

    [TestMethod]
    public void CssVisibilityLegalCollapse()
    {
        var snippet = "visibility:collapse";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("visibility", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<VisibilityProperty>(property);
        var concrete = (VisibilityProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.AreEqual("collapse", concrete.Value);
        Assert.AreEqual("collapse", concrete.Original);
    }

    [TestMethod]
    public void CssVisibilityLegalHiddenCompleteUppercase()
    {
        var snippet = "VISIBILITY:HIDDEN";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("visibility", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<VisibilityProperty>(property);
        var concrete = (VisibilityProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.AreEqual("hidden", concrete.Value);
        Assert.AreEqual("HIDDEN", concrete.Original);
    }

    [TestMethod]
    public void CssOverflowLegalAuto()
    {
        var snippet = "overflow:auto";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("overflow", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<OverflowProperty>(property);
        var concrete = (OverflowProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.AreEqual("auto", concrete.Value);
        Assert.AreEqual("auto", concrete.Original);
    }

    [TestMethod]
    public void CssTableLayoutLegalFixedCapitalX()
    {
        var snippet = "table-layout: fiXed";
        var property = ParseDeclaration(snippet);
        Assert.AreEqual("table-layout", property.Name);
        Assert.IsTrue(property.HasValue);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<TableLayoutProperty>(property);
        var concrete = (TableLayoutProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.AreEqual("fixed", concrete.Value);
        Assert.AreEqual("fiXed", concrete.Original);
    }
}
