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
                p_builder.CommandFromMethod(p_x => p_x.OnProcessButtonListCommand()).CommandName("ProcessButtonListCommand");
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

        public void CreateChannel()
        {
            channel = new Channel(string.Concat(Host, ":", Port), ChannelCredentials.Insecure); //--- without SSL
            client = new EndpointChecker.EndpointChecker.EndpointCheckerClient(channel);
        }

        #region Methods
        public void OnProcessButtonCommand()
        {
            items = JsonConvert.DeserializeObject<List<EndpointItem>>(endpointJson);
            outText = "Processing..." + Environment.NewLine;
            CreateChannel();
            ProcessEndpointListByEntry();
            outText += "Complete" + Environment.NewLine;
        }

        private void ProcessEndpointListByEntry()
        {
            foreach (var _endpoint in items)
            {
                var endpoint = ProcessEndpoint(_endpoint);
                outText += String.Format("Checking: {0} @ {1} - Passed={2}", endpoint.Name, endpoint.IPaddress, endpoint.Success.ToString()) + Environment.NewLine;

            }
        }

        private EndpointItem ProcessEndpoint(EndpointItem endpointItem)
        {

            try
            {
                var endpointrequest = new EndpointRequest
                    {Name = endpointItem.Name, IPaddress = endpointItem.IPaddress, Platform = endpointItem.Platform};
                var reply = client.CheckEndpoint(endpointrequest);
                
                endpointItem.Success = reply.Success;
                endpointItem.StartTime = DateTime.Parse(reply.StartTime);
                endpointItem.EndTime = DateTime.Parse(reply.EndTime);

            }
            catch (Exception e)
            {
                endpointItem.Success = false;
            }


            return endpointItem;
        }

        public void OnProcessButtonListCommand()
        {
            var requestList = endpointJson;
            outText = "Processing..." + Environment.NewLine;
            
            CreateChannel();

            var endpointrequest = new EndpointListRequest
            {
                Content = requestList
            };

            var reply = client.CheckEndpointList(endpointrequest);

            var successList = JsonConvert.DeserializeObject<List<EndpointItem>>(reply.SuccessList);
            var failList = JsonConvert.DeserializeObject<List<EndpointItem>>(reply.FailList);

            outText += "Started: " + reply.StartTime + Environment.NewLine;
            outText += "SUCCESS: " + successList.Count + Environment.NewLine;
            outText += "FAILED: " + failList.Count + Environment.NewLine;
            outText += "End: " + reply.EndTime + Environment.NewLine;

            outText += "Complete" + Environment.NewLine;
        }

        #endregion
    }
}