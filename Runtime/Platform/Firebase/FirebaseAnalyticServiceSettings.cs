using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using com.ktgame.core;

#if UNITY_EDITOR
using System.Text;
using System.Text.RegularExpressions;
#endif

namespace com.ktgame.analytics.tracker.firebase
{
	public enum ValueType
	{
		Int,
		Long,
		Float,
		Double,
		String,
		Bool
	}

	[Serializable]
	public class EventData
	{
		[Serializable]
		public struct Param
		{
			[SerializeField] private ValueType type;
			[SerializeField] private string key;

			public ValueType Type
			{
				get => type;
				set => type = value;
			}

			public string Key
			{
				get => key;
				set => key = value;
			}
		}

		[Serializable]
		public class Event
		{
			[SerializeField] [FoldoutGroup("$name", expanded: false)] private string name;

			[SerializeField] [FoldoutGroup("$name", expanded: false)] private List<Param> @params = new List<Param>();

			public string Name => name;
			public List<Param> Params => @params;
		}

		[SerializeField] private List<Event> events;

		public List<Event> Events
		{
			get => events ??= new List<Event>();
			set => events = value;
		}
	}

	[Serializable]
	public class UserPropertyData
	{
		[Serializable]
		public struct UserProperties
		{
			[SerializeField] private string name;
			public string Name => name;
		}

		[SerializeField] private List<UserProperties> properties = new List<UserProperties>();

		public List<UserProperties> Properties
		{
			get => properties ??= new List<UserProperties>();
			set => properties = value;
		}
	}

	public class FirebaseAnalyticServiceSettings : ServiceSettingsSingleton<FirebaseAnalyticServiceSettings>
	{
		public override string PackageName => GetType().Namespace;

		[SerializeField] private EventData eventData;

		[SerializeField] private UserPropertyData userPropertyData;

		[Tooltip("Log every event / user property to the Console for QA. Should be disabled in release builds.")]
		[SerializeField] private bool logEventsToConsole;

		public bool LogEventsToConsole
		{
			get => logEventsToConsole;
			set => logEventsToConsole = value;
		}

		public EventData EventData
		{
			get => eventData;
			set => eventData = value;
		}

		public UserPropertyData UserPropertyData
		{
			get => userPropertyData;
			set => userPropertyData = value;
		}

#if UNITY_EDITOR
		[Button("Generate Event")]
		public void GenerateEventEditor()
		{
			if (eventData == null || eventData.Events == null || eventData.Events.Count == 0)
				return;

			ValidateEventsInEditor();

			var builder = new StringBuilder();

			builder.AppendLine("using com.ktgame.analytics.tracker;");
			builder.AppendLine();
			builder.AppendLine($"namespace {PackageName}");
			builder.AppendLine("{");

			var hasError = false;
			var usedStructNames = new HashSet<string>();

			foreach (var e in eventData.Events)
			{
				if (string.IsNullOrWhiteSpace(e.Name))
					continue;

				var structName = $"FirebaseTracking_{Sanitize(e.Name)}";
				var parameters = new List<TrackingCodeGenerator.GeneratedParam>();

				foreach (var param in e.Params)
				{
					if (string.IsNullOrWhiteSpace(param.Key))
						continue;

					parameters.Add(new TrackingCodeGenerator.GeneratedParam
					{
						TypeName = TypeMap.TryGetValue(param.Type, out var mappedType) ? mappedType : "string",
						Name = Sanitize(param.Key)
					});
				}

				if (!TrackingCodeGenerator.ValidateEvent($"Event \"{e.Name}\"", structName, parameters, usedStructNames))
				{
					hasError = true;
					continue;
				}

				TrackingCodeGenerator.AppendEventStruct(builder, structName, e.Name, parameters);
			}

			builder.AppendLine("}");

			if (hasError)
			{
				Debug.LogError("[Analytics] Generate Firebase Event aborted due to the errors above. The existing file was kept.");
				return;
			}

			TrackingCodeGenerator.SaveToFile("FirebaseTrackingGenerate.cs", builder.ToString());
		}

		[Button("Generate User Property")]
		public void GenerateUserProperty()
		{
			if (userPropertyData == null || userPropertyData.Properties == null || userPropertyData.Properties.Count == 0)
			{
				Debug.LogWarning("User Property Data is empty. Nothing to generate.");
				return;
			}

			var builder = new StringBuilder();

			builder.AppendLine("using com.ktgame.analytics.tracker;");
			builder.AppendLine();
			builder.AppendLine($"namespace {PackageName}");
			builder.AppendLine("{");

			var hasError = false;
			var usedStructNames = new HashSet<string>();

			foreach (var property in userPropertyData.Properties)
			{
				if (string.IsNullOrWhiteSpace(property.Name))
					continue;

				var structName = $"FirebaseUserProperty_{Sanitize(property.Name)}";
				if (!usedStructNames.Add(structName))
				{
					Debug.LogError($"[Analytics] User property \"{property.Name}\": struct name \"{structName}\" duplicates another property.");
					hasError = true;
					continue;
				}

				TrackingCodeGenerator.AppendUserPropertyStruct(builder, structName, property.Name);
			}

			builder.AppendLine("}");

			if (hasError)
			{
				Debug.LogError("[Analytics] Generate Firebase User Property aborted due to the errors above. The existing file was kept.");
				return;
			}

			TrackingCodeGenerator.SaveToFile("FirebaseUserPropertyGenerate.cs", builder.ToString());
		}
		
		private void ValidateEventsInEditor()
		{
			foreach (var e in eventData.Events)
			{
				if (string.IsNullOrWhiteSpace(e.Name))
					continue;

				var eventError = FirebaseNameRules.ValidateEventName(e.Name);
				if (eventError != null)
					Debug.LogWarning($"[Analytics] Event \"{e.Name}\": name {eventError}.");

				if (e.Params.Count > FirebaseNameRules.MaxParams)
					Debug.LogWarning($"[Analytics] Event \"{e.Name}\": has {e.Params.Count} params (max {FirebaseNameRules.MaxParams}).");

				foreach (var param in e.Params)
				{
					if (string.IsNullOrWhiteSpace(param.Key))
						continue;
					
					var sentName = Sanitize(param.Key);
					var paramError = FirebaseNameRules.ValidateParamName(sentName);
					if (paramError != null)
						Debug.LogWarning($"[Analytics] Event \"{e.Name}\": param \"{sentName}\" {paramError}.");
				}
			}
		}

		private static readonly Dictionary<ValueType, string> TypeMap = new()
		{
			{ ValueType.Int, "int" },
			{ ValueType.Long, "long" },
			{ ValueType.Float, "float" },
			{ ValueType.Double, "double" },
			{ ValueType.String, "string" },
			{ ValueType.Bool, "bool" }
		};

		private static string Sanitize(string input)
		{
			if (string.IsNullOrWhiteSpace(input))
			{
				return "_invalid";
			}

			var result = input.Trim();

			result = result.Replace(" ", "_")
				.Replace("-", "_");
			
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
#endif
	}
}
