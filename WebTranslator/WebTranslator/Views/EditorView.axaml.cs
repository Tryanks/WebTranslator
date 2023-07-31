using System.IO;
using System.Xml;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;

namespace WebTranslator.Views;

public partial class EditorView : UserControl
{
    public EditorView()
    {
        InitializeComponent();
    }
}