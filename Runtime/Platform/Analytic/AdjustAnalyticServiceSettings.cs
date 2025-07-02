using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using com.ktgame.core;

#if UNITY_EDITOR
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
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
	public struct EventData
	{
		[Serializable]
		public struct Param
		{
			[SerializeField] private ValueType type;
			[SerializeField] private string key;

			public ValueType Type => type;

			public string Key => key;
		}

		[Serializable]
		public struct Event
		{
			[SerializeField] [FoldoutGroup("$name", expanded: false)] private string name;

			[SerializeField] [FoldoutGroup("$name", expanded: false)] private string id;

			[SerializeField] [FoldoutGroup("$name", expanded: false)] private List<Param> @params;

			public string Name => name;

			public string Id => id;

			public List<Param> Params => @params;
		}

		[SerializeField] private List<Event> events;

		public List<Event> Events => events;
	}

	public class AdjustAnalyticServiceSettings : ServiceSettingsSingleton<AdjustAnalyticServiceSettings>
	{
		public override string PackageName => GetType().Namespace;

		[SerializeField] private string appToken;
		[SerializeField] private Environment environment;
		[SerializeField] private int logLevel = 1;
		[SerializeField] private bool sendInBackground = true;
		[SerializeField] private bool launchDeferredDeeplink = false;
		[SerializeField] private EventData eventData;

		public string AppToken => appToken;

		public Environment Environment => environment;

		public int LogLevel => logLevel;
		
		public bool SendInBackground => sendInBackground;
		
		public bool LaunchDeferredDeeplink => launchDeferredDeeplink;

		public EventData EventData => eventData;

#if UNITY_EDITOR
		[Button("Generate Event")]
		private void GenerateEvent()
		{
			if (eventData.Events.Count <= 0)
				return;

			var builder = new StringBuilder();
			builder.Append("using com.ktgame.analytics.tracker;").Append("\n").Append("\n");
			builder.AppendFormat("namespace {0}", PackageName).Append("\n");
			builder.Append("{").Append("\n");
			foreach (var @event in eventData.Events)
			{
				builder.Append("\t").AppendFormat("public struct AdjustTracking_{0} : IEventData ", RemoveSpecialCharacters(@event.Name)).Append("\n");
				builder.Append("\t").Append("{").Append("\n");
				builder.Append("\t\t").AppendFormat("private const string EventName = \"{0}\"", @event.Id).Append(";").Append("\n");
				var paramString = "";
				for (var i = 0; i < @event.Params.Count; i++)
				{
					var param = @event.Params[i];
					switch (param.Type)
					{
						case ValueType.Int:
							paramString += $"int {param.Key.Trim()}";
							builder.Append("\t\t").AppendFormat("public int {0} ", param.Key.Trim()).Append("{ get; }").Append("\n");
							break;
						case ValueType.Float:
							paramString += $"float {param.Key.Trim()}";
							builder.Append("\t\t").AppendFormat("public float {0} ", param.Key.Trim()).Append("{ get; }").Append("\n");
							break;
						case ValueType.String:
							paramString += $"string {param.Key.Trim()}";
							builder.Append("\t\t").AppendFormat("public string {0} ", param.Key.Trim()).Append("{ get; }").Append("\n");
							break;
						case ValueType.Boolean:
							paramString += $"bool {param.Key.Trim()}";
							builder.Append("\t\t").AppendFormat("public bool {0} ", param.Key.Trim()).Append("{ get; }").Append("\n");
							break;
						default:
							paramString += $"string {param.Key.Trim()}";
							builder.Append("\t\t").AppendFormat("public string {0} ", param.Key.Trim()).Append("{ get; }").Append("\n");
							break;
					}

					if (i < @event.Params.Count - 1)
						paramString += ", ";
				}

				if (@event.Params.Count > 0)
				{
					builder.Append("\n");
					builder.Append("\t\t").AppendFormat("public AdjustTracking_{0}", @event.Name).Append("(").Append(paramString).Append(")").Append("\n");
					builder.Append("\t\t").Append("{").Append("\n");
					foreach (var param in @event.Params)
					{
						builder.Append("\t\t\t").AppendFormat("this.{0} = {1}", param.Key, param.Key).Append(";").Append("\n");
					}

					builder.Append("\t\t").Append("}").Append("\n");
				}

				builder.Append("\t").Append("}").Append("\n");
				builder.Append("\n");
			}

			builder.Append("}");
			var fileText = builder.ToString();

			var saveFolderPath = Path.Combine(Application.dataPath, "Scripts/Generated");
			var saveFilePath = Path.Combine(saveFolderPath, "AdjustTrackingGenerate.cs");

			if (!Directory.Exists(saveFolderPath))
			{
				Directory.CreateDirectory(saveFolderPath);
			}

			if (File.Exists(saveFilePath))
			{
				File.Delete(saveFilePath);
			}

			if (File.Exists(saveFilePath + ".meta"))
			{
				File.Delete(saveFilePath + ".meta");
			}

			File.WriteAllText(saveFilePath, fileText, Encoding.UTF8);
			AssetDatabase.ImportAsset(saveFilePath);
			AssetDatabase.Refresh();
		}

		private static string RemoveSpecialCharacters(string str)
		{
			return Regex.Replace(str, "[^a-zA-Z0-9_]+", "", RegexOptions.Compiled);
		}
#endif
	}
}
