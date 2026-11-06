namespace com.ktgame.analytics.tracker
{
	public interface IUserPropertyDataWriter : IUserPropertyData
	{
		string GetPropertyName();

		string GetPropertyValue();
	}
}
