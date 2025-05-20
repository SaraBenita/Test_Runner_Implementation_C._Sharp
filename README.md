# Custom C# Test Runner – Sara Benita

## Overview
This solution implements a **custom test runner in C# from scratch**, without relying on existing test frameworks. It **discovers, executes, and reports** unit tests marked with custom attributes.

---

## Project Structure
- **TestFrameworkCore**  
  Contains custom attributes `[MyTest]`, `[MySetup]`, and `[MyTeardown]` used to mark test methods and setup/teardown methods.

- **TestsAssembly**  
  A class library containing sample test classes using the custom attributes.

- **TestRunner**  
  A console application that:
  - Loads `TestsAssembly.dll`
  - Discovers test methods via reflection
  - Executes them with setup/teardown logic
  - Prints results to the console
  - Writes a summary to `results.txt`

---

## How to Run

1. Open the solution in Visual Studio and **build all projects** (`Ctrl+Shift+B`).
2. Set **TestRunner** as the **Startup Project**.
3. Run without debugging (`Ctrl+F5`).
4. The console will display:
   - Discovered tests
   - Setup/teardown messages
   - Pass/fail statuses
   - A final summary  
5. After completion, a file `results.txt` will be created in the `TestRunner` output folder (e.g., `TestRunner\bin\Debug\netX.X`).  
   The console will print the exact file path.

---

## Design Decisions

- **Custom Attributes**  
  Isolated in `TestFrameworkCore` to avoid circular dependencies and allow both the runner and test projects to reference them.

- **Reflection-Based Discovery**  
  Uses `Assembly.LoadFrom` and `BindingFlags` to find methods marked with `[MyTest]`. Ensures methods have a `void` return type and no parameters.

- **Setup/Teardown Support**  
  Before each test, the runner invokes a single method marked `[MySetup]`; after each test, a method marked `[MyTeardown]`.  
  For simplicity, only the **first found** setup/teardown method is executed.

- **Immediate Reporting**  
  Each test’s pass/fail status is printed **immediately after execution**, interleaved with setup/teardown logs for clarity.

- **Results File**  
  `results.txt` logs all test pass/fail messages and the final summary. This enables:
  - CI/CD pipeline integration
  - Further analysis or logging

- **Exit Code**  
  The process exits with:
  - Code `0` if **all tests pass**
  - Code `1` if **any test fails**  
  This supports automated build failure on test errors.

---

## Extensibility Ideas

- 🧵 **Parallel Execution**  
  Enhance the runner to execute tests in parallel.

- 🧪 **Additional Attributes**  
  Add support for:
  - Test categories
  - Expected exceptions
  - Data-driven tests

---
