using System.Reflection;
using TestFrameworkCore;

namespace TestRunner
{
    /// <summary>
    /// Entry point for the custom test runner application.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // Determine the path to the compiled test assembly
            string testAssemblyPath = Path.GetFullPath(Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "..", "..", "..", "..",
                "TestsAssembly", "bin", "Debug", "net8.0",
                "TestsAssembly.dll"));

            if (!File.Exists(testAssemblyPath))
            {
                Console.WriteLine("Error: Test assembly not found at: " + testAssemblyPath);
                return;
            }

            // Load the test assembly dynamically
            Assembly testAssembly = Assembly.LoadFrom(testAssemblyPath);

            // 1. Discover all test methods
            List<MethodInfo> testMethods = DiscoverTestMethods(testAssembly);
            Console.WriteLine($"Discovered {testMethods.Count} test(s).\n");

            // 2. Execute all tests with setup and teardown
            List<TestResult> testResults = ExecuteTests(testMethods);

            // 3. Print aggregate summary
            int passedCount = testResults.Count(r => r.Passed);
            int failedCount = testResults.Count - passedCount;
            Console.WriteLine("\n=== Test Summary ===");
            Console.WriteLine($"Passed: {passedCount}");
            Console.WriteLine($"Failed: {failedCount}");
            Console.WriteLine($"Total : {testResults.Count}");

            // 4. Write results to file
            string resultsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "results.txt");
            using (var writer = new StreamWriter(resultsFilePath))
            {
                foreach (var result in testResults)
                {
                    string line = result.Passed
                        ? $"[PASS] {result.Name}"
                        : $"[FAIL] {result.Name} -> {result.ErrorMessage}";
                    writer.WriteLine(line);
                }
                writer.WriteLine();
                writer.WriteLine("=== Test Summary ===");
                writer.WriteLine($"Passed: {passedCount}");
                writer.WriteLine($"Failed: {failedCount}");
                writer.WriteLine($"Total : {testResults.Count}");
            }
            Console.WriteLine($"\nResults written to: {resultsFilePath}");

            // Exit with error code if any tests failed
            Environment.Exit(failedCount == 0 ? 0 : 1);
        }

        /// <summary>
        /// Scans the given assembly for methods marked with [MyTest] attribute.
        /// </summary>
        static List<MethodInfo> DiscoverTestMethods(Assembly assembly)
        {
            var discoveredMethods = new List<MethodInfo>();

            foreach (Type type in assembly.GetTypes())
            {
                foreach (MethodInfo method in type.GetMethods(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                {
                    bool hasTestAttribute = method.GetCustomAttributes(typeof(MyTestAttribute), false).Any();

                    if (hasTestAttribute &&
                        method.ReturnType == typeof(void) &&
                        method.GetParameters().Length == 0)
                    {
                        discoveredMethods.Add(method);
                    }
                }
            }

            return discoveredMethods;
        }

        /// <summary>
        /// Executes each test method, invoking setup and teardown if defined, and reports immediate pass/fail.
        /// </summary>
        static List<TestResult> ExecuteTests(List<MethodInfo> testMethods)
        {
            var results = new List<TestResult>();

            foreach (MethodInfo testMethod in testMethods)
            {
                string testFullName = $"{testMethod.DeclaringType.FullName}.{testMethod.Name}";

                // Locate optional setup and teardown methods in the same type
                MethodInfo setupMethod = testMethod.DeclaringType
                    .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .FirstOrDefault(m => m.GetCustomAttributes(typeof(MySetupAttribute), false).Any());

                MethodInfo teardownMethod = testMethod.DeclaringType
                    .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .FirstOrDefault(m => m.GetCustomAttributes(typeof(MyTeardownAttribute), false).Any());

                object testInstance = null;

                try
                {
                    if (!testMethod.IsStatic)
                        testInstance = Activator.CreateInstance(testMethod.DeclaringType);

                    // Invoke setup if available
                    setupMethod?.Invoke(testInstance, null);

                    // Execute the test method
                    testMethod.Invoke(testInstance, null);

                    // Report pass immediately
                    Console.WriteLine($"[PASS] {testFullName}");
                    results.Add(new TestResult(testFullName, passed: true));
                }
                catch (TargetInvocationException tie) when (tie.InnerException != null)
                {
                    string error = tie.InnerException.Message;
                    Console.WriteLine($"[FAIL] {testFullName} -> {error}");
                    results.Add(new TestResult(testFullName, passed: false, errorMessage: error));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[FAIL] {testFullName} -> {ex.Message}");
                    results.Add(new TestResult(testFullName, passed: false, errorMessage: ex.Message));
                }
                finally
                {
                    // Always attempt teardown, ignore exceptions
                    try { teardownMethod?.Invoke(testInstance, null); } catch { }
                }
            }

            return results;
        }
    }
}
