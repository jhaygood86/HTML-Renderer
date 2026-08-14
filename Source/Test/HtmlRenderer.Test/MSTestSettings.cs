// HtmlRenderer.Core.Parse.RegexParserUtils caches compiled regexes in a plain, unlocked static
// Dictionary<string, Regex> (see RegexParserUtils.GetRegex). That's fine for the library's normal
// single-threaded-per-container usage, but running this test assembly's many independent
// CssParser/CssData-driven tests concurrently races on populating that shared static dictionary and
// intermittently throws (e.g. IndexOutOfRangeException/ArgumentException from Dictionary internals).
// That's a pre-existing thread-safety gap in shared library state, not something a single test can
// work around, so this assembly runs its tests sequentially rather than opting into MSTest's
// parallel execution.
