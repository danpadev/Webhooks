﻿// Copyright 2022 Deveel
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;

using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Webhooks {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddWebhookReceivers(this IServiceCollection services, Action<IWebhookReceiverBuilder> configure = null) {


			if (configure != null) {
				var builder = new WebhookReceiverConfigurationBuilder(services);
				configure(builder);
			}

			return services;
		}

		class WebhookReceiverConfigurationBuilder : IWebhookReceiverBuilder {
			public WebhookReceiverConfigurationBuilder(IServiceCollection services) {
				Services = services;
			}

			public IServiceCollection Services { get; }

			public IWebhookReceiverBuilder ConfigureServices(Action<IServiceCollection> configure) {
				if (configure != null)
					configure(Services);

				return this;
			}
		}
	}
}
