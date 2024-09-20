using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
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

        DataContextChanged += (sender, _) =>
        {
            var vm = ((OrgTree)sender).DataContext as ViewModel; // TODO this is gonna bite me
            if (vm is null) return;
            vm.TreeType.Subscribe(tt =>
            {
                // TODO extension dispose with
                // TODO dispose on DataContextChanged to avoid a memory leak

                var isTopDown = tt != 0;

                _logger.Information("Disposed old tree grid because switching top down orientation.");
                OrgTreeSource?.Dispose();
                var source = BuildTable(vm.People, isTopDown);
                _logger.Information("Created new tree grid.");
                source.DisposeWith(ref OrgTreeSource);

                PersonTable.Source = source;
                PersonTable.DoubleTapped += (_, e) =>
                {
                    var found = PersonTable.GetInputElementsAt(e.GetPosition(PersonTable));
                    var cells = (TreeDataGridCellsPresenter?)found.FirstOrDefault(ie => ie is TreeDataGridCellsPresenter);

                    // ignore if it wasn't a row that wasn't tapped
                    // iunno. maybe it was a column divider?
                    if (cells is null) return;

                    var row = PersonTable.Rows!.RowIndexToModelIndex(cells.RowIndex);
                    source.Expand(row);
                };
            });

            _logger.Information("Creating first tree grid!");
            vm.TreeType.ForceNotify();
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
                    x => isTopDown
                        ? (x.DirectReports)
                        : (x.Manager is null
                            ? []
                            : [x.Manager])
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
