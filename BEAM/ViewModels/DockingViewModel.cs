using System;
using System.Collections.ObjectModel;
using BEAM.Docking;
using BEAM.ImageSequence;
using BEAM.ViewModels;
using CommunityToolkit.Mvvm.Input;

namespace BEAM.ViewModels;

public partial class DockingViewModel : ViewModelBase
{
    public ObservableCollection<IDockBase> Items = [];

    /// <summary>
    /// Opens a new dock window with the matching view to the model
    /// That means that the viewModel has to be placed inside the "ViewModels", and the corresponding view inside the "View" package
    /// </summary>
    /// <param name="viewModel">The ViewModel to open as a dock</param>
    public void OpenDock(IDockBase viewModel)
    {
        Items.Add(viewModel);
    }

    [RelayCommand]
    public void Foo()
    {
        OpenDock(new InspectionViewModel());
    }


    [RelayCommand]
    public void OpenSequenceView(ISequence sequence)
    {
        OpenDock(new SequenceViewModel(sequence));
    }
}