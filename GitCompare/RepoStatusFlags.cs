using System;

namespace GitCompare
{
    [Flags]
    public enum RepoStatusFlags
    {
        CleanAndUpToDate = 0x00,
        OutgoingChanges = 0x01,
        IncomingChanges = 0x02,
        UncommittedChanges = 0x04
    }
}
