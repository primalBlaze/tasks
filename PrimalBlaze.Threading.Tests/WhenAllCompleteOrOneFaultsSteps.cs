namespace UnitTestProject1
{
	using PrimalBlaze.Threading;
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using TechTalk.SpecFlow;
	using Xunit;
	using Xunit.Sdk;

	[Binding]
	class WhenAllCompleteOrOneFaultsSteps
	{
		List<Func<CancellationToken, Task>> _factories;
		List<Task> _tasks;
		List<int> _values;

		Exception _unhandledException;
		Exception _unobservedException;

		CancellationTokenSource _cancellationTokenSource;

		Task _wrapperTask;

		public WhenAllCompleteOrOneFaultsSteps() {
			//if (AppDomain.CurrentDomain.IsDefaultAppDomain()) {
			//	AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
			//};
			TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

			Trace.Listeners.Add(new ConsoleTraceListener());
		}

		//private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
		//	_unhandledException = e.ExceptionObject as Exception;		//need to cast, as non-Exception objects are allowed to be thrown by the CLR, but we can assume never are
		//}

		private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e) {
			_unobservedException = e.Exception.Flatten();
		}


		[BeforeScenario]
		public void InitScenario() {
			_factories = new List<Func<CancellationToken, Task>>();
			_tasks = new List<Task>();
			_values = new List<int>();

			_unhandledException = null;
			_unobservedException = null;

			_cancellationTokenSource = new CancellationTokenSource();

			_wrapperTask = null;
		}

		[StepArgumentTransformation]
		public int[] TransformToArrayOfInt32(string s) {
			return s.Split(',').Select(s2 => int.Parse(s2.Trim())).ToArray();
		}

		[StepArgumentTransformation]
		public TaskStatus TransformToTaskStatus(string s) {
			return (TaskStatus)Enum.Parse(typeof(TaskStatus), s, true);
		}

		[StepArgumentTransformation]
		public TaskNumber TransformToTaskNumber(string s) {
			return (TaskNumber)Enum.Parse(typeof(TaskNumber), s, true);
		}

		[Given(@"I prepare a Task which increments a number every (.*)ms until (.*)")]
		public void GivenIPrepareATaskWhichIncrementsANumberEveryMsUntil(int interval, int max) {
			_values.Add(0);
			var index = _values.Count - 1;
			_tasks.Add(IncrementForABit(index, interval, max, _cancellationTokenSource.Token)/*.Log($"Increment[{index}]: every [{interval}]ms til [{max}].", index)*/);
		}

		[Given(@"I prepare a Factory for a Task which increments a number every (.*)ms until (.*)")]
		public void GivenIPrepareAFactoryForATaskWhichIncrementsANumberEveryMsUntil(int interval, int max) {
			_values.Add(0);
			_tasks.Add(null);
			var index = _values.Count - 1;
			_factories.Add((ct) => {
				var task = IncrementForABit(index, interval, max, ct);
				_tasks[index] = task;
				return task;
			});
		}

		[When(@"I await them using WhenAllCompleteOrOneFaults")]
		public async Task WhenIAwaitThemUsingWhenAllCompleteOrOneFaults() {
			try {
				await (_wrapperTask = Tasks.WhenAllCompleteOrOneFaults(_tasks.ToArray()));
			}
			catch (Exception ex) {
				_unhandledException = ex;
			}
		}

		[When(@"I await them using WhenAllCompleteOrOneFaults with the Cancel Others option")]
		public async Task WhenIAwaitThemUsingWhenAllCompleteOrOneFaultsWithTheCancelOthersOption() {
			try {
				await (_wrapperTask = Tasks.WhenAllCompleteOrOneFaults(_cancellationTokenSource, _tasks.ToArray()));
			}
			catch (Exception ex) {
				_unhandledException = ex;
			}
		}

		[When(@"I await the Factory Tasks using WhenAllCompleteOrOneFaults")]
		public async Task WhenIAwaitTheFactoryTasksUsingWhenAllCompleteOrOneFaults() {
			try {
				await (_wrapperTask = Tasks.WhenAllCompleteOrOneFaults(_cancellationTokenSource.Token, _factories.ToArray()));
			}
			catch (Exception ex) {
				_unhandledException = ex;
			}
		}

		[When(@"the call returns")]
		public void WhenTheCallReturns() {
			try {
				_wrapperTask.Wait();
			}
			catch { }
		}

		[Then(@"both Tasks are (.*)")]
		public void ThenBothTasksAre(TaskStatus expectedStatus) {
			_tasks.ForEach(t => Assert.Equal(expectedStatus, t.Status));
		}

		[Then(@"the results are (.*)")]
		public void ThenTheResultsAre(int[] expectedResults) {
			for (var i = 0; i < expectedResults.Length; i++) {
				Assert.Equal(expectedResults[i], _values[i]);
			}
		}

		[Given(@"I prepare a Task which throws an Exception prior to executing any async code")]
		public void GivenIPrepareATaskWhichThrowsAnExceptionPriorToExecutingAnyAsyncCode() {
			_values.Add(0);
			var index = _values.Count - 1;
			_tasks.Add(IncrementForABit(index, 50, 5, _cancellationTokenSource.Token, FailureOptions.Synchronous)/*.Log($"Increment[{index}]: every 50ms til 5.", index)*/);
		}

		[Given(@"I prepare a Factory for a Task which throws an Exception prior to executing any async code")]
		public void GivenIPrepareAFactoryForATaskWhichThrowsAnExceptionPriorToExecutingAnyAsyncCode() {
			_values.Add(0);
			_tasks.Add(null);
			var index = _values.Count - 1;
			_factories.Add((ct) => {
				var task = IncrementForABit(index, 50, 5, _cancellationTokenSource.Token, FailureOptions.Synchronous);
				_tasks[index] = task;
				return task;
			});
		}

		[Then(@"the (.*) Task is ([a-zA-Z]*)")]
		public void ThenTheTaskIs(TaskNumber taskNumber, TaskStatus expectedStatus) {
			Assert.Equal(expectedStatus, _tasks[(int)taskNumber].Status);
		}

		[Then(@"the (.*) Task is probably (.*)")]
		public void ThenTheTaskIsProbably(TaskNumber taskNumber, TaskStatus expectedStatus) {
			try {
				Assert.Equal(expectedStatus, _tasks[(int)taskNumber].Status);
			}
			catch (EqualException ex) {
				throw new ProbableXunitException(ex);
			}
		}

		[Then(@"the (.*) Result is (\d*)")]
		public void ThenTheResultIs(TaskNumber taskNumber, int expectedResult) {
			Assert.Equal(expectedResult, _values[(int)taskNumber]);
		}


		[Then(@"the (.*) Result is probably less than or equal to (.*)")]
		public void ThenTheResultIsProbably(TaskNumber taskNumber, int expectedResult) {
			try {
				Assert.InRange(_values[(int)taskNumber], 0, expectedResult);
			}
			catch (EqualException ex) {
				throw new ProbableXunitException(ex);
			}
		}

		[Then(@"the Exception was logged")]
		public void ThenTheExceptionWasLogged() {
			Assert.NotNull(_unhandledException);
			Assert.IsType(typeof(InvalidOperationException), _unhandledException);
		}

		[Then(@"no Unobserved Exception was raised")]
		public void ThenNoUnobservedExceptionWasRaised() {
			Assert.Null(_unobservedException);
		}

		[Given(@"I prepare a Task which increments a number every (.*)ms until (.*) but throws an Exception after (.*)")]
		public void GivenIPrepareATaskWhichIncrementsANumberEveryMsUntilButThrowsAnExceptionAfter(int interval, int max, int exceptionAfter) {
			_values.Add(0);
			var index = _values.Count - 1;
			_tasks.Add(IncrementForABit(index, interval, max, _cancellationTokenSource.Token, FailureOptions.Asynchronous, exceptionAfter)/*.Log($"Increment[{index}]: every [{interval}]ms til [{max}].", index)*/);
		}

		[Given(@"I prepare a Factory for a Task which increments a number every (.*)ms until (.*) but throws an Exception after (.*)")]
		public void GivenIPrepareAFactoryForATaskWhichIncrementsANumberEveryMsUntilButThrowsAnExceptionAfter(int interval, int max, int exceptionAfter) {
			_values.Add(0);
			_tasks.Add(null);
			var index = _values.Count - 1;
			_factories.Add((ct) => {
				var task = IncrementForABit(index, interval, max, _cancellationTokenSource.Token, FailureOptions.Asynchronous, exceptionAfter);
				_tasks[index] = task;
				return task;
			});
		}

		private async Task IncrementForABit(int index, int interval, int max, CancellationToken cancellationToken, FailureOptions fail = FailureOptions.None, int failAfter = 0) {
			if (fail.HasFlag(FailureOptions.Synchronous)) {
				if (fail.HasFlag(FailureOptions.Delayed)) Thread.Sleep(interval);
				var tcs = new TaskCompletionSource<object>();
				tcs.SetException(new InvalidOperationException($"[{fail}] thrown for Increment [{index}], every [{interval}]ms til [{max}]. Value was [{_values[index]}]."));
				await tcs.Task;
			};
			while (_values[index] < max) {
				cancellationToken.ThrowIfCancellationRequested();
				await Task.Delay(interval);
				cancellationToken.ThrowIfCancellationRequested();
				if (fail.HasFlag(FailureOptions.Asynchronous) && failAfter == Math.Max(_values[index], 0)) {
					if (fail.HasFlag(FailureOptions.Delayed)) await Task.Delay(interval);
					throw new InvalidOperationException($"[{fail}] thrown for Increment [{index}], every [{interval}]ms til [{max}]. Value was [{_values[index]}].");
				};
				_values[index]++;
			}
		}

		public enum TaskNumber : int
		{
			First = 0,
			Second = 1
		}

		[Flags]
		private enum FailureOptions : int
		{
			None = 0,
			Synchronous = 1,
			Asynchronous = 1 << 1,
			Delayed = 1 << 2
		}
	}
}
