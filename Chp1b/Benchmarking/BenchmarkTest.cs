using BenchmarkDotNet.Attributes; // [Benchmark]

public class NumbersBenchmarks
{
    int[] numbers;

    public NumbersBenchmarks()
    {
        numbers = Enumerable.Range(
            start: 1, count: 50000).ToArray();
    }

    [Benchmark(Baseline = true)]
    public int LinqTest()
    {
        return numbers.Where(n => n % 3 == 0).Count();
    }

    [Benchmark]
    public int LoopTest()
    {
        int count = 0;
        
        foreach(var i in numbers)
        {
            if (i % 3 == 0) count++;
        }
        return count;
    }
    
}