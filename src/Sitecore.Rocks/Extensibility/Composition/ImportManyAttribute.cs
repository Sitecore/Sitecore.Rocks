// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Extensibility.Composition
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class ImportManyAttribute : Attribute
    {
        public ImportManyAttribute()
        {
        }

        public ImportManyAttribute([NotNull] string contractName)
        {
            Assert.ArgumentNotNull(contractName, nameof(contractName));

            ContractName = contractName;
        }

        public ImportManyAttribute([NotNull] Type contractType)
        {
            Assert.ArgumentNotNull(contractType, nameof(contractType));

            ContractType = contractType;
        }

        public ImportManyAttribute([NotNull] Type contractType, [NotNull] string contractName)
        {
            Assert.ArgumentNotNull(contractType, nameof(contractType));
            Assert.ArgumentNotNull(contractName, nameof(contractName));

            ContractType = contractType;
            ContractName = contractName;
        }

        [CanBeNull]
        public string ContractName { get; private set; }

        [CanBeNull]
        public Type ContractType { get; private set; }
    }
}
