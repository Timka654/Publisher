using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerPublisher.Shared.Models.RequestModels
{
    [NSLBIOType]
    public partial class PublishUploadFileBytesRequestModel
    {
        public Guid FileId { get; set; }

        public byte[] Bytes { get; set; }

        public bool EOF { get; set; }
    }
}
