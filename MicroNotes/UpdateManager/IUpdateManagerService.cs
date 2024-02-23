using System.Threading.Tasks;

namespace MicroNotes.UpdateManager;

public interface IUpdateManagerService
{
    Task TryUpdate();
}