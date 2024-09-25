using System;
using System.Collections.Generic;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace Merviche.FontAwesome;

public enum FaIcon
{
    Sitemap,
    Foo,
}

public enum FaFamily
{
    Regular,
}

public class Icon(FaIcon icon, FaFamily family) : MarkupExtension
{
    public override Geometry ProvideValue(IServiceProvider serviceProvider)
    {
        // null seems to not cause exceptions, despite not matching type signature
        // but maybe it would be better to use a sentinal value?
        // maybe the runtime exception happens but is discarded?
        // or sent to System.Trace?
        return _cache.GetValueOrDefault((icon, family))!;
    }

    private static readonly Dictionary<(FaIcon, FaFamily), PathGeometry> _cache = new()
    {
        {
            (FaIcon.Sitemap, FaFamily.Regular), PathGeometry.Parse(
                "M208 80c0-26.5 21.5-48 48-48l64 0c26.5 0 48 21.5 48 48l0 64c0 26.5-21.5 48-48 48l-8 0 0 40 152 0c30.9 0 56 25.1 56 56l0 32 8 0c26.5 0 48 21.5 48 48l0 64c0 26.5-21.5 48-48 48l-64 0c-26.5 0-48-21.5-48-48l0-64c0-26.5 21.5-48 48-48l8 0 0-32c0-4.4-3.6-8-8-8l-152 0 0 40 8 0c26.5 0 48 21.5 48 48l0 64c0 26.5-21.5 48-48 48l-64 0c-26.5 0-48-21.5-48-48l0-64c0-26.5 21.5-48 48-48l8 0 0-40-152 0c-4.4 0-8 3.6-8 8l0 32 8 0c26.5 0 48 21.5 48 48l0 64c0 26.5-21.5 48-48 48l-64 0c-26.5 0-48-21.5-48-48l0-64c0-26.5 21.5-48 48-48l8 0 0-32c0-30.9 25.1-56 56-56l152 0 0-40-8 0c-26.5 0-48-21.5-48-48l0-64z")
        },
    };
}
