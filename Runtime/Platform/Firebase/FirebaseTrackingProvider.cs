#if FIREBASE_ANALYTICS
using Firebase.Analytics;
#endif

namespace com.ktgame.analytics.tracker.firebase
{
	public class FirebaseTrackingProvider : ITrackingProvider
	{
		public bool IsReady { private set; get; }

		public FirebaseTrackingProvider()
		{
			IsReady = true;
		}

		public ITrackingProvider SetUserId(string id)
		{
#if FIREBASE_ANALYTICS
			FirebaseAnalytics.SetUserId(id);
#endif
			return this;
		}

		public ITrackingProvider SetUserProperty(string id, string value)
		{
#if FIREBASE_ANALYTICS
			FirebaseAnalytics.SetUserProperty(id, value);
#endif
			return this;
		}

		public IEvent NewEvent(string id)
		{
			return new FirebaseEvent(id);
		}
	}
}
