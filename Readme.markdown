# A Windows Azure Media Services explorative sample #

This sample is an MVC showcase app for basic functionalities of WAMS. The MVC App relies on a Common library.
To start simply put your MediaAccount and MediaKey in the web.config of the Web App, appSettings section.

## WaMedia.Common library architecture. ##

The WaMedia.Common library constits of both contracts (Interfaces) and their concrete implementations.
The idea behid the contracts is to logically separate different operations on the Media Services API.

The application has 6 menu items: [Home] [Assets] [Jobs] [Procs] [Locators] [~ Reset ~]:
* [Home] - is the main screen with some marketing info
* [Assets] - the Asset relate screen. Here you can create asset, encode to smooth streaming, protect with play ready, get the smooth streming url, decrypt (if it is decrypted). Please note the following contraints while working with Asset:
 ** Only ISO MP4 video Assets can be encoded into Smooth Streaming format (the [encode] link), so first use the [convert to mp4] link, if the asset is not MP4
 ** Only Smooth streaming asset can be PlayReady protected! So make sure you execute this over an asset, which is Smooth Streaming encoded
 ** You can get the Streaming URL for Smooth Streaming (and Play Ready protected) assets only
 ** You can get the mp4 progressive download link for MP4 assets only
* [Jobs] - the Jobs screen. Here you can view all the jobs with their states and accosiated tasks. You can delete job from this screen
* [Procs] - a static list of available media processors
* [Locators] - a static list of locators that are currently created for the media account
* [~ RESET ~] - resets the whole Media Services account. This includes: deleting all the jobs, deleting all locators, deleting all assets.

Following contracts are implemented

### IMediaService ###
  Base contract, which is referenced by all others, as it holds reference to the CloudMediaContext.
  Unfortunatelly the CloudMediaContext does not implement any interface and cannot be abstracted as a Contract.
  IMediaService has following members
  * CloudMediaContext MediaContext 
  * void Reset() - resets the whole Media Service account. Just leaves the Content Keys. Be careful with the content Keys. If you delete them, you will no longer be able to create a non-empty asset!
  * IMediaProcessor GetMediaProcessorByName(string name) - retrieves an instance of media processor, which can later be used to create Tasks


  ### IAssetService ###
  Contract for managing Assets. It has following members
  * IMediaService MediaService - reference to the IMediaService
  * IQueryable<Asset> Assets - Asset is local model created with the idea to provide additional properties like Thumbnail, but it appeared to be harder to implement thumbnailing, so it is depricated feature for now
  * Asset GetAssetById(string assetId) - As its name suggests, this method retrieves an assed by given ID
  * [Obsolete("Dont use!")] string ThumbnailUrl(IAsset asset);
  * void CreateAsset(string pathToFile) - Creates asset from local filename. So far, this is the only option we have with the current .NET API.
  * void Publish(string assetId) - tries to publish the asset (executing PUBLISH action). Not working on the WAMS so far - the WAMS is having issues publishing the asset
  * void AssignThumbnail(string assetId) - tries to assign thumbnail to an existing asset. Not working (not implemented) due to too much overhead complexity for implementing such a trivial feature
  * __[new]__ void DelteAsset(string assetId) - deletes an Asset. Of course by removing associated locators and content keys first

  ### IJobService ###
  Contract for managing Media Services Jobs.
  * IMediaService MediaService  - reference to the IMediaService
  * IEnumerable<IJob> Jobs - a list of all Jobs that are registered witht he given Media Services account
  * void CreateEncodeToSmoothStreamingJob(Asset asset, bool decrypt = false) - creates JOB with 2 parallel tasks. First task is to create Smooth Streaming asset. Second task is to Create thumbnails. Note that Smooth Streaming can only be produced of an MP4 asset! The decrypt parameter is used to identify whether to include a storage decryption immediately after converting.
  * void CreateNewJob(Asset asset, string mediaEncoder, string taskPreset) - creates single job with given media encoder name and task preset.
  * void DecryptAsset(Asset theAsset) - executes single task on decrypting a storage encrypted asset
  * void DeleteJob(string jobId) - deletes given job
  * __[new]__ void CancelJob(string jobId) - cancels a job
  * string GetPlayReadyTask(string contentKey = "", string keyId = "", string keySeed = "", string playReadyServerUrl = "") - gets a task preset for PlayReady protection of media

  ### ILocatorService ###
  Contract used to get streaming origins.
  * IMediaService MediaService  - reference to the IMediaService
  * string GetSmoothStreamingOriginLocator(Asset assetToStream) - gets smooth streaming origin locator
  * string GetMp4StreamingOriginLocator(Asset assetToStream) - gets progressive download locator with SAS signature
