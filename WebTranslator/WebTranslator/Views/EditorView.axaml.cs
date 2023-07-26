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
    
    private void GridLoaded(object? sender, RoutedEventArgs e)
    {
        var xshdText = """
<?xml version="1.0"?>
<SyntaxDefinition name="MCTranslator" xmlns="http://icsharpcode.net/sharpdevelop/syntaxdefinition/2008">

	<Color name="Color_0" foreground="#FF000000" background=""/>
	<Color name="Color_1" foreground="#FF0000AA" background=""/>
	<Color name="Color_2" foreground="#FF00AA00" background=""/>
	<Color name="Color_3" foreground="#FF00AAAA" background=""/>
	<Color name="Color_4" foreground="#FFAA0000" background=""/>
	<Color name="Color_5" foreground="#FFAA00AA" background=""/>
	<Color name="Color_6" foreground="#FFFFAA00" background=""/>
	<Color name="Color_7" foreground="#FFAAAAAA" background=""/>
	<Color name="Color_8" foreground="#FF555555" background=""/>
	<Color name="Color_9" foreground="#FF5555FF" background=""/>
	<Color name="Color_a" foreground="#FF55FF55" background=""/>
	<Color name="Color_b" foreground="#FF55FFFF" background=""/>
	<Color name="Color_c" foreground="#FFFF5555" background=""/>
	<Color name="Color_d" foreground="#FFFF55FF" background=""/>
	<Color name="Color_e" foreground="#FFFFFF55" background=""/>
	<Color name="Color_f" foreground="#FFFFFFFF" background=""/>

	<Color name="Color_l" fontWeight="bold"/>
	<Color name="Color_o" fontStyle="italic"/>

	<Color name="Format" fontWeight="bold" foreground="#FFAA00AA"/>

	<RuleSet>
		<Rule color="Color_0">§0(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
		<Rule color="Color_1">§1(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
		<Rule color="Color_2">§2(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
		<Rule color="Color_3">§3(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
		<Rule color="Color_4">§4(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
		<Rule color="Color_5">§5(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
		<Rule color="Color_6">§6(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
		<Rule color="Color_7">§7(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
		<Rule color="Color_8">§8(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
		<Rule color="Color_9">§9(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
		<Rule color="Color_a">§a(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
		<Rule color="Color_b">§b(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
		<Rule color="Color_c">§c(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
		<Rule color="Color_d">§d(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
		<Rule color="Color_e">§e(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
		<Rule color="Color_f">§f(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
		
		<Rule color="Color_l">§l(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
		<Rule color="Color_o">§o(.*?)(?=(§[0-9a-f])|$|§r)</Rule>

		<Rule color="Format">%(\d+\$)?[a-hs%]</Rule>
	</RuleSet>

</SyntaxDefinition>
""";
        var reader = new XmlTextReader(new StringReader(xshdText));
        var xshd = HighlightingLoader.LoadXshd(reader);
        reader.Close();
        SyntaxEditor1.SyntaxHighlighting = HighlightingLoader.Load(xshd, HighlightingManager.Instance);
        SyntaxEditor2.SyntaxHighlighting = HighlightingLoader.Load(xshd, HighlightingManager.Instance);
    }
}