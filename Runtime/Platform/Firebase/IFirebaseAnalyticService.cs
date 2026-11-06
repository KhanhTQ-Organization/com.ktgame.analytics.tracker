using com.ktgame.core;

namespace com.ktgame.analytics.tracker.firebase
{
	public interface IFirebaseAnalyticService : IService, IInitializable
	{
		void SetUserId(string id);

		void SetUserProperty(IUserPropertyData userPropertyData);

		void LogEvent(IEventData eventData);
		
		void LogEvent(string eventName, params AnalyticParam[] parameters);
		
		void SetConsent(bool analyticsStorage, bool adStorage, bool adUserData, bool adPersonalization);
	}
}
