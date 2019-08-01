using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Sitecore.Rocks.Server.IntegrationTests.Extensions;
using Sitecore.Rocks.Server.IntegrationTests.HardRocks;
using Xunit;

namespace Sitecore.Rocks.Server.IntegrationTests
{
    public class TemplateTests
    {
        [Fact]
        public async Task GetsTemplateXml()
        {
            const string testId = "{4928522A-9CDF-4353-B579-154C3AAD8C1F}";
            var response = await ClientFactory.Client.ExecuteAsync($"{Properties.RequestNamespace}.Templates.GetTemplateXml",
                new ArrayOfAnyType { Properties.MasterDb, testId, false }, Properties.Credentials);
            var result = response?.Body?.ExecuteResult?.ToDynamic();
            Assert.NotNull(result);
            Assert.Equal("Template", result.template.name);
            Assert.Equal(testId, result.template.id);
            Assert.Contains("{C12BEDF5-D08F-407A-9C6B-B7528B250383}", result.template.basetemplates);
            Assert.Contains("spider", result.template.icon);
            Assert.Equal("{E8AEBB1A-0EC5-4EAF-A092-FC7B8F572EB5}", result.template.standardvaluesitemid);
            Assert.Equal("/sitecore/templates/Tests/Template/GetsTemplateXml/Template", result.template.path);
            var sections = (IList<dynamic>) result.template.section;
            Assert.Equal(2, sections.Count);
            Assert.Equal("SectionA", sections[0].name);
            var fields = (IList<dynamic>) sections[0].field;
            Assert.Equal(3, fields.Count);
            Assert.Collection(fields,
                field => Assert.True(field.name == "FieldA" && field.type == "Number"),
                field => Assert.True(field.name == "FieldB" && field.type == "Rich Text"),
                field => Assert.True(field.name == "FieldC" && field.type == "Image" && field.shared == "1" && field.source == "/sitecore")
            );
        }

        [Fact]
        public async Task SavingTemplateXmlAddsAndRemovesFields()
        {
            const string testId = "{61257BE1-50EE-435D-A824-5F2398E81B2A}";

            async Task<string> GetTemplateXml()
            {
                var getResponse = await ClientFactory.Client.ExecuteAsync($"{Properties.RequestNamespace}.Templates.GetTemplateXml", new ArrayOfAnyType {Properties.MasterDb, testId, false}, Properties.Credentials);
                return getResponse.Body.ExecuteResult;
            }

            var templateXml = XDocument.Parse(await GetTemplateXml()).Root;
            var sections = templateXml.Elements("section");

            // Remove FieldA
            var existingFields = sections.First().Elements("field");
            Assert.Equal(3, existingFields.Count());
            existingFields.Take(1).Single().Remove();

            // Add FieldD
            const string fieldDName = "FieldD";
            const string fieldDType = "Rich Text";
            const string fieldDShared = "1";
            const string fieldDSource = "/sitecore";
            sections.First().Add(new XElement("field",
                new XAttribute("name", fieldDName),
                new XAttribute("type", fieldDType),
                new XAttribute("shared", fieldDShared),
                new XAttribute("source", fieldDSource)
            ));

            // Remove SectionB
            sections.Skip(1).Single().Remove();

            // Add SectionC and FieldE, move FieldB
            const string sectionCName = "SectionC";
            const string fieldEName = "FieldE";
            var fieldB = sections.First().Element("field");
            fieldB.Remove();
            templateXml.Add(new XElement("section",
                new XAttribute("name", sectionCName),
                new XElement("field",
                    new XAttribute("name", fieldEName),
                    new XAttribute("type", "Image")
                ),
                fieldB
            ));

            await ClientFactory.Client.ExecuteAsync(
                $"{Properties.RequestNamespace}.Templates.SaveTemplateXml",
                new ArrayOfAnyType { Properties.MasterDb, templateXml.ToString() }, Properties.Credentials);
            var result = (await GetTemplateXml()).ToDynamic();

            // removed and added a section
            var resultSections = (IList<dynamic>)result.template.section;
            Assert.Equal(2, resultSections.Count);

            // removed a field, added a field, moved another field
            var sectionAFields = (IList<dynamic>) resultSections[0].field;
            Assert.Equal(2, sectionAFields.Count);

            // added FieldD
            var fieldD = sectionAFields.Last();
            Assert.Equal(fieldDName, fieldD.name);
            Assert.Equal(fieldDType, fieldD.type);
            Assert.Equal(fieldDShared, fieldD.shared);
            Assert.Equal(fieldDSource, fieldD.source);

            // added SectionC
            var sectionC = resultSections.Last();
            Assert.Equal(sectionCName, sectionC.name);
            var sectionCFields = (IList<dynamic>) sectionC.field;
            Assert.Collection(sectionCFields,
                field => Assert.Equal(fieldEName, field.name),
                field => Assert.Equal("FieldB", field.name)
            );
        }
    }
}
