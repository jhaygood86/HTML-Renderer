using HtmlRenderer.IntegrationTest.TestSupport;

namespace HtmlRenderer.IntegrationTest.Text;

/// <summary>
/// Regression coverage for a <c>&lt;style&gt;</c> element whose CSS contains a <c>&lt;</c> character (e.g. a
/// <c>content: "&lt;"</c>-style value). Ported from PeachPDF's <c>StyleElementTextConcatenationTests</c>, which
/// exists because PeachPDF's HTML tokenizer splits such raw text into several data tokens at each <c>&lt;</c>,
/// and PeachPDF's <c>DomParser</c> used to parse each fragment as an independent (possibly syntactically
/// incomplete) stylesheet instead of concatenating them first - breaking any rule after the split point.
/// </summary>
/// <remarks>
/// <para>
/// <b>The same split premise IS real here too.</b> This fork's <c>DomParser.CascadeParseStyles</c>
/// (<c>Core/Parse/DomParser.cs</c>, ~line 118-123) parses a <c>&lt;style&gt;</c> element's child text nodes
/// independently in a <c>foreach</c> loop - <c>_cssParser.ParseStyleSheet(cssData, child.Text)</c> - with no
/// concatenation, exactly like PeachPDF's bug. And this fork's HTML tokenizer (HtmlKit's <c>HtmlTokenizer</c>,
/// used by <c>HtmlParser.ParseDocument</c>) DOES split a <c>&lt;style&gt;</c> element's raw text into multiple
/// data tokens at an embedded <c>&lt;</c> - confirmed by direct execution of <c>HtmlTokenizer</c> against the
/// exact markup below: it yields two separate <c>Data</c> tokens split right at the <c>&lt;</c> inside
/// <c>"&lt;"</c>, becoming two separate anonymous child <see cref="TheArtOfDev.HtmlRenderer.Core.Dom.CssBox"/>
/// text nodes under the <c>&lt;style&gt;</c> box.
/// </para>
/// <para>
/// <b>But the bug does not reproduce here.</b> Confirmed by direct execution of the full <c>SetHtml</c>/layout
/// pipeline against the built assembly: <c>#b</c>'s rule still applies. The reason is this fork's
/// <c>CssParser.ParseStyleBlocks</c> (<c>Core/Parse/CssParser.cs</c>) is not a sequential/stateful CSS
/// tokenizer the way PeachPDF's is - it is a simple brace-matching scanner that walks a fragment looking for
/// the next <c>{</c>...<c>}</c> pair, treating any stray <c>}</c> it meets before finding a <c>{</c> as noise
/// to skip past rather than a parse failure. The second fragment here (<c>"&lt;"; }\n #b { color: green; }"</c>)
/// has exactly that shape: the leading <c>&lt;"; }</c> is garbage the scanner skips over, and it still finds
/// and correctly parses the well-formed <c>#b { color: green; }</c> block that follows. So although the
/// underlying "no concatenation" premise is confirmed real in this fork, this specific regression scenario is
/// not actually broken by it, because the two engines fail differently: PeachPDF needs the full stylesheet
/// text to parse a rule; this fork only needs a self-contained <c>{ }</c> block, which the split still leaves
/// intact.
/// </para>
/// </remarks>
// This fork's CssParser keeps a process-wide, non-thread-safe regex cache
// (RegexParserUtils.GetRegex's static Dictionary) that SetHtml/DefaultCssData populate lazily on first use per
// AppDomain; running HtmlContainerInt.SetHtml from more than one thread at once (as MSTestSettings.cs's
// assembly-wide [Parallelize(Scope = ExecutionScope.MethodLevel)] does by default) can corrupt it and throw
// "A concurrent update was performed on this collection". [DoNotParallelize] avoids tripping that pre-existing
// library race rather than masking it.
[DoNotParallelize]
[TestClass]
public sealed class StyleElementTextConcatenationTests
{
    [TestMethod]
    public void RuleAfterLessThanInDeclaration_StillApplies()
    {
        const string html = """
            <html><head><style>
              #a { --x: "<"; }
              #b { color: green; }
            </style></head><body style='margin:0'>
            <div id="a">a</div><div id="b">b</div>
            </body></html>
            """;

        var (root, _) = LayoutHarness.Layout(html);
        var b = LayoutHarness.FindById(root, "b")!;

        Assert.IsNotNull(b);
        // Without concatenation, "#b { color: green }" would need to land in a mis-parsed fragment starting
        // at '<' and never apply, leaving the default "black" - but this fork's brace-matching CssParser
        // recovers from the leading garbage and still finds the well-formed block (see class remarks).
        Assert.AreEqual("green", b.Color);
    }
}
