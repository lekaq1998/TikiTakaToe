using NRedisStack;
using NRedisStack.RedisStackCommands;
using StackExchange.Redis;

namespace TickTackToe.Hubs
{
    public class ApplicationDbContext
    {

        ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
        IDatabase database;
        public ApplicationDbContext()
        {
            redis = ConnectionMultiplexer.Connect("localhost");
            database = redis.GetDatabase();
        }

    }
}
