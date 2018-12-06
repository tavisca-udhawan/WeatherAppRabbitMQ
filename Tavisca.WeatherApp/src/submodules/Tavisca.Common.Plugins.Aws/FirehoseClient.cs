using Amazon.KinesisFirehose;
using Amazon.KinesisFirehose.Model;
using System;
using System.Threading.Tasks;

namespace Tavisca.Common.Plugins.Aws
{
    public class FirehoseClient : IDisposable
    {
        private DateTime _validUntil;
        private AmazonKinesisFirehoseClient _firehose;        

        public FirehoseClient(AmazonKinesisFirehoseClient client)
        {
            _firehose = client;
            _validUntil = DateTime.MaxValue;
        }

        public FirehoseClient(AmazonKinesisFirehoseClient client, DateTime validUntil)
        {
            _firehose = client;
            _validUntil = validUntil;
        }

        private bool IsFaulted { get; set; } = false;        

        public async Task WriteAsync(byte[] data, string streamName)
        {
            var req = new PutRecordRequest
            {
                DeliveryStreamName = streamName,
                Record = new Record
                {
                    Data = ByteHelper.Compress(data)
                }
            };
            
            try
            {
                await _firehose.PutRecordAsync(req);
            }
            catch(Exception ex) when (ex is AmazonKinesisFirehoseException == false)
            {
                IsFaulted = true;
               
                //throw ex so that it can be exeuted by secondary sink(if any)
                throw;
            }
        }

        public void Dispose()
        {
            try
            {
                _firehose?.Dispose();
                _firehose = null;
            }
            catch
            {
                // ignored
            }
        }

        public bool IsValid()
        {
            if (IsFaulted)
                return false;

            //Client will be marked as invalid 5 mins before its expire time
            if (_validUntil.Subtract(DateTime.UtcNow).TotalMinutes <= 5)
                return false;

            return true;
        }
    }
}
