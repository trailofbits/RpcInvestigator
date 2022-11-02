using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Microsoft.CSharp;
using System.CodeDom.Compiler;

namespace RpcInvestigator
{
    public static class TaskWorker
    {
        public class TaskWorkerResult<T>
        {
            public StringBuilder Messages;
            public T TaskResult;
        }

        public
        static
        async
        Task<List<TaskWorkerResult<T>>>
        Run<T, T2>(
            List<T2> Input,
            int WorkSize,
            Func<List<T2>, Task<TaskWorkerResult<T>>> WorkRoutine
            )
        {
            //
            // This routine takes an input array of work items and splices it into
            // multiple smaller lists that are passed to an asynchronous work routine.
            // The results of all those workers is returned.
            //
            var workers = new List<Task<TaskWorkerResult<T>>>();
            for (int i = 0; i < Input.Count; i += WorkSize)
            {
                var input = Input.Skip(i).Take(WorkSize).ToList();
                workers.Add(Task.Run(() => WorkRoutine(input)));
            }
            //
            // If a WorkRoutine task threw an unhandled exception, it will be treated
            // as a TPL AggregateException which itself must be handled, or the whole
            // application will go down.
            //
            var results = await Task.WhenAll(workers.ToArray()).ContinueWith(final =>
            {
                if (final.Exception != null)
                {
                    final.Exception.Flatten().Handle(ex =>
                    {
                        Trace.TraceError("Exception in RunSync: " + ex.Message);
                        return true;
                    });
                }
                return final.Result;
            });
            return results.ToList();
        }

        public
        static
        async
        Task<bool>
        Run<T>(
            List<T> Input,
            int WorkSize,
            Func<List<T>, Task<bool>> WorkRoutine
            )
        {
            //
            // This routine takes an input array of work items and splices it into
            // multiple smaller lists that are passed to an asynchronous work routine.
            // The results of all those workers is returned.
            //
            var workers = new List<Task<bool>>();
            for (int i = 0; i < Input.Count; i += WorkSize)
            {
                var input = Input.Skip(i).Take(WorkSize).ToList();
                workers.Add(Task.Run(() => WorkRoutine(input)));
            }
            //
            // If a WorkRoutine task threw an unhandled exception, it will be treated
            // as a TPL AggregateException which itself must be handled, or the whole
            // application will go down.
            //
            _ = await Task.WhenAll(workers.ToArray()).ContinueWith(final =>
            {
                if (final.Exception != null)
                {
                    final.Exception.Flatten().Handle(ex =>
                    {
                        Trace.TraceError("Exception in RunSync: " + ex.Message);
                        return true;
                    });
                }
                return final.Result;
            });
            return true;
        }

        public
        static
        async
        Task<List<TaskWorkerResult<T>>>
        RunSync<T, T2>(
            List<T2> Input,
            int WorkSize,
            Func<List<T2>, TaskWorkerResult<T>> WorkRoutine
            )
        {
            //
            // This routine takes an input array of work items and splices it into
            // multiple smaller lists that are passed to a synchronous work routine.
            // The results of all those workers is returned.
            //
            var workers = new List<Task<TaskWorkerResult<T>>>();
            for (int i = 0; i < Input.Count; i += WorkSize)
            {
                var input = Input.Skip(i).Take(WorkSize).ToList();
                workers.Add(Task.Run(() => WorkRoutine(input)));
            }
            //
            // If a WorkRoutine task threw an unhandled exception, it will be treated
            // as a TPL AggregateException which itself must be handled, or the whole
            // application will go down.
            //
            var results = await Task.WhenAll(workers.ToArray()).ContinueWith(final =>
            {
                if (final.Exception != null)
                {
                    final.Exception.Flatten().Handle(ex =>
                    {
                        Trace.TraceError("Exception in RunSync: " + ex.Message);
                        return true;
                    });
                }
                return final.Result;
            });
            return results.ToList();
        }
    }
}
