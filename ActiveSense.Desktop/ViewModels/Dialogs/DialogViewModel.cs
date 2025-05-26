using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ActiveSense.Desktop.ViewModels.Dialogs;

public partial class DialogViewModel : ViewModelBase
{
    private TaskCompletionSource _closeTask = new();
    [ObservableProperty] private bool _isDialogOpen;

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