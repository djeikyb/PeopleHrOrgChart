using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PeopleHrOrgChart.Vm;

// TODO fix null warnings by using json schema?
// https://github.com/json-everything/json-everything
// https://blog.json-everything.net/posts/deserialization-with-schemas/
// https://github.com/dotnet/runtime/issues/29887

public class PersonRoot
{
    [JsonPropertyName("Data")] public PersonData? Data { get; set; }
}

public class PersonData
{
    [JsonPropertyName("EmployeeList")] public List<Person>? EmployeeList { get; set; }
}

public class Person
{
    [JsonPropertyName("EmployeeId")] public int EmployeeId { get; set; }
    [JsonPropertyName("ReportsTo")] public int ReportsTo { get; set; }
    [JsonPropertyName("Picture")] public string Picture { get; set; }
    [JsonPropertyName("EmployeeName")] public string EmployeeName { get; set; }
    [JsonPropertyName("JobRole")] public string JobRole { get; set; }
    [JsonPropertyName("Company")] public string Company { get; set; }
    [JsonPropertyName("Location")] public string Location { get; set; }
    [JsonPropertyName("Department")] public string Department { get; set; }
    [JsonPropertyName("Length")] public string Length { get; set; }

    /// Populated in memory, not part of People HR dump
    [JsonIgnore] public List<Person> DirectReports { get; set; }

    /// Populated in memory, not part of People HR dump
    [JsonIgnore] public Person? Manager { get; set; }
}
