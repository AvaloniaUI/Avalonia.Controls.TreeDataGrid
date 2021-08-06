using System;
using System.IO;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using PlasticAvalonia.WorkspaceWindow.Views.PendingChanges;

using PlasticGui;
using PlasticGui.WorkspaceWindow.PendingChanges;

using UiAvalonia.Table;

namespace ReproCase
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            DockPanel content = new DockPanel();

            Button clearButton = new Button();
            clearButton.Content = "Clear data";
            clearButton.Click += ClearButton_Click;

            Button removeTreeButton = new Button();
            removeTreeButton.Content = "Remove tree from parent container";
            removeTreeButton.Click += RemoveTreeButton_Click;

            Button addTreeButton = new Button();
            addTreeButton.Content = "Add tree to parent container";
            addTreeButton.Click += AddTreeButton_Click;

            mContainerPanel = new DockPanel();

            StackPanel toolbarPanel = new StackPanel();
            toolbarPanel.Spacing = 5;
            toolbarPanel.Orientation = Avalonia.Layout.Orientation.Horizontal;
            toolbarPanel.Children.Add(clearButton);
            toolbarPanel.Children.Add(removeTreeButton);
            toolbarPanel.Children.Add(addTreeButton);

            mPlasticTree = new PlasticTree<PendingChangeInfo>(
                PendingChangesTreeDefinition.BuildColumns(),
                PendingChangesTreeDefinition.BuildCellRenderFunction(OnCheckBoxClicked),
                TreeOperations.BuildExpandAllFunction(),
                null,
                PendingChangesTreeDefinition.AreEqual,
                PendingChangesTreeDefinition.BuildColumnComparers(),
                "ItemColumn",
                true,
                PendingChangesTreeDefinition.NAME,
                new Thickness(0),
                DoNothing);

            PendingChanges pendingChanges = new PendingChanges();
            pendingChanges.Changed.Add(new ChangeInfo()
            {
                
            });
            PendingChangesTree pendingChangesTree = new PendingChangesTree();
            pendingChangesTree.BuildChangeCategories(
                Path.GetTempPath(), pendingChanges);

            mPlasticTree.Fill(pendingChangesTree, null, new Filter(string.Empty));

            mContainerPanel.Children.Add(mPlasticTree.Tree);

            DockPanel.SetDock(toolbarPanel, Dock.Top);

            content.Children.Add(toolbarPanel);
            content.Children.Add(mContainerPanel);

            this.Content = content;
        }

        void AddTreeButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            mContainerPanel.Children.Add(mPlasticTree.Tree);
        }

        void RemoveTreeButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            mContainerPanel.Children.Remove(mPlasticTree.Tree);
        }

        void ClearButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            PendingChanges pendingChanges = new PendingChanges();
            PendingChangesTree pendingChangesTree = new PendingChangesTree();

            pendingChangesTree.BuildChangeCategories(
                Path.GetTempPath(), pendingChanges);

            mPlasticTree.Fill(pendingChangesTree, null, new Filter(string.Empty));
        }

        private void OnCheckBoxClicked(IPlasticTreeNode node, bool isChecked)
        {
            
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        void DoNothing()
        {

        }

        PlasticTree<PendingChangeInfo> mPlasticTree;
        DockPanel mContainerPanel;
    }
}
