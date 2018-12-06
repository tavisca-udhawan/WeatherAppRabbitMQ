using System;

namespace Tavisca.Common.Plugins.ServiceCaller
{
    public class CallerSettingsBuilder
    {
        private readonly Action<CallerSetting>[] _actions = new Action<CallerSetting>[7];
        public CallerSettingsBuilder WithClientSettings(ClientSetting clientSetting)
        {
            _actions[5] = (s =>
                           {
                               new ClientSettingsValidator().Validate(clientSetting);
                               s.ClientSetting = clientSetting;
                           });
            return this;
        }

        public CallerSettingsBuilder WithErrorPayloadType(Type errorPayloadType)
        {
            _actions[4] = (s =>
                           {
                               s.ErrorPayloadType = errorPayloadType;
                           });
            return this;
        }

        public CallerSettingsBuilder WithErrorPayloadTypeSelector(ErrorPayloadTypeSelector errorPayloadTypeSelector)
        {
            _actions[6] = (s =>
                           {
                               s.ErrorPayloadTypeSelector = errorPayloadTypeSelector;
                           });
            return this;
        }

        public CallerSettingsBuilder WithSerializerSettings(object serializerSettings)
        {
            _actions[3] = (s =>
                           {
                               s.SerializerSetting = serializerSettings;
                           });
            return this;
        }

        public CallerSettingsBuilder WithLogger(Logger logger)
        {
            _actions[2] = (s =>
                           {
                               s.Logger = logger;
                           });
            return this;
        }

        public CallerSettingsBuilder WithHttpClient(Client client)
        {
            _actions[1] = (s =>
                           {
                               s.Client = client;
                           });
            return this;
        }

        public CallerSettingsBuilder WithSerializer(Serializer serializer)
        {
            _actions[0] = (s =>
                           {
                               s.Serializer = serializer;
                           });
            return this;
        }

        public void Apply()
        {
            SetDefaults(WebCaller.Settings);
            foreach(var x in _actions)
            {
                x?.Invoke(WebCaller.Settings);
            }
        }

        public void Reset()
        {
            Array.Clear(_actions, 0, 6);
            WebCaller.Settings.Client = null;
            WebCaller.Settings.Logger = null;
            WebCaller.Settings.Serializer = null;
            WebCaller.Settings.ClientSetting = null;
            WebCaller.Settings.SerializerSetting = null;
            WebCaller.Settings.ErrorPayloadType = null;
            WebCaller.Settings.ErrorPayloadTypeSelector = null;
            SetDefaults(WebCaller.Settings);

        }

        private void SetDefaults(CallerSetting callerSettings)
        {
            if (callerSettings.Client == null)
                callerSettings.Client = DefaultCallerSettings.DefaultHttpClient;
            if (callerSettings.Serializer == null)
                callerSettings.Serializer = DefaultCallerSettings.DefaultSerializer;
            if (callerSettings.SerializerSetting == null)
                callerSettings.SerializerSetting = DefaultCallerSettings.DefaultSerializerSettings;
            if (callerSettings.ClientSetting == null)
                callerSettings.ClientSetting = DefaultCallerSettings.DefaultClientSetting;
        }
    }
}