using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskForge.Application.DTOs;
using TaskForge.Application.Interfaces;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;
using TaskForge.Domain.Interfaces.Repository;

namespace TaskForge.Application.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ISubscriptionRepository _subscriptionRepository;

        public SubscriptionService(ISubscriptionRepository subscriptionRepository)
        {
            _subscriptionRepository = subscriptionRepository;
        }

        public Task<Subscription> CreateSubscriptionAsync(string name, decimal amount, Currency currency, DateTime billingDate, bool notify, int intervalValue, IntervalUnit intervalUnit, int userId)
        {
            return _subscriptionRepository.CreateSubscriptionAsync(name, amount, currency, billingDate, notify, intervalValue, intervalUnit, userId);
        }

        public async Task<List<SubscriptionRecordDto>> GetUserSubscriptionsAsync(int userId)
        {
            var subscriptions = await _subscriptionRepository.GetSubscriptionsByUserIdAsync(userId);

            return subscriptions.Select(s => new SubscriptionRecordDto
            {
                Id = s.Id,
                Name = s.Name,
                Amount = s.Amount,
                Currency = s.Currency.ToString(),
                BillingDate = s.BillingDate,
                Notify = s.Notify,
                IntervalValue = s.IntervalValue,
                IntervalUnit = s.IntervalUnit.ToString()
            }).ToList();
        }

        public Task<Subscription?> GetSubscriptionByIdAsync(int subscriptionId)
        {
            return _subscriptionRepository.GetSubscriptionByIdAsync(subscriptionId);
        }

        public Task<bool> DeleteSubscriptionAsync(int subscriptionId)
        {
            return _subscriptionRepository.DeleteSubscriptionAsync(subscriptionId);
        }

        public Task<Subscription> UpdateSubscriptionAsync(Subscription subscription)
        {
            return _subscriptionRepository.UpdateSubscriptionAsync(subscription);
        }
    }
}