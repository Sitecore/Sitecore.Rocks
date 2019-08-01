using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.Rocks.Server.Abstractions.Jobs
{
    public class RunJobOptions
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public Action Action { get; set; }
    }
}