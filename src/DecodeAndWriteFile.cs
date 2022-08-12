using System;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Azure.Identity;
using Azure.Storage.Blobs.Models;
using System.Text;
using System.Text.Json;

namespace src
{
  public class DecodeAndWriteFile
  {
    private readonly ILogger _logger;
    private readonly string STORAGE_ACCOUNT_NAME;
    private readonly string STORAGE_ACCOUNT_INPUT_CONTAINER_NAME;
    private readonly string STORAGE_ACCOUNT_OUTPUT_CONTAINER_NAME;

    public DecodeAndWriteFile(ILoggerFactory loggerFactory)
    {
      _logger = loggerFactory.CreateLogger<DecodeAndWriteFile>();
      STORAGE_ACCOUNT_NAME = System.Environment.GetEnvironmentVariable("StorageAccountName");
      STORAGE_ACCOUNT_INPUT_CONTAINER_NAME = System.Environment.GetEnvironmentVariable("StorageAccountInputContainerName");
      STORAGE_ACCOUNT_OUTPUT_CONTAINER_NAME = System.Environment.GetEnvironmentVariable("StorageAccountOutputContainerName");
    }

    [Function("DecodeAndWriteFile")]
    public async Task Run([EventGridTrigger] MyEvent input)
    {
      _logger.LogInformation(input.Data.ToString());

      BlobClient downloadBlobClient = new BlobClient(new Uri(input.Data.Url),
                                   new DefaultAzureCredential(new DefaultAzureCredentialOptions
                                   {
                                     ManagedIdentityClientId = System.Environment.GetEnvironmentVariable("ManagedIdentityClientId")
                                   }));

      BlobDownloadResult downloadResult = await downloadBlobClient.DownloadContentAsync();
      EncodedMessageData encodedMessageData = downloadResult.Content.ToObjectFromJson<EncodedMessageData>();

      DecodedMessageData decodedMessageData = new DecodedMessageData()
      {
        Timestamp = encodedMessageData.Timestamp,
        DecodedMessage = Encoding.UTF8.GetString(Convert.FromBase64String(encodedMessageData.EncodedMessage))
      };

      Uri uploadBlobUri = new Uri($"https://{STORAGE_ACCOUNT_NAME}.blob.core.windows.net/{STORAGE_ACCOUNT_OUTPUT_CONTAINER_NAME}/{input.Data.Url.Split('/').Last()}");

      BlobClient uploadBlobClient = new BlobClient(uploadBlobUri, new DefaultAzureCredential(new DefaultAzureCredentialOptions
      {
        ManagedIdentityClientId = System.Environment.GetEnvironmentVariable("ManagedIdentityClientId")
      }));

      using (var uploadStream = new MemoryStream())
      {
        await JsonSerializer.SerializeAsync(uploadStream, decodedMessageData);

        uploadStream.Position = 0;

        await uploadBlobClient.UploadAsync(uploadStream, true);
      }
    }
  }
}

public class EncodedMessageData
{
  public DateTime Timestamp { get; set; }
  public string EncodedMessage { get; set; }
}

public class DecodedMessageData
{
  public DateTime Timestamp { get; set; }
  public string DecodedMessage { get; set; }
}

public class MyEvent
{
  public string Id { get; set; }

  public string Topic { get; set; }

  public string Subject { get; set; }

  public string EventType { get; set; }

  public DateTime EventTime { get; set; }

  public BlobCreatedEventData Data { get; set; }
}

public class BlobCreatedEventData
{
  public string Api { get; set; }
  public string ClientRequestId { get; set; }
  public string RequestId { get; set; }
  public string eTag { get; set; }
  public string ContentType { get; set; }
  public int ContentLength { get; set; }
  public string BlobType { get; set; }
  public string Url { get; set; }
  public string Sequencer { get; set; }
  public object StorageDiagnostics { get; set; }
}
