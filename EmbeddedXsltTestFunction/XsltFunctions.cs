using System.Linq;
using System.Xml.XPath;

namespace EmbeddedXsltTestFunction
{
    /// <summary>
    /// C# containing methods visible in Functions.xslt for use in transformations.
    /// </summary>
    public class XsltFunctions
    {
        public static string ToLower(XPathNodeIterator iterator)
        {
            return iterator.OfType<XPathNavigator>().FirstOrDefault()?.Value.ToLower();
        }
    }
}
