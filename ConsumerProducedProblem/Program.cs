namespace ConsumerProducedProblem
{
    internal class Program
    {
        static private readonly object _lockObj = new object();
        static Queue<int> queue = new Queue<int>();

        const int MAX_SIZE = 20;
        const int randomWaitingTIme = 5000;
        /// <summary>
        /// If the queue is full (count = MAX_SIZE), producer should wait until consumer consumes some items 0.9 * MAX_SIZE.
        /// </summary>
        public static void Producer()
        {
            Random random = new Random();
            while (true)
            {
                int waitTime = random.Next(500, randomWaitingTIme);
                int number = random.Next();

                Thread.Sleep(waitTime);

                lock (_lockObj)
                {
                    while (queue.Count >= MAX_SIZE)
                    {
                        Console.WriteLine("Queue is full. Producer is waiting...");
                        Monitor.Wait(_lockObj);
                    }

                    queue.Enqueue(number);

                    if(queue.Count <= 0.9 * MAX_SIZE)
                    {
                        Monitor.PulseAll(_lockObj);
                    }
                }
            }
        }

        /// <summary>
        /// If the queue is empty (count = 0), consumer should wait until producer produces some items.
        /// </summary>
        public static void Consumer()
        {
            Random random = new Random();

            while (true)
            {
                int waitTime = random.Next(500, randomWaitingTIme);
                Thread.Sleep(waitTime);

                lock (_lockObj)
                {
                    while (queue.Count == 0)
                    {
                        Console.WriteLine("Queue is empty. Consumer is waiting...");
                        Monitor.Wait(_lockObj);
                    }

                    int number = queue.Dequeue();

                    if (queue.Count >= 0.1 * MAX_SIZE )
                    {
                        Monitor.PulseAll(_lockObj);
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            Thread producerThread = new Thread(Producer);
            producerThread.Name="producer";

            Thread consumerThread = new Thread(Consumer);
            consumerThread.Name = "consumer";

            producerThread.Start();
            Thread.Sleep(3000);
            consumerThread.Start();

            while (true)
            {
                lock (_lockObj)
                {
                    Console.WriteLine(queue.Count);
                }
                Thread.Sleep(500);
            }
        }
    }
}
