using Microsoft.EntityFrameworkCore;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;
using TaskForge.Domain.Interfaces;
using TaskForge.Domain.Interfaces.Repository;
using TaskForge.Infrastructure.Data;

namespace TaskForge.Infrastructure.Repositories
{
    public class SubscriptionRepository: ISubscriptionRepository
    {
        private readonly TaskForgeDbContext _context;

        public SubscriptionRepository(TaskForgeDbContext context)
        {
            _context = context;
        }

        public async Task<Subscription> CreateSubscriptionAsync(string name, decimal amount, Currency currency, DateTime billingDate, bool notify, int intervalValue, IntervalUnit intervalUnit, int userId)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                throw new InvalidOperationException($"Користувача з ID {userId} не знайдено.");
            }

            var subscription = new Subscription
            {
                Name = name,
                Amount = amount,
                Currency = currency,
                BillingDate = billingDate,
                Notify = notify,
                IntervalValue = intervalValue,
                IntervalUnit = intervalUnit,
                UserId = userId
            };

            await _context.Subscriptions.AddAsync(subscription);
            await _context.SaveChangesAsync();

            return subscription;
        }

        public async Task<List<Subscription>> GetSubscriptionsByUserIdAsync(int userId)
        {
            return await _context.Subscriptions
                .Where(s => s.UserId == userId)
                .OrderBy(s => s.BillingDate)
                .ToListAsync();
        }

        public async Task<Subscription?> GetSubscriptionByIdAsync(int subscriptionId)
        {
            return await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.Id == subscriptionId);
        }

        public async Task<bool> DeleteSubscriptionAsync(int subscriptionId)
        {
            var subscription = await _context.Subscriptions.FindAsync(subscriptionId);

            if (subscription == null)
            {
                return false;
            }

            _context.Subscriptions.Remove(subscription);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<Subscription> UpdateSubscriptionAsync(Subscription subscription)
        {
            _context.Subscriptions.Update(subscription);
            await _context.SaveChangesAsync();
            return subscription;
        }
    }
}