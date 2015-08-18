using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.VersionControl.Controls.Extensibility;
using Microsoft.VisualStudio.TeamFoundation.WorkItemTracking;

namespace Osiris.AssociateRecentWorkItems
{
    public partial class RecentWorkItemsView : UserControl
    {
        public RecentWorkItemsView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Parent section.
        /// </summary>
        public RecentWorkItemsSection ParentSection
        {
            get { return (RecentWorkItemsSection)GetValue(ParentSectionProperty); }
            set { SetValue(ParentSectionProperty, value); }
        }

        public ITeamFoundationContext Context { get; set; }

        public DocumentService DocumentService { get; set; }

        public static readonly DependencyProperty ParentSectionProperty =
            DependencyProperty.Register("ParentSection", typeof(RecentWorkItemsSection), typeof(RecentWorkItemsView));


        private void workItemList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OnOpenWorkItem(sender, e);
        }

        private void OnOpenWorkItem(object sender, RoutedEventArgs e)
        {
            var item = this.workItemList.SelectedItem;
            if (item == null)
                return;

            var selectedWorkItem = item as AssociatedWorkItemInfo;
            if (selectedWorkItem == null)
                return;

            int selectedWorkItemId = selectedWorkItem.Id;

            IWorkItemDocument widoc = null;
            try
            {

                widoc = DocumentService.GetWorkItem(Context.TeamProjectCollection, selectedWorkItemId, this);
                DocumentService.ShowWorkItem(widoc);
            }
            finally
            {
                if( widoc != null )
                    widoc.Release(this);
            }
        }

        private void OnAddWorkItem(object sender, RoutedEventArgs e)
        {
            try
            {
                var item = this.workItemList.SelectedItem;
                if (item == null)
                    return;

                var selectedWorkItem = item as AssociatedWorkItemInfo;
                if (selectedWorkItem == null)
                    return;

                int selectedWorkItemId = selectedWorkItem.Id;

                var pc = ParentSection.GetService<IPendingChangesExt>();

                var model = pc.GetType().GetField("m_workItemsSection", BindingFlags.NonPublic | BindingFlags.Instance);
                var t = model.FieldType;
                var mm = model.GetValue(pc);
                var m = t.GetMethod("AddWorkItemById", BindingFlags.NonPublic | BindingFlags.Instance);

                m.Invoke(mm, new object[] { selectedWorkItemId });

                var workItem = ParentSection.RecentWorkItems.FirstOrDefault(wi => wi.Id == selectedWorkItemId);
                if (workItem != null)
                {
                    ParentSection.RecentWorkItems.Remove(workItem);
                }
            }
            catch (Exception ex)
            {
                ParentSection.ShowNotification(ex.ToString(), NotificationType.Error);
            }
        }
    }
}
