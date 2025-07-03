using System;
using Cysharp.Threading.Tasks;
using com.ktgame.analytics.tracker;
using com.ktgame.core;
using UnityEngine;

#if ADJUST_ANALYTICS
using com.ktgame.core.di;
#endif

namespace com.ktgame.analytics.tracker.adjust
{
	[Service(typeof(IAdjustAnalyticService))]
	public class AdjustAnalyticService : MonoBehaviour, IAdjustAnalyticService
	{
		public int Priority => 0;
		public bool Initialized { get; set; }

		public IAnalyticTracker Tracker { get; private set; }

		public ITrackingProvider Provider { get; private set; }

		public string DeferredDeeplink { get; private set; }

		public Action<string> OnDeferredDeeplink { get; set; }

		public UniTask OnInitialize(IArchitecture architecture)
		{
			var settings = AdjustAnalyticServiceSettings.Instance;

#if ADJUST_ANALYTICS
			var environment = settings.Environment == Environment.Production
				? AdjustTrackingEnvironment.Production
				: AdjustTrackingEnvironment.Sandbox;

			var externalDeviceId = string.Empty;

			Provider = new AdjustTrackingProvider.Builder(settings.AppToken, environment)
				.WithLogLevel(settings.LogLevel)
				.WithSendInBackground(settings.SendInBackground)
				.WithLaunchDeferredDeeplink(settings.LaunchDeferredDeeplink)
				.WithDeferredDeeplinkCallbackId(OnDeferredDeeplinkHandler)
				.WithExternalDeviceId(externalDeviceId)
				.Build();
#else
			Provider = new NullTrackingProvider();
#endif

			Tracker = new AnalyticTracker(Provider);
			return UniTask.CompletedTask;
		}

		public void SetUserId(string id)
		{
			Provider?.SetUserId(id);
		}

		public void SetUserProperty(string id, string value)
		{
			Provider?.SetUserProperty(id, value);
		}

		public void LogEvent(IEventData eventData)
		{
			Tracker?.LogEvent(eventData);
		}

		private void OnDeferredDeeplinkHandler(string deeplink)
		{
			DeferredDeeplink = deeplink;
			OnDeferredDeeplink?.Invoke(deeplink);
		}
	}
}
