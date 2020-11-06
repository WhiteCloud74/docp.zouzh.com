using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace GatewayService.SocketAdapter
{
    public class SocketAsyncEventArgsPool
    {
        public int EventArgsUsed { get { return _eventArgsUsed; } }
        public int EventArgsCount { get; private set; }
        public int EventArgsMaxUsed { get; private set; }
        int _eventArgsUsed;
        readonly int _multiply = 2;
        int _singleBufferSize;
        BlockingCollection<SocketAsyncEventArgs> _eventArgsPool;
        byte[] _byteArrayBuffer;
        Semaphore _semaphore;

        public void Init(int gatewayMaxCount, int singlePackageMaxSize,
            EventHandler<SocketAsyncEventArgs> OnIoComplete, IPEndPoint remoteEndPoint = null)
        {
            EventArgsCount = gatewayMaxCount * _multiply;
            _singleBufferSize = singlePackageMaxSize;
            _eventArgsUsed = 0;
            _byteArrayBuffer = new byte[_singleBufferSize * EventArgsCount];
            _eventArgsPool = new BlockingCollection<SocketAsyncEventArgs>(EventArgsCount);
            _semaphore = new Semaphore(EventArgsCount, EventArgsCount);

            for (int i = 0; i < EventArgsCount; i++)
            {
                SocketAsyncEventArgs socketAsyncEventArg = new SocketAsyncEventArgs
                {
                    RemoteEndPoint = remoteEndPoint
                };
                socketAsyncEventArg.Completed += OnIoComplete;
                socketAsyncEventArg.SetBuffer(_byteArrayBuffer, i * _singleBufferSize, _singleBufferSize);

                _eventArgsPool.Add(socketAsyncEventArg);
            }
        }

        public void Push(SocketAsyncEventArgs item)
        {
            _eventArgsPool.Add(item);
            Interlocked.Decrement(ref _eventArgsUsed);
            _semaphore.Release();
        }

        public SocketAsyncEventArgs Pop(object userToken)
        {
            _semaphore.WaitOne();
            Interlocked.Increment(ref _eventArgsUsed);
            EventArgsMaxUsed = EventArgsMaxUsed > _eventArgsUsed ? EventArgsMaxUsed : _eventArgsUsed;

            SocketAsyncEventArgs s = _eventArgsPool.Take();
            s.SetBuffer(s.Offset, _singleBufferSize);
            s.UserToken = userToken;
            return s;
        }
    }
}
