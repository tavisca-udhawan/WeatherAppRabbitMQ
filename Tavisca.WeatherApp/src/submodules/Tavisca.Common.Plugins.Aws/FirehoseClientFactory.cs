using Amazon;
using Amazon.KinesisFirehose;
using Amazon.Runtime;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tavisca.Platform.Common;

namespace Tavisca.Common.Plugins.Aws
{
    internal class FirehoseClientFactory
    {
        private static readonly Dictionary<string, FirehoseClient> Cache = new Dictionary<string, FirehoseClient>(StringComparer.OrdinalIgnoreCase);
        private static readonly AsyncReadWriteLock Lock = new AsyncReadWriteLock();

        public FirehoseClient Create(FirehoseSettings settings)
        {
            FirehoseClient client = null;

            var waitHandle = new ManualResetEvent(false);
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    client = await CreateAsync(settings);
                }
                finally
                {
                    waitHandle.Set();
                }
            });

            waitHandle.WaitOne();
            return client;
        }

        public async Task<FirehoseClient> CreateAsync(FirehoseSettings settings)
        {
            using (await Lock.ReadLockAsync())
            {
                FirehoseClient client;
                if (Cache.TryGetValue(settings.Signature, out client) && client.IsValid())
                    return client;
            }

            using (await Lock.WriteLockAsync())
            {
                FirehoseClient client;
                if (Cache.TryGetValue(settings.Signature, out client) && client.IsValid())
                    return client;

                if(client != null && client.IsValid() == false)
                {
                    client.Dispose();
                    Cache.Remove(settings.Signature);
                }

                var newClient = await CreateNewAsync(settings);
                Cache[settings.Signature] = newClient;
                return newClient;
            }
        }

        private async Task<FirehoseClient> CreateNewAsync(FirehoseSettings settings)
        {
            return settings.HasRole
                ? await CreateNewWithRoleAsync(settings)
                : CreateNew(settings);
        }

        private FirehoseClient CreateNew(FirehoseSettings settings)
        {
            var region = RegionEndpoint.GetBySystemName(settings.Region);
            if (settings.HasKeys == true)
            {
                var credentials = new BasicAWSCredentials(settings.AccessKey, settings.SecretKey);
                return new FirehoseClient(new AmazonKinesisFirehoseClient(credentials, region));
            }
            else
            {
                return new FirehoseClient(new AmazonKinesisFirehoseClient(region));
            }
        }

        private async Task<FirehoseClient> CreateNewWithRoleAsync(FirehoseSettings settings)
        {
            using (var stsClient = new AmazonSecurityTokenServiceClient())
            {
                Credentials credentials = null;

                var assumeRoleRequest = new AssumeRoleRequest()
                {
                    RoleArn = settings.RoleArn,
                    RoleSessionName = settings.Stream,
                    DurationSeconds = settings.StsTokenAgeInMinutes * 60
                };

                var assumeRoleResponse = await stsClient.AssumeRoleAsync(assumeRoleRequest);
                credentials = assumeRoleResponse.Credentials;
                
                var region = RegionEndpoint.GetBySystemName(settings.Region);
                var client = new FirehoseClient(new AmazonKinesisFirehoseClient(credentials, region), DateTime.UtcNow.AddMinutes(settings.StsTokenAgeInMinutes));

                return client;
            }
        }        
    }
}