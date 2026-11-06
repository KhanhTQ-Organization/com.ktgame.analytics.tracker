namespace com.ktgame.analytics.tracker
{
	public class NullTrackingProvider : ITrackingProvider
	{
		public bool IsReady { private set; get; }

		public NullTrackingProvider()
		{
			IsReady = false;
		}
		
		public NullTrackingProvider(bool isReady)
		{
			IsReady = isReady;
		}

		public ITrackingProvider SetUserId(string id)
		{
			return this;
		}

		public ITrackingProvider SetUserProperty(string id, string value)
		{
			return this;
		}

		public IEvent NewEvent(string id)
		{
			return new NullEvent();
		}
	}
}
