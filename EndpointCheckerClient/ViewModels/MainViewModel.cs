using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
            channel = new Channel(string.Concat(Host, ":", Port), ChannelCredentials.Insecure);
            client = new EndpointChecker.EndpointChecker.EndpointCheckerClient(channel);
        }

        private void ProcessEndpointList()
        {
            foreach (var i in items)
            {
                if (i.Name != null)
                {
                    try
                    {
                        var newreply = client.CheckEndpoint(new EndpointRequest
                            { Name = i.Name, IPaddress = i.IPaddress, Platform = i.Platform });
                        outText += "Checking: " + newreply.Message + Environment.NewLine;

                    }
                    catch (Exception e)
                    {
                        outText += "Exception: " + e.Message + Environment.NewLine;
                    }
                }
            }
        }

        #endregion
    }
}