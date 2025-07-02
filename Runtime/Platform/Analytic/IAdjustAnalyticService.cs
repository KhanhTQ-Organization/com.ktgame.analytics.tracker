using System;
using com.ktgame.analytics.tracker;
using com.ktgame.core;

namespace com.ktgame.analytics.tracker.adjust
{
	public interface IAdjustAnalyticService : IService, IInitializable
	{
		IAnalyticTracker Tracker { get; }

		ITrackingProvider Provider { get; }

		string DeferredDeeplink { get; }

		Action<string> OnDeferredDeeplink { get; set; }

		void SetUserId(string id);

		void SetUserProperty(string id, string value);

		void LogEvent(IEventData eventData);
	}
}
