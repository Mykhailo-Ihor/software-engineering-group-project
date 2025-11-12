using System.Threading.Tasks;
using TaskForge.Domain.Entities;

namespace TaskForge.Application.Interfaces
{
    public interface ICalendarService
    {
        Task CreateOrUpdateAppointmentAsync(Subscription subscription);

        Task DeleteAppointmentAsync(int subscriptionId);
    }
}