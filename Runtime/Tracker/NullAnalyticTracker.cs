namespace com.ktgame.analytics.tracker
{
	public class NullAnalyticTracker : IAnalyticTracker
	{
		public void LogEvent(IEventData eventData) { }

		public void SetUserProperty(IUserPropertyData property) { }
	}
}
