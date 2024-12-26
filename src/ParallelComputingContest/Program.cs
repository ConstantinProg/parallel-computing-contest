using System.Diagnostics;

int[] sizes = { 100000, 1000000, 10000000, 100000000 };

foreach (var size in sizes)
{
    Console.WriteLine($"Array size: {size}");

    int[] array = GenerateArray(size);

    MeasureTime("Sequential", () =>
    {
        int sum = 0;
        foreach (var num in array)
            sum += num;
    });

    MeasureTime("Parallel (Threads)", () =>
    {
        int sum = 0;
        List<Thread> threads = new List<Thread>();
        int threadCount = System.Environment.ProcessorCount;
        int partSize = array.Length / threadCount;
        for (int i = 0; i < threadCount; i++)
        {
            int startIndex = i * partSize;
            int endIndex = (i == threadCount - 1) ? array.Length : (i + 1) * partSize;
            var thread = new Thread(() =>
            {
                int localSum = 0;
                for (int j = startIndex; j < endIndex; j++)
                {
                    localSum += array[j];
                }
                Interlocked.Add(ref sum, localSum);
            });
            threads.Add(thread);
            thread.Start();
        }
        foreach (var thread in threads)
        {
            thread.Join();
        }
    });

    MeasureTime("Parallel (LINQ)", () =>
    {
        var sum = array.AsParallel().Sum();
    });

    static int[] GenerateArray(int size)
    {
        Random rand = new Random();
        int[] array = new int[size];
        for (int i = 0; i < size; i++)
        {
            array[i] = rand.Next(1, 10);
        }
        return array;
    }

    static void MeasureTime(string methodName, Action action)
    {
        var stopwatch = Stopwatch.StartNew();
        action();
        stopwatch.Stop();
        Console.WriteLine($"{methodName} - Time: {stopwatch.ElapsedMilliseconds} ms");
    }
}
