#nullable enable

namespace TheArtOfDev.HtmlRenderer.Core.Entities
{
    /// <summary>
    /// One comma-separated candidate of an <c>@font-face</c> <c>src</c> descriptor, e.g. the
    /// <c>url(font.woff2) format("woff2")</c> in <c>src: url(font.woff2) format("woff2"), local("My Font")</c>.
    /// </summary>
    internal record CssFontFace(string? Url, string? Format, string? Tech, string? Local);
}
