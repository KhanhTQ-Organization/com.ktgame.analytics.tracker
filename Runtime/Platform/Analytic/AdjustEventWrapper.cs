#if ADJUST_ANALYTICS
using AdjustSdk;
using System.Collections.Generic;
#endif

using com.ktgame.analytics.tracker;

namespace com.ktgame.analytics.tracker.adjust
{
	public class AdjustEventWrapper : IEvent
	{
		private readonly string _eventToken;

#if ADJUST_ANALYTICS
		private readonly Dictionary<string, string> _parameters =
			new Dictionary<string, string>();
#endif

		public AdjustEventWrapper(string eventToken)
		{
			_eventToken = eventToken;
		}

		public IEvent AddStringParam(string name, string value)
		{
#if ADJUST_ANALYTICS
			_parameters[name] = value;
#endif
			return this;
		}

		public IEvent AddIntParam(string name, int value)
		{
#if ADJUST_ANALYTICS
			_parameters[name] = value.ToString();
#endif
			return this;
		}

		public IEvent AddLongParam(string name, long value)
		{
#if ADJUST_ANALYTICS
			_parameters[name] = value.ToString();
#endif
			return this;
		}

		public IEvent AddFloatParam(string name, float value)
		{
#if ADJUST_ANALYTICS
			_parameters[name] = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
#endif
			return this;
		}

		public IEvent AddDoubleParam(string name, double value)
		{
#if ADJUST_ANALYTICS
			_parameters[name] = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
#endif
			return this;
		}

		public void Track()
		{
#if ADJUST_ANALYTICS
			if (string.IsNullOrEmpty(_eventToken))
			{
				return;
			}

			var adjustEvent = new AdjustEvent(_eventToken);

			foreach (var param in _parameters)
			{
				adjustEvent.AddCallbackParameter(param.Key, param.Value);
			}

			Adjust.TrackEvent(adjustEvent);
#endif
		}
	}
}
