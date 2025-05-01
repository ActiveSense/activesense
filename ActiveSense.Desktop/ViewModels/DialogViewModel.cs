using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ActiveSense.Desktop.ViewModels;

public partial class DialogViewModel : ViewModelBase
{
    [ObservableProperty] private bool _isDialogOpen;

    private TaskCompletionSource _closeTask = new();

    public async Task WaitAsync()
    {
        await _closeTask.Task;
    }

    public void Show()
    {
        if (_closeTask.Task.IsCompleted)
            _closeTask = new TaskCompletionSource();

        IsDialogOpen = true;
    }

    protected void Close()
    {
        IsDialogOpen = false;

        _closeTask.TrySetResult();
    }
}