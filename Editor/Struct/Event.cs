using System.Collections.Generic;

namespace com.ktgame.analytics.tracker.editor
{
	[System.Serializable]
	public struct Event
	{
		public string eventName;
		public List<Param> parameters;
	}
}
