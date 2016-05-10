// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Extensibility.Composition
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class ImportAttribute : Attribute
    {
        public ImportAttribute([NotNull] string contractName)
        {
            Assert.ArgumentNotNull(contractName, nameof(contractName));

            ContractName = contractName;
        }

        public ImportAttribute([NotNull] Type contractType)
        {
            Assert.ArgumentNotNull(contractType, nameof(contractType));

            ContractType = contractType;
        }

        public ImportAttribute([NotNull] Type contractType, [NotNull] string contractName)
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
