using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace com.ktgame.analytics.tracker.editor
{
    public class EventImporter
    {
        private const string EventTrackingFileName = "EventTracking.json";
        private const string UserPropertyTrackingFileName = "UserPropertyTracking.json";
        private const string MachineLearningTrackingFileName = "MachineLearningTracking.json";
        private readonly string _inputPath;

        public EventImporter(string inputPath)
        {
            _inputPath = inputPath;
        }

        public bool ExistsEventTrackingFile()
        {
            return File.Exists(Path.Combine(_inputPath, EventTrackingFileName));
        }

        public bool ExistsUserPropertyTrackingFile()
        {
            return File.Exists(Path.Combine(_inputPath, UserPropertyTrackingFileName));
        }
        
        public bool ExistsMachineLearningTrackingFile()
        {
            return File.Exists(Path.Combine(_inputPath, MachineLearningTrackingFileName));
        }
        
        public DataEventTracking GetEventData()
        {
            var reader = new StreamReader(Path.Combine(_inputPath, EventTrackingFileName));
            var dataEventTracking = JsonUtility.FromJson<DataEventTracking>(reader.ReadToEnd());
            reader.Close();
            return ValidateEventDataTracking(dataEventTracking);
        }

        public DataPropertyTracking GetUserPropertiesData()
        {
            var reader = new StreamReader(Path.Combine(_inputPath, UserPropertyTrackingFileName));
            var dataPropertyTracking = JsonUtility.FromJson<DataPropertyTracking>(reader.ReadToEnd());
            reader.Close();
            return ValidateUserPropertiesTracking(dataPropertyTracking);
        }

        public DataEventMachineLearning GetMachineLearningEventData()
        {
            var reader = new StreamReader(Path.Combine(_inputPath, MachineLearningTrackingFileName));
            var dataEventMachineLearning = JsonUtility.FromJson<DataEventMachineLearning>(reader.ReadToEnd());
            reader.Close();
            return ValidateEventMachineLearningTracking(dataEventMachineLearning);
        }

        public DataEventTracking ValidateEventDataTracking(DataEventTracking dataTracking)
        {
            var validateDataEventTracking = new DataEventTracking() { events = new List<Event>() };

            foreach (var eventData in dataTracking.events)
            {
                if (validateDataEventTracking.events.Exists(e => e.eventName == eventData.eventName))
                {
                    var eventIndex = validateDataEventTracking.events.FindIndex(e => e.eventName == eventData.eventName);
                    validateDataEventTracking.events.RemoveAt(eventIndex);
                }

                var validateEvent = new Event()
                {
                    eventName = eventData.eventName.Trim(),
                    parameters = new List<Param>()
                };
                foreach (var paramData in eventData.parameters)
                {
                    var validateParamData = new Param
                    {
                        keyName = paramData.keyName.Trim(),
                        valueType = paramData.valueType.Trim()
                    };

                    validateEvent.parameters.Add(validateParamData);
                }

                validateDataEventTracking.events.Add(eventData);
            }

            return validateDataEventTracking;
        }

        public DataPropertyTracking ValidateUserPropertiesTracking(DataPropertyTracking dataTracking)
        {
            var validateDataPropertyTracking = new DataPropertyTracking() { properties = new List<UserProperty>() };

            foreach (var propertyData in dataTracking.properties)
            {
                if (validateDataPropertyTracking.properties.Exists(x => x.propertyName == propertyData.propertyName))
                {
                    var propertyIndex = validateDataPropertyTracking.properties.FindIndex(x => x.propertyName == propertyData.propertyName);
                    validateDataPropertyTracking.properties.RemoveAt(propertyIndex);
                }
                
                var validateUserProperty = new UserProperty
                {
                    propertyName = propertyData.propertyName.Trim()
                };
                validateDataPropertyTracking.properties.Add(validateUserProperty);
            }

            return validateDataPropertyTracking;
        }

        public DataEventMachineLearning ValidateEventMachineLearningTracking(DataEventMachineLearning dataTracking)
        {
            var validateDataEventMachineLearning = new DataEventMachineLearning() {events = new List<string>()};
            foreach (var eventData in dataTracking.events)
            {
                if (validateDataEventMachineLearning.events.Exists(x => x == eventData))
                {
                    var eventIndex = validateDataEventMachineLearning.events.FindIndex(x => x == eventData);
                    validateDataEventMachineLearning.events.RemoveAt(eventIndex);
                }
                
                validateDataEventMachineLearning.events.Add(eventData);
            }
            
            return validateDataEventMachineLearning;
        }
    }
}