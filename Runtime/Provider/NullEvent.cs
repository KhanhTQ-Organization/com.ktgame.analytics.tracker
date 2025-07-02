namespace com.ktgame.analytics.tracker
{
	public class NullEvent : IEvent
	{
		public IEvent AddStringParam(string name, string value)
		{
			return this;
		}

		public IEvent AddIntParam(string name, int value)
		{
			return this;
		}

		public IEvent AddLongParam(string name, long value)
		{
			return this;
		}

		public IEvent AddFloatParam(string name, float value)
		{
			return this;
		}

		public IEvent AddDoubleParam(string name, double value)
		{
			return this;
		}

		public void Track() { }
	}
}
