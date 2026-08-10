#nullable enable

using System;

namespace TheArtOfDev.HtmlRenderer.Core.Utils
{
    /// <summary>
    /// Resolves an <c>@font-face</c> rule's own <c>font-weight</c>/<c>font-style</c>/<c>font-stretch</c>
    /// descriptor strings (<see cref="CssEngine.IFontFaceRule"/>) into the override values a registered
    /// face's matching entry takes - these are authoritative for how a specific registered face
    /// participates in matching, independent of what the font file's own internal tables say. Returns null
    /// for any descriptor this can't confidently resolve to a single concrete value (absent, or a
    /// variable-font weight/stretch *range* like <c>100 900</c>/<c>50% 200%</c> - real interpolated
    /// variable fonts are out of scope), so the caller falls back to the value sniffed from the file itself
    /// instead of silently forcing a wrong/arbitrary one.
    /// </summary>
    internal static class FontFaceDescriptorResolver
    {
        /// <summary>
        /// Resolves a <c>font-weight</c> descriptor (<c>normal</c>/<c>bold</c>/a number/absent) to a
        /// concrete CSS Fonts numeric weight. A two-token range (variable-font syntax) resolves to its
        /// lower bound as a reasonable single-value approximation, since there is no variable-font
        /// interpolation here. Any other multi-token or unparseable value returns null.
        /// </summary>
        internal static int? ResolveWeight(string? weightDescriptor)
        {
            if (string.IsNullOrWhiteSpace(weightDescriptor))
                return null;

            // See UnicodeRangeParser.Parse for why the ! is needed here on netstandard2.0.
            var tokens = weightDescriptor!.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length == 1 && int.TryParse(tokens[0], out var numeric))
                return numeric;
            if (tokens.Length == 1 && tokens[0] == CssConstants.Bold)
                return 700;
            if (tokens.Length == 1 && tokens[0] == CssConstants.Normal)
                return 400;
            if (tokens.Length == 2 && int.TryParse(tokens[0], out var lowerBound))
                return lowerBound;

            return null;
        }

        /// <summary>
        /// Resolves a <c>font-style</c> descriptor (<c>normal</c>/<c>italic</c>/<c>oblique</c>/
        /// <c>oblique &lt;angle&gt;</c>/absent) to whether the face should be treated as italic for
        /// matching purposes. Returns null for absent/unrecognized values.
        /// </summary>
        internal static bool? ResolveIsItalic(string? styleDescriptor)
        {
            if (string.IsNullOrWhiteSpace(styleDescriptor))
                return null;

            var firstToken = styleDescriptor!.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0];

            if (firstToken == CssConstants.Italic || firstToken == CssConstants.Oblique)
                return true;
            if (firstToken == CssConstants.Normal)
                return false;

            return null;
        }

        /// <summary>
        /// Resolves a <c>font-stretch</c> descriptor (one of the 9 CSS Fonts keywords, or absent) to the
        /// matching 1-9 numeric scale via <see cref="FontStretchResolver"/>. Percentage values/ranges
        /// (variable-font syntax) and any other unrecognized value return null rather than being silently
        /// coerced to normal.
        /// </summary>
        internal static int? ResolveStretch(string? stretchDescriptor)
        {
            if (string.IsNullOrWhiteSpace(stretchDescriptor))
                return null;

            var trimmed = stretchDescriptor!.Trim();

            switch (trimmed)
            {
                case CssConstants.UltraCondensed:
                case CssConstants.ExtraCondensed:
                case CssConstants.Condensed:
                case CssConstants.SemiCondensed:
                case CssConstants.Normal:
                case CssConstants.SemiExpanded:
                case CssConstants.Expanded:
                case CssConstants.ExtraExpanded:
                case CssConstants.UltraExpanded:
                    return FontStretchResolver.Resolve(trimmed);
                default:
                    return null;
            }
        }
    }
}
