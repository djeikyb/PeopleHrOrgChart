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
        var path = "/Users/jacob/Downloads/org.json";
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
        TopDown.Subscribe(isTopDown =>
        {
            if (isTopDown)
            {
                logger.Information("Switched from bottom up to top down.");
                view.AttachFilter(x => x.DirectReports.Count != 0);
            }
            else
            {
                logger.Information("Switched from top down to bottom up.");
            }
        });

        SearchText = new();
        SearchText.Subscribe(term =>
        {
            view.AttachFilter(x =>
            {
                if (term is not { Length: > 0 }) return true;

                var parts = x.EmployeeName.Split(' ');
                foreach (var part in parts)
                {
                    if (part.StartsWith(term, StringComparison.InvariantCultureIgnoreCase)) return true;
                }

                return false;
            });
        });

        // TODO every AttachFilter removes the prior
        DepartmentList = new(root.Data.EmployeeList.Select(x => x.Department).Order().Distinct());
        DepartmentSelected = new();
        DepartmentSelected.Subscribe(department =>
        {
            if (department is { Length: > 0 }) view.AttachFilter(x => department.Equals(x.Department));
            else view.ResetFilter();
        });

        LocationList = new(root.Data.EmployeeList.Select(x => x.Location).Order().Distinct());
        LocationSelected = new();
        LocationSelected.Subscribe(location =>
        {
            if (location is { Length: > 0}) view.AttachFilter(x => location.Equals(x.Location));
            else view.ResetFilter();
        });
    }

    public BindableReactiveProperty<string> DepartmentSelected { get; set; }
    public ObservableList<string> DepartmentList { get; set; }

    public BindableReactiveProperty<string> LocationSelected { get; set; }
    public ObservableList<string> LocationList { get; set; }

    public BindableReactiveProperty<bool> TopDown { get; }
    public INotifyCollectionChangedSynchronizedViewList<Person> People { get; }
    public BindableReactiveProperty<string> SearchText { get; }
}
