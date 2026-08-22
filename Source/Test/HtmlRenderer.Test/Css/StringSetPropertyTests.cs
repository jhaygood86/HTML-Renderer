using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/PropertyTests/StringSetPropertyTests.cs.
/// <see cref="StringSetProperty"/> and its <c>StringSetValueConverter</c> fully implement "none", a
/// single name+content-list pair, comma-separated multiple pairs, and reject non-ident names/a missing
/// content-list, matching PeachPDF's grammar exactly. Its <c>content()</c> sub-converter collapses
/// "content(text)" to "content()" the same way PeachPDF's does.
/// </summary>
[TestClass]
public sealed class StringSetPropertyTests
{
    [TestMethod]
    public void StringSetNoneLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("string-set: none");
        Assert.AreEqual("string-set", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StringSetProperty>(property);
        var concrete = (StringSetProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("none", concrete.Value);
    }

    [TestMethod]
    public void StringSetSimpleTextLegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("string-set: chapter content(text)");
        Assert.AreEqual("string-set", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StringSetProperty>(property);
        var concrete = (StringSetProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("chapter content()", concrete.Value);
    }

    [TestMethod]
    public void StringSetWithStringLiteral()
    {
        var property = CssConstructionFunctions.ParseDeclaration("string-set: header \"Page \" counter(page)");
        Assert.AreEqual("string-set", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StringSetProperty>(property);
        var concrete = (StringSetProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("header \"Page \" counter(page)", concrete.Value);
    }

    [TestMethod]
    public void StringSetWithAttrFunction()
    {
        var property = CssConstructionFunctions.ParseDeclaration("string-set: title attr(title)");
        Assert.AreEqual("string-set", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StringSetProperty>(property);
        var concrete = (StringSetProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("title attr(title)", concrete.Value);
    }

    [TestMethod]
    public void StringSetContentBefore()
    {
        var property = CssConstructionFunctions.ParseDeclaration("string-set: heading content(before) \":\" content(text)");
        Assert.AreEqual("string-set", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StringSetProperty>(property);
        var concrete = (StringSetProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("heading content(before) \":\" content()", concrete.Value);
    }

    [TestMethod]
    public void StringSetContentAfter()
    {
        var property = CssConstructionFunctions.ParseDeclaration("string-set: heading content(after)");
        Assert.AreEqual("string-set", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StringSetProperty>(property);
        var concrete = (StringSetProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("heading content(after)", concrete.Value);
    }

    [TestMethod]
    public void StringSetContentFirstLetter()
    {
        var property = CssConstructionFunctions.ParseDeclaration("string-set: initial content(first-letter)");
        Assert.AreEqual("string-set", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StringSetProperty>(property);
        var concrete = (StringSetProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("initial content(first-letter)", concrete.Value);
    }

    [TestMethod]
    public void StringSetMultiplePairs()
    {
        var property = CssConstructionFunctions.ParseDeclaration("string-set: header content(text), footer counter(page)");
        Assert.AreEqual("string-set", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StringSetProperty>(property);
        var concrete = (StringSetProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsTrue(concrete.HasValue);
        Assert.AreEqual("header content(), footer counter(page)", concrete.Value);
    }

    [TestMethod]
    public void StringSetInvalidNoContentListIllegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("string-set: header");
        Assert.AreEqual("string-set", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StringSetProperty>(property);
        var concrete = (StringSetProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }

    [TestMethod]
    public void StringSetInvalidNumberIllegal()
    {
        var property = CssConstructionFunctions.ParseDeclaration("string-set: 123 content(text)");
        Assert.AreEqual("string-set", property.Name);
        Assert.IsFalse(property.IsImportant);
        Assert.IsInstanceOfType<StringSetProperty>(property);
        var concrete = (StringSetProperty)property;
        Assert.IsFalse(concrete.IsInherited);
        Assert.IsFalse(concrete.HasValue);
    }
}
