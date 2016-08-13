Feature: WhenAllCompleteOrOneFaults
	In order to run two or more parallel tasks efficiently
	As a developer
	I want to be notified when one fails and have the option of requesting the cancellation of its peers

@2tasks
Scenario: Two tasks run in parallel, the second is longer
	Given I prepare a Task which increments a number every 50ms until 2
	And I prepare a Task which increments a number every 50ms until 5
	When I await them using WhenAllCompleteOrOneFaults
	And the call returns
	Then both Tasks are RanToCompletion
	And the results are 2,5

@2tasks
Scenario: Two tasks run in parallel, the first is longer
	Given I prepare a Task which increments a number every 50ms until 4
	And I prepare a Task which increments a number every 50ms until 2
	When I await them using WhenAllCompleteOrOneFaults
	And the call returns
	Then both Tasks are RanToCompletion
	And the results are 4,2

@2tasks
Scenario: Two tasks run in parallel, the first fails immediately in synchronous code
	Given I prepare a Task which throws an Exception prior to executing any async code
	And I prepare a Task which increments a number every 50ms until 2
	When I await them using WhenAllCompleteOrOneFaults
	And the call returns
	Then the first Task is Faulted
	And the second Task is RanToCompletion
	And the results are 0,2
	And the Exception was logged
	And no Unobserved Exception was raised

@2tasks
Scenario: Two tasks run in parallel, the second fails immediately in synchronous code
	Given I prepare a Task which increments a number every 50ms until 1
	And I prepare a Task which throws an Exception prior to executing any async code
	When I await them using WhenAllCompleteOrOneFaults
	And the call returns
	Then the first Task is RanToCompletion
	And the second Task is Faulted
	And the results are 1,0
	And the Exception was logged
	And no Unobserved Exception was raised

@2tasks
Scenario: Two tasks run in parallel, the first fails midway through some async work, by which time the second has already completed
	Given I prepare a Task which increments a number every 50ms until 5 but throws an Exception after 3
	And I prepare a Task which increments a number every 50ms until 2
	When I await them using WhenAllCompleteOrOneFaults
	And the call returns
	Then the first Task is Faulted
	And the second Task is RanToCompletion
	And the results are 3,2
	And the Exception was logged
	And no Unobserved Exception was raised

@2tasks
Scenario: Two tasks run in parallel, the second fails midway through some async work, by which time the first has already completed
	Given I prepare a Task which increments a number every 50ms until 2
	And I prepare a Task which increments a number every 50ms until 5 but throws an Exception after 3
	When I await them using WhenAllCompleteOrOneFaults
	And the call returns
	Then the first Task is RanToCompletion
	And the second Task is Faulted
	And the results are 2,3
	And the Exception was logged
	And no Unobserved Exception was raised

@2tasks
Scenario: Two tasks run in parallel, the first fails midway through some async work, the second proceeds unaffected
	Given I prepare a Task which increments a number every 50ms until 2 but throws an Exception after 1
	And I prepare a Task which increments a number every 50ms until 5
	When I await them using WhenAllCompleteOrOneFaults
	And the call returns
	Then the first Task is Faulted
	And the second Task is RanToCompletion
	And the results are 1,5
	And the Exception was logged
	And no Unobserved Exception was raised

@2tasks
Scenario: Two tasks run in parallel, the second fails midway through some async work, the first proceeds unaffected
	Given I prepare a Task which increments a number every 50ms until 5
	And I prepare a Task which increments a number every 50ms until 2 but throws an Exception after 1
	When I await them using WhenAllCompleteOrOneFaults
	And the call returns
	Then the first Task is RanToCompletion
	And the second Task is Faulted
	And the results are 5,1
	And the Exception was logged
	And no Unobserved Exception was raised

@2tasks
Scenario: Two tasks run in parallel, the first fails immediately in synchronous code, the second is cancelled
	Given I prepare a Task which throws an Exception prior to executing any async code
	And I prepare a Task which increments a number every 50ms until 2
	When I await them using WhenAllCompleteOrOneFaults with the Cancel Others option
	And the call returns
	Then the first Task is Faulted
	And the second Task is Canceled
	And the results are 0,0
	And the Exception was logged
	And no Unobserved Exception was raised

@2tasks
Scenario: Two tasks run in parallel, the second fails immediately in synchronous code, the first is cancelled
	Given I prepare a Task which increments a number every 50ms until 1
	And I prepare a Task which throws an Exception prior to executing any async code
	When I await them using WhenAllCompleteOrOneFaults with the Cancel Others option
	And the call returns
	Then the first Task is Canceled
	And the second Task is Faulted
	And the results are 0,0
	And the Exception was logged
	And no Unobserved Exception was raised

@2tasks
Scenario: Two tasks run in parallel, the first fails midway through some async work, the second depends on it but has already completed so is not cancelled
	Given I prepare a Task which increments a number every 50ms until 5 but throws an Exception after 3
	And I prepare a Task which increments a number every 50ms until 2
	When I await them using WhenAllCompleteOrOneFaults with the Cancel Others option
	And the call returns
	Then the first Task is Faulted
	And the second Task is RanToCompletion
	And the results are 3,2
	And the Exception was logged
	And no Unobserved Exception was raised

@2tasks
Scenario: Two tasks run in parallel, the second fails midway through some async work, the first depends on it but has already completed so is not cancelled
	Given I prepare a Task which increments a number every 50ms until 2
	And I prepare a Task which increments a number every 50ms until 5 but throws an Exception after 3
	When I await them using WhenAllCompleteOrOneFaults with the Cancel Others option
	And the call returns
	Then the first Task is RanToCompletion
	And the second Task is Faulted
	And the results are 2,3
	And the Exception was logged
	And no Unobserved Exception was raised

@2tasks
Scenario: Two tasks run in parallel, the first fails midway through some async work, the second depends on it so is cancelled
	Given I prepare a Task which increments a number every 50ms until 2 but throws an Exception after 1
	And I prepare a Task which increments a number every 50ms until 5
	When I await them using WhenAllCompleteOrOneFaults with the Cancel Others option
	And the call returns
	Then the first Task is Faulted
	And the second Task is probably Canceled
	And the first Result is 1
	And the second Result is probably less than or equal to 2
	And the Exception was logged
	And no Unobserved Exception was raised

@2tasks
Scenario: Two tasks run in parallel, the second fails midway through some async work, the first depends on it so is cancelled
	Given I prepare a Task which increments a number every 50ms until 5
	And I prepare a Task which increments a number every 50ms until 2 but throws an Exception after 1
	When I await them using WhenAllCompleteOrOneFaults with the Cancel Others option
	And the call returns
	Then the first Task is probably Canceled
	And the second Task is Faulted
	And the first Result is probably less than or equal to 2
	And the second Result is 1
	And the Exception was logged
	And no Unobserved Exception was raised

#@2tasks @tasksthrowifcanceled
#Scenario: Two tasks run in parallel, the first fails midway through some async work, the second depends on it so is cancelled
#	Given I prepare a Task which increments a number every 50ms until 2 but throws an Exception after 1
#	And I prepare a Task which increments a number every 50ms until 5
#	When I await them using WhenAllCompleteOrOneFaults with the Cancel Others option
#	And the call returns
#	Then the first Task is Faulted
#	And the second Task is probably Canceled
#	And the first Result is 1
#	And the second Result is probably less than or equal to 2
#	And the Exception was logged
#	And no Unobserved Exception was raised
#
#@2tasks @tasksthrowifcanceled
#Scenario: Two tasks run in parallel, the second fails midway through some async work, the first depends on it so is cancelled
#	Given I prepare a Task which increments a number every 50ms until 5
#	And I prepare a Task which increments a number every 50ms until 2 but throws an Exception after 1
#	When I await them using WhenAllCompleteOrOneFaults with the Cancel Others option
#	And the call returns
#	Then the first Task is probably Canceled
#	And the second Task is Faulted
#	And the first Result is probably less than or equal to 2
#	And the second Result is 1
#	And the Exception was logged
#	And no Unobserved Exception was raised


#NB: all @usingfactories tasks automatically set the internal cancellation token, so in most cases
#			one task Faulting should leave the other(s) Canceled (as opposed to above where unless 
#			Cancel Others was specified, the non-Faulting task(s) would Complete.

@2tasks @usingfactories
Scenario: Two factory-generated tasks run in parallel, the second is longer
	Given I prepare a Factory for a Task which increments a number every 50ms until 2
	And I prepare a Factory for a Task which increments a number every 50ms until 5
	When I await the Factory Tasks using WhenAllCompleteOrOneFaults
	And the call returns
	Then both Tasks are RanToCompletion
	And the results are 2,5

@2tasks @usingfactories
Scenario: Two factory-generated tasks run in parallel, the first is longer
	Given I prepare a Factory for a Task which increments a number every 50ms until 4
	And I prepare a Factory for a Task which increments a number every 50ms until 2
	When I await the Factory Tasks using WhenAllCompleteOrOneFaults
	And the call returns
	Then both Tasks are RanToCompletion
	And the results are 4,2

@2tasks @usingfactories
Scenario: Two factory-generated tasks run in parallel, the first fails immediately in synchronous code
	Given I prepare a Factory for a Task which throws an Exception prior to executing any async code
	And I prepare a Factory for a Task which increments a number every 50ms until 2
	When I await the Factory Tasks using WhenAllCompleteOrOneFaults
	And the call returns
	Then the first Task is Faulted
	And the second Task is Canceled
	And the results are 0,0
	And the Exception was logged
	And no Unobserved Exception was raised

@2tasks @usingfactories
Scenario: Two factory-generated tasks run in parallel, the second fails immediately in synchronous code
	Given I prepare a Factory for a Task which increments a number every 50ms until 1
	And I prepare a Factory for a Task which throws an Exception prior to executing any async code
	When I await the Factory Tasks using WhenAllCompleteOrOneFaults
	And the call returns
	Then the first Task is Canceled
	And the second Task is Faulted
	And the first Result is probably less than or equal to 1
	And the second Result is 0
	And the Exception was logged
	And no Unobserved Exception was raised

@2tasks @usingfactories
Scenario: Two factory-generated tasks run in parallel, the first fails midway through some async work, by which time the second has already completed
	Given I prepare a Factory for a Task which increments a number every 50ms until 5 but throws an Exception after 3
	And I prepare a Factory for a Task which increments a number every 50ms until 2
	When I await the Factory Tasks using WhenAllCompleteOrOneFaults
	And the call returns
	Then the first Task is Faulted
	And the second Task is RanToCompletion
	And the results are 3,2
	And the Exception was logged
	And no Unobserved Exception was raised

@2tasks @usingfactories
Scenario: Two factory-generated tasks run in parallel, the second fails midway through some async work, by which time the first has already completed
	Given I prepare a Factory for a Task which increments a number every 50ms until 2
	And I prepare a Factory for a Task which increments a number every 50ms until 5 but throws an Exception after 3
	When I await the Factory Tasks using WhenAllCompleteOrOneFaults
	And the call returns
	Then the first Task is RanToCompletion
	And the second Task is Faulted
	And the results are 2,3
	And the Exception was logged
	And no Unobserved Exception was raised

@2tasks @usingfactories
Scenario: Two factory-generated tasks run in parallel, the first fails midway through some async work, the second proceeds unaffected
	Given I prepare a Factory for a Task which increments a number every 50ms until 2 but throws an Exception after 1
	And I prepare a Factory for a Task which increments a number every 50ms until 5
	When I await the Factory Tasks using WhenAllCompleteOrOneFaults
	And the call returns
	Then the first Task is Faulted
	And the second Task is Canceled
	And the first Result is 1
	And the second Result is probably less than or equal to 2
	And the Exception was logged
	And no Unobserved Exception was raised

@2tasks @usingfactories
Scenario: Two factory-generated tasks run in parallel, the second fails midway through some async work, the first proceeds unaffected
	Given I prepare a Factory for a Task which increments a number every 50ms until 5
	And I prepare a Factory for a Task which increments a number every 50ms until 2 but throws an Exception after 1
	When I await the Factory Tasks using WhenAllCompleteOrOneFaults
	And the call returns
	Then the first Task is Canceled
	And the second Task is Faulted
	And the first Result is probably less than or equal to 2
	And the second Result is 1
	And the Exception was logged
	And no Unobserved Exception was raised

@2tasks @usingfactories
Scenario: Two factory-generated tasks run in parallel, the first fails midway through some async work, the second depends on it but has already completed so is not cancelled
	Given I prepare a Factory for a Task which increments a number every 50ms until 5 but throws an Exception after 3
	And I prepare a Factory for a Task which increments a number every 50ms until 2
	When I await the Factory Tasks using WhenAllCompleteOrOneFaults
	And the call returns
	Then the first Task is Faulted
	And the second Task is RanToCompletion
	And the results are 3,2
	And the Exception was logged
	And no Unobserved Exception was raised

@2tasks @usingfactories
Scenario: Two factory-generated tasks run in parallel, the second fails midway through some async work, the first depends on it but has already completed so is not cancelled
	Given I prepare a Factory for a Task which increments a number every 50ms until 2
	And I prepare a Factory for a Task which increments a number every 50ms until 5 but throws an Exception after 3
	When I await the Factory Tasks using WhenAllCompleteOrOneFaults
	And the call returns
	Then the first Task is RanToCompletion
	And the second Task is Faulted
	And the results are 2,3
	And the Exception was logged
	And no Unobserved Exception was raised

@2tasks @usingfactories
Scenario: Two factory-generated tasks run in parallel, the first fails immediately in synchronous code, the second is cancelled
	Given I prepare a Factory for a Task which throws an Exception prior to executing any async code
	And I prepare a Factory for a Task which increments a number every 50ms until 2
	When I await the Factory Tasks using WhenAllCompleteOrOneFaults
	And the call returns
	Then the first Task is Faulted
	And the second Task is Canceled
	And the results are 0,0
	And the Exception was logged
	And no Unobserved Exception was raised

@2tasks @usingfactories
Scenario: Two factory-generated tasks run in parallel, the second fails immediately in synchronous code, the first is cancelled
	Given I prepare a Factory for a Task which increments a number every 50ms until 1
	And I prepare a Factory for a Task which throws an Exception prior to executing any async code
	When I await the Factory Tasks using WhenAllCompleteOrOneFaults
	And the call returns
	Then the first Task is Canceled
	And the second Task is Faulted
	And the results are 0,0
	And the Exception was logged
	And no Unobserved Exception was raised

@2tasks @usingfactories
Scenario: Two factory-generated tasks run in parallel, the first fails midway through some async work, the second depends on it so is cancelled
	Given I prepare a Factory for a Task which increments a number every 50ms until 2 but throws an Exception after 1
	And I prepare a Factory for a Task which increments a number every 50ms until 5
	When I await the Factory Tasks using WhenAllCompleteOrOneFaults
	And the call returns
	Then the first Task is Faulted
	And the second Task is probably Canceled
	And the first Result is 1
	And the second Result is probably less than or equal to 2
	And the Exception was logged
	And no Unobserved Exception was raised

@2tasks @usingfactories
Scenario: Two factory-generated tasks run in parallel, the second fails midway through some async work, the first depends on it so is cancelled
	Given I prepare a Factory for a Task which increments a number every 50ms until 5
	And I prepare a Factory for a Task which increments a number every 50ms until 2 but throws an Exception after 1
	When I await the Factory Tasks using WhenAllCompleteOrOneFaults
	And the call returns
	Then the first Task is probably Canceled
	And the second Task is Faulted
	And the first Result is probably less than or equal to 2
	And the second Result is 1
	And the Exception was logged
	And no Unobserved Exception was raised

#@2tasks @usingfactories @tasksthrowifcanceled
#Scenario: Two factory-generated tasks run in parallel, the first fails midway through some async work, the second depends on it so is cancelled
#	Given I prepare a Factory for a Task which increments a number every 50ms until 2 but throws an Exception after 1
#	And I prepare a Factory for a Task which increments a number every 50ms until 5
#	When I await the Factory Tasks using WhenAllCompleteOrOneFaults
#	And the call returns
#	Then the first Task is Faulted
#	And the second Task is probably Canceled
#	And the first Result is 1
#	And the second Result is probably less than or equal to 2
#	And the Exception was logged
#	And no Unobserved Exception was raised
#
#@2tasks @usingfactories @tasksthrowifcanceled
#Scenario: Two factory-generated tasks run in parallel, the second fails midway through some async work, the first depends on it so is cancelled
#	Given I prepare a Factory for a Task which increments a number every 50ms until 5
#	And I prepare a Factory for a Task which increments a number every 50ms until 2 but throws an Exception after 1
#	When I await the Factory Tasks using WhenAllCompleteOrOneFaults
#	And the call returns
#	Then the first Task is probably Canceled
#	And the second Task is Faulted
#	And the first Result is probably less than or equal to 2
#	And the second Result is 1
#	And the Exception was logged
#	And no Unobserved Exception was raised

