using System.Threading.Tasks;

namespace AccelByte.Extend.Tisane.Plugin.Services
{
    public interface ITisaneService
    {
        Task<(bool IsProfane, string Message)> ParseAsync(string content);
    }
}
