using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WaMedia.Common.Contracts
{
    public interface IFileService
    {
        IMediaService MediaService { get; }
    }
}
