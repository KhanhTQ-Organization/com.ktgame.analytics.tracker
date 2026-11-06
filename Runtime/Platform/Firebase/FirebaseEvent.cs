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

#if UNITY_EDITOR || DEVELOPMENT_BUILD
		
		private readonly List<string> _paramNames = new List<string>();
		private readonly List<string> _longStringParams = new List<string>();
		private static readonly HashSet<string> ReportedEvents = new HashSet<string>();
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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			_paramNames.Add(name);
			if (value != null && value.Length > FirebaseNameRules.MaxStringValueLength)
			{
				_longStringParams.Add(name);
			}
#endif
#if FIREBASE_ANALYTICS
			_parameters.Add(new Parameter(name, value));
#endif
			return this;
		}

		public IEvent AddIntParam(string name, int value)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			_paramNames.Add(name);
#endif
#if FIREBASE_ANALYTICS
			_parameters.Add(new Parameter(name, value));
#endif
			return this;
		}

		public IEvent AddLongParam(string name, long value)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			_paramNames.Add(name);
#endif
#if FIREBASE_ANALYTICS
			_parameters.Add(new Parameter(name, value));
#endif
			return this;
		}

		public IEvent AddFloatParam(string name, float value)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			_paramNames.Add(name);
#endif
#if FIREBASE_ANALYTICS
			_parameters.Add(new Parameter(name, value));
#endif
			return this;
		}

		public IEvent AddDoubleParam(string name, double value)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			_paramNames.Add(name);
#endif
#if FIREBASE_ANALYTICS
			_parameters.Add(new Parameter(name, value));
#endif
			return this;
		}

		public void Track()
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			Validate();
#endif
#if FIREBASE_ANALYTICS
			FirebaseAnalytics.LogEvent(_id, _parameters.ToArray());
#endif
		}

#if UNITY_EDITOR || DEVELOPMENT_BUILD
		private void Validate()
		{
			var problems = new List<string>();

			var eventError = FirebaseNameRules.ValidateEventName(_id);
			if (eventError != null)
			{
				problems.Add($"event name {eventError}");
			}

			if (_paramNames.Count > FirebaseNameRules.MaxParams)
			{
				problems.Add($"has {_paramNames.Count} params (max {FirebaseNameRules.MaxParams})");
			}

			foreach (var name in _paramNames)
			{
				var paramError = FirebaseNameRules.ValidateParamName(name);
				if (paramError != null)
				{
					problems.Add($"param \"{name}\" {paramError}");
				}
			}

			foreach (var name in _longStringParams)
			{
				problems.Add($"param \"{name}\" value is longer than {FirebaseNameRules.MaxStringValueLength} characters (will be truncated)");
			}

			if (problems.Count == 0)
			{
				return;
			}

			lock (ReportedEvents)
			{
				if (!ReportedEvents.Add(_id ?? string.Empty))
				{
					return;
				}
			}

			UnityEngine.Debug.LogWarning($"[Analytics] Event \"{_id}\" violates Firebase rules: {string.Join("; ", problems)}");
		}
#endif
	}
}
