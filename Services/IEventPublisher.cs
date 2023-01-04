using CloudNotes.Models;
using System.Threading.Tasks;

namespace CloudNotes.Services
{
    public interface IEventPublisher
    {
        Task PublishEvent(Event eventData);
    }
}