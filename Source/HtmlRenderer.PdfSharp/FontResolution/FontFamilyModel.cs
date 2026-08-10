using System.Collections.Generic;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.PdfSharp.FontResolution
{
    /// <summary>
    /// One registered face of a <see cref="FontFamilyModel"/>: its CSS Fonts Level 4 matching axes
    /// (numeric weight, italic, stretch), the codepoint <see cref="CodepointRange"/>s it is restricted to
    /// (from an <c>@font-face</c> <c>unicode-range</c> descriptor) or null when it has none - a face with
    /// no explicit ranges is treated as covering everything (matching the same, deliberately simpler,
    /// scope as the shared <c>Core.Handlers.FontsHandler</c> face registry - see its own doc comments for
    /// why cmap-based coverage extraction isn't ported here: this codebase resolves one font per box, not
    /// per glyph, so there is no per-glyph precision to exploit) - and the sniffed description
    /// <see cref="FontResolver.ResolveTypeface(string,int,bool,int,int?)"/> hands back once this face is
    /// chosen; <see cref="TtfFontDescription.FontNameInvariantCulture"/> doubles as this face's storage
    /// key into <see cref="FontResolver.GetFont"/>.
    /// </summary>
    /// <remarks>Ported from PeachPDF's <c>Fonts\FontFamilyModel.cs</c>.</remarks>
    internal sealed class FontFaceEntry
    {
        public FontFaceEntry(int weight, bool italic, int stretch, IReadOnlyList<CodepointRange> explicitRanges, TtfFontDescription description)
        {
            Weight = weight;
            Italic = italic;
            Stretch = stretch;
            ExplicitRanges = explicitRanges;
            Description = description;
        }

        public int Weight { get; }
        public bool Italic { get; }
        public int Stretch { get; }
        public IReadOnlyList<CodepointRange> ExplicitRanges { get; }
        public TtfFontDescription Description { get; }
    }

    /// <summary>One CSS font-family: its registered faces. Ported from PeachPDF's <c>Fonts\FontFamilyModel.cs</c>.</summary>
    internal sealed class FontFamilyModel
    {
        public string Name { get; set; }

        /// <summary>
        /// Every registered face for this family - a list, not a dictionary keyed by (weight,italic,
        /// stretch), because two faces can legitimately share the same matching axes yet differ by
        /// <c>unicode-range</c> (e.g. a Latin subset and a Cyrillic subset of one webfont family, both
        /// regular weight).
        /// </summary>
        public List<FontFaceEntry> Faces { get; } = new List<FontFaceEntry>();
    }
}
