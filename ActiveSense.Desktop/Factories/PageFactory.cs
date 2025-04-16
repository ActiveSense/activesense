using System;
using ActiveSense.Desktop.Data;
using ActiveSense.Desktop.ViewModels;

namespace ActiveSense.Desktop.Factories;

public class PageFactory(Func<ApplicationPageNames, PageViewModel> factory)
{
    public PageViewModel GetPageViewModel(ApplicationPageNames pageName) => factory.Invoke(pageName);
}