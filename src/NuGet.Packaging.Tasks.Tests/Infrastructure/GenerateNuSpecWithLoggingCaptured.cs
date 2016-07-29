
using System.Collections.Generic;

namespace NuGet.Packaging.Tasks.Tests.Infrastructure
{
    class GenerateNuSpecWithLoggingCaptured : GenerateNuSpec
    {
        public List<string> WarningsLogged = new List<string>();

        protected override void LogWarning(string message, params object[] messageArgs)
        {
            WarningsLogged.Add(string.Format(message, messageArgs));
        }
    }
}
