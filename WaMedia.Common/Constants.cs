using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WaMedia.Common
{
    public class MediaEncoders
    {
        public const string WINDOWS_AZURE_MEDIA_ENCODER = "Windows Azure Media Encoder";
        public const string WINDOWS_AZURE_MEDIA_PACKAGER = "Windows Azure Media Packager";
        public const string WINDOWS_AZURE_MEDIA_ENCRYPTOR = "Windows Azure Media Encryptor";
        public const string STORAGE_DECRYPTION_ENCODER = "Storage Decryption";
        public const string PLAY_READY_ENCODER = "Windows Azure Media Encryptor";
        public const string SMOOTH_TO_HLS = "Smooth Streams to HLS Task";
    }

    public class Tasks
    {
        public const string H264_SD_4x3_CBR = "H264 Broadband SD 4x3";
        public const string H264_HD_720P_CBR = "H264 Broadband 720p";
        public const string H264_Smooth_720p_3G_4G = "H264 Smooth Streaming 720p for 3G or 4G";
        public const string VC1_SMOOTH_STREAMING_HD_720P = "VC1 Smooth Streaming 720p";
        public const string H264_ADAPTIVE_BITRATE_SD_16x9 = "H264 Adaptive Bitrate MP4 Set SD 16x9";
    }

    public class PlayReady
    {
        public const string DEV_SERVER_KEY_SEED = "XVBovsmzhP9gRIZxWfFta3VVRPzVEWmJsazEJ46I";
        public const string DEV_SERVER_LICENSE_URL = "http://playready.directtaps.net/pr/svc/rightsmanager.asmx?PlayRight=1&amp;UseSimpleNonPersistentLicense=1";
    }
}
