﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Deveel.Webhooks {
	public sealed class MongoDbWebhookSubscriptionFactory : IWebhookSubscriptionFactory {
		public IWebhookSubscription Create(WebhookSubscriptionInfo subscriptionInfo) {
			return new WebhookSubscriptionDocument {
				Name = subscriptionInfo.Name,
				EventTypes = subscriptionInfo.EventTypes?.ToList(),
				DestinationUrl = subscriptionInfo.DestinationUrl.ToString(),
				RetryCount = subscriptionInfo.RetryCount,
				Secret = subscriptionInfo.Secret,
				Status = subscriptionInfo.Active ? 
					WebhookSubscriptionStatus.Active : 
					WebhookSubscriptionStatus.None,
				LastStatusTime = subscriptionInfo.Active ? 
					DateTimeOffset.UtcNow : 
					DateTimeOffset.MinValue,
				Headers = subscriptionInfo.Headers != null
					? new Dictionary<string, string>(subscriptionInfo.Headers)
					: null,
				Filters = subscriptionInfo.Filters?.Select(MapFilter).ToList(),
				Metadata = subscriptionInfo.Metadata != null
					? new Dictionary<string, object>(subscriptionInfo.Metadata)
					: new Dictionary<string, object>()
			};
		}


		private WebhookFilterField MapFilter(IWebhookFilter filter)
			=> new WebhookFilterField {
				Expression = filter.Expression,
				Format = filter.Format
			};
	}
}
