#nullable enable
using System;

namespace TheArtOfDev.HtmlRenderer.Core.Network
{
    /// <summary>
    /// Wraps <see cref="System.Uri"/> for use throughout the resource-loading pipeline, adding
    /// special-case handling for <c>data:</c> URIs (which <see cref="System.Uri"/> can parse but cannot
    /// round-trip correctly through <see cref="AbsoluteUri"/> on the target frameworks this library
    /// supports) and helpers for resolving a relative URI against a base URI.
    /// </summary>
    public class RUri
    {
        private readonly Uri? _uri;
        private readonly string? _originalUri;

        /// <summary>
        /// Parses <paramref name="uriString"/> as either an absolute or relative URI.
        /// </summary>
        /// <param name="uriString">The URI string to parse.</param>
        public RUri(string uriString)
        {
            if (uriString is null) throw new ArgumentNullException(nameof(uriString));

            if (uriString.StartsWith("data:"))
            {
                _originalUri = uriString;
            }
            else
            {
                _uri = new Uri(uriString);
            }
        }

        /// <summary>
        /// Parses <paramref name="uriString"/> as the given <paramref name="uriKind"/>.
        /// </summary>
        /// <param name="uriString">The URI string to parse.</param>
        /// <param name="uriKind">Whether the string is known to be absolute, relative, or either.</param>
        public RUri(string uriString, UriKind uriKind)
        {
            if (uriString.StartsWith("data:"))
            {
                _originalUri = uriString;
            }
            else
            {
                _uri = new Uri(uriString, uriKind);
            }
        }

        /// <summary>
        /// Resolves <paramref name="uri"/> against <paramref name="baseUri"/>, the same way a relative
        /// <c>href</c>/<c>src</c>/<c>url()</c> reference is resolved against a document's base URL.
        /// </summary>
        /// <param name="baseUri">The base URI to resolve against.</param>
        /// <param name="uri">The (typically relative) URI to resolve.</param>
        public RUri(RUri baseUri, RUri uri)
        {
            if (uri.Scheme == "data")
            {
                // See the string-uri overload below for why data: URIs skip Uri-combining entirely.
                _originalUri = uri.AbsoluteUri;
            }
            else
            {
                _uri = new Uri(baseUri.Uri, uri.Uri);
            }
        }

        /// <summary>
        /// Resolves <paramref name="uri"/> against <paramref name="baseUri"/>, the same way a relative
        /// <c>href</c>/<c>src</c>/<c>url()</c> reference is resolved against a document's base URL.
        /// </summary>
        /// <param name="baseUri">The base URI to resolve against.</param>
        /// <param name="uri">The (typically relative) URI string to resolve.</param>
        public RUri(RUri baseUri, string uri)
        {
            if (uri.StartsWith("data:"))
            {
                // A data: URI is already absolute and has no hierarchical structure to resolve against a
                // base - new Uri(baseUri, dataUriString) would just reproduce dataUriString verbatim, so
                // skip the combine and store it directly, matching the single-string constructor's own
                // data: fast path above. This isn't just an optimization: System.Uri's combining
                // constructor (unlike its single-string constructor's data: handling one layer up in
                // RUri's other constructor) has no such fast path of its own, and re-enables a legacy
                // ~64K-character length limit on Windows-Desktop-targeting builds (net8.0-windows/net462 -
                // confirmed via a throwaway repro project; a plain net8.0/net10.0 console app does not hit
                // it) - a real image's base64 payload routinely exceeds that. Skipping the combine avoids
                // the limit entirely for the one kind of reference it can never legitimately apply to.
                _originalUri = uri;
            }
            else
            {
                _uri = new Uri(baseUri.Uri, uri);
            }
        }

        /// <summary>
        /// Wraps an existing <see cref="System.Uri"/> instance.
        /// </summary>
        /// <param name="uri">The URI to wrap.</param>
        public RUri(Uri uri)
        {
            _uri = uri;
        }

        /// <summary>
        /// The underlying <see cref="System.Uri"/>.
        /// </summary>
        public Uri Uri => _uri ?? new Uri(_originalUri!);

        /// <summary>
        /// The URI scheme (e.g. <c>https</c>, <c>file</c>, <c>data</c>).
        /// </summary>
        public string Scheme => _uri is not null ? _uri.Scheme : _originalUri!.Split(':')[0];

        /// <summary>
        /// The fully escaped absolute URI string.
        /// </summary>
        public string AbsoluteUri => _uri is not null ? _uri.AbsoluteUri : _originalUri!;

        /// <summary>
        /// The original, unescaped URI string as it was parsed.
        /// </summary>
        public string OriginalString => _uri is not null ? _uri.OriginalString : _originalUri!;

        /// <summary>
        /// Whether this instance represents an absolute URI.
        /// </summary>
        public bool IsAbsoluteUri => _uri?.IsAbsoluteUri ?? true;

        /// <summary>
        /// Whether this URI refers to a local file.
        /// </summary>
        public bool IsFile => _uri?.IsFile ?? false;
    }
}
