using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.GameLogic;
using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class BeatmapInfoRequest : ISharedPacketIn
    {
        public List<string> Filenames { get; set; }

        public void Handle(HubEventContext context)
        {
            
        }
    }
}