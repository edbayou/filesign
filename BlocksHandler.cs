using System;
using System.Security.Cryptography;
using System.Threading;

namespace FileSignature
{
	internal class BlocksHandler
	{
		private readonly SemaphoreSlim _handledCounter;
		private readonly BufferedTextWriter _writer;
		private readonly int _counterStartValue;
		private int _noBlocksExpected;

		public ManualResetEvent WorkDoneAwaiter;

		public BlocksHandler(SemaphoreSlim handledCounter, BufferedTextWriter writer)
		{
			if (handledCounter == null) throw new ArgumentNullException(nameof(handledCounter));
			if (writer == null) throw new ArgumentNullException(nameof(writer));

			_handledCounter = handledCounter;
			_writer = writer;
			_counterStartValue = handledCounter.CurrentValue;
			WorkDoneAwaiter = new ManualResetEvent(false);
		}

		private static string ToOutputFormat(Block block)
		{
			using (var sha256 = SHA256.Create())
			{
				var hash = sha256.ComputeHash(block.Bytes, 0, block.Size);
				var output = BitConverter.ToString(hash).Replace("-","");
				return output;
			}
		}

		private void HandleBlock(Block block)
		{
			string blockString = ToOutputFormat(block);
			_writer.Write(block.Number, blockString);
			_handledCounter.Release();
			UpdateAwaiter();
		}

		private void UpdateAwaiter()
		{
			Thread.MemoryBarrier();
			if (Thread.VolatileRead(ref _noBlocksExpected) != 0)
				if (_counterStartValue == _handledCounter.CurrentValue)
					WorkDoneAwaiter.Set();
		}

		public void HandleBlockAsync(Block block)
		{
			FixedSizeThreadPool.QueueAction(() => HandleBlock(block));
		}

		public void EndOfBlocks()
		{
			Interlocked.Exchange(ref _noBlocksExpected, 1);
			UpdateAwaiter();
		}
	}
}
