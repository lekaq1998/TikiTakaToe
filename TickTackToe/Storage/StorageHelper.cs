using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TickTackToe.Storage
{
    public static class StorageHelper
    {
        public static bool IsPlayerInTheClub (string clubNameX, string clubnameY, string playerName)
        {
            using (var dbContext = new DbContext("localhost:6379"))
            {
                string value = dbContext.Get("Clubs");
                var clubs = JsonConvert.DeserializeObject<List<Club>>(value);
                var clubX = clubs.FirstOrDefault(x => x.ClubName.Equals(clubNameX, StringComparison.OrdinalIgnoreCase));
                if(clubX == null) { return false; }
                var clubY = clubs.FirstOrDefault(x => x.ClubName.Equals(clubnameY, StringComparison.OrdinalIgnoreCase));
                if (clubY == null) { return false; }
                return (clubX.Players.Any(x=>x.Equals(playerName, StringComparison.OrdinalIgnoreCase)) && clubY.Players.Any(x => x.Equals(playerName, StringComparison.OrdinalIgnoreCase)));
            } 
        }
    }
}
