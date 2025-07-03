using System.Collections.Generic;

#if FIREBASE_ANALYTICS
using Firebase.Analytics;
#endif

namespace com.ktgame.analytics.tracker.firebase
{
	public class FirebaseEvent : IEvent
	{
		private readonly string _id;
		private readonly List<Parameter> _parameters;

		public FirebaseEvent(string id)
		{
			_id = id;
			_parameters = new List<Parameter>();
		}

		public IEvent AddStringParam(string name, string value)
		{
			_parameters.Add(new Parameter(name, value));
			return this;
		}

		public IEvent AddIntParam(string name, int value)
		{
			_parameters.Add(new Parameter(name, value));
			return this;
		}

		public IEvent AddLongParam(string name, long value)
		{
			_parameters.Add(new Parameter(name, value));
			return this;
		}

		public IEvent AddFloatParam(string name, float value)
		{
			_parameters.Add(new Parameter(name, value));
			return this;
		}

		public IEvent AddDoubleParam(string name, double value)
		{
			_parameters.Add(new Parameter(name, value));
			return this;
		}

		public void Track()
		{
#if FIREBASE_ANALYTICS
			FirebaseAnalytics.LogEvent(_id, _parameters.ToArray());
#endif
		}
	}
}
