using System.Collections.Concurrent;
using System.Threading;

namespace Jint.Runtime;

internal sealed record EventLoop
{
    private static readonly object _lock = new();

    private readonly ConcurrentQueue<Action> _events = new();

    public static void NotifyLoop()
    {
        lock (_lock)
            Monitor.PulseAll(_lock);
    }

    public void Enqueue(Action continuation)
    {
        _events.Enqueue(continuation);

        lock (_lock)
            Monitor.Pulse(_lock);
    }

    public void DoProcessEventLoop()
    {
        DoProcessEventLoop(_events);
    }

    public void DoProcessEventLoop(ManualResetEventSlim until)
    {
        DoProcessEventLoop(_events, until);
    }

    private static void DoProcessEventLoop(ConcurrentQueue<Action> queue, ManualResetEventSlim until)
    {
        // We need to run all continuations at least once even if event is set
        while (true)
        {
            while (queue.TryDequeue(out var nextContinuation))
            {
                // note that continuation can enqueue new events
                nextContinuation();
            }

            if (until.IsSet)
                break;
            
            lock (_lock)
            {
                // Fragile as NotifyLoop must be called when setting ManualResetEventSlim
                if (!until.IsSet)
                    Monitor.Wait(_lock);
            }
        }
    }

    private static void DoProcessEventLoop(ConcurrentQueue<Action> queue)
    {
        while (queue.TryDequeue(out var nextContinuation))
        {
            // note that continuation can enqueue new events
            nextContinuation();
        }
    }
}
