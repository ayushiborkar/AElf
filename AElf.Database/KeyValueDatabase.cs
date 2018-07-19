﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AElf.Database
{
    public class KeyValueDatabase : IKeyValueDatabase
    {
        private readonly ConcurrentDictionary<string, byte[]> _dictionary = new ConcurrentDictionary<string, byte[]>();
        
        public Task<byte[]> GetAsync(string key, Type type)
        {
            return _dictionary.TryGetValue(key, out var value) ? Task.FromResult(value) : Task.FromResult<byte[]>(null);
        }

        public Task SetAsync(string key, byte[] bytes)
        {
            _dictionary[key] = bytes;
            return Task.CompletedTask;
        }

        public async Task<bool> PipelineSetAsync(IEnumerable<KeyValuePair<string, byte[]>> queue)
        {
            return await Task.Factory.StartNew(() =>
            {
                foreach (var update in queue)
                {
                    _dictionary[update.Key] = update.Value;
                }

                return true;
            });
        }

        public bool IsConnected()
        {
            return true;
        }

        public void ReSet(string host, int port)
        {
            throw new NotImplementedException();
        }
    }
}