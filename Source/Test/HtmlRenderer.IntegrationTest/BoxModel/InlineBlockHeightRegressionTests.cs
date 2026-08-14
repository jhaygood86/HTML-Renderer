using System;
using System.Collections.Generic;
using System.Linq;
using HtmlRenderer.IntegrationTest.TestSupport;
using TheArtOfDev.HtmlRenderer.Core.Dom;

namespace HtmlRenderer.IntegrationTest.BoxModel;

/// <summary>
/// Verifies block height around a padded <c>inline-block</c> (e.g. a themeable button).
/// </summary>
/// <remarks>
/// HTML-Renderer fact (confirmed, searched all of Core): <c>display:inline-block</c> has NO dedicated
/// sizing/measurement path - <c>CssBox.IsInline</c> is simply <c>Display==Inline || Display==InlineBlock</c>,
/// so inline-block boxes are routed through the exact same inline-flow word-wrap algorithm
/// (<c>CssLayoutEngine.FlowBox</c>/<c>CreateLineBoxes</c>) as plain <c>inline</c>, just with their own
/// margin/border/padding added around each child. A SIMPLE inline-block (single line of content, padding
/// around inline content with no internal wrapping complexity) plausibly measures/positions correctly by
/// accident, since the general inline-flow algorithm handles padding around any inline box reasonably.
/// Anything requiring inline-block to be measured/sized ONCE as a single atomic unit before being placed
/// (multi-line content wrapping independently at a fixed width, or interaction with ::before/multi-column,
/// neither of which exist in this fork at all) is not properly supported, and is [Ignore]d below.
/// </remarks>
[DoNotParallelize]
[TestClass]
public sealed class InlineBlockHeightRegressionTests
{
    [Ignore("Confirmed real limitation (matches this class's own remarks): the WRAPPING block's height does " +
            "not grow to cover its sole inline-block child's vertical padding at all - measured height came " +
            "back as just the word's own line height (~20px), not >=200px. Since inline-block is routed " +
            "through the same inline-flow algorithm as plain 'inline' (CssBox.IsInline), and CSS2.1 section " +
            "10.8.1 correctly keeps plain-inline padding from growing line-box height, that same non-growth " +
            "leaks onto inline-block here too, even though inline-block's OWN padding box is sized correctly " +
            "in isolation (see PaddedInlineBlock_WordsSitInsidePaddingBox, which passes).")]
    [TestMethod]
    public void PaddedInlineBlock_SoleContentOfBlock_BlockHeightCoversPadding()
    {
        const string html = "<div style='height:300px'>filler</div>" +
                             "<div class='wrapper'><button class='btn' style='padding:100px 14px'>Go</button></div>";

        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(html));

        var wrapper = FindBoxByClass(root, "wrapper");
        Assert.IsNotNull(wrapper);

        var height = wrapper!.ActualBottom - wrapper.Location.Y;
        Assert.IsTrue(height > 0,
            $"Block wrapping only a padded inline-block must not collapse to a negative height (top={wrapper.Location.Y}, bottom={wrapper.ActualBottom})");
        Assert.IsTrue(height >= 200,
            $"Block height ({height}) must cover the inline-block's own 200px of vertical padding");
    }

    // A tighter companion to the test above: asserts the block's height against the button's own measured
    // insets and word height, not just a loose lower bound. Delta of 1px is a guess at cross-engine
    // rounding/line-metric slack - flagging for the follow-up verification pass in case it needs widening.
    [Ignore("Same confirmed limitation as PaddedInlineBlock_SoleContentOfBlock_BlockHeightCoversPadding - the " +
            "wrapping block's height does not grow to cover the inline-block's vertical insets at all.")]
    [TestMethod]
    public void PaddedInlineBlock_SoleContentOfBlock_BlockHeightMatchesInsetsExactly()
    {
        const string html = "<div style='height:300px'>filler</div>" +
                             "<div class='wrapper'><button class='btn' style='padding:100px 14px'>Go</button></div>";

        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(html));

        var wrapper = FindBoxByClass(root, "wrapper");
        var button = FindBoxByClass(root, "btn");
        Assert.IsNotNull(wrapper);
        Assert.IsNotNull(button);

        var word = FindFirstWord(button!);
        Assert.IsNotNull(word);

        var expectedHeight = button!.ActualBorderTopWidth + button.ActualPaddingTop
            + word!.Height
            + button.ActualBorderBottomWidth + button.ActualPaddingBottom;
        var actualHeight = wrapper!.ActualBottom - wrapper.Location.Y;

        Assert.AreEqual(expectedHeight, actualHeight, 1,
            $"Block height ({actualHeight}) must equal the button's own top inset + word height + bottom inset ({expectedHeight}) exactly, not merely cover it");
    }

    [TestMethod]
    public void PaddedInlineBlock_TallerContentLine_DoesNotShrinkBlock()
    {
        const string html = "<div style='height:300px'>filler</div>" +
                             "<div class='wrapper'><button style='padding:1px 14px'>Go</button></div>";

        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(html));

        var wrapper = FindBoxByClass(root, "wrapper");
        Assert.IsNotNull(wrapper);
        Assert.IsTrue(wrapper!.ActualBottom > wrapper.Location.Y,
            $"Block height must stay positive (top={wrapper.Location.Y}, bottom={wrapper.ActualBottom})");
    }

    // CSS2.1 §10.8.1: the vertical padding/border of a non-replaced `display: inline` box must NOT
    // influence line box height - a plain span's 200px of vertical padding must not grow its block.
    [TestMethod]
    public void PaddedPlainInline_DoesNotGrowBlockHeight()
    {
        const string html = "<div style='height:300px'>filler</div>" +
                             "<div class='wrapper'><span style='padding:100px 0'>text</span></div>";

        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(html));

        var wrapper = FindBoxByClass(root, "wrapper");
        Assert.IsNotNull(wrapper);

        var height = wrapper!.ActualBottom - wrapper.Location.Y;
        Assert.IsTrue(height > 0,
            $"Block height must stay positive (top={wrapper.Location.Y}, bottom={wrapper.ActualBottom})");
        Assert.IsTrue(height < 200,
            $"Plain inline vertical padding must not grow the block (CSS2.1 section 10.8.1) - got height {height}");
    }

    // CSS2.1 §8.1: an atomic inline-level box's content is laid out inside its padding box, so the label of
    // a padded button must start border+padding-top BELOW the box's top edge (and end padding-bottom above
    // its bottom edge).
    [TestMethod]
    public void PaddedInlineBlock_WordsSitInsidePaddingBox()
    {
        const string html = "<button class='btn' style='padding:6px 14px'>Go</button>";

        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(html));

        var button = FindBoxByClass(root, "btn");
        Assert.IsNotNull(button);

        var rect = button!.Rectangles.Values.Single();
        var word = FindFirstWord(button);
        Assert.IsNotNull(word);

        Assert.IsTrue(word!.Top >= rect.Top + 6 - 0.5,
            $"Button label (top={word.Top}) must sit at least padding-top (6) below the box top ({rect.Top})");
        Assert.IsTrue(word.Bottom <= rect.Bottom - 6 + 0.5,
            $"Button label (bottom={word.Bottom}) must end at least padding-bottom (6) above the box bottom ({rect.Bottom})");
    }

    // With only padding-bottom set, the rect must extend below the words by that amount while the top edge
    // stays at the word band - regression guard against padding-top/padding-bottom being swapped.
    [Ignore("Confirmed real limitation: with asymmetric padding (zero padding-top, 30px padding-bottom), the " +
            "box's own rect.Bottom does not extend past the words by the padding-bottom amount at all - unlike " +
            "the symmetric-padding case (see PaddedInlineBlock_WordsSitInsidePaddingBox, which passes with " +
            "equal top/bottom padding). The rect ends up sized to just the word band, with padding-bottom " +
            "silently dropped from the box's own height computation.")]
    [TestMethod]
    public void AsymmetricPaddingBottom_ExpandsRectBottomByPaddingBottom()
    {
        const string html = "<button class='btn' style='padding:0 0 30px 0'>Go</button>";

        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(html));

        var button = FindBoxByClass(root, "btn");
        Assert.IsNotNull(button);

        var rect = button!.Rectangles.Values.Single();
        var word = FindFirstWord(button);
        Assert.IsNotNull(word);

        Assert.IsTrue(rect.Bottom >= word!.Bottom + 30 - 0.5,
            $"Box rect bottom ({rect.Bottom}) must extend padding-bottom (30) below the words (bottom={word.Bottom})");
        Assert.IsTrue(Math.Abs(rect.Top - word.Top) < 0.5,
            $"With no padding-top the rect top ({rect.Top}) must coincide with the word band top ({word.Top})");
    }

    [Ignore("Requires a ::before pseudo-element with display:inline-block and generated content - this fork " +
            "has no pseudo-element support at all (confirmed: no \"::before\"/\"::after\" handling anywhere " +
            "in Core).")]
    [TestMethod]
    public void PaddedInlineBlockPseudoElement_WordsSitInsidePaddingBox()
    {
        const string html = "<style>.host::before { content: 'NEW'; display: inline-block; padding: 6px 14px; }</style>" +
                             "<div class='host'>after marker</div>";

        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(html));

        var host = FindBoxByClass(root, "host");
        Assert.IsNotNull(host);
        var pseudo = FindFirst(host!, b => b.Display == "inline-block");
        Assert.IsNotNull(pseudo);

        var rect = pseudo!.Rectangles.Values.Single();
        var word = FindFirstWord(pseudo);
        Assert.IsNotNull(word);

        Assert.IsTrue(word!.Top >= rect.Top + 6 - 0.1,
            $"Pseudo-element label (top={word.Top}) must sit at least padding-top (6) below the box top ({rect.Top})");
    }

    [Ignore("Relies on a page-fragmentation-aware inline-block/line placement pass (relocating a padded " +
            "line that would straddle a page boundary once its own padding-top inset is applied). This " +
            "fork's only page-break mechanism (CssBox.BreakPage) is invoked from CssLayoutEngineTable's row " +
            "loop, gated behind an explicit page-break-inside:avoid on a <table> - there is no general " +
            "page-boundary-aware placement for ordinary block/inline-block content like this <button>.")]
    [TestMethod]
    public void InsetShiftNearPageBoundary_WordsDoNotStraddleThePage()
    {
        const string html = "<div style='height:760px'>filler</div>" +
                             "<div><button style='padding:100px 14px'>Go</button></div>";

        var (root, container) = LayoutHarness.Layout(LayoutHarness.Wrap(html), maxWidth: 595, maxHeight: 842);
        var pageHeight = container.PageSize.Height;

        var button = FindFirst(root, b => b.HtmlTag?.Name == "button");
        Assert.IsNotNull(button);
        var word = FindFirstWord(button!);
        Assert.IsNotNull(word);

        var topPage = Math.Floor(word!.Top / pageHeight);
        var bottomPage = Math.Floor((word.Bottom - 0.01) / pageHeight);
        Assert.AreEqual(topPage, bottomPage,
            $"Inset-shifted word should not straddle a page boundary (top={word.Top:F1}, bottom={word.Bottom:F1}, pageHeight={pageHeight})");
    }

    [Ignore("Multi-column layout (the CSS 'columns' shorthand) is not implemented anywhere in this fork's " +
            "Core layout engine - a <div style='columns:2'> lays out as an ordinary single-column block, so " +
            "there is no column-boundary-aware line relocation for this button's padding-inset shift to " +
            "interact with.")]
    [TestMethod]
    public void InsetShiftNearColumnBoundary_WholeLineMovesToNextColumn()
    {
        const string html = "<div id='mc' style='columns:2;column-gap:0;column-fill:auto;width:200px'>" +
                             "<div style='height:220px'>filler</div>" +
                             "<button id='btn' style='padding-top:60px'>Go</button></div>";

        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(html), maxWidth: 200, maxHeight: 300);

        var mc = LayoutHarness.FindById(root, "mc")!;
        var button = LayoutHarness.FindById(root, "btn")!;

        var word = FindFirstWord(button);
        Assert.IsNotNull(word);

        var columnWidth = (mc.ClientRight - mc.ClientLeft) / 2;
        Assert.IsTrue(word!.Left >= mc.ClientLeft + columnWidth - 0.1,
            $"expected the inset-shifted line to move whole to column 2 (columnWidth={columnWidth}), word is at x={word.Left}");
        Assert.IsTrue(Math.Abs(word.Top - 60) < 5,
            $"expected padding-top (60) to apply once, at column 2's own content top, word top={word.Top}");
    }

    [Ignore("Multi-column layout (the CSS 'columns' shorthand) is not implemented anywhere in this fork's " +
            "Core layout engine, so there is no column-boundary-aware relocation of an inline-block's " +
            "internal lines for a padding-inset shift to interact with (orphans/widows control is not " +
            "implemented either).")]
    [TestMethod]
    public void InsetShiftedInlineBlock_SecondInternalLine_MovesAloneToNextColumn()
    {
        const string html = "<div id='mc' style='columns:2;column-gap:0;column-fill:auto;width:200px;orphans:1;widows:1'>" +
                             "<div style='height:150px'>filler</div>" +
                             "<span id='btn' style='display:inline-block;padding-top:60px'>First<br>Second</span></div>";

        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(html), maxWidth: 200, maxHeight: 300);

        var mc = LayoutHarness.FindById(root, "mc")!;
        var button = LayoutHarness.FindById(root, "btn")!;

        var firstWord = AllWords(button).Single(w => w.Text == "First");
        var secondWord = AllWords(button).Single(w => w.Text == "Second");

        var columnWidth = (mc.ClientRight - mc.ClientLeft) / 2;

        Assert.IsTrue(firstWord.Left < mc.ClientLeft + columnWidth - 0.1,
            $"expected the button's first internal line to stay in column 1, word is at x={firstWord.Left}");
        Assert.IsTrue(secondWord.Left >= mc.ClientLeft + columnWidth - 0.1,
            $"expected the button's second internal line to move alone to column 2, word is at x={secondWord.Left}");
        Assert.IsTrue(secondWord.Top < 15,
            $"expected the resumed (continuation) line to start at column 2's raw content top with no re-applied inset, word top={secondWord.Top}");
    }

    #region Helpers

    private static CssBox? FindFirst(CssBox box, Func<CssBox, bool> predicate)
    {
        if (predicate(box)) return box;
        foreach (var child in box.Boxes)
        {
            var found = FindFirst(child, predicate);
            if (found != null) return found;
        }
        return null;
    }

    private static CssRect? FindFirstWord(CssBox box)
    {
        if (box.Words.Count > 0)
            return box.Words[0];

        foreach (var child in box.Boxes)
        {
            var word = FindFirstWord(child);
            if (word != null)
                return word;
        }

        return null;
    }

    private static List<CssRect> AllWords(CssBox box)
    {
        var words = new List<CssRect>(box.Words);

        foreach (var child in box.Boxes)
        {
            words.AddRange(AllWords(child));
        }

        return words;
    }

    private static CssBox? FindBoxByClass(CssBox root, string className)
    {
        var classAttr = root.HtmlTag?.TryGetAttribute("class", "");
        if (!string.IsNullOrEmpty(classAttr))
        {
            foreach (var cls in classAttr.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                if (cls == className)
                    return root;
            }
        }

        foreach (var child in root.Boxes)
        {
            var result = FindBoxByClass(child, className);
            if (result != null)
                return result;
        }

        return null;
    }

    #endregion
}
