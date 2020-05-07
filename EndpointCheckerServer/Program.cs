using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using EndpointChecker;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
                #region Configuration
                
                IConfiguration AppConfiguration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .Build();
                ConnectionHelper.Connect(AppConfiguration, autoCreateOption: AutoCreateOption.DatabaseAndSchema);

                #endregion

                Server server = new Server
                {
                    Services = { EndpointChecker.EndpointChecker.BindService(new EndpointCheckerImpl()) },
                    Ports = { new ServerPort(Host, Port, ServerCredentials.Insecure) }
                };
                server.Start();

                Console.WriteLine("Server listening on " + Host + " port " + Port);
                Console.WriteLine("Press any key to stop the server...");
                Console.ReadKey();

                server.ShutdownAsync().Wait();
            }
        }

        public override Task<EndpointReply> CheckEndpoint(EndpointRequest request, ServerCallContext context)
        {
            var reply = new EndpointReply
            {
                StartTime = DateTime.Now.ToString(),
                Success = true
            };

            // DO TESTS
            if (request.Platform.ToLower() != "windows")
            {
                reply.Success = false;
            }

            if (!request.IPaddress.StartsWith("10."))
            {
                reply.Success = false;
            }

            reply.EndTime = DateTime.Now.ToString();

            //Add result to DB
            InsertCheck(request, reply);

            return Task.FromResult(reply);
        }


        public override Task<EndpointListReply> CheckEndpointList(EndpointListRequest request, ServerCallContext context)
        {
            var reply = new EndpointListReply
            {
                StartTime = DateTime.Now.ToString()
            };
            var items = JsonConvert.DeserializeObject<List<EndpointItem>>(request.Content);
            var successList = new List<EndpointItem>();
            var faiList = new List<EndpointItem>();

            foreach (var item in items)
            {
                item.Success = true;

                // DO TESTS
                if (item.Platform.ToLower() != "windows")
                {
                    item.Success = false;
                }

                if (!item.IPaddress.StartsWith("10."))
                {
                    item.Success = false;
                }

                if (item.Success)
                {
                    successList.Add(item);
                } else
                {
                    faiList.Add(item);
                }

            }

            reply.SuccessList = JsonConvert.SerializeObject(successList);
            reply.FailList = JsonConvert.SerializeObject(faiList);
            reply.EndTime = DateTime.Now.ToString();

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
                    StartTime = DateTime.Parse(reply.StartTime),
                    EndTime = DateTime.Parse(reply.EndTime)
                };
                unitOfWork.CommitChanges();
            }
        }




    }
}
