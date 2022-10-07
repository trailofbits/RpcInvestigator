using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Text;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;

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
            var results = await Task.WhenAll(workers.ToArray());
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
            _ = await Task.WhenAll(workers.ToArray());
            return true;
        }
    }
}
