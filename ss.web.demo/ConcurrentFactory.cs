using System;
using System.Collections.Concurrent;
using System.Threading;

namespace lingvo.sentsplitting
{
    /// <summary>
    /// 
    /// </summary>
	internal sealed class ConcurrentFactory //: IDisposable
	{
		private Semaphore                       _Semaphore;
        private ConcurrentStack< SentSplitter > _Stack;
		//private bool                              _IsDisposed;

        public ConcurrentFactory( SentSplitterConfig config, int instanceCount )
		{
            if ( instanceCount <= 0 ) throw (new ArgumentException("instanceCount"));
            if ( config == null     ) throw (new ArgumentNullException("config"));

            _Semaphore = new Semaphore( instanceCount, instanceCount );
            _Stack     = new ConcurrentStack< SentSplitter >();
			for ( int i = 0; i < instanceCount; i++ )
			{
                _Stack.Push( new SentSplitter( config ) );
			}
		}

        public sent_t[] AllocateSents( string text, bool splitBySmiles )
		{
            /*if ( _IsDisposed )
			{
				throw (new ObjectDisposedException( this.GetType().Name ));
			}*/

			_Semaphore.WaitOne();
			var worker = default(SentSplitter);
			try
			{
                worker = _Stack.Pop();
                if ( worker == null )
                {
                    for ( var i = 0; ; i++ )
                    {
                        worker = _Stack.Pop();
                        if ( worker != null )
                            break;

                        Thread.Sleep( 25 ); //SpinWait.SpinUntil(

                        if ( 10000 <= i )
                            throw (new InvalidOperationException( this.GetType().Name + ": no (fusking) worker item in queue" ));
                    }
                }

                var result = worker.AllocateSents( text, splitBySmiles ).ToArray();
                return (result);
			}
			finally
			{
				if ( worker != null )
				{
					_Stack.Push( worker );
				}
				_Semaphore.Release();
			}

            throw (new InvalidOperationException( this.GetType().Name + ": nothing to return (fusking)" ));
		}

        /*public bool IsDisposed
		{
			get { return _IsDisposed; }
		}
		public void Dispose()
		{
            if ( !_IsDisposed )
			{
				_IsDisposed = true;
				for ( int i = 0; i < _InstanceCount; i++ )
				{
					_Semaphore.WaitOne();
				}
                /*foreach ( var sentSplitter in _SentSplitters )
                {
                    sentSplitter.Dispose();
                }* /
                _Semaphore.Release( _InstanceCount );
				_Semaphore = null;
				_Stack = null;				
			}
		}*/
	}

    /// <summary>
    /// 
    /// </summary>
    internal static class ConcurrentFactoryExtensions
    {
        public static T Pop< T >( this ConcurrentStack< T > stack )
        {
            var t = default(T);
            if ( stack.TryPop( out t ) )
                return (t);
            return (default(T));
        }
    }
}
