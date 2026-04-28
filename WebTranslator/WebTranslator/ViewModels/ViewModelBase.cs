using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WebTranslator.Interfaces;

namespace WebTranslator.ViewModels;

public class ViewModelBase : INotifyPropertyChanged, IPageParameter
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public virtual void SetParameter(object? parameter)
    {
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        RaisePropertyChanged(propertyName);
        return true;
    }

    protected void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
    {
        if (propertyName is null) return;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
