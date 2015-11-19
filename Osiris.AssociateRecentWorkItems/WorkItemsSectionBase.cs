using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.VersionControl.Controls.Extensibility;
using Microsoft.VisualStudio.TeamFoundation.WorkItemTracking;
using Osiris.TeamExplorer.Extensions.Common;

namespace Osiris.AssociateRecentWorkItems
{
    public abstract class WorkItemsSectionBase : TeamExplorerBaseSection
    {
        private ObservableCollection<WorkItemInfo> recentWorkItems = new ObservableCollection<WorkItemInfo>();
        private readonly string baseTitle;

        /// <summary>
        /// Constructor.
        /// </summary>
        protected WorkItemsSectionBase(string title, bool expanded)
            : base()
        {
            baseTitle = title;
            this.Title = baseTitle;
            this.IsExpanded = expanded;
            this.IsBusy = false;
            this.SectionContent = new WorkItemsView();
            this.View.ParentSection = this;
        }

        /// <summary>
        /// Get the view.
        /// </summary>
        protected WorkItemsView View
        {
            get { return this.SectionContent as WorkItemsView; }
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

                WorkItemInfo[] workItems = null;

                // Make the server call asynchronously to avoid blocking the UI
                await Task.Run(() =>
                {
                    workItems = GetWorkItems(currentlyAssociatedWorkItems).Take(5).ToArray();
                });

                // Now back on the UI thread, update the bound collection and section title
                this.RecentWorkItems = new ObservableCollection<WorkItemInfo>(workItems);
                this.Title = this.RecentWorkItems.Count > 0 ? String.Format(" {0} ({1})", baseTitle, this.RecentWorkItems.Count)
                                                       : baseTitle;
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

        protected abstract IEnumerable<WorkItemInfo> GetWorkItems(WorkItemCheckinInfo[] currentlyAssociatedWorkItems);

        public ObservableCollection<WorkItemInfo> RecentWorkItems
        {
            get { return recentWorkItems; }
            protected set { recentWorkItems = value; RaisePropertyChanged("RecentWorkItems"); }
        }
    }
}
