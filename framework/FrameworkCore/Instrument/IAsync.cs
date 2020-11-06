using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace FrameworkCore.Instrument
{
    public interface IAwaiter<out T> : INotifyCompletion
    {
        bool IsCompleted { get; }
        T GetResult();
    }

    public interface IAwaitable<out T>
    {
        IAwaiter<T> GetAwaiter();
    }

    public class MyWaiter<T> : IAwaitable<T>, IAwaiter<T>
    {
        public bool IsCompleted { get; private set; }

        public IAwaiter<T> GetAwaiter() => this;

        public T GetResult() { IsCompleted = false; return _result; }

        public void OnCompleted(Action continuation)
        {
            continuation?.Invoke();
        }

        T _result;
        public void SetResult(T t)
        {
            _result = t;
            IsCompleted = true;
        }
    }
}
/*
// https://github.com/walterlv/sharing-demo/blob/master/src/Walterlv.Core/Threading/AwaiterInterfaces.cs
public interface IAwaiter : INotifyCompletion
{
    bool IsCompleted { get; }

    void GetResult();
}

public interface IAwaitable<out TAwaiter> where TAwaiter : IAwaiter
{
    TAwaiter GetAwaiter();
}

public interface IAwaiter<out TResult> : INotifyCompletion
{
    bool IsCompleted { get; }

    TResult GetResult();
}

public interface IAwaitable<out TAwaiter, out TResult> where TAwaiter : IAwaiter<TResult>
{
    TAwaiter GetAwaiter();
}

public interface ICriticalAwaiter : IAwaiter, ICriticalNotifyCompletion
{
}

public interface ICriticalAwaiter<out TResult> : IAwaiter<TResult>, ICriticalNotifyCompletion
{
}

public interface IAwaiter<out T> : INotifyCompletion
{
    bool IsCompleted { get; }
    T Result { get; }
    T GetResult();
}

public interface IAwaitable<out T>
{
    IAwaiter<T> GetAwaiter();
}

public class AwaitableFunc<T> : IAwaitable<T>
{
    private Func<T> fun = null;

    public IAwaiter<T> GetAwaiter()
    {
        return new InnerAwaitableImplement(fun);
    }

    public AwaitableFunc(Func<T> func)
    {
        fun = func;
    }

    private class InnerAwaitableImplement : IAwaiter<T>
    {
        private Func<T> fun = null;
        private bool isFinished = false;
        private T result = default(T);

        public InnerAwaitableImplement(Func<T> func)
        {
            fun = func;
        }

        public bool IsCompleted => isFinished;
        public T Result => GetResult();

        public void OnCompleted(Action continuation)
        {
            ThreadPool.QueueUserWorkItem(obj =>
            {
                isFinished = true;
                continuation();
            }, null);
        }

        public T GetResult()
        {
            result = fun();
            return result;
        }
    }
}
}
*/