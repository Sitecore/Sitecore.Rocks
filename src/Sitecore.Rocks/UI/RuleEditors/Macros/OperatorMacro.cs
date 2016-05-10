// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

namespace Sitecore.Rocks.UI.RuleEditors.Macros
{
    [RuleEditorMacro("operator")]
    public class OperatorMacro : OperatorMacroBase
    {
        public OperatorMacro()
        {
            Operators.Add(@"{066602E2-ED1D-44C2-A698-7ED27FD3A2CC}", @"is equal to");
            Operators.Add(@"{B88CD556-082E-4385-BB76-E4D1B565F290}", @"is greater than");
            Operators.Add(@"{814EF7D0-1639-44FD-AEEF-735B5AC14425}", @"is greater than or equal to");
            Operators.Add(@"{E362A3A4-E230-4A40-A7C4-FC42767E908F}", @"is less than");
            Operators.Add(@"{2E1FC840-5919-4C66-8182-A33A1039EDBF}", @"is less than or equal to");
            Operators.Add(@"{3627ED99-F454-4B83-841A-A0194F0FB8B4}", @"is not equal to");
        }
    }
}
