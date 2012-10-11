using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WaMedia.Common.Contracts;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WaMedia.Common.Implementations
{
    public class JobService : IJobService
    {
        private IMediaService _mediaService;
        public IMediaService MediaService
        {
            get { return this._mediaService; }
        }

        public JobService(IMediaService mediaService)
        {
            this._mediaService = mediaService;
        }

        public IEnumerable<Microsoft.WindowsAzure.MediaServices.Client.IJob> Jobs
        {
            get
            {
                return (from j in this.MediaService.MediaContext.Jobs select j).ToList();
            }
        }

        public void DeleteJob(string jobId)
        {
            var jobToDelete = (from j in this.MediaService.MediaContext.Jobs where j.Id == jobId select j).FirstOrDefault();
            if (jobToDelete != null)
            {
                jobToDelete.Delete();
            }
        }


        public void CreateEncodeToSmoothStreamingJob(Models.Asset asset, bool decrypt = false)
        {
            // Declare a new job.
            IJob job = this.MediaService.MediaContext.Jobs.Create(asset.MediaAsset.Name + " Encoding Job");

            // Get a media processor reference, and pass to it the name of the 
            // processor to use for the specific task. Use a method like the 
            // GetMediaProcessor method shown in this document.
            IMediaProcessor processor = this.MediaService
                .GetMediaProcessorByName(MediaEncoders.WINDOWS_AZURE_MEDIA_ENCODER);
            //  Create a task with the encoding details, using a string preset.
            ITask task = job.Tasks.AddNew("New Encoding Task",
                processor,
                Tasks.H264_IIS_SMOOTH_STREAMING_HD_720P_CBR,
                TaskCreationOptions.None);
            // Specify the input asset to be encoded.
            task.InputMediaAssets.Add(asset.MediaAsset);

            //Add an output asset to contain the results of the job. 
            //This output is specified as AssetCreationOptions.None, which 
            //means the output asset is in the clear (unencrypted). 
            IAsset mp4Asset = task.OutputMediaAssets.AddNew(asset.MediaAsset.Name + " smooth streaming",
                true,
                AssetCreationOptions.None);

            IMediaProcessor decryptProcessor = this.MediaService.
                GetMediaProcessorByName(MediaEncoders.STORAGE_DECRYPTION_ENCODER);
            if (decrypt)
            {
                ITask decryptTask = job.Tasks.AddNew("decryption task",
                    decryptProcessor,
                    string.Empty,
                    TaskCreationOptions.None);
                decryptTask.InputMediaAssets.Add(mp4Asset);
                IAsset decryptedMp4 = decryptTask.OutputMediaAssets.AddNew(mp4Asset.Name + "Decrypted " + mp4Asset.Name, true, AssetCreationOptions.None);
            }

            ITask task3 = job.Tasks.AddNew("Thumnail creator",
                processor,
                @"<?xml version=""1.0"" encoding=""utf-16""?>
<Thumbnail Size=""80,60"" Type=""Jpeg"" 
Filename=""{OriginalFilename}_{ThumbnailTime}.{DefaultExtension}"">
  <Time Value=""0:0:0""/>
  <Time Value=""0:0:4"" Step=""0:0:1"" Stop=""0:0:5""/>
</Thumbnail>",
                TaskCreationOptions.None);
            task3.InputMediaAssets.Add(asset.MediaAsset);
            IAsset thumbprintAssets = task3.OutputMediaAssets
                .AddNew(asset.MediaAsset.Name + " Thumbprint", true, AssetCreationOptions.None);

            if (decrypt)
            {
                ITask decryptThumbnails = job.Tasks
                    .AddNew(mp4Asset.Name + "Decrypted thumbnails", decryptProcessor, string.Empty, TaskCreationOptions.None);
                decryptThumbnails.InputMediaAssets.Add(thumbprintAssets);
                decryptThumbnails.OutputMediaAssets.AddNew(thumbprintAssets.Name + " Thumbnails", true, AssetCreationOptions.None);
            }
            // Launch the job. 
            job.Submit();
        }


        public void DecryptAsset(Models.Asset theAsset)
        {
            // Declare a new job.
            IJob job = this.MediaService.MediaContext.Jobs.Create(theAsset.MediaAsset.Name + " Decrypting Job");

            // Get a media processor reference, and pass to it the name of the 
            // processor to use for the specific task. Use a method like the 
            // GetMediaProcessor method shown in this document.
            IMediaProcessor processor = this.MediaService
                .GetMediaProcessorByName(MediaEncoders.STORAGE_DECRYPTION_ENCODER);
            //  Create a task with the encoding details, using a string preset.
            ITask task = job.Tasks.AddNew("New Decrypting",
                processor,
                string.Empty,
                TaskCreationOptions.None);
            // Specify the input asset to be encoded.
            task.InputMediaAssets.Add(theAsset.MediaAsset);

            //Add an output asset to contain the results of the job. 
            //This output is specified as AssetCreationOptions.None, which 
            //means the output asset is in the clear (unencrypted). 
            IAsset decruptedAsset = task.OutputMediaAssets
                .AddNew(theAsset.MediaAsset.Name + " decrypted",
                true,
                AssetCreationOptions.None);
            // Launch the job. 
            job.Submit();
        }


        public void CreateNewJob(Models.Asset asset, string mediaEncoder, string taskPreset)
        {
            // Declare a new job.
            IJob job = this.MediaService.MediaContext.Jobs.Create(asset.MediaAsset.Name + " Job");

            // Get a media processor reference, and pass to it the name of the 
            // processor to use for the specific task. Use a method like the 
            // GetMediaProcessor method shown in this document.
            IMediaProcessor processor = this.MediaService
                .GetMediaProcessorByName(mediaEncoder);
            //  Create a task with the encoding details, using a string preset.
            ITask task = job.Tasks.AddNew("Ad-hoc Task",
                processor,
                taskPreset,
                TaskCreationOptions.None);
            // Specify the input asset to be encoded.
            task.InputMediaAssets.Add(asset.MediaAsset);

            //Add an output asset to contain the results of the job. 
            //This output is specified as AssetCreationOptions.None, which 
            //means the output asset is in the clear (unencrypted). 
            IAsset outputAsset = task.OutputMediaAssets.AddNew(asset.MediaAsset.Name + " result ",
                true,
                AssetCreationOptions.None);
            job.Submit();
        }


        public string GetPlayReadyTask(string contentKey = "", string keyId = "", string keySeed = "", string playReadyServerUrl = "")
        {
            string task = @"<taskDefinition xmlns='http://schemas.microsoft.com/iis/media/v4/TM/TaskDefinition#'>
  <name>PlayReady Protection</name>
  <id>9A3BFEAC-F8AE-41CA-87FA-D639E4D1C753</id>
  <properties namespace='http://schemas.microsoft.com/iis/media/v4/SharedData#' prefix='sd'>
    <property name='contentKey'  required='false' value='{0}' helpText='A base64-encoded 16-byte value, which is produced by the key seed in conjunction with the key ID and is used to encrypt content. You must enter a content key value if no key seed value is specified.' />
    <property name='customAttributes'  required='false' value='' helpText='A comma-delimited list of name:value pairs (in the form name1:value1,name2:value2,name3:value3) to be included in the CUSTOMATTRIBUTES section of the WRM header. The WRM header is XML metadata added to encrypted content and included in the client manifest. It is also included in license challenges made to license servers.' />
    <property name='dataFormats' required='false' value='h264, avc1, mp4a, vc1, wma, owma, ovc1, aacl, aach, ac-3, ec-3, mlpa, dtsc, dtsh, dtsl, dtse' helpText='A comma-delimited list of four-character codes (FourCCs) that specify the data formats to be encrypted. If no value is specified, all data formats are encrypted.' />
    <property name='keyId' required='false' value='{1}' helpText='A globally unique identifier (GUID) that uniquely identifies content for the purposes of licensing. Each presentation should use a unique value. If no value is specified, a random value is used.' />
    <property name='keySeedValue' required='false' value='{2}' helpText='A base64-encoded 30-byte value, which is used in conjunction with the key ID to create the content key. Typically, one key seed is used with many key IDs to protect multiple files or sets of files; for example, all files issued by a license server or perhaps all files by a particular artist. Key seeds are stored on license servers.' />
    <property name='licenseAcquisitionUrl'  required='true'  value='{3}' helpText='The Web page address on a license server from which clients can obtain a license to play the encrypted content.' />
  </properties>
  <description xml:lang='en'>Encrypts on-demand Smooth Streams for use by Microsoft PlayReady and updates the client manifest used by Silverlight clients.</description>
  <inputFolder></inputFolder>
  <outputFolder>Protected</outputFolder>
  <taskCode>
    <type>Microsoft.Web.Media.TransformManager.DigitalRightsManagementTask, Microsoft.Web.Media.TransformManager.DigitalRightsManagement, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35</type>
  </taskCode>
</taskDefinition>";
            return string.Format(task, contentKey, keyId, keySeed, playReadyServerUrl);
        }
    }
}
