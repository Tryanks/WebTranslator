using System.Collections.Generic;
using System.IO;
using System.Xml;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;

namespace WebTranslator.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }
    
    private void GridLoaded(object? sender, RoutedEventArgs e)
    {
        var xshdText = """
<?xml version="1.0"?>
<SyntaxDefinition name="MCTranslator" xmlns="http://icsharpcode.net/sharpdevelop/syntaxdefinition/2008">

	<Color name="Color0" foreground="#FF000000" background=""/>
	<Color name="Color1" foreground="#FF0000AA" background=""/>
	<Color name="Color2" foreground="#FF00AA00" background=""/>
	<Color name="Color3" foreground="#FF00AAAA" background=""/>
	<Color name="Color4" foreground="#FFAA0000" background=""/>
	<Color name="Color5" foreground="#FFAA00AA" background=""/>
	<Color name="Color6" foreground="#FFFFAA00" background=""/>
	<Color name="Color7" foreground="#FFAAAAAA" background=""/>
	<Color name="Color8" foreground="#FF555555" background=""/>
	<Color name="Color9" foreground="#FF5555FF" background=""/>
	<Color name="Colora" foreground="#FF55FF55" background=""/>
	<Color name="Colorb" foreground="#FF55FFFF" background=""/>
	<Color name="Colorc" foreground="#FFFF5555" background=""/>
	<Color name="Colord" foreground="#FFFF55FF" background=""/>
	<Color name="Colore" foreground="#FFFFFF55" background=""/>
	<Color name="Colorf" foreground="#FFFFFFFF" background=""/>

	<Color name="Colorl" fontWeight="bold"/>
	<Color name="Coloro" fontStyle="italic"/>

	<Color name="Format" fontWeight="bold" foreground="#FFAA00AA"/>

	<RuleSet>
		<Rule color="Color0">§0(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
		<Rule color="Color1">§1(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
		<Rule color="Color2">§2(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
		<Rule color="Color3">§3(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
		<Rule color="Color4">§4(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
		<Rule color="Color5">§5(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
		<Rule color="Color6">§6(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
		<Rule color="Color7">§7(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
		<Rule color="Color8">§8(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
		<Rule color="Color9">§9(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
		<Rule color="Colora">§a(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
		<Rule color="Colorb">§b(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
		<Rule color="Colorc">§c(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
		<Rule color="Colord">§d(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
		<Rule color="Colore">§e(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
		<Rule color="Colorf">§f(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
		
		<Rule color="Colorl">§l(.*?)(?=(§[0-9a-f])|$|§r)</Rule>
		<Rule color="Coloro">§o(.*?)(?=(§[0-9a-f])|$|§r)</Rule>

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