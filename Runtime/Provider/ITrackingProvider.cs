namespace com.ktgame.analytics.tracker
{
	public interface ITrackingProvider
	{
		bool IsReady { get; }
		ITrackingProvider SetUserId(string id);
		ITrackingProvider SetUserProperty(string id, string value);
		IEvent NewEvent(string id);
	}
}
