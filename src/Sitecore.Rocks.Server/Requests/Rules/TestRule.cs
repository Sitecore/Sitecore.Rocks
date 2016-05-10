// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Extensions.StringExtensions;
using Sitecore.Rocks.Server.Extensions.XElementExtensions;
using Sitecore.Rules;
using Sitecore.Rules.Conditions;

namespace Sitecore.Rocks.Server.Requests.Rules
{
    public class TestRule
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, string itemId, [NotNull] string rules)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("rule");

            Execute(output, databaseName, itemId, rules);

            output.WriteEndElement();

            return writer.ToString();
        }

        private void Evaluate(XmlTextWriter output, Item item, XElement root, RuleList<RuleContext> ruleList)
        {
            var ruleContext = new RuleContext
            {
                Item = item
            };

            var index = 0;
            foreach (var rule in ruleList.Rules)
            {
                var ruleElement = root.Element(index);

                index++;

                output.WriteElementString("entry", string.Format("Evaluating rule {0} of {1}.", index, ruleList.Rules.Count()));

                if (rule.Condition == null)
                {
                    output.WriteElementString("entry", string.Format("    There are no conditions and the rule is skipped.", index));
                    continue;
                }

                output.WriteElementString("entry", "    Evaluating conditions");

                var stack = new RuleStack();
                var conditionsElement = ruleElement.Element("conditions");

                Evaluate(output, rule.Condition, ruleContext, stack, item.Database, conditionsElement.Element(0));

                if (ruleContext.IsAborted)
                {
                    output.WriteElementString("entry", string.Format("    Evaluation is aborted by a condition.", index));
                    break;
                }

                var result = stack.Count != 0 && (bool)stack.Pop();

                output.WriteElementString("entry", string.Format("    The evaluation result is {0}.", result ? "True" : "False"));

                if (result)
                {
                    RenderActions(output, item, ruleElement, rule);
                }
            }
        }

        private void Evaluate(XmlTextWriter output, RuleCondition<RuleContext> condition, RuleContext ruleContext, RuleStack stack, Database database, XElement element)
        {
            var andCondition = condition as AndCondition<RuleContext>;
            if (andCondition != null)
            {
                Evaluate(output, andCondition.LeftOperand, ruleContext, stack, database, element.Element(0));
                if (!(bool)stack.Pop())
                {
                    stack.Push(false);
                }
                else
                {
                    Evaluate(output, andCondition.RightOperand, ruleContext, stack, database, element.Element(1));
                }

                return;
            }

            var orCondition = condition as OrCondition<RuleContext>;
            if (orCondition != null)
            {
                Evaluate(output, orCondition.LeftOperand, ruleContext, stack, database, element.Element(0));
                if ((bool)stack.Pop())
                {
                    stack.Push(true);
                }
                else
                {
                    Evaluate(output, orCondition.RightOperand, ruleContext, stack, database, element.Element(1));
                }

                return;
            }

            var notCondition = condition as NotCondition<RuleContext>;
            if (notCondition != null)
            {
                Evaluate(output, notCondition.Operand, ruleContext, stack, database, element.Element(0));

                var flag = (bool)stack.Pop();
                stack.Push(!flag);

                return;
            }

            condition.Evaluate(ruleContext, stack);

            var text = condition.GetType().Name;

            var conditionItem = database.GetItem(element.GetAttributeValue("id"));
            if (conditionItem != null)
            {
                text = Expand(database, conditionItem["Text"], element);
            }

            var result = stack.Peek();

            output.WriteElementString("entry", string.Format("        Condition \"{0}\" returned \"{1}\".", text, result.ToString()));
        }

        private void Execute(XmlTextWriter output, string databaseName, string itemId, string rules)
        {
            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                output.WriteElementString("entry", "Database not found.");
                return;
            }

            var item = database.GetItem(itemId);
            if (item == null)
            {
                output.WriteElementString("entry", "Source item not found.");
                return;
            }

            XElement root;
            try
            {
                var doc = XDocument.Parse(rules);
                root = doc.Root;
            }
            catch
            {
                output.WriteElementString("entry", "Rules Xml is not welformed.");
                return;
            }

            if (root == null)
            {
                output.WriteElementString("entry", "There are no rules defined.");
                return;
            }

            var ruleList = RuleFactory.ParseRules<RuleContext>(database, rules);
            if (!ruleList.Rules.Any())
            {
                output.WriteElementString("entry", "There are no rules defined.");
                return;
            }

            Evaluate(output, item, root, ruleList);
        }

        private string Expand(Database database, string text, XElement element)
        {
            var result = new StringBuilder();

            var s = text.IndexOf('[');
            while (s >= 0)
            {
                var e = text.IndexOf(']', s);
                if (e < 0)
                {
                    break;
                }

                result.Append(text.Left(s));

                var macro = text.Mid(s + 1, e - s - 1);
                text = text.Mid(e + 1);

                var parts = macro.Split(',');

                if (parts.Length >= 4)
                {
                    macro = parts[3];

                    var value = element.GetAttributeValue(parts[0], null);
                    if (value != null)
                    {
                        if (ID.IsID(value))
                        {
                            var item = database.GetItem(value);
                            if (item != null)
                            {
                                value = item.Name;
                            }
                        }

                        macro = value;
                    }
                }

                result.Append(macro);

                s = text.IndexOf('[');
            }

            if (!string.IsNullOrEmpty(text))
            {
                result.Append(text);
            }

            return result.ToString();
        }

        private void RenderActions(XmlTextWriter output, Item item, XElement ruleElement, Rule<RuleContext> rule)
        {
            var actionsElement = ruleElement.Element("actions");

            output.WriteElementString("entry", "    Applying actions (don't worry, nothing will be changed)");

            var index = 0;

            foreach (var action in rule.Actions)
            {
                var text = action.GetType().Name;

                var actionElement = actionsElement.Element(index);
                var actionItem = item.Database.GetItem(actionElement.GetAttributeValue("id"));
                if (actionItem != null)
                {
                    text = Expand(item.Database, actionItem["Text"], actionElement);
                }

                output.WriteElementString("entry", string.Format("        Action \"{0}\".", text));
                index++;
            }
        }
    }
}
