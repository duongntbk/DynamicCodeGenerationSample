using System;

namespace DynamicCodeGenerationSample
{
    public class TestClass
    {
        private string _firstName = "Default first name";
        private string _lastName = "Default last name";

        public void SetName(string firstName, string lastName)
        {
            _firstName = firstName;
            _lastName = lastName;
        }

        public string GetLastName() => _lastName;

        public string Introduce(string first, string last) => $"The name is {last}, {first} {last}.";

        public void PrintDetails()
        {
            Console.WriteLine($"First name: {_firstName}");
            Console.WriteLine($"Last name: {_lastName}");
        }
    }
}
