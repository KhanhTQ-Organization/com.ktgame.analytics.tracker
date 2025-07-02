namespace com.ktgame.analytics.tracker
{
	public interface IAnalyticTracker
	{
		void LogEvent(IEventData eventData);

		void SetUserProperty(IUserPropertyData userPropertyData);
	}
}
