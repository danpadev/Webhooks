# Webhooks

This project provides a set of .NET tools for the management of subscriptions to events, basic transformations and notifications of such event occurrences (_[webhooks](docs/concept_webhook.md)_): in a global design scope, this model enables event-driven architectures, triggering system processes upon the occurrence of expected occurrences from other systems.

Although this integration model is widely adopted by major service providers (like _[SendGrid](https://docs.sendgrid.com/for-developers/tracking-events/getting-started-event-webhook)_, _[Twilio](https://www.twilio.com/docs/usage/webhooks)_, _[GitHub](https://docs.github.com/en/developers/webhooks-and-events/webhooks/about-webhooks)_, _[Slack](https://api.slack.com/messaging/webhooks)_, etc.), there is no formal protocol or authority that would enforce a compliance (like for other cases, such as OpenID, OpenAPI, etc.).

Anyway, a typical implementation consists of the following elements:

* Webhooks are transported through _HTTP POST_ callbacks
* The webhook payload is represented as a JSON object (or alternatively as XML or Form)
* The webhook payload includes properties that describe the type of event and the time-stamp of the occurrence
* An optional signature in the header of the request or a query-string parameter ensures the authenticity of the caller

I tried to express the concepts in more details in [this page](docs/concept_webhook.md) within this repository (without any ambition to be pedagogic).

## Motivation

While working on a .NET Core 3.1/.NET 5 *aaS (_as-a-Service_) project that functionally required the capability of users of the service being able to create system-to-system subscriptions and notifications of events through HTTP channel (that is typically named _webhooks_, or _HTTP callbacks_), I started my design with the ambition to use existing solutions, to avoid the bad practice of _reinventing the wheel_, but I ended up frustrated in such ambition:

* [Microsoft's ASP.NET Webhooks](https://github.com/aspnet/WebHooks) project was archived and moved back to the [Microsoft ASP Labs](https://github.com/aspnet/AspLabs/tree/main/src/WebHooks) (that has no visibility on its release), aiming one day to provide compatibility with .NET Core (which eventually evolved, becoming LTS)
* Both Microsoft's projects (the _legacy_ and the _experimental_ ones) are not compatible with the latest .NET stacks (_.NET 5_ / _.NET 6_)
* Microsoft's _experimental_ projects never implemented any capability of handling subscriptions, and eventually removing also the _sender_ capability, focusing exclusively on _receivers_
* Alternative implementations providing similar capabilities are embedded and organic part of larger frameworks (like [ASP.NET Boilerplate](https://github.com/aspnetboilerplate/aspnetboilerplate)), that would have forced me to adopt the the entirety of such frameworks, beyond my design intentions

## Usage Documentation

We would like to help you getting started with this framework and to eventually extend it: please refer to the [Documentation](docs/README.md) section that we have produced for you.
