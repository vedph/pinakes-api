using System.Threading.Tasks;

namespace Pinix.Cli.Commands
{
    public interface ICommand
    {
        Task Run();
    }
}
