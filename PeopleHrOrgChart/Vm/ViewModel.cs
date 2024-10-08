using System;
using System.Linq;
using ObservableCollections;
using OrgChart.Core;
using R3;
using Serilog;

namespace PeopleHrOrgChart.Vm;

public class ViewModel
{
    public ViewModel()
    {
        var logger = Log.ForContext<ViewModel>();

        PersonRoot root;
        try
        {
            using var db = Db.Open();
            root = db.Latest();
        }
        catch (Exception e)
        {
            // TODO communicate this to the ui
            //      no data?
            //      bad data?
            //      something else?
            //      maybe let drag-and-drop json file onto app and save _that_?
            root = new PersonRoot();
            logger.Error(e, "Failed to fetch latest org chart from app data.");
        }

        var employees = root.Data?.EmployeeList ?? [];
        employees.Sort((a, b) => string.CompareOrdinal(a.JobRole, b.JobRole) switch
        {
            0 => -string.CompareOrdinal(a.Location, b.Location),
            var v => v
        });
        foreach (var person in employees)
        {
            person.DirectReports = employees.Where(x => x.ReportsTo == person.EmployeeId).ToList();
            person.Manager = employees.FirstOrDefault(x => x.EmployeeId == person.ReportsTo);
        }

        ObservableList<Person> peopleModels = new(employees);
        var view = peopleModels.CreateView(x => x);
        People = view.ToNotifyCollectionChanged(); // can't use AddRange with Slim

        TreeType = new(0);
        SearchText = new();

        DepartmentList = new(
            employees
                .Select(x => x.Department)
                .Order()
                .Distinct()
                .Prepend("Any department")
        );
        DepartmentSelected = new("Any department");

        LocationList = new(
            employees
                .Select(x => x.Location)
                .Order()
                .Distinct()
                .Prepend("Any location")
        );
        LocationSelected = new("Any location");

        Observable.Merge(
                TreeType.Select(_ => string.Empty),
                SearchText.Select(_ => string.Empty),
                DepartmentSelected,
                LocationSelected
            )
            .Subscribe(_ =>
            {
                view.AttachFilter(new Filter(
                    SearchText.Value,
                    TreeType.Value,
                    DepartmentSelected.Value,
                    LocationSelected.Value
                ));
            });
    }

    class Filter(string? term, int treeType, string department, string location) : ISynchronizedViewFilter<Person>
    {
        public bool IsMatch(Person person)
        {
            bool topDown = treeType != 0; // an enum would be more proper
            //                            // tho tbh if i wasn't using a listbox, this would be a bool

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

    public BindableReactiveProperty<int> TreeType { get; }
    public INotifyCollectionChangedSynchronizedViewList<Person> People { get; }
    public BindableReactiveProperty<string?> SearchText { get; }
}
