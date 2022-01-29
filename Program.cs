using System.Linq.Expressions;
using System;
using System.IO;

namespace FileSignature
{
	internal static class Program
	{
		static void Main(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

			string fileName; // = "C:/123.pdf"; for test
			int blockSize; //= 100000
			GetFileNameAndBlockSize(out fileName, out blockSize);

			using (var fs = new FileStream(fileName, FileMode.Open))
			{
				int processors = Environment.ProcessorCount;
				var syncObject = new SemaphoreSlim(processors * 2, processors * 2);
				var reader = new AdvancedStreamReader(fs, blockSize, syncObject);
				var bufferedWriter = new BufferedTextWriter(Console.Out, processors);
				var blocksHandler = new BlocksHandler(syncObject, bufferedWriter);
				reader.BlockReaded += blocksHandler.HandleBlockAsync;
				reader.EndOfStream += blocksHandler.EndOfBlocks;
				reader.ProcessRead();
				blocksHandler.WorkDoneAwaiter.WaitOne();
			}
		}

		static void GetFileNameAndBlockSize(out string fileName, out int blockSize)
		{
			GetParam:
			fileName = "";
			blockSize = 0;
			Console.WriteLine("Enter file:");
			string _fileName = Console.ReadLine();
			Console.WriteLine("Enter block size:");
			string _blockSize = Console.ReadLine();
			try{
				if(File.Exists(_fileName)){
					fileName = _fileName;
				}
				else {
					throw new ArgumentException("File is not exist");
				}
				
				if(int.TryParse(_blockSize,out int newBlockSize)) {
					if (newBlockSize <= 0){
						throw new ArgumentException("Block Size should be more then 0");
					}
					blockSize = newBlockSize;
				}
				else{
					throw new ArgumentException("Block Size should be number");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				goto GetParam;
			}
		}

		private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Logger.Log((Exception)e.ExceptionObject);
			Environment.Exit(1);
		}
	}
}