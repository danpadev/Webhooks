﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Deveel.Util;
using Deveel.Webhooks;
using Deveel.Webhooks.Storage;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

using Mongo2Go;

using MongoDB.Bson;

using Xunit;

namespace Deveel.Webhooks {
	[Trait("Category", "Webhooks")]
	[Trait("Category", "Subscriptions")]
	public class WebhookManagementTests : IDisposable {
		private readonly MongoDbRunner mongoDbCluster;
		
		private readonly string tenantId = Guid.NewGuid().ToString("N");
		private readonly string userId = Guid.NewGuid().ToString("N");

		private readonly IWebhookSubscriptionManager webhookManager;
		private readonly IWebhookSubscriptionStoreProvider storeProvider;

		private readonly IWebhookSubscriptionFactory subscriptionFactory;

		public WebhookManagementTests() {
			mongoDbCluster = MongoDbRunner.Start(logger: NullLogger.Instance);

			var services = new ServiceCollection();
			services.AddWebhooks(buidler => {
				buidler.ConfigureDelivery(options =>
					options.SignWebhooks())
				.UseSubscriptionManager()
				.UseMongoDb(options => {
					options.SubscriptionsCollectionName = "webhooks_subscription";
					options.DatabaseName = "webhooks";
					options.ConnectionString = mongoDbCluster.ConnectionString;
				})
				.AddDynamicLinqFilterEvaluator();
			})
				.AddTestHttpClient();

			var provider = services.BuildServiceProvider();

			webhookManager = provider.GetService<IWebhookSubscriptionManager>();
			storeProvider = provider.GetRequiredService<IWebhookSubscriptionStoreProvider>();
			subscriptionFactory = provider.GetRequiredService<IWebhookSubscriptionFactory>();
		}

		private async Task<string> CreateSubscription(WebhookSubscriptionInfo subscriptionInfo) {
			var subscription = subscriptionFactory.Create(subscriptionInfo);
			return await storeProvider.CreateAsync(tenantId, subscription);
		}

		private async Task<IWebhookSubscription> GetSubscription(string subscriptionId)
			=> await storeProvider.GetByIdAsync(tenantId, subscriptionId);

		[Fact]
		public async Task AddSubscription() {
			var subscriptionId = await webhookManager.AddSubscriptionAsync(tenantId, Guid.NewGuid().ToString("N"), new WebhookSubscriptionInfo("test.event", "https://callback.test.io/webhook"), CancellationToken.None);

			Assert.NotNull(subscriptionId);
			var subscription = await storeProvider.GetByIdAsync(tenantId, subscriptionId);

			Assert.NotNull(subscription);
			Assert.Contains("test.event", subscription.EventTypes);
		}

		[Fact]
		public async Task RemoveExistingSubscription() {
			var subscriptionId = await CreateSubscription(new WebhookSubscriptionInfo("test.event", "https://callback.test.io/webhook") {
				Name = "Test Callback"
			});

			var result = await webhookManager.RemoveSubscriptionAsync(tenantId, Guid.NewGuid().ToString("N"), subscriptionId, default);

			Assert.True(result);

			var subscription = await GetSubscription(subscriptionId);
			Assert.Null(subscription);
		}

		[Fact]
		public async Task RemoveNotExistingSubscription() {
			var subscriptionId = ObjectId.GenerateNewId().ToString();

			await Assert.ThrowsAsync<SubscriptionNotFoundException>(() => webhookManager.RemoveSubscriptionAsync(tenantId, Guid.NewGuid().ToString("N"), subscriptionId, default));
		}


		[Fact]
		public async Task GetExistingSubscription() {
			var subscriptionId = await CreateSubscription(new WebhookSubscriptionInfo("test.event", "https://callback.test.io/webhook") {
				Filter = WebhookFilter.WildcardFilter ,
				Name = "Test Callback"
			});

			var subscription = await webhookManager.GetSubscriptionAsync(tenantId, subscriptionId, CancellationToken.None);

			Assert.NotNull(subscription);
			Assert.Equal(subscriptionId, subscription.SubscriptionId);
			Assert.Contains("test.event", subscription.EventTypes);
			Assert.NotNull(subscription.Filters);
			Assert.NotEmpty(subscription.Filters);
			Assert.Single(subscription.Filters);
			Assert.True(subscription.Filters.First().IsWildcard());
		}

		[Fact]
		public async Task GetNotExistingSubscription() {
			var subscriptionId = ObjectId.GenerateNewId().ToString();
			var subscription = await webhookManager.GetSubscriptionAsync(tenantId, subscriptionId, CancellationToken.None);

			Assert.Null(subscription);
		}

		[Fact]
		public async Task GetPageOfSubscriptions() {
			var subscriptionId1 = await CreateSubscription(new WebhookSubscriptionInfo("test.event", "https://callback.test.io/webhook") {
				Filter =  WebhookFilter.WildcardFilter,
				Name = "Test Callback"
			});

			var subscriptionId2 = await CreateSubscription(new WebhookSubscriptionInfo("test.otherEvent", "https://callback.test.io/webhook") {
				Filter = WebhookFilter.WildcardFilter,
				Name = "Second Test Callback"
			});

			var query = new WebhookSubscriptionQuery<IWebhookSubscription>(1, 10);
			var result = await webhookManager.GetSubscriptionsAsync(tenantId, query, default);

			Assert.NotNull(result);
			Assert.NotEmpty(result.Subscriptions);
			Assert.Equal(2, result.TotalCount);
			Assert.Equal(1, result.TotalPages);
			Assert.Equal(subscriptionId1, result.Subscriptions.ElementAt(0).SubscriptionId);
			Assert.Equal(subscriptionId2, result.Subscriptions.ElementAt(1).SubscriptionId);
		}

		[Fact]
		public async Task ActivateExistingSubscription() {
			var subscriptionId = await CreateSubscription(new WebhookSubscriptionInfo("test.event", "https://callback.test.io/webhook") {
				Name = "Test Callback",
				Active = false
			});

			var result = await webhookManager.EnableSubscriptionAsync(tenantId, Guid.NewGuid().ToString("N"), subscriptionId, default);

			Assert.True(result);

			var subscription = await GetSubscription(subscriptionId);

			Assert.NotNull(subscription);
			Assert.Equal(WebhookSubscriptionStatus.Active, subscription.Status);
		}

		[Fact]
		public async Task ActivateNotExistingSubscription() {
			var subscriptionId = ObjectId.GenerateNewId().ToString();

			await Assert.ThrowsAsync<SubscriptionNotFoundException>(() => webhookManager.EnableSubscriptionAsync(tenantId, Guid.NewGuid().ToString("N"), subscriptionId, default));
		}

		[Fact]
		public async Task ActivateAlreadyActiveSubscription() {
			var subscriptionId = await CreateSubscription(new WebhookSubscriptionInfo("test.event", "https://callback.test.io/webhook") {
				Name = "Test Callback",
				Active = true
			});

			var result = await webhookManager.EnableSubscriptionAsync(tenantId, Guid.NewGuid().ToString("N"), subscriptionId, default);

			Assert.False(result);

			var subscription = await GetSubscription(subscriptionId);

			Assert.NotNull(subscription);
			Assert.Equal(WebhookSubscriptionStatus.Active, subscription.Status);
		}

		[Fact]
		public async Task DisableExistingSubscription() {
			var subscriptionId = await CreateSubscription(new WebhookSubscriptionInfo("test.event", "https://callback.test.io/webhook") {
				Name = "Test Callback",
				Active = true
			});

			var result = await webhookManager.DisableSubscriptionAsync(tenantId, Guid.NewGuid().ToString("N"), subscriptionId, default);

			Assert.True(result);

			var subscription = await GetSubscription(subscriptionId);

			Assert.NotNull(subscription);
			Assert.Equal(WebhookSubscriptionStatus.Suspended, subscription.Status);
		}

		[Fact]
		public async Task DisableNoyExistingSubscription() {
			var subscriptionId = ObjectId.GenerateNewId().ToString();

			await Assert.ThrowsAsync<SubscriptionNotFoundException>(() => webhookManager.DisableSubscriptionAsync(tenantId, Guid.NewGuid().ToString("N"), subscriptionId, default));

			Assert.True(result);

			var subscription = await GetSubscription(subscriptionId);

			Assert.NotNull(subscription);
			Assert.Equal(WebhookSubscriptionStatus.Suspended, subscription.Status);
		}




		public void Dispose() {
			mongoDbCluster?.Dispose();
		}
	}
}
