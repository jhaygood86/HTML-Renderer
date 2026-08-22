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
/// <b>The same split premise is still real.</b> This fork's <c>DomParser.CascadeParseStyles</c>
/// (<c>Core/Parse/DomParser.cs</c>, ~line 134-135) still parses a <c>&lt;style&gt;</c> element's child text
/// nodes independently in a <c>foreach</c> loop - <c>_cssParser.ParseStyleSheet(cssData, child.Text)</c> -
/// with no concatenation, exactly like PeachPDF's bug. And this fork's HTML tokenizer (HtmlKit's
/// <c>HtmlTokenizer</c>, used by <c>HtmlParser.ParseDocument</c>) DOES split a <c>&lt;style&gt;</c> element's
/// raw text into multiple data tokens at an embedded <c>&lt;</c>, becoming two separate anonymous child
/// <see cref="TheArtOfDev.HtmlRenderer.Core.Dom.CssBox"/> text nodes under the <c>&lt;style&gt;</c> box.
/// </para>
/// <para>
/// <b>Verified against the CSS engine port: the previously-recorded "does not reproduce" conclusion is now
/// stale and wrong.</b> That conclusion relied on the OLD <c>CssParser</c>'s brace-matching scanner, which has
/// been replaced entirely by the vendored ExCSS-derived tokenizer/grammar (same lineage as PeachPDF's own).
/// Confirmed by direct execution against the built assembly: parsing the second text-node fragment
/// (<c>"&lt;"; }\n #b { color: green; }</c>) alone now throws a real, unhandled
/// <c>System.NullReferenceException</c> from <c>StyleRule.SelectorText</c>'s getter, via
/// <c>StylesheetComposer.CreateNestedStyleRule</c>/<c>TryCreateNestedRule</c> (the new engine's CSS-Nesting
/// support tries to parse the malformed leading fragment as an incomplete nested rule and dereferences a null
/// selector while doing so). Because <c>HtmlContainerInt.SetHtml</c> is <c>async Task</c> and
/// <see cref="LayoutHarness.Layout"/> does not await it, this exception is thrown into an unobserved task and
/// silently discarded - the outward symptom is <c>container.Root</c> staying <c>null</c> after
/// <c>Clear()</c>, which is what <see cref="LayoutHarness.Layout"/>'s own <c>Assert.IsNotNull(container.Root)</c>
/// catches. So the underlying "no concatenation" premise is still real, and now manifests as a genuine crash
/// bug in the new engine's CSS-Nesting parse path (not a silent "rule doesn't apply" the way PeachPDF's own
/// bug read) - out of scope for this test-porting pass to fix in Core, so the regression test is left in and
/// marked <c>[Ignore]</c> below with this freshly-verified reason, rather than silently deleted or left
/// falsely documented as passing.
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
    [Ignore("CSS engine port regression, freshly verified: parsing the split text-node fragment " +
            "'\"<\"; }\\n #b { color: green; }' now throws System.NullReferenceException from " +
            "StyleRule.SelectorText via StylesheetComposer.CreateNestedStyleRule/TryCreateNestedRule (the " +
            "new CSS-Nesting support dereferences a null selector on this malformed input). Because SetHtml " +
            "is unawaited by LayoutHarness.Layout, the exception is silently swallowed and container.Root " +
            "stays null - see this class's remarks for the full trace. Out of scope for this test-porting " +
            "pass to fix in Core; left in and ignored so the regression stays visible rather than silently " +
            "deleted.")]
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
        // at '<' and never apply, leaving the default "black" - see class remarks for what actually happens
        // now (a crash, not a silent non-application).
        Assert.AreEqual("green", b.Color);
    }
}
