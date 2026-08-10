#nullable enable

using System;
using System.IO;
using System.Text;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace TheArtOfDev.HtmlRenderer.Core.Utils
{
    /// <summary>
    /// Sniffs a TTF/OTF font file's family/subfamily name, style, and CSS Fonts numeric weight/stretch
    /// directly from its <c>name</c>/<c>OS/2</c> tables - a single self-contained reader, ported from
    /// PeachPDF's <c>Fonts\TtfFontDescription.cs</c> (adapted to this project's own <see cref="RFontStyle"/>
    /// instead of PeachPDF's PdfSharpCore-specific <c>XFontStyle</c>, since this type lives in the shared
    /// Core project used by every backend, not just PdfSharp - both WPF's <c>@font-face</c> loading, which
    /// needs the font's own internal family name to look it up via <c>System.Windows.Media.FontFamily</c>
    /// after registering it with <c>AddFontMemResourceEx</c>, and the PdfSharp <c>FontResolver</c> port,
    /// which needs the numeric weight/stretch for CSS Fonts Level 4 matching, need this). Public: it's
    /// consumed from the WPF and PdfSharp backend assemblies, not just Core itself.
    /// </summary>
    /// <remarks>
    /// Replaces this project's older <c>HtmlRenderer.PdfSharp\FontResolution\FontMetadata.cs</c>/
    /// <c>Parsing\FontParser.cs</c> (PdfSharp-backend-only, name-table-only, no numeric weight/stretch at
    /// all - it could not supply what CSS Fonts Level 4 matching needs).
    /// </remarks>
    public readonly struct TtfFontDescription
    {
        /// <summary>Default CSS Fonts numeric weight (400 = "normal") used when a font has no OS/2 table, or its <see cref="Weight"/> field is out of the valid 1-1000 range.</summary>
        public const int DefaultWeight = 400;

        /// <summary>Default CSS Fonts stretch value (5 = "normal" on the 1-9 <c>usWidthClass</c> scale) used when a font has no OS/2 table, or its value is out of the valid 1-9 range.</summary>
        public const int DefaultStretch = 5;

        public TtfFontDescription(string fontFamilyInvariantCulture, string fontNameInvariantCulture, RFontStyle style, int weight, int stretch)
        {
            FontFamilyInvariantCulture = fontFamilyInvariantCulture;
            FontNameInvariantCulture = fontNameInvariantCulture;
            Style = style;
            Weight = weight;
            Stretch = stretch;
        }

        public string FontFamilyInvariantCulture { get; }
        public string FontNameInvariantCulture { get; }
        public RFontStyle Style { get; }

        /// <summary>
        /// CSS Fonts Level 4 numeric weight (1-1000), read from the OS/2 table's <c>usWeightClass</c>
        /// field when present and valid; falls back to a value derived from <see cref="Style"/>'s
        /// name-table-subfamily-sniffed Bold bit (700 if bold, else <see cref="DefaultWeight"/>) when
        /// OS/2 is absent or its <c>usWeightClass</c> is 0 (a real font can legitimately omit/zero this
        /// field even though the spec range is 1-1000).
        /// </summary>
        public int Weight { get; }

        /// <summary>
        /// CSS Fonts Level 3 <c>font-stretch</c> classification (1-9, matching the OS/2 <c>usWidthClass</c>
        /// scale directly: 1=ultra-condensed ... 5=normal ... 9=ultra-expanded), read from the OS/2 table
        /// when present and valid; <see cref="DefaultStretch"/> (normal) otherwise.
        /// </summary>
        public int Stretch { get; }

        public static TtfFontDescription LoadDescription(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                return LoadDescription(stream);
            }
        }

        public static TtfFontDescription LoadDescription(Stream stream)
        {
            // TTF/OTF files are big-endian. Read the offset table to locate the name/OS2 tables.
            var buf4 = new byte[4];
            var buf2 = new byte[2];

            ReadExactly(stream, buf4, 4); // sfVersion - skip
            ReadExactly(stream, buf2, 2);
            int numTables = ReadUInt16BE(buf2);
            ReadExactly(stream, buf2, 2); // searchRange
            ReadExactly(stream, buf2, 2); // entrySelector
            ReadExactly(stream, buf2, 2); // rangeShift

            long nameTableOffset = -1;
            long os2TableOffset = -1;
            for (int i = 0; i < numTables; i++)
            {
                ReadExactly(stream, buf4, 4);
                var tag = Encoding.ASCII.GetString(buf4);
                ReadExactly(stream, buf4, 4); // checkSum
                ReadExactly(stream, buf4, 4);
                uint tableOffset = ReadUInt32BE(buf4);
                ReadExactly(stream, buf4, 4); // length

                if (tag == "name")
                    nameTableOffset = tableOffset;
                else if (tag == "OS/2")
                    os2TableOffset = tableOffset;
            }

            if (nameTableOffset < 0)
                throw new InvalidOperationException("Font file does not contain a name table.");

            stream.Seek(nameTableOffset, SeekOrigin.Begin);
            ReadExactly(stream, buf2, 2); // format
            ReadExactly(stream, buf2, 2);
            int count = ReadUInt16BE(buf2);
            ReadExactly(stream, buf2, 2);
            int stringOffset = ReadUInt16BE(buf2);
            long storageBase = nameTableOffset + stringOffset;

            // Read all name records (6 uint16 fields each)
            var platformIDs = new ushort[count];
            var encodingIDs = new ushort[count];
            var languageIDs = new ushort[count];
            var nameIDs = new ushort[count];
            var lengths = new ushort[count];
            var offsets = new ushort[count];

            for (int i = 0; i < count; i++)
            {
                ReadExactly(stream, buf2, 2); platformIDs[i] = (ushort)ReadUInt16BE(buf2);
                ReadExactly(stream, buf2, 2); encodingIDs[i] = (ushort)ReadUInt16BE(buf2);
                ReadExactly(stream, buf2, 2); languageIDs[i] = (ushort)ReadUInt16BE(buf2);
                ReadExactly(stream, buf2, 2); nameIDs[i] = (ushort)ReadUInt16BE(buf2);
                ReadExactly(stream, buf2, 2); lengths[i] = (ushort)ReadUInt16BE(buf2);
                ReadExactly(stream, buf2, 2); offsets[i] = (ushort)ReadUInt16BE(buf2);
            }

            string? familyName = ReadBestNameRecord(stream, platformIDs, encodingIDs, languageIDs, nameIDs, lengths, offsets, count, storageBase, 1);
            string? subfamilyName = ReadBestNameRecord(stream, platformIDs, encodingIDs, languageIDs, nameIDs, lengths, offsets, count, storageBase, 2);
            string? fullName = ReadBestNameRecord(stream, platformIDs, encodingIDs, languageIDs, nameIDs, lengths, offsets, count, storageBase, 4);

            RFontStyle style;
            switch (subfamilyName != null ? subfamilyName.ToLowerInvariant() : null)
            {
                case "bold italic":
                case "bold oblique":
                    style = RFontStyle.Bold | RFontStyle.Italic;
                    break;
                case "bold":
                    style = RFontStyle.Bold;
                    break;
                case "italic":
                case "oblique":
                    style = RFontStyle.Italic;
                    break;
                default:
                    style = RFontStyle.Regular;
                    break;
            }

            int weight, stretch;
            ReadOs2WeightAndStretch(stream, os2TableOffset, out weight, out stretch);
            if (weight == 0)
                weight = (style & RFontStyle.Bold) != 0 ? 700 : DefaultWeight;

            return new TtfFontDescription(
                familyName ?? fullName ?? string.Empty,
                fullName ?? familyName ?? string.Empty,
                style,
                weight,
                stretch);
        }

        /// <summary>
        /// Reads <c>usWeightClass</c> (offset 4) and <c>usWidthClass</c> (offset 6) from the OS/2 table,
        /// per the OpenType spec's OS/2 table layout (both fields are present in every OS/2 table
        /// version, including the oldest version 0). Yields (0, <see cref="DefaultStretch"/>) - a
        /// sentinel the caller substitutes a Style-derived default for - when there's no OS/2 table at
        /// all, or a value is outside its spec-valid range (weight: 1-1000, stretch: 1-9).
        /// </summary>
        private static void ReadOs2WeightAndStretch(Stream stream, long os2TableOffset, out int weight, out int stretch)
        {
            if (os2TableOffset < 0)
            {
                weight = 0;
                stretch = DefaultStretch;
                return;
            }

            var buf2 = new byte[2];
            stream.Seek(os2TableOffset + 4, SeekOrigin.Begin);
            ReadExactly(stream, buf2, 2);
            var weightClass = ReadUInt16BE(buf2);
            ReadExactly(stream, buf2, 2);
            var widthClass = ReadUInt16BE(buf2);

            weight = weightClass >= 1 && weightClass <= 1000 ? weightClass : 0;
            stretch = widthClass >= 1 && widthClass <= 9 ? widthClass : DefaultStretch;
        }

        // Prefers platformID=3/encodingID=1 (Windows Unicode) with en-US, then any language,
        // then platformID=1 (Mac Roman), then whatever is available.
        private static string? ReadBestNameRecord(
            Stream stream,
            ushort[] platformIDs, ushort[] encodingIDs, ushort[] languageIDs,
            ushort[] nameIDs, ushort[] lengths, ushort[] offsets,
            int count, long storageBase, ushort targetNameID)
        {
            int best = -1;
            int bestPriority = int.MaxValue;

            for (int i = 0; i < count; i++)
            {
                if (nameIDs[i] != targetNameID) continue;

                int priority;
                if (platformIDs[i] == 3 && encodingIDs[i] == 1 && languageIDs[i] == 0x0409)
                    priority = 0;
                else if (platformIDs[i] == 3 && encodingIDs[i] == 1)
                    priority = 1;
                else if (platformIDs[i] == 1)
                    priority = 2;
                else
                    priority = 3;

                if (priority < bestPriority)
                {
                    bestPriority = priority;
                    best = i;
                }
            }

            if (best < 0) return null;

            stream.Seek(storageBase + offsets[best], SeekOrigin.Begin);
            var bytes = new byte[lengths[best]];
            ReadExactly(stream, bytes, bytes.Length);

            return platformIDs[best] == 1
                ? GetLatin1().GetString(bytes, 0, bytes.Length)
                : Encoding.BigEndianUnicode.GetString(bytes, 0, bytes.Length);
        }

        // Encoding.Latin1 is a .NET 5+ convenience shortcut - GetEncoding("iso-8859-1") is the portable
        // equivalent available on every TFM this project targets (including netstandard2.0/net462).
        private static Encoding GetLatin1() => Encoding.GetEncoding("iso-8859-1");

        /// <summary>
        /// Fills <paramref name="buffer"/>'s first <paramref name="count"/> bytes from <paramref name="stream"/>,
        /// looping until satisfied (a plain <see cref="Stream.Read"/> is not guaranteed to fill the buffer
        /// in one call). The portable equivalent of <c>Stream.ReadExactly</c> (.NET 7+ only, not available
        /// on this project's netstandard2.0/net462 legs).
        /// </summary>
        private static void ReadExactly(Stream stream, byte[] buffer, int count)
        {
            var offset = 0;
            while (offset < count)
            {
                var read = stream.Read(buffer, offset, count - offset);
                if (read <= 0)
                    throw new EndOfStreamException();
                offset += read;
            }
        }

        private static int ReadUInt16BE(byte[] b) =>
            (b[0] << 8) | b[1];

        private static uint ReadUInt32BE(byte[] b) =>
            ((uint)b[0] << 24) | ((uint)b[1] << 16) | ((uint)b[2] << 8) | b[3];
    }
}
