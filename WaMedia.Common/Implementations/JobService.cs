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

        public void CreateThumbnails(Models.Asset asset)
        {
            // Declare a new job.
            IJob job = this.MediaService.MediaContext.Jobs.Create(asset.MediaAsset.Name + " Thumbnails");
            // Get a media processor reference, and pass to it the name of the 
            // processor to use for the specific task. Use a method like the 
            // GetMediaProcessor method shown in this document.
            IMediaProcessor processor = this.MediaService
                .GetMediaProcessorByName(MediaEncoders.WINDOWS_AZURE_MEDIA_ENCODER);

            ITask task3 = job.Tasks.AddNew("Thumnail creator",
                processor,
                @"<?xml version=""1.0"" encoding=""utf-16""?>
<Thumbnail Size=""150,150"" Type=""Jpeg"" 
Filename=""{OriginalFilename}_{ThumbnailTime}.{DefaultExtension}"">
  <Time Value=""0:0:4"" Step=""0:0:1"" Stop=""0:0:5""/>
</Thumbnail>",
                TaskOptions.None);
            task3.InputAssets.Add(asset.MediaAsset);
            IAsset thumbprintAssets = task3.OutputAssets
                .AddNew(asset.MediaAsset.Name + " Thumbpnails", AssetCreationOptions.None);

            job.Submit();
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
                Tasks.VC1_SMOOTH_STREAMING_HD_720P,
                TaskOptions.None);
            // Specify the input asset to be encoded.
            task.InputAssets.Add(asset.MediaAsset);

            //Add an output asset to contain the results of the job. 
            //This output is specified as AssetCreationOptions.None, which 
            //means the output asset is in the clear (unencrypted). 
            IAsset mp4Asset = task.OutputAssets.AddNew(asset.MediaAsset.Name + " smooth streaming",
                AssetCreationOptions.None);

            IMediaProcessor decryptProcessor = this.MediaService.
                GetMediaProcessorByName(MediaEncoders.STORAGE_DECRYPTION_ENCODER);
            if (decrypt)
            {
                ITask decryptTask = job.Tasks.AddNew("decryption task",
                    decryptProcessor,
                    string.Empty,
                    TaskOptions.None);
                decryptTask.InputAssets.Add(mp4Asset);
                IAsset decryptedMp4 = decryptTask.OutputAssets.AddNew(mp4Asset.Name + "Decrypted " + mp4Asset.Name, AssetCreationOptions.None);
            }

            ITask task3 = job.Tasks.AddNew("Thumnail creator",
                processor,
                @"<?xml version=""1.0"" encoding=""utf-16""?>
<Thumbnail Size=""150,150"" Type=""Jpeg"" 
Filename=""{OriginalFilename}_{ThumbnailTime}.{DefaultExtension}"">
  <Time Value=""0:0:1""/>
  <Time Value=""0:0:4"" Step=""0:0:1"" Stop=""0:0:5""/>
</Thumbnail>",
                TaskOptions.None);
            task3.InputAssets.Add(asset.MediaAsset);
            IAsset thumbprintAssets = task3.OutputAssets
                .AddNew(asset.MediaAsset.Name + " Thumbpnails", AssetCreationOptions.None);

            if (decrypt)
            {
                ITask decryptThumbnails = job.Tasks
                    .AddNew(mp4Asset.Name + "Decrypted thumbnails", decryptProcessor, string.Empty, TaskOptions.None);
                decryptThumbnails.InputAssets.Add(thumbprintAssets);
                decryptThumbnails.OutputAssets.AddNew(thumbprintAssets.Name + " Thumbnails", AssetCreationOptions.None);
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
                TaskOptions.None);
            // Specify the input asset to be encoded.
            task.InputAssets.Add(theAsset.MediaAsset);

            //Add an output asset to contain the results of the job. 
            //This output is specified as AssetCreationOptions.None, which 
            //means the output asset is in the clear (unencrypted). 
            IAsset decruptedAsset = task.OutputAssets
                .AddNew(theAsset.MediaAsset.Name + " decrypted",
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
                TaskOptions.None);
            // Specify the input asset to be encoded.
            task.InputAssets.Add(asset.MediaAsset);

            //Add an output asset to contain the results of the job. 
            //This output is specified as AssetCreationOptions.None, which 
            //means the output asset is in the clear (unencrypted). 
            IAsset outputAsset = task.OutputAssets.AddNew(asset.MediaAsset.Name + " result ",
                AssetCreationOptions.None);
            job.Submit();
        }


        public void CancelJob(string jobId)
        {
            IJob job = this.MediaService.MediaContext.Jobs.Where(j => j.Id.Equals(jobId)).FirstOrDefault();
            if (job != null)
            {
                job.Cancel();
            }
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
            task = @"<taskDefinition xmlns=""http://schemas.microsoft.com/iis/media/v4/TM/TaskDefinition#"">
  <name>PlayReady Protection</name>
  <id>9A3BFEAC-F8AE-41CA-87FA-D639E4D1C753</id>
  <properties namespace=""http://schemas.microsoft.com/iis/media/v4/SharedData#"" prefix=""sd"">
    <property name=""contentKey""              required=""false"" value="""" helpText=""A base64-encoded 16-byte value, which is produced by the key seed in conjunction with the key ID and is used to encrypt content. You must enter a content key value if no key seed value is specified."" />
    <property name=""customAttributes""        required=""false"" value="""" helpText=""A comma-delimited list of name:value pairs (in the form name1:value1,name2:value2,name3:value3) to be included in the CUSTOMATTRIBUTES section of the WRM header. The WRM header is XML metadata added to encrypted content and included in the client manifest. It is also included in license challenges made to license servers."" />
    <property name=""dataFormats""             required=""false"" value=""h264, avc1, mp4a, vc1, wma, owma, ovc1, aacl, aach, ac-3, ec-3, mlpa, dtsc, dtsh, dtsl, dtse"" helpText=""A comma-delimited list of four-character codes (FourCCs) that specify the data formats to be encrypted. If no value is specified, all data formats are encrypted."" />
    <property name=""keyId""                   required=""false"" value="""" helpText=""A globally unique identifier (GUID) that uniquely identifies content for the purposes of licensing. Each presentation should use a unique value. If no value is specified, a random value is used."" />
    <property name=""keySeedValue""            required=""false"" value=""XVBovsmzhP9gRIZxWfFta3VVRPzVEWmJsazEJ46I"" helpText=""A base64-encoded 30-byte value, which is used in conjunction with the key ID to create the content key. Typically, one key seed is used with many key IDs to protect multiple files or sets of files; for example, all files issued by a license server or perhaps all files by a particular artist. Key seeds are stored on license servers."" />
    <property name=""licenseAcquisitionUrl""   required=""true""  value=""https://play-lic.cimcontent.net/playready/RightsManager.asmx"" helpText=""The Web page address on a license server from which clients can obtain a license to play the encrypted content."" />
  </properties>
  <description xml:lang=""en"">Encrypts on-demand Smooth Streams for use by Microsoft PlayReady and updates the client manifest used by Silverlight clients.</description>
  <inputFolder></inputFolder>
  <outputFolder>Protected</outputFolder>
  <taskCode>
    <type>Microsoft.Web.Media.TransformManager.DigitalRightsManagementTask, Microsoft.Web.Media.TransformManager.DigitalRightsManagement, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35</type>
  </taskCode>
</taskDefinition>
";
            return string.Format(task, contentKey, keyId, keySeed, playReadyServerUrl);
        }



        public string GetSmoothToHlsTask()
        {
            return @"
<taskDefinition xmlns='http://schemas.microsoft.com/iis/media/v4/TM/TaskDefinition#'>
    <name>Smooth Streams to Apple HTTP Live Streams</name>
  <id>A72D7A5D-3022-45f2-89B4-1DDC5457C111</id>
    <description xml:lang='en'>Converts on-demand Smooth Streams encoded with H.264 (AVC) video and AAC-LC audio codecs to Apple HTTP Live Streams (MPEG-2 TS) and creates an Apple HTTP Live Streaming playlist (.m3u8) file for the converted presentation.</description>
    <inputDirectory></inputDirectory>
    <outputFolder>TS_Out</outputFolder>
    <properties namespace='http://schemas.microsoft.com/iis/media/AppleHTTP#' prefix='hls'>
        <property name='maxbitrate' required='true' value='8500000' helpText='The maximum bit rate, in bits per second (bps), to be converted to MPEG-2 TS. On-demand Smooth Streams at or below this value are converted to MPEG-2 TS segments. Smooth Streams above this value are not converted. Most Apple devices can play media encoded at bit rates up to 8,500 Kbps.'/>
        <property name='manifest' required='false' value='' helpText='The file name to use for the converted Apple HTTP Live Streaming playlist file (a file with an .m3u8 file name extension). If no value is specified, the following default value is used: &lt;ISM_file_name&gt;-m3u8-aapl.m3u8'/>
        <property name='segment' required='false' value='10' helpText='The duration of each MPEG-2 TS segment, in seconds. 10 seconds is the Apple-recommended setting for most Apple mobile digital devices.'/>
        <property name='log'  required='false' value='' helpText='The file name to use for a log file (with a .log file name extension) that records the conversion activity. If you specify a log file name, the file is stored in the task output folder.' />
        <property name='encrypt'  required='false' value='false' helpText='Enables encryption of MPEG-2 TS segments by using the Advanced Encryption Standard (AES) with a 128-bit key (AES-128).' />
        <property name='pid'  required='false' value='' helpText='The program ID of the MPEG-2 TS presentation. Different encodings of MPEG-2 TS streams in the same presentation use the same program ID so that clients can easily switch between bit rates.' />
        <property name='codecs'  required='false' value='false' helpText='Enables codec format identifiers, as defined by RFC 4281, to be included in the Apple HTTP Live Streaming playlist (.m3u8) file.' />
        <property name='backwardcompatible'  required='false' value='false' helpText='Enables playback of the MPEG-2 TS presentation on devices that use the Apple iOS 3.0 mobile operating system.' />
        <property name='allowcaching'  required='false' value='true' helpText='Enables the MPEG-2 TS segments to be cached on Apple devices for later playback.' />
        <property name='passphrase'  required='false' value='' helpText='A passphrase that is used to generate the content key identifier.' />
        <property name='key'  required='false' value='' helpText='The hexadecimal representation of the 16-octet content key value that is used for encryption.' />
        <property name='keyuri'  required='false' value='' helpText='An alternate URI to be used by clients for downloading the key file. If no value is specified, it is assumed that the Live Smooth Streaming publishing point provides the key file.' />
        <property name='overwrite'  required='false' value='true' helpText='Enables existing files in the output folder to be overwritten if converted output files have identical file names.' />
    </properties>
    <taskCode>
        <type>Microsoft.Web.Media.TransformManager.SmoothToHLS.SmoothToHLSTask, Microsoft.Web.Media.TransformManager.SmoothToHLS, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35</type>
    </taskCode>
</taskDefinition>
";
        }
    }
}
