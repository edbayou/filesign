using System;
using System.Collections.Generic;
using System.Threading;

namespace FileSignature
{
	internal static class FixedSizeThreadPool
	{
		private static readonly Queue<Action> Work;
		private static readonly List<Thread> Threads;

		static FixedSizeThreadPool()
		{
			Work = new Queue<Action>();
			Threads = new List<Thread>();
			int processorCount = Environment.ProcessorCount;
			for (int i = 0; i < processorCount; ++i)
			{
				var thread = new Thread(DoWork) {IsBackground = true};
				thread.Start();
				Threads.Add(thread);
			}
		}

		private static void DoWork()
		{
			while (true)
			{
				Action work;
				lock (Work)
				{
					while (Work.Count == 0)
						Monitor.Wait(Work);
					work = Work.Dequeue();
				}
				work();
			}
		}

		public static void QueueAction(Action action)
		{
			if (action == null)
				throw new ArgumentNullException(nameof(action));
			lock (Work)
			{
				Work.Enqueue(action);
				Monitor.Pulse(Work);
			}	
		}
	}
}
