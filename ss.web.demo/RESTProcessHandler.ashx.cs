using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
//using System.Web.SessionState;

using lingvo.urls;
using Newtonsoft.Json;
using _SentSplitter_ = lingvo.sentsplitting.SentSplitter;

namespace lingvo
{
    /// <summary>
    /// 
    /// </summary>
    internal static class Config
    {
        public static readonly string URL_DETECTOR_RESOURCES_XML_FILENAME  = ConfigurationManager.AppSettings[ "URL_DETECTOR_RESOURCES_XML_FILENAME"  ];
        public static readonly string SENT_SPLITTER_RESOURCES_XML_FILENAME = ConfigurationManager.AppSettings[ "SENT_SPLITTER_RESOURCES_XML_FILENAME" ];
        
        public static readonly int    MAX_INPUTTEXT_LENGTH                 = int.Parse( ConfigurationManager.AppSettings[ "MAX_INPUTTEXT_LENGTH" ] );
        public static readonly int    CONCURRENT_FACTORY_INSTANCE_COUNT    = int.Parse( ConfigurationManager.AppSettings[ "CONCURRENT_FACTORY_INSTANCE_COUNT" ] );
        public static readonly int    SAME_IP_INTERVAL_REQUEST_IN_SECONDS  = int.Parse( ConfigurationManager.AppSettings[ "SAME_IP_INTERVAL_REQUEST_IN_SECONDS" ] );
        public static readonly int    SAME_IP_MAX_REQUEST_IN_INTERVAL      = int.Parse( ConfigurationManager.AppSettings[ "SAME_IP_MAX_REQUEST_IN_INTERVAL" ] );        
        public static readonly int    SAME_IP_BANNED_INTERVAL_IN_SECONDS   = int.Parse( ConfigurationManager.AppSettings[ "SAME_IP_BANNED_INTERVAL_IN_SECONDS" ] );
    }
}

namespace lingvo.sentsplitting
{
    /// <summary>
    /// Summary description for RESTProcessHandler
    /// </summary>
    public sealed class RESTProcessHandler : IHttpHandler
    {
        /// <summary>
        /// 
        /// </summary>
        public abstract class sent_info_base
        {
            [JsonProperty(PropertyName="i")]  public int startIndex
            {
                get;
                set;
            }
            [JsonProperty(PropertyName="l")] public int length
            {
                get;
                set;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private abstract class result_base
        {
            protected result_base()
            {
            }
            protected result_base( Exception ex )
            {
                exceptionMessage = ex.ToString();
            }

            [JsonProperty(PropertyName="err")]
            public string exceptionMessage
            {
                get;
                private set;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private sealed class result : result_base
        {
            /// <summary>
            /// 
            /// </summary>
            public sealed class sent_info : sent_info_base
            {
            }

            public result( Exception ex ) : base( ex )
            {
            }
            public result( IList< sent_t > _sents )
            {
                sents = (from sent in _sents
                            select
                                new sent_info()
                                {
                                    startIndex = sent.startIndex,
                                    length     = sent.length,
                                }
                        ).ToArray();
            }

            public sent_info[] sents
            {
                get;
                private set;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private sealed class result_with_text : result_base
        {
            /// <summary>
            /// 
            /// </summary>
            public sealed class sent_info : sent_info_base
            {
                [JsonProperty(PropertyName="t")]
                public string text
                {
                    get;
                    set;
                }
            }

            public result_with_text( Exception ex ) : base( ex )
            {
            }
            public result_with_text( IList< sent_t > _sents, string originalText )
            {
                sents = (from sent in _sents
                            select
                                new sent_info()
                                {
                                    text       = sent.GetValue( originalText ),
                                    startIndex = sent.startIndex,
                                    length     = sent.length,
                                }
                        ).ToArray();
            }

            public sent_info[] sents
            {
                get;
                private set;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private sealed class http_context_data
        {
            private static readonly object _Lock = new object();
            private readonly HttpContext _Context;

            public http_context_data( HttpContext context )
            {
                _Context = context;
            }

            /*private ConcurrentFactory _ConcurrentFactory
            {
                get { return ((ConcurrentFactory) _Context.Cache[ "_ConcurrentFactory" ]); }
                set
                {                    
                    if ( value != null )
                        _Context.Cache[ "_ConcurrentFactory" ] = value;
                    else
                        _Context.Cache.Remove( "_ConcurrentFactory" );
                }
            }*/

            private static ConcurrentFactory _ConcurrentFactory;

            public ConcurrentFactory GetConcurrentFactory()
            {
                var cf = _ConcurrentFactory;
                if ( cf == null )
                {
                    lock ( _Lock )
                    {
                        cf = _ConcurrentFactory;
                        if ( cf == null )
                        {
                            var config = new SentSplitterConfig()
                            {
                                Model             = new SentSplitterModel( Config.SENT_SPLITTER_RESOURCES_XML_FILENAME ),
                                UrlDetectorConfig = new UrlDetectorConfig( Config.URL_DETECTOR_RESOURCES_XML_FILENAME ),
                                SplitBySmiles     = true,
                            };
                            cf = new ConcurrentFactory( config, Config.CONCURRENT_FACTORY_INSTANCE_COUNT );
                            _ConcurrentFactory = cf;
                        }
                    }
                }
                return (cf);
            }
        }

        static RESTProcessHandler()
        {
            Environment.CurrentDirectory = HttpContext.Current.Server.MapPath( "~/" );
        }

        public bool IsReusable
        {
            get { return (true); }
        }

        public void ProcessRequest( HttpContext context )
        {
            try
            {
                var text          = context.GetRequestStringParam( "text", Config.MAX_INPUTTEXT_LENGTH );
                var splitBySmiles = context.Request[ "splitBySmiles" ].Try2Boolean( true );
                var returnText    = context.Request[ "returnText"    ].Try2Boolean( true );

                var hcd = new http_context_data( context );
                var sents = hcd.GetConcurrentFactory().AllocateSents( text, splitBySmiles );

                SendJsonResponse( context, sents, text, returnText );
            }
            catch ( Exception ex )
            {
                SendJsonResponse( context, ex );
            }
        }

        private static void SendJsonResponse( HttpContext context, IList< sent_t > sents, string originalText, bool returnText )
        {
            if ( returnText )
            {
                SendJsonResponse( context, new result_with_text( sents, originalText ) );
            }
            else
            {
                SendJsonResponse( context, new result( sents ) );
            }
        }
        private static void SendJsonResponse( HttpContext context, Exception ex )
        {
            SendJsonResponse( context, new result( ex ) );
        }
        private static void SendJsonResponse( HttpContext context, result_base result )
        {
            context.Response.ContentType = "application/json";
            //---context.Response.Headers.Add( "Access-Control-Allow-Origin", "*" );

            var json = JsonConvert.SerializeObject( result );
            context.Response.Write( json );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal static class Extensions
    {
        public static bool Try2Boolean( this string value, bool defaultValue )
        {
            if ( value != null )
            {
                var result = default(bool);
                if ( bool.TryParse( value, out result ) )
                    return (result);
            }
            return (defaultValue);
        }

        public static string GetRequestStringParam( this HttpContext context, string paramName, int maxLength )
        {
            var value = context.Request[ paramName ];
            if ( (value != null) && (maxLength < value.Length) && (0 < maxLength) )
            {
                return (value.Substring( 0, maxLength ));
            }
            return (value);
        }
    }
}