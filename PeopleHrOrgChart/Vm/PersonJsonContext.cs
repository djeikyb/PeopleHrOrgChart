using System.Text.Json.Serialization;

namespace PeopleHrOrgChart.Vm;

[JsonSerializable(typeof(PersonRoot))]
public partial class PersonJsonContext : JsonSerializerContext
{
}
