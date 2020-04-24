using System;
using System.Threading.Tasks;
using Grpc.Core;
using EndpointChecker;

namespace EndpointCheckerServer
{
    class EndpointCheckerImpl : EndpointChecker.EndpointChecker.EndpointCheckerBase
    {

        public override Task<EndpointReply> CheckEndpoint(EndpointRequest request, ServerCallContext context)
        {
            var msg = string.Format("Checking {0} @ {1} : ", request.Name, request.IPaddress);
            var success = true;
            if (request.Platform.ToLower() == "windows")
            {
                msg += " Server Passed";

            }
            else
            {
                msg += " Server is not a windows server";
                success = false;
            }
            return Task.FromResult(new EndpointReply { Message = msg, Success = success });
        }

    }

    class Program
    {
        const int Port = 50051;

        public static void Main(string[] args)
        {
            Server server = new Server
            {
                Services = { EndpointChecker.EndpointChecker.BindService(new EndpointCheckerImpl()) },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };
            server.Start();

            Console.WriteLine("Server listening on port " + Port);
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            server.ShutdownAsync().Wait();
        }
    }
}
