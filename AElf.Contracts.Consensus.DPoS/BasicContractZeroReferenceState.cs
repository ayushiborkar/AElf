using System;
using AElf.Common;
using AElf.Sdk.CSharp.State;

namespace AElf.Contracts.Consensus.DPoS
{
    public class BasicContractZeroReferenceState : ContractReferenceState
    {
        public Func<Hash, Address> GetContractAddressByName { get; set; }
    }
}