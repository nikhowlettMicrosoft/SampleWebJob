using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Azure.Messaging.ServiceBus;
using System.Text;
using System.Runtime.Serialization;
using System.Xml;
using Microsoft.Extensions.Hosting;
using Microsoft.Azure.WebJobs.ServiceBus;

using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

public class Receiver
{
	public Receiver()
	{
	}

    public static async Task RunView(
        [ServiceBusTrigger(topicName: "xcardView", subscriptionName: "ViewCall", Connection = "ServiceBus")] ServiceBusReceivedMessage thisMessage,
        [ServiceBus("prodcompletedscorecardids", entityType: ServiceBusEntityType.Topic, Connection = "ServiceBus")]
    IAsyncCollector<ServiceBusMessage> xCardViewClient)
        
    {
        var xCardPreloadScorecardMessage = new ServiceBusMessage(thisMessage);
        await xCardViewClient.AddAsync(xCardPreloadScorecardMessage);
    }

    public async Task RunReceive(
        [ServiceBusTrigger(topicName: "prodcompletedscorecardids", subscriptionName: "ViewCall", Connection = "ServiceBus")] ServiceBusReceivedMessage thisMessage)
    {
        var receivedData = thisMessage;
    }
}
