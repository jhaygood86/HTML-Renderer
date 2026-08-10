namespace TheArtOfDev.HtmlRenderer.Core.Utils
{
    /// <summary>
    /// Resolves a CSS <c>font-stretch</c> keyword to the 1-9 numeric scale matching the OpenType OS/2
    /// table's <c>usWidthClass</c> field directly (1=ultra-condensed ... 5=normal ... 9=ultra-expanded) -
    /// the same scale a ported <c>TtfFontDescription</c>'s <c>Stretch</c> reads from that field for each
    /// registered face, so the two are directly comparable without any extra translation.
    /// </summary>
    internal static class FontStretchResolver
    {
        internal const int Normal = 5;

        /// <summary>
        /// Resolves a raw <c>font-stretch</c> keyword (a box's cascaded value, or an <c>@font-face</c>
        /// descriptor - see <see cref="FontFaceDescriptorResolver.ResolveStretch"/>) to the 1-9 scale.
        /// Unlike PeachPDF's own version of this resolver, there's only this one string-keyword overload -
        /// HTML-Renderer's CSS engine works with raw cascaded strings throughout (no separate strongly-typed
        /// CSS-OM enum for font-stretch the way PeachPDF's does), so a second typed-enum overload would have
        /// no caller.
        /// </summary>
        internal static int Resolve(string fontStretchValue) => fontStretchValue switch
        {
            CssConstants.UltraCondensed => 1,
            CssConstants.ExtraCondensed => 2,
            CssConstants.Condensed => 3,
            CssConstants.SemiCondensed => 4,
            CssConstants.SemiExpanded => 6,
            CssConstants.Expanded => 7,
            CssConstants.ExtraExpanded => 8,
            CssConstants.UltraExpanded => 9,
            _ => Normal
        };
    }
}
