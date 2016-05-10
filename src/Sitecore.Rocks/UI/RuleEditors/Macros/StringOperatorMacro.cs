// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

namespace Sitecore.Rocks.UI.RuleEditors.Macros
{
    [RuleEditorMacro("stringoperator")]
    public class StringOperatorMacro : OperatorMacroBase
    {
        public StringOperatorMacro()
        {
            Operators.Add(@"{10537C58-1684-4CAB-B4C0-40C10907CE31}", @"is equal to");
            Operators.Add(@"{537244C2-3A3F-4B81-A6ED-02AF494C0563}", @"is case-insensitively equal to");
            Operators.Add(@"{A6AC5A6B-F409-48B0-ACE7-C3E8C5EC6406}", @"is not equal to");
            Operators.Add(@"{6A7294DF-ECAE-4D5F-A8D2-C69CB1161C09}", @"is not case-insensitively equal to");
            Operators.Add(@"{2E67477C-440C-4BCA-A358-3D29AED89F47}", @"contains");
            Operators.Add(@"{F8641C26-EE27-483C-9FEA-35529ECC8541}", @"matches the regular expression");
            Operators.Add(@"{22E1F05F-A17A-4D0C-B376-6F7661500F03}", @"ends with");
            Operators.Add(@"{FDD7C6B1-622A-4362-9CFF-DDE9866C68EA}", @"starts with");
        }
    }
}
