using System.Text.Json.Serialization;
using System.Text.Json;

namespace BoardingSimulationV3.Classes
{
    internal class FamilyMemberPassenger
    {
        public int PassengerID { get; set; } = 0;

        public int Row { get; set; }
        public string SeatLetter { get; set; }
        
        public int OverheadBinSlot { get; set; } = 0;

        // true as default as most passengers have carry on luggage
        // we don't need to transmit that unless they don't have carry on 
        // https://chatgpt.com/g/g-n7Rs0IK86-grimoire/c/75e30218-403a-4058-be60-4418558d53c5 
        //[JsonConverter(typeof(ConditionalBoolPropertyConverterFactory))] 
        public bool HasCarryOn { get; set; } = true;

        public int Age { get; set; }
        public bool IsLuggageHandler { get; set; } = false;

        public bool IsMinor => Age is > 0 and < 18;
        public bool IsSmallChild => Age is > 0 and < 7;

        public int BackTracks { get; set; }


    }

    public class ConditionalBoolPropertyConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert) =>
            typeToConvert == typeof(bool);

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            return new ConditionalBoolPropertyConverter();
        }

        private class ConditionalBoolPropertyConverter : JsonConverter<bool>
        {
            public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
                reader.GetBoolean();

            public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
            {
                // Only write the value if it is false
                if (!value)
                {
                    writer.WriteBooleanValue(value);
                }
            }
        }
    }
}
 
