using System;
using System.Collections.Generic;
using System.Text;

namespace Leo.Abstractions.Features
{
    public interface IFeatureInfo
    {
        string Id { get; }
        string Name { get; }
        int Priority { get; }
        string Category { get; }
        string Description { get; }
        bool DefaultTenantOnly { get; }
        //IExtensionInfo Extension { get; }
        string[] Dependencies { get; }
    }
}
