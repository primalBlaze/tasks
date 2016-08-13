namespace PrimalBlaze.Threading
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.ExceptionServices;
	using System.Threading;
	using System.Threading.Tasks;

	/// <summary>
	/// Contains static methods in a similar vein to the static members of <see cref="Task"/>.
	/// </summary>
	public static class Tasks
	{
		/// <summary>
		/// Creates a task that will complete when all of the <see cref="Task"/> objects in an array have completed, or
		/// when an <see cref="Exception"/> is detected as thrown from one of those Task objects. It will notify the
		/// specified <paramref name="workerCancellationSource"/> if a fault occurs.
		/// </summary>
		/// <param name="workerCancellationSource">
		/// A <see cref="CancellationTokenSource"/> which will be requested to Cancel if one of the <paramref name="workerTasks"/>
		/// faults. It is expected, but not required, that the worker tasks monitor this source's <see cref="CancellationTokenSource.Token"/>.
		/// </param>
		/// <param name="workerTasks">
		/// The <see cref="Task"/>s to wait on for completion.
		/// </param>
		/// <returns>
		/// A task that represents the completion of all of the supplied tasks, or the faulted state
		/// of at least one of the tasks.
		/// </returns>
		/// <remarks>
		/// The first <see cref="Exception"/>, if any, detected as thrown within any of the tasks will be rethrown.
		/// 
		/// If a <paramref name="workerCancellationSource"/> is provided then this will be used to notify when one of the tasks
		/// faults. This can be used by the calling method to cancel the other tasks. For automated cancellation of the other
		/// tasks, consider using one of the overloads which accepts a taskFactories parameter, e.g.
		/// <see cref="WhenAllCompleteOrOneFaults(CancellationToken, Func{CancellationToken, Task}[])"/>.
		/// </remarks>
		public async static Task WhenAllCompleteOrOneFaults(CancellationTokenSource workerCancellationSource, params Task[] workerTasks) {
			if (workerCancellationSource == null) throw new ArgumentNullException(nameof(workerCancellationSource));

			await WhenAllCompleteOrOneFaultsInternal(workerTasks, workerCancellationSource);
		}

		/// <summary>
		/// Creates a task that will complete when all of the <see cref="Task"/> objects in an array have completed, or
		/// when an <see cref="Exception"/> is detected as thrown from one of those Task objects.
		/// </summary>
		/// <param name="workerTasks">The <see cref="Task"/>s to wait on for completion.</param>
		/// <returns>A task that represents the completion of all of the supplied tasks, or the faulted state
		/// of at least one of the tasks.</returns>
		/// <remarks>
		/// The first <see cref="Exception"/>, if any, detected as thrown within any of the tasks will be rethrown.
		/// 
		/// This overload makes no attempt to notify or cancel other worker tasks if one faults. Use the
		/// <see cref="WhenAllCompleteOrOneFaults(CancellationTokenSource, Task[])"/> overload to notify a
		/// specified <see cref="CancellationTokenSource"/>, or one of the overloads which accepts the taskFactories
		/// parameter for automatic cancellation of other tasks.
		/// </remarks>
		public async static Task WhenAllCompleteOrOneFaults(params Task[] workerTasks) {
			await WhenAllCompleteOrOneFaultsInternal(workerTasks, GenerateInternalCancellationSource(CancellationToken.None));
		}

		/// <summary>
		/// Generates an internal <see cref="CancellationTokenSource"/> which will be cancelled if an external
		/// <see cref="CancellationToken"/> is cancelled.
		/// </summary>
		/// <param name="hostCancellationToken">The external cancellation token which should trigger cancellation
		/// of this source.</param>
		/// <returns>The internal <see cref="CancellationTokenSource"/>.</returns>
		private static CancellationTokenSource GenerateInternalCancellationSource(CancellationToken hostCancellationToken) {
			var internalCts = new CancellationTokenSource();
			if (hostCancellationToken != CancellationToken.None) {
				hostCancellationToken.Register(internalCts.Cancel);
			};
			return internalCts;
		}

		/// <summary>
		/// Creates a task that will complete when all of the <see cref="Task"/> objects created by an array of factories
		/// have completed, or when an <see cref="Exception"/> is detected as thrown from one of those Task objects. It 
		/// uses the specified <paramref name="options"/>, and can be cancelled by the specified 
		/// <paramref name="hostCancellationToken"/>.
		/// </summary>
		/// <param name="options">
		/// Flags specifying various <see cref="TaskCancellationOptions"/>.
		/// </param>
		/// <param name="hostCancellationToken">
		/// A <see cref="CancellationToken"/> which can be used to cancel the Wait.
		/// </param>
		/// <param name="taskFactories">
		/// An array of <see cref="Func{CancellationToken, Task}"/> which generate the worker
		/// <see cref="Task"/>s which will be waited upon. Each factory method is passed an internal 
		/// <see cref="CancellationToken"/> which the Task can choose to monitor to determine whether one of its peer Tasks
		/// has faulted.
		/// </param>
		/// <returns>
		/// A task that represents the completion of all of the supplied tasks, or the faulted state
		/// of at least one of the tasks.
		/// </returns>
		/// <remarks>
		/// The first <see cref="Exception"/>, if any, detected as thrown within any of the tasks will be rethrown.
		/// 
		/// Each task produced by the <paramref name="taskFactories" /> can elect to monitor the internal 
		/// <see cref="CancellationToken"/> which it is supplied. This token's <see cref="CancellationToken.IsCancellationRequested"/>
		/// property will be set to <b>true</b> (<b>True</b> in VB .NET) if an exception is detected as having been
		/// thrown by any of the Tasks. In this way, work need not continue in one task if another has failed.
		/// 
		/// The exceptions thrown by worker tasks may become unobserved due to a race condition where the cancellation
		/// of a peer <see cref="Task"/> may occur before the original exception is handled, triggering an 
		/// <see cref="OperationCanceledException"/> first. The default behavior is to capture the original exception
		/// using <see cref="ExceptionDispatchInfo"/>, then re-throw it in preference to the OperationCanceledException.
		/// To prevent this behaviour, specify the <see cref="TaskCancellationOptions.DisableExceptionChanneling"/> option.
		/// </remarks>
		public async static Task WhenAllCompleteOrOneFaults(TaskCancellationOptions options, CancellationToken hostCancellationToken, params Func<CancellationToken, Task>[] taskFactories) {
			var internalCts = GenerateInternalCancellationSource(hostCancellationToken);

			ExceptionDispatchInfo ex = null;
			try {
				var workerTasks = taskFactories.Select((f) => f(internalCts.Token));
				if (!options.HasFlag(TaskCancellationOptions.DisableExceptionChanneling)) {
					workerTasks = workerTasks.Select(t => t.ContinueWith(t2 => {
						ex = ExceptionDispatchInfo.Capture(t2.Exception.Flatten().InnerException);
						internalCts.Cancel();
						return t2;
					}, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously)).ToArray();
				};

				await WhenAllCompleteOrOneFaultsInternal(workerTasks, internalCts);
			}
			catch (OperationCanceledException) {
				////Not needed as ex will always be null if this flag is not set
				//if (!options.HasFlag(TaskCancellationOptions.DisableExceptionChanneling))
				if (ex != null) ex.Throw(); //throw the original exception

				throw;  //only rethrow the cancellationException if there was no original
			}
		}

		/// <summary>
		/// Creates a task that will complete when all of the <see cref="Task"/> objects created by an array of factories
		/// have completed, or when an <see cref="Exception"/> is detected as thrown from one of those Task objects. It 
		/// can be cancelled by the specified <paramref name="hostCancellationToken"/>.
		/// </summary>
		/// <param name="hostCancellationToken">
		/// A <see cref="CancellationToken"/> which can be used to cancel the Wait.
		/// </param>
		/// <param name="taskFactories">
		/// An array of <see cref="Func{CancellationToken, Task}"/> which generate the worker
		/// <see cref="Task"/>s which will be waited upon. Each factory method is passed an internal 
		/// <see cref="CancellationToken"/> which the Task can choose to monitor to determine whether one of its peer Tasks
		/// has faulted.
		/// </param>
		/// <returns>
		/// A task that represents the completion of all of the supplied tasks, or the faulted state
		/// of at least one of the tasks.
		/// </returns>
		/// <remarks>
		/// The first <see cref="Exception"/>, if any, detected as thrown within any of the tasks will be rethrown.
		/// 
		/// Each task produced by the <paramref name="taskFactories" /> can elect to monitor the internal 
		/// <see cref="CancellationToken"/> which it is supplied. This token's <see cref="CancellationToken.IsCancellationRequested"/>
		/// property will be set to <b>true</b> (<b>True</b> in VB .NET) if an exception is detected as having been
		/// thrown by any of the Tasks. In this way, work need not continue in one task if another has failed.
		/// 
		/// The exceptions thrown by worker tasks may become unobserved due to a race condition where the cancellation
		/// of a peer <see cref="Task"/> may occur before the original exception is handled, triggering an 
		/// <see cref="OperationCanceledException"/> first. The default behavior is to capture the original exception
		/// using <see cref="ExceptionDispatchInfo"/>, then re-throw it in preference to the OperationCanceledException.
		/// To prevent this behaviour, specify the <see cref="TaskCancellationOptions.DisableExceptionChanneling"/> option
		/// using the <see cref="WhenAllCompleteOrOneFaults(TaskCancellationOptions, CancellationToken, Func{CancellationToken, Task}[])"/>
		/// overload.
		/// </remarks>
		public async static Task WhenAllCompleteOrOneFaults(CancellationToken hostCancellationToken, params Func<CancellationToken, Task>[] taskFactories) {
			await WhenAllCompleteOrOneFaults(TaskCancellationOptions.None, hostCancellationToken, taskFactories);
		}

		/// <summary>
		/// Creates a task that will complete when all of the <see cref="Task"/> objects in an array have completed, or
		/// when an <see cref="Exception"/> is detected as thrown from one of those Task objects. It will notify the
		/// specified <paramref name="internalCts"/> if a fault occurs.
		/// </summary>
		/// <param name="workerTasks">
		/// The <see cref="Task"/>s to wait on for completion.
		/// </param>
		/// <param name="internalCts">
		/// A <see cref="CancellationTokenSource"/> which will be requested to Cancel if one of the <paramref name="workerTasks"/>
		/// faults.
		/// </param>
		/// <returns>
		/// The first <see cref="Exception"/>, if any, detected as thrown within any of the tasks will be rethrown.
		/// </returns>
		private async static Task WhenAllCompleteOrOneFaultsInternal(IEnumerable<Task> workerTasks, CancellationTokenSource internalCts) {
			var cancelMonitorTask = Task.Delay(Timeout.Infinite, internalCts.Token);
			await Task.WhenAll(workerTasks.Select(async (t) => {
				await Task.WhenAny(t.ContinueWith(t2 => {
					internalCts.Cancel();
					return t2;
				}, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously), cancelMonitorTask);
			}));

			await Task.WhenAll(workerTasks);
		}
	}
}
