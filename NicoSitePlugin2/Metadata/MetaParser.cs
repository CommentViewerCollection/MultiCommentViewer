using Newtonsoft.Json;

namespace NicoSitePlugin.Metadata
{
    static class MetaParser
    {
        public static IMetaMessage Parse(string raw)
        {
            dynamic d = JsonConvert.DeserializeObject(raw);
            IMetaMessage ret;
            var type = (string)d.type;
            switch (type)
            {
                case "serverTime":
                    ret = new ServerTime(raw);
                    break;
                case "ping":
                    ret = new Ping();
                    break;
                case "disconnect":
                    var reason = (string)d.data.reason;
                    ret = new Disconnect(reason);
                    break;
                case "room":
                    ret = new Room(raw);
                    break;
                case "seat":
                    ret = new Seat(raw);
                    break;
                case "statistics":
                    ret = new Statistics(raw);
                    break;
                case "stream":
                    ret = new IgnoredMessage(raw);
                    break;
                case "postCommentResult":
                    ret = new PostCommentResult(raw);
                    break;
                default:
                    //{"type":"stream","data":{"uri":"https://pb055f90f61.dmc.nico/hlslive/ht2_nicolive/nicolive-production-pg30617414664794_d7631f12f9ca9a54f470dc16195dc60bd751f0b45095978f77dd8d441d16bf93/master.m3u8?ht2_nicolive=2297426.gol5z4z91c_qnf4r5_158ksq0ka2uei","syncUri":"https://pb055f90f61.dmc.nico/hlslive/ht2_nicolive/nicolive-production-pg30617414664794_d7631f12f9ca9a54f470dc16195dc60bd751f0b45095978f77dd8d441d16bf93/stream_sync.json?ht2_nicolive=2297426.gol5z4z91c_qnf4r5_158ksq0ka2uei","quality":"abr","availableQualities":["abr","super_high","high","normal","low","super_low","audio_high"],"protocol":"hls"}}
                    //{"type":"room","data":{"name":"アリーナ","messageServer":{"uri":"wss://msgd.live2.nicovideo.jp/websocket","type":"niwavided"},"threadId":"M.F6z6aDgDo9ZTeIT7TjyEfA","yourPostKey":"T.Gxu4P9bDRWJZsQAecKlQ5EWcA-lKxVSVPef1pW7MP1fGb3btJP6xNwf_","isFirst":true,"waybackkey":"1611205291.N9TT1vQOpe73RBtnEx8hyK1unEg"}}
                    //{"type":"schedule","data":{"begin":"2021-01-24T11:18:31+09:00","end":"2021-01-24T12:18:31+09:00"}}
                    //{"type":"statistics","data":{"viewers":4941,"comments":9762,"adPoints":28800,"giftPoints":13100}}
                    //{"type":"error","data":{"code":"INVALID_MESSAGE"}}
                    //{"type":"tagUpdated","data":{"tags":{"items":[{"text":"旅部41","locked":true},{"text":"力也","locked":true,"nicopediaArticleUrl":"https://dic.nicovideo.jp/l/%E5%8A%9B%E4%B9%9F"},{"text":"ハニー大木","locked":true,"nicopediaArticleUrl":"https://dic.nicovideo.jp/l/%E3%83%8F%E3%83%8B%E3%83%BC%E5%A4%A7%E6%9C%A8"},{"text":"旅部","locked":true},{"text":"ワンピーススクラッチくじ","locked":true},{"text":"森義之","locked":false}],"ownerLocked":false}}}
                    ret = new UnknownMessage(raw);
                    break;
            }
            return ret;
        }
    }
}