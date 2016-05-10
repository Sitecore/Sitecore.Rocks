#Field Controls

A Field control displays a field in the Item Editor.

Field controls are usually implemented as WPF ```UserControls```.

Field controls must be marked with the ```FieldControl``` attribute and implement the ```IFieldControl``` interface.

The ```FieldControl``` attribute takes a single parameter, which is the name of the field type, e.g. "Single-line Text".

The ```IFieldControl``` interface has the following members:

###### GetControl
Returns the WPF control that is displayed in the item Editor. If the FieldControl is implemented as a WPF UserControl, return the user control itself.

###### GetFocusableControl
Returns the WPF control that should receive focus. For WPF UserControl return the first control that is focusable.

###### GetValue
Returns the current value of the field.

###### IsSupported
Returns true, if the field control can be used for the specified field.

###### SetField
Sets the source field. This is called when the control is initialized and before SetValue.

###### SetValue
Sets the value of the field. Please notice that this method maybe be called directly from other parts of the system, e.g. Edit Externally.

###### ValueModified
This event should be raised when the Item Editor should be marked as modified. Usually this is called from SetValue.