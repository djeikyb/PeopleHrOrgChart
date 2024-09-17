using Avalonia.Controls;
using Avalonia.Platform;

namespace PeopleHrOrgChart;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        // Main.ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.PreferSystemChrome |
        //                                    ExtendClientAreaChromeHints.OSXThickTitleBar;
        // Main.ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.PreferSystemChrome;
        // Main.ExtendClientAreaTitleBarHeightHint = 100;
    }
}
