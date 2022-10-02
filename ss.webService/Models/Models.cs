using System;
using System.Collections.Generic;
using System.Linq;

using lingvo.sentsplitting;
using JP = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace ss.webService
{
    /// <summary>
    /// 
    /// </summary>
    public struct InitParamsVM
    {
        public string Text { get; set; }
#if DEBUG
        public override string ToString() => Text;
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    internal readonly struct ResultVM
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly struct sent_info
        {
            [JP("startIndex")] public int    startIndex { get; init; }
            [JP("length")    ] public int    length     { get; init; }
            [JP("text")      ] public string text       { get; init; }
        }

        public ResultVM( in InitParamsVM m, Exception ex ) : this() => (init_params, exception_message) = (m, ex.Message);
        public ResultVM( in InitParamsVM m, IList< sent_t > sents ) : this()
        {
            init_params = m;
            if ( sents != null && sents.Count != 0 )
            {
                var originalText = m.Text;
                sent_infos = (from sent in sents
                              select
                                  new sent_info()
                                  {
                                      text       = sent.GetValue( originalText ),
                                      startIndex = sent.startIndex,
                                      length     = sent.length,
                                  }
                             ).ToList();
            }
        }

        [JP("ip")   ] public InitParamsVM               init_params       { get; }
        [JP("sents")] public IReadOnlyList< sent_info > sent_infos        { get; }
        [JP("err")  ] public string                     exception_message { get; }
    }
}
