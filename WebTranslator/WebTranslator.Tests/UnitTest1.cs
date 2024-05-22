using WebTranslator.Models;
using Xunit.Abstractions;

namespace WebTranslator.Tests;

public class UnitTest1(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void TestJson5Parser()
    {
        var dict = new ModDictionary("1", "test", MinecraftVersion.Version1Dot16);
        var currentDirectory = Directory.GetCurrentDirectory();
        var originFilePath = Path.Combine(currentDirectory, "TestFiles", "original.json5");
        var translatedPath = Path.Combine(currentDirectory, "TestFiles", "translated.json");
        var originText = File.ReadAllText(originFilePath);
        var translatedText = File.ReadAllText(translatedPath);
        dict.LoadOriginalFile(originText);
        // _testOutputHelper.WriteLine(dict.OriginalTemplate);
        dict.LoadTranslatedFile(translatedText);

        foreach (var (key, value) in dict.TextDictionary)
            testOutputHelper.WriteLine(key + "\n" + value.ReplaceTemplate + "\n" + value.OriginalText + "\n" +
                                       value.TranslatedText + "\n\n");
        testOutputHelper.WriteLine(dict.ToString());
    }

    [Fact]
    public void TestLangParser()
    {
        var dict = new ModDictionary("2", "test", MinecraftVersion.Version1Dot12Dot2);
        var currentDirectory = Directory.GetCurrentDirectory();
        var originFilePath = Path.Combine(currentDirectory, "TestFiles", "original.lang");
        var translatedPath = Path.Combine(currentDirectory, "TestFiles", "translated.lang");
        var originText = File.ReadAllText(originFilePath);
        var translatedText = File.ReadAllText(translatedPath);
        dict.LoadOriginalFile(originText);
        // _testOutputHelper.WriteLine(dict.OriginalTemplate);
        dict.LoadTranslatedFile(translatedText);

        foreach (var (key, value) in dict.TextDictionary)
            testOutputHelper.WriteLine(key + "\n" + value.ReplaceTemplate + "\n" + value.OriginalText + "\n" +
                                       value.TranslatedText + "\n\n");
        testOutputHelper.WriteLine(dict.ToString());
    }

    [Fact]
    public void TestGithubConverter()
    {
        const string link1 =
            "https://github.com/CFPAOrg/Minecraft-Mod-Language-Package/tree/main/projects/1.20-fabric/assets/better-than-bunnies-fabric/betterthanbunnies/lang";
        const string link2 =
            "https://github.com/CFPAOrg/Minecraft-Mod-Language-Package/tree/1436a5721038715e8d0e5bac08446b98e0d83d1e/projects/1.20/assets/better-tips/bettertags/lang";
        testOutputHelper.WriteLine(GithubHelper.GithubConvert(link1));
        testOutputHelper.WriteLine(GithubHelper.GithubConvert(link2));
    }

    [Fact]
    public void TestGithubFiles()
    {
        const string GithubLink =
            "https://github.com/CFPAOrg/Minecraft-Mod-Language-Package/tree/main/projects/1.20-fabric/assets/better-than-bunnies-fabric/betterthanbunnies/lang";
        var link = GithubHelper.GithubConvert(GithubLink);
        var GithubFileInfos = GithubHelper.GetLanguageFilesAsync(link).Result;
        foreach (var githubFileInfo in GithubFileInfos) testOutputHelper.WriteLine(githubFileInfo.Content().Result);
    }
}