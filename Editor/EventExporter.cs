using System.IO;

namespace com.ktgame.analytics.tracker.editor
{
	public class EventExporter
	{
		private const string EventTrackingScript = "EventTrackingGenerate.cs";
		private const string UserPropertyTrackingScript = "UserPropertyTrackingGenerate.cs";
		private const string MachineLearningTrackingScript = "MachineLearningTrackingGenerate.cs";
		private readonly string _outputPath;

		public EventExporter(string outputPath)
		{
			_outputPath = outputPath;
		}

		public void ExportEvent(string content)
		{
			if (!Directory.Exists(_outputPath))
			{
				Directory.CreateDirectory(_outputPath);
			}

			WriteStringToFile(Path.Combine(_outputPath, EventTrackingScript), content);
		}

		public void ExportUserProperties(string content)
		{
			if (!Directory.Exists(_outputPath))
			{
				Directory.CreateDirectory(_outputPath);
			}

			WriteStringToFile(Path.Combine(_outputPath, UserPropertyTrackingScript), content);
		}

		public void ExportMachineLearningEvent(string content)
		{
			if (!Directory.Exists(_outputPath))
			{
				Directory.CreateDirectory(_outputPath);
			}

			WriteStringToFile(Path.Combine(_outputPath, MachineLearningTrackingScript), content);
		}

		private static void WriteStringToFile(string path, string content)
		{
			var writer = new StreamWriter(path);
			writer.WriteLine(content);
			writer.Close();
		}
	}
}
