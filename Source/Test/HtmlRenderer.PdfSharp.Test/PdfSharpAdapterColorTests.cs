using TheArtOfDev.HtmlRenderer.PdfSharp.Adapters;

namespace HtmlRenderer.PdfSharp.Test;

/// <summary>
/// Direct unit tests for <see cref="PdfSharpAdapter"/>'s named-color resolution
/// (<c>GetColorInt</c>), which maps a CSS/system color name to an <c>RColor</c> by
/// matching it against PdfSharp's known-color table (<c>XColorResourceManager</c>).
/// Guards that name lookup path against regression.
/// </summary>
[TestClass]
public sealed class PdfSharpAdapterColorTests
{
    [TestMethod]
    [DataRow("Red", (byte)255, (byte)0, (byte)0)]
    [DataRow("red", (byte)255, (byte)0, (byte)0)] // case-insensitive
    [DataRow("Lime", (byte)0, (byte)255, (byte)0)]
    [DataRow("Blue", (byte)0, (byte)0, (byte)255)]
    public void GetColor_KnownColorName_ResolvesToRgb(string name, byte r, byte g, byte b)
    {
        var adapter = PdfSharpAdapter.Instance;

        var color = adapter.GetColor(name);

        Assert.IsFalse(color.IsEmpty);
        Assert.AreEqual(r, color.R);
        Assert.AreEqual(g, color.G);
        Assert.AreEqual(b, color.B);
    }

    [TestMethod]
    public void GetColor_UnknownColorName_ReturnsEmpty()
    {
        var adapter = PdfSharpAdapter.Instance;

        var color = adapter.GetColor("not-a-real-color-name");

        Assert.IsTrue(color.IsEmpty);
    }
}
