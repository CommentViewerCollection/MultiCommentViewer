using Codeplex.Data;
using System.Text.RegularExpressions;

namespace PeriscopeSitePlugin
{
    internal static class MessageParser
    {
        /// <summary>
        /// websocketでサーバから送られてきた生データをIWebsocketMessageに変換する
        /// </summary>
        /// <param name="websocketMessageRaw"> websocketでサーバから送られてきた生データ</param>
        /// <returns></returns>
        public static IWebsocketMessage ParseWebsocketMessage(string websocketMessageRaw)
        {
            var raw = websocketMessageRaw;
            var d = DynamicJson.Parse(raw);
            return new WebsocketMessage
            {
                Kind = (int)d.kind,
                Payload = d.payload.ToString(),
                Raw = raw,
            };
        }
        static readonly Regex regex = new Regex("\\\\\"type\\\\\":(\\d+)", RegexOptions.Compiled);
        private static int GetType(string payload)
        {
            var match = regex.Match(payload);
            return int.Parse(match.Groups[1].Value);
        }
        public static IInternalMessage Parse(IWebsocketMessage websocketMessage)
        {
            IInternalMessage message = null;
            var payload = websocketMessage.Payload;
            var raw = websocketMessage.Raw;
            switch (websocketMessage.Kind)
            {
                case 1:
                    {
                        //{"room":"1lPKqorLvldJb","body":"{\"displayName\":\"Bill\",\"ntpForBroadcasterFrame\":16166207731395686400,\"ntpForLiveFrame\":16166207721358794903,\"participant_index\":824787836,\"remoteID\":\"1476841\",\"timestamp\":1554999751502,\"type\":2,\"uuid\":\"3A6AE282-D227-46BF-AFE6-6BB1CD8E8C47\",\"v\":2}","lang":"","sender":{"user_id":"1476841","participant_index":824787836,"twitter_id":"2160881996"},"timestamp":1554999751631707918}

                        //2019/04/12
                        //type1とtype2はどちらもkind1だけど、payloadの要素には若干の違いがある
                        //type1の要素はroom,body,lang,sender,timestamp,uuid
                        //type2の要素はroom,body,lang,sender,timestamp
                        //またsenderの要素にも違いがある
                        //type1:user_id,username,display_name,profile_image_url,participant_index,locale,twitter_id,lang_superfan
                        //type2:user_id,particiant_index
                        switch (GetType(payload))
                        {
                            case 1://CHAT
                                {
                                    var kind1payloadtype1Low = Tools.Deserialize<Low.kind1payloadtype1.RootObject>(payload);
                                    Kind1Type1 kind1type1;
                                    if (!kind1payloadtype1Low.Body.Contains("eggmojiOverride"))
                                    {
                                        //payload
                                        //
                                        //body
                                        //{"body":"Yeah","displayName":"PanAm Style","ntpForBroadcasterFrame":16166206334443356160,"ntpForLiveFrame":16166206323526947868,"participant_index":1043054637,"remoteID":"1WgKgapJvplEv","timestamp":1554999426288,"type":1,"username":"PanamStyle","uuid":"BDDDFC39-FDE0-42D7-8977-D3945E8EC783","v":2}
                                        var kind1type1Low = Tools.Deserialize<Low.kind1type1.RootObject>(kind1payloadtype1Low.Body);
                                        kind1type1 = new Kind1Type1(kind1type1Low, kind1payloadtype1Low.Sender, raw);
                                    }
                                    else
                                    {
                                        var kind1type1_newtypeLow = Tools.Deserialize<Low.kind1type1_newtype.RootObject>(kind1payloadtype1Low.Body);
                                        kind1type1 = new Kind1Type1(kind1payloadtype1Low, kind1type1_newtypeLow, kind1payloadtype1Low.Sender, raw);
                                    }
                                    message = kind1type1;
                                }
                                break;
                            case 2://HEART
                                {
                                    //{"displayName":"Bill","ntpForBroadcasterFrame":16166207731395686400,"ntpForLiveFrame":16166207721358794903,"participant_index":824787836,"remoteID":"1476841","timestamp":1554999751502,"type":2,"uuid":"3A6AE282-D227-46BF-AFE6-6BB1CD8E8C47","v":2}
                                    var kind1payloadtype2Low = Tools.Deserialize<Low.kind1payloadtype2.RootObject>(payload);
                                    var kind1type2Low = Tools.Deserialize<Low.kind1type2.RootObject>(kind1payloadtype2Low.Body);
                                    var kind1type2 = new Kind1Type2(kind1type2Low, raw);
                                    message = kind1type2;
                                }
                                break;
                            case 3://JOIN
                                {
                                    //{"kind":1,"payload":"{\"room\":\"1BRKjvpBpzRKw\",\"body\":\"{\\\"body\\\":\\\"joined\\\",\\\"displayName\\\":\\\"Zack Harris\\\",\\\"ntpForBroadcasterFrame\\\":16166278715298549760,\\\"ntpForLiveFrame\\\":16166278617955100672,\\\"participant_index\\\":1313653130,\\\"programDateTime\\\":\\\"2019-04-11T20:57:36.125+0000\\\",\\\"remoteID\\\":\\\"tw-972105987996966913\\\",\\\"timestamp\\\":1555016278753,\\\"type\\\":3,\\\"uuid\\\":\\\"44511935-1FED-4575-84AE-E48091C6F532\\\",\\\"v\\\":2}\",\"lang\":\"\",\"sender\":{\"user_id\":\"tw-972105987996966913\",\"username\":\"OverripeStraw\",\"display_name\":\"Zack Harris\",\"profile_image_url\":\"https://pbs.twimg.com/profile_images/1075132685079597056/RkfK1LaQ_reasonably_small.jpg\",\"participant_index\":1313653130,\"locale\":\"en\",\"twitter_id\":\"972105987996966913\",\"lang\":[\"en\"]},\"timestamp\":1555016278827194743,\"uuid\":\"44511935-1FED-4575-84AE-E48091C6F532\"}","signature":"3URgAe5-4_Z2B9c2aQztcg2q2xSSaAcSntX3Bfw"}
                                    //{"body":"joined","displayName":"-'Charisma❤","ntpForBroadcasterFrame":0,"ntpForLiveFrame":0,"participant_index":2138289218,"programDateTime":"2019-04-11T21:05:16.541+0000","remoteID":"1ayQVvRAdxqQp","timestamp":1555016739928,"type":3,"username":"charismaao18","uuid":"457D8A32-EF08-4595-972D-DB4A5B37F86B","v":2}
                                }
                                break;
                            case 4://LOCATION
                                {
                                    //{"kind":1,"payload":"{\"room\":\"1mnxevLRDzoKX\",\"body\":\"{\\\"heading\\\":-1,\\\"lat\\\":35.694,\\\"lng\\\":139.703,\\\"ntpForBroadcasterFrame\\\":0,\\\"ntpForLiveFrame\\\":16166293121886601216,\\\"timestamp\\\":1555019633078,\\\"type\\\":4,\\\"uuid\\\":\\\"0C5546F6-E008-4155-9908-A6FD4970826A\\\",\\\"v\\\":2}\",\"lang\":\"\",\"sender\":{\"user_id\":\"1DYKXPPbJpKgL\",\"username\":\"KappaHiroki\",\"display_name\":\"Kappa Hiroki🐸🇯🇵\",\"profile_image_url\":\"https://prod-profile.pscp.tv/1DYKXPPbJpKgL/0e52c61b1e3c023b483fc418fa9bb681-128.jpg\",\"locale\":\"de\",\"vip\":\"gold\",\"twitter_id\":\"627018079\",\"lang\":[\"de\",\"en\",\"ru\",\"ja\",\"es\"]},\"timestamp\":1555019633125755723,\"uuid\":\"0C5546F6-E008-4155-9908-A6FD4970826A\"}","signature":"3M4wB8fq2iX6coToMo-TWX7xNqWuf4bCXBj3lgg"}
                                }
                                break;
                            case 5://BROADCAST_ENDED
                                {
                                    //{"kind":1,"payload":"{\"room\":\"1OdKrvPzEgqKX\",\"body\":\"{\\\"displayName\\\":\\\"Kerry\\\",\\\"ntpForBroadcasterFrame\\\":16166287562458474496,\\\"ntpForLiveFrame\\\":16166287562458396672,\\\"participant_index\\\":0,\\\"remoteID\\\":\\\"11390212\\\",\\\"timestamp\\\":1555018338820,\\\"type\\\":5,\\\"uuid\\\":\\\"DDA02FA4-DB96-475C-B42B-FB912798DBC7\\\",\\\"v\\\":2}\",\"lang\":\"\",\"sender\":{\"user_id\":\"11390212\",\"username\":\"KerryVernonSing\",\"display_name\":\"Kerry\",\"profile_image_url\":\"https://prod-profile.pscp.tv/11390212/3c9ee863eb08c0dece678411a9e1a8ba-128.jpg\",\"locale\":\"en\",\"twitter_id\":\"1051809474\",\"lang\":[\"en\",\"ru\",\"es\"]},\"timestamp\":1555018338851795839,\"uuid\":\"DDA02FA4-DB96-475C-B42B-FB912798DBC7\"}","signature":"3h1BZgC_vYdOaq289zENoUccLIZyL1r34aCXW2A"}
                                    //{"kind":1,"payload":"{\"room\":\"1LyxBAVlPErJN\",\"body\":\"{\\\"ntpForBroadcasterFrame\\\":16171701947848055586,\\\"ntpForLiveFrame\\\":16171701947848055586,\\\"timestamp\\\":1556278972,\\\"type\\\":5,\\\"uuid\\\":\\\"1f73705f-98dd-4811-be73-162681fa5215\\\",\\\"v\\\":2}\",\"lang\":\"\",\"sender\":{\"user_id\":\"10098802\",\"username\":\"walthollick\",\"display_name\":\"Walt \\u0026 Snoop\",\"profile_image_url\":\"https://prod-profile.pscp.tv/10098802/5e40c1597980e23c3ef523b25b909485-128.jpg\",\"locale\":\"en\",\"vip\":\"gold\",\"twitter_id\":\"340547450\",\"lang\":[\"en\"]},\"timestamp\":1556278973101810023,\"uuid\":\"1f73705f-98dd-4811-be73-162681fa5215\"}","signature":"3wSUd1dDpVVkm_iMjCKSD9ZN2OH97sssJZwuLpg"}
                                }
                                break;
                            case 6:
                                {
                                    //{"kind":1,"payload":"{\"room\":\"1vOxwqbdgNvGB\",\"body\":\"{\\\"body\\\":\\\"*%s* invited followers\\\",\\\"displayName\\\":\\\"#OwnVoices Curator Janice Temple 🎙️\\\",\\\"initials\\\":\\\"\\\",\\\"invited_count\\\":0,\\\"ntpForBroadcasterFrame\\\":16166541913558127476,\\\"ntpForLiveFrame\\\":16166541822689504395,\\\"participant_index\\\":1131066516,\\\"timestamp\\\":1555077559,\\\"type\\\":6,\\\"user_id\\\":\\\"12878263\\\",\\\"username\\\":\\\"JaniceTemple\\\",\\\"uuid\\\":\\\"282a8545-72c2-451c-a517-ec4772c2b47f\\\",\\\"v\\\":2}\",\"lang\":\"\",\"sender\":{\"user_id\":\"12878263\",\"username\":\"JaniceTemple\",\"display_name\":\"#OwnVoices Curator Janice Temple 🎙️\",\"profile_image_url\":\"https://prod-profile.pscp.tv/12878263/466c46e33839a84d85c9fac4cac8fe8e-128.jpg\",\"participant_index\":1131066516,\"locale\":\"en\",\"vip\":\"gold\",\"twitter_id\":\"43377345\",\"lang\":[\"en\"],\"superfan\":true},\"timestamp\":1555077560289135080,\"uuid\":\"282a8545-72c2-451c-a517-ec4772c2b47f\"}","signature":"3H7euavYgpZyM-0jnkTOW_hwynvvjTB2M3l0A0w"}
                                }
                                break;
                            case 9:
                                {
                                    //{"kind":1,"payload":"{\"room\":\"1nAKEzrkZBOGL\",\"body\":\"{\\\"ntpForBroadcasterFrame\\\":0,\\\"ntpForLiveFrame\\\":0,\\\"timestamp\\\":1564853368785,\\\"timestampPlaybackOffset\\\":6.619704008102417,\\\"type\\\":9,\\\"uuid\\\":\\\"3ECA53AE-C00C-4430-A5B7-0F6F4E3B6C58\\\",\\\"v\\\":2}\",\"lang\":\"\",\"sender\":{\"user_id\":\"1PXEdBqYmmoKe\",\"username\":\"moecheru0221\",\"display_name\":\"moe🐣\",\"profile_image_url\":\"https://prod-profile.pscp.tv/1PXEdBqYmmoKe/eb2fa0bbcad5744a22939b3a3c233062-128.jpg\",\"locale\":\"en\",\"lang\":[\"en\",\"ja\"]},\"timestamp\":1564853368791216016,\"uuid\":\"3ECA53AE-C00C-4430-A5B7-0F6F4E3B6C58\"}","signature":"3xY5t2FvJv8C4j6OLBm8uLNXSisWYXmBYLmKgaw"}
                                }
                                break;
                            case 12:
                                {
                                    //{"kind":1,"payload":"{\"room\":\"1yoJMEQYAjWJQ\",\"body\":\"{\\\"broadcasterBlockedMessageBody\\\":\\\"tu stop la ton dessin ok !!!!!!!!\\\",\\\"broadcasterBlockedRemoteID\\\":\\\"1wBKAYWNBaXKP\\\",\\\"broadcasterBlockedUsername\\\":\\\"OXYS_1\\\",\\\"ntpForBroadcasterFrame\\\":16171680149849646301,\\\"ntpForLiveFrame\\\":16171680149849646301,\\\"timestamp\\\":1556273897,\\\"type\\\":12,\\\"uuid\\\":\\\"9fd0d2fd-11e5-4f81-92df-da79c4f41b00\\\",\\\"v\\\":2}\",\"lang\":\"\",\"sender\":{\"user_id\":\"1XJQkVDmmgVEL\",\"username\":\"Life660\",\"display_name\":\"Life660\",\"profile_image_url\":\"https://prod-profile.pscp.tv/1XJQkVDmmgVEL/9b7f10cc4f8b7f1e8319ca578f116151-128.jpg\",\"locale\":\"fr\",\"vip\":\"gold\",\"twitter_id\":\"1041748771938426881\",\"lang\":[\"fr\"]},\"timestamp\":1556273898394781529,\"uuid\":\"9fd0d2fd-11e5-4f81-92df-da79c4f41b00\"}","signature":"3rp4y2rXulwSOAx933p6pjAGPJO6PbNyE3ESskA"}
                                }
                                break;
                            case 13:
                                {
                                    //{"kind":1,"payload":"{\"room\":\"1OdKrvPzEgqKX\",\"body\":\"{\\\"ntpForBroadcasterFrame\\\":16166287087707176960,\\\"ntpForLiveFrame\\\":16166287076027675370,\\\"participant_index\\\":477826440,\\\"remoteID\\\":\\\"4163380\\\",\\\"timestamp\\\":1555018228129,\\\"type\\\":13,\\\"username\\\":\\\"lizzibear\\\",\\\"uuid\\\":\\\"6038F834-7A44-43B5-8F1C-B6F2B5F4C634\\\",\\\"v\\\":2}\",\"lang\":\"\",\"sender\":{\"user_id\":\"4163380\",\"username\":\"lizzibear\",\"display_name\":\"Lizzibear\",\"profile_image_url\":\"https://prod-profile.pscp.tv/4163380/14b209888fa2706053120623824a02d3-128.jpg\",\"participant_index\":477826440,\"locale\":\"en\",\"vip\":\"gold\",\"twitter_id\":\"18548750\",\"lang\":[\"en\"],\"superfan\":true},\"timestamp\":1555018228194745123,\"uuid\":\"6038F834-7A44-43B5-8F1C-B6F2B5F4C634\"}","signature":"3YFBZ-PesDtezJyKnDHsEVIy9_wBMyu8sTDsntA"}
                                }
                                break;
                            case 16:
                                {
                                    //{"kind":1,"payload":"{\"room\":\"1mnGevLazEQJX\",\"body\":\"{\\\"displayName\\\":\\\"Enjolia Nichole\\\",\\\"ntpForBroadcasterFrame\\\":16166285818055839744,\\\"ntpForLiveFrame\\\":16166285734811643904,\\\"participant_index\\\":318870136,\\\"programDateTime\\\":\\\"2019-04-11T21:25:13.094+0000\\\",\\\"remoteID\\\":\\\"1eWEyMVerwzEA\\\",\\\"timestamp\\\":1555017932509,\\\"type\\\":16,\\\"uuid\\\":\\\"AD2673B2-5E37-4811-995C-2A76F24E34A2\\\",\\\"v\\\":2}\",\"lang\":\"\",\"sender\":{\"user_id\":\"1eWEyMVerwzEA\",\"username\":\"EnjoliaNichole1\",\"display_name\":\"Enjolia Nichole\",\"profile_image_url\":\"https://pbs.twimg.com/profile_images/1060897780036706304/4yzGCJkz_reasonably_small.jpg\",\"participant_index\":318870136,\"locale\":\"en\",\"twitter_id\":\"349447529\",\"lang\":[\"en\"]},\"timestamp\":1555017932526950908,\"uuid\":\"AD2673B2-5E37-4811-995C-2A76F24E34A2\"}","signature":"3Jr-NSq3khazCFjAwqMDD1ktLP4cgr-C4V-8lfA"}
                                }
                                break;
                            case 33:
                                {
                                    //{"kind":1,"payload":"{\"room\":\"1yoJMEQYAjWJQ\",\"body\":\"{\\\"body\\\":\\\"Suivez *Life660* pour ne pas manquer sa prochaine diffusion.\\\",\\\"ntpForBroadcasterFrame\\\":16171678293697924956,\\\"ntpForLiveFrame\\\":16171678293697924956,\\\"timestamp\\\":1556273465,\\\"type\\\":33,\\\"user_id\\\":\\\"1XJQkVDmmgVEL\\\",\\\"uuid\\\":\\\"cf4c92c1-6862-4916-bd0b-e57e7f3aa8a2\\\",\\\"v\\\":2}\",\"lang\":\"\",\"sender\":{\"user_id\":\"1XJQkVDmmgVEL\",\"username\":\"Life660\",\"display_name\":\"Life660\",\"profile_image_url\":\"https://prod-profile.pscp.tv/1XJQkVDmmgVEL/9b7f10cc4f8b7f1e8319ca578f116151-128.jpg\",\"locale\":\"fr\",\"vip\":\"gold\",\"twitter_id\":\"1041748771938426881\",\"lang\":[\"fr\"]},\"timestamp\":1556273466227124737,\"uuid\":\"cf4c92c1-6862-4916-bd0b-e57e7f3aa8a2\"}","signature":"3nUfHLZvaNmo8Pu-Wq5FiBpx6wYtqbAj-92SG0w"}
                                }
                                break;
                            case 36:
                                {
                                    //{"kind":1,"payload":"{\"room\":\"1OdKrvPzEgqKX\",\"body\":\"{\\\"gift_amount\\\":33,\\\"gift_style\\\":\\\"sparkle\\\",\\\"gift_tier\\\":0,\\\"ntpForBroadcasterFrame\\\":16166286632815544320,\\\"ntpForLiveFrame\\\":16166286621071766960,\\\"remoteID\\\":\\\"1mMEPOgnbJNjG\\\",\\\"sparkle_id\\\":\\\"gift_id_1\\\",\\\"timestamp\\\":1555018122,\\\"type\\\":36,\\\"v\\\":2}\",\"lang\":\"\",\"sender\":{\"user_id\":\"1mMEPOgnbJNjG\",\"username\":\"delof2\",\"display_name\":\"Josher\",\"profile_image_url\":\"https://prod-profile.pscp.tv/1mMEPOgnbJNjG/89d713a75028c4360f16b18507b711e6-128.jpg\",\"participant_index\":113887880},\"timestamp\":1555018122294465299}","signature":"3Pfeha-I8gKU59u0_jDKpZPzFk1dfNS12xKyCkw"}
                                }
                                break;
                            case 37:
                                {
                                    //{"kind":1,"payload":"{\"room\":\"1OdKrvPzEgqKX\",\"body\":\"{\\\"gift_amount\\\":33,\\\"gift_style\\\":\\\"sparkle\\\",\\\"gift_tier\\\":0,\\\"ntpForBroadcasterFrame\\\":16166286631458054144,\\\"ntpForLiveFrame\\\":16166286619705973830,\\\"remoteID\\\":\\\"1mMEPOgnbJNjG\\\",\\\"sparkle_id\\\":\\\"gift_id_1\\\",\\\"timestamp\\\":1555018121,\\\"type\\\":37,\\\"v\\\":2}\",\"lang\":\"\",\"sender\":{\"user_id\":\"1mMEPOgnbJNjG\",\"username\":\"delof2\",\"display_name\":\"Josher\",\"profile_image_url\":\"https://prod-profile.pscp.tv/1mMEPOgnbJNjG/89d713a75028c4360f16b18507b711e6-128.jpg\",\"participant_index\":113887880},\"timestamp\":1555018121980998629}","signature":"30GSwb4YT0Cq0vWoZu6riMCGXa59do8twHFqXXQ"}
                                }
                                break;
                            case 39:
                                {
                                    //{"kind":1,"payload":"{\"room\":\"1mnGevLazEQJX\",\"body\":\"{\\\"broadcast_id\\\":\\\"1mnGevLazEQJX\\\",\\\"message_body\\\":\\\"Waste of taxpayers money\\\",\\\"message_uuid\\\":\\\"D4055AC9-24B5-44CA-9AAE-E91E2BF0C00F\\\",\\\"report_type\\\":1,\\\"type\\\":39,\\\"v\\\":2,\\\"verdict\\\":3}\",\"lang\":\"\",\"sender\":{\"user_id\":\"moderator\"},\"timestamp\":1555018631098084649}","signature":"3oAKbXviLk9dtxw3YwvibbvSCdeXtpjKCoGrPmQ"}
                                }
                                break;
                            case 40:
                                {
                                    //{"kind":1,"payload":"{\"room\":\"1BRKjvpBpzRKw\",\"body\":\"{\\\"callInsEnabled\\\":false,\\\"displayName\\\":\\\"gsg\\\",\\\"guestBroadcastingEvent\\\":2,\\\"guestChatMessageAPIVersion\\\":2,\\\"isAudioOnlyEnabled\\\":false,\\\"ntpForBroadcasterFrame\\\":0,\\\"ntpForLiveFrame\\\":0,\\\"participant_index\\\":570583730,\\\"programDateTime\\\":\\\"2019-04-11T21:01:54.302+0000\\\",\\\"remoteID\\\":\\\"1dvKOrXrrLVQX\\\",\\\"timestamp\\\":1555016538358,\\\"type\\\":40,\\\"uuid\\\":\\\"0452C6CE-6832-4956-9EB2-D06B0761175B\\\",\\\"v\\\":2}\",\"lang\":\"\",\"sender\":{\"user_id\":\"1dvKOrXrrLVQX\",\"username\":\"cullen_cdog\",\"display_name\":\"gsg\",\"profile_image_url\":\"https://pbs.twimg.com/profile_images/509479313880932352/W1fpIYvl_reasonably_small.jpeg\",\"participant_index\":570583730,\"locale\":\"en\",\"twitter_id\":\"1146174307\",\"lang\":[\"en\"]},\"timestamp\":1555016538397696033,\"uuid\":\"0452C6CE-6832-4956-9EB2-D06B0761175B\"}","signature":"3r5yuzFSBbo9Z0pnirxVkk86OiCmLYIMWO50H1w"}
                                    //{"kind":1,"payload":"{\"room\":\"1BRKjvpBpzRKw\",\"body\":\"{\\\"callInsEnabled\\\":false,\\\"displayName\\\":\\\"Seb Gruszecki\\\",\\\"guestBroadcastingEvent\\\":2,\\\"guestChatMessageAPIVersion\\\":2,\\\"isAudioOnlyEnabled\\\":false,\\\"ntpForBroadcasterFrame\\\":0,\\\"ntpForLiveFrame\\\":0,\\\"participant_index\\\":187989079,\\\"programDateTime\\\":\\\"2019-04-11T21:07:09.450+0000\\\",\\\"remoteID\\\":\\\"1eRKxqbDgPGjw\\\",\\\"timestamp\\\":1555016851284,\\\"type\\\":40,\\\"uuid\\\":\\\"39466129-85C4-4E1F-B47F-766EF9ED6AAC\\\",\\\"v\\\":2}\",\"lang\":\"\",\"sender\":{\"user_id\":\"1eRKxqbDgPGjw\",\"username\":\"SebGruszecki13\",\"display_name\":\"Seb Gruszecki\",\"profile_image_url\":\"https://pbs.twimg.com/profile_images/1113546381166354433/CkKRCLCM_reasonably_small.jpg\",\"participant_index\":187989079,\"locale\":\"en\",\"twitter_id\":\"523615135\",\"lang\":[\"en\"]},\"timestamp\":1555016851301259873,\"uuid\":\"39466129-85C4-4E1F-B47F-766EF9ED6AAC\"}","signature":"3lJ54INV5s_f0rdDOgx9Rwl_ymdK_r7KKQr_wvA"}
                                }
                                break;
                            default:
                                //{"kind":1,"payload":"{\"room\":\"1BRKjvpBpzRKw\",\"body\":\"{\\\"displayName\\\":\\\"えいいち\\\",\\\"ntpForBroadcasterFrame\\\":16166282458676879949,\\\"ntpForLiveFrame\\\":16166282053472485376,\\\"participant_index\\\":1646829248,\\\"programDateTime\\\":\\\"1970-01-01T09:00:04.926+0900\\\",\\\"timestamp\\\":1555017344,\\\"type\\\":13,\\\"user_id\\\":\\\"tw-124755397\\\",\\\"username\\\":\\\"e_ez\\\",\\\"uuid\\\":\\\"ff4cc90b-45e1-4094-accc-e9312c044989\\\",\\\"v\\\":2}\",\"lang\":\"\",\"sender\":{\"user_id\":\"tw-124755397\",\"username\":\"e_ez\",\"display_name\":\"えいいち\",\"profile_image_url\":\"https://pbs.twimg.com/profile_images/872681069/F1010027_reasonably_small.jpg\",\"participant_index\":1646829248,\"locale\":\"ja\",\"twitter_id\":\"124755397\",\"lang\":[\"ja\"]},\"timestamp\":1555017150502478139,\"uuid\":\"ff4cc90b-45e1-4094-accc-e9312c044989\"}","signature":"3XXZMx7kXGmTztS1-db-PtbWY7Auz24OCsFRT9g"}
                                //
                                throw new ParseException(websocketMessage.Raw);
                        }

                    }
                    break;
                case 2:
                    {
                        var d = DynamicJson.Parse(payload);
                        switch ((int)d.kind)
                        {
                            case 1:
                                {
                                    //{"kind":2,"payload":"{\"kind\":1,\"sender\":{\"user_id\":\"tw-948335626092359680\",\"username\":\"Chaze2182\",\"display_name\":\"Charles Johnson\",\"profile_image_url\":\"https://pbs.twimg.com/profile_images/950898633381236736/AKKQJnQy_reasonably_small.jpg\",\"participant_index\":386353554,\"locale\":\"en\",\"twitter_id\":\"948335626092359680\",\"lang\":[\"en\"]},\"body\":\"{\\\"room\\\":\\\"1BRKjvpBpzRKw\\\",\\\"following\\\":false,\\\"unlimited\\\":false}\"}","signature":"3gwuZ30fntnmcp7m95aBagVWunwWSQM2zXL0Sng"}
                                    var kind2payloadkind1Low = Tools.Deserialize<Low.kind2payloadkind1.RootObject>(payload);
                                    var kind2kind1Low = Tools.Deserialize<Low.kind2kind1.RootObject>(kind2payloadkind1Low.Body);
                                    var kind2kind1 = new Kind2Kind1(kind2kind1Low, kind2payloadkind1Low.Sender, raw);
                                    message = kind2kind1;
                                }
                                break;
                            case 2:
                                {
                                    //{"kind":2,"payload":"{\"kind\":2,\"sender\":{\"user_id\":\"1xeEWPNdOyOQP\",\"username\":\"mohammadamir1\",\"display_name\":\"Mohammad Amir\",\"profile_image_url\":\"https://platform-lookaside.fbsbx.com/platform/profilepic/?asid=1303299106487104\\u0026height=50\\u0026width=50\\u0026ext=1558863314\\u0026hash=AeSBy203S8rdJ0VK\",\"participant_index\":1168116142,\"locale\":\"hy\",\"new_user\":true,\"lang\":[\"hy\",\"en\",\"ar\"]},\"body\":\"{\\\"room\\\":\\\"1vOxwqmBoWRGB\\\",\\\"following\\\":false}\"}"}
                                    var kind2payloadkind2Low = Tools.Deserialize<Low.kind2payloadkind2.RootObject>(payload);
                                    var kind2kind2Low = Tools.Deserialize<Low.kind2kind2.RootObject>(kind2payloadkind2Low.Body);
                                    var kind2kind2 = new Kind2Kind2(kind2kind2Low, kind2payloadkind2Low.Sender, raw);
                                    message = kind2kind2;
                                }
                                break;
                            case 4:
                                {
                                    //{"kind":4,"sender":{"user_id":""},"body":"{\"room\":\"1lPKqorLvldJb\",\"occupancy\":50,\"total_participants\":740}"}
                                    var kind2payloadkind4Low = Tools.Deserialize<Low.kind2payloadkind4.RootObject>(payload);
                                    var kind2kind4Low = Tools.Deserialize<Low.kind2kind4.RootObject>(kind2payloadkind4Low.Body);
                                    var kind2kind4 = new Kind2Kind4(kind2kind4Low, kind2payloadkind4Low.Sender, raw);
                                    message = kind2kind4;
                                }
                                break;
                            default:
                                throw new ParseException(websocketMessage.Raw);
                        }
                        //JOIN: 1,
                        //LEAVE: 2,
                        //ROSTER: 3,
                        //PRESENCE: 4,
                        //BAN: 8
                    }
                    break;
                default:
                    throw new ParseException(websocketMessage.Raw);
            }
            return message;
        }
    }
}
