using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace com.ktgame.analytics.tracker
{
	public class AnalyticTracker : IAnalyticTracker
	{
		public static bool LogEnabled { get; set; }

		private readonly ITrackingProvider _trackingProvider;

		public ITrackingProvider TrackingProvider => _trackingProvider;

		private sealed class EventMeta
		{
			public string EventName;
			public PropertyInfo[] Properties;
		}

		private sealed class UserPropertyMeta
		{
			public string PropertyName;
			public PropertyInfo ValueProperty;
		}

		private static readonly Dictionary<Type, EventMeta> EventMetas = new Dictionary<Type, EventMeta>();
		private static readonly Dictionary<Type, UserPropertyMeta> UserPropertyMetas = new Dictionary<Type, UserPropertyMeta>();
		private static readonly HashSet<Type> MissingNameReported = new HashSet<Type>();

		public AnalyticTracker(ITrackingProvider trackingProvider)
		{
			_trackingProvider = trackingProvider;
		}

		public virtual void LogEvent(IEventData e)
		{
			if (e == null || !_trackingProvider.IsReady)
			{
				return;
			}
			
			try
			{
				if (e is IEventDataWriter writer)
				{
					LogWriterEvent(writer);
				}
				else
				{
					LogReflectionEvent(e);
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		
		private void LogWriterEvent(IEventDataWriter writer)
		{
			var eventName = writer.GetEventName();
			if (string.IsNullOrEmpty(eventName))
			{
				ReportMissingEventName(writer.GetType());
				return;
			}

			var newEvent = _trackingProvider.NewEvent(eventName);
			if (newEvent == null)
			{
				return;
			}

			if (!LogEnabled)
			{
				writer.WriteParams(newEvent);
				newEvent.Track();
				return;
			}

			var loggingEvent = new LoggingEvent(newEvent);
			writer.WriteParams(loggingEvent);
			newEvent.Track();
			Debug.Log($"[Analytics] {eventName} ({loggingEvent.Log.ToString().TrimEnd(',', ' ')})");
		}
		
		private void LogReflectionEvent(IEventData e)
		{
			var type = e.GetType();
			var meta = GetEventMeta(type);
			if (string.IsNullOrEmpty(meta.EventName))
			{
				ReportMissingEventName(type);
				return;
			}

			var newEvent = _trackingProvider.NewEvent(meta.EventName);
			if (newEvent == null)
			{
				return;
			}

			var log = LogEnabled ? new StringBuilder() : null;

			foreach (var property in meta.Properties)
			{
				var value = property.GetValue(e);
				if (value == null)
				{
					continue;
				}

				var propertyName = property.Name;
				switch (value)
				{
					case int intValue:
						newEvent.AddIntParam(propertyName, intValue);
						break;
					case long longValue:
						newEvent.AddLongParam(propertyName, longValue);
						break;
					case float floatValue:
						newEvent.AddFloatParam(propertyName, floatValue);
						break;
					case double doubleValue:
						newEvent.AddDoubleParam(propertyName, doubleValue);
						break;
					case string stringValue:
						newEvent.AddStringParam(propertyName, stringValue);
						break;
					case bool boolValue:
						newEvent.AddStringParam(propertyName, boolValue ? "true" : "false");
						break;
					default:
						continue;
				}

				log?.Append(propertyName).Append('=').Append(value is bool b ? (b ? "true" : "false") : value).Append(", ");
			}

			newEvent.Track();

			if (log != null)
			{
				Debug.Log($"[Analytics] {meta.EventName} ({log.ToString().TrimEnd(',', ' ')})");
			}
		}

		private static void ReportMissingEventName(Type type)
		{
			bool firstTime;
			lock (MissingNameReported)
			{
				firstTime = MissingNameReported.Add(type);
			}

			if (firstTime)
			{
				Debug.LogError($"Missing [EventName] field to track for {type.FullName} type.");
			}
		}

		public void LogEvent(string eventName, params AnalyticParam[] parameters)
		{
			if (string.IsNullOrEmpty(eventName) || !_trackingProvider.IsReady)
			{
				return;
			}

			try
			{
				LogParamsEvent(eventName, parameters);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}

		private void LogParamsEvent(string eventName, AnalyticParam[] parameters)
		{
			var newEvent = _trackingProvider.NewEvent(eventName);
			if (newEvent == null)
			{
				return;
			}

			if (parameters != null)
			{
				foreach (var param in parameters)
				{
					if (string.IsNullOrEmpty(param.Name))
					{
						continue;
					}

					switch (param.Kind)
					{
						case AnalyticParam.ValueKind.Long:
							newEvent.AddLongParam(param.Name, param.LongValue);
							break;
						case AnalyticParam.ValueKind.Double:
							newEvent.AddDoubleParam(param.Name, param.DoubleValue);
							break;
						default:
							if (param.StringValue != null)
							{
								newEvent.AddStringParam(param.Name, param.StringValue);
							}

							break;
					}
				}
			}

			newEvent.Track();

			if (LogEnabled)
			{
				Debug.Log($"[Analytics] {eventName} ({(parameters == null ? string.Empty : string.Join(", ", parameters))})");
			}
		}

		public void SetUserProperty(IUserPropertyData userPropertyData)
		{
			if (userPropertyData == null || !_trackingProvider.IsReady)
			{
				return;
			}

			try
			{
				string propertyName;
				string propertyValue;

				if (userPropertyData is IUserPropertyDataWriter writer)
				{
					propertyName = writer.GetPropertyName();
					propertyValue = writer.GetPropertyValue();
				}
				else
				{
					var meta = GetUserPropertyMeta(userPropertyData.GetType());
					propertyName = meta.PropertyName;
					propertyValue = meta.ValueProperty?.GetValue(userPropertyData)?.ToString();
				}

				if (!string.IsNullOrEmpty(propertyName) && !string.IsNullOrEmpty(propertyValue))
				{
					_trackingProvider.SetUserProperty(propertyName, propertyValue);

					if (LogEnabled)
					{
						Debug.Log($"[Analytics] UserProperty {propertyName} = {propertyValue}");
					}
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}

		private static EventMeta GetEventMeta(Type type)
		{
			lock (EventMetas)
			{
				if (EventMetas.TryGetValue(type, out var meta))
				{
					return meta;
				}

				meta = new EventMeta
				{
					EventName = type.GetField("EventName", BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null)?.ToString(),
					Properties = type.GetProperties()
				};
				EventMetas[type] = meta;
				return meta;
			}
		}

		private static UserPropertyMeta GetUserPropertyMeta(Type type)
		{
			lock (UserPropertyMetas)
			{
				if (UserPropertyMetas.TryGetValue(type, out var meta))
				{
					return meta;
				}

				meta = new UserPropertyMeta
				{
					PropertyName = type.GetField("PropertyName", BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null)?.ToString(),
					ValueProperty = type.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance)
				};
				UserPropertyMetas[type] = meta;
				return meta;
			}
		}
		
		private sealed class LoggingEvent : IEvent
		{
			private readonly IEvent _inner;

			public readonly StringBuilder Log = new StringBuilder();

			public LoggingEvent(IEvent inner)
			{
				_inner = inner;
			}

			public IEvent AddStringParam(string name, string value)
			{
				_inner.AddStringParam(name, value);
				Log.Append(name).Append('=').Append(value).Append(", ");
				return this;
			}

			public IEvent AddIntParam(string name, int value)
			{
				_inner.AddIntParam(name, value);
				Log.Append(name).Append('=').Append((object)value).Append(", ");
				return this;
			}

			public IEvent AddLongParam(string name, long value)
			{
				_inner.AddLongParam(name, value);
				Log.Append(name).Append('=').Append((object)value).Append(", ");
				return this;
			}

			public IEvent AddFloatParam(string name, float value)
			{
				_inner.AddFloatParam(name, value);
				Log.Append(name).Append('=').Append((object)value).Append(", ");
				return this;
			}

			public IEvent AddDoubleParam(string name, double value)
			{
				_inner.AddDoubleParam(name, value);
				Log.Append(name).Append('=').Append((object)value).Append(", ");
				return this;
			}

			public void Track()
			{
				_inner.Track();
			}
		}
	}
}
