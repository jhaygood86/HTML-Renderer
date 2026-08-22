using System.Linq;
using HtmlRenderer.Test.TestSupport;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Parse;

namespace HtmlRenderer.Test.Css;

/// <summary>
/// Ported from PeachPDF.Tests/CSS/DocumentTreePseudoClassTests.cs.
/// The three pseudo-classes that depend only on the document tree - <c>:empty</c> (Selectors 4 §9.5),
/// <c>:any-link</c> (§7.1) and <c>:scope</c> (§6.6) - are genuinely implemented in PeachPDF, but
/// HTML-Renderer's <c>CssData.DoesSelectorMatch(PseudoClassSelector, CssBox)</c> (Core/CssData.cs:739-755)
/// only recognizes "hover", "root" and "link" - every other pseudo-class name (including these three)
/// falls through to "never matches". Every test below is therefore ignored: the assertions encode the
/// PeachPDF-correct behavior this engine doesn't have yet, not today's actual (structurally-registered-but-
/// unmatchable) behavior. The two SVG-specific PeachPDF tests are omitted entirely - there is no SVG
/// pipeline (no <c>ICssDomNode</c>/<c>SvgCssBoxDomNode</c> duality) in this engine to exercise. The two
/// tests that inspected PeachPDF's <c>ICssDomNode</c> facade directly (no HTML-Renderer counterpart) are
/// adapted to check the equivalent <see cref="CssBox"/> state directly instead.
/// </summary>
[TestClass]
public sealed class DocumentTreePseudoClassTests
{
    private const string Blue = "rgb(0, 0, 255)";
    private const string IgnoreReason =
        "HTML-Renderer's CssData.DoesSelectorMatch(PseudoClassSelector, CssBox) only recognizes " +
        "\"hover\"/\"root\"/\"link\" - :empty/:any-link/:scope parse but never match. See " +
        "Source/HtmlRenderer/Core/CssData.cs:739-755.";

    // ── :empty ─────────────────────────────────────────────────────────────────────────────────

    [TestMethod]
    [Ignore(IgnoreReason)]
    // "no children at all, other than white space" — including a run of newlines and tabs, which the
    // parser does hand to the box tree as a text box.
    [DataRow("<p id='p'></p>", true)]
    [DataRow("<p id='p'>   </p>", true)]
    [DataRow("<p id='p'>\n\t\r\n </p>", true)]
    // A comment is not a child for :empty's purposes (and never reaches the box tree at all).
    [DataRow("<p id='p'><!-- a comment --></p>", true)]
    [DataRow("<p id='p'>\n  <!-- a comment -->\n</p>", true)]
    // Any text that is not white space.
    [DataRow("<p id='p'>x</p>", false)]
    [DataRow("<p id='p'> x </p>", false)]
    // U+00A0 is significant content, not collapsible white space.
    [DataRow("<p id='p'>&nbsp;</p>", false)]
    // Any element child, even an empty or void one.
    [DataRow("<p id='p'><span></span></p>", false)]
    [DataRow("<p id='p'><br></p>", false)]
    public void Empty_MatchesOnlyAnElementWithNoNonWhitespaceChildren(string body, bool shouldMatch)
    {
        var box = Box(Html("p:empty { color: #0000ff; }", body), "p");
        Assert.AreEqual(shouldMatch, box.Color == Blue);
    }

    [TestMethod]
    [Ignore(IgnoreReason)]
    public void Empty_IsUnaffectedByGeneratedContent()
    {
        // ::before/::after synthesize a real child box as a side effect of their own rule matching, in the
        // same cascade pass :empty is evaluated in. :empty is defined over the source tree, so none of
        // that may count as a child.
        var box = Box(Html("p::before { content: 'x'; } p:empty { color: #0000ff; }", "<p id='p'></p>"), "p");
        Assert.AreEqual(Blue, box.Color);
    }

    [TestMethod]
    [Ignore(IgnoreReason)]
    public void Empty_IsUnaffectedByASynthesizedMarker()
    {
        // An <li>'s ::marker box is generated too, so an empty list item is still :empty.
        var box = Box(Html("li:empty { color: #0000ff; }", "<ul><li id='li'></li></ul>"), "li");
        Assert.AreEqual(Blue, box.Color);
    }

    [TestMethod]
    [Ignore(IgnoreReason)]
    public void Empty_ComposesWithCombinatorsAndNegation()
    {
        var html = Html(
            ".row :empty { color: #0000ff; } .row p:not(:empty) { color: #ff0000; }",
            "<div class='row'><p id='blank'></p><p id='filled'>text</p></div>");

        Assert.AreEqual(Blue, Box(html, "blank").Color);
        Assert.AreEqual("rgb(255, 0, 0)", Box(html, "filled").Color);
    }

    [TestMethod]
    [Ignore(IgnoreReason)]
    public void Empty_SharingAListWithAMatchableSelector_StillApplies()
    {
        // :empty was previously registered-but-unmatchable, so the list stayed valid. Making it match
        // must not change that half of the contract.
        var html = Html("h1, p:empty { color: #0000ff; }", "<h1 id='h'>Title</h1><p id='p'></p>");
        Assert.AreEqual(Blue, Box(html, "h").Color);
        Assert.AreEqual(Blue, Box(html, "p").Color);
    }

    [TestMethod]
    [Ignore(IgnoreReason)]
    public void Empty_SeesThroughAnAnonymousBox()
    {
        // The restructuring passes that run after the cascade wrap element/text children in anonymous
        // boxes, which are not source children — the test descends into them rather than stopping there.
        // (PeachPDF's ICssDomNode.IsEmpty/TagName facade has no HTML-Renderer counterpart; checked here
        // directly against CssBox state instead.)
        var root = BuildRoot(Html(
            "p { display: block; }",
            "<div id='mixed'>loose text<p>block</p></div><div id='blank'></div>"));

        var mixed = LayoutHarness.FindById(root, "mixed")!;
        var blank = LayoutHarness.FindById(root, "blank")!;

        Assert.IsTrue(mixed.Boxes.Any(b => b.HtmlTag is null && b.Boxes.Count > 0));
        Assert.IsFalse(Matches(mixed, ":empty"));
        Assert.IsTrue(Matches(blank, ":empty"));
    }

    [TestMethod]
    [Ignore(IgnoreReason)]
    public void Empty_NeverMatchesANonElementBox()
    {
        // Anonymous/text boxes have no tag, so they are not elements and can never be :empty.
        var root = BuildRoot(Html("p { display: block; }", "<div id='mixed'>loose text<p>block</p></div>"));
        var anonymous = LayoutHarness.FindById(root, "mixed")!.Boxes.First(b => b.HtmlTag is null);

        Assert.IsNull(anonymous.HtmlTag);
        Assert.IsFalse(Matches(anonymous, ":empty"));
    }

    // ── :any-link ──────────────────────────────────────────────────────────────────────────────

    [TestMethod]
    [Ignore(IgnoreReason)]
    public void AnyLink_MatchesAnAnchorWithAnHref_AndNothingElse()
    {
        var html = Html(
            ":any-link { color: #0000ff; }",
            "<a id='link' href='https://example.com'>link</a><a id='anchor' name='top'>anchor</a><p id='p'>text</p>");

        Assert.AreEqual(Blue, Box(html, "link").Color);
        Assert.AreNotEqual(Blue, Box(html, "anchor").Color);
        Assert.AreNotEqual(Blue, Box(html, "p").Color);
    }

    [TestMethod]
    [Ignore(IgnoreReason)]
    public void AnyLink_AndLink_SelectTheSameElements()
    {
        // :any-link is the union of :link and :visited; :visited never matches (no browsing history in a
        // static render), so the two must agree everywhere.
        var body = "<a id='link' href='#x'>link</a><a id='anchor'>anchor</a>";
        var withAnyLink = Html("a:any-link { color: #0000ff; }", body);
        var withLink = Html("a:link { color: #0000ff; }", body);

        foreach (var id in new[] { "link", "anchor" })
        {
            Assert.AreEqual(
                Box(withLink, id).Color == Blue,
                Box(withAnyLink, id).Color == Blue);
        }
    }

    [TestMethod]
    [Ignore(IgnoreReason)]
    public void AnyLink_ComposesWithACompoundSelector()
    {
        var html = Html(
            "a.cta:any-link { color: #0000ff; }",
            "<a id='cta' class='cta' href='#x'>go</a><a id='plain' href='#y'>plain</a>");

        Assert.AreEqual(Blue, Box(html, "cta").Color);
        Assert.AreNotEqual(Blue, Box(html, "plain").Color);
    }

    // ── :scope ─────────────────────────────────────────────────────────────────────────────────

    [TestMethod]
    [Ignore(IgnoreReason)]
    public void Scope_MatchesTheRootElement_LikeRoot()
    {
        // With no scoping root in play, :scope is the document's root element — the same answer :root
        // gives, on the same element and on no other.
        var root = BuildRoot(Html(":scope { color: #0000ff; }", "<p id='p'>text</p>"));
        var htmlBox = LayoutHarness.Descendants(root).First(b => b.HtmlTag != null && b.HtmlTag.Name == "html");

        Assert.AreEqual(Blue, htmlBox.Color);
        Assert.IsTrue(Matches(htmlBox, ":scope"));
        Assert.IsTrue(Matches(htmlBox, ":root"));

        foreach (var tag in new[] { "body", "p" })
        {
            var box = LayoutHarness.Descendants(root).First(b => b.HtmlTag != null && b.HtmlTag.Name == tag);
            Assert.IsFalse(Matches(box, ":scope"));
            Assert.IsFalse(Matches(box, ":root"));
        }
    }

    [TestMethod]
    [Ignore(IgnoreReason)]
    public void Scope_WorksAsTheLeftHandSideOfACombinator()
    {
        // The idiomatic use: `:scope x` / `:scope > x`, which stylesheets write in place of `:root x`.
        var html = Html(
            ":scope p { color: #0000ff; } :scope > body > p.direct { font-weight: bold; }",
            "<p id='deep' class='direct'>text</p>");

        var box = Box(html, "deep");
        Assert.AreEqual(Blue, box.Color);
        Assert.AreEqual("bold", box.FontWeight);
    }

    [TestMethod]
    [Ignore(IgnoreReason)]
    public void Scope_AndRoot_ShareTheirDeclarationsInAList()
    {
        var box = Box(Html(":root, :scope { --brand: #0000ff; } p { color: var(--brand); }", "<p id='p'>text</p>"), "p");
        Assert.AreEqual(Blue, box.Color);
    }

    // ── Helpers (mirrors CssDataRuleIndexingTests.cs conventions) ────────────────────────────────

    /// <summary>
    /// Does <paramref name="selector"/> match <paramref name="box"/>? Asked through the ordinary
    /// author-rule lookup, so the rule index and the matcher both run exactly as they do in a cascade.
    /// </summary>
    private static bool Matches(CssBox box, string selector)
    {
        var cssData = new CssParser(new MockAdapter()).ParseStyleSheet($"{selector} {{ color: red; }}", false);
        return cssData.GetAuthorStyleRules(MediaQueryContext.TypeOnly("print"), box).Any();
    }

    private static string Html(string css, string body) =>
        $"<!DOCTYPE html><html><head><style>{css}</style></head><body>{body}</body></html>";

    private static CssBox Box(string html, string id)
    {
        var root = BuildRoot(html);
        var box = LayoutHarness.FindById(root, id);
        Assert.IsNotNull(box);
        return box!;
    }

    private static CssBox BuildRoot(string html) => LayoutHarness.Layout(html).Root;
}
