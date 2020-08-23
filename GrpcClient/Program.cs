namespace GrpcClient
{
    using Google.Protobuf;
    using Google.Protobuf.WellKnownTypes;
    using Grpc.Core;
    using Grpc.Net.Client;
    using GrpcServer.Web.protos;
    using GrpcService.Web.protos;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the <see cref="Program" />.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// The Main.
        /// </summary>
        /// <param name="args">The args<see cref="string[]"/>.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        internal static async Task Main(string[] args)
        {

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();
            Log.Information("Client starting...");

            using var channel = GrpcChannel.ForAddress("https://localhost:5001",
                new GrpcChannelOptions
                {
                    LoggerFactory = new SerilogLoggerFactory()
                });
            var client = new EmployeeService.EmployeeServiceClient(channel);

            var option = int.Parse(args[0]);
            switch (option)
            {
                case 1:
                    await GetByNoAsync(client);
                    break;
                case 2:
                    await GetAllAsync(client);
                    break;
                case 3:
                    await AddPhotoAsync(client);
                    break;
                case 4:
                    await SaveAllAsync(client);
                    break;
            }
            Console.ReadKey();
            Log.CloseAndFlush();

          
        }

        public static async Task GetByNoAsync(EmployeeService.EmployeeServiceClient client)
        {
            var md = new Metadata
            {
                {"userName","dave"},
                {"rolr","administrator"}
            };

            try
            {
                var response = await client.GetByNoAsync(new GetByNoRequest
                {
                    No = 1991
                }, md);

                Console.WriteLine($"Response message: {response}");
                Console.ReadKey();
            }
            catch (RpcException e)
            {
                if (e.StatusCode == StatusCode.DataLoss)
                {
                    Log.Logger.Error($"{e.Trailers}");
                }
                Log.Logger.Error(e.Message);
            }
            
            
            
          

            
        }

        public static async Task GetAllAsync(EmployeeService.EmployeeServiceClient client)
        {
            using var call = client.GetAll(new GetAllRequest());
            var responseStream = call.ResponseStream;
            while (await responseStream.MoveNext())
            {
                Console.WriteLine(responseStream.Current.Employee);
            }
        }

        public static async Task AddPhotoAsync(EmployeeService.EmployeeServiceClient client)
        {
            var md = new Metadata
            {
                {"userName","dave"},
                {"rolr","administrator"}
            };
            
            FileStream fs = File.OpenRead("logo.png");
            using var call = client.AddPhoto(md);

            var stream = call.RequestStream;

            while (true)
            {
                byte[] buffer = new byte[1024];
                int numRead = await fs.ReadAsync(buffer, 0, buffer.Length);
                if (numRead == 0)
                {
                    break;
                }
                if (numRead < buffer.Length)
                {
                    Array.Resize(ref buffer,numRead);
                }

                await stream.WriteAsync(new AddPhotoRequests() 
                { 
                    Data = ByteString.CopyFrom(buffer)
                });
            }

            await stream.CompleteAsync();
            var response = await call.ResponseAsync;

            Console.WriteLine(response.IsOk);
        }

        public static async Task SaveAllAsync(EmployeeService.EmployeeServiceClient client)
        {
            var employes = new List<Employee>
            {
                new Employee
                {
                    No = 111,
                    FirstName = "monica",
                    LastName = "Geller",
                    //Salary = 7916.1f
                    MonthSalary = new MonthSalary
                    {
                        Basic = 5000f,
                        Bonus = 125.5f
                    },
                    Status = EmployeeStatus.Resigned,
                    LastModified = Timestamp.FromDateTime(DateTime.UtcNow)
                },
                new Employee
                {
                    No = 112,
                    FirstName = "Joey",
                    LastName = "Tribbiani",
                    //Salary = 500
                    MonthSalary = new MonthSalary
                    {
                        Basic = 5000f,
                        Bonus = 125.5f
                    },
                    Status = EmployeeStatus.Resigned,
                    LastModified = Timestamp.FromDateTime(DateTime.UtcNow)
                }
            };

            using var call = client.SaveAll();
            var resquestStream = call.RequestStream;
            var responseStream = call.ResponseStream;


            var responseTask = Task.Run(async () =>
            {
                while(await responseStream.MoveNext())
                {
                    Console.WriteLine($"Saved: {responseStream.Current.Employee}");
                }
            });

            foreach (var employee in employes)
            {
                await resquestStream.WriteAsync(new EmployeeRequest
                {
                    E = employee
                });
            }

            await resquestStream.CompleteAsync();
            await responseTask;

        }
    }
}
