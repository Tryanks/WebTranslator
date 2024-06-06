using System.Threading.Tasks;

namespace WebTranslator.Interfaces;

public interface IFileInfo
{
    string Name { get; }

    Task<string> String();
}