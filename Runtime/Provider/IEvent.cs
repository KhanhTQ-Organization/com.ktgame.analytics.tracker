namespace com.ktgame.analytics.tracker
{
	public interface IEvent
	{
		IEvent AddStringParam(string name, string value);
		IEvent AddIntParam(string name, int value);
		IEvent AddLongParam(string name, long value);
		IEvent AddFloatParam(string name, float value);
		IEvent AddDoubleParam(string name, double value);
		void Track();
	}
}
