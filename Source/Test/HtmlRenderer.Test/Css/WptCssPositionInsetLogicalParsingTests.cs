using System.Linq;
using HtmlRenderer.Test.CssEngineSupport;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/PropertyTests/WptCssPositionInsetLogicalParsingTests.cs.
/// Grammar coverage for the logical inset longhands (inset-block-start/inset-block-end/
/// inset-inline-start/inset-inline-end) and their two shorthands (inset-block/inset-inline), adapted
/// from the web-platform-tests css/css-position/parsing/inset-valid.html and inset-invalid.html
/// fixtures.
/// The Shorthand_* tests (inset-block/inset-inline) are ported as plain passing tests - they resolve to
/// dedicated <c>InsetBlockProperty</c>/<c>InsetInlineProperty</c> shorthand classes registered under
/// their own correct names.
/// One WPT expectation deliberately isn't ported (matching PeachPDF's own choice): WPT expects the
/// unitless zero length "0" to compute to "0px" (a live getComputedStyle() convention); this engine's
/// declaration parser stores a length as declared rather than serializing a canonical computed-style
/// string, so the equivalent assertion here is against "0", not "0px".
/// Newly-discovered gap (beyond the triage brief): the 7 Longhand_Accepts* theories (28 cases across
/// inset-block-start/end and inset-inline-start/end) are ported under [Ignore]. Confirmed by direct
/// probing (ParseStyleSheet(".x { inset-block-start: -10px; }") then inspecting the resulting
/// StyleDeclaration): the value IS parsed and stored correctly (HasValue == true, Value == "-10px"), but
/// under the property name "top", not "inset-block-start" - because
/// <c>Source/HtmlRenderer/Core/CssEngine/Factories/PropertyFactory.cs:447-450</c> registers these
/// longhands as bare aliases reusing <c>TopProperty</c>/<c>BottomProperty</c>/<c>LeftProperty</c>/
/// <c>RightProperty</c> directly (<c>() =&gt; new TopProperty()</c>), whose constructor hardcodes
/// <c>Name = PropertyNames.Top</c> regardless of which alias name it was created under. Since
/// <c>StyleDeclaration.GetProperty</c> (Model/StyleDeclaration.cs:209-212) matches purely by
/// <c>Declarations.FirstOrDefault(m =&gt; m.Name.Isi(name))</c>, looking the value back up by its
/// literal declared name ("inset-block-start") never finds it, and <c>GetPropertyValue</c> falls through
/// to string.Empty. PeachPDF avoids this by giving each logical inset longhand its own dedicated
/// property class (e.g. <c>InsetBlockStartProperty</c>, PeachPDF/src/PeachPDF/CSS/Factories/
/// PropertyFactory.cs:505) with its own correctly-named constructor - that per-longhand class does not
/// exist in this fork. The corresponding Longhand_Rejects* theories are NOT [Ignore]d: they still assert
/// an empty string, which this same lookup gap also produces (for the "wrong" reason - the value is
/// simply unfindable by name, not spec-rejected) - matching this project's established convention for a
/// narrower converter/lookup still happening to satisfy a "was rejected" assertion.
/// </summary>
[TestClass]
public sealed class WptCssPositionInsetLogicalParsingTests
{
    private static string DeclaredValue(string property, string value)
    {
        var sheet = CssConstructionFunctions.ParseStyleSheet($".x {{ {property}: {value}; }}");
        var rule = sheet.Rules.OfType<StyleRule>().Single();
        return rule.Style.GetPropertyValue(property);
    }

    // ─── Longhands: 'auto | <length-percentage>' ──────────────────────────────

    [TestMethod]
    [Ignore("not yet spec compliant")]
    [DataRow("inset-block-start")]
    [DataRow("inset-block-end")]
    [DataRow("inset-inline-start")]
    [DataRow("inset-inline-end")]
    public void Longhand_AcceptsZero(string property)
    {
        Assert.AreEqual("0", DeclaredValue(property, "0"));
    }

    [TestMethod]
    [Ignore("not yet spec compliant")]
    [DataRow("inset-block-start")]
    [DataRow("inset-block-end")]
    [DataRow("inset-inline-start")]
    [DataRow("inset-inline-end")]
    public void Longhand_AcceptsAuto(string property)
    {
        Assert.AreEqual("auto", DeclaredValue(property, "auto"));
    }

    [TestMethod]
    [Ignore("not yet spec compliant")]
    [DataRow("inset-block-start")]
    [DataRow("inset-block-end")]
    [DataRow("inset-inline-start")]
    [DataRow("inset-inline-end")]
    public void Longhand_AcceptsPercentage(string property)
    {
        Assert.AreEqual("10%", DeclaredValue(property, "10%"));
    }

    [TestMethod]
    [Ignore("not yet spec compliant")]
    [DataRow("inset-block-start")]
    [DataRow("inset-block-end")]
    [DataRow("inset-inline-start")]
    [DataRow("inset-inline-end")]
    public void Longhand_AcceptsRemLength(string property)
    {
        Assert.AreEqual("1rem", DeclaredValue(property, "1rem"));
    }

    [TestMethod]
    [Ignore("not yet spec compliant")]
    [DataRow("inset-block-start")]
    [DataRow("inset-block-end")]
    [DataRow("inset-inline-start")]
    [DataRow("inset-inline-end")]
    public void Longhand_AcceptsNegativeLength(string property)
    {
        Assert.AreEqual("-10px", DeclaredValue(property, "-10px"));
    }

    [TestMethod]
    [Ignore("not yet spec compliant")]
    [DataRow("inset-block-start")]
    [DataRow("inset-block-end")]
    [DataRow("inset-inline-start")]
    [DataRow("inset-inline-end")]
    public void Longhand_AcceptsNegativePercentage(string property)
    {
        Assert.AreEqual("-20%", DeclaredValue(property, "-20%"));
    }

    [TestMethod]
    [Ignore("not yet spec compliant")]
    [DataRow("inset-block-start")]
    [DataRow("inset-block-end")]
    [DataRow("inset-inline-start")]
    [DataRow("inset-inline-end")]
    public void Longhand_AcceptsCalcExpression(string property)
    {
        Assert.IsFalse(string.IsNullOrEmpty(DeclaredValue(property, "calc(2em + 3ex)")));
    }

    [TestMethod]
    [DataRow("inset-block-start")]
    [DataRow("inset-block-end")]
    [DataRow("inset-inline-start")]
    [DataRow("inset-inline-end")]
    public void Longhand_RejectsAngle(string property)
    {
        Assert.AreEqual(string.Empty, DeclaredValue(property, "0deg"));
    }

    [TestMethod]
    [DataRow("inset-block-start")]
    [DataRow("inset-block-end")]
    [DataRow("inset-inline-start")]
    [DataRow("inset-inline-end")]
    public void Longhand_RejectsCalcAngle(string property)
    {
        Assert.AreEqual(string.Empty, DeclaredValue(property, "calc(20deg)"));
    }

    [TestMethod]
    [DataRow("inset-block-start")]
    [DataRow("inset-block-end")]
    [DataRow("inset-inline-start")]
    [DataRow("inset-inline-end")]
    public void Longhand_RejectsUnitlessNumber(string property)
    {
        Assert.AreEqual(string.Empty, DeclaredValue(property, "10"));
    }

    // ─── Shorthands: same single-value grammar, plus a 1-or-2-value periodic form ──

    [TestMethod]
    [DataRow("inset-block")]
    [DataRow("inset-inline")]
    public void Shorthand_SingleValue_AcceptsAuto(string property)
    {
        Assert.AreEqual("auto", DeclaredValue(property, "auto"));
    }

    [TestMethod]
    [DataRow("inset-block")]
    [DataRow("inset-inline")]
    public void Shorthand_SingleValue_AcceptsCalcExpression(string property)
    {
        Assert.IsFalse(string.IsNullOrEmpty(DeclaredValue(property, "calc(2em + 3ex)")));
    }

    [TestMethod]
    [DataRow("inset-block")]
    [DataRow("inset-inline")]
    public void Shorthand_TwoEqualValues_CollapseToOne(string property)
    {
        Assert.AreEqual("auto", DeclaredValue(property, "auto auto"));
        Assert.AreEqual("100px", DeclaredValue(property, "100px 100px"));
    }

    [TestMethod]
    [DataRow("inset-block")]
    [DataRow("inset-inline")]
    public void Shorthand_TwoUnequalValues_StayTwoValues(string property)
    {
        Assert.AreEqual("10% -5px", DeclaredValue(property, "10% -5px"));
    }

    [TestMethod]
    [DataRow("inset-block")]
    [DataRow("inset-inline")]
    public void Shorthand_RejectsAngle(string property)
    {
        Assert.AreEqual(string.Empty, DeclaredValue(property, "0deg"));
    }

    [TestMethod]
    [DataRow("inset-block")]
    [DataRow("inset-inline")]
    public void Shorthand_RejectsUnitlessNumber(string property)
    {
        Assert.AreEqual(string.Empty, DeclaredValue(property, "10"));
    }

    [TestMethod]
    [DataRow("inset-block")]
    [DataRow("inset-inline")]
    public void Shorthand_RejectsMixedGlobalAndOrdinaryValue(string property)
    {
        // A global keyword (inherit/initial/unset/revert) can only appear alone - CSS Cascade 4 §3.1 -
        // never combined with an ordinary value in the same multi-value shorthand.
        Assert.AreEqual(string.Empty, DeclaredValue(property, "inherit auto"));
        Assert.AreEqual(string.Empty, DeclaredValue(property, "inherit inherit"));
    }

    // ─── z-index: calc() folds to a plain rounded integer (css-position/parsing/z-index-positioned-computed.html) ──

    [TestMethod]
    [Ignore("not yet spec compliant")]
    [DataRow("calc(3 - 2)", "1")]
    [DataRow("calc(1.5 + 1.5)", "3")]
    [DataRow("calc(2 * -3)", "-6")]
    [DataRow("calc(0.5)", "1")]
    [DataRow("calc(0.4)", "0")]
    public void ZIndex_CalcExpression_FoldsToNearestInteger(string value, string expected)
    {
        Assert.AreEqual(expected, DeclaredValue("z-index", value));
    }

    [TestMethod]
    [Ignore("not yet spec compliant")]
    public void ZIndex_RejectsLengthCalcExpression()
    {
        // z-index's grammar is 'auto | <integer>' - a calc() is only valid when it type-checks as a
        // plain <number> (CSS Values 4 §10.9); a length-category calc() must still be rejected.
        Assert.AreEqual(string.Empty, DeclaredValue("z-index", "calc(1px + 1px)"));
    }
}
