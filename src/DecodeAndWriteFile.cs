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
      STORAGE_ACCOUNT_NAME = Environment.GetEnvironmentVariable("StorageAccountName");
      STORAGE_ACCOUNT_INPUT_CONTAINER_NAME = Environment.GetEnvironmentVariable("StorageAccountInputContainerName");
      STORAGE_ACCOUNT_OUTPUT_CONTAINER_NAME = Environment.GetEnvironmentVariable("StorageAccountOutputContainerName");
    }

    [Function("DecodeAndWriteFile")]
    public async Task Run([EventGridTrigger] BlobCreatedEvent input)
    {
      EncodedMessageData encodedMessageData = await DownloadEncodedMessageDataAsync(input.Data.Url);

      DecodedMessageData decodedMessageData = new DecodedMessageData()
      {
        Timestamp = encodedMessageData.Timestamp,
        DecodedMessage = Encoding.UTF8.GetString(Convert.FromBase64String(encodedMessageData.EncodedMessage))
      };

      string uploadBlobUri = $"https://{STORAGE_ACCOUNT_NAME}.blob.core.windows.net/{STORAGE_ACCOUNT_OUTPUT_CONTAINER_NAME}/{input.Data.Url.Split('/').Last()}";

      await UploadDecodedMessageDataAsync(uploadBlobUri, decodedMessageData);
    }

    private async Task<EncodedMessageData> DownloadEncodedMessageDataAsync(string url)
    {
      BlobClient downloadBlobClient = new BlobClient(new Uri(url),
                                                     new DefaultAzureCredential(new DefaultAzureCredentialOptions
                                                     {
                                                       ManagedIdentityClientId = Environment.GetEnvironmentVariable("ManagedIdentityClientId")
                                                     }));

      BlobDownloadResult downloadResult = await downloadBlobClient.DownloadContentAsync();
      return downloadResult.Content.ToObjectFromJson<EncodedMessageData>();
    }

    private async Task UploadDecodedMessageDataAsync(string url, DecodedMessageData decodedMessageData)
    {
      BlobClient uploadBlobClient = new BlobClient(new Uri(url), new DefaultAzureCredential(new DefaultAzureCredentialOptions
      {
        ManagedIdentityClientId = Environment.GetEnvironmentVariable("ManagedIdentityClientId")
      }));

      using (var uploadStream = new MemoryStream())
      {
        await JsonSerializer.SerializeAsync(uploadStream, decodedMessageData);

        //have to reset MemoryStream before trying to use it to upload
        uploadStream.Position = 0;

        await uploadBlobClient.UploadAsync(uploadStream, true);
      }
    }
  }
}
