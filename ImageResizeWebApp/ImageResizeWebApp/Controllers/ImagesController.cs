```csharp
using Azure.Identity;
using Azure.Storage.Blobs;

public ImagesController(IOptions<AzureStorageConfig> config)
{
    storageConfig = config.Value;
    blobServiceClient = new BlobServiceClient(new Uri($"https://{storageConfig.AccountName}.blob.core.windows.net/"), new DefaultAzureCredential());
}

public async Task<IActionResult> Upload(ICollection<IFormFile> files)
{
    bool isUploaded = false;

    try
    {
        if (files.Count == 0)
            return BadRequest("No files received from the upload");

        if (storageConfig.ImageContainer == string.Empty)
            return BadRequest("Please provide a name for your image container in the azure blob storage");

        foreach (var formFile in files)
        {
            if (StorageHelper.IsImage(formFile))
            {
                if (formFile.Length > 0)
                {
                    using (Stream stream = formFile.OpenReadStream())
                    {
                        isUploaded = await StorageHelper.UploadFileToStorage(stream, formFile.FileName, storageConfig, blobServiceClient);
                    }
                }
            }
            else
            {
                return new UnsupportedMediaTypeResult();
            }
        }

        if (isUploaded)
        {
            if (storageConfig.ThumbnailContainer != string.Empty)
                return new AcceptedAtActionResult("GetThumbNails", "Images", null, null);
            else
                return new AcceptedResult();
        }
        else
            return BadRequest("Look like the image couldn't upload to the storage");
    }
    catch (Exception ex)
    {
        return BadRequest(ex.Message);
    }
}

public async Task<IActionResult> GetThumbNails()
{
    try
    {
        if (storageConfig.ImageContainer == string.Empty)
            return BadRequest("Please provide a name for your image container in Azure blob storage.");

        List<string> thumbnailUrls = await StorageHelper.GetThumbNailUrls(storageConfig, blobServiceClient);
        return new ObjectResult(thumbnailUrls);            
    }
    catch (Exception ex)
    {
        return BadRequest(ex.Message);
    }
}
```