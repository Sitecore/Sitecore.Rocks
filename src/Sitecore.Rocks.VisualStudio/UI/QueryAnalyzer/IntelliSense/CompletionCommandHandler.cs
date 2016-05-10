// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompletionCommandHandler.cs" company="Sitecore A/S">
//   Copyright (C) 2010 by Sitecore A/S
// </copyright>
// <summary>
//   The completion command handler.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.VisualStudio.UI.QueryAnalyzer.IntelliSense
{
  using System;
  using System.Runtime.InteropServices;
  using Microsoft.VisualStudio;
  using Microsoft.VisualStudio.Language.Intellisense;
  using Microsoft.VisualStudio.OLE.Interop;
  using Microsoft.VisualStudio.Shell;
  using Microsoft.VisualStudio.Text;
  using Microsoft.VisualStudio.Text.Editor;
  using Microsoft.VisualStudio.TextManager.Interop;
  using Sitecore.VisualStudio.Annotations;
  using Sitecore.VisualStudio.Diagnostics;

  /// <summary>The completion command handler.</summary>
  public class CompletionCommandHandler : IOleCommandTarget
  {
    #region Constants and Fields

    /// <summary>The next command handler.</summary>
    private readonly IOleCommandTarget nextCommandHandler;

    /// <summary>The provider.</summary>
    private readonly CompletionHandlerProvider provider;

    /// <summary>The text view.</summary>
    private readonly ITextView textView;

    /// <summary>The session.</summary>
    private ICompletionSession session;

    #endregion

    #region Constructors and Destructors

    /// <summary>Initializes a new instance of the <see cref="CompletionCommandHandler"/> class.</summary>
    /// <param name="textViewAdapter">The text view adapter.</param>
    /// <param name="textView">The text view.</param>
    /// <param name="provider">The provider.</param>
    internal CompletionCommandHandler([NotNull] IVsTextView textViewAdapter, [NotNull] ITextView textView, [NotNull] CompletionHandlerProvider provider)
    {
      Debug.ArgumentNotNull(textViewAdapter, "textViewAdapter");
      Debug.ArgumentNotNull(textView, "textView");
      Debug.ArgumentNotNull(provider, "provider");

      this.textView = textView;
      this.provider = provider;

      textViewAdapter.AddCommandFilter(this, out this.nextCommandHandler);
    }

    #endregion

    #region Implemented Interfaces

    #region IOleCommandTarget

    /// <summary>Executes the specified command.</summary>
    /// <param name="pguidCmdGroup">The GUID of the command group.</param>
    /// <param name="nCmdID">The command ID.</param>
    /// <param name="nCmdexecopt">Specifies how the object should execute the command. Possible values are taken from the <see cref="T:Microsoft.VisualStudio.OLE.Interop.OLECMDEXECOPT"/> and <see cref="T:Microsoft.VisualStudio.OLE.Interop.OLECMDID_WINDOWSTATE_FLAG"/> enumerations.</param>
    /// <param name="pvaIn">The input arguments of the command.</param>
    /// <param name="pvaOut">The output arguments of the command.</param>
    /// <returns>This method returns S_OK on success. Other possible return values include the following.Return codeDescriptionOLECMDERR_E_UNKNOWNGROUPThe <paramref name="pguidCmdGroup"/> parameter is not null but does not specify a recognized command group.OLECMDERR_E_NOTSUPPORTEDThe <paramref name="nCmdID"/> parameter is not a valid command in the group identified by <paramref name="pguidCmdGroup"/>.OLECMDERR_E_DISABLEDThe command identified by <paramref name="nCmdID"/> is currently disabled and cannot be executed.OLECMDERR_E_NOHELPThe caller has asked for help on the command identified by <paramref name="nCmdID"/>, but no help is available.OLECMDERR_E_CANCELEDThe user canceled the execution of the command.</returns>
    public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
    {
      if (VsShellUtilities.IsInAutomationFunction(this.provider.ServiceProvider))
      {
        return this.nextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
      }

      // make a copy of this so we can look at it after forwarding some commands
      var commandID = nCmdID;
      var typedChar = char.MinValue;

      // make sure the input is a char before getting it
      if (pguidCmdGroup == VSConstants.VSStd2K && nCmdID == (uint)VSConstants.VSStd2KCmdID.TYPECHAR)
      {
        typedChar = (char)(ushort)Marshal.GetObjectForNativeVariant(pvaIn);
      }

      // check for a commit character
      if (nCmdID == (uint)VSConstants.VSStd2KCmdID.RETURN || nCmdID == (uint)VSConstants.VSStd2KCmdID.TAB || (char.IsWhiteSpace(typedChar) || char.IsPunctuation(typedChar)))
      {
        // check for a a selection
        if (this.session != null && !this.session.IsDismissed)
        {
          // if the selection is fully selected, commit the current session
          if (this.session.SelectedCompletionSet.SelectionStatus.IsSelected)
          {
            this.session.Commit();

            // also, don't add the character to the buffer
            return VSConstants.S_OK;
          }
          else
          {
            // if there is no selection, dismiss the session
            this.session.Dismiss();
          }
        }
      }

      // pass along the command so the char is added to the buffer
      var retVal = this.nextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
      var handled = false;

      if (!typedChar.Equals(char.MinValue) && char.IsLetterOrDigit(typedChar))
      {
        if (this.session == null || this.session.IsDismissed)
        {
          // If there is no active session, bring up completion
          this.TriggerCompletion();
          this.session.Filter();
        }
        else
        {
          // the completion session is already active, so just filter
          this.session.Filter();
        }

        handled = true;
      }
      else if (commandID == (uint)VSConstants.VSStd2KCmdID.BACKSPACE || commandID == (uint)VSConstants.VSStd2KCmdID.DELETE)
      {
        if (this.session != null && !this.session.IsDismissed)
        {
          this.session.Filter();
        }

        handled = true;
      }

      if (handled)
      {
        return VSConstants.S_OK;
      }

      return retVal;
    }

    /// <summary>Queries the object for the status of one or more commands generated by user interface events.</summary>
    /// <param name="pguidCmdGroup">The GUID of the command group.</param>
    /// <param name="cCmds">The number of commands in <paramref name="prgCmds"/>.</param>
    /// <param name="prgCmds">An array of <see cref="T:Microsoft.VisualStudio.OLE.Interop.OLECMD"/> structures that indicate the commands for which the caller needs status information. This method fills the <paramref name="cmdf"/> member of each structure with values taken from the <see cref="T:Microsoft.VisualStudio.OLE.Interop.OLECMDF"/> enumeration.</param>
    /// <param name="pCmdText">An <see cref="T:Microsoft.VisualStudio.OLE.Interop.OLECMDTEXT"/> structure in which to return name and/or status information of a single command. This parameter can be null to indicate that the caller does not need this information.</param>
    /// <returns>This method returns S_OK on success. Other possible return values include the following.Return codeDescriptionE_FAILThe operation failed.E_UNEXPECTEDAn unexpected error has occurred.E_POINTERThe <paramref name="prgCmds"/> argument is null.OLECMDERR_E_UNKNOWNGROUPThe <paramref name="pguidCmdGroup"/> parameter is not null but does not specify a recognized command group.</returns>
    public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, [CanBeNull] OLECMD[] prgCmds, IntPtr pCmdText)
    {
      return this.nextCommandHandler.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
    }

    #endregion

    #endregion

    #region Methods

    /// <summary>The on session dismissed.</summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The e.</param>
    private void OnSessionDismissed([CanBeNull] object sender, [CanBeNull] EventArgs e)
    {
      this.session.Dismissed -= this.OnSessionDismissed;
      this.session = null;
    }

    /// <summary>Triggers the completion.</summary>
    /// <returns>Returns <c>true</c>, if successful, otherwise <c>false</c>.</returns>
    private bool TriggerCompletion()
    {
      // the caret must be in a non-projection location 
      var caretPoint = this.textView.Caret.Position.Point.GetPoint(textBuffer => (!textBuffer.ContentType.IsOfType(@"projection")), PositionAffinity.Predecessor);
      if (!caretPoint.HasValue)
      {
        return false;
      }

      this.session = this.provider.CompletionBroker.CreateCompletionSession(this.textView, caretPoint.Value.Snapshot.CreateTrackingPoint(caretPoint.Value.Position, PointTrackingMode.Positive), true);

      // subscribe to the Dismissed event on the session 
      this.session.Dismissed += this.OnSessionDismissed;
      this.session.Start();

      return true;
    }

    #endregion
  }
}