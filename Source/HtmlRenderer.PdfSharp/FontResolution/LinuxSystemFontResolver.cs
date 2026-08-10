using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace TheArtOfDev.HtmlRenderer.PdfSharp.FontResolution
{
    /// <summary>
    /// Linux system font-file discovery, used by <see cref="FontResolver.DiscoverSupportedFonts"/>.
    /// </summary>
    /// <remarks>
    /// Adapted, not verbatim, from PeachPDF's <c>Fonts\LinuxSystemFontResolver.cs</c>: PeachPDF
    /// P/Invokes <c>libfontconfig.so.1</c> directly via source-generated <c>[LibraryImport]</c> bindings,
    /// which need .NET 7+ and don't exist on this project's <c>netstandard2.0</c> target framework.
    /// Shelling out to the <c>fc-list</c> CLI - fontconfig's own command-line front end, present wherever
    /// <c>libfontconfig</c> itself is - returns the identical font-file listing with no native interop and
    /// no TFM split; this project's own pre-existing Linux discovery already took this approach, so it's
    /// kept here rather than replaced. When <c>fc-list</c> isn't available (or fails), falls back to
    /// scanning <c>fonts.conf</c>'s declared directories plus the conventional well-known ones directly -
    /// matching PeachPDF's own fallback behavior.
    /// </remarks>
    internal static class LinuxSystemFontResolver
    {
        public static string[] Resolve()
        {
            try
            {
                var files = ResolveViaFontConfig();
                if (files.Length > 0)
                    return files;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            return ResolveFallback();
        }

        private static string[] ResolveViaFontConfig()
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "fc-list",
                Arguments = ": file",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(startInfo))
            {
                if (process == null)
                    return Array.Empty<string>();

                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0 || string.IsNullOrWhiteSpace(output))
                    return Array.Empty<string>();

                // Each line looks like: "/path/to/font.ttf: file"
                return output
                    .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(line => line.Split(':')[0].Trim())
                    .Where(IsSupportedFontFile)
                    .Where(File.Exists)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();
            }
        }

        private static bool IsSupportedFontFile(string path) =>
            path.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".otf", StringComparison.OrdinalIgnoreCase);

        private static string[] ResolveFallback()
        {
            var fontFiles = new List<string>();

            foreach (var path in GetSearchPaths().Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (!Directory.Exists(path))
                    continue;

                try
                {
                    fontFiles.AddRange(Directory.EnumerateFiles(path, "*.ttf", SearchOption.AllDirectories));
                    fontFiles.AddRange(Directory.EnumerateFiles(path, "*.otf", SearchOption.AllDirectories));
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }

            return fontFiles.ToArray();
        }

        private static IEnumerable<string> GetSearchPaths()
        {
            var dirs = new List<string>();

            try
            {
                var confDirRegex = new Regex("<dir>(?<dir>.*)</dir>");
                if (File.Exists("/etc/fonts/fonts.conf"))
                {
                    foreach (var line in File.ReadLines("/etc/fonts/fonts.conf"))
                    {
                        var match = confDirRegex.Match(line);
                        if (!match.Success)
                            continue;

                        var path = match.Groups["dir"].Value.Trim();
                        if (path.StartsWith("~"))
                            path = Environment.GetEnvironmentVariable("HOME") + path.Substring(1);

                        dirs.Add(path);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            dirs.Add("/usr/share/fonts");
            dirs.Add("/usr/local/share/fonts");
            var home = Environment.GetEnvironmentVariable("HOME");
            if (!string.IsNullOrEmpty(home))
                dirs.Add(Path.Combine(home, ".fonts"));

            return dirs;
        }
    }
}
