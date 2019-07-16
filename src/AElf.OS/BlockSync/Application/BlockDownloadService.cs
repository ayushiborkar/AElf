using System;
using System.Linq;
using System.Threading.Tasks;
using AElf.Kernel;
using AElf.Kernel.Blockchain.Application;
using AElf.OS.BlockSync.Infrastructure;
using AElf.OS.BlockSync.Types;
using AElf.OS.Network.Application;
using AElf.Types;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AElf.OS.BlockSync.Application
{
    public class BlockDownloadService : IBlockDownloadService
    {
        private readonly IBlockchainService _blockchainService;
        private readonly INetworkService _networkService;
        private readonly IBlockSyncAttachService _blockSyncAttachService;
        private readonly IBlockSyncQueueService _blockSyncQueueService;
        private readonly IBlockSyncStateProvider _blockSyncStateProvider;

        public ILogger<BlockDownloadService> Logger { get; set; }

        public BlockDownloadService(IBlockchainService blockchainService,
            INetworkService networkService,
            IBlockSyncAttachService blockSyncAttachService,
            IBlockSyncQueueService blockSyncQueueService,
            IBlockSyncStateProvider blockSyncStateProvider)
        {
            Logger = NullLogger<BlockDownloadService>.Instance;

            _blockchainService = blockchainService;
            _networkService = networkService;
            _blockSyncAttachService = blockSyncAttachService;
            _blockSyncQueueService = blockSyncQueueService;
            _blockSyncStateProvider = blockSyncStateProvider;
        }

        public async Task<DownloadBlocksResult> DownloadBlocksAsync(Hash previousBlockHash, long previousBlockHeight,
            int batchRequestBlockCount, string suggestedPeerPubKey)
        {
            Logger.LogDebug(
                $"Trigger download blocks from peers, previous block height: {previousBlockHeight}, previous block hash: {previousBlockHash}");

            var downloadBlockCount = 0;
            var lastDownloadBlockHash = previousBlockHash;
            var lastDownloadBlockHeight = previousBlockHeight;

            while (true)
            {
                // Limit block sync job count, control memory usage
                var chain = await _blockchainService.GetChainAsync();
                if (chain.LongestChainHeight <= lastDownloadBlockHeight - BlockSyncConstants.BlockDownloadHeightOffset)
                {
                    Logger.LogWarning(
                        $"Pause sync task and wait for synced block to be processed, best chain height: {chain.BestChainHeight}");
                    break;
                }

                Logger.LogDebug($"Request blocks start with {lastDownloadBlockHash}");

                var blocksWithTransactions = await _networkService.GetBlocksAsync(lastDownloadBlockHash,
                    batchRequestBlockCount, suggestedPeerPubKey);

                if (blocksWithTransactions == null || !blocksWithTransactions.Any())
                {
                    Logger.LogWarning($"No blocks returned, current chain height: {chain.LongestChainHeight}.");
                    break;
                }

                if (blocksWithTransactions.First().Header.PreviousBlockHash != lastDownloadBlockHash)
                {
                    throw new InvalidOperationException(
                        $"Previous block not match previous {lastDownloadBlockHash}, network back {blocksWithTransactions.First().Header.PreviousBlockHash}");
                }

                foreach (var blockWithTransactions in blocksWithTransactions)
                {
                    Logger.LogDebug(
                        $"Processing block {blockWithTransactions},  longest chain hash: {chain.LongestChainHash}, best chain hash : {chain.BestChainHash}");

                    _blockSyncQueueService.Enqueue(
                        async () =>
                        {
                            await _blockSyncAttachService.AttachBlockWithTransactionsAsync(blockWithTransactions,
                                async (blockHash, blockHeight) =>
                                {
                                    _blockSyncStateProvider.DownloadJobTargetState.TryUpdate(blockHash, true, false);
                                });
                        },
                        OSConstants.BlockSyncAttachQueueName);

                    downloadBlockCount++;
                }

                var lastBlock = blocksWithTransactions.Last();
                lastDownloadBlockHash = lastBlock.GetHash();
                lastDownloadBlockHeight = lastBlock.Height;
            }

            return new DownloadBlocksResult
            {
                DownloadBlockCount = downloadBlockCount,
                LastDownloadBlockHash = lastDownloadBlockHash,
                LastDownloadBlockHeight = lastDownloadBlockHeight
            };
        }

        public bool ValidateQueueAvailability()
        {
            if (!_blockSyncQueueService.ValidateQueueAvailability(OSConstants.BlockSyncAttachQueueName))
            {
                Logger.LogWarning("Block sync attach queue is too busy.");
                return false;
            }

            if (!_blockSyncQueueService.ValidateQueueAvailability(KernelConstants.UpdateChainQueueName))
            {
                Logger.LogWarning("Block sync attach and execute queue is too busy.");
                return false;
            }

            return true;
        }
    }
}