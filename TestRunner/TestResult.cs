namespace TestRunner
{
    public class TestResult
    {
        public string Name { get; }
        public bool Passed { get; }
        public string ErrorMessage { get; }
        public TestResult(string name, bool passed, string errorMessage = null)
        {
            Name = name;
            Passed = passed;
            ErrorMessage = errorMessage;
        }
    }
}
