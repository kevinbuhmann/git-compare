using System;
using System.Diagnostics.CodeAnalysis;

namespace GitCompare
{
    [Flags]
    [SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue", Justification = "'CleanAndUpToDate' name is used for output.")]
    public enum RepoStatus
    {
        CleanAndUpToDate = 0x00,
        OutgoingChanges = 0x01,
        IncomingChanges = 0x02,
        UncommittedChanges = 0x04
    }
}
