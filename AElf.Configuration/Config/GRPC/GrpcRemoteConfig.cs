using System;
using System.Collections.Generic;

namespace AElf.Configuration.Config.GRPC
{
    [ConfigFile(FileName = "grpcremote.json")]
    public class GrpcRemoteConfig : ConfigBase<GrpcRemoteConfig>
    {
        public Dictionary<string, Uri> ParentChain { get; set; }
        public Dictionary<string, Uri> ChildChains { get; set; }

        public GrpcRemoteConfig()
        {
            ConfigChanged+= OnGrpcRemoteConfigChanged;
        }

        private void OnGrpcRemoteConfigChanged(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
    
    public class Uri
    {
        public string Address { get; set; }
        public int Port { get; set; }
    }
}    