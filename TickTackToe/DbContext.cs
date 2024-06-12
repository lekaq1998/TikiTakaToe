using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;

namespace TickTackToe
{
    public class DbContext : IDisposable
    {
  
            private readonly ConnectionMultiplexer _redis;
            private readonly IDatabase _database;

            public DbContext(string connectionString)
            {
                _redis = ConnectionMultiplexer.Connect(connectionString);
                _database = _redis.GetDatabase();
            }

            public void SaveObjects<T>(string key, List<T> objects)
            {
                var json = JsonConvert.SerializeObject(objects);
                _database.StringSet(key, json);
            }

            public void Set(string key, string value)
            {
                _database.StringSet(key, value);
            }

            public string Get(string key)
            {
                return _database.StringGet(key);
            }

            public void Dispose()
            {
                _redis.Dispose();
            }
        }
}
