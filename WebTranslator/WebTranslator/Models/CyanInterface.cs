using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WebTranslator.Models;

public static partial class CyanInterface
{
    public static async Task<ReviewPrMsg> ReviewPr(string prNumber)
    {
        var link = $"https://cfpa.cyan.cafe/api/CFPATools/PRRelation/{prNumber}";
        var json = await Utils.Request(link);
        var doc = JsonConvert.DeserializeObject<ReviewPrMsg>(json);
        return doc;
    }
}

public class ReviewPrMsg
{
    public int Number { get; set; }
    public List<ModInfo> ModList { get; set; } = new();
}

public class ModInfo
{
    public string Type { get; set; } = ""; // Curseforge
    public string Id { get; set; } = "";
    public string ModId { get; set; } = "";
    public string EnLink { get; set; } = "";
    public string ZhLink { get; set; } = "";
    public string Version { get; set; } = "";
    public List<OtherPrInfo> Other { get; set; } = new();
}

public class OtherPrInfo
{
    public int Number { get; set; }
    public string EnLink { get; set; } = "";
    public string ZhLink { get; set; } = "";
    public string Version { get; set; } = "";
}