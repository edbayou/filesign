using System;
using System.IO;
using System.Threading;

namespace FileSignature
{
	internal class BufferedTextWriter
	{
		private readonly TextWriter _writer;
		private int _nextBlockNumber;
		private readonly string[] _buffer;
		private readonly object _locker;

		public BufferedTextWriter(TextWriter writer, int bufferSize)
		{
			if (writer == null) throw new ArgumentNullException(nameof(writer));
			if (bufferSize < 0) throw new ArgumentOutOfRangeException(nameof(bufferSize));

			_writer = writer;
			_buffer = new string[bufferSize];
			_locker = new object();
		}

		/// <remarks>
		/// Puts the calling thread to sleep if blockNumber is too large
		/// </remarks>
		public void Write(int blockNumber, string blockValue)
		{
			if (blockNumber < _nextBlockNumber) ThrowBlockAlreadyWritten(blockNumber);
			if (blockValue == null) throw new ArgumentNullException(nameof(blockValue));

			lock (_locker)
			{
				int maxBlockNumber = _nextBlockNumber + _buffer.Length;
				while (blockNumber > maxBlockNumber || blockNumber == maxBlockNumber && _buffer.Length != 0)
				{
					Monitor.Wait(_locker);
					maxBlockNumber = _nextBlockNumber + _buffer.Length;
				}

				if (_buffer.Length == 0)
				{
					_writer.Write(blockValue);
					ShiftBlocks(1);
					return;
				}

				int bufferIndex = blockNumber - _nextBlockNumber;
				if (_buffer[bufferIndex] != null)
				{
					ThrowBlockAlreadyWritten(blockNumber);
				}

				_buffer[blockNumber - _nextBlockNumber] = blockValue;
				if (blockNumber == _nextBlockNumber)
				{
					int shift = CalculateShift();
					WriteBlocks(blockNumber,shift);
					ShiftBlocks(shift);
				}
			}
		}

		private void ThrowBlockAlreadyWritten(int blockNumber)
		{
			throw new ArgumentOutOfRangeException($"Block with same number ({blockNumber}) has already written");
		}

		private void ShiftBlocks(int shift)
		{
			for (int i = 0; i < _buffer.Length; ++i)
			{
				if (i >= shift)
					_buffer[i - shift] = _buffer[i];
				_buffer[i] = null;
			}
			_nextBlockNumber += shift;
			Monitor.PulseAll(_locker);
		}

		private void WriteBlocks(int blockNumber,int shift)
		{
			for (int i = 0; i < shift; ++i)
			{
				
				Console.WriteLine($"{blockNumber} - {_buffer[i]}");}
		}

		private int CalculateShift()
		{
			int shift = 0;
			while (shift < _buffer.Length && _buffer[shift] != null)
				++shift;
			return shift;
		}
	}
}
