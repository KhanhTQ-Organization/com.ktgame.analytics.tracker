namespace com.ktgame.analytics.tracker
{
	public readonly struct AnalyticParam
	{
		public enum ValueKind
		{
			String,
			Long,
			Double
		}

		public readonly string Name;
		public readonly ValueKind Kind;
		public readonly string StringValue;
		public readonly long LongValue;
		public readonly double DoubleValue;

		private AnalyticParam(string name, ValueKind kind, string stringValue, long longValue, double doubleValue)
		{
			Name = name;
			Kind = kind;
			StringValue = stringValue;
			LongValue = longValue;
			DoubleValue = doubleValue;
		}

		public static AnalyticParam Of(string name, string value) => new AnalyticParam(name, ValueKind.String, value, 0, 0);

		public static AnalyticParam Of(string name, int value) => new AnalyticParam(name, ValueKind.Long, null, value, 0);

		public static AnalyticParam Of(string name, long value) => new AnalyticParam(name, ValueKind.Long, null, value, 0);

		public static AnalyticParam Of(string name, float value) => new AnalyticParam(name, ValueKind.Double, null, 0, value);

		public static AnalyticParam Of(string name, double value) => new AnalyticParam(name, ValueKind.Double, null, 0, value);
		
		public static AnalyticParam Of(string name, bool value) => new AnalyticParam(name, ValueKind.String, value ? "true" : "false", 0, 0);

		public override string ToString()
		{
			switch (Kind)
			{
				case ValueKind.Long: return $"{Name}={LongValue}";
				case ValueKind.Double: return $"{Name}={DoubleValue}";
				default: return $"{Name}={StringValue}";
			}
		}
	}
}
