using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Exceptions.PacketHandling;
using Oldsu.Bancho.GameLogic;
using Oldsu.Bancho.GameLogic.Events;
using Oldsu.Bancho.Objects;
using Oldsu.Bancho.Packet.Shared.Out;
using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class BeatmapInfoRequest : ISharedPacketIn
    {
        public string[] Filenames { get; set; }

        private Rankings RankingFromString(string? str) =>
            str switch
            {
                "XH" => Rankings.XH,
                "SH" => Rankings.SH,
                "X" => Rankings.X,
                "S" => Rankings.S,
                "A" => Rankings.A,
                "B" => Rankings.B,
                "C" => Rankings.C,
                "D" => Rankings.D,
                "F" => Rankings.F,
                "N" => Rankings.F,
                null => Rankings.N,
                
                _ => throw new ArgumentOutOfRangeException(nameof(str), str, null)
            };
        
        public void Handle(HubEventContext context)
        {
            if (Filenames.Length > 100)
                throw new RequestTooBigException();
            
            Task.Run(async () =>
            {
                await using var database = new Database();

                try
                {
                    var query = await database.Beatmaps.Include(b => b.Beatmapset)
                        .Select(beatmap => new {beatmap.Beatmapset.RankingStatus, beatmap.Filename, beatmap.BeatmapID, beatmap.BeatmapsetID, beatmap.BeatmapHash})
                        .Where(beatmap => Filenames.Contains(beatmap.Filename))
                        .ToArrayAsync(context.User!.CancellationToken);

                    BeatmapInfo[] beatmapInfos = new BeatmapInfo[query.Length]; // Four modes for each beatmap
                    
                    for (int i = 0; i < query.Length; i++)
                    {
                        var beatmap = query[i];

                        var gradeOsu =
                            RankingFromString(await database.HighScoresWithRank
                                .Where(score => score.BeatmapHash == beatmap.BeatmapHash 
                                                && score.UserId == context.User.UserID && score.Gamemode == 0)
                                .Select(score => score.Grade)
                                .FirstOrDefaultAsync(context.User.CancellationToken));

                        var gradeTaiko =
                            RankingFromString(await database.HighScoresWithRank
                                .Where(score => score.BeatmapHash == beatmap.BeatmapHash 
                                                && score.UserId == context.User.UserID && score.Gamemode == 1)
                                .Select(score => score.Grade)
                                .FirstOrDefaultAsync(context.User.CancellationToken));

                        var gradeCtb =
                            RankingFromString(await database.HighScoresWithRank
                                .Where(score => score.BeatmapHash == beatmap.BeatmapHash 
                                                && score.UserId == context.User.UserID && score.Gamemode == 2)
                                .Select(score => score.Grade)
                                .FirstOrDefaultAsync(context.User.CancellationToken));

                        var gradeMania =
                            RankingFromString(await database.HighScoresWithRank
                                .Where(score => score.BeatmapHash == beatmap.BeatmapHash 
                                                && score.UserId == context.User.UserID && score.Gamemode == 3)
                                .Select(score => score.Grade)
                                .FirstOrDefaultAsync(context.User.CancellationToken));

                        beatmapInfos[i] = new BeatmapInfo
                        {
                            Ranked = beatmap.RankingStatus == 1,
                            GradeOsu = (byte) gradeOsu,
                            GradeCatch = (byte) gradeCtb,
                            GradeTaiko = (byte) gradeTaiko,
                            GradeMania = (byte) gradeMania,
                            ID = (ushort) i,
                            MapHash = beatmap.BeatmapHash,
                            BeatmapID = beatmap.BeatmapID,
                            BeatmapsetID = beatmap.BeatmapsetID,
                            ThreadID = -1
                        };
                    }
                    
                    context.User.CancellationToken.ThrowIfCancellationRequested();
                    
                    context.User.SendPacket(new BeatmapInfoReply{BeatmapInfos = beatmapInfos});
                }
                catch (Exception exception)
                {
                    context.HubEventLoop.SendEvent(new HubEventAsyncError(exception, context.User));
                }
            });
        }
    }
}