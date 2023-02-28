using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.CSharp.RuntimeBinder;
using System.Linq;

namespace NuGetizer.Tasks;

public class EntryTarget : Task
{
    [Output]
    public string Target { get; set; } = "";

    public override bool Execute()
    {
        IEnumerable<string> targets;

        if (IsDesktopNaming(BuildEngine))
        {
            // Older .NET/MSBuild desktop field naming convention.
            try
            {
                targets = BuildEngine.AsDynamicReflection().targetBuilderCallback.requestEntry.Request.Targets;
            }
            catch (RuntimeBinderException) { return true; }
            catch (InvalidCastException) { return true; }
        }
        else
        {
            try
            {
                targets = BuildEngine.AsDynamicReflection()._targetBuilderCallback._requestEntry.Request.Targets;
            }
            catch (RuntimeBinderException) { return true; }
            catch (InvalidCastException) { return true; }
        }

        Target = targets.FirstOrDefault() ?? "";
        return true;
    }

    static bool IsDesktopNaming(IBuildEngine engine) => engine.GetType()
        .GetField("targetBuilderCallback", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public) != null;
}
