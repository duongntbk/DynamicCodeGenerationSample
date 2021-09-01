This repository hosts the sample code for my blog post at the following link.

[https://duongnt.com/dynamic-code-generation](https://duongnt.com/dynamic-code-generation)

# DynamicCodeGenerationSample project

Run the four sample methods by executing the following command.
```
dotnet run
```

You should see the following result.
```
Instance is of type: DynamicCodeGenerationSample.TestClass
Last name is: Default last name
First name: new first name
Last name: new last name
The name is Bond, James Bond.
```

# Benchmark project

Run the following command to start the benchmark.
```
dotnet run --configuration Release
```

You should then see the following list of tests
```
Available Benchmarks:
  #0 TestCreatorRunner
  #1 TestMethodRunner
```

Then select the test you want to run by typing its name or number.
```
1
```

Or

```
TestMethodRunner
```


# License

MIT License

https://opensource.org/licenses/MIT
