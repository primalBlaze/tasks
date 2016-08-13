using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimalBlaze.Threading
{
	/// <summary>
	/// Specifies the behavior when generating and processing the results of a cancellation which is caused
	/// by the fault of a worker task supplied to <see cref="Tasks.WhenAllCompleteOrOneFaults(TaskCancellationOptions, System.Threading.CancellationToken, Func{System.Threading.CancellationToken, Task}[])"/>.
	/// </summary>
	[Flags]
	public enum TaskCancellationOptions : int
	{
		/// <summary>
		/// When no cancellation options are specified, specifies that default behavior should be used when 
		/// generating a cancellation. The cancellation from a worker task will be assumed to be resulting 
		/// from an attempt to cancel that Task following an antecedent fault in another worker task, and the
		/// ensuing <see cref="OperationCanceledException"/> will be observed but not rethrown.
		/// </summary>
		None = 0,
		/// <summary>
		/// Specifies that the <see cref="Tasks.WhenAllCompleteOrOneFaults(TaskCancellationOptions, System.Threading.CancellationToken, Func{System.Threading.CancellationToken, Task}[])"/>
		/// method should not suppress <see cref="OperationCanceledException"/>s which it perceives to have 
		/// been caused by the cancellation of a worker task in response to the fault of another worker task.
		/// </summary>
		DisableExceptionChanneling = 1
	}
}
