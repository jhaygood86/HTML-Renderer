using HtmlRenderer.IntegrationTest.TestSupport;

namespace HtmlRenderer.IntegrationTest.BoxModel;

/// <summary>
/// <see href="https://www.w3.org/TR/CSS21/box.html#collapsing-margins">CSS 2.1 §8.3.1</see>'s adjoining-margin
/// set, resolved at one break point by the frame the boxes are children of.
/// </summary>
/// <remarks>
/// <para>
/// The set spans two frames, and these pin both halves: what precedes a box is the frame's to see (a
/// predecessor's bottom margin, and everything a run of self-collapsing predecessors folds in), while the box's
/// own top margin and the first-in-flow-child chain adjoining it are a walk into its own subtree. §8.3.1
/// collapses the whole set at once, so the two are folded together rather than resolved separately.
/// </para>
/// <para>
/// This fork's margin-collapse machinery (<c>CssBox.MarginTopCollapse</c>/<c>MarginBottomCollapse</c>,
/// Dom/CssBox.cs ~1103-1183) only ever resolves a <i>pairwise</i> <c>Math.Max</c> between exactly two margins at
/// a time - never the full CSS2.1 "adjoining margin set" (which can span an arbitrary run of self-collapsing
/// boxes and nested first-in-flow children, and which spec requires collapsing to
/// <c>max(positives) + min(negatives)</c>, not <c>Math.Max</c> alone). <c>Math.Max</c> on two same-signed margins
/// happens to equal the spec result by coincidence, but it diverges the moment the set has to look past a single
/// pair - whether because a negative margin is mixed in, or because more than two margins are actually
/// adjoining. See each test's own <see cref="IgnoreAttribute"/> for the specific way it trips over this.
/// </para>
/// </remarks>
[DoNotParallelize]
[TestClass]
public sealed class CollapsedMarginBeforeTests
{
    private const double Delta = 0.5;

    [TestMethod]
    public void AdjoiningSiblingMargins_CollapseToTheLargerPositive()
    {
        // Same-sign case: Math.Max(30, 10) = 30 happens to equal the spec result (max of the positives), so this
        // one genuinely passes on this fork.
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='a' style='height:40px;margin-bottom:30px'></div>"
            + "<div id='b' style='height:40px;margin-top:10px'></div>"));

        var a = LayoutHarness.FindById(root, "a")!;
        var b = LayoutHarness.FindById(root, "b")!;

        Assert.AreEqual(30, b.Location.Y - a.ActualBottom, Delta);
    }

    [Ignore("Confirmed real bug, not a porting mistake: CssBox.MarginTopCollapse's sibling branch (Dom/CssBox.cs " +
            "~line 1108) computes `Math.Max(prevSibling.ActualMarginBottom, ActualMarginTop)` for the gap between " +
            "two in-flow siblings - for a positive/negative mixed pair (30, -10) that picks 30 (the larger " +
            "value, treating the negative margin as if it weren't there), instead of CSS2.1's mixed-sign rule of " +
            "summing the largest positive and the most-negative value (30 + (-10) = 20). Confirmed: " +
            "b.Location.Y - a.ActualBottom is 30 on this fork, not the spec-correct 20.")]
    [TestMethod]
    public void MixedSignSiblingMargins_Sum()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='a' style='height:40px;margin-bottom:30px'></div>"
            + "<div id='b' style='height:40px;margin-top:-10px'></div>"));

        var a = LayoutHarness.FindById(root, "a")!;
        var b = LayoutHarness.FindById(root, "b")!;

        Assert.AreEqual(20, b.Location.Y - a.ActualBottom, Delta);
    }

    [Ignore("Confirmed real bug, not a porting mistake: because MarginTopCollapse only ever looks at the single " +
            "immediate previous sibling (Dom/CssBox.cs ~1103-1127), it cannot treat a self-collapsing empty box " +
            "as transparent for a single 3-way adjoining set the way CSS2.1 requires. Instead it resolves two " +
            "SEPARATE pairwise gaps in series - a-to-middle: Math.Max(a.marginBottom=10, middle.marginTop=40)=40, " +
            "then middle-to-b: Math.Max(middle.marginBottom=40, b.marginTop=10)=40 - and since the empty middle " +
            "box has zero height (its own margin-bottom is never folded into its own ActualBottom), those two " +
            "40px gaps stack in series instead of collapsing to one shared 40px value. Confirmed: " +
            "b.Location.Y - a.ActualBottom is 80 on this fork (40+40), not the spec-correct 40.")]
    [TestMethod]
    public void ASelfCollapsingSiblingBetweenTwoBoxes_JoinsTheSet()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='a' style='height:40px;margin-bottom:10px'></div>"
            + "<div style='margin:40px 0'></div>"
            + "<div id='b' style='height:40px;margin-top:10px'></div>"));

        var a = LayoutHarness.FindById(root, "a")!;
        var b = LayoutHarness.FindById(root, "b")!;

        // The whole run should collapse to one value - the largest member, not a sum of the pairs.
        Assert.AreEqual(40, b.Location.Y - a.ActualBottom, Delta);
    }

    [Ignore("Confirmed real bug, not a porting mistake: MarginTopCollapse's parent-child branch (Dom/CssBox.cs " +
            "~1111-1114) is `Math.Max(0, ActualMarginTop - Math.Max(parent.ActualMarginTop, parent.CollapsedMarginTop))` " +
            "- an approximation that does not implement 'the whole chain collapses to one value, taken once' the " +
            "way CSS2.1 requires. For outer(margin-top:10)/inner(margin-top:30) here: outer's own placement " +
            "against 'a' only applies outer's OWN 10px margin (Math.Max(a.marginBottom=0, outer.marginTop=10)=10, " +
            "not the spec-correct 30 which should reflect the whole outer+inner set), and inner then gets pushed " +
            "an ADDITIONAL Math.Max(0, 30-Math.Max(10,10))=20px below outer's own top - so inner ends up 20px " +
            "below outer (not flush with it), and outer is only 10px below 'a' (not 30px). Confirmed on this " +
            "fork: outer.Location.Y - a.ActualBottom is 10 (not 30), and inner.Location.Y - outer.Location.Y is " +
            "20 (not 0).")]
    [TestMethod]
    public void AFirstInFlowChildsTopMargin_JoinsItsParentsOwnSet()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='a' style='height:40px'></div>"
            + "<div id='outer' style='margin-top:10px'>"
            + "<div id='inner' style='height:40px;margin-top:30px'></div></div>"));

        var a = LayoutHarness.FindById(root, "a")!;
        var outer = LayoutHarness.FindById(root, "outer")!;
        var inner = LayoutHarness.FindById(root, "inner")!;

        // One collapsed value for the whole chain, taken once: the outer box should move down by it and the
        // inner box should sit at its parent's content top rather than being pushed down again.
        Assert.AreEqual(30, outer.Location.Y - a.ActualBottom, Delta);
        Assert.AreEqual(outer.Location.Y, inner.Location.Y, Delta);
    }

    [Ignore("Confirmed real bug, not a porting mistake: the parent-child join's guard condition (Dom/CssBox.cs " +
            "~1111) only checks `ActualPaddingTop < 0.1 && ActualPaddingBottom < 0.1` on the box and its parent - " +
            "it never checks BORDER width at all, even though CSS2.1 requires ANY border or padding between " +
            "parent and child to block the join, not just padding. So outer's border-top:1px here fails to block " +
            "the outer/inner join: inner still gets pulled into the same Math.Max(0, inner.marginTop=30 - " +
            "Math.Max(outer.marginTop=10, outer.CollapsedMarginTop=10))=20px-below-outer's-ClientTop computation " +
            "as the no-border case, landing it 1px (border) + 20px = 21px below outer, not the spec-correct " +
            "1px + 30px = 31px where the border fully blocks the join and both margins stand independently. (The " +
            "outer-vs-'a' gap of 10px does coincidentally match here, since that part never depended on the " +
            "border in the first place.)")]
    [TestMethod]
    public void ABorderOnTheParent_BlocksTheChainAndLeavesBothMarginsStanding()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='a' style='height:40px'></div>"
            + "<div id='outer' style='margin-top:10px;border-top:1px solid black'>"
            + "<div id='inner' style='height:40px;margin-top:30px'></div></div>"));

        var a = LayoutHarness.FindById(root, "a")!;
        var outer = LayoutHarness.FindById(root, "outer")!;
        var inner = LayoutHarness.FindById(root, "inner")!;

        Assert.AreEqual(10, outer.Location.Y - a.ActualBottom, Delta);
        Assert.AreEqual(31, inner.Location.Y - outer.Location.Y, Delta);
    }

    [Ignore("Confirmed real bug, not a porting mistake, compounding two gaps: (1) this fork's block-placement " +
            "seam does not special-case Position=float the way it does display:none/absolute/fixed siblings (the " +
            "same gap BoxModel.BlockPlacementSeamTests.AFloatedSibling_IsNotThePredecessorTheFrameResolvesAgainst " +
            "documents) - so the floated 'f' box is positioned via the exact same in-flow " +
            "top = prevSibling.ActualBottom + MarginTopCollapse(prevSibling) formula as any ordinary block, " +
            "instead of being excluded from margin collapsing entirely per CSS2.1 (a float's margin should never " +
            "adjoin anything). (2) That collapse is then itself the mixed-sign Math.Max bug this class's remarks " +
            "describe: Math.Max(a.marginBottom=20, f.marginTop=-5)=20, not the spec-correct sum " +
            "20+(-5)=15. Confirmed: f.Location.Y - a.ActualBottom is 20 on this fork, not 15.")]
    [TestMethod]
    public void AFloatsOwnMarginDoesNotMerge_ButThePredecessorsStillOccupiesSpace()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap(
            "<div id='a' style='height:40px;margin-bottom:20px'></div>"
            + "<div id='f' style='float:left;width:40px;height:40px;margin-top:-5px'></div>"));

        var a = LayoutHarness.FindById(root, "a")!;
        var f = LayoutHarness.FindById(root, "f")!;

        // Summed, not merged: a float is out of flow so its margin should never adjoin anything, but the
        // predecessor's trailing margin is real space it must sit after.
        Assert.AreEqual(15, f.Location.Y - a.ActualBottom, Delta);
    }
}
