using System;
using System.Threading;

namespace FileSignature
{
	internal class SemaphoreSlim
	{
		private int _currentValue;
		private readonly int _maxValue;
		private readonly object _locker;

		public SemaphoreSlim(int currentValue, int maxValue)
		{
			if (currentValue < 0 || maxValue < currentValue)
				throw new ArgumentOutOfRangeException(nameof(currentValue) + " " + nameof(maxValue));

			_currentValue = currentValue;
			_maxValue = maxValue;
			_locker = new object();
		}

		public int CurrentValue => Thread.VolatileRead(ref _currentValue);

		public void WaitOne()
		{
			lock (_locker)
			{
				while (_currentValue == 0)
					Monitor.Wait(_locker);
				_currentValue--;
			}
		}

		public void Release()
		{
			lock (_locker)
			{
				if (_currentValue == _maxValue)
					throw new SemaphoreFullException();
				_currentValue++;
				Monitor.Pulse(_locker);
			}
		}
	}
}
