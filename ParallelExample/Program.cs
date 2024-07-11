using BenchmarkDotNet.Running;
using ParalellExample;

// BenchmarkRunner.Run<ApiParallelBencmarks>();
BenchmarkRunner.Run<ParallelPresentation>();

// Firstly run the api project

// Then --> dotnet build -c Release
// Above code will provide a path and copy it
// Then --> dotnet {path}