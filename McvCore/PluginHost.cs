using PluginV2 = Mcv.PluginV2;
using System;
using Mcv.PluginV2.Messages.ToCore;
using Mcv.PluginV2.Messages;

namespace McvCore
{
    class PluginHost : PluginV2.IPluginHost
    {
        private readonly McvCore _core;

        public void SetMessage(IMessageToCore message)
        {
            switch (message)
            {
                case Mcv.PluginV2.Messages.ToCore.RequestCloseApp _:
                    _core.RequestCloseApp();
                    break;



            }
        }

        public PluginV2.Messages.ToPlugin.IAnswerMessageToPlugin RequestMessage(IRequestMessageToCore message)
        {
            switch (message)
            {
                case Mcv.PluginV2.Messages.ToCore.RequestSettingsPanels reqPanels:
                    var panels = _core.GetSettingsPanels();
                    return new PluginV2.Messages.ToPlugin.AnswerSettingsPanels(panels);
            }
            throw new Exception("bug");
        }

        public IReplyMessageV2 RequestMessage(IRequestMessageV2 message)
        {
            switch (message)
            {
                case Mcv.PluginV2.Messages.RequestLoadPluginOptions reqPluginOptions:
                    {
                        var rawOptions = _core.LoadPluginOptionsRaw(reqPluginOptions.PluginName);
                        return new Mcv.PluginV2.Messages.ReplyPluginOptions(rawOptions);
                    }
                case Mcv.PluginV2.Messages.RequestLegacyOptions reqLegacyOptions:
                    {
                        var rawOptions = _core.LoadLegacyOptionsRaw();
                        return new Mcv.PluginV2.Messages.ReplyLegacyOptions(rawOptions);
                    }
                case Mcv.PluginV2.Messages.RequestConnectionStatus reqConnSt:
                    {
                        var connSt = _core.GetConnectionStatus(reqConnSt.ConnId);
                        return new Mcv.PluginV2.Messages.ReplyConnectionStatus(connSt);
                    }
                case Mcv.PluginV2.Messages.RequestAppName reqAppName:
                    {
                        var appName = _core.GetAppName();
                        return new Mcv.PluginV2.Messages.ReplyAppName(appName);
                    }
            }
            throw new Exception("bug");
        }

        public void SetMessage(ISetMessageToCoreV2 message)
        {
            switch (message)
            {
                case Mcv.PluginV2.Messages.RequestChangeConnectionStatus connStDiffMsg:
                    _core.ChangeConnectionStatus(connStDiffMsg.ConnStDiff);
                    break;
                case Mcv.PluginV2.Messages.RequestAddConnection _:
                    _core.AddConnection();
                    break;
                case Mcv.PluginV2.Messages.SetConnectionStatus setConnSt:
                    _core.ChangeConnectionStatus(setConnSt.ConnStDiff);
                    break;
                case Mcv.PluginV2.Messages.RequestShowSettingsPanel reqShowSettingsPanel:
                    _core.ShowPluginSettingsPanel(reqShowSettingsPanel.PluginId);
                    break;
                case Mcv.PluginV2.Messages.RequestRemoveConnection removeConn:
                    _core.RemoveConnection(removeConn.ConnId);
                    break;
                case Mcv.PluginV2.Messages.RequestSavePluginOptions savePluginOptions:
                    _core.SavePluginOptions(savePluginOptions.Filename, savePluginOptions.PluginOptionsRaw);
                    break;
            }
        }

        public PluginHost(McvCore core)
        {
            _core = core;
        }
    }

}
