using System.Collections.Generic;
using System.Linq;
using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Osiris.AssociateRecentWorkItems
{
    [TeamExplorerSection(SectionId, TeamExplorerPageIds.PendingChanges, 34)]
    public class NewWorkItemsSection : WorkItemsSectionBase
    {
        public const string SectionId = "433130C1-5E7B-48A3-A142-B1F1567B2AB3";
        private const string SectionTitle = "My Newest Work Items";

        public NewWorkItemsSection()
            : base(SectionTitle, false) { }

        protected override IEnumerable<WorkItemInfo> GetWorkItems(WorkItemCheckinInfo[] currentlyAssociatedWorkItems)
        {
            var context = CurrentContext;
            if (context == null || !context.HasCollection || !context.HasTeamProject)
                return Enumerable.Empty<WorkItemInfo>();

            var store = context.TeamProjectCollection.GetService<WorkItemStore>();
            if (store == null)
                return Enumerable.Empty<WorkItemInfo>();

            var workItems = new List<WorkItemInfo>();

            var results = store.Query("Select [ID], [Title] From WorkItems Where [Assigned to] = @Me Order By [Created Date] desc");

            foreach (WorkItem workItem in results)
            {
                if (currentlyAssociatedWorkItems.All(w => w.WorkItem.Id != workItem.Id))
                {
                    workItems.Add(new WorkItemInfo(workItem.Id, workItem.Title, workItem.Type.Name, workItem.State, "@Me"));

                    if (workItems.Count == 5)
                        break;
                }
            }

            return workItems;
        }
    }
}
