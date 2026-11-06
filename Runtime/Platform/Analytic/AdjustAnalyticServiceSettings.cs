using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using com.ktgame.core;

#if ADJUST_ANALYTICS
using AdjustSdk;
#endif

#if UNITY_EDITOR
using System.Text;
using System.Text.RegularExpressions;
#endif

namespace com.ktgame.analytics.tracker.adjust
{
	public enum Environment
	{
		Sandbox,
		Production
	}

	public enum ValueType
	{
		Int,
		Float,
		String,
		Boolean,
	}

	[Serializable]
	public class EventData
	{
		[Serializable]
		public class Param
		{
			[SerializeField] private ValueType type;
			[SerializeField] private string key;

			public ValueType Type => type;
			public string Key => key;
		}

		[Serializable]
		public class Event
		{
			[SerializeField] [FoldoutGroup("$name", expanded: false)] private string name;

			[SerializeField] [FoldoutGroup("$name", expanded: false)] private string id;

			[SerializeField] [FoldoutGroup("$name", expanded: false)] private List<Param> @params = new();

			public string Name => name;
			public string Id => id;
			public List<Param> Params => @params ??= new List<Param>();
		}

		[SerializeField] private List<Event> events = new();

		public List<Event> Events
		{
			get => events ??= new List<Event>();
			set => events = value;
		}
	}

	public class AdjustAnalyticServiceSettings : ServiceSettingsSingleton<AdjustAnalyticServiceSettings>
	{
		public override string PackageName => GetType().Namespace;

		[SerializeField] private string appToken;
		[SerializeField] private Environment environment;

#if ADJUST_ANALYTICS
        [SerializeField] private AdjustLogLevel logLevel = AdjustLogLevel.Info;
#endif

		[SerializeField] private bool sendInBackground = true;
		[SerializeField] private bool launchDeferredDeeplink = false;
		[SerializeField] private EventData eventData = new();

		public string AppToken => appToken;
		public Environment Environment => environment;

#if ADJUST_ANALYTICS
        public AdjustLogLevel LogLevel => logLevel;
#endif

		public bool SendInBackground => sendInBackground;
		public bool LaunchDeferredDeeplink => launchDeferredDeeplink;
		public EventData EventData => eventData;

#if UNITY_EDITOR

		private static readonly Dictionary<ValueType, string> TypeMap = new()
		{
			{ ValueType.Int, "int" },
			{ ValueType.Float, "float" },
			{ ValueType.String, "string" },
			{ ValueType.Boolean, "bool" }
		};

		private static string Sanitize(string input)
		{
			if (string.IsNullOrWhiteSpace(input))
			{
				return "_invalid";
			}

			var result = input.Trim();
			result = Regex.Replace(result, "[^a-zA-Z0-9_]", "");

			if (string.IsNullOrEmpty(result))
			{
				return "_invalid";
			}

			if (char.IsDigit(result[0]))
			{
				result = "_" + result;
			}

			return result;
		}

		[Button("Generate Event")]
		public void GenerateEventEditor()
		{
			if (eventData == null || eventData.Events.Count == 0)
				return;

			var builder = new StringBuilder();

			builder.AppendLine("using com.ktgame.analytics.tracker;");
			builder.AppendLine();
			builder.AppendLine($"namespace {PackageName}");
			builder.AppendLine("{");

			var hasError = false;
			var usedStructNames = new HashSet<string>();

			foreach (var e in eventData.Events)
			{
				if (string.IsNullOrWhiteSpace(e.Name) || string.IsNullOrWhiteSpace(e.Id))
					continue;

				var structName = $"AdjustTracking_{Sanitize(e.Name)}";
				var parameters = new List<TrackingCodeGenerator.GeneratedParam>();

				foreach (var param in e.Params)
				{
					if (string.IsNullOrWhiteSpace(param.Key))
						continue;

					parameters.Add(new TrackingCodeGenerator.GeneratedParam
					{
						TypeName = TypeMap.TryGetValue(param.Type, out var mapped) ? mapped : "string",
						Name = Sanitize(param.Key)
					});
				}

				if (!TrackingCodeGenerator.ValidateEvent($"Adjust event \"{e.Name}\"", structName, parameters, usedStructNames))
				{
					hasError = true;
					continue;
				}

				TrackingCodeGenerator.AppendEventStruct(builder, structName, e.Id, parameters);
			}

			builder.AppendLine("}");

			if (hasError)
			{
				Debug.LogError("[Analytics] Generate Adjust Event aborted due to the errors above. The existing file was kept.");
				return;
			}

			TrackingCodeGenerator.SaveToFile("AdjustTrackingGenerate.cs", builder.ToString());
		}

#endif
	}
}
