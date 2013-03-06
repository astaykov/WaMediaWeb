﻿using System;
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
        public const string H264_HD_720P_VBR = "H.264 HD 720p VBR";
        public const string H264_512k_DSL_CBR = "H.264 512k DSL CBR";
        public const string VC1_SMOOTH_STREAMING_HD_720P = "VC1 Smooth Streaming 720p";
    }

    public class PlayReady
    {
        public const string DEV_SERVER_KEY_SEED = "XVBovsmzhP9gRIZxWfFta3VVRPzVEWmJsazEJ46I";
        public const string DEV_SERVER_LICENSE_URL = "http://playready.directtaps.net/pr/svc/rightsmanager.asmx?PlayRight=1&amp;UseSimpleNonPersistentLicense=1";
    }
}
