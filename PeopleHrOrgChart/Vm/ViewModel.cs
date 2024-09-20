using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using ObservableCollections;
using R3;
using Serilog;

namespace PeopleHrOrgChart.Vm;

public class ViewModel
{
    public ViewModel()
    {
        var logger = Log.ForContext<ViewModel>();
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var path = Path.Combine(home, "peoplehr.json");
        const int defaultBufferSize = 4096;
        var stream = new FileStream(
            path: path,
            mode: FileMode.Open,
            access: FileAccess.Read,
            share: FileShare.Read,
            bufferSize: defaultBufferSize * 2,
            options: FileOptions.SequentialScan
        );
        var root = JsonSerializer.Deserialize<PersonRoot>(stream, PersonJsonContext.Default.PersonRoot);

        foreach (var person in root.Data.EmployeeList)
        {
            person.DirectReports = root.Data.EmployeeList.Where(x => x.ReportsTo == person.EmployeeId).ToList();
            person.Manager = root.Data.EmployeeList.FirstOrDefault(x => x.EmployeeId == person.ReportsTo);
        }

        ObservableList<Person> peopleModels = new(root.Data.EmployeeList);
        var view = peopleModels.CreateView(x => x);
        People = view.ToNotifyCollectionChanged(); // can't use AddRange with Slim

        TopDown = new(false);
        SearchText = new();

        DepartmentList = new(
            root.Data.EmployeeList
                .Select(x => x.Department)
                .Order()
                .Distinct()
                .Prepend("Any department")
        );
        DepartmentSelected = new("Any department");

        LocationList = new(
            root.Data.EmployeeList
                .Select(x => x.Location)
                .Order()
                .Distinct()
                .Prepend("Any location")
        );
        LocationSelected = new("Any location");

        Observable.Merge(
                TopDown.Select(_ => string.Empty),
                SearchText.Select(_ => string.Empty),
                DepartmentSelected,
                LocationSelected
            )
            .Subscribe(_ =>
            {
                view.AttachFilter(new Filter(
                    SearchText.Value,
                    TopDown.Value,
                    DepartmentSelected.Value,
                    LocationSelected.Value
                ));
            });
    }

    class Filter(string? term, bool topDown, string department, string location) : ISynchronizedViewFilter<Person>
    {
        public bool IsMatch(Person person)
        {
            bool td = !topDown || topDown && person.DirectReports.Count > 0;
            bool d = department is "Any department" || department.Equals(person.Department);
            bool l = location is "Any location" || location.Equals(person.Location);

            bool t = false;
            if (term?.Trim() is { Length: > 0 })
            {
                foreach (var part in person.EmployeeName.Split(' '))
                {
                    if (part.StartsWith(term, StringComparison.InvariantCultureIgnoreCase))
                    {
                        t = true;
                    }
                }
            }
            else
            {
                t = true;
            }

            return td && d && l && t;
        }
    }

    public BindableReactiveProperty<string> DepartmentSelected { get; set; }
    public ObservableList<string> DepartmentList { get; set; }

    public BindableReactiveProperty<string> LocationSelected { get; set; }
    public ObservableList<string> LocationList { get; set; }

    public BindableReactiveProperty<bool> TopDown { get; }
    public INotifyCollectionChangedSynchronizedViewList<Person> People { get; }
    public BindableReactiveProperty<string?> SearchText { get; }
}
