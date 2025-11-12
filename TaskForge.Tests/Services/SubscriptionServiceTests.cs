using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskForge.Application.DTOs;
using TaskForge.Application.Services;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;
using TaskForge.Domain.Interfaces;
using TaskForge.Domain.Interfaces.Repository;
using Xunit;

namespace TaskForge.Tests.Services
{
    public class SubscriptionServiceTests
    {
        private readonly Mock<ISubscriptionRepository> _mockSubscriptionRepository;
        private readonly SubscriptionService _subscriptionService;

        public SubscriptionServiceTests()
        {
            _mockSubscriptionRepository = new Mock<ISubscriptionRepository>();
            _subscriptionService = new SubscriptionService(_mockSubscriptionRepository.Object);
        }

        [Fact]
        public async Task CreateSubscriptionAsync_ShouldCreateAndReturnSubscription()
        {
            // Arrange
            var name = "Netflix";
            var amount = 15.99m;
            var currency = Currency.USD;
            var date = DateTime.Today;
            var intervalValue = 1;
            var intervalUnit = IntervalUnit.Month;
            var userId = 1;
            var notify = true;

            var createdSub = new Subscription
            {
                Id = 1,
                Name = name,
                Amount = amount,
                UserId = userId,
                Currency = currency,
                BillingDate = date,
                Notify = notify,
                IntervalValue = intervalValue,
                IntervalUnit = intervalUnit
            };

            _mockSubscriptionRepository
                .Setup(repo => repo.CreateSubscriptionAsync(name, amount, currency, date, notify, intervalValue, intervalUnit, userId))
                .ReturnsAsync(createdSub);

            // Act
            var result = await _subscriptionService.CreateSubscriptionAsync(name, amount, currency, date, notify, intervalValue, intervalUnit, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(name, result.Name);
            Assert.Equal(userId, result.UserId);
            Assert.Equal(amount, result.Amount);
            _mockSubscriptionRepository.Verify(repo => repo.CreateSubscriptionAsync(name, amount, currency, date, notify, intervalValue, intervalUnit, userId), Times.Once);
        }

        [Fact]
        public async Task GetUserSubscriptionsAsync_ShouldReturnMappedDtos_WhenSubscriptionsExist()
        {
            // Arrange
            var userId = 3;
            var subscriptions = new List<Subscription>
            {
                new Subscription
                {
                    Id = 1,
                    Name = "Spotify",
                    Amount = 9.99m,
                    Currency = Currency.USD,
                    IntervalUnit = IntervalUnit.Month,
                    IntervalValue = 1,
                    BillingDate = DateTime.Today,
                    Notify = true,
                    UserId = userId
                },
                new Subscription
                {
                    Id = 2,
                    Name = "Adobe Cloud",
                    Amount = 52.99m,
                    Currency = Currency.EUR,
                    IntervalUnit = IntervalUnit.Year,
                    IntervalValue = 1,
                    BillingDate = DateTime.Today.AddDays(10),
                    Notify = false,
                    UserId = userId
                }
            };

            _mockSubscriptionRepository
                .Setup(repo => repo.GetSubscriptionsByUserIdAsync(userId))
                .ReturnsAsync(subscriptions);

            // Act
            var result = await _subscriptionService.GetUserSubscriptionsAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.IsType<List<SubscriptionRecordDto>>(result); 

            var first = result[0];
            Assert.Equal(1, first.Id);
            Assert.Equal("Spotify", first.Name);
            Assert.Equal("USD", first.Currency);
            Assert.Equal(IntervalUnit.Month, (IntervalUnit)Enum.Parse(typeof(IntervalUnit), first.IntervalUnit));

            _mockSubscriptionRepository.Verify(repo => repo.GetSubscriptionsByUserIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetUserSubscriptionsAsync_ShouldReturnEmptyList_WhenNoSubscriptions()
        {
            // Arrange
            var userId = 99;
            _mockSubscriptionRepository
                .Setup(repo => repo.GetSubscriptionsByUserIdAsync(userId))
                .ReturnsAsync(new List<Subscription>());

            // Act
            var result = await _subscriptionService.GetUserSubscriptionsAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockSubscriptionRepository.Verify(repo => repo.GetSubscriptionsByUserIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetSubscriptionByIdAsync_ShouldReturnSubscription_WhenExists()
        {
            // Arrange
            var subscriptionId = 10;
            var expectedSubscription = new Subscription
            {
                Id = subscriptionId,
                Name = "Test Sub",
                Amount = 50m
            };

            _mockSubscriptionRepository
                .Setup(repo => repo.GetSubscriptionByIdAsync(subscriptionId))
                .ReturnsAsync(expectedSubscription);

            // Act
            var result = await _subscriptionService.GetSubscriptionByIdAsync(subscriptionId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(subscriptionId, result.Id);
            Assert.Equal(expectedSubscription.Name, result.Name);
            _mockSubscriptionRepository.Verify(repo => repo.GetSubscriptionByIdAsync(subscriptionId), Times.Once);
        }

        [Fact]
        public async Task GetSubscriptionByIdAsync_ShouldReturnNull_WhenDoesNotExist()
        {
            // Arrange
            var subscriptionId = 999;
            _mockSubscriptionRepository
                .Setup(repo => repo.GetSubscriptionByIdAsync(subscriptionId))
                .ReturnsAsync((Subscription?)null);

            // Act
            var result = await _subscriptionService.GetSubscriptionByIdAsync(subscriptionId);

            // Assert
            Assert.Null(result);
            _mockSubscriptionRepository.Verify(repo => repo.GetSubscriptionByIdAsync(subscriptionId), Times.Once);
        }

        [Fact]
        public async Task DeleteSubscriptionAsync_ShouldReturnTrue_WhenExists()
        {
            // Arrange
            var subscriptionId = 7;
            _mockSubscriptionRepository
                .Setup(repo => repo.DeleteSubscriptionAsync(subscriptionId))
                .ReturnsAsync(true);

            // Act
            var result = await _subscriptionService.DeleteSubscriptionAsync(subscriptionId);

            // Assert
            Assert.True(result);
            _mockSubscriptionRepository.Verify(repo => repo.DeleteSubscriptionAsync(subscriptionId), Times.Once);
        }

        [Fact]
        public async Task DeleteSubscriptionAsync_ShouldReturnFalse_WhenDoesNotExist()
        {
            // Arrange
            var subscriptionId = 888;
            _mockSubscriptionRepository
                .Setup(repo => repo.DeleteSubscriptionAsync(subscriptionId))
                .ReturnsAsync(false);

            // Act
            var result = await _subscriptionService.DeleteSubscriptionAsync(subscriptionId);

            // Assert
            Assert.False(result);
            _mockSubscriptionRepository.Verify(repo => repo.DeleteSubscriptionAsync(subscriptionId), Times.Once);
        }

        [Fact]
        public async Task UpdateSubscriptionAsync_ShouldUpdateAndReturnSubscription()
        {
            // Arrange
            var updatedSubscription = new Subscription
            {
                Id = 4,
                Name = "Updated Name",
                Amount = 199.99m,
                Currency = Currency.USD,
                UserId = 2
            };

            _mockSubscriptionRepository
                .Setup(repo => repo.UpdateSubscriptionAsync(updatedSubscription))
                .ReturnsAsync(updatedSubscription);

            // Act
            var result = await _subscriptionService.UpdateSubscriptionAsync(updatedSubscription);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(updatedSubscription.Id, result.Id);
            Assert.Equal(199.99m, result.Amount);
            Assert.Equal("Updated Name", result.Name);
            _mockSubscriptionRepository.Verify(repo => repo.UpdateSubscriptionAsync(updatedSubscription), Times.Once);
        }
    }
}