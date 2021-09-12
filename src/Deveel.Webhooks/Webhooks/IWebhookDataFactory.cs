﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace Deveel.Webhooks {
	public interface IWebhookDataFactory {
		bool AppliesTo(string eventType);

		Task<object> CreateDataAsync(EventInfo eventInfo, CancellationToken cancellationToken);
	}
}