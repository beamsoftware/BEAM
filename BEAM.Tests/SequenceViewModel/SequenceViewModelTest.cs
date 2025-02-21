using System.Collections.ObjectModel;
using Avalonia.Rendering;
using BEAM.ViewModels;
using BEAM.Datatypes;
using BEAM.Docking;
using BEAM.ImageSequence;
using Moq;
using Xunit;

namespace BEAM.Tests.SequenceViewModel;

public class SequenceViewModelTest
{
    [Fact]
    public void SequenceViewModel_Initialization_Test()
    {
        // Arrange
        var mockSequence = new Mock<ISequence>();
        var dockingVm = new DockingViewModel();

        // Act
        var viewModel = new ViewModels.SequenceViewModel(mockSequence.Object, dockingVm);

        // Assert
        Assert.NotNull(viewModel.Sequence);
        Assert.Equal(dockingVm, viewModel.DockingVm);
        Assert.NotNull(viewModel.Renderers);
        Assert.NotEmpty(viewModel.Renderers);
        Assert.Equal(viewModel.RendererSelection, viewModel.Renderers.Length > 1 ? 1 : 0);
    }
    
    
}