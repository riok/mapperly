using System.Diagnostics;

namespace Riok.Mapperly.Symbols.Members;

[DebuggerDisplay("{MemberPath} ({Type})")]
public record SourceMemberPath(MemberPath MemberPath, SourceMemberType Type);
