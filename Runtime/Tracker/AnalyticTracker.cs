using System.Reflection;
using UnityEngine;

namespace com.ktgame.analytics.tracker
{
	public class AnalyticTracker : IAnalyticTracker
	{
		private readonly ITrackingProvider _trackingProvider;

		public ITrackingProvider TrackingProvider => _trackingProvider;

		public AnalyticTracker(ITrackingProvider trackingProvider)
		{
			_trackingProvider = trackingProvider;
		}

		public virtual void LogEvent(IEventData e)
		{
			if (!_trackingProvider.IsReady)
			{
				return;
			}

			var eventName = e.GetType().GetField("EventName", BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(e).ToString();
			if (!string.IsNullOrEmpty(eventName))
			{
				var newEvent = _trackingProvider.NewEvent(eventName);
				var properties = e.GetType().GetProperties();
				if (properties.Length > 0)
				{
					foreach (var property in properties)
					{
						if (property.GetValue(e) == null)
						{
							continue;
						}

						var propertyName = property.Name;
						if (property.GetValue(e) is int propertyIntValue)
						{
							newEvent.AddIntParam(propertyName, propertyIntValue);
						}
						else if (property.GetValue(e) is long propertyLongValue)
						{
							newEvent.AddLongParam(propertyName, propertyLongValue);
						}
						else if (property.GetValue(e) is float propertyFloatValue)
						{
							newEvent.AddFloatParam(propertyName, propertyFloatValue);
						}
						else if (property.GetValue(e) is double propertyDoubleValue)
						{
							newEvent.AddDoubleParam(propertyName, propertyDoubleValue);
						}
						else if (property.GetValue(e) is string propertyStringValue)
						{
							newEvent.AddStringParam(propertyName, propertyStringValue);
						}
					}
				}

				newEvent.Track();
			}
			else
			{
				Debug.LogError($"Missing [EventName] field to track for {e.GetType().FullName} type.");
			}
		}

		public void SetUserProperty(IUserPropertyData userPropertyData)
		{
			if (!_trackingProvider.IsReady)
			{
				return;
			}

			var propertyName = userPropertyData.GetType().GetField("PropertyName", BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(userPropertyData)
				.ToString();
			var propertyValue = userPropertyData.GetType().GetProperty("Value", BindingFlags.Public | BindingFlags.Instance)?.GetValue(userPropertyData)
				.ToString();
			if (!string.IsNullOrEmpty(propertyName) && !string.IsNullOrEmpty(propertyValue))
			{
				_trackingProvider.SetUserProperty(propertyName, propertyValue);
			}
		}
	}
}
