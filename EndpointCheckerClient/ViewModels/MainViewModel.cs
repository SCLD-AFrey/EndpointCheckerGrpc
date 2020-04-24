using System.ComponentModel.DataAnnotations;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;

using System;
using System.IO;
using System.Collections.Generic;
using DevExpress.Mvvm.Native;
using Grpc;
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
            string jsonFile = "../../../../EndpointChecker/endpoint-list.json";
            using (StreamReader r = new StreamReader(jsonFile))
            {
                string json = r.ReadToEnd();
                JsonList = json;
                TextOut = "Client Started" + Environment.NewLine;
            }
        }

        public static MainViewModel Create()
        {
            return ViewModelSource.Create(() => new MainViewModel());
        }

        #endregion

        #region Fields and Properties

        public virtual string JsonList { get; set; }
        public virtual string TextOut { get; set; }

        #endregion

        #region Methods
        public void OnProcessButtonCommand()
        {

            TextOut += "Processing..." + Environment.NewLine;
            Channel channel = new Channel("127.0.0.1:50051", ChannelCredentials.Insecure);
            var client = new EndpointChecker.EndpointChecker.EndpointCheckerClient(channel);

            ProcessEndpointList(client);
            TextOut += "Complete" + Environment.NewLine;
        }

        private void ProcessEndpointList(EndpointChecker.EndpointChecker.EndpointCheckerClient client)
        {
            List<EndpointItem> items = JsonConvert.DeserializeObject<List<EndpointItem>>(JsonList);

            var s = client.ReturnSuccess();

            foreach (var i in items)
            {
                if (i.Name != null)
                {


                    try
                    {
                        var newreply = client.CheckEndpoint(new EndpointRequest
                            { Name = i.Name, IPaddress = i.IPaddress, Platform = i.Platform });
                        TextOut += "Checking: " + newreply.Message + Environment.NewLine;

                    }
                    catch (Exception e)
                    {
                        TextOut += "Exception: " + e.Message + Environment.NewLine;
                    }


                    

                }
            }
        }

        #endregion
    }
}