using System;
using ActiveSense.Desktop.Enums;
using ActiveSense.Desktop.ViewModels;

namespace ActiveSense.Desktop.Factories;

public class PageFactory(Func<ApplicationPageNames, PageViewModel> factory)
{
    public PageViewModel GetPageViewModel(ApplicationPageNames pageName)
    {
        return factory.Invoke(pageName);
    }
}