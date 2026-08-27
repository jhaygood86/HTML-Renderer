using System.Collections.Generic;
using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

/// <summary>Ported from PeachPDF.Tests/CSS/PropertyTests/ColumnsProperty.cs. Pure CSSOM parse tests - no layout involved.</summary>
[TestClass]
public sealed class ColumnsPropertyTests
{
    [TestMethod]
    public void CssColumnWidthLengthLegal()
    {
        var snippet = "column-width: 300px";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("column-width", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ColumnWidthProperty>(property);
        var concrete = (ColumnWidthProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("300px", concrete.Value);
    }

    [TestMethod]
    public void CssColumnWidthPercentIllegal()
    {
        var snippet = "column-width: 30%";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("column-width", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ColumnWidthProperty>(property);
        var concrete = (ColumnWidthProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void CssColumnWidthVwLegal()
    {
        var snippet = "column-width: 0.3vw";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("column-width", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ColumnWidthProperty>(property);
        var concrete = (ColumnWidthProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("0.3vw", concrete.Value);
    }

    [TestMethod]
    public void CssColumnWidthAutoUppercaseLegal()
    {
        var snippet = "column-width: AUTO";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("column-width", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ColumnWidthProperty>(property);
        var concrete = (ColumnWidthProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("auto", concrete.Value);
    }

    [TestMethod]
    public void CssColumnCountAutoLowercaseLegal()
    {
        var snippet = "column-count: auto";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("column-count", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ColumnCountProperty>(property);
        var concrete = (ColumnCountProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("auto", concrete.Value);
    }

    [TestMethod]
    public void CssColumnCountNumberLegal()
    {
        var snippet = "column-count: 3";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("column-count", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ColumnCountProperty>(property);
        var concrete = (ColumnCountProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("3", concrete.Value);
    }

    [TestMethod]
    public void CssColumnCountZeroLegal()
    {
        var snippet = "column-count: 0";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("column-count", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ColumnCountProperty>(property);
        var concrete = (ColumnCountProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("0", concrete.Value);
    }

    [TestMethod]
    public void CssColumsZeroLegal()
    {
        var snippet = "columns: 0";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("columns", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ColumnsProperty>(property);
        var concrete = (ColumnsProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("0", concrete.Value);
    }

    [TestMethod]
    public void CssColumsLengthLegal()
    {
        var snippet = "columns: 10px";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("columns", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ColumnsProperty>(property);
        var concrete = (ColumnsProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("10px", concrete.Value);
    }

    [TestMethod]
    public void CssColumsNumberLegal()
    {
        var snippet = "columns: 4";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("columns", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ColumnsProperty>(property);
        var concrete = (ColumnsProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("4", concrete.Value);
    }

    [TestMethod]
    public void CssColumsLengthNumberLegal()
    {
        var snippet = "columns: 25em 5";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("columns", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ColumnsProperty>(property);
        var concrete = (ColumnsProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("25em 5", concrete.Value);
    }

    [TestMethod]
    public void CssColumsNumberLengthLegal()
    {
        var snippet = "columns : 5   25em  ";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("columns", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ColumnsProperty>(property);
        var concrete = (ColumnsProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("25em 5", concrete.Value);
    }

    [TestMethod]
    public void CssColumsAutoAutoLegal()
    {
        var snippet = "columns : auto auto";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("columns", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ColumnsProperty>(property);
        var concrete = (ColumnsProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("auto auto", concrete.Value);
    }

    [TestMethod]
    public void CssColumsAutoWidthOrderIndependentLegal()
    {
        // "auto" is valid for both column-count and column-width, so a positional match lets
        // column-width claim "auto" and strand "12em"; order-independent matching keeps it legal
        // ("columns: <'column-width'> || <'column-count'>", CSS Multi-column 1).
        var snippet = "columns: auto 12em";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("columns", property.Name);
        Assert.IsInstanceOfType<ColumnsProperty>(property);
        var concrete = (ColumnsProperty)property;
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("12em auto", concrete.Value);
    }

    [TestMethod]
    public void CssColumsWidthAutoLegal()
    {
        // Control: the already-ordered form parses through the fast path.
        var snippet = "columns: 12em auto";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("columns", property.Name);
        Assert.IsInstanceOfType<ColumnsProperty>(property);
        var concrete = (ColumnsProperty)property;
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("12em auto", concrete.Value);
    }

    [TestMethod]
    public void CssColumsAutoLegal()
    {
        var snippet = "columns : auto  ";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("columns", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ColumnsProperty>(property);
        var concrete = (ColumnsProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("auto", concrete.Value);
    }

    [TestMethod]
    public void CssColumsNumberPercenIllegal()
    {
        var snippet = "columns : 5   25%  ";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("columns", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ColumnsProperty>(property);
        var concrete = (ColumnsProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void CssColumSpanAllLegal()
    {
        var snippet = "column-span: all";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("column-span", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ColumnSpanProperty>(property);
        var concrete = (ColumnSpanProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("all", concrete.Value);
    }

    [TestMethod]
    public void CssColumSpanNoneUppercaseLegal()
    {
        var snippet = "column-span: None";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("column-span", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ColumnSpanProperty>(property);
        var concrete = (ColumnSpanProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("none", concrete.Value);
    }

    [TestMethod]
    public void CssColumSpanLengthIllegal()
    {
        var snippet = "column-span: 10px";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("column-span", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ColumnSpanProperty>(property);
        var concrete = (ColumnSpanProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    [DynamicData(nameof(ColumnGapTestDataValues))]
    public void ColumnGapLegalValues(string value)
        => CssConstructionFunctions.TestForLegalValue<ColumnGapProperty>(PropertyNames.ColumnGap, value);

    [TestMethod]
    public void CssColumFillBalanceLegal()
    {
        var snippet = "column-fill: balance;";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("column-fill", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ColumnFillProperty>(property);
        var concrete = (ColumnFillProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("balance", concrete.Value);
    }

    [TestMethod]
    public void CssColumFillAutoLegal()
    {
        var snippet = "column-fill: auto;";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("column-fill", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ColumnFillProperty>(property);
        var concrete = (ColumnFillProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("auto", concrete.Value);
    }

    [TestMethod]
    public void CssColumRuleColorTransparentLegal()
    {
        var snippet = "column-rule-color: transparent";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("column-rule-color", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ColumnRuleColorProperty>(property);
        var concrete = (ColumnRuleColorProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("rgba(0, 0, 0, 0)", concrete.Value);
    }

    [TestMethod]
    public void CssColumRuleColorRgbLegal()
    {
        var snippet = "column-rule-color: rgb(192, 56, 78)";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("column-rule-color", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ColumnRuleColorProperty>(property);
        var concrete = (ColumnRuleColorProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("rgb(192, 56, 78)", concrete.Value);
    }

    [TestMethod]
    public void CssColumRuleColorRedLegal()
    {
        var snippet = "column-rule-color: red";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("column-rule-color", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ColumnRuleColorProperty>(property);
        var concrete = (ColumnRuleColorProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("rgb(255, 0, 0)", concrete.Value);
    }

    [TestMethod]
    public void CssColumRuleColorNoneIllegal()
    {
        var snippet = "column-rule-color: none";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("column-rule-color", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ColumnRuleColorProperty>(property);
        var concrete = (ColumnRuleColorProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void CssColumRuleStyleInsetTailUpperLegal()
    {
        var snippet = "column-rule-style: inSET";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("column-rule-style", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ColumnRuleStyleProperty>(property);
        var concrete = (ColumnRuleStyleProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("inset", concrete.Value);
    }

    [TestMethod]
    public void CssColumRuleStyleNoneLegal()
    {
        var snippet = "column-rule-style: none";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("column-rule-style", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ColumnRuleStyleProperty>(property);
        var concrete = (ColumnRuleStyleProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("none", concrete.Value);
    }

    [TestMethod]
    public void CssColumRuleStyleAutoIllegal()
    {
        var snippet = "column-rule-style: auto ";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("column-rule-style", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ColumnRuleStyleProperty>(property);
        var concrete = (ColumnRuleStyleProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void CssColumRuleWidthLengthLegal()
    {
        var snippet = "column-rule-width: 2px";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("column-rule-width", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ColumnRuleWidthProperty>(property);
        var concrete = (ColumnRuleWidthProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("2px", concrete.Value);
    }

    [TestMethod]
    public void CssColumRuleWidthThickLegal()
    {
        var snippet = "column-rule-width: thick";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("column-rule-width", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ColumnRuleWidthProperty>(property);
        var concrete = (ColumnRuleWidthProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("5px", concrete.Value);
    }

    [TestMethod]
    public void CssColumRuleWidthMediumLegal()
    {
        var snippet = "column-rule-width : medium !important ";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("column-rule-width", property.Name);
        Assert.IsTrue(property.IsImportant);
        Assert.IsInstanceOfType<ColumnRuleWidthProperty>(property);
        var concrete = (ColumnRuleWidthProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("3px", concrete.Value);
    }

    [TestMethod]
    public void CssColumRuleWidthThinUppercaseLegal()
    {
        var snippet = "column-rule-width: THIN";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("column-rule-width", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ColumnRuleWidthProperty>(property);
        var concrete = (ColumnRuleWidthProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("1px", concrete.Value);
    }

    [TestMethod]
    public void CssColumRuleDottedLegal()
    {
        var snippet = "column-rule: dotted";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("column-rule", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ColumnRuleProperty>(property);
        var concrete = (ColumnRuleProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("dotted", concrete.Value);
    }

    [TestMethod]
    public void CssColumRuleSolidBlueLegal()
    {
        var snippet = "column-rule: solid  blue";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("column-rule", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ColumnRuleProperty>(property);
        var concrete = (ColumnRuleProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("rgb(0, 0, 255) solid", concrete.Value);
    }

    [TestMethod]
    public void CssColumRuleSolidLengthLegal()
    {
        var snippet = "column-rule: solid 8px";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("column-rule", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ColumnRuleProperty>(property);
        var concrete = (ColumnRuleProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("8px solid", concrete.Value);
    }

    [TestMethod]
    public void CssColumRuleThickInsetBlueLegal()
    {
        var snippet = "column-rule: thick inset blue";
        var property = CssConstructionFunctions.ParseDeclaration(snippet);
        Assert.AreEqual("column-rule", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<ColumnRuleProperty>(property);
        var concrete = (ColumnRuleProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("rgb(0, 0, 255) 5px inset", concrete.Value);
    }

    public static IEnumerable<object[]> ColumnGapTestDataValues => CssConstructionFunctions.LengthOrPercentOrGlobalTestValues;
}
