using System.Text.Json.Serialization;

namespace OrgChart.Core;

[JsonSerializable(typeof(PersonRoot))]
public partial class PersonJsonContext : JsonSerializerContext
{
}
