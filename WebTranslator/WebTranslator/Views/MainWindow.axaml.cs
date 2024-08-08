using System;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Media;

namespace WebTranslator.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        if (IsWindows11()) Background = Brushes.Transparent;
        return;

        bool IsWindows11()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return false;
            return Environment.OSVersion.Version is { Major: >= 10, Build: >= 22000 };
        }
    }
}