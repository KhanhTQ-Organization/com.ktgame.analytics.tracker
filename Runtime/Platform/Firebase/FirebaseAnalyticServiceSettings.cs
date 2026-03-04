using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using com.ktgame.core;

#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;
#endif

namespace com.ktgame.analytics.tracker.firebase
{
	public enum ValueType
	{
		Int,
		Long,
		Float,
		Double,
		String
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
		private void GenerateEvent()
		{
			if (eventData == null || eventData.Events == null || eventData.Events.Count == 0)
				return;

			var builder = new StringBuilder();

			builder.AppendLine("using com.ktgame.analytics.tracker;");
			builder.AppendLine();
			builder.AppendLine($"namespace {PackageName}");
			builder.AppendLine("{");

			foreach (var e in eventData.Events)
			{
				if (string.IsNullOrWhiteSpace(e.Name))
					continue;

				var eventName = Sanitize(e.Name);

				builder.AppendLine($"\tpublic struct FirebaseTracking_{eventName} : IEventData");
				builder.AppendLine("\t{");
				builder.AppendLine($"\t\tprivate const string EventName = \"{e.Name}\";");

				var paramDefs = new List<string>();

				foreach (var param in e.Params)
				{
					if (string.IsNullOrWhiteSpace(param.Key))
						continue;

					var key = Sanitize(param.Key);
					var type = TypeMap.TryGetValue(param.Type, out var mappedType)
						? mappedType
						: "string";

					paramDefs.Add($"{type} {key}");
					builder.AppendLine($"\t\tpublic {type} {key} {{ get; }}");
				}

				if (paramDefs.Count > 0)
				{
					builder.AppendLine();
					builder.AppendLine($"\t\tpublic FirebaseTracking_{eventName}({string.Join(", ", paramDefs)})");
					builder.AppendLine("\t\t{");

					foreach (var param in e.Params)
					{
						if (string.IsNullOrWhiteSpace(param.Key))
							continue;

						var key = Sanitize(param.Key);
						builder.AppendLine($"\t\t\tthis.{key} = {key};");
					}

					builder.AppendLine("\t\t}");
				}

				builder.AppendLine("\t}");
				builder.AppendLine();
			}

			builder.AppendLine("}");

			SaveToFile("FirebaseTrackingGenerate.cs", builder.ToString());
		}

		private void SaveToFile(string fileName, string content)
		{
			var folderPath = Path.Combine(Application.dataPath, "Scripts/Generated");
			var filePath = Path.Combine(folderPath, fileName);

			if (!Directory.Exists(folderPath))
			{
				Directory.CreateDirectory(folderPath);
			}

			if (File.Exists(filePath))
			{
				File.Delete(filePath);
			}

			if (File.Exists(filePath + ".meta"))
			{
				File.Delete(filePath + ".meta");
			}

			File.WriteAllText(filePath, content, Encoding.UTF8);

			AssetDatabase.ImportAsset(filePath);
			AssetDatabase.Refresh();
		}

		private static readonly Dictionary<ValueType, string> TypeMap = new()
		{
			{ ValueType.Int, "int" },
			{ ValueType.Long, "long" },
			{ ValueType.Float, "float" },
			{ ValueType.Double, "double" },
			{ ValueType.String, "string" }
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

			if (char.IsDigit(result[0]))
			{
				result = "_" + result;
			}

			return result;
		}
#endif
	}
}
