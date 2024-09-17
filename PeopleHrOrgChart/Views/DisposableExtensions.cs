using System;
using R3;

namespace PeopleHrOrgChart.Views;

public static class DisposableExtensions
{
    public static void DisposeWith(this IDisposable disposable, DisposableBag bag)
    {
        bag.Add(disposable);
    }

    public static void DisposeWith(this IDisposable disposable, ref IDisposable? areYouHappyWithYourLifeChoices)
    {
        areYouHappyWithYourLifeChoices = disposable;
    }
}
