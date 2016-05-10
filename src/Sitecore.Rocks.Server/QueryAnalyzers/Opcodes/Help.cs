// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Linq;
using System.Web.UI;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Opcodes
{
    public class Help : Opcode
    {
        public Help([NotNull] string keyword)
        {
            Assert.ArgumentNotNull(keyword, nameof(keyword));

            Keyword = keyword;
        }

        [NotNull]
        public string Keyword { get; }

        [NotNull]
        public override object Evaluate([NotNull] Query query, [NotNull] QueryContext contextNode)
        {
            Assert.ArgumentNotNull(query, nameof(query));
            Assert.ArgumentNotNull(contextNode, nameof(contextNode));

            var output = new StringWriter();

            if (string.IsNullOrEmpty(Keyword))
            {
                output.WriteLine("KEYWORDS:");

                foreach (var keyword in QueryAnalyzerManager.Keywords.OrderBy(keyword => keyword.Attribute.Keyword))
                {
                    var attribute = keyword.Attribute;

                    output.Write(attribute.Keyword);
                    output.Write(" - ");
                    output.WriteLine(attribute.ShortHelp);
                }

                output.WriteLine();
                output.WriteLine("FUNCTIONS:");

                foreach (var function in QueryAnalyzerManager.Functions.OrderBy(f => f.Attribute.FunctionName))
                {
                    var attribute = function.Attribute;

                    output.Write(attribute.FunctionName);
                    output.Write(" - ");
                    output.WriteLine(attribute.ShortHelp ?? "[No help available]");
                }
            }
            else
            {
                var keyword = QueryAnalyzerManager.Keywords.FirstOrDefault(k => string.Compare(k.Attribute.Keyword, Keyword, StringComparison.InvariantCultureIgnoreCase) == 0);
                if (keyword != null)
                {
                    RenderKeyword(keyword, output);
                }

                var function = QueryAnalyzerManager.Functions.FirstOrDefault(k => string.Compare(k.Attribute.FunctionName, Keyword, StringComparison.InvariantCultureIgnoreCase) == 0);
                if (function != null)
                {
                    RenderFunction(function, output);
                }
            }

            return output;
        }

        public override void Print([NotNull] HtmlTextWriter output)
        {
            Assert.ArgumentNotNull(output, nameof(output));

            PrintIndent(output);
            PrintLine(output, GetType().Name);
        }

        private void RenderFunction(QueryAnalyzerManager.FunctionDescriptor function, StringWriter output)
        {
            var attribute = function.Attribute;

            output.WriteLine("FUNCTION:");
            output.WriteLine(attribute.FunctionName);
            output.WriteLine();

            output.WriteLine("SHORT DESCRIPTION:");
            output.WriteLine(attribute.ShortHelp);
            output.WriteLine();

            output.WriteLine("LONG DESCRIPTION:");
            output.WriteLine(attribute.LongHelp);
            output.WriteLine();

            output.WriteLine("EXAMPLE:");
            output.WriteLine(attribute.Example);
            output.WriteLine();
        }

        private static void RenderKeyword(QueryAnalyzerManager.Keyword keyword, StringWriter output)
        {
            var attribute = keyword.Attribute;

            output.WriteLine("KEYWORD:");
            output.WriteLine(attribute.Keyword);
            output.WriteLine();

            output.WriteLine("SYNTAX (EBNF):");
            output.WriteLine(attribute.Syntax);
            output.WriteLine();

            output.WriteLine("SHORT DESCRIPTION:");
            output.WriteLine(attribute.ShortHelp);
            output.WriteLine();

            output.WriteLine("LONG DESCRIPTION:");
            output.WriteLine(attribute.LongHelp);
            output.WriteLine();

            output.WriteLine("EXAMPLE:");
            output.WriteLine(attribute.Example);
            output.WriteLine();

            output.WriteLine(@"ADDITIONAL SYNTAX (EBNF):
  Queries = Query | ( Queries '|' Query )
  Query = PathStep ( PathStep )*
  PathStep = Node | Children | Descendants 
  Children = '/' Node
  Descendants = '//' Node
  Node = Attribute | Element
  Attribute = '@' Name
  Element = [ Axis ] ( '*' | '.' | '..' | Name ) [ Predicate ]
  Name = Identifier | Name Identifier
  Axis = ( 'ancestor' | 'ancestor-or-self' | 'descendants' | 'descendants-or-self' | 'child' | 'parent' | 'following' | 'preceding' | 'self' ) '::'
  Predicate = '[' Expression ']'
  Expression = Term3 [ ( 'or' | 'xor' ) Term3 ]
  Term3 = Term2 [ 'and' Term2 ]
  Term2 = Term1 [ ( '=' | '&gt;' | '&gt;=' | '&lt;' | '&lt;=' | '!=' ) Term1 ]
  Term1 = Term0 [ ( '+' | '-' ) Term0 ]
  Term0 = Factor [ ( '*' | 'div' | 'mod' ) Factor ]
  Factor = Queries | Literal | Number | True | False | '(' Expression ')' | Attribute | Function | Parameter
  Parameter = '$' Identifier
  Function = Identifier '(' Expressions ')'
  Expressions = Expression | ( Expressions ',' Expression )

  Use -- for comments.
");
        }
    }
}
