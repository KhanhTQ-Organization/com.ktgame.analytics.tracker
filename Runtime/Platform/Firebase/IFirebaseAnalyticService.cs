using com.ktgame.core;

namespace com.ktgame.analytics.tracker.firebase
{
	public interface IFirebaseAnalyticService : IService, IInitializable
	{
		void SetUserId(string id);

		void SetUserProperty(IUserPropertyData userPropertyData);

		void LogEvent(IEventData eventData);
	}
}
