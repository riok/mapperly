using System;

namespace Riok.Mapperly.IntegrationTests.Helpers
{
    /// <summary>
    /// Uses different paths for snapshots of each version with expected changes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class VersionedSnapshotAttribute : Attribute
    {
        /// <summary>
        /// Uses different paths for snapshots of each version with expected changes.
        /// </summary>
        /// <param name="versionsWithChanges">The versions which result in different snapshot content than the previous version.</param>
        public VersionedSnapshotAttribute(Versions versionsWithChanges)
        {
            VersionsWithChanges = versionsWithChanges;
        }

        public Versions VersionsWithChanges { get; }
    }
}
