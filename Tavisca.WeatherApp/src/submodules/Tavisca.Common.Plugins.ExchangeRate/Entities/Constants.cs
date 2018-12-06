using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Common.Plugins.ExchangeRate.Entities
{
    public static class Constants
    {
        public const string ApplicationName = "exchange_rate";

        public const string SectionName = "periodic_rates";

        public const string NumberOfSlots = "number_of_slots";

        public const string DynamoDBServiceUrl = "dynamodb_service_url";

        public const string AWSAccessKey = "dynamodb_aws_api_key";

        public const string AWSSecretKey = "dynamodb-aws_secret_key";

        public const string OskiUserIp = "oski-user-ip";

        public const string UserIp = "192.168.4.14";

        public static class ExchangeRateService
        {
            public const string GetAllRates = "getrate/all";

        }

        public static class ConfigurationSections
        {
            public static readonly string ExternalServiceConfigurations = "external_service_configurations";
        }

        public static class TenantSettings
        {
            public static readonly string ExchangeRateService = "exchange_rate_service";

        }
    }

}
