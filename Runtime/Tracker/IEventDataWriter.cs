namespace com.ktgame.analytics.tracker
{
	public interface IEventDataWriter : IEventData
	{
		string GetEventName();

		void WriteParams(IEvent analyticEvent);
	}
}
