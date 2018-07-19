﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AElf.Database.Config;
using NServiceKit.Redis;

namespace AElf.Database
{
    public class SsdbDatabase : IKeyValueDatabase
    {
        private readonly PooledRedisClientManager _client;

        public SsdbDatabase()
        {
            _client = new PooledRedisClientManager($"{DatabaseConfig.Instance.Host}:{DatabaseConfig.Instance.Port}");
        }

        public async Task<byte[]> GetAsync(string key, Type type)
        {
            return await Task.FromResult(_client.GetCacheClient().Get<byte[]>(key));
        }

        public async Task SetAsync(string key, byte[] bytes)
        {
            await Task.FromResult(_client.GetCacheClient().Set(key, bytes));
        }

        public async Task<bool> PipelineSetAsync(IEnumerable<KeyValuePair<string, byte[]>> queue)
        {
            throw new NotImplementedException();
        }

        public bool IsConnected()
        {
            try
            {
                _client.GetCacheClient().Set<byte[]>("ping", null);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public void ReSet(string host, int port)
        {
            throw new NotImplementedException();
        }
    }
}