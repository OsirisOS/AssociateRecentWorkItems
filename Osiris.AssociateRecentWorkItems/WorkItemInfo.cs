namespace Osiris.AssociateRecentWorkItems
{
    public class WorkItemInfo
    {   
        public int Id { get; private set; }

        public string Title { get; private set; }

        public string WorkItemType { get; private set; }

        public string State { get; private set; }

        public string AssignedTo { get; private set; }

        public WorkItemInfo(int id, string title, string workItemType, string state, string assignedTo)
        {
            Id = id;
            Title = title;
            WorkItemType = workItemType;
            State = state;
            AssignedTo = assignedTo;
        }
    }
}