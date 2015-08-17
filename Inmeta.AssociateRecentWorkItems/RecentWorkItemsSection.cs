using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.VersionControl.Controls.Extensibility;
using Microsoft.VisualStudio.TeamFoundation.WorkItemTracking;
using Osiris.TeamExplorer.Extensions.Common;

namespace Osiris.AssociateRecentWorkItems
{
    /// <summary>
    /// Selected file info section.
    /// </summary>
    [TeamExplorerSection(SectionId, TeamExplorerPageIds.PendingChanges, 35)]
    public class RecentWorkItemsSection : TeamExplorerBaseSection
    {
        public const string SectionId = "A7D7E0F2-6847-439F-834A-7CDB508FEBFA";
        private const string SectionTitle = "Recently Associated Work Items";

        private ObservableCollection<AssociatedWorkItemInfo> recentWorkItems = new ObservableCollection<AssociatedWorkItemInfo>();

        /// <summary>
        /// Constructor.
        /// </summary>
        public RecentWorkItemsSection()
            : base()
        {            
            this.Title = SectionTitle;
            this.IsExpanded = true;
            this.IsBusy = false;
            this.SectionContent = new RecentWorkItemsView();
            this.View.ParentSection = this;
        }

        /// <summary>
        /// Get the view.
        /// </summary>
        protected RecentWorkItemsView View
        {
            get { return this.SectionContent as RecentWorkItemsView; }
        }

        /// <summary>
        /// Initialize override.
        /// </summary>
        public override void Initialize(object sender, SectionInitializeEventArgs e)
        {
            base.Initialize(sender, e);

            // Find the Pending Changes extensibility service and sign up for
            // property change notifications
            var pcExt = this.GetService<IPendingChangesExt>();
            if (pcExt != null)
            {
                pcExt.PropertyChanged += pcExt_PropertyChanged;
            }

            var ds = this.GetService<DocumentService>();
            this.View.Context = this.CurrentContext;
            this.View.DocumentService = ds;
            this.RefreshAsync();
        }

        /// <summary>
        /// Dispose override.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();

            var pcExt = this.GetService<IPendingChangesExt>();
            if (pcExt != null)
            {
                pcExt.PropertyChanged -= pcExt_PropertyChanged;
            }
        }

        /// <summary>
        /// Pending Changes Extensibility PropertyChanged event handler.
        /// </summary>
        private void pcExt_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "WorkItems":
                    Refresh();
                    break;
            }
        }

        /// <summary>
        /// Refresh override.
        /// </summary>
        public async override void Refresh()
        {
            base.Refresh();
            await RefreshAsync();
        }

        /// <summary>
        /// Refresh the changeset data asynchronously.
        /// </summary>
        private async Task RefreshAsync()
        {
            try
            {
                var pc = GetService<IPendingChangesExt>();
                var currentlyAssociatedWorkItems = pc.WorkItems;

                // Set our busy flag and clear the previous data
                this.IsBusy = true;
                this.RecentWorkItems.Clear();

                var workItems = new ObservableCollection<AssociatedWorkItemInfo>();

                // Make the server call asynchronously to avoid blocking the UI
                await Task.Run(() =>
                {
                    ITeamFoundationContext context = this.CurrentContext;
                    if (context != null && context.HasCollection && context.HasTeamProject)
                    {
                        var vcs = context.TeamProjectCollection.GetService<VersionControlServer>();
                        if (vcs != null)
                        {
                            string path = "$/" + context.TeamProjectName;
                            foreach (Changeset changeset in vcs.QueryHistory(path, VersionSpec.Latest, 0, RecursionType.Full,
                                                                             vcs.AuthorizedUser, null, null, 10, false, true))
                            {
                                foreach (var wi in changeset.AssociatedWorkItems)
                                {
                                    if (workItems.All(w => w.Id != wi.Id) && currentlyAssociatedWorkItems.All(w => w.WorkItem.Id != wi.Id))
                                    {
                                        workItems.Add(wi);
                                    }
                                }
                            }
                        }
                    }
                });

                // Now back on the UI thread, update the bound collection and section title
                this.RecentWorkItems = new ObservableCollection<AssociatedWorkItemInfo>(workItems.Take(5));
                this.Title = this.RecentWorkItems.Count > 0 ? String.Format(" {0} ({1})", SectionTitle, this.RecentWorkItems.Count)
                                                       : SectionTitle;
            }
            catch (Exception ex)
            {
                ShowNotification(ex.Message, NotificationType.Error);
            }
            finally
            {
                // Always clear our busy flag when done
                this.IsBusy = false;
            }
        }

        public ObservableCollection<AssociatedWorkItemInfo> RecentWorkItems
        {
            get { return recentWorkItems; }
            protected set { recentWorkItems = value; RaisePropertyChanged("RecentWorkItems"); }
        }

    }


}
