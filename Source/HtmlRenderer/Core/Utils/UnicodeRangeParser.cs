#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using TheArtOfDev.HtmlRenderer.Core.Parse;

namespace TheArtOfDev.HtmlRenderer.Core.Utils
{
    /// <summary>
    /// Parses a CSS <c>@font-face</c> <c>unicode-range</c> descriptor into a compact set of inclusive
    /// codepoint <see cref="CodepointRange"/>s, reusing the existing CSS tokenizer's <c>U+</c> grammar
    /// (<see cref="CssValueParser.GetCssTokens"/> → <see cref="RangeToken"/>, already fully implemented -
    /// see <see cref="TheArtOfDev.HtmlRenderer.Core.CssEngine.Parser.Lexer"/>'s <c>UnicodeRange</c> method)
    /// rather than re-implementing it.
    /// </summary>
    public static class UnicodeRangeParser
    {
        /// <summary>
        /// Parses a <c>unicode-range</c> descriptor into inclusive codepoint ranges, or returns null when
        /// the descriptor is absent/blank or contains no valid range (meaning "no explicit subset - the
        /// face applies to whatever its font actually covers").
        /// </summary>
        public static IReadOnlyList<CodepointRange>? Parse(string? descriptor)
        {
            if (string.IsNullOrWhiteSpace(descriptor))
                return null;

            List<CodepointRange>? ranges = null;

            // The value can arrive either in its CSS source form ("U+41-5A, U+61-7A") or, once round-
            // tripped through the CSS-OM, with the "U+" prefix dropped ("41-5A, 61-7A"). Split on the
            // top-level commas and re-tokenize each segment with the "U+" prefix the lexer's range
            // grammar expects, so both forms parse identically through the one shared tokenizer.
            // The netstandard2.0 reference assembly's string.IsNullOrWhiteSpace has no [NotNullWhen]
            // annotation, so the compiler can't narrow descriptor to non-null past the check above on
            // that TFM alone (net8.0 already narrows it) - the ! makes both legs agree.
            foreach (var segment in descriptor!.Split(','))
            {
                var normalized = segment.Trim();

                if (normalized.Length == 0)
                    continue;

                if (!normalized.StartsWith("U+", StringComparison.OrdinalIgnoreCase))
                    normalized = "U+" + normalized;

                var rangeToken = CssValueParser.GetCssTokens(normalized).OfType<RangeToken>().FirstOrDefault();

                if (rangeToken == null)
                    continue;

                if (!int.TryParse(rangeToken.Start, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var start) ||
                    !int.TryParse(rangeToken.End, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var end))
                    continue;

                if (start > Symbols.MaximumCodepoint)
                    continue;

                if (end > Symbols.MaximumCodepoint)
                    end = Symbols.MaximumCodepoint;

                if (end < start)
                    continue;

                // A declared range whose bound lands on a surrogate can't be a real Unicode scalar value
                // (surrogates aren't valid codepoints on their own); nudge inward, and skip a range that
                // is nothing but surrogates.
                if (start is >= 0xD800 and <= 0xDFFF)
                    start = 0xE000;
                if (end is >= 0xD800 and <= 0xDFFF)
                    end = 0xD7FF;
                if (start > end)
                    continue;

                (ranges ??= new List<CodepointRange>()).Add(new CodepointRange(start, end));
            }

            return ranges;
        }

        /// <summary>
        /// Whether <paramref name="codepoint"/> falls inside any of <paramref name="ranges"/>.
        /// </summary>
        public static bool Covers(IReadOnlyList<CodepointRange> ranges, int codepoint)
        {
            for (var i = 0; i < ranges.Count; i++)
            {
                if (ranges[i].Contains(codepoint))
                    return true;
            }
            return false;
        }
    }
}
