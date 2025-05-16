using TestFrameworkCore;

namespace TestsAssembly
{
    public class SampleTests
    {
        private List<int> _list;

        [MySetup]
        public void InitList()
        {
            _list = new List<int>();
            Console.WriteLine(">> Setup: List initialized");
        }

        [MyTest]
        public void Test_Addition_Works()
        {
            if (2 + 2 != 4)
                throw new Exception("2 + 2 should equal 4");
        }

        [MyTest]
        private void Test_String_NotEmpty()
        {
            string s = "hello";
            if (string.IsNullOrEmpty(s))
                throw new Exception("String should not be empty");
        }

        [MyTest]
        public void Test_Failure_Demo()
        {
            throw new Exception("Oops");
        }

        [MyTeardown]
        public void Cleanup()
        {
            _list.Clear();
            Console.WriteLine(">> Teardown: List cleared");
        }
        
        // Not checked - will not run
        public void HelperMethod() { }
    }
}
