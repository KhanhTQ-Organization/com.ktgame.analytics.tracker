using System.Text;

namespace com.ktgame.analytics.tracker.editor
{
    public class EventGenerator
    {
        private const string NameSpace = "com.ktgame.analytics.tracker";
        private readonly EventImporter _importer;
        private readonly EventExporter _exporter;

        public EventGenerator(EventImporter importer, EventExporter exporter)
        {
            _importer = importer;
            _exporter = exporter;
        }

        public void GenerateStructEvent()
        {
            if (!_importer.ExistsEventTrackingFile()) return;
            var eventTrackingData = _importer.GetEventData();

            var builder = new StringBuilder();
            builder.AppendFormat("namespace {0} {1}", NameSpace, "{").Append("\n");

            foreach (var eventData in eventTrackingData.events)
            {
                builder.Append("\t").AppendFormat("public class Event_{0} : IEventData ", eventData.eventName).Append("{").Append("\n");
                builder.Append("\t\t").AppendFormat("private const string EventName = {0}", eventData.eventName).Append(";").Append("\n");
                foreach (var propertyData in eventData.parameters)
                {
                    switch (propertyData.valueType)
                    {
                        case "int":
                        case "Int":
                        case "INT":
                            builder.Append("\t\t").AppendFormat("public int {0} ", propertyData.keyName).Append("{ get; set }").Append("\n");
                            break;
                        case "float":
                        case "Float":
                        case "FLOAT":
                            builder.Append("\t\t").AppendFormat("public float {0} ", propertyData.keyName).Append("{ get; set }").Append("\n");
                            break;
                        case "string":
                        case "String":
                        case "STRING":
                            builder.Append("\t\t").AppendFormat("public string {0} ", propertyData.keyName).Append("{ get; set }").Append("\n");
                            break;
                        case "bool":
                        case "Bool":
                        case "BOOL":
                            builder.Append("\t\t").AppendFormat("public bool {0} ", propertyData.keyName).Append("{ get; set }").Append("\n");
                            break;
                        default:
                            builder.Append("\t\t").AppendFormat("public string {0} ", propertyData.keyName).Append("{ get; set }").Append("\n");
                            break;
                    }
                }

                builder.Append("\t").Append("}").Append("\n");
            }

            builder.Append("}");

            _exporter.ExportEvent(builder.ToString());
        }

        public void GenerateStructUserProperties()
        {
            if (!_importer.ExistsUserPropertyTrackingFile()) return;
        }

        public void GenerateStructMachineLearningEvent()
        {
            if (!_importer.ExistsMachineLearningTrackingFile()) return;
        }
    }
}