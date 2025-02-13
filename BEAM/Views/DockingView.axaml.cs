using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using BEAM.Docking;
using BEAM.ViewModels;
using NP.Ava.UniDock;
using NP.Ava.UniDockService;

namespace BEAM.Views;

public partial class DockingView : UserControl
{
    private DockManager _dockManager;

    public DockingView()
    {
        InitializeComponent();
        DataContextChanged += (s, e) =>
        {
            if (!IsInitialized) return;
            var vm = (DockingViewModel)DataContext!;
            vm.Items.CollectionChanged += _ItemsOnCollectionChanged;
        };

        _dockManager = (DockManager)this.FindResource("TheDockManager")!;
        _dockManager.DockItemsViewModels = [];
    }

    private int _i = 0;

    private void _ItemsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action != NotifyCollectionChangedAction.Add) return;
        if (e.NewItems is null || e.NewItems.Count == 0) return;

        var dock = (e.NewItems[0] as IDockBase)!;

        var model = new DockItemViewModelBase
        {
            DockId = $"{dock.Name}#{_i++}",
            Header = dock.Name,
            Content = dock,
            DefaultDockGroupId = "StackDock",
            IsActive = true,
            IsSelected = false,
        };
        _dockManager.DockItemsViewModels!.Add(model);
    }
}