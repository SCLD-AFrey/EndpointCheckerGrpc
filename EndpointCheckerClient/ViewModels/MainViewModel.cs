using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using Grpc.Core;
using EndpointChecker;
using Newtonsoft.Json;

namespace EndpointCheckerClient.ViewModels
{
    [MetadataType(typeof(MetaData))]
    public class MainViewModel
    {
        public class MetaData : IMetadataProvider<MainViewModel>
        {
            void IMetadataProvider<MainViewModel>.BuildMetadata
                (MetadataBuilder<MainViewModel> p_builder)
            {
                p_builder.CommandFromMethod(p_x => p_x.OnProcessButtonCommand()).CommandName("ProcessButtonCommand");
            }
        }

        #region Constructors

        protected MainViewModel()
        {
            //Config
            Host = "127.0.0.1";
            Port = "50051";
            string jsonFile = "../../../../EndpointChecker/endpoint-list.json";

            //Load initial json file
            using (StreamReader r = new StreamReader(jsonFile))
            {
                endpointJson = r.ReadToEnd();
            }

        }

        public static MainViewModel Create()
        {
            return ViewModelSource.Create(() => new MainViewModel());
        }

        #endregion

        #region Fields and Properties

        public virtual string endpointJson { get; set; }
        public virtual string outText { get; set; }
        public virtual EndpointChecker.EndpointChecker.EndpointCheckerClient client { get; set; }
        public virtual Channel channel { get; set; }
        public virtual List<EndpointItem> items { get; set; }
        public virtual string Host { get; set; }
        public virtual string Port { get; set; }

        #endregion

        #region Methods
        public void OnProcessButtonCommand()
        {
            items = JsonConvert.DeserializeObject<List<EndpointItem>>(endpointJson);
            outText += "Processing..." + Environment.NewLine;
            CreateChannel();
            ProcessEndpointList();
            outText += "Complete" + Environment.NewLine;
        }

        public void CreateChannel()
        {
            var channelCreds = new SslCredentials();


            channel = new Channel(string.Concat(Host, ":", Port), ChannelCredentials.Insecure); //--- without SSL
            //channel = new Channel(string.Concat(Host, ":", Port), channelCreds);
            client = new EndpointChecker.EndpointChecker.EndpointCheckerClient(channel);
        }

        private void ProcessEndpointList()
        {
            foreach (var _endpoint in items)
            {
                //var endpoint = ProcessEndpoint(_endpoint);
                var endpoint = ProcessEndpointAsync(_endpoint).Result;
                outText += String.Format("Checking: {0} @ {1} - Passed={2} Error={3}", endpoint.Name, endpoint.IPaddress, endpoint.Success.ToString(), endpoint.Error) + Environment.NewLine;

            }

            async Task<EndpointItem> ProcessEndpointAsync(EndpointItem item)
            {
                return ProcessEndpoint(item);
            }
        }

        private EndpointItem ProcessEndpoint(EndpointItem endpoint)
        {
            if (endpoint.Name != null)
            {
                try
                {
                    var reply = client.CheckEndpoint(new EndpointRequest
                        { Name = endpoint.Name, IPaddress = endpoint.IPaddress, Platform = endpoint.Platform });

                    endpoint.Success = reply.Success;
                    endpoint.Error = reply.Error.Split("|");
                    endpoint.StartTime = DateTime.Parse(reply.StartTime);
                    endpoint.EndTime = DateTime.Parse(reply.EndTime);

                }
                catch (Exception e)
                {
                    endpoint.Success = false;
                    Array.Resize(ref endpoint.Error, endpoint.Error.Length + 1);
                    endpoint.Error[^1] = "FAILURE: " + e.Message;
                }
            }
            else
            {
                Array.Resize(ref endpoint.Error, endpoint.Error.Length + 1);
                endpoint.Error[^1] = "FAILURE: Endpoint is invalid";
            }

            return endpoint;
        }

        #endregion
    }
}