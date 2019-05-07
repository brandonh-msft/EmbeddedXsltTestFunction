using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Xsl;

namespace EmbeddedXsltTestFunction
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static IActionResult Run(
#pragma warning disable IDE0060 // Remove unused parameter
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            // sample xml document
            var xmlDocumentString = @"<?xml version=""1.0"" encoding=""utf-8""?>
<MyXml>
    <MyElt>
        MyEltValue
    </MyElt>
</MyXml>";

            // xml transform pointing to custom code
            var xsltDocumentString = $@"<xsl:stylesheet version=""1.0"" xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"" xmlns:msxsl=""urn:schemas-microsoft-com:xslt"" xmlns:user=""{nameof(XsltFunctions)}""> 
    <xsl:template match=""MyElt"">
<MyElt>
    <OriginalValue>
        <xsl:copy-of select=""node()""/>
    </OriginalValue>
    <ToLowerValue>
        <xsl:value-of select=""user:ToLower(node())""/>
    </ToLowerValue>
</MyElt>
</xsl:template>
</xsl:stylesheet>";

            // Compile the style sheet.
            var xslt_settings = new XsltSettings();
            var xslt = new XslCompiledTransform();
            var styleSheetReader = new XmlTextReader(new StringReader(xsltDocumentString));
            xslt.Load(styleSheetReader, xslt_settings, new XmlUrlResolver());

            // Add capability to ref external functions
            var xsltArgList = new XsltArgumentList();
            xsltArgList.AddExtensionObject(nameof(XsltFunctions), new XsltFunctions());

            // Load the XML source file.
            XmlReader inputDataReader = new XmlTextReader(new StringReader(xmlDocumentString));

            // Create an XmlWriter.
            var settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.Indent = true;

            var output = new StringBuilder();
            var writer = new XmlTextWriter(new StringWriter(output));

            // Execute the transformation.
            xslt.Transform(inputDataReader, xsltArgList, writer);

            // return the transformed XML response to the caller
            var result = new OkObjectResult(output.ToString());

            var xmlOutputFormatter = new StringOutputFormatter();
            xmlOutputFormatter.SupportedMediaTypes.Add(@"application/xml");

            result.Formatters = new FormatterCollection<IOutputFormatter>() { xmlOutputFormatter };
            result.ContentTypes.Add(@"application/xml");

            return result;
        }
    }
}
