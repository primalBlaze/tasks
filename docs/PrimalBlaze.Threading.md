# PrimalBlaze.Threading

<table>
<tbody>
<tr>
<td><a href="#taskcancellationoptions">TaskCancellationOptions</a></td>
<td><a href="#tasks">Tasks</a></td>
</tr>
</tbody>
</table>

The <a href="#primalblaze.threading">PrimalBlaze.Threading</a> namespace contains classes which provide extensions and useful enhancements for .NET's <a href="#system.threading.tasks">System.Threading.Tasks</a>.


## TaskCancellationOptions

Specifies the behavior when generating and processing the results of a cancellation which is caused by the fault of a worker task supplied to <a href="#tasks.whenallcompleteoronefaults(primalblaze.threading.taskcancellationoptions,system.threading.cancellationtoken,system.func{system.threading.cancellationtoken,system.threading.tasks.task}[])">Tasks.WhenAllCompleteOrOneFaults(PrimalBlaze.Threading.TaskCancellationOptions,System.Threading.CancellationToken,System.Func{System.Threading.CancellationToken,System.Threading.Tasks.Task}[])</a>.

### DisableExceptionChanneling

Specifies that the <a href="#tasks.whenallcompleteoronefaults(primalblaze.threading.taskcancellationoptions,system.threading.cancellationtoken,system.func{system.threading.cancellationtoken,system.threading.tasks.task}[])">Tasks.WhenAllCompleteOrOneFaults(PrimalBlaze.Threading.TaskCancellationOptions,System.Threading.CancellationToken,System.Func{System.Threading.CancellationToken,System.Threading.Tasks.Task}[])</a> method should not suppress <a href="#system.operationcanceledexception">System.OperationCanceledException</a>s which it perceives to have been caused by the cancellation of a worker task in response to the fault of another worker task.

### None

When no cancellation options are specified, specifies that default behavior should be used when generating a cancellation. The cancellation from a worker task will be assumed to be resulting from an attempt to cancel that Task following an antecedent fault in another worker task, and the ensuing <a href="#system.operationcanceledexception">System.OperationCanceledException</a> will be observed but not rethrown.


## Tasks

Contains static methods in a similar vein to the static members of <a href="#system.threading.tasks.task">System.Threading.Tasks.Task</a>.

### GenerateInternalCancellationSource(hostCancellationToken)

Generates an internal <a href="#system.threading.cancellationtokensource">System.Threading.CancellationTokenSource</a> which will be cancelled if an external <a href="#system.threading.cancellationtoken">System.Threading.CancellationToken</a> is cancelled.

| Name | Description |
| ---- | ----------- |
| hostCancellationToken | *System.Threading.CancellationToken*<br>The external cancellation token which should trigger cancellation of this source. |

#### Returns

The internal <a href="#system.threading.cancellationtokensource">System.Threading.CancellationTokenSource</a>.

### WhenAllCompleteOrOneFaults(options, hostCancellationToken, taskFactories)

Creates a task that will complete when all of the <a href="#system.threading.tasks.task">System.Threading.Tasks.Task</a> objects created by an array of factories have completed, or when an <a href="#system.exception">System.Exception</a> is detected as thrown from one of those Task objects. It uses the specified options, and can be cancelled by the specified hostCancellationToken.

| Name | Description |
| ---- | ----------- |
| options | *PrimalBlaze.Threading.TaskCancellationOptions*<br>Flags specifying various <a href="#taskcancellationoptions">TaskCancellationOptions</a>. |
| hostCancellationToken | *System.Threading.CancellationToken*<br>A <a href="#system.threading.cancellationtoken">System.Threading.CancellationToken</a> which can be used to cancel the Wait. |
| taskFactories | *System.Func{System.Threading.CancellationToken,System.Threading.Tasks.Task}[]*<br>An array of <a href="#system.func\`2">System.Func\`2</a> which generate the worker <a href="#system.threading.tasks.task">System.Threading.Tasks.Task</a>s which will be waited upon. Each factory method is passed an internal <a href="#system.threading.cancellationtoken">System.Threading.CancellationToken</a> which the Task can choose to monitor to determine whether one of its peer Tasks has faulted. |

#### Returns

A task that represents the completion of all of the supplied tasks, or the faulted state of at least one of the tasks.

#### Remarks

The first <a href="#system.exception">System.Exception</a>, if any, detected as thrown within any of the tasks will be rethrown. Each task produced by the taskFactories can elect to monitor the internal <a href="#system.threading.cancellationtoken">System.Threading.CancellationToken</a> which it is supplied. This token's <a href="#system.threading.cancellationtoken.iscancellationrequested">System.Threading.CancellationToken.IsCancellationRequested</a> property will be set to  ( in VB .NET) if an exception is detected as having been thrown by any of the Tasks. In this way, work need not continue in one task if another has failed. The exceptions thrown by worker tasks may become unobserved due to a race condition where the cancellation of a peer <a href="#system.threading.tasks.task">System.Threading.Tasks.Task</a> may occur before the original exception is handled, triggering an <a href="#system.operationcanceledexception">System.OperationCanceledException</a> first. The default behavior is to capture the original exception using <a href="#system.runtime.exceptionservices.exceptiondispatchinfo">System.Runtime.ExceptionServices.ExceptionDispatchInfo</a>, then re-throw it in preference to the OperationCanceledException. To prevent this behaviour, specify the <a href="#taskcancellationoptions.disableexceptionchanneling">TaskCancellationOptions.DisableExceptionChanneling</a> option.

### WhenAllCompleteOrOneFaults(hostCancellationToken, taskFactories)

Creates a task that will complete when all of the <a href="#system.threading.tasks.task">System.Threading.Tasks.Task</a> objects created by an array of factories have completed, or when an <a href="#system.exception">System.Exception</a> is detected as thrown from one of those Task objects. It can be cancelled by the specified hostCancellationToken.

| Name | Description |
| ---- | ----------- |
| hostCancellationToken | *System.Threading.CancellationToken*<br>A <a href="#system.threading.cancellationtoken">System.Threading.CancellationToken</a> which can be used to cancel the Wait. |
| taskFactories | *System.Func{System.Threading.CancellationToken,System.Threading.Tasks.Task}[]*<br>An array of <a href="#system.func\`2">System.Func\`2</a> which generate the worker <a href="#system.threading.tasks.task">System.Threading.Tasks.Task</a>s which will be waited upon. Each factory method is passed an internal <a href="#system.threading.cancellationtoken">System.Threading.CancellationToken</a> which the Task can choose to monitor to determine whether one of its peer Tasks has faulted. |

#### Returns

A task that represents the completion of all of the supplied tasks, or the faulted state of at least one of the tasks.

#### Remarks

The first <a href="#system.exception">System.Exception</a>, if any, detected as thrown within any of the tasks will be rethrown. Each task produced by the taskFactories can elect to monitor the internal <a href="#system.threading.cancellationtoken">System.Threading.CancellationToken</a> which it is supplied. This token's <a href="#system.threading.cancellationtoken.iscancellationrequested">System.Threading.CancellationToken.IsCancellationRequested</a> property will be set to  ( in VB .NET) if an exception is detected as having been thrown by any of the Tasks. In this way, work need not continue in one task if another has failed. The exceptions thrown by worker tasks may become unobserved due to a race condition where the cancellation of a peer <a href="#system.threading.tasks.task">System.Threading.Tasks.Task</a> may occur before the original exception is handled, triggering an <a href="#system.operationcanceledexception">System.OperationCanceledException</a> first. The default behavior is to capture the original exception using <a href="#system.runtime.exceptionservices.exceptiondispatchinfo">System.Runtime.ExceptionServices.ExceptionDispatchInfo</a>, then re-throw it in preference to the OperationCanceledException. To prevent this behaviour, specify the <a href="#taskcancellationoptions.disableexceptionchanneling">TaskCancellationOptions.DisableExceptionChanneling</a> option using the <a href="#tasks.whenallcompleteoronefaults(primalblaze.threading.taskcancellationoptions,system.threading.cancellationtoken,system.func{system.threading.cancellationtoken,system.threading.tasks.task}[])">Tasks.WhenAllCompleteOrOneFaults(PrimalBlaze.Threading.TaskCancellationOptions,System.Threading.CancellationToken,System.Func{System.Threading.CancellationToken,System.Threading.Tasks.Task}[])</a> overload.

### WhenAllCompleteOrOneFaults(workerCancellationSource, workerTasks)

Creates a task that will complete when all of the <a href="#system.threading.tasks.task">System.Threading.Tasks.Task</a> objects in an array have completed, or when an <a href="#system.exception">System.Exception</a> is detected as thrown from one of those Task objects. It will notify the specified workerCancellationSource if a fault occurs.

| Name | Description |
| ---- | ----------- |
| workerCancellationSource | *System.Threading.CancellationTokenSource*<br>A <a href="#system.threading.cancellationtokensource">System.Threading.CancellationTokenSource</a> which will be requested to Cancel if one of the workerTasks faults. It is expected, but not required, that the worker tasks monitor this source's <a href="#system.threading.cancellationtokensource.token">System.Threading.CancellationTokenSource.Token</a>. |
| workerTasks | *System.Threading.Tasks.Task[]*<br>The <a href="#system.threading.tasks.task">System.Threading.Tasks.Task</a>s to wait on for completion. |

#### Returns

A task that represents the completion of all of the supplied tasks, or the faulted state of at least one of the tasks.

#### Remarks

The first <a href="#system.exception">System.Exception</a>, if any, detected as thrown within any of the tasks will be rethrown. If a workerCancellationSource is provided then this will be used to notify when one of the tasks faults. This can be used by the calling method to cancel the other tasks. For automated cancellation of the other tasks, consider using one of the overloads which accepts a taskFactories parameter, e.g. <a href="#tasks.whenallcompleteoronefaults(system.threading.cancellationtoken,system.func{system.threading.cancellationtoken,system.threading.tasks.task}[])">Tasks.WhenAllCompleteOrOneFaults(System.Threading.CancellationToken,System.Func{System.Threading.CancellationToken,System.Threading.Tasks.Task}[])</a>.

### WhenAllCompleteOrOneFaults(workerTasks)

Creates a task that will complete when all of the <a href="#system.threading.tasks.task">System.Threading.Tasks.Task</a> objects in an array have completed, or when an <a href="#system.exception">System.Exception</a> is detected as thrown from one of those Task objects.

| Name | Description |
| ---- | ----------- |
| workerTasks | *System.Threading.Tasks.Task[]*<br>The <a href="#system.threading.tasks.task">System.Threading.Tasks.Task</a>s to wait on for completion. |

#### Returns

A task that represents the completion of all of the supplied tasks, or the faulted state of at least one of the tasks.

#### Remarks

The first <a href="#system.exception">System.Exception</a>, if any, detected as thrown within any of the tasks will be rethrown. This overload makes no attempt to notify or cancel other worker tasks if one faults. Use the <a href="#tasks.whenallcompleteoronefaults(system.threading.cancellationtokensource,system.threading.tasks.task[])">Tasks.WhenAllCompleteOrOneFaults(System.Threading.CancellationTokenSource,System.Threading.Tasks.Task[])</a> overload to notify a specified <a href="#system.threading.cancellationtokensource">System.Threading.CancellationTokenSource</a>, or one of the overloads which accepts the taskFactories parameter for automatic cancellation of other tasks.

### WhenAllCompleteOrOneFaultsInternal(workerTasks, internalCts)

Creates a task that will complete when all of the <a href="#system.threading.tasks.task">System.Threading.Tasks.Task</a> objects in an array have completed, or when an <a href="#system.exception">System.Exception</a> is detected as thrown from one of those Task objects. It will notify the specified internalCts if a fault occurs.

| Name | Description |
| ---- | ----------- |
| workerTasks | *System.Collections.Generic.IEnumerable{System.Threading.Tasks.Task}*<br>The <a href="#system.threading.tasks.task">System.Threading.Tasks.Task</a>s to wait on for completion. |
| internalCts | *System.Threading.CancellationTokenSource*<br>A <a href="#system.threading.cancellationtokensource">System.Threading.CancellationTokenSource</a> which will be requested to Cancel if one of the workerTasks faults. |

#### Returns

The first <a href="#system.exception">System.Exception</a>, if any, detected as thrown within any of the tasks will be rethrown.
