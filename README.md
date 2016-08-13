# tasks
Extensions and useful enhancements for. NET's System.Threading.Tasks. 

##WhenAllCompleteOrOneFaults

`WhenAllCompleteOrOneFaults` provides a `CancellationToken`-ready equivalent of the native `Task.WhenAll`, and the ability to cancel the worker tasks from the external token, or when one fails and the others should not continue.
Cancellation is, as always, opt-in by the worker tasks. They can choose to acknowledge or ignore the token's `IsCancellationRequested` property.

### Features

- OperationCanceledException stomping
- Task Factories
- External and Internal Cancellation

### Usage

TBC.


