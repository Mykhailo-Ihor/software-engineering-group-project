using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;

namespace TaskForge.Domain.Interfaces.Repository
{
    public interface ISubscriptionRepository
    {
        Task<Subscription> CreateSubscriptionAsync(string name, decimal amount, Currency currency, DateTime billingDate, bool notify, int intervalValue, IntervalUnit intervalUnit, int userId);
        Task<List<Subscription>> GetSubscriptionsByUserIdAsync(int userId);
        Task<Subscription?> GetSubscriptionByIdAsync(int subscriptionId);
        Task<bool> DeleteSubscriptionAsync(int subscriptionId);
        Task<Subscription> UpdateSubscriptionAsync(Subscription subscription);
    }
}