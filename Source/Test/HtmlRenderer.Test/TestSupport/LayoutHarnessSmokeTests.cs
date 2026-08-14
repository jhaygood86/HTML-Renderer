using HtmlRenderer.Test.TestSupport;

namespace HtmlRenderer.Test.TestSupportTests;

[TestClass]
public sealed class LayoutHarnessSmokeTests
{
    [TestMethod]
    public void Layout_SimpleParagraph_ProducesBoxWithId()
    {
        var (root, _) = LayoutHarness.Layout(LayoutHarness.Wrap("<p id='target'>hello world</p>"));

        var target = LayoutHarness.FindById(root, "target");

        Assert.IsNotNull(target);
        Assert.IsTrue(target.ActualRight > target.Location.X);
    }
}
