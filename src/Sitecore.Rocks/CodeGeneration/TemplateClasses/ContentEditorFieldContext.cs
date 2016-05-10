// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ContentEditorFieldContext.cs" company="Sitecore A/S">
//   Copyright (C) 2010 by Sitecore A/S
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.VisualStudio.CodeGeneration.TemplateClasses
{
  using Sitecore.VisualStudio.Annotations;
  using Sitecore.VisualStudio.Commands;
  using Sitecore.VisualStudio.Diagnostics;

  /// <summary>The content editor command args.</summary>
  public class TemplateClassesContext : ICommandContext
  {
    #region Constructors and Destructors

    /// <summary>Initializes a new instance of the <see cref="TemplateClassesContext"/> class.</summary>
    /// <param name="generator">The generator.</param>
    public TemplateClassesContext([NotNull] TemplateClassesCodeGenerator generator)
    {
      Assert.ArgumentNotNull(generator, "generator");

      this.Generator = generator;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the generator.
    /// </summary>
    /// <value>The generator.</value>
    public TemplateClassesCodeGenerator Generator { get; set; }

    #endregion
  }
}