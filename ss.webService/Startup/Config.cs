using System;
using System.Configuration;

namespace ss.webService
{
    /// <summary>
    /// 
    /// </summary>
    public interface IConfig 
    {
        string URL_DETECTOR_RESOURCES_XML_FILENAME  { get; }
        string SENT_SPLITTER_RESOURCES_XML_FILENAME { get; }
        int CONCURRENT_FACTORY_INSTANCE_COUNT { get; }
    }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class Config : IConfig
    {
        public string URL_DETECTOR_RESOURCES_XML_FILENAME  { get; } = ConfigurationManager.AppSettings[ "URL_DETECTOR_RESOURCES_XML_FILENAME"  ];
        public string SENT_SPLITTER_RESOURCES_XML_FILENAME { get; } = ConfigurationManager.AppSettings[ "SENT_SPLITTER_RESOURCES_XML_FILENAME" ];

        public int CONCURRENT_FACTORY_INSTANCE_COUNT { get; } = (int.TryParse( ConfigurationManager.AppSettings[ "CONCURRENT_FACTORY_INSTANCE_COUNT" ], out var i ) ? Math.Min( 100, Math.Max( 1, i ) ) : Environment.ProcessorCount);
    }
}
