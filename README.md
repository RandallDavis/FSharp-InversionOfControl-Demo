# FSharp-InversionOfControl-Demo
Demonstration with commentary to illustrate IoC for testing in F#.

### TODO: intro

### Module A_Composed

This code is well modularized. *postData()* is doing operational work, *getPairs()* has business logic, and *matchInterns()* orchestrates everything.

<img src="images/IoC_DependencyTree_A.png" alt="Drawing" width="400" />

From a testing perspective, we can only test *getPairs()* independently from the other functions because it has no dependencies on other code. One of the major difficulties we have here is that in order to test *matchInterns()*, our tests have reflect business logic from *getPairs()* *TODO: show that testing for failure conditions requires seniors.length to be less than interns.length*
* This business logic has nothing to do with *matchInterns()*'s purpose.
* Changes to *getPairs()* will likely cause changes to *matchInterns()*'s tests.

What all of this means is that while our code is cleanly modularlized for a regular production flow, it's not modularized from a testing perspective. It's going to be a large burden to write and maintain tests for this code, and we're only going to have so much faith in the tests' accuracy.

We also have another problem here, which is that even when we run tests, we're going to be making actual web posts. If we were to write tests for this code in its current state, we could have a hosted endpoint that receives posts in testing scenarios, and we can pass that endpoint into *matchInterns()*, but this is overly complex and slow, if not unreliable. That might be a good solution for end-to-end testing, but not for unit tests. Solving this problem is annoying, but conceptually easy, so let's knock it out first.


### Module B_ComposedIsolated

*WebClient* is a sealed class that doesn't have a strong interface. If it wasn't sealed, we could potentially inherit from it to build a fake, and if it had a clean interface, we could make a mock out of that.

Because *WebClient* is locked down, we had to build a wrapper type with an interface. In our production flow, the wrapper type will call a *WebClient* on our behalf, but now in our tests, we can have a fake implementation that emulates the behavior of a web post. For convenience, the byte array conversion logic was moved into the Wrapper, which makes *postData()*'s work in formatting the data and triggering a post a little more clear.

<img src="images/IoC_DependencyTree_B.png" alt="Drawing" width="400" />

As a result of our change, *postData()* is now fully testable because we can supply it our own versions of its dependency. We can now effectively treat it as a leaf on this dependency tree. This is what "inversion of control" is - basically some outside force is choosing what this function should act upon, while guaranteeing that the function knows how to interact with whatever it's given.

Note how we have to pass *webClient* into *matchInterns()*, even though it doesn't do anything but pass it along - let's fix that with partial application.
