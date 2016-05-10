// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;

namespace Sitecore.Rocks.Annotations
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public sealed class LocalizationRequiredAttribute : Attribute
    {
        public LocalizationRequiredAttribute(bool required)
        {
            Required = required;
        }

        public bool Required { get; set; }

        public override bool Equals([NotNull] object obj)
        {
            var attribute = obj as LocalizationRequiredAttribute;
            return attribute != null && attribute.Required == Required;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class StringFormatMethodAttribute : Attribute
    {
        private readonly string myFormatParameterName;

        public StringFormatMethodAttribute([NotNull] string formatParameterName)
        {
            myFormatParameterName = formatParameterName;
        }

        public string FormatParameterName
        {
            get { return myFormatParameterName; }
        }
    }

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public sealed class InvokerParameterNameAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class AssertionMethodAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public sealed class AssertionConditionAttribute : Attribute
    {
        private readonly AssertionConditionType myConditionType;

        public AssertionConditionAttribute(AssertionConditionType conditionType)
        {
            myConditionType = conditionType;
        }

        public AssertionConditionType ConditionType
        {
            get { return myConditionType; }
        }
    }

    public enum AssertionConditionType
    {
        IS_TRUE = 0,

        IS_FALSE = 1,

        IS_NULL = 2,

        IS_NOT_NULL = 3,
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class TerminatesProgramAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Delegate | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class CanBeNullAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Delegate | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class NotNullAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public sealed class CannotApplyEqualityOperatorAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true), BaseTypeRequired(typeof(Attribute))]
    public sealed class BaseTypeRequiredAttribute : Attribute
    {
        private readonly Type[] myBaseTypes;

        public BaseTypeRequiredAttribute([NotNull] params Type[] baseTypes)
        {
            myBaseTypes = baseTypes;
        }

        public IEnumerable<Type> BaseTypes
        {
            get { return myBaseTypes; }
        }
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true), MeansImplicitUse]
    public sealed class UsedImplicitlyAttribute : Attribute
    {
        [UsedImplicitly]
        public UsedImplicitlyAttribute() : this(ImplicitUseKindFlags.Default, ImplicitUseTargetFlags.Default)
        {
        }

        [UsedImplicitly]
        public UsedImplicitlyAttribute(ImplicitUseKindFlags useKindFlags, ImplicitUseTargetFlags targetFlags)
        {
            UseKindFlags = useKindFlags;
            TargetFlags = targetFlags;
        }

        [UsedImplicitly]
        public UsedImplicitlyAttribute(ImplicitUseKindFlags useKindFlags) : this(useKindFlags, ImplicitUseTargetFlags.Default)
        {
        }

        [UsedImplicitly]
        public UsedImplicitlyAttribute(ImplicitUseTargetFlags targetFlags) : this(ImplicitUseKindFlags.Default, targetFlags)
        {
        }

        [UsedImplicitly]
        public ImplicitUseTargetFlags TargetFlags { get; private set; }

        [UsedImplicitly]
        public ImplicitUseKindFlags UseKindFlags { get; private set; }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class MeansImplicitUseAttribute : Attribute
    {
        [UsedImplicitly]
        public MeansImplicitUseAttribute() : this(ImplicitUseKindFlags.Default, ImplicitUseTargetFlags.Default)
        {
        }

        [UsedImplicitly]
        public MeansImplicitUseAttribute(ImplicitUseKindFlags useKindFlags, ImplicitUseTargetFlags targetFlags)
        {
            UseKindFlags = useKindFlags;
            TargetFlags = targetFlags;
        }

        [UsedImplicitly]
        public MeansImplicitUseAttribute(ImplicitUseKindFlags useKindFlags) : this(useKindFlags, ImplicitUseTargetFlags.Default)
        {
        }

        [UsedImplicitly]
        public MeansImplicitUseAttribute(ImplicitUseTargetFlags targetFlags) : this(ImplicitUseKindFlags.Default, targetFlags)
        {
        }

        [UsedImplicitly]
        public ImplicitUseTargetFlags TargetFlags { get; private set; }

        [UsedImplicitly]
        public ImplicitUseKindFlags UseKindFlags { get; private set; }
    }

    [Flags]
    public enum ImplicitUseKindFlags
    {
        Default = Access | Assign | Instantiated,

        Access = 1,

        Assign = 2,

        Instantiated = 4,
    }

    [Flags]
    public enum ImplicitUseTargetFlags
    {
        Default = Itself,

        Itself = 1,

        Members = 2,

        WithMembers = Itself | Members
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class NotifyPropertyChangedInvocatorAttribute : Attribute
    {
        public NotifyPropertyChangedInvocatorAttribute()
        {
        }

        public NotifyPropertyChangedInvocatorAttribute(string parameterName)
        {
            ParameterName = parameterName;
        }

        [UsedImplicitly]
        public string ParameterName { get; private set; }
    }
}
