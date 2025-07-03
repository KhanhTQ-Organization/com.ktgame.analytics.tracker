using System.Collections.Generic;

#if FIREBASE_ANALYTICS
using Firebase.Analytics;
#endif

namespace com.ktgame.analytics.tracker.firebase
{
	public class FirebaseEvent : IEvent
	{
		private readonly string _id;
#if FIREBASE_ANALYTICS
		private readonly List<Parameter> _parameters;
#endif

		public FirebaseEvent(string id)
		{
			_id = id;
#if FIREBASE_ANALYTICS
			_parameters = new List<Parameter>();
#endif
		}

		public IEvent AddStringParam(string name, string value)
		{
#if FIREBASE_ANALYTICS
			_parameters.Add(new Parameter(name, value));
#endif
			return this;
		}

		public IEvent AddIntParam(string name, int value)
		{
#if FIREBASE_ANALYTICS
			_parameters.Add(new Parameter(name, value));
#endif
			return this;
		}

		public IEvent AddLongParam(string name, long value)
		{
#if FIREBASE_ANALYTICS
			_parameters.Add(new Parameter(name, value));
#endif
			return this;
		}

		public IEvent AddFloatParam(string name, float value)
		{
#if FIREBASE_ANALYTICS
			_parameters.Add(new Parameter(name, value));
#endif
			return this;
		}

		public IEvent AddDoubleParam(string name, double value)
		{
#if FIREBASE_ANALYTICS
			_parameters.Add(new Parameter(name, value));
#endif
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
