﻿using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
//using AElf.Runtime.CSharp.Core.ABI;
using Google.Protobuf;
using AElf.Kernel;
using Type = System.Type;
using AElf.Common;
using AElf.Kernel.ABI;
using AElf.Kernel.Blockchain.Application;
using AElf.Kernel.SmartContract.Domain;
using AElf.Kernel;
using AElf.Kernel.Types;
using AElf.SmartContract.Contexts;
using AElf.Types.CSharp;
using Volo.Abp.DependencyInjection;

namespace AElf.SmartContract
{
    //TODO: remove _executivePools, _contractHashs, change ISingletonDependency to ITransientDependency
    public class SmartContractService : ISmartContractService, ISingletonDependency
    {
        private readonly ISmartContractManager _smartContractManager;
        private readonly ISmartContractRunnerContainer _smartContractRunnerContainer;

        private readonly ConcurrentDictionary<Hash, ConcurrentBag<IExecutive>> _executivePools =
            new ConcurrentDictionary<Hash, ConcurrentBag<IExecutive>>();

        private readonly IStateProviderFactory _stateProviderFactory;
        private readonly IFunctionMetadataService _functionMetadataService;
        private readonly IBlockchainService _chainService;

        public SmartContractService(ISmartContractManager smartContractManager,
            ISmartContractRunnerContainer smartContractRunnerContainer, IStateProviderFactory stateProviderFactory,
            IFunctionMetadataService functionMetadataService, IBlockchainService chainService)
        {
            _smartContractManager = smartContractManager;
            _smartContractRunnerContainer = smartContractRunnerContainer;
            _stateProviderFactory = stateProviderFactory;
            _functionMetadataService = functionMetadataService;
            _chainService = chainService;
        }

        public async Task DeployZeroContractAsync(int chainId, SmartContractRegistration registration)
        {
            registration.ContractHash = Hash.FromMessage(ContractHelpers.GetGenesisBasicContractAddress(chainId));

            await _smartContractManager.InsertAsync(registration);
        }



        /// <inheritdoc/>
        public async Task DeployContractAsync(int chainId, Address contractAddress,
            SmartContractRegistration registration, bool isPrivileged)
        {
            // get runner
            var runner = _smartContractRunnerContainer.GetRunner(registration.Category);
            runner.CodeCheck(registration.ContractBytes.ToByteArray(), isPrivileged);

            //Todo New version metadata handle it
//            var contractType = runner.GetContractType(registration);
//            var contractTemplate = runner.ExtractMetadata(contractType);
//            await _functionMetadataService.DeployContract(chainId, contractAddress, contractTemplate);

            await _smartContractManager.InsertAsync(registration);
        }

        public async Task UpdateContractAsync(int chainId, Address contractAddress,
            SmartContractRegistration newRegistration, bool isPrivileged)
        {
            // get runner
            var runner = _smartContractRunnerContainer.GetRunner(newRegistration.Category);
            runner.CodeCheck(newRegistration.ContractBytes.ToByteArray(), isPrivileged);

            //Todo New version metadata handle it
//            var oldRegistration = await GetContractByAddressAsync(chainId, contractAddress);
//            var oldContractType = runner.GetContractType(oldRegistration);
//            var oldContractTemplate = runner.ExtractMetadata(oldContractType);
//
//            var newContractType = runner.GetContractType(newRegistration);
//            var newContractTemplate = runner.ExtractMetadata(newContractType);
//            await _functionMetadataService.UpdateContract(chainId, contractAddress, newContractTemplate,
//                oldContractTemplate);

            await _smartContractManager.InsertAsync(newRegistration);
        }

//        public async Task<IMessage> GetAbiAsync(int chainId, Address account)
//        {
//            var reg = await GetContractByAddressAsync(chainId, account);
//            return GetAbiAsync(reg);
//        }

        /// <inheritdoc/>
//        public async Task<IEnumerable<string>> GetInvokingParams(Hash chainId, Transaction transaction)
//        {
//            var reg = await GetContractByAddressAsync(chainId, transaction.To);
//            var abi = (Module) GetAbiAsync(reg);
//            
//            // method info 
//            var methodInfo = GetContractType(reg).GetMethod(transaction.MethodName);
//            var parameters = ParamsPacker.Unpack(transaction.Params.ToByteArray(),
//                methodInfo.GetParameters().Select(y => y.ParameterType).ToArray());
//            // get method in abi
//            var method = abi.Methods.First(m => m.Name.Equals(transaction.MethodName));
//            
//            // deserialize
//            return method.DeserializeParams(parameters);
//        }
        
//        private IMessage GetAbiAsync(SmartContractRegistration reg)
//        {
//            var runner = _smartContractRunnerContainer.GetRunner(reg.Category);
//            return runner.GetAbi(reg);
//        }
//

//
//        
//        
//        private async Task<ConcurrentBag<IExecutive>> GetPoolForAsync(int chainId, Address account)
//        {
//            var contractHash = await GetContractHashAsync(chainId, account);
//            if (!_executivePools.TryGetValue(contractHash, out var pool))
//            {
//                pool = new ConcurrentBag<IExecutive>();
//                _executivePools[contractHash] = pool;
//            }
//
//            return pool;
//        }
//
//        private async Task<Hash> GetContractHashAsync(int chainId, Address address)
//        {
//            Hash contractHash;
//            var zeroContractAddress = ContractHelpers.GetGenesisBasicContractAddress(chainId);
//
//            if (address == zeroContractAddress)
//            {
//                contractHash = Hash.FromMessage(zeroContractAddress);
//            }
//            else
//            {
//                var result = await CallContractAsync(true, chainId, zeroContractAddress, "GetContractHash", address);
//
//                contractHash = result.DeserializeToPbMessage<Hash>();
//            }
//
//            return contractHash;
//        }

//        public async Task<SmartContractRegistration> GetContractByAddressAsync(int chainId, Address address)
//        {
//            var contractHash = await GetContractHashAsync(chainId, address);
//            return await _smartContractManager.GetAsync(contractHash);
//        }
//
//        public async Task<IExecutive> GetExecutiveAsync(Address contractAddress, int chainId)
//        {
//            var pool = await GetPoolForAsync(chainId, contractAddress);
//            if (!pool.TryTake(out var executive))
//            {
//                // get registration
//                var reg = await GetContractByAddressAsync(chainId, contractAddress);
//                // get runner
//                var runner = _smartContractRunnerContainer.GetRunner(reg.Category);
//                if (runner == null)
//                {
//                    throw new NotSupportedException($"Runner for category {reg.Category} is not registered.");
//                }
//                // run smartcontract executive info and return executive
//                executive = await runner.RunAsync(reg);
//                executive.ContractHash = reg.ContractHash;
//            }
//            executive.SetStateProviderFactory(_stateProviderFactory);
//
//            executive.SetSmartContractContext(new SmartContractContext()
//            {
//                ChainId = chainId,
//                ContractAddress = contractAddress,
//                SmartContractService = this,
//                ChainService = _chainService
//            });
//
//            return executive;
//        }
//
//        public async Task PutExecutiveAsync(int chainId, Address account, IExecutive executive)
//        {
//            executive.SetTransactionContext(new TransactionContext()
//            {
//                Transaction = new Transaction()
//                {
//                    To = account // This is to ensure that the contract has same address
//                }
//            });
//            executive.SetDataCache(new Dictionary<StatePath, StateCache>());
//            (await GetPoolForAsync(chainId, account)).Add(executive);
//
//            await Task.CompletedTask;
//        }
//        private Type GetContractType(SmartContractRegistration registration)
//        {
//            var runner = _smartContractRunnerContainer.GetRunner(registration.Category);
//            if (runner == null)
//            {
//                throw new NotSupportedException($"Runner for category {registration.Category} is not registered.");
//            }
//
//            return runner.GetContractType(registration);
//        }

    }
}