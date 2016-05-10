// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Extensibility.Composition
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class ExportAttribute : ExtensibilityAttribute
    {
        public ExportAttribute([NotNull] string contractName)
        {
            Assert.ArgumentNotNull(contractName, nameof(contractName));

            ContractName = contractName;
            SortName = GetType().Name;
        }

        public ExportAttribute([NotNull] Type contractType)
        {
            Assert.ArgumentNotNull(contractType, nameof(contractType));

            ContractType = contractType;
            SortName = string.Empty;
        }

        public ExportAttribute([NotNull] Type contractType, [NotNull] string contractName)
        {
            Assert.ArgumentNotNull(contractType, nameof(contractType));
            Assert.ArgumentNotNull(contractName, nameof(contractName));

            ContractType = contractType;
            ContractName = contractName;
            SortName = string.Empty;
        }

        [CanBeNull]
        public string ContractName { get; private set; }

        [CanBeNull]
        public Type ContractType { get; private set; }

        public CreationPolicy CreationPolicy { get; set; }

        public double Priority { get; set; }

        [NotNull]
        public string SortName { get; set; }

        public override void Initialize(Type type)
        {
            if (type != null)
            {
                AppHost.Extensibility.RegisterExport(this, type);
            }
        }
    }
}
