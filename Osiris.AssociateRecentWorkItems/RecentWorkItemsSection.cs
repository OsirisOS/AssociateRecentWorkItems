using System.Collections.Generic;
using System.Linq;
using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace Osiris.AssociateRecentWorkItems
{
    /// <summary>
    /// Selected file info section.
    /// </summary>
    [TeamExplorerSection(SectionId, TeamExplorerPageIds.PendingChanges, 35)]
    public class RecentWorkItemsSection : WorkItemsSectionBase
    {
        public const string SectionId = "A7D7E0F2-6847-439F-834A-7CDB508FEBFA";
        private const string SectionTitle = "Recently Associated Work Items";

        /// <summary>
        /// Constructor.
        /// </summary>
        public RecentWorkItemsSection()
            : base(SectionTitle, true) { }

        protected override IEnumerable<WorkItemInfo> GetWorkItems(WorkItemCheckinInfo[] currentlyAssociatedWorkItems)
        {
            var context = CurrentContext;
            if (context == null || !context.HasCollection || !context.HasTeamProject)
                return Enumerable.Empty<WorkItemInfo>();

            var vcs = context.TeamProjectCollection.GetService<VersionControlServer>();
            if (vcs == null)
                return Enumerable.Empty<WorkItemInfo>();

            var workItems = new List<WorkItemInfo>();

            string path = "$/" + context.TeamProjectName;
            foreach (Changeset changeset in vcs.QueryHistory(path, VersionSpec.Latest, 0, RecursionType.Full, vcs.AuthorizedUser, null, null, 10, false, true))
            {
                foreach (var wi in changeset.AssociatedWorkItems)
                {
                    if (workItems.All(w => w.Id != wi.Id) && currentlyAssociatedWorkItems.All(w => w.WorkItem.Id != wi.Id))
                    {
                        workItems.Add(new WorkItemInfo(wi.Id, wi.Title, wi.WorkItemType, wi.State, wi.AssignedTo));
                    }
                }
            }

            return workItems;
        }
    }
}
