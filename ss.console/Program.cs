using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;

using lingvo.urls;

namespace lingvo.sentsplitting
{
    /// <summary>
    /// 
    /// </summary>
    internal static class Config
    {
        public static string URL_DETECTOR_RESOURCES_XML_FILENAME  => ConfigurationManager.AppSettings[ "URL_DETECTOR_RESOURCES_XML_FILENAME"  ];
        public static string SENT_SPLITTER_RESOURCES_XML_FILENAME => ConfigurationManager.AppSettings[ "SENT_SPLITTER_RESOURCES_XML_FILENAME" ];
    }

    /// <summary>
    /// 
    /// </summary>
    internal static class Program
    {
        private static void Main( string[] args )
        {
            try
            {
                Run_1();
                //Run_2( "C:\\" );
            }
            catch ( Exception ex )
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine( ex );
                Console.ResetColor();
            }
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine( Environment.NewLine + "[.....finita fusking comedy.....]" );
            Console.ReadLine();
        }

        /// <summary>
        /// 
        /// </summary>
        private sealed class Env : IDisposable
        {
            private SentSplitterModel _Model;
            public void Dispose() => _Model.Dispose();

            public SentSplitter SentSplitter { get; init; }
            public static Env Create()
            {
                var model  = new SentSplitterModel( Config.SENT_SPLITTER_RESOURCES_XML_FILENAME );
                var config = new SentSplitterConfig( model )
                {
                    UrlDetectorConfig = new UrlDetectorConfig( Config.URL_DETECTOR_RESOURCES_XML_FILENAME ),
                    SplitBySmiles     = true,
                };
                var sentSplitter = new SentSplitter( config );

                return (new Env() { _Model = model, SentSplitter = sentSplitter });
            }
        }

        private static void Run_1()
        {
            using var env = Env.Create();

            var text = "Напомню, что, как правило, поисковые системы работают с так называемым обратным индексом, отличной метафорой которого будет алфавитный указатель в конце книги: все использованные термины приведены в нормальной форме и упорядочены лексикографически — проще говоря, по алфавиту, и после каждого указан номер страницы, на которой этот термин встречается. Разница только в том, что такая координатная информация в поисковиках, как правило, значительно подробнее. Например, корпоративный поиск МойОфис (рабочее название — baalbek), для каждого появления слова в документе хранит, кроме порядкового номера, ещё и его грамматическую форму и привязку к разметке.";
            var sents = env.SentSplitter.AllocateSents( text );

            var sentTexts = sents.Select( (s, i) => $"{++i}). '{s.GetValue( text ).Cut().Norm()}'" );

            Console.WriteLine( $"text: '{text.Cut().Norm()}'" );
            Console.WriteLine( sentTexts.Any() ? "  " + string.Join( "\r\n  ", sentTexts ) : "  [sents is not found (?)]" );
        }
        private static void Run_2( string path )
        {
            using var env = Env.Create();

            var n = 0;
            var array_1 = new string[ 1 ];
            foreach ( var fn in EnumerateAllFiles( path ) )
            {
                var text = File.ReadAllText( fn );

                var sents = env.SentSplitter.AllocateSents( text );
                
                var sentTexts = sents.Select( (s, i) => $"{++i}). '{s.GetValue( text ).Cut().Norm()}'" ).Take( 25 );
                if ( 25 < sents.Count )
                {
                    array_1[ 0 ] = $"..more {sents.Count - 25}..."; 
                    sentTexts = sentTexts.Concat( array_1 );
                }

                Console_Write( $"{++n}.) ", ConsoleColor.DarkGray );
                Console.Write( $"text: " );
                Console_WriteLine( $"'{text.Cut().Norm()}'", ConsoleColor.DarkGray );
                if ( sentTexts.Any() )
                    Console.WriteLine( "  " + string.Join( "\r\n  ", sentTexts ) );
                else
                    Console_WriteLine( "  [sents is not found (?)]", ConsoleColor.DarkRed );
                Console.WriteLine();
            }
        }

        private static IEnumerable< string > EnumerateAllFiles( string path, string searchPattern = "*.txt" )
        {
            try
            {
                var seq = Directory.EnumerateDirectories( path ).SafeWalk().SelectMany( path => EnumerateAllFiles( path ) );
                return (seq.Concat( Directory.EnumerateFiles( path, searchPattern )/*.SafeWalk()*/ ));
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( $"{ex.GetType().Name}: '{ex.Message}'" );
                return (Enumerable.Empty< string >());
            }
        }

        private static void Console_Write( string msg, ConsoleColor color )
        {
            var fc = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write( msg );
            Console.ForegroundColor = fc;
        }
        private static void Console_WriteLine( string msg, ConsoleColor color )
        {
            var fc = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine( msg );
            Console.ForegroundColor = fc;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal static class Extensions
    {
        public static string Cut( this string s, int max_len = 150 ) => (max_len < s.Length) ? s.Substring( 0, max_len ) + "..." : s;
        public static string Norm( this string s ) => s.Replace( '\n', ' ' ).Replace( '\r', ' ' ).Replace( '\t', ' ' ).Replace( "  ", " " ).TrimStart(' ');
        public static bool IsNullOrEmpty( this string value ) => string.IsNullOrEmpty( value );
        public static IEnumerable< T > SafeWalk< T >( this IEnumerable< T > source )
        {
            using ( var enumerator = source.GetEnumerator() )
            {
                for (; ; )
                {
                    try
                    {
                        if ( !enumerator.MoveNext() )
                            break;
                    }
                    catch ( Exception ex )
                    {
                        Debug.WriteLine( $"{ex.GetType().Name}: '{ex.Message}'" );
                        continue;
                    }

                    yield return (enumerator.Current);
                }
            }
        }
    }
}
