using System;
using System.Net;
using Amazon;
using Amazon.Runtime;
using Amazon.S3.Model;
using System.Threading.Tasks;
using Tavisca.Platform.Common.FileStore;
using Amazon.S3;
using System.IO;
using Tavisca.Platform.Common.Profiling;
using System.Threading;
using System.Collections.Specialized;
using Tavisca.Platform.Common.Configurations;

namespace Tavisca.Common.Plugins.Aws.S3
{
    public class S3Store : IFileStore
    {
        private readonly IAmazonS3 _client;

        public S3Store(IAmazonS3 amazonS3Client)
        {
            _client = amazonS3Client;
        }
        public S3Store(IConfigurationProvider configurationProvider)
        {
            _client = CreateClient(GetSettings(configurationProvider));
        }


        private S3Settings GetSettings(IConfigurationProvider configurationProvider)
        {
            var nvc = configurationProvider.GetGlobalConfigurationAsNameValueCollection("file_storage", "s3_settings");

            return S3Settings.Load(nvc ?? new NameValueCollection());
        }

        private IAmazonS3 CreateClient(S3Settings settings)
        {
            var region = RegionEndpoint.GetBySystemName(settings.Region);
            if (settings.HasKeys)
            {
                var credentials = new BasicAWSCredentials(settings.AccessKey, settings.SecretKey);
                return new AmazonS3Client(credentials, region);
            }
            else
            {
                return new AmazonS3Client(region);
            }
        }

        public async Task AddAsync(string key, string path, byte[] value)
        {
            try
            {
                using (var scope = new ProfileContext($"Add data bytes to S3"))
                {
                    var request = GetPutObjectRequest(key, path, value);

                    var response = await _client.PutObjectAsync(request);

                    if (response.HttpStatusCode != HttpStatusCode.OK)
                        throw Errors.ServerSide.S3CommunicationError();

                    return;
                }
            }
            catch (Exception ex)
            {
                Platform.Common.ExceptionPolicy.HandleException(ex, Constants.LogOnlyPolicy);
            }
            throw Errors.ServerSide.S3CommunicationError();
        }

        public async Task<byte[]> GetAsync(string key, string path)
        {
            try
            {
                using (var scope = new ProfileContext($"Get data bytes to S3"))
                {
                    var request = GetObjectRequest(key, path);

                    var response = await _client.GetObjectAsync(request);

                    if (response.HttpStatusCode != HttpStatusCode.OK)
                        throw Errors.ServerSide.S3CommunicationError();

                    return ParseResponse(response);
                }
            }
            catch (AmazonS3Exception s3Exception)
                when (s3Exception.StatusCode.Equals(HttpStatusCode.NotFound))
            {
                Platform.Common.ExceptionPolicy.HandleException(s3Exception, Constants.LogOnlyPolicy);
                return null;
            }
            catch (Exception ex)
            {
                Platform.Common.ExceptionPolicy.HandleException(ex, Constants.LogOnlyPolicy);
            }
            throw Errors.ServerSide.S3CommunicationError();
        }

        public void Add(string key, string path, byte[] value)
        {

            using (var scope = new ProfileContext($"Add data bytes to S3"))
            {
                var request = GetPutObjectRequest(key, path, value);
                PutObjectResponse response = null;
                var waitHandle = new ManualResetEvent(false);

                Task.Factory.StartNew(async () =>
                {
                    try
                    {
                        response = await _client.PutObjectAsync(request);
                    }
                    catch (Exception ex)
                    {
                        Platform.Common.ExceptionPolicy.HandleException(ex, Constants.LogOnlyPolicy);
                        throw Errors.ServerSide.S3CommunicationError();
                    }
                    finally
                    {
                        waitHandle.Set();
                    }
                    
                });
                waitHandle.WaitOne();

                if (response.HttpStatusCode != HttpStatusCode.OK)
                    throw Errors.ServerSide.S3CommunicationError();

                return;
            }
        }

        public byte[] Get(string key, string path)
        {
            using (var scope = new ProfileContext($"Get data bytes to S3"))
            {
                var request = GetObjectRequest(key, path);

                GetObjectResponse response = null;
                var waitHandle = new ManualResetEvent(false);

                Task.Factory.StartNew(async () =>
                {
                    try
                    {
                        response = await _client.GetObjectAsync(request);
                    }
                    catch (AmazonS3Exception s3Exception)
            when (s3Exception.StatusCode.Equals(HttpStatusCode.NotFound))
                    {
                        Platform.Common.ExceptionPolicy.HandleException(s3Exception, Constants.LogOnlyPolicy);
                        response = null;
                    }
                    catch (Exception ex)
                    {
                        Platform.Common.ExceptionPolicy.HandleException(ex, Constants.LogOnlyPolicy);
                        throw Errors.ServerSide.S3CommunicationError();
                    }
                    finally
                    {
                        waitHandle.Set();
                    }

                });
                waitHandle.WaitOne();

                if (response == null)
                    return null;

                if (response.HttpStatusCode != HttpStatusCode.OK)
                    throw Errors.ServerSide.S3CommunicationError();

                return ParseResponse(response);
            }

        }

        public void Dispose()
        {
            try
            {
                _client?.Dispose();
            }
            catch
            {
                // ignored
            }
        }

        private byte[] ParseResponse(GetObjectResponse response)
        {
            if (response == null || response.ResponseStream == null)
                return null;

            using (var memoryStream = new MemoryStream())
            {
                response.ResponseStream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        private static GetObjectRequest GetObjectRequest(string key, string path)
        {
            return new GetObjectRequest
            {
                BucketName = path,
                Key = key
            };
        }

        private static PutObjectRequest GetPutObjectRequest(string key, string path, byte[] value)
        {
            return new PutObjectRequest
            {
                BucketName = path,
                Key = key,
                InputStream = new MemoryStream(value)
            };
        }
    }
}
