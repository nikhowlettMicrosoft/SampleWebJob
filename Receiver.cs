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
        var bytes = thisMessage.Body.ToArray();
        var scorecardId = 260964494;
        /*BlobServiceClient client = new BlobServiceClient(
                    new Uri($"https://testhipaa543.blob.core.windows.net"),
                    new DefaultAzureCredential());
        BlobContainerClient container = client.GetBlobContainerClient("whateverItdoesntMatter");
        */
        //var blockBlob = container.GetBlobClient(filePath);
        var data = "just a string";
        //await blockBlob.UploadAsync(data).ConfigureAwait(false);
        var xCardPreloadScorecardMessage = new ServiceBusMessage(thisMessage);
        await xCardViewClient.AddAsync(xCardPreloadScorecardMessage);
    }

    public async Task RunReceive(
        [ServiceBusTrigger(topicName: "prodcompletedscorecardids", subscriptionName: "ViewCall", Connection = "ServiceBus")] ServiceBusReceivedMessage thisMessage)
    {
        var receivedData = thisMessage;
    }
}
