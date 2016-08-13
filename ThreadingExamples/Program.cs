namespace ThreadingExamples
{
	using PrimalBlaze.Threading;
	using System;
	using System.Threading;
	using System.Threading.Tasks;

	class Program
	{
		static void Main(string[] args) {
			string input;
			ShowMenu();
			do {
				input = Console.ReadLine();
				if (input != string.Empty) {
					if (input == "1") {
						RunWhenAllCompleteOrOneFaultsThreeFilesExample();
					};
					ShowMenu();
				}
			} while (input != string.Empty);
		}

		static void ShowMenu() {
			Console.WriteLine("Select an example:");
			Console.WriteLine("  WhenAllCompletesOrOneFaults");
			Console.WriteLine("    1) Wait for 3 tasks. One throws an exception; this may abort the other tasks.");
			Console.WriteLine("Or hit Enter to exit.");
		}

		static void RunWhenAllCompleteOrOneFaultsThreeFilesExample() {
			Console.WriteLine("Task 1 will complete in 3 seconds.");
			Console.WriteLine("Task 2 will fail after 4 seconds.");
			Console.WriteLine("Task 3 will be cancelled after the failure of Task 2.");
			Console.WriteLine("Press enter within 10 seconds to externally trigger cancellation.");

			var externalCancellationTokenSource = new CancellationTokenSource();

			var monitorTask = new Task(() => {
				for (var i = 0; i < 10; i++) {
					if (externalCancellationTokenSource.IsCancellationRequested) return;
					Thread.Sleep(1000);
					Console.WriteLine($"{i + 1,2} second(s) elapsed.");
				}
			});
			monitorTask.Start();

			Task.Run(() => {
				Task.WhenAny(Task.Delay(10000, externalCancellationTokenSource.Token), Task.Run(async () => {
					await Console.In.ReadLineAsync();
					externalCancellationTokenSource.Cancel();
				}));
				if (externalCancellationTokenSource.IsCancellationRequested) {
					Console.WriteLine("Cancellation WAS externally triggered.");
				}
				else {
					Console.WriteLine("Cancellation was NOT externally triggered.");
				};
			});

			try {
				RunWhenAllCompleteOrOneFaultsThreeFilesExampleAsync(externalCancellationTokenSource.Token).Wait();
			}
			catch (AggregateException ex) {
				Console.WriteLine($"Exception caught by wrapper method: {ex.Flatten().InnerException}");
			}

			monitorTask.Wait();

			Console.WriteLine("Example complete.");
			Console.WriteLine();
		}

		static async Task RunWhenAllCompleteOrOneFaultsThreeFilesExampleAsync(CancellationToken cancellationToken) {
			await Tasks.WhenAllCompleteOrOneFaults(cancellationToken,
				(internalToken) => DoWork("Task 1", 3000, 1000, internalToken),
				(internalToken) => DoWorkButFail("Task 2", 5000, 1000, 4000, internalToken),
				(internalToken) => DoWork("Task 3", 10000, 2000, internalToken)
			);
		}

		static async Task DoWork(string id, int forHowLong, int checkForCancelEvery, CancellationToken cancellationToken) {
			var iterations = forHowLong / checkForCancelEvery;
			int i = 0;
			try {
				for (i = 0; i < iterations; i++) {
					cancellationToken.ThrowIfCancellationRequested();
					await Console.Out.WriteLineAsync($"{id}: iteration {i + 1}.");
					await Task.Delay(checkForCancelEvery);
				}
			}
			catch (OperationCanceledException) {
				await Console.Out.WriteLineAsync($"{id}: CANCELED before iteration {i + 1}.");
				throw;
			}
			await Console.Out.WriteLineAsync($"{id}: COMPLETED {i + 1} iterations.");
		}

		static async Task DoWorkButFail(string id, int forHowLong, int checkForCancelEvery, int failAfter, CancellationToken cancellationToken) {
			var iterations = forHowLong / checkForCancelEvery;
			var failAfterIteration = failAfter / checkForCancelEvery;
			int i = 0;
			try {
				for (i = 0; i < iterations; i++) {
					if (i == failAfterIteration) {
						await Console.Out.WriteLineAsync($"{id}: FAILING at iteration {i + 1}.");
						throw new InvalidOperationException("Failing.");
					};
					cancellationToken.ThrowIfCancellationRequested();
					await Console.Out.WriteLineAsync($"{id}: iteration {i + 1}.");
					await Task.Delay(checkForCancelEvery);
				}
			}
			catch (OperationCanceledException) {
				await Console.Out.WriteLineAsync($"{id}: CANCELED before iteration {i + 1}.");
				throw;
			}
			await Console.Out.WriteLineAsync($"{id}: COMPLETED {i + 1} iterations.");
		}
	}
}
