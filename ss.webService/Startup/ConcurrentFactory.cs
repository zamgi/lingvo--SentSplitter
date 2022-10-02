using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using lingvo.sentsplitting;

namespace ss.webService
{
    /// <summary>
    /// 
    /// </summary>
	public sealed class ConcurrentFactory : IDisposable
	{
		private readonly SemaphoreSlim                   _Semaphore;
        private readonly ConcurrentStack< SentSplitter > _Stack;

        public ConcurrentFactory( SentSplitterConfig config, int instanceCount )
		{
            if ( instanceCount <= 0 ) throw (new ArgumentException( nameof(instanceCount) ));
            if ( config == null     ) throw (new ArgumentNullException( nameof(config) ));

            _Semaphore = new SemaphoreSlim( instanceCount, instanceCount );
            _Stack     = new ConcurrentStack< SentSplitter >();
			for ( int i = 0; i < instanceCount; i++ )
			{
                _Stack.Push( new SentSplitter( config ) );
			}
		}
        public void Dispose()
        {
            foreach ( var worker in _Stack )
			{
                worker.Dispose();
			}
			_Semaphore.Dispose();
        }

        public async Task< IList< sent_t > > Run( string text )
		{
			await _Semaphore.WaitAsync().ConfigureAwait( false );
			var worker = default(SentSplitter);
			var result = default(IList< sent_t >);
			try
			{
                worker = Pop( _Stack );
                result = worker.AllocateSents( text );
			}
			finally
			{
                if ( worker != null )
				{
                    _Stack.Push( worker );
				}
				_Semaphore.Release();
			}
			return (result);
		}

        private static T Pop< T >( ConcurrentStack< T > stack ) => stack.TryPop( out var t ) ? t : default;
	}
}
