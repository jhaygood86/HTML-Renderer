using System;

namespace TheArtOfDev.HtmlRenderer.Core.Utils
{
    /// <summary>
    /// Resolves a box's cascaded <c>font-weight</c> value (<c>normal</c>/<c>bold</c>/<c>bolder</c>/
    /// <c>lighter</c>/a number) to a concrete CSS Fonts Level 4 numeric weight (1-1000), for
    /// <see cref="Dom.CssBoxProperties.ActualFont"/>'s face-matching call into
    /// <see cref="Handlers.FontsHandler.GetCachedFont(string,double,Adapters.Entities.RFontStyle,int,int,int?)"/>.
    /// </summary>
    internal static class CssFontWeightResolver
    {
        internal const int Normal = 400;
        internal const int Bold = 700;

        /// <summary>
        /// <c>bolder</c>/<c>lighter</c> are relative to the parent's resolved weight; true CSS2.1 defines
        /// this via a lookup table over 9 named weight steps, not a fixed offset. A flat +/-300 clamped to
        /// [1,1000] is a real, explicitly-simplified approximation of that table - close enough to move a
        /// normal-weight (400) parent to bold (700) and back, without reproducing the full step table for
        /// a CSS feature real-world content rarely nests more than one level deep.
        /// </summary>
        private const int RelativeStep = 300;

        internal static int Resolve(string fontWeightValue, int parentWeight)
        {
            if (string.IsNullOrWhiteSpace(fontWeightValue))
                return Normal;

            switch (fontWeightValue)
            {
                case CssConstants.Normal:
                case CssConstants.Inherit:
                    return Normal;
                case CssConstants.Bold:
                    return Bold;
                case CssConstants.Bolder:
                    return Clamp(parentWeight + RelativeStep);
                case CssConstants.Lighter:
                    return Clamp(parentWeight - RelativeStep);
                default:
                    return int.TryParse(fontWeightValue, out var numeric) ? Clamp(numeric) : Normal;
            }
        }

        private static int Clamp(int weight) => Math.Max(1, Math.Min(1000, weight));
    }
}
