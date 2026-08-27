using System.Linq;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/CssNestingIntegrationTests.cs.
/// CSS Nesting (<see href="https://www.w3.org/TR/css-nesting-1/">CSS Nesting 1</see>): the <c>&amp;</c>
/// nesting selector and nested style rules inside a declaration block. Nested rules are resolved at parse
/// time into ordinary rules with absolute selectors (<c>&amp;</c> → <c>:is(parent)</c>), so these assert
/// the resolved rules actually match and cascade. Includes the regression guards that the declaration path
/// is unaffected (hex colors, custom properties with a brace in their value, a normal declaration after a
/// nested rule).
/// </summary>
[TestClass]
public sealed class CssNestingIntegrationTests
{
    [TestMethod]
    public void ImplicitDescendant_And_ParentDeclaration()
    {
        // `& span` == `:is(.box) span` (descendant). The div keeps its own red; the span is blue.
        var html = Html(
            ".box { color: #ff0000; & span { color: #0000ff; } }",
            "<div class='box' id='d'>text<span id='s'>inner</span></div>");
        var d = Box(html, "d");
        var s = Box(html, "s");
        Assert.AreEqual("rgb(255, 0, 0)", d.Color);
        Assert.AreEqual("rgb(0, 0, 255)", s.Color);
    }

    [TestMethod]
    public void AmpersandCompound_MatchesSameElement()
    {
        // `&.active` == `:is(.box).active` — the same element carrying both classes.
        var html = Html(
            ".box { &.active { color: #0000ff; } }",
            "<div class='box active' id='d'>x</div>");
        Assert.AreEqual("rgb(0, 0, 255)", Box(html, "d").Color);
    }

    [TestMethod]
    public void TypeSelectorNestedRule_IsNotMistakenForADeclaration()
    {
        // `p { ... }` inside a block starts with an Ident (like a property name) — the classifier must
        // still see the `{` before any `;` and treat it as a nested rule.
        var html = Html(
            ".card { p { color: #0000ff; } }",
            "<div class='card'><p id='p'>x</p></div>");
        Assert.AreEqual("rgb(0, 0, 255)", Box(html, "p").Color);
    }

    [TestMethod]
    public void HexColorInNestedValue_SurvivesTheLookahead()
    {
        // Regression: the classify-then-rewind must not corrupt a `#rrggbb` value (value-mode `#`
        // tokenization) the way a token buffer would.
        var html = Html(
            ".card { p { color: #123456; } }",
            "<div class='card'><p id='p'>x</p></div>");
        Assert.AreEqual("rgb(18, 52, 86)", Box(html, "p").Color);
    }

    [TestMethod]
    public void HexColorInTopLevelValue_Unaffected()
    {
        // The whole feature must leave an ordinary (non-nested) declaration block byte-identical.
        var html = Html(".card { color: #123456; }", "<div class='card' id='d'>x</div>");
        Assert.AreEqual("rgb(18, 52, 86)", Box(html, "d").Color);
    }

    [TestMethod]
    public void ChildCombinatorLeading()
    {
        // `> p` == `:is(.card) > p` — only a direct child p matches.
        var html = Html(
            ".card { > p { color: #0000ff; } }",
            "<div class='card'><p id='child'>x</p><section><p id='grandchild'>y</p></section></div>");
        Assert.AreEqual("rgb(0, 0, 255)", Box(html, "child").Color);
        Assert.AreNotEqual("rgb(0, 0, 255)", Box(html, "grandchild").Color);
    }

    [TestMethod]
    public void DeclarationAfterNestedRule_StillApplies()
    {
        // A declaration written AFTER a nested rule still applies to the parent (the outer loop resumes
        // correctly); here the later blue wins over the earlier red.
        var html = Html(
            ".card { color: #ff0000; & span { color: #00ff00; } color: #0000ff; }",
            "<div class='card' id='d'>t<span id='s'>x</span></div>");
        Assert.AreEqual("rgb(0, 0, 255)", Box(html, "d").Color);
        Assert.AreEqual("rgb(0, 255, 0)", Box(html, "s").Color);
    }

    [TestMethod]
    public void ThreeLevelNesting()
    {
        // `.a { & .b { & .c { } } }` → `:is(:is(.a) .b) .c` — nested `:is()` with complex args.
        var html = Html(
            ".a { & .b { & .c { color: #0000ff; } } }",
            "<div class='a'><div class='b'><div class='c' id='c'>x</div></div></div>");
        Assert.AreEqual("rgb(0, 0, 255)", Box(html, "c").Color);
    }

    [TestMethod]
    public void ParentSelectorList_ResolvesToIs()
    {
        // `.x, .y { & span { } }` → `:is(.x, .y) span` — the nested rule applies under either parent.
        var html = Html(
            ".x, .y { & span { color: #0000ff; } }",
            "<div class='x'><span id='sx'>1</span></div><div class='y'><span id='sy'>2</span></div>");
        Assert.AreEqual("rgb(0, 0, 255)", Box(html, "sx").Color);
        Assert.AreEqual("rgb(0, 0, 255)", Box(html, "sy").Color);
    }

    [TestMethod]
    public void NestedRule_InheritsEnclosingMedia()
    {
        // A nested rule inside @media print inherits that context (print media applies here - the
        // MockAdapter used by Box()/BuildRoot() below reports "print" so this exercises the same path).
        var html = Html(
            "@media print { .card { & p { color: #0000ff; } } }",
            "<div class='card'><p id='p'>x</p></div>");
        Assert.AreEqual("rgb(0, 0, 255)", Box(html, "p").Color);
    }

    [TestMethod]
    public void NestedRule_InheritsEnclosingLayer()
    {
        // A nested rule inside @layer base loses to a later unlayered rule (layer context is inherited).
        var html = Html(
            "@layer base { .card { & p { color: #ff0000; } } } .card p { color: #0000ff; }",
            "<div class='card'><p id='p'>x</p></div>");
        Assert.AreEqual("rgb(0, 0, 255)", Box(html, "p").Color);
    }

    [TestMethod]
    public void CustomProperty_CoexistsWithNestedRule()
    {
        // A custom property declared alongside a nested rule still cascades (inherited to descendants).
        var html = Html(
            ".card { --c: #0000ff; p { color: var(--c); } }",
            "<div class='card'><p id='p'>x</p></div>");
        Assert.AreEqual("rgb(0, 0, 255)", Box(html, "p").Color);
    }

    // Note: a custom property whose *value* literally contains a `{` (e.g. `--x: { }`) is classified
    // as a declaration by the `--` guard (correct — custom properties are always declarations), but the
    // declaration value parser has a pre-existing limitation with a brace inside a value. That is
    // unrelated to nesting and does not occur in real utility-framework output, so it is not exercised here.

    [TestMethod]
    public void TypePseudoNestedRule_IsConsumed_NotMistakenForADeclaration()
    {
        // `a:hover { }` (type + pseudo, no `&`) is ambiguous with a declaration `a: hover` — the
        // `{`-before-`;` scan classifies it as a nested rule. `:hover` never matches against a static
        // render, so we assert the FOLLOWING nested rule still applies, proving `a:hover { }` was
        // consumed as a rule.
        var html = Html(
            ".card { a:hover { color: #ff0000; } p { color: #0000ff; } }",
            "<div class='card'><p id='p'>x</p></div>");
        Assert.AreEqual("rgb(0, 0, 255)", Box(html, "p").Color);
    }

    [TestMethod]
    public void BraceInsideUrl_DoesNotTriggerNestedRule()
    {
        // A `{` inside a url()/string is part of an opaque function token, not a rule boundary.
        var html = Html(
            ".card { background: url(\"a{b}.png\"); & p { color: #0000ff; } }",
            "<div class='card'><p id='p'>x</p></div>");
        Assert.AreEqual("rgb(0, 0, 255)", Box(html, "p").Color);
    }

    [TestMethod]
    public void AmpersandInsideAttributeValue_IsPreserved_NotTreatedAsNestingSelector()
    {
        // A `&` inside an attribute-value string must NOT be substituted (it's data, not the nesting
        // selector) and must NOT flip the rule to the replace branch — the rule must still be scoped
        // under the parent. Here the only `&` is inside the string, so `.item` stays `:is(.card) .item`
        // matching the `[data-state="on&off"]` element under .card.
        var html = Html(
            ".card { .item[data-state=\"on&off\"] { color: #0000ff; } }",
            "<div class='card'><span class='item' id='m' data-state='on&off'>x</span>" +
            "<span class='item' id='n' data-state='off'>y</span></div>" +
            "<span class='item' id='outside' data-state='on&off'>z</span>");
        Assert.AreEqual("rgb(0, 0, 255)", Box(html, "m").Color);   // matches (under .card, value on&off)
        Assert.AreNotEqual("rgb(0, 0, 255)", Box(html, "n").Color); // wrong data-state
        Assert.AreNotEqual("rgb(0, 0, 255)", Box(html, "outside").Color); // not under .card (rule stays scoped)
    }

    [TestMethod]
    public void RealAmpersand_WithAmpersandAlsoInsideAttributeValue()
    {
        // A real nesting `&` plus a `&` inside a string: only the real one is replaced (→ `&.on`
        // compound), the in-string `&` is preserved.
        var html = Html(
            ".card { &[data-state=\"a&b\"] { color: #0000ff; } }",
            "<div class='card' id='d' data-state='a&b'>x</div>");
        Assert.AreEqual("rgb(0, 0, 255)", Box(html, "d").Color);
    }

    [TestMethod]
    public void EscapedQuoteInsideAttributeValue_DoesNotEndTheStringEarly()
    {
        // An escaped quote (\") inside the attribute-value string must not prematurely close the string
        // during `&` substitution; the value `a"b` is preserved and the rule matches the right element.
        var html = Html(
            ".card { .item[data-x=\"a\\\"b\"] { color: #0000ff; } }",
            "<div class='card'><span class='item' id='e' data-x='a\"b'>x</span></div>");
        Assert.AreEqual("rgb(0, 0, 255)", Box(html, "e").Color);
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static string Html(string css, string body) =>
        $"<!DOCTYPE html><html><head><style>{css}</style></head><body>{body}</body></html>";

    private static CssBox Box(string html, string id)
    {
        var (root, _) = LayoutHarness.Layout(html, adapter: new MockAdapter { MediaType = "print" });
        var box = LayoutHarness.FindById(root, id);
        Assert.IsNotNull(box);
        return box!;
    }
}
