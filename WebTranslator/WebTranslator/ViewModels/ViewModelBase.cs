using ReactiveUI;
using WebTranslator.Interfaces;

namespace WebTranslator.ViewModels;

public class ViewModelBase : ReactiveObject, IPageParameter
{
    public virtual void SetParameter(object? parameter)
    {
    }
}