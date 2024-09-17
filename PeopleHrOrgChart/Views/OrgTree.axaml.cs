using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using ObservableCollections;
using PeopleHrOrgChart.Vm;
using R3;
using Serilog;

namespace PeopleHrOrgChart.Views;

public partial class OrgTree : UserControl
{
    private IDisposable? OrgTreeSource = null;

    public OrgTree()
    {
        var _logger = Log.ForContext<OrgTree>();

        InitializeComponent();

        DataContextChanged += (sender, e) =>
        {
            var vm = ((OrgTree)sender).DataContext as ViewModel; // TODO this is gonna bite me
            vm.TopDown.Subscribe(isTopDown =>
            {
                // TODO extension dispose with
                // TODO dispose on DataContextChanged to avoid a memory leak

                _logger.Information("Disposed old tree grid because switching top down orientation.");
                OrgTreeSource?.Dispose();
                var source = BuildTable(vm.People, isTopDown);
                _logger.Information("Created new tree grid.");
                source.DisposeWith(ref OrgTreeSource);

                PersonTable.Source = source;
            });

            _logger.Information("Creating first tree grid!");
            vm.TopDown.ForceNotify();
        };
    }

    private static HierarchicalTreeDataGridSource<Person> BuildTable(IReadOnlyList<Person> people, bool isTopDown)
    {
        var personSource = new HierarchicalTreeDataGridSource<Person>(people)
        {
            Columns =
            {
                new HierarchicalExpanderColumn<Person>(
                    new TextColumn<Person, string>("Name", x => x.EmployeeName, width: GridLength.Auto),
                    x => isTopDown ? (x.DirectReports) : (x.Manager is null ? [] : [x.Manager])
                ),
                new TextColumn<Person, string>("Id", x => "" + x.EmployeeId, width: GridLength.Auto),
                new TextColumn<Person, string>("Department", x => x.Department, width: GridLength.Auto),
                new TextColumn<Person, string>("Location", x => x.Location, width: GridLength.Auto),
                new TextColumn<Person, string>("Title", x => x.JobRole, width: GridLength.Star),
            }
        };

        personSource.RowSelection!.SingleSelect = false;
        return personSource;
    }
}
