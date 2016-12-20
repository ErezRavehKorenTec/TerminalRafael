using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using WpfTerminal.Enums;

namespace WpfTerminal.Configuration
{
    public class ConfigurationHolder
    {
        private static ConfigurationHolder _configuration;
        private static XmlDocument _xmlDoc;
        private ConfigurationHolder()
        {
            try
            {
                _xmlDoc = new XmlDocument();
                _xmlDoc.Load(ConfigurationManager.AppSettings["ConfigFilePath"].ToString());
            }
            catch (Exception e)
            {
                throw new NullReferenceException(e.Message + " (Configuration)");
            }
        }

        public static ConfigurationHolder GetInstance()
        {

            if (_configuration == null)
                _configuration = new ConfigurationHolder();
            return _configuration;
        }
        public Dictionary<string, string> GetValue(ConfigurationParameter _paramName)
        {
            try
            {

            Dictionary<string, string> result = new Dictionary<string, string>();
            XmlNodeList parameterElement = _xmlDoc.GetElementsByTagName(_paramName.ToString());
            foreach (XmlNode node in parameterElement[0].ChildNodes)
            {
                if(node.NodeType!= XmlNodeType.Comment)
                result.Add(node.Attributes["Name"].Value, node.InnerText);
            }
            return (Dictionary<string, string>)result;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message+" (Configuration)");
            }
        }
    }
}
