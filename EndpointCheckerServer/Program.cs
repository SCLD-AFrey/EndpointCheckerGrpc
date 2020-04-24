using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Grpc.Core;
using EndpointChecker;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using Microsoft.Extensions.Configuration;

namespace EndpointCheckerServer
{
    class EndpointCheckerImpl : EndpointChecker.EndpointChecker.EndpointCheckerBase
    {
        class Program
        {
            const string Host = "127.0.0.1";
            const int Port = 50051;

            public static void Main(string[] args)
            {
                //XPO Database Connections
                IConfiguration AppConfiguration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .Build();
                ConnectionHelper.Connect(AppConfiguration, autoCreateOption: AutoCreateOption.DatabaseAndSchema);

                Server server = new Server
                {
                    Services = { EndpointChecker.EndpointChecker.BindService(new EndpointCheckerImpl()) },
                    Ports = { new ServerPort(Host, Port, ServerCredentials.Insecure) }
                };
                server.Start();

                Console.WriteLine("Server listening on port " + Port);
                Console.WriteLine("Press any key to stop the server...");
                Console.ReadKey();

                server.ShutdownAsync().Wait();
            }
        }

        public override Task<EndpointReply> CheckEndpoint(EndpointRequest request, ServerCallContext context)
        {
            var lErrors = new List<string>();
            var reply = new EndpointReply
            {
                StartTime = DateTime.Now.ToString(),
                Success = true,
                Error = "none"
            };

            // DO TESTS
            if (request.Platform.ToLower() != "windows")
            {
                reply.Success = false;
                lErrors.Add(request.Platform + " not allowed");
            }

            if (!request.IPaddress.StartsWith("10."))
            {
                reply.Success = false;
                lErrors.Add(request.IPaddress + " not within IP range of 10.*.*.*");
            }

            reply.EndTime = DateTime.Now.ToString();
            reply.Message = string.Format("Server {0} at {1} returned a Success of {2}",
                request.Name,
                request.IPaddress,
                reply.Success.ToString()
            );

            //Check for succcess
            if (!reply.Success)
            {
                reply.Error = string.Join(", ", lErrors);
                reply.Message += " due to " + reply.Error;
            }

            //Add result to DB
            InsertCheck(request, reply);

            return Task.FromResult(reply);
        }

        public void InsertCheck(EndpointRequest request, EndpointReply reply)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                EndpointCheck check = new EndpointCheck(unitOfWork)
                {
                    Name = request.Name,
                    IPaddress = request.IPaddress,
                    Platform = request.Platform,
                    Success = reply.Success,
                    Error = reply.Error,
                    StartTime = DateTime.Parse(reply.StartTime),
                    EndTime = DateTime.Parse(reply.EndTime)
                };
                unitOfWork.CommitChanges();
            }
        }
    }
}
