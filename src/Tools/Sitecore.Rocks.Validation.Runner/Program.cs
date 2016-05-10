// © 2015 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Runners;

namespace Sitecore
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var validation = new ValidationRunner();
            validation.Run();
        }
    }
}
