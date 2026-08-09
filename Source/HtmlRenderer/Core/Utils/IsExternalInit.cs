#if NETSTANDARD2_0
// C# 9 records/init-only setters need this marker type, which only ships in the BCL from
// netstandard2.1/.NET 5 onward. LangVersion 12 (set project-wide in Directory.Build.props) makes the
// record/init-accessor *syntax* available on netstandard2.0 too, but the compiler still needs the type to
// exist somewhere in the compilation - this is the standard, widely-used polyfill for that gap.
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit
    {
    }
}
#endif
