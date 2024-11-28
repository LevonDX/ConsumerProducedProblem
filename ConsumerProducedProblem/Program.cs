namespace ConsumerProducedProblem
{
    internal class Program
    {
        static private readonly object _lockObj = new object();
        static Queue<int> queue = new Queue<int>();

        const int MAX_SIZE = 20;
        const int randomWaitingTime = 5000;

        /// <summary>
        /// If the queue is full (count = MAX_SIZE), producer should wait until consumer consumes some items (<= 0.9 * MAX_SIZE).
        /// </summary>
        public static void Producer(int producerId)
        {
            Random random = new Random();
            while (true)
            {
                int waitTime = random.Next(500, randomWaitingTime);
                int number = random.Next();

                Thread.Sleep(waitTime);

                lock (_lockObj)
                {
                    while (queue.Count >= MAX_SIZE)
                    {
                        Console.WriteLine($"Producer {producerId} is waiting. Queue is full...");
                        Monitor.Wait(_lockObj);
                    }

                    queue.Enqueue(number);
                    Console.WriteLine($"Producer {producerId} produced: {number}, Queue size: {queue.Count}");

                    if (queue.Count >= 0.1 * MAX_SIZE) // Notify consumers when the queue is no longer empty
                    {
                        Monitor.PulseAll(_lockObj);
                    }
                }
            }
        }

        /// <summary>
        /// If the queue is empty (count = 0), consumer should wait until producer produces some items.
        /// </summary>
        public static void Consumer(int consumerId)
        {
            Random random = new Random();

            while (true)
            {
                int waitTime = random.Next(500, randomWaitingTime);
                Thread.Sleep(waitTime);

                lock (_lockObj)
                {
                    while (queue.Count == 0)
                    {
                        Console.WriteLine($"Consumer {consumerId} is waiting. Queue is empty...");
                        Monitor.Wait(_lockObj);
                    }

                    int number = queue.Dequeue();
                    Console.WriteLine($"Consumer {consumerId} consumed: {number}, Queue size: {queue.Count}");

                    if (queue.Count == 0.9 * MAX_SIZE) // Notify producers when the queue is no longer full
                    {
                        Monitor.PulseAll(_lockObj);
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            const int producerCount = 3; 
            const int consumerCount = 2; 

            // Start producer threads
            for (int i = 1; i <= producerCount; i++)
            {
                int producerId = i;
                Thread producerThread = new Thread(() => Producer(producerId))
                {
                    Name = $"Producer-{producerId}"
                };
                producerThread.Start();
            }

            // Start consumer threads
            for (int i = 1; i <= consumerCount; i++)
            {
                int consumerId = i;
                Thread consumerThread = new Thread(() => Consumer(consumerId))
                {
                    Name = $"Consumer-{consumerId}"
                };
                consumerThread.Start();
            }

            // Monitor the queue size
            while (true)
            {
                lock (_lockObj)
                {
                    Console.WriteLine($"Current Queue Size: {queue.Count}");
                }
                Thread.Sleep(1000); 
            }
        }
    }
}
