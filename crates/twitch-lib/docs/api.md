# TwitchのAPI
## StreamMetadata
payload
```
{
  "operationName": "StreamMetadata",
  "variables": {
    "channelLogin": "amauta_sau",
    "includeIsDJ": true
  },
  "extensions": {
    "persistedQuery": {
      "version": 1,
      "sha256Hash": "b57f9b910f8cd1a4659d894fe7550ccc81ec9052c01e438b290fd66a040b9b93"
    }
  }
}
```

response
```
{
    "data": {
        "user": {
            "id": "825030170",
            "primaryColorHex": "65BFF1",
            "roles": {
                "isPartner": true,
                "isParticipatingDJ": false,
                "__typename": "UserRoles"
            },
            "profileImageURL": "https://static-cdn.jtvnw.net/jtv_user_pictures/4a66d3c0-471f-4814-b7a2-5448da3fc027-profile_image-70x70.png",
            "primaryTeam": null,
            "channel": {
                "id": "825030170",
                "__typename": "Channel"
            },
            "lastBroadcast": {
                "id": "315344929393",
                "title": "ストグラseason２ 11日目🐼",
                "__typename": "Broadcast"
            },
            "stream": {
                "id": "315344929393",
                "type": "live",
                "createdAt": "2026-01-15T12:15:50Z",
                "game": {
                    "id": "32982",
                    "slug": "grand-theft-auto-v",
                    "name": "Grand Theft Auto V",
                    "__typename": "Game"
                },
                "__typename": "Stream"
            },
            "__typename": "User"
        }
    },
    "extensions": {
        "durationMilliseconds": 47,
        "operationName": "StreamMetadata",
        "requestID": "01KF102N9VG7CNMFZ6XT2KQNJZ"
    }
}
```

## UseLiveBroadcast
payload
```
{
  "operationName": "UseLiveBroadcast",
  "variables": {
    "channelLogin": "amauta_sau"
  },
  "extensions": {
    "persistedQuery": {
      "version": 1,
      "sha256Hash": "0b47cc6d8c182acd2e78b81c8ba5414a5a38057f2089b1bbcfa6046aae248bd2"
    }
  }
}
```

response
```
{
    "data": {
        "user": {
            "id": "825030170",
            "lastBroadcast": {
                "id": "315344929393",
                "title": "ストグラseason２ 11日目🐼",
                "game": {
                    "id": "32982",
                    "slug": "grand-theft-auto-v",
                    "name": "Grand Theft Auto V",
                    "displayName": "グランド セフト オートV",
                    "__typename": "Game"
                },
                "__typename": "Broadcast"
            },
            "__typename": "User"
        }
    },
    "extensions": {
        "durationMilliseconds": 40,
        "operationName": "UseLiveBroadcast",
        "requestID": "01KF102N9VG7CNMFZ6XT2KQNJZ"
    }
}
```

## UserLive
payload
```
{
  "operationName": "UseLive",
  "variables": {
    "channelLogin": "amauta_sau"
  },
  "extensions": {
    "persistedQuery": {
      "version": 1,
      "sha256Hash": "639d5f11bfb8bf3053b424d9ef650d04c4ebb7d94711d644afb08fe9a0fad5d9"
    }
  }
}
```

response
```
{
    "data": {
        "user": {
            "id": "825030170",
            "login": "amauta_sau",
            "stream": {
                "id": "315344929393",
                "createdAt": "2026-01-15T12:15:50Z",
                "__typename": "Stream"
            },
            "__typename": "User"
        }
    },
    "extensions": {
        "durationMilliseconds": 42,
        "operationName": "UseLive",
        "requestID": "01KF102NC8PGDG7V3091JV3SNW"
    }
}
```
## UseViewCount
payload
```
{
  "operationName": "UseViewCount",
  "variables": {
    "channelLogin": "amauta_sau"
  },
  "extensions": {
    "persistedQuery": {
      "version": 1,
      "sha256Hash": "e28de6b91c2ac736882f4960e7de60ca4a4eeebc06affdc45d6408b19318cef7"
    }
  }
}
```

response
```
{
    "data": {
        "user": {
            "id": "825030170",
            "stream": {
                "id": "315344929393",
                "viewersCount": 1481,
                "collaborationViewersCount": null,
                "costreamDetails": null,
                "__typename": "Stream"
            },
            "__typename": "User"
        }
    },
    "extensions": {
        "durationMilliseconds": 22,
        "operationName": "UseViewCount",
        "requestID": "01KF102JPYB4CEA2KAJB9D6WK8"
    }
}
```

## GetUserID
payload
```
{
  "operationName": "GetUserID",
  "variables": {
    "login": "amauta_sau",
    "lookupType": "ACTIVE"
  },
  "extensions": {
    "persistedQuery": {
      "version": 1,
      "sha256Hash": "bf6c594605caa0c63522f690156aa04bd434870bf963deb76668c381d16fcaa5"
    }
  }
}
```

response
```
{
    "data": {
        "user": {
            "id": "825030170",
            "__typename": "User"
        }
    },
    "extensions": {
        "durationMilliseconds": 4,
        "operationName": "GetUserID",
        "requestID": "01KF102JPYB4CEA2KAJB9D6WK8"
    }
}
```

## MessageBufferChatHistory
payload
```
{
  "operationName": "MessageBufferChatHistory",
  "variables": {
    "channelLogin": "amauta_sau"
  },
  "extensions": {
    "persistedQuery": {
      "version": 1,
      "sha256Hash": "33dba0e0c249135052e930cbd6c4a66daa32249ba00d1c8def75857fa3f3431d"
    }
  }
}
```

response
```
{
    "data": {
        "channel": {
            "id": "825030170",
            "recentChatMessages": [
                {
                    "id": "cca04b76-7cf1-4c97-a1ff-0122d5334a5e",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:09:58.521147832Z",
                    "content": {
                        "text": "びっちゃびちゃｗ",
                        "fragments": [
                            {
                                "text": "びっちゃびちゃｗ",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "807173118",
                        "login": "yamitsukisan",
                        "displayName": "やみつき_",
                        "__typename": "User"
                    },
                    "senderBadges": [
                        {
                            "setID": "subscriber",
                            "version": "0",
                            "id": "c3Vic2NyaWJlcjswOw==",
                            "__typename": "Badge"
                        },
                        {
                            "setID": "twitch-recap-2025",
                            "version": "1",
                            "id": "dHdpdGNoLXJlY2FwLTIwMjU7MTs=",
                            "__typename": "Badge"
                        }
                    ],
                    "senderChatColor": "#008000",
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "19c0d0f6-c3dc-45bb-946f-68a9d5917e4a",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:10:05.33642659Z",
                    "content": {
                        "text": "あばれてるｗ",
                        "fragments": [
                            {
                                "text": "あばれてるｗ",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "970049732",
                        "login": "remrem777",
                        "displayName": "remrem777",
                        "__typename": "User"
                    },
                    "senderBadges": [
                        {
                            "setID": "subscriber",
                            "version": "0",
                            "id": "c3Vic2NyaWJlcjswOw==",
                            "__typename": "Badge"
                        },
                        {
                            "setID": "premium",
                            "version": "1",
                            "id": "cHJlbWl1bTsxOw==",
                            "__typename": "Badge"
                        }
                    ],
                    "senderChatColor": "#0000FF",
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "b508d58d-2b96-42df-b3ff-1eab3066b577",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:10:11.159691307Z",
                    "content": {
                        "text": "え？ｗｗｗ",
                        "fragments": [
                            {
                                "text": "え？ｗｗｗ",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "205570749",
                        "login": "hien_noire",
                        "displayName": "氷炎",
                        "__typename": "User"
                    },
                    "senderBadges": [
                        {
                            "setID": "subscriber",
                            "version": "0",
                            "id": "c3Vic2NyaWJlcjswOw==",
                            "__typename": "Badge"
                        }
                    ],
                    "senderChatColor": null,
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "339ee1ae-a4b7-4b45-8f5d-a61cc64ce155",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:10:12.75822346Z",
                    "content": {
                        "text": "迷子ｗ",
                        "fragments": [
                            {
                                "text": "迷子ｗ",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "659596358",
                        "login": "yomogi_ys2",
                        "displayName": "楪よもぎ",
                        "__typename": "User"
                    },
                    "senderBadges": [
                        {
                            "setID": "subscriber",
                            "version": "0",
                            "id": "c3Vic2NyaWJlcjswOw==",
                            "__typename": "Badge"
                        }
                    ],
                    "senderChatColor": "#ACA7BB",
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "15eedc9a-9949-4803-9e75-fbb628f2eb13",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:10:17.338470551Z",
                    "content": {
                        "text": "迷子w",
                        "fragments": [
                            {
                                "text": "迷子w",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "261812729",
                        "login": "k821s",
                        "displayName": "k821s",
                        "__typename": "User"
                    },
                    "senderBadges": [
                        {
                            "setID": "subscriber",
                            "version": "0",
                            "id": "c3Vic2NyaWJlcjswOw==",
                            "__typename": "Badge"
                        },
                        {
                            "setID": "premium",
                            "version": "1",
                            "id": "cHJlbWl1bTsxOw==",
                            "__typename": "Badge"
                        }
                    ],
                    "senderChatColor": "#0000FF",
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "c922c95e-d959-405c-bcdc-e7495270b7e7",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:10:17.81293191Z",
                    "content": {
                        "text": "自分の車どこかみうしなったｗｗｗ",
                        "fragments": [
                            {
                                "text": "自分の車どこかみうしなったｗｗｗ",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "952176849",
                        "login": "ujuujun",
                        "displayName": "うじゅー",
                        "__typename": "User"
                    },
                    "senderBadges": [
                        {
                            "setID": "subscriber",
                            "version": "0",
                            "id": "c3Vic2NyaWJlcjswOw==",
                            "__typename": "Badge"
                        },
                        {
                            "setID": "bits",
                            "version": "1",
                            "id": "Yml0czsxOw==",
                            "__typename": "Badge"
                        }
                    ],
                    "senderChatColor": "#1E90FF",
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "eecc4cbf-dc20-4c64-82ec-369cc548ca61",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:10:20.082164201Z",
                    "content": {
                        "text": "停めたというか止まったというか",
                        "fragments": [
                            {
                                "text": "停めたというか止まったというか",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "807173118",
                        "login": "yamitsukisan",
                        "displayName": "やみつき_",
                        "__typename": "User"
                    },
                    "senderBadges": [
                        {
                            "setID": "subscriber",
                            "version": "0",
                            "id": "c3Vic2NyaWJlcjswOw==",
                            "__typename": "Badge"
                        },
                        {
                            "setID": "twitch-recap-2025",
                            "version": "1",
                            "id": "dHdpdGNoLXJlY2FwLTIwMjU7MTs=",
                            "__typename": "Badge"
                        }
                    ],
                    "senderChatColor": "#008000",
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "f76cb1f5-e965-4982-9efa-17a4d3eb444a",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:10:22.297037663Z",
                    "content": {
                        "text": "停めた？ｗ",
                        "fragments": [
                            {
                                "text": "停めた？ｗ",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "1038123324",
                        "login": "kugyurururu",
                        "displayName": "くぎゅるん",
                        "__typename": "User"
                    },
                    "senderBadges": [
                        {
                            "setID": "subscriber",
                            "version": "0",
                            "id": "c3Vic2NyaWJlcjswOw==",
                            "__typename": "Badge"
                        },
                        {
                            "setID": "twitch-recap-2024",
                            "version": "1",
                            "id": "dHdpdGNoLXJlY2FwLTIwMjQ7MTs=",
                            "__typename": "Badge"
                        }
                    ],
                    "senderChatColor": null,
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "48e76c04-9ac9-4fb6-a27b-e635e5f6880d",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:10:35.55106769Z",
                    "content": {
                        "text": "停めてるんじゃない！ハマッてるんだ！",
                        "fragments": [
                            {
                                "text": "停めてるんじゃない！ハマッてるんだ！",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "523139671",
                        "login": "astet1",
                        "displayName": "astet1",
                        "__typename": "User"
                    },
                    "senderBadges": [
                        {
                            "setID": "subscriber",
                            "version": "0",
                            "id": "c3Vic2NyaWJlcjswOw==",
                            "__typename": "Badge"
                        }
                    ],
                    "senderChatColor": null,
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "7644d56f-9b8c-4012-b629-ebebe30263b1",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:10:46.07639034Z",
                    "content": {
                        "text": "ダイナミックだ",
                        "fragments": [
                            {
                                "text": "ダイナミックだ",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "952176849",
                        "login": "ujuujun",
                        "displayName": "うじゅー",
                        "__typename": "User"
                    },
                    "senderBadges": [
                        {
                            "setID": "subscriber",
                            "version": "0",
                            "id": "c3Vic2NyaWJlcjswOw==",
                            "__typename": "Badge"
                        },
                        {
                            "setID": "bits",
                            "version": "1",
                            "id": "Yml0czsxOw==",
                            "__typename": "Badge"
                        }
                    ],
                    "senderChatColor": "#1E90FF",
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "2d6c705e-a21e-40ba-8382-8393cd225208",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:10:57.908022908Z",
                    "content": {
                        "text": "きれいにハマったなぁｗｗｗ",
                        "fragments": [
                            {
                                "text": "きれいにハマったなぁｗｗｗ",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "952176849",
                        "login": "ujuujun",
                        "displayName": "うじゅー",
                        "__typename": "User"
                    },
                    "senderBadges": [
                        {
                            "setID": "subscriber",
                            "version": "0",
                            "id": "c3Vic2NyaWJlcjswOw==",
                            "__typename": "Badge"
                        },
                        {
                            "setID": "bits",
                            "version": "1",
                            "id": "Yml0czsxOw==",
                            "__typename": "Badge"
                        }
                    ],
                    "senderChatColor": "#1E90FF",
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "feabe65f-c6d9-496b-beb9-3df08fd2b3a3",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:11:32.492010731Z",
                    "content": {
                        "text": "おー",
                        "fragments": [
                            {
                                "text": "おー",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "957782812",
                        "login": "nayutakirisaki",
                        "displayName": "霧咲ナユタ",
                        "__typename": "User"
                    },
                    "senderBadges": [],
                    "senderChatColor": null,
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "cf4c36fd-b114-4507-85eb-8eb1cb9c607e",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:11:33.319860274Z",
                    "content": {
                        "text": "いいね",
                        "fragments": [
                            {
                                "text": "いいね",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "426559271",
                        "login": "hikage0077",
                        "displayName": "ひかちゃん_ひかげ",
                        "__typename": "User"
                    },
                    "senderBadges": [
                        {
                            "setID": "subscriber",
                            "version": "0",
                            "id": "c3Vic2NyaWJlcjswOw==",
                            "__typename": "Badge"
                        },
                        {
                            "setID": "bits",
                            "version": "25000",
                            "id": "Yml0czsyNTAwMDs=",
                            "__typename": "Badge"
                        }
                    ],
                    "senderChatColor": "#F86666",
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "1c7c4e5a-c502-43db-a9c6-78cec5c18822",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:11:33.631652817Z",
                    "content": {
                        "text": "おおー",
                        "fragments": [
                            {
                                "text": "おおー",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "523139671",
                        "login": "astet1",
                        "displayName": "astet1",
                        "__typename": "User"
                    },
                    "senderBadges": [
                        {
                            "setID": "subscriber",
                            "version": "0",
                            "id": "c3Vic2NyaWJlcjswOw==",
                            "__typename": "Badge"
                        }
                    ],
                    "senderChatColor": null,
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "421d93a9-a2ae-4382-ae0c-fe9f0a625b6b",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:11:34.149362169Z",
                    "content": {
                        "text": "うわーデカイ！",
                        "fragments": [
                            {
                                "text": "うわーデカイ！",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "1232743199",
                        "login": "toumi_u",
                        "displayName": "toumi_u",
                        "__typename": "User"
                    },
                    "senderBadges": [
                        {
                            "setID": "subscriber",
                            "version": "0",
                            "id": "c3Vic2NyaWJlcjswOw==",
                            "__typename": "Badge"
                        },
                        {
                            "setID": "bits",
                            "version": "100",
                            "id": "Yml0czsxMDA7",
                            "__typename": "Badge"
                        }
                    ],
                    "senderChatColor": "#FF0000",
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "d54af761-1120-45cb-bb00-4b030b9f4e16",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:11:34.307082296Z",
                    "content": {
                        "text": "おー",
                        "fragments": [
                            {
                                "text": "おー",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "978122052",
                        "login": "mocamarun",
                        "displayName": "もかまろん",
                        "__typename": "User"
                    },
                    "senderBadges": [
                        {
                            "setID": "subscriber",
                            "version": "0",
                            "id": "c3Vic2NyaWJlcjswOw==",
                            "__typename": "Badge"
                        },
                        {
                            "setID": "premium",
                            "version": "1",
                            "id": "cHJlbWl1bTsxOw==",
                            "__typename": "Badge"
                        }
                    ],
                    "senderChatColor": null,
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "8562ccb6-2c8f-4fb8-856d-dc97c60dd33b",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:11:35.486395153Z",
                    "content": {
                        "text": "おー！",
                        "fragments": [
                            {
                                "text": "おー！",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "962545450",
                        "login": "sodashi1",
                        "displayName": "ソダシちゃん",
                        "__typename": "User"
                    },
                    "senderBadges": [
                        {
                            "setID": "subscriber",
                            "version": "0",
                            "id": "c3Vic2NyaWJlcjswOw==",
                            "__typename": "Badge"
                        },
                        {
                            "setID": "hype-train",
                            "version": "2",
                            "id": "aHlwZS10cmFpbjsyOw==",
                            "__typename": "Badge"
                        }
                    ],
                    "senderChatColor": "#1E90FF",
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "c50325dc-0327-4cb6-8486-b879a3516ea8",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:11:35.822317261Z",
                    "content": {
                        "text": "おおお",
                        "fragments": [
                            {
                                "text": "おおお",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "659596358",
                        "login": "yomogi_ys2",
                        "displayName": "楪よもぎ",
                        "__typename": "User"
                    },
                    "senderBadges": [
                        {
                            "setID": "subscriber",
                            "version": "0",
                            "id": "c3Vic2NyaWJlcjswOw==",
                            "__typename": "Badge"
                        }
                    ],
                    "senderChatColor": "#ACA7BB",
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "b2afb73c-37d6-4805-b35d-72e37eb50f93",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:11:38.35174557Z",
                    "content": {
                        "text": "いいお値段！",
                        "fragments": [
                            {
                                "text": "いいお値段！",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "952176849",
                        "login": "ujuujun",
                        "displayName": "うじゅー",
                        "__typename": "User"
                    },
                    "senderBadges": [
                        {
                            "setID": "subscriber",
                            "version": "0",
                            "id": "c3Vic2NyaWJlcjswOw==",
                            "__typename": "Badge"
                        },
                        {
                            "setID": "bits",
                            "version": "1",
                            "id": "Yml0czsxOw==",
                            "__typename": "Badge"
                        }
                    ],
                    "senderChatColor": "#1E90FF",
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "9196321a-3c4b-4bf7-8780-5f81e4d8f189",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:11:43.06556951Z",
                    "content": {
                        "text": "こみ蔵さんお金持ちか",
                        "fragments": [
                            {
                                "text": "こみ蔵さんお金持ちか",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "952176849",
                        "login": "ujuujun",
                        "displayName": "うじゅー",
                        "__typename": "User"
                    },
                    "senderBadges": [
                        {
                            "setID": "subscriber",
                            "version": "0",
                            "id": "c3Vic2NyaWJlcjswOw==",
                            "__typename": "Badge"
                        },
                        {
                            "setID": "bits",
                            "version": "1",
                            "id": "Yml0czsxOw==",
                            "__typename": "Badge"
                        }
                    ],
                    "senderChatColor": "#1E90FF",
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "12620635-dd9f-4cde-9c14-10eeb7e4cf4a",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:11:43.306977727Z",
                    "content": {
                        "text": "うおおおでかい",
                        "fragments": [
                            {
                                "text": "うおおおでかい",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "467454042",
                        "login": "24h_alchemist",
                        "displayName": "金平糖ライオン",
                        "__typename": "User"
                    },
                    "senderBadges": [
                        {
                            "setID": "subscriber",
                            "version": "0",
                            "id": "c3Vic2NyaWJlcjswOw==",
                            "__typename": "Badge"
                        },
                        {
                            "setID": "bits",
                            "version": "1000",
                            "id": "Yml0czsxMDAwOw==",
                            "__typename": "Badge"
                        }
                    ],
                    "senderChatColor": "#00C3C7",
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "0f914758-9406-41db-ad5f-8c0031f50244",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:11:52.713511923Z",
                    "content": {
                        "text": "すごいなあ",
                        "fragments": [
                            {
                                "text": "すごいなあ",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "1038123324",
                        "login": "kugyurururu",
                        "displayName": "くぎゅるん",
                        "__typename": "User"
                    },
                    "senderBadges": [
                        {
                            "setID": "subscriber",
                            "version": "0",
                            "id": "c3Vic2NyaWJlcjswOw==",
                            "__typename": "Badge"
                        },
                        {
                            "setID": "twitch-recap-2024",
                            "version": "1",
                            "id": "dHdpdGNoLXJlY2FwLTIwMjQ7MTs=",
                            "__typename": "Badge"
                        }
                    ],
                    "senderChatColor": null,
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "c8bdb1cc-7d5e-4d51-bacb-5434fd7af4d0",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:12:06.039081982Z",
                    "content": {
                        "text": "ギャンブラーやｗ",
                        "fragments": [
                            {
                                "text": "ギャンブラーやｗ",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "952176849",
                        "login": "ujuujun",
                        "displayName": "うじゅー",
                        "__typename": "User"
                    },
                    "senderBadges": [
                        {
                            "setID": "subscriber",
                            "version": "0",
                            "id": "c3Vic2NyaWJlcjswOw==",
                            "__typename": "Badge"
                        },
                        {
                            "setID": "bits",
                            "version": "1",
                            "id": "Yml0czsxOw==",
                            "__typename": "Badge"
                        }
                    ],
                    "senderChatColor": "#1E90FF",
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "8414baff-1659-4bae-85f9-1e9e1ad33635",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:12:08.675735626Z",
                    "content": {
                        "text": "ギャンブラーや",
                        "fragments": [
                            {
                                "text": "ギャンブラーや",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "957782812",
                        "login": "nayutakirisaki",
                        "displayName": "霧咲ナユタ",
                        "__typename": "User"
                    },
                    "senderBadges": [],
                    "senderChatColor": null,
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "9db44948-7d73-406f-b99a-8ca24a8174fb",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:12:11.541055961Z",
                    "content": {
                        "text": "流石元カジノ店員",
                        "fragments": [
                            {
                                "text": "流石元カジノ店員",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "261812729",
                        "login": "k821s",
                        "displayName": "k821s",
                        "__typename": "User"
                    },
                    "senderBadges": [
                        {
                            "setID": "subscriber",
                            "version": "0",
                            "id": "c3Vic2NyaWJlcjswOw==",
                            "__typename": "Badge"
                        },
                        {
                            "setID": "premium",
                            "version": "1",
                            "id": "cHJlbWl1bTsxOw==",
                            "__typename": "Badge"
                        }
                    ],
                    "senderChatColor": "#0000FF",
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "8b1e2047-2afb-46eb-a34a-b1e068316295",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:12:11.936623976Z",
                    "content": {
                        "text": "さすが元カジノ店員",
                        "fragments": [
                            {
                                "text": "さすが元カジノ店員",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "1232743199",
                        "login": "toumi_u",
                        "displayName": "toumi_u",
                        "__typename": "User"
                    },
                    "senderBadges": [
                        {
                            "setID": "subscriber",
                            "version": "0",
                            "id": "c3Vic2NyaWJlcjswOw==",
                            "__typename": "Badge"
                        },
                        {
                            "setID": "bits",
                            "version": "100",
                            "id": "Yml0czsxMDA7",
                            "__typename": "Badge"
                        }
                    ],
                    "senderChatColor": "#FF0000",
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "92407250-d640-4a3c-9faa-f9d8c5cb397f",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:12:14.119598906Z",
                    "content": {
                        "text": "すんごｗｗ",
                        "fragments": [
                            {
                                "text": "すんごｗｗ",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "952176849",
                        "login": "ujuujun",
                        "displayName": "うじゅー",
                        "__typename": "User"
                    },
                    "senderBadges": [
                        {
                            "setID": "subscriber",
                            "version": "0",
                            "id": "c3Vic2NyaWJlcjswOw==",
                            "__typename": "Badge"
                        },
                        {
                            "setID": "bits",
                            "version": "1",
                            "id": "Yml0czsxOw==",
                            "__typename": "Badge"
                        }
                    ],
                    "senderChatColor": "#1E90FF",
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "fe9b8641-9c6f-4ebb-b504-cfff352230aa",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:12:15.273956382Z",
                    "content": {
                        "text": "20000！？",
                        "fragments": [
                            {
                                "text": "20000！？",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "441666266",
                        "login": "ik55d",
                        "displayName": "愛さん",
                        "__typename": "User"
                    },
                    "senderBadges": [
                        {
                            "setID": "subscriber",
                            "version": "0",
                            "id": "c3Vic2NyaWJlcjswOw==",
                            "__typename": "Badge"
                        }
                    ],
                    "senderChatColor": "#DD19A7",
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "410799a4-8a5d-4abf-9449-f62c2f79da40",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:12:23.583618425Z",
                    "content": {
                        "text": "にまん！？",
                        "fragments": [
                            {
                                "text": "にまん！？",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "962545450",
                        "login": "sodashi1",
                        "displayName": "ソダシちゃん",
                        "__typename": "User"
                    },
                    "senderBadges": [
                        {
                            "setID": "subscriber",
                            "version": "0",
                            "id": "c3Vic2NyaWJlcjswOw==",
                            "__typename": "Badge"
                        },
                        {
                            "setID": "hype-train",
                            "version": "2",
                            "id": "aHlwZS10cmFpbjsyOw==",
                            "__typename": "Badge"
                        }
                    ],
                    "senderChatColor": "#1E90FF",
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "55ef4fc9-a1f8-47e5-8dd2-4879749c68d7",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:12:23.78788383Z",
                    "content": {
                        "text": "さて",
                        "fragments": [
                            {
                                "text": "さて",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "205570749",
                        "login": "hien_noire",
                        "displayName": "氷炎",
                        "__typename": "User"
                    },
                    "senderBadges": [
                        {
                            "setID": "subscriber",
                            "version": "0",
                            "id": "c3Vic2NyaWJlcjswOw==",
                            "__typename": "Badge"
                        }
                    ],
                    "senderChatColor": null,
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "ac0630dd-f931-4aae-b435-e659c047be38",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:12:24.372388064Z",
                    "content": {
                        "text": "戻せるかｗ",
                        "fragments": [
                            {
                                "text": "戻せるかｗ",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "807173118",
                        "login": "yamitsukisan",
                        "displayName": "やみつき_",
                        "__typename": "User"
                    },
                    "senderBadges": [
                        {
                            "setID": "subscriber",
                            "version": "0",
                            "id": "c3Vic2NyaWJlcjswOw==",
                            "__typename": "Badge"
                        },
                        {
                            "setID": "twitch-recap-2025",
                            "version": "1",
                            "id": "dHdpdGNoLXJlY2FwLTIwMjU7MTs=",
                            "__typename": "Badge"
                        }
                    ],
                    "senderChatColor": "#008000",
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "9bd5ecf8-1de5-4dbb-a61b-68be19104fe0",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:12:25.75523434Z",
                    "content": {
                        "text": "がんばってるなあ",
                        "fragments": [
                            {
                                "text": "がんばってるなあ",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "970049732",
                        "login": "remrem777",
                        "displayName": "remrem777",
                        "__typename": "User"
                    },
                    "senderBadges": [
                        {
                            "setID": "subscriber",
                            "version": "0",
                            "id": "c3Vic2NyaWJlcjswOw==",
                            "__typename": "Badge"
                        },
                        {
                            "setID": "premium",
                            "version": "1",
                            "id": "cHJlbWl1bTsxOw==",
                            "__typename": "Badge"
                        }
                    ],
                    "senderChatColor": "#0000FF",
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "7898abe3-1afc-49a9-9226-ba1b88b7b467",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:12:27.335707819Z",
                    "content": {
                        "text": "さぁ車をどうするかｗ",
                        "fragments": [
                            {
                                "text": "さぁ車をどうするかｗ",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "952176849",
                        "login": "ujuujun",
                        "displayName": "うじゅー",
                        "__typename": "User"
                    },
                    "senderBadges": [
                        {
                            "setID": "subscriber",
                            "version": "0",
                            "id": "c3Vic2NyaWJlcjswOw==",
                            "__typename": "Badge"
                        },
                        {
                            "setID": "bits",
                            "version": "1",
                            "id": "Yml0czsxOw==",
                            "__typename": "Badge"
                        }
                    ],
                    "senderChatColor": "#1E90FF",
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "bffcb73c-c1b3-4a9f-b848-86ad552a1210",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:12:33.908268974Z",
                    "content": {
                        "text": "ひっくりかえしたら動かないかなぁ",
                        "fragments": [
                            {
                                "text": "ひっくりかえしたら動かないかなぁ",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "659596358",
                        "login": "yomogi_ys2",
                        "displayName": "楪よもぎ",
                        "__typename": "User"
                    },
                    "senderBadges": [
                        {
                            "setID": "subscriber",
                            "version": "0",
                            "id": "c3Vic2NyaWJlcjswOw==",
                            "__typename": "Badge"
                        }
                    ],
                    "senderChatColor": "#ACA7BB",
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "afeb6218-0707-4aea-9395-2c8af66305ce",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:12:40.743844117Z",
                    "content": {
                        "text": "須藤さんｗｗｗｗ",
                        "fragments": [
                            {
                                "text": "須藤さんｗｗｗｗ",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "952176849",
                        "login": "ujuujun",
                        "displayName": "うじゅー",
                        "__typename": "User"
                    },
                    "senderBadges": [
                        {
                            "setID": "subscriber",
                            "version": "0",
                            "id": "c3Vic2NyaWJlcjswOw==",
                            "__typename": "Badge"
                        },
                        {
                            "setID": "bits",
                            "version": "1",
                            "id": "Yml0czsxOw==",
                            "__typename": "Badge"
                        }
                    ],
                    "senderChatColor": "#1E90FF",
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "bdeba791-dfe6-4810-a418-c16d370a2a02",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:12:42.209158465Z",
                    "content": {
                        "text": "須藤さんw",
                        "fragments": [
                            {
                                "text": "須藤さんw",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "1232743199",
                        "login": "toumi_u",
                        "displayName": "toumi_u",
                        "__typename": "User"
                    },
                    "senderBadges": [
                        {
                            "setID": "subscriber",
                            "version": "0",
                            "id": "c3Vic2NyaWJlcjswOw==",
                            "__typename": "Badge"
                        },
                        {
                            "setID": "bits",
                            "version": "100",
                            "id": "Yml0czsxMDA7",
                            "__typename": "Badge"
                        }
                    ],
                    "senderChatColor": "#FF0000",
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "7fc86450-6290-4cb8-8b1a-5e330c3e7b15",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:12:43.179632516Z",
                    "content": {
                        "text": "車起こし直したら",
                        "fragments": [
                            {
                                "text": "車起こし直したら",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "948854346",
                        "login": "honyotu",
                        "displayName": "ぽんたに",
                        "__typename": "User"
                    },
                    "senderBadges": [
                        {
                            "setID": "premium",
                            "version": "1",
                            "id": "cHJlbWl1bTsxOw==",
                            "__typename": "Badge"
                        }
                    ],
                    "senderChatColor": "#0000FF",
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "3244a2a1-f1cc-4211-b0b5-44d822874a9c",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:12:43.340584322Z",
                    "content": {
                        "text": "いいところに！",
                        "fragments": [
                            {
                                "text": "いいところに！",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "426559271",
                        "login": "hikage0077",
                        "displayName": "ひかちゃん_ひかげ",
                        "__typename": "User"
                    },
                    "senderBadges": [
                        {
                            "setID": "subscriber",
                            "version": "0",
                            "id": "c3Vic2NyaWJlcjswOw==",
                            "__typename": "Badge"
                        },
                        {
                            "setID": "bits",
                            "version": "25000",
                            "id": "Yml0czsyNTAwMDs=",
                            "__typename": "Badge"
                        }
                    ],
                    "senderChatColor": "#F86666",
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "ce5a7fe9-9594-4372-95a1-869608ed9475",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:12:46.60273169Z",
                    "content": {
                        "text": "フリップしたほうが早そうではあるw",
                        "fragments": [
                            {
                                "text": "フリップしたほうが早そうではあるw",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "925545565",
                        "login": "fujikai14",
                        "displayName": "けーり_",
                        "__typename": "User"
                    },
                    "senderBadges": [
                        {
                            "setID": "subscriber",
                            "version": "0",
                            "id": "c3Vic2NyaWJlcjswOw==",
                            "__typename": "Badge"
                        }
                    ],
                    "senderChatColor": "#2E8B57",
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "0734fb72-c75c-4e8c-817f-28ab0b9f453a",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:12:49.149056042Z",
                    "content": {
                        "text": "押してもらうしかないｗ",
                        "fragments": [
                            {
                                "text": "押してもらうしかないｗ",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "807173118",
                        "login": "yamitsukisan",
                        "displayName": "やみつき_",
                        "__typename": "User"
                    },
                    "senderBadges": [
                        {
                            "setID": "subscriber",
                            "version": "0",
                            "id": "c3Vic2NyaWJlcjswOw==",
                            "__typename": "Badge"
                        },
                        {
                            "setID": "twitch-recap-2025",
                            "version": "1",
                            "id": "dHdpdGNoLXJlY2FwLTIwMjU7MTs=",
                            "__typename": "Badge"
                        }
                    ],
                    "senderChatColor": "#008000",
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "961b24ef-409a-4127-abcb-1ee90317e7b7",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:12:54.87633479Z",
                    "content": {
                        "text": "サウ汰ガールｗｗｗ",
                        "fragments": [
                            {
                                "text": "サウ汰ガールｗｗｗ",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "952176849",
                        "login": "ujuujun",
                        "displayName": "うじゅー",
                        "__typename": "User"
                    },
                    "senderBadges": [
                        {
                            "setID": "subscriber",
                            "version": "0",
                            "id": "c3Vic2NyaWJlcjswOw==",
                            "__typename": "Badge"
                        },
                        {
                            "setID": "bits",
                            "version": "1",
                            "id": "Yml0czsxOw==",
                            "__typename": "Badge"
                        }
                    ],
                    "senderChatColor": "#1E90FF",
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "d85e038d-bae0-4c1e-9c7f-25af7bc67533",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:13:01.403057217Z",
                    "content": {
                        "text": "須藤さんなら助けてくれそうｗｗ",
                        "fragments": [
                            {
                                "text": "須藤さんなら助けてくれそうｗｗ",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "467454042",
                        "login": "24h_alchemist",
                        "displayName": "金平糖ライオン",
                        "__typename": "User"
                    },
                    "senderBadges": [
                        {
                            "setID": "subscriber",
                            "version": "0",
                            "id": "c3Vic2NyaWJlcjswOw==",
                            "__typename": "Badge"
                        },
                        {
                            "setID": "bits",
                            "version": "1000",
                            "id": "Yml0czsxMDAwOw==",
                            "__typename": "Badge"
                        }
                    ],
                    "senderChatColor": "#00C3C7",
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "b0237f45-b531-4f4a-aa5f-82c3f8960185",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:13:08.670393055Z",
                    "content": {
                        "text": "やっぱフリップかｗ",
                        "fragments": [
                            {
                                "text": "やっぱフリップかｗ",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "807173118",
                        "login": "yamitsukisan",
                        "displayName": "やみつき_",
                        "__typename": "User"
                    },
                    "senderBadges": [
                        {
                            "setID": "subscriber",
                            "version": "0",
                            "id": "c3Vic2NyaWJlcjswOw==",
                            "__typename": "Badge"
                        },
                        {
                            "setID": "twitch-recap-2025",
                            "version": "1",
                            "id": "dHdpdGNoLXJlY2FwLTIwMjU7MTs=",
                            "__typename": "Badge"
                        }
                    ],
                    "senderChatColor": "#008000",
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "ddb29407-8726-4acc-9b69-513cc7a9d06c",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:13:11.137777567Z",
                    "content": {
                        "text": "さすが須藤さん",
                        "fragments": [
                            {
                                "text": "さすが須藤さん",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "952176849",
                        "login": "ujuujun",
                        "displayName": "うじゅー",
                        "__typename": "User"
                    },
                    "senderBadges": [
                        {
                            "setID": "subscriber",
                            "version": "0",
                            "id": "c3Vic2NyaWJlcjswOw==",
                            "__typename": "Badge"
                        },
                        {
                            "setID": "bits",
                            "version": "1",
                            "id": "Yml0czsxOw==",
                            "__typename": "Badge"
                        }
                    ],
                    "senderChatColor": "#1E90FF",
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "1a3ea577-f2ba-40ef-bdbc-cc86416b14b0",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:13:11.481802503Z",
                    "content": {
                        "text": "おぉ",
                        "fragments": [
                            {
                                "text": "おぉ",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "667190548",
                        "login": "m_y_co",
                        "displayName": "まい胡",
                        "__typename": "User"
                    },
                    "senderBadges": [
                        {
                            "setID": "subscriber",
                            "version": "0",
                            "id": "c3Vic2NyaWJlcjswOw==",
                            "__typename": "Badge"
                        },
                        {
                            "setID": "premium",
                            "version": "1",
                            "id": "cHJlbWl1bTsxOw==",
                            "__typename": "Badge"
                        }
                    ],
                    "senderChatColor": null,
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "c25bffb9-9df7-449d-b0e3-059f8af2f2ec",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:13:12.296274127Z",
                    "content": {
                        "text": "おー",
                        "fragments": [
                            {
                                "text": "おー",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "261812729",
                        "login": "k821s",
                        "displayName": "k821s",
                        "__typename": "User"
                    },
                    "senderBadges": [
                        {
                            "setID": "subscriber",
                            "version": "0",
                            "id": "c3Vic2NyaWJlcjswOw==",
                            "__typename": "Badge"
                        },
                        {
                            "setID": "premium",
                            "version": "1",
                            "id": "cHJlbWl1bTsxOw==",
                            "__typename": "Badge"
                        }
                    ],
                    "senderChatColor": "#0000FF",
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "27125c09-143e-475c-800e-6ab9d675fb4d",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:13:15.943270596Z",
                    "content": {
                        "text": "おおおお",
                        "fragments": [
                            {
                                "text": "おおおお",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "1414649336",
                        "login": "junker_mee",
                        "displayName": "junker_mee",
                        "__typename": "User"
                    },
                    "senderBadges": [],
                    "senderChatColor": null,
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "7da789b9-e1db-42e8-9032-0c88d41eeb49",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:13:24.425603789Z",
                    "content": {
                        "text": "まずいｗ",
                        "fragments": [
                            {
                                "text": "まずいｗ",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "952176849",
                        "login": "ujuujun",
                        "displayName": "うじゅー",
                        "__typename": "User"
                    },
                    "senderBadges": [
                        {
                            "setID": "subscriber",
                            "version": "0",
                            "id": "c3Vic2NyaWJlcjswOw==",
                            "__typename": "Badge"
                        },
                        {
                            "setID": "bits",
                            "version": "1",
                            "id": "Yml0czsxOw==",
                            "__typename": "Badge"
                        }
                    ],
                    "senderChatColor": "#1E90FF",
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "dd0e6770-cec9-4ca8-adff-763e00d014ca",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:13:41.258025814Z",
                    "content": {
                        "text": "ないすー",
                        "fragments": [
                            {
                                "text": "ないすー",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "807173118",
                        "login": "yamitsukisan",
                        "displayName": "やみつき_",
                        "__typename": "User"
                    },
                    "senderBadges": [
                        {
                            "setID": "subscriber",
                            "version": "0",
                            "id": "c3Vic2NyaWJlcjswOw==",
                            "__typename": "Badge"
                        },
                        {
                            "setID": "twitch-recap-2025",
                            "version": "1",
                            "id": "dHdpdGNoLXJlY2FwLTIwMjU7MTs=",
                            "__typename": "Badge"
                        }
                    ],
                    "senderChatColor": "#008000",
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                },
                {
                    "id": "77b0ff25-f46e-4918-8b32-d60547f27c32",
                    "deletedAt": null,
                    "sentAt": "2026-01-15T14:13:41.42314542Z",
                    "content": {
                        "text": "よかったｗ",
                        "fragments": [
                            {
                                "text": "よかったｗ",
                                "content": null,
                                "__typename": "MessageFragment"
                            }
                        ],
                        "__typename": "MessageContent"
                    },
                    "parentMessage": null,
                    "threadParentMessage": null,
                    "sender": {
                        "id": "952176849",
                        "login": "ujuujun",
                        "displayName": "うじゅー",
                        "__typename": "User"
                    },
                    "senderBadges": [
                        {
                            "setID": "subscriber",
                            "version": "0",
                            "id": "c3Vic2NyaWJlcjswOw==",
                            "__typename": "Badge"
                        },
                        {
                            "setID": "bits",
                            "version": "1",
                            "id": "Yml0czsxOw==",
                            "__typename": "Badge"
                        }
                    ],
                    "senderChatColor": "#1E90FF",
                    "sourceChannel": null,
                    "sourceSenderBadges": null,
                    "__typename": "Message"
                }
            ],
            "__typename": "Channel"
        }
    },
    "extensions": {
        "durationMilliseconds": 60,
        "operationName": "MessageBufferChatHistory",
        "requestID": "01KF102K5ZFQQ3QYB87Q7QS8F5"
    }
}
```

## MessageBuffer_Channel
payload
```
{
  "operationName": "MessageBuffer_Channel",
  "variables": {
    "channelLogin": "amauta_sau"
  },
  "extensions": {
    "persistedQuery": {
      "version": 1,
      "sha256Hash": "bfc959904f55b5003ae4674d4bea83ebdcd8867ad76e12f38957d433902d2fcc"
    }
  }
}
```

response
```
{
    "data": {
        "user": {
            "id": "825030170",
            "chatSettings": {
                "chatDelayMs": 0,
                "__typename": "ChatSettings"
            },
            "__typename": "User"
        }
    },
    "extensions": {
        "durationMilliseconds": 5,
        "operationName": "MessageBuffer_Channel",
        "requestID": "01KF102K5ZFQQ3QYB87Q7QS8F5"
    }
}
```

## GetDisplayName
channel_loginからdisplayNameとidを取得する
payload
```
{
  "operationName": "GetDisplayName",
  "variables": {
    "login": "amauta_sau"
  },
  "extensions": {
    "persistedQuery": {
      "version": 1,
      "sha256Hash": "ba351b3d3018c3779fcaa398507e41579ae6cf12ad123a04f090943c21dedb8a"
    }
  }
}
```

response
```
{
  "data": {
    "user": {
      "__typename": "User",
      "displayName": "天唄サウ",
      "id": "825030170",
      "login": "amauta_sau"
    }
  },
  "extensions": {
    "durationMilliseconds": 19,
    "operationName": "GetDisplayName",
    "requestID": "01KF6BT9D71JCD6JBENV6W59T7"
  }
}
```

## VideoComments
payload
```
{
  "operationName": "VideoComments",
  "variables": {
    "videoID": "2669748313",
    "hasVideoID": true
  },
  "extensions": {
    "persistedQuery": {
      "version": 1,
      "sha256Hash": "be06407e8d7cda72f2ee086ebb11abb6b062a7deb8985738e648090904d2f0eb"
    }
  }
}
```

response
```
{
        "data": {
            "badges": [
                {
                    "id": "MTAteWVhcnMtYXMtdHdpdGNoLXN0YWZmOzE7",
                    "setID": "10-years-as-twitch-staff",
                    "version": "1",
                    "title": "Twitchスタッフとなって10年",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/e48bfab8-6697-4c5b-84df-e64fb0150701/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/e48bfab8-6697-4c5b-84df-e64fb0150701/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/e48bfab8-6697-4c5b-84df-e64fb0150701/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "MTUteWVhcnMtYXMtdHdpdGNoLXN0YWZmOzE7",
                    "setID": "15-years-as-twitch-staff",
                    "version": "1",
                    "title": "Twitchスタッフとなって15年",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/523802ec-086b-4dec-b441-90e28b0806d8/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/523802ec-086b-4dec-b441-90e28b0806d8/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/523802ec-086b-4dec-b441-90e28b0806d8/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "MTk3OS1yZXZvbHV0aW9uXzE7MTs=",
                    "setID": "1979-revolution_1",
                    "version": "1",
                    "title": "1979 Revolution",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/7833bb6e-d20d-48ff-a58d-67fe827a4f84/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/7833bb6e-d20d-48ff-a58d-67fe827a4f84/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/7833bb6e-d20d-48ff-a58d-67fe827a4f84/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/1979%20Revolution/details",
                    "__typename": "Badge"
                },
                {
                    "id": "NS15ZWFycy1hcy10d2l0Y2gtc3RhZmY7MTs=",
                    "setID": "5-years-as-twitch-staff",
                    "version": "1",
                    "title": "Twitchスタッフとなって5年",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/d53671d0-0ce0-4706-905f-7fe8b122a27a/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/d53671d0-0ce0-4706-905f-7fe8b122a27a/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/d53671d0-0ce0-4706-905f-7fe8b122a27a/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "NjAtc2Vjb25kc18xOzE7",
                    "setID": "60-seconds_1",
                    "version": "1",
                    "title": "60 Seconds!",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/1e7252f9-7e80-4d3d-ae42-319f030cca99/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/1e7252f9-7e80-4d3d-ae42-319f030cca99/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/1e7252f9-7e80-4d3d-ae42-319f030cca99/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/60%20Seconds!/details",
                    "__typename": "Badge"
                },
                {
                    "id": "NjAtc2Vjb25kc18yOzE7",
                    "setID": "60-seconds_2",
                    "version": "1",
                    "title": "60 Seconds!",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/64513f7d-21dd-4a05-a699-d73761945cf9/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/64513f7d-21dd-4a05-a699-d73761945cf9/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/64513f7d-21dd-4a05-a699-d73761945cf9/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/60%20Seconds!/details",
                    "__typename": "Badge"
                },
                {
                    "id": "NjAtc2Vjb25kc18zOzE7",
                    "setID": "60-seconds_3",
                    "version": "1",
                    "title": "60 Seconds!",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/f4306617-0f96-476f-994e-5304f81bcc6e/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/f4306617-0f96-476f-994e-5304f81bcc6e/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/f4306617-0f96-476f-994e-5304f81bcc6e/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/60%20Seconds!/details",
                    "__typename": "Badge"
                },
                {
                    "id": "SDFaMV8xOzE7",
                    "setID": "H1Z1_1",
                    "version": "1",
                    "title": "H1Z1",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/fc71386c-86cd-11e7-a55d-43f591dc0c71/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/fc71386c-86cd-11e7-a55d-43f591dc0c71/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/fc71386c-86cd-11e7-a55d-43f591dc0c71/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/H1Z1/details",
                    "__typename": "Badge"
                },
                {
                    "id": "YWRtaW47MTs=",
                    "setID": "admin",
                    "version": "1",
                    "title": "アドミン",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/9ef7e029-4cdf-4d4d-a0d5-e2b3fb2583fe/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/9ef7e029-4cdf-4d4d-a0d5-e2b3fb2583fe/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/9ef7e029-4cdf-4d4d-a0d5-e2b3fb2583fe/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "YWxvbmU7MTs=",
                    "setID": "alone",
                    "version": "1",
                    "title": "アローン",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/10ba11a2-0171-42b6-9bba-8f2f14248172/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/10ba11a2-0171-42b6-9bba-8f2f14248172/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/10ba11a2-0171-42b6-9bba-8f2f14248172/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "YW1iYXNzYWRvcjsxOw==",
                    "setID": "ambassador",
                    "version": "1",
                    "title": "Twitch Ambassador",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/2cbc339f-34f4-488a-ae51-efdf74f4e323/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/2cbc339f-34f4-488a-ae51-efdf74f4e323/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/2cbc339f-34f4-488a-ae51-efdf74f4e323/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/team/ambassadors",
                    "__typename": "Badge"
                },
                {
                    "id": "YW5vbWFseS0yXzE7MTs=",
                    "setID": "anomaly-2_1",
                    "version": "1",
                    "title": "Anomaly 2",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/d1d1ad54-40a6-492b-882e-dcbdce5fa81e/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/d1d1ad54-40a6-492b-882e-dcbdce5fa81e/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/d1d1ad54-40a6-492b-882e-dcbdce5fa81e/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/Anomaly%202/details",
                    "__typename": "Badge"
                },
                {
                    "id": "YW5vbWFseS13YXJ6b25lLWVhcnRoXzE7MTs=",
                    "setID": "anomaly-warzone-earth_1",
                    "version": "1",
                    "title": "Anomaly Warzone Earth",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/858be873-fb1f-47e5-ad34-657f40d3d156/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/858be873-fb1f-47e5-ad34-657f40d3d156/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/858be873-fb1f-47e5-ad34-657f40d3d156/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/Anomaly:%20Warzone%20Earth/details",
                    "__typename": "Badge"
                },
                {
                    "id": "YW5vbnltb3VzLWNoZWVyZXI7MTs=",
                    "setID": "anonymous-cheerer",
                    "version": "1",
                    "title": "匿名の支援者",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/ca3db7f7-18f5-487e-a329-cd0b538ee979/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/ca3db7f7-18f5-487e-a329-cd0b538ee979/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/ca3db7f7-18f5-487e-a329-cd0b538ee979/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "YXJjLXJhaWRlcnMtbGF1bmNoLTIwMjU7MTs=",
                    "setID": "arc-raiders-launch-2025",
                    "version": "1",
                    "title": "Arc Raidersローンチ2025",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/d4aa495f-a0e4-4ab4-b3eb-7c2ea573b03f/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/d4aa495f-a0e4-4ab4-b3eb-7c2ea573b03f/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/d4aa495f-a0e4-4ab4-b3eb-7c2ea573b03f/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "YXJjYW5lLXNlYXNvbi0yLXByZW1pZXJlOzE7",
                    "setID": "arcane-season-2-premiere",
                    "version": "1",
                    "title": "『Arcane』シーズン2 プレミア",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/1d833bde-edc7-4d23-b7b6-ad5a13296675/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/1d833bde-edc7-4d23-b7b6-ad5a13296675/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/1d833bde-edc7-4d23-b7b6-ad5a13296675/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "YXJ0aXN0LWJhZGdlOzE7",
                    "setID": "artist-badge",
                    "version": "1",
                    "title": "Artist",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/4300a897-03dc-4e83-8c0e-c332fee7057f/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/4300a897-03dc-4e83-8c0e-c332fee7057f/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/4300a897-03dc-4e83-8c0e-c332fee7057f/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "YXhpb20tdmVyZ2VfMTsxOw==",
                    "setID": "axiom-verge_1",
                    "version": "1",
                    "title": "Axiom Verge",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/f209b747-45ee-42f6-8baf-ea7542633d10/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/f209b747-45ee-42f6-8baf-ea7542633d10/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/f209b747-45ee-42f6-8baf-ea7542633d10/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/Axiom%20Verge/details",
                    "__typename": "Badge"
                },
                {
                    "id": "YmF0dGxlY2hlZmJyaWdhZGVfMTsxOw==",
                    "setID": "battlechefbrigade_1",
                    "version": "1",
                    "title": "Battle Chef Brigade",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/24e32e67-33cd-4227-ad96-f0a7fc836107/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/24e32e67-33cd-4227-ad96-f0a7fc836107/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/24e32e67-33cd-4227-ad96-f0a7fc836107/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/Battle%20Chef%20Brigade/details",
                    "__typename": "Badge"
                },
                {
                    "id": "YmF0dGxlY2hlZmJyaWdhZGVfMjsxOw==",
                    "setID": "battlechefbrigade_2",
                    "version": "1",
                    "title": "Battle Chef Brigade",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/ef1e96e8-a0f9-40b6-87af-2977d3c004bb/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/ef1e96e8-a0f9-40b6-87af-2977d3c004bb/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/ef1e96e8-a0f9-40b6-87af-2977d3c004bb/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/Battle%20Chef%20Brigade/details",
                    "__typename": "Badge"
                },
                {
                    "id": "YmF0dGxlY2hlZmJyaWdhZGVfMzsxOw==",
                    "setID": "battlechefbrigade_3",
                    "version": "1",
                    "title": "Battle Chef Brigade",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/107ebb20-4fcd-449a-9931-cd3f81b84c70/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/107ebb20-4fcd-449a-9931-cd3f81b84c70/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/107ebb20-4fcd-449a-9931-cd3f81b84c70/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/Battle%20Chef%20Brigade/details",
                    "__typename": "Badge"
                },
                {
                    "id": "YmF0dGxlZmllbGQtNjsxOw==",
                    "setID": "battlefield-6",
                    "version": "1",
                    "title": "バトルフィールド6",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/d7750af0-caca-47c5-b207-1af9be69ce1b/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/d7750af0-caca-47c5-b207-1af9be69ce1b/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/d7750af0-caca-47c5-b207-1af9be69ce1b/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "YmF0dGxlcml0ZV8xOzE7",
                    "setID": "battlerite_1",
                    "version": "1",
                    "title": "Battlerite",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/484ebda9-f7fa-4c67-b12b-c80582f3cc61/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/484ebda9-f7fa-4c67-b12b-c80582f3cc61/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/484ebda9-f7fa-4c67-b12b-c80582f3cc61/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/Battlerite/details",
                    "__typename": "Badge"
                },
                {
                    "id": "Yml0czsxMDAwMDA7",
                    "setID": "bits",
                    "version": "100000",
                    "title": "cheer 100000",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/96f0540f-aa63-49e1-a8b3-259ece3bd098/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/96f0540f-aa63-49e1-a8b3-259ece3bd098/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/96f0540f-aa63-49e1-a8b3-259ece3bd098/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://bits.twitch.tv",
                    "__typename": "Badge"
                },
                {
                    "id": "Yml0czsxOw==",
                    "setID": "bits",
                    "version": "1",
                    "title": "cheer 1",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/73b5c3fb-24f9-4a82-a852-2f475b59411c/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/73b5c3fb-24f9-4a82-a852-2f475b59411c/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/73b5c3fb-24f9-4a82-a852-2f475b59411c/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://bits.twitch.tv",
                    "__typename": "Badge"
                },
                {
                    "id": "Yml0czs1MDAwMDA7",
                    "setID": "bits",
                    "version": "500000",
                    "title": "cheer 500000",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/f6932b57-6a6e-4062-a770-dfbd9f4302e5/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/f6932b57-6a6e-4062-a770-dfbd9f4302e5/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/f6932b57-6a6e-4062-a770-dfbd9f4302e5/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://bits.twitch.tv",
                    "__typename": "Badge"
                },
                {
                    "id": "Yml0czsyMDAwMDA7",
                    "setID": "bits",
                    "version": "200000",
                    "title": "cheer 200000",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/4a0b90c4-e4ef-407f-84fe-36b14aebdbb6/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/4a0b90c4-e4ef-407f-84fe-36b14aebdbb6/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/4a0b90c4-e4ef-407f-84fe-36b14aebdbb6/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://bits.twitch.tv",
                    "__typename": "Badge"
                },
                {
                    "id": "Yml0czsxMDAwMDs=",
                    "setID": "bits",
                    "version": "10000",
                    "title": "cheer 10000",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/68af213b-a771-4124-b6e3-9bb6d98aa732/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/68af213b-a771-4124-b6e3-9bb6d98aa732/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/68af213b-a771-4124-b6e3-9bb6d98aa732/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://bits.twitch.tv",
                    "__typename": "Badge"
                },
                {
                    "id": "Yml0czsxMDAwOw==",
                    "setID": "bits",
                    "version": "1000",
                    "title": "cheer 1000",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/0d85a29e-79ad-4c63-a285-3acd2c66f2ba/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/0d85a29e-79ad-4c63-a285-3acd2c66f2ba/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/0d85a29e-79ad-4c63-a285-3acd2c66f2ba/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://bits.twitch.tv",
                    "__typename": "Badge"
                },
                {
                    "id": "Yml0czs2MDAwMDA7",
                    "setID": "bits",
                    "version": "600000",
                    "title": "cheer 600000",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/4d908059-f91c-4aef-9acb-634434f4c32e/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/4d908059-f91c-4aef-9acb-634434f4c32e/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/4d908059-f91c-4aef-9acb-634434f4c32e/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://bits.twitch.tv",
                    "__typename": "Badge"
                },
                {
                    "id": "Yml0czs0MDAwMDAwOw==",
                    "setID": "bits",
                    "version": "4000000",
                    "title": "cheer 4000000",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/79fe642a-87f3-40b1-892e-a341747b6e08/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/79fe642a-87f3-40b1-892e-a341747b6e08/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/79fe642a-87f3-40b1-892e-a341747b6e08/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://bits.twitch.tv",
                    "__typename": "Badge"
                },
                {
                    "id": "Yml0czsyNTAwMDs=",
                    "setID": "bits",
                    "version": "25000",
                    "title": "cheer 25000",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/64ca5920-c663-4bd8-bfb1-751b4caea2dd/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/64ca5920-c663-4bd8-bfb1-751b4caea2dd/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/64ca5920-c663-4bd8-bfb1-751b4caea2dd/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://bits.twitch.tv",
                    "__typename": "Badge"
                },
                {
                    "id": "Yml0czszNTAwMDAwOw==",
                    "setID": "bits",
                    "version": "3500000",
                    "title": "cheer 3500000",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/c3d218f5-1e45-419d-9c11-033a1ae54d3a/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/c3d218f5-1e45-419d-9c11-033a1ae54d3a/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/c3d218f5-1e45-419d-9c11-033a1ae54d3a/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://bits.twitch.tv",
                    "__typename": "Badge"
                },
                {
                    "id": "Yml0czs1MDAwOw==",
                    "setID": "bits",
                    "version": "5000",
                    "title": "cheer 5000",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/57cd97fc-3e9e-4c6d-9d41-60147137234e/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/57cd97fc-3e9e-4c6d-9d41-60147137234e/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/57cd97fc-3e9e-4c6d-9d41-60147137234e/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://bits.twitch.tv",
                    "__typename": "Badge"
                },
                {
                    "id": "Yml0czsxMDAwMDAwOw==",
                    "setID": "bits",
                    "version": "1000000",
                    "title": "cheer 1000000",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/494d1c8e-c3b2-4d88-8528-baff57c9bd3f/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/494d1c8e-c3b2-4d88-8528-baff57c9bd3f/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/494d1c8e-c3b2-4d88-8528-baff57c9bd3f/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://bits.twitch.tv",
                    "__typename": "Badge"
                },
                {
                    "id": "Yml0czsxMDA7",
                    "setID": "bits",
                    "version": "100",
                    "title": "cheer 100",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/09d93036-e7ce-431c-9a9e-7044297133f2/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/09d93036-e7ce-431c-9a9e-7044297133f2/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/09d93036-e7ce-431c-9a9e-7044297133f2/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://bits.twitch.tv",
                    "__typename": "Badge"
                },
                {
                    "id": "Yml0czs0NTAwMDAwOw==",
                    "setID": "bits",
                    "version": "4500000",
                    "title": "cheer 4500000",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/736d4156-ac67-4256-a224-3e6e915436db/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/736d4156-ac67-4256-a224-3e6e915436db/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/736d4156-ac67-4256-a224-3e6e915436db/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://bits.twitch.tv",
                    "__typename": "Badge"
                },
                {
                    "id": "Yml0czs3MDAwMDA7",
                    "setID": "bits",
                    "version": "700000",
                    "title": "cheer 700000",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/a1d2a824-f216-4b9f-9642-3de8ed370957/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/a1d2a824-f216-4b9f-9642-3de8ed370957/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/a1d2a824-f216-4b9f-9642-3de8ed370957/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://bits.twitch.tv",
                    "__typename": "Badge"
                },
                {
                    "id": "Yml0czsxMjUwMDAwOw==",
                    "setID": "bits",
                    "version": "1250000",
                    "title": "cheer 1250000",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/ce217209-4615-4bf8-81e3-57d06b8b9dc7/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/ce217209-4615-4bf8-81e3-57d06b8b9dc7/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/ce217209-4615-4bf8-81e3-57d06b8b9dc7/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://bits.twitch.tv",
                    "__typename": "Badge"
                },
                {
                    "id": "Yml0czs0MDAwMDA7",
                    "setID": "bits",
                    "version": "400000",
                    "title": "cheer 400000",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/a8f393af-76e6-4aa2-9dd0-7dcc1c34f036/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/a8f393af-76e6-4aa2-9dd0-7dcc1c34f036/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/a8f393af-76e6-4aa2-9dd0-7dcc1c34f036/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://bits.twitch.tv",
                    "__typename": "Badge"
                },
                {
                    "id": "Yml0czsyMDAwMDAwOw==",
                    "setID": "bits",
                    "version": "2000000",
                    "title": "cheer 2000000",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/7ea89c53-1a3b-45f9-9223-d97ae19089f2/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/7ea89c53-1a3b-45f9-9223-d97ae19089f2/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/7ea89c53-1a3b-45f9-9223-d97ae19089f2/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://bits.twitch.tv",
                    "__typename": "Badge"
                },
                {
                    "id": "Yml0czs4MDAwMDA7",
                    "setID": "bits",
                    "version": "800000",
                    "title": "cheer 800000",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/5ec2ee3e-5633-4c2a-8e77-77473fe409e6/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/5ec2ee3e-5633-4c2a-8e77-77473fe409e6/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/5ec2ee3e-5633-4c2a-8e77-77473fe409e6/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://bits.twitch.tv",
                    "__typename": "Badge"
                },
                {
                    "id": "Yml0czs1MDAwMDs=",
                    "setID": "bits",
                    "version": "50000",
                    "title": "cheer 50000",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/62310ba7-9916-4235-9eba-40110d67f85d/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/62310ba7-9916-4235-9eba-40110d67f85d/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/62310ba7-9916-4235-9eba-40110d67f85d/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://bits.twitch.tv",
                    "__typename": "Badge"
                },
                {
                    "id": "Yml0czszMDAwMDA7",
                    "setID": "bits",
                    "version": "300000",
                    "title": "cheer 300000",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/ac13372d-2e94-41d1-ae11-ecd677f69bb6/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/ac13372d-2e94-41d1-ae11-ecd677f69bb6/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/ac13372d-2e94-41d1-ae11-ecd677f69bb6/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://bits.twitch.tv",
                    "__typename": "Badge"
                },
                {
                    "id": "Yml0czsxNzUwMDAwOw==",
                    "setID": "bits",
                    "version": "1750000",
                    "title": "cheer 1750000",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/183f1fd8-aaf4-450c-a413-e53f839f0f82/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/183f1fd8-aaf4-450c-a413-e53f839f0f82/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/183f1fd8-aaf4-450c-a413-e53f839f0f82/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://bits.twitch.tv",
                    "__typename": "Badge"
                },
                {
                    "id": "Yml0czszMDAwMDAwOw==",
                    "setID": "bits",
                    "version": "3000000",
                    "title": "cheer 3000000",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/5671797f-5e9f-478c-a2b5-eb086c8928cf/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/5671797f-5e9f-478c-a2b5-eb086c8928cf/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/5671797f-5e9f-478c-a2b5-eb086c8928cf/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://bits.twitch.tv",
                    "__typename": "Badge"
                },
                {
                    "id": "Yml0czsyNTAwMDAwOw==",
                    "setID": "bits",
                    "version": "2500000",
                    "title": "cheer 2500000",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/cf061daf-d571-4811-bcc2-c55c8792bc8f/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/cf061daf-d571-4811-bcc2-c55c8792bc8f/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/cf061daf-d571-4811-bcc2-c55c8792bc8f/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://bits.twitch.tv",
                    "__typename": "Badge"
                },
                {
                    "id": "Yml0czs5MDAwMDA7",
                    "setID": "bits",
                    "version": "900000",
                    "title": "cheer 900000",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/088c58c6-7c38-45ba-8f73-63ef24189b84/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/088c58c6-7c38-45ba-8f73-63ef24189b84/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/088c58c6-7c38-45ba-8f73-63ef24189b84/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://bits.twitch.tv",
                    "__typename": "Badge"
                },
                {
                    "id": "Yml0czs3NTAwMDs=",
                    "setID": "bits",
                    "version": "75000",
                    "title": "cheer 75000",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/ce491fa4-b24f-4f3b-b6ff-44b080202792/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/ce491fa4-b24f-4f3b-b6ff-44b080202792/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/ce491fa4-b24f-4f3b-b6ff-44b080202792/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://bits.twitch.tv",
                    "__typename": "Badge"
                },
                {
                    "id": "Yml0czs1MDAwMDAwOw==",
                    "setID": "bits",
                    "version": "5000000",
                    "title": "cheer 5000000",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/3f085f85-8d15-4a03-a829-17fca7bf1bc2/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/3f085f85-8d15-4a03-a829-17fca7bf1bc2/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/3f085f85-8d15-4a03-a829-17fca7bf1bc2/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://bits.twitch.tv",
                    "__typename": "Badge"
                },
                {
                    "id": "Yml0czsxNTAwMDAwOw==",
                    "setID": "bits",
                    "version": "1500000",
                    "title": "cheer 1500000",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/c4eba5b4-17a7-40a1-a668-bc1972c1e24d/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/c4eba5b4-17a7-40a1-a668-bc1972c1e24d/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/c4eba5b4-17a7-40a1-a668-bc1972c1e24d/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://bits.twitch.tv",
                    "__typename": "Badge"
                },
                {
                    "id": "Yml0cy1jaGFyaXR5OzE7",
                    "setID": "bits-charity",
                    "version": "1",
                    "title": "Direct Relief - Charity 2018",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/a539dc18-ae19-49b0-98c4-8391a594332b/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/a539dc18-ae19-49b0-98c4-8391a594332b/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/a539dc18-ae19-49b0-98c4-8391a594332b/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://link.twitch.tv/blizzardofbits",
                    "__typename": "Badge"
                },
                {
                    "id": "Yml0cy1sZWFkZXI7MTs=",
                    "setID": "bits-leader",
                    "version": "1",
                    "title": "ビッツリーダー1",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/8bedf8c3-7a6d-4df2-b62f-791b96a5dd31/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/8bedf8c3-7a6d-4df2-b62f-791b96a5dd31/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/8bedf8c3-7a6d-4df2-b62f-791b96a5dd31/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://bits.twitch.tv",
                    "__typename": "Badge"
                },
                {
                    "id": "Yml0cy1sZWFkZXI7Mzs=",
                    "setID": "bits-leader",
                    "version": "3",
                    "title": "ビッツリーダー3",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/f1d2aab6-b647-47af-965b-84909cf303aa/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/f1d2aab6-b647-47af-965b-84909cf303aa/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/f1d2aab6-b647-47af-965b-84909cf303aa/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://bits.twitch.tv",
                    "__typename": "Badge"
                },
                {
                    "id": "Yml0cy1sZWFkZXI7Mjs=",
                    "setID": "bits-leader",
                    "version": "2",
                    "title": "ビッツリーダー2",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/f04baac7-9141-4456-a0e7-6301bcc34138/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/f04baac7-9141-4456-a0e7-6301bcc34138/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/f04baac7-9141-4456-a0e7-6301bcc34138/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://bits.twitch.tv",
                    "__typename": "Badge"
                },
                {
                    "id": "YmxhY2stb3BzLTctZ2xvYmFsLWxhdW5jaDsxOw==",
                    "setID": "black-ops-7-global-launch",
                    "version": "1",
                    "title": "Black Ops 7 グローバルリリース",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/e225aad6-3780-4bdc-ae38-48d3ab7dc36e/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/e225aad6-3780-4bdc-ae38-48d3ab7dc36e/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/e225aad6-3780-4bdc-ae38-48d3ab7dc36e/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "Ym9yZGVybGFuZHMtNC1iYWRnZS0tLXJpcHBlcjsxOw==",
                    "setID": "borderlands-4-badge---ripper",
                    "version": "1",
                    "title": "ボーダーランズ4バッジ - リッパー",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/098219cb-48d8-4945-96a6-80594c7a90dd/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/098219cb-48d8-4945-96a6-80594c7a90dd/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/098219cb-48d8-4945-96a6-80594c7a90dd/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "Ym9yZGVybGFuZHMtNC1iYWRnZS0tLXZhdWx0LXN5bWJvbDsxOw==",
                    "setID": "borderlands-4-badge---vault-symbol",
                    "version": "1",
                    "title": "ボーダーランズ4バッジ - Vault シンボル",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/97eee27d-c87f-4afb-a020-04a9d04456df/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/97eee27d-c87f-4afb-a020-04a9d04456df/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/97eee27d-c87f-4afb-a020-04a9d04456df/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "Ym90LWJhZGdlOzE7",
                    "setID": "bot-badge",
                    "version": "1",
                    "title": "チャットボット",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/3ffa9565-c35b-4cad-800b-041e60659cf2/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/3ffa9565-c35b-4cad-800b-041e60659cf2/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/3ffa9565-c35b-4cad-800b-041e60659cf2/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "YnJhd2xoYWxsYV8xOzE7",
                    "setID": "brawlhalla_1",
                    "version": "1",
                    "title": "Brawlhalla",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/bf6d6579-ab02-4f0a-9f64-a51c37040858/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/bf6d6579-ab02-4f0a-9f64-a51c37040858/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/bf6d6579-ab02-4f0a-9f64-a51c37040858/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/Brawlhalla/details",
                    "__typename": "Badge"
                },
                {
                    "id": "YnJvYWRjYXN0ZXI7MTs=",
                    "setID": "broadcaster",
                    "version": "1",
                    "title": "ストリーマー",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/5527c58c-fb7d-422d-b71b-f309dcb85cc1/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/5527c58c-fb7d-422d-b71b-f309dcb85cc1/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/5527c58c-fb7d-422d-b71b-f309dcb85cc1/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "YnJva2VuLWFnZV8xOzE7",
                    "setID": "broken-age_1",
                    "version": "1",
                    "title": "Broken Age",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/56885ed2-9a09-4c8e-8131-3eb9ec15af94/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/56885ed2-9a09-4c8e-8131-3eb9ec15af94/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/56885ed2-9a09-4c8e-8131-3eb9ec15af94/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/Broken%20Age/details",
                    "__typename": "Badge"
                },
                {
                    "id": "YnVic3ktdGhlLXdvb2xpZXNfMTsxOw==",
                    "setID": "bubsy-the-woolies_1",
                    "version": "1",
                    "title": "Bubsy: The Woolies Strike Back",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/c8129382-1f4e-4d15-a8d2-48bdddba9b81/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/c8129382-1f4e-4d15-a8d2-48bdddba9b81/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/c8129382-1f4e-4d15-a8d2-48bdddba9b81/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/Bubsy:%20The%20Woolies%20Strike%20Back/details",
                    "__typename": "Badge"
                },
                {
                    "id": "Y2hhdHRlci1jcy1nby0yMDIyOzE7",
                    "setID": "chatter-cs-go-2022",
                    "version": "1",
                    "title": "CS:GO Week Brazil 2022",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/57b6bd6b-a1b5-4204-9e6c-eb8ed5831603/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/57b6bd6b-a1b5-4204-9e6c-eb8ed5831603/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/57b6bd6b-a1b5-4204-9e6c-eb8ed5831603/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "Y2xpcC1jaGFtcDsxOw==",
                    "setID": "clip-champ",
                    "version": "1",
                    "title": "Power Clipper",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/f38976e0-ffc9-11e7-86d6-7f98b26a9d79/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/f38976e0-ffc9-11e7-86d6-7f98b26a9d79/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/f38976e0-ffc9-11e7-86d6-7f98b26a9d79/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://help.twitch.tv/customer/portal/articles/2918323-clip-champs-guide",
                    "__typename": "Badge"
                },
                {
                    "id": "Y2xpcC10aGUtaGFsbHM7MTs=",
                    "setID": "clip-the-halls",
                    "version": "1",
                    "title": "ツリーにクリップを飾り付けよう",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/ce9e266a-f490-4fb2-9989-aee20036bfa5/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/ce9e266a-f490-4fb2-9989-aee20036bfa5/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/ce9e266a-f490-4fb2-9989-aee20036bfa5/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://blog.twitch.tv/en/2024/12/02/twitch-holiday-hoopla/",
                    "__typename": "Badge"
                },
                {
                    "id": "Y2xpcHMtbGVhZGVyOzM7",
                    "setID": "clips-leader",
                    "version": "3",
                    "title": "クリップリーダー3",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/fb838633-6ff6-46df-98b4-9e53fcff84f6/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/fb838633-6ff6-46df-98b4-9e53fcff84f6/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/fb838633-6ff6-46df-98b4-9e53fcff84f6/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "Y2xpcHMtbGVhZGVyOzI7",
                    "setID": "clips-leader",
                    "version": "2",
                    "title": "クリップリーダー2",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/9eddf7ab-aa46-4798-abe2-710db1043254/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/9eddf7ab-aa46-4798-abe2-710db1043254/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/9eddf7ab-aa46-4798-abe2-710db1043254/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "Y2xpcHMtbGVhZGVyOzE7",
                    "setID": "clips-leader",
                    "version": "1",
                    "title": "クリップリーダー1",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/12f70951-efea-48c2-b42b-d5e2ea0d71f7/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/12f70951-efea-48c2-b42b-d5e2ea0d71f7/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/12f70951-efea-48c2-b42b-d5e2ea0d71f7/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "Y3JlYXRvci1jcy1nby0yMDIyOzE7",
                    "setID": "creator-cs-go-2022",
                    "version": "1",
                    "title": "CS:GO Week Brazil 2022",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/a2ea6df9-ac0a-4956-bfe9-e931f50b94fa/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/a2ea6df9-ac0a-4956-bfe9-e931f50b94fa/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/a2ea6df9-ac0a-4956-bfe9-e931f50b94fa/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "Y3VwaGVhZF8xOzE7",
                    "setID": "cuphead_1",
                    "version": "1",
                    "title": "Cuphead",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/4384659a-a2e3-11e7-a564-87f6b1288bab/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/4384659a-a2e3-11e7-a564-87f6b1288bab/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/4384659a-a2e3-11e7-a564-87f6b1288bab/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/Cuphead/details",
                    "__typename": "Badge"
                },
                {
                    "id": "ZGFya2VzdC1kdW5nZW9uXzE7MTs=",
                    "setID": "darkest-dungeon_1",
                    "version": "1",
                    "title": "Darkest Dungeon",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/52a98ddd-cc79-46a8-9fe3-30f8c719bc2d/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/52a98ddd-cc79-46a8-9fe3-30f8c719bc2d/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/52a98ddd-cc79-46a8-9fe3-30f8c719bc2d/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/Darkest%20Dungeon/details",
                    "__typename": "Badge"
                },
                {
                    "id": "ZGVjZWl0XzE7MTs=",
                    "setID": "deceit_1",
                    "version": "1",
                    "title": "Deceit",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/b14fef48-4ff9-4063-abf6-579489234fe9/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/b14fef48-4ff9-4063-abf6-579489234fe9/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/b14fef48-4ff9-4063-abf6-579489234fe9/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/Deceit/details",
                    "__typename": "Badge"
                },
                {
                    "id": "ZGVzdGlueS0yLWZpbmFsLXNoYXBlLXJhaWQtcmFjZTsxOw==",
                    "setID": "destiny-2-final-shape-raid-race",
                    "version": "1",
                    "title": "Destiny 2: The Final Shape Raid Race",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/e79ee64f-31f1-4485-9c81-b93957e69f8a/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/e79ee64f-31f1-4485-9c81-b93957e69f8a/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/e79ee64f-31f1-4485-9c81-b93957e69f8a/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/category/destiny-2",
                    "__typename": "Badge"
                },
                {
                    "id": "ZGVzdGlueS0yLXRoZS1maW5hbC1zaGFwZS1zdHJlYW1lcjsxOw==",
                    "setID": "destiny-2-the-final-shape-streamer",
                    "version": "1",
                    "title": "Destiny 2: The Final Shape ストリーマー",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/b1bcaf3c-d7a2-442b-b407-03f2b8ff624d/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/b1bcaf3c-d7a2-442b-b407-03f2b8ff624d/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/b1bcaf3c-d7a2-442b-b407-03f2b8ff624d/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/category/destiny-2\t",
                    "__typename": "Badge"
                },
                {
                    "id": "ZGV2aWwtbWF5LWNyeS1oZF8xOzE7",
                    "setID": "devil-may-cry-hd_1",
                    "version": "1",
                    "title": "Devil May Cry HD Collection",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/633877d4-a91c-4c36-b75b-803f82b1352f/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/633877d4-a91c-4c36-b75b-803f82b1352f/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/633877d4-a91c-4c36-b75b-803f82b1352f/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/Devil%20May%20Cry%20HD%20Collection/details",
                    "__typename": "Badge"
                },
                {
                    "id": "ZGV2aWwtbWF5LWNyeS1oZF8yOzE7",
                    "setID": "devil-may-cry-hd_2",
                    "version": "1",
                    "title": "Devil May Cry HD Collection",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/408548fe-aa74-4d53-b5e9-960103d9b865/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/408548fe-aa74-4d53-b5e9-960103d9b865/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/408548fe-aa74-4d53-b5e9-960103d9b865/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/Devil%20May%20Cry%20HD%20Collection/details",
                    "__typename": "Badge"
                },
                {
                    "id": "ZGV2aWwtbWF5LWNyeS1oZF8zOzE7",
                    "setID": "devil-may-cry-hd_3",
                    "version": "1",
                    "title": "Devil May Cry HD Collection",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/df84c5bf-8d66-48e2-b9fb-c014cc9b3945/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/df84c5bf-8d66-48e2-b9fb-c014cc9b3945/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/df84c5bf-8d66-48e2-b9fb-c014cc9b3945/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/Devil%20May%20Cry%20HD%20Collection/details",
                    "__typename": "Badge"
                },
                {
                    "id": "ZGV2aWwtbWF5LWNyeS1oZF80OzE7",
                    "setID": "devil-may-cry-hd_4",
                    "version": "1",
                    "title": "Devil May Cry HD Collection",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/af836b94-8ffd-4c0a-b7d8-a92fad5e3015/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/af836b94-8ffd-4c0a-b7d8-a92fad5e3015/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/af836b94-8ffd-4c0a-b7d8-a92fad5e3015/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/Devil%20May%20Cry%20HD%20Collection/details",
                    "__typename": "Badge"
                },
                {
                    "id": "ZGV2aWxpYW5fMTsxOw==",
                    "setID": "devilian_1",
                    "version": "1",
                    "title": "Devilian",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/3cb92b57-1eef-451c-ac23-4d748128b2c5/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/3cb92b57-1eef-451c-ac23-4d748128b2c5/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/3cb92b57-1eef-451c-ac23-4d748128b2c5/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/Devilian/details",
                    "__typename": "Badge"
                },
                {
                    "id": "ZGlhbmE7MTs=",
                    "setID": "diana",
                    "version": "1",
                    "title": "Diana",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/38801cc6-4d01-40f6-8949-6b9f9d5334b8/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/38801cc6-4d01-40f6-8949-6b9f9d5334b8/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/38801cc6-4d01-40f6-8949-6b9f9d5334b8/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "ZHJhZ29uc2NpbW15OzE7",
                    "setID": "dragonscimmy",
                    "version": "1",
                    "title": "DragonScimmy",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/da4c6554-4472-4949-b33b-e79164dbba0b/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/da4c6554-4472-4949-b33b-e79164dbba0b/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/da4c6554-4472-4949-b33b-e79164dbba0b/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "ZHJlYW1jb24tMjAyNDsxOw==",
                    "setID": "dreamcon-2024",
                    "version": "1",
                    "title": "Dream Con 2024",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/5dfbd056-8ac1-407f-bdf3-f83183fa97ae/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/5dfbd056-8ac1-407f-bdf3-f83183fa97ae/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/5dfbd056-8ac1-407f-bdf3-f83183fa97ae/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "ZHVlbHlzdF8xOzE7",
                    "setID": "duelyst_1",
                    "version": "1",
                    "title": "Duelyst",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/7d9c12f4-a2ac-4e88-8026-d1a330468282/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/7d9c12f4-a2ac-4e88-8026-d1a330468282/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/7d9c12f4-a2ac-4e88-8026-d1a330468282/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/Duelyst/details",
                    "__typename": "Badge"
                },
                {
                    "id": "ZHVlbHlzdF8yOzE7",
                    "setID": "duelyst_2",
                    "version": "1",
                    "title": "Duelyst",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/1938acd3-2d18-471d-b1af-44f2047c033c/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/1938acd3-2d18-471d-b1af-44f2047c033c/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/1938acd3-2d18-471d-b1af-44f2047c033c/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/Duelyst/details",
                    "__typename": "Badge"
                },
                {
                    "id": "ZHVlbHlzdF8zOzE7",
                    "setID": "duelyst_3",
                    "version": "1",
                    "title": "Duelyst",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/344c07fc-1632-47c6-9785-e62562a6b760/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/344c07fc-1632-47c6-9785-e62562a6b760/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/344c07fc-1632-47c6-9785-e62562a6b760/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/Duelyst/details",
                    "__typename": "Badge"
                },
                {
                    "id": "ZHVlbHlzdF80OzE7",
                    "setID": "duelyst_4",
                    "version": "1",
                    "title": "Duelyst",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/39e717a8-00bc-49cc-b6d4-3ea91ee1be25/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/39e717a8-00bc-49cc-b6d4-3ea91ee1be25/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/39e717a8-00bc-49cc-b6d4-3ea91ee1be25/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/Duelyst/details",
                    "__typename": "Badge"
                },
                {
                    "id": "ZHVlbHlzdF81OzE7",
                    "setID": "duelyst_5",
                    "version": "1",
                    "title": "Duelyst",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/290419b6-484a-47da-ad14-a99d6581f758/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/290419b6-484a-47da-ad14-a99d6581f758/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/290419b6-484a-47da-ad14-a99d6581f758/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/Duelyst/details",
                    "__typename": "Badge"
                },
                {
                    "id": "ZHVlbHlzdF82OzE7",
                    "setID": "duelyst_6",
                    "version": "1",
                    "title": "Duelyst",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/c5e54a4b-0bf1-463a-874a-38524579aed0/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/c5e54a4b-0bf1-463a-874a-38524579aed0/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/c5e54a4b-0bf1-463a-874a-38524579aed0/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/Duelyst/details",
                    "__typename": "Badge"
                },
                {
                    "id": "ZHVlbHlzdF83OzE7",
                    "setID": "duelyst_7",
                    "version": "1",
                    "title": "Duelyst",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/cf508179-3183-4987-97e0-56ca44babb9f/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/cf508179-3183-4987-97e0-56ca44babb9f/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/cf508179-3183-4987-97e0-56ca44babb9f/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/Duelyst/details",
                    "__typename": "Badge"
                },
                {
                    "id": "ZWxkZW4tcmluZy1yZWNsdXNlOzE7",
                    "setID": "elden-ring-recluse",
                    "version": "1",
                    "title": "Elden Ring SuperFanバッジ - Recluse",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/5afadc6c-933b-4ede-b318-3752bbf267a9/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/5afadc6c-933b-4ede-b318-3752bbf267a9/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/5afadc6c-933b-4ede-b318-3752bbf267a9/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "ZWxkZW4tcmluZy13eWxkZXI7MTs=",
                    "setID": "elden-ring-wylder",
                    "version": "1",
                    "title": "Elden Ring Nightreignクリップバッジ - Wylder",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/5d5ab328-0916-4655-90b9-78b983ca4262/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/5d5ab328-0916-4655-90b9-78b983ca4262/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/5d5ab328-0916-4655-90b9-78b983ca4262/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "ZW50ZXItdGhlLWd1bmdlb25fMTsxOw==",
                    "setID": "enter-the-gungeon_1",
                    "version": "1",
                    "title": "Enter The Gungeon",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/53c9af0b-84f6-4f9d-8c80-4bc51321a37d/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/53c9af0b-84f6-4f9d-8c80-4bc51321a37d/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/53c9af0b-84f6-4f9d-8c80-4bc51321a37d/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/Enter%20the%20Gungeon/details",
                    "__typename": "Badge"
                },
                {
                    "id": "ZXNvXzE7MTs=",
                    "setID": "eso_1",
                    "version": "1",
                    "title": "Elder Scrolls Online",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/18647a68-a35f-48d7-bf97-ae5deb6b277d/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/18647a68-a35f-48d7-bf97-ae5deb6b277d/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/18647a68-a35f-48d7-bf97-ae5deb6b277d/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "ZXZvLTIwMjU7MTs=",
                    "setID": "evo-2025",
                    "version": "1",
                    "title": "Evo 2025",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/1469e9cf-14d9-4a48-a91c-81712d027439/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/1469e9cf-14d9-4a48-a91c-81712d027439/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/1469e9cf-14d9-4a48-a91c-81712d027439/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "ZXh0ZW5zaW9uOzE7",
                    "setID": "extension",
                    "version": "1",
                    "title": "拡張機能",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/ea8b0f8c-aa27-11e8-ba0c-1370ffff3854/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/ea8b0f8c-aa27-11e8-ba0c-1370ffff3854/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/ea8b0f8c-aa27-11e8-ba0c-1370ffff3854/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "ZmFsbG91dC1zZWFzb24tMi1naG91bDsxOw==",
                    "setID": "fallout-season-2-ghoul",
                    "version": "1",
                    "title": "Fallout Season 2 Ghoul",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/815334c4-3123-489b-8854-2af0f4027b00/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/815334c4-3123-489b-8854-2af0f4027b00/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/815334c4-3123-489b-8854-2af0f4027b00/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "ZmlyZXdhdGNoXzE7MTs=",
                    "setID": "firewatch_1",
                    "version": "1",
                    "title": "Firewatch",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/b6bf4889-4902-49e2-9658-c0132e71c9c4/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/b6bf4889-4902-49e2-9658-c0132e71c9c4/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/b6bf4889-4902-49e2-9658-c0132e71c9c4/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/Firewatch/details",
                    "__typename": "Badge"
                },
                {
                    "id": "Zm91bmRlcjswOw==",
                    "setID": "founder",
                    "version": "0",
                    "title": "ファウンダー",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/511b78a9-ab37-472f-9569-457753bbe7d3/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/511b78a9-ab37-472f-9569-457753bbe7d3/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/511b78a9-ab37-472f-9569-457753bbe7d3/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://help.twitch.tv/s/article/founders-badge",
                    "__typename": "Badge"
                },
                {
                    "id": "ZnJpZ2h0LWZlc3QtMjAyNTsxOw==",
                    "setID": "fright-fest-2025",
                    "version": "1",
                    "title": "Fright Fest 2025",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/2ef2cd27-2210-4640-bbf8-69b5c4d9e302/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/2ef2cd27-2210-4640-bbf8-69b5c4d9e302/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/2ef2cd27-2210-4640-bbf8-69b5c4d9e302/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://blog.twitch.tv/en/2025/10/20/twitch-fright-fest-2025/",
                    "__typename": "Badge"
                },
                {
                    "id": "ZnJvZy1sYW50ZXJuOzE7",
                    "setID": "frog-lantern",
                    "version": "1",
                    "title": "カエルのランタン",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/dfc75f94-14f9-404b-b953-37eba481df37/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/dfc75f94-14f9-404b-b953-37eba481df37/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/dfc75f94-14f9-404b-b953-37eba481df37/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "ZnJvemVuLWNvcnRleHRfMTsxOw==",
                    "setID": "frozen-cortext_1",
                    "version": "1",
                    "title": "Frozen Cortext",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/2015f087-01b5-4a01-a2bb-ecb4d6be5240/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/2015f087-01b5-4a01-a2bb-ecb4d6be5240/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/2015f087-01b5-4a01-a2bb-ecb4d6be5240/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/Frozen%20Cortex/details",
                    "__typename": "Badge"
                },
                {
                    "id": "ZnJvemVuLXN5bmFwc2VfMTsxOw==",
                    "setID": "frozen-synapse_1",
                    "version": "1",
                    "title": "Frozen Synapse",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/d4bd464d-55ea-4238-a11d-744f034e2375/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/d4bd464d-55ea-4238-a11d-744f034e2375/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/d4bd464d-55ea-4238-a11d-744f034e2375/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/Frozen%20Synapse/details",
                    "__typename": "Badge"
                },
                {
                    "id": "Z2FtZS1kZXZlbG9wZXI7MTs=",
                    "setID": "game-developer",
                    "version": "1",
                    "title": "Game Developer",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/85856a4a-eb7d-4e26-a43e-d204a977ade4/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/85856a4a-eb7d-4e26-a43e-d204a977ade4/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/85856a4a-eb7d-4e26-a43e-d204a977ade4/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "Z2FtZXJkdW87MTs=",
                    "setID": "gamerduo",
                    "version": "1",
                    "title": "GamerDuo",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/be750d4d-a3b9-4116-ae75-6ee4f3294a19/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/be750d4d-a3b9-4116-ae75-6ee4f3294a19/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/be750d4d-a3b9-4116-ae75-6ee4f3294a19/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://blog.twitch.tv/2025/10/02/sub-for-super-duolingo/",
                    "__typename": "Badge"
                },
                {
                    "id": "Z2VhcnMtb2Ytd2FyLXN1cGVyZmFuLWJhZGdlOzE7",
                    "setID": "gears-of-war-superfan-badge",
                    "version": "1",
                    "title": "Gears of Warスーパーファンバッジ",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/18b92728-aa7a-4e24-acb5-b14ea17c8b2b/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/18b92728-aa7a-4e24-acb5-b14ea17c8b2b/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/18b92728-aa7a-4e24-acb5-b14ea17c8b2b/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "Z2V0dGluZy1vdmVyLWl0XzE7MTs=",
                    "setID": "getting-over-it_1",
                    "version": "1",
                    "title": "Getting Over It",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/8d4e178c-81ec-4c71-af68-745b40733984/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/8d4e178c-81ec-4c71-af68-745b40733984/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/8d4e178c-81ec-4c71-af68-745b40733984/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/Getting%20Over%20It/details",
                    "__typename": "Badge"
                },
                {
                    "id": "Z2V0dGluZy1vdmVyLWl0XzI7MTs=",
                    "setID": "getting-over-it_2",
                    "version": "1",
                    "title": "Getting Over It",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/bb620b42-e0e1-4373-928e-d4a732f99ccb/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/bb620b42-e0e1-4373-928e-d4a732f99ccb/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/bb620b42-e0e1-4373-928e-d4a732f99ccb/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/Getting%20Over%20It/details",
                    "__typename": "Badge"
                },
                {
                    "id": "Z2luZ2tvLWxlYWY7MTs=",
                    "setID": "gingko-leaf",
                    "version": "1",
                    "title": "イチョウの葉",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/394abbbd-cc1d-427f-bc00-bce294353448/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/394abbbd-cc1d-427f-bc00-bce294353448/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/394abbbd-cc1d-427f-bc00-bce294353448/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "Z2xoZi1wbGVkZ2U7MTs=",
                    "setID": "glhf-pledge",
                    "version": "1",
                    "title": "GLHF Pledge",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/3158e758-3cb4-43c5-94b3-7639810451c5/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/3158e758-3cb4-43c5-94b3-7639810451c5/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/3158e758-3cb4-43c5-94b3-7639810451c5/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.anykey.org/pledge",
                    "__typename": "Badge"
                },
                {
                    "id": "Z2xpdGNoY29uMjAyMDsxOw==",
                    "setID": "glitchcon2020",
                    "version": "1",
                    "title": "GlitchCon 2020",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/1d4b03b9-51ea-42c9-8f29-698e3c85be3d/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/1d4b03b9-51ea-42c9-8f29-698e3c85be3d/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/1d4b03b9-51ea-42c9-8f29-698e3c85be3d/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitchcon.com/",
                    "__typename": "Badge"
                },
                {
                    "id": "Z2xvYmFsX21vZDsxOw==",
                    "setID": "global_mod",
                    "version": "1",
                    "title": "グローバルモデレーター",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/9384c43e-4ce7-4e94-b2a1-b93656896eba/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/9384c43e-4ce7-4e94-b2a1-b93656896eba/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/9384c43e-4ce7-4e94-b2a1-b93656896eba/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "Z29sZC1waXhlbC1oZWFydDsxOw==",
                    "setID": "gold-pixel-heart",
                    "version": "1",
                    "title": "ゴールド・ピクセルハート",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/1687873b-cf38-412c-aad3-f9a4ce17f8b6/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/1687873b-cf38-412c-aad3-f9a4ce17f8b6/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/1687873b-cf38-412c-aad3-f9a4ce17f8b6/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://help.twitch.tv/s/article/twitch-charity",
                    "__typename": "Badge"
                },
                {
                    "id": "Z29sZC1waXhlbC1oZWFydC0tLXRvZ2V0aGVyLWZvci1nb29kLTI0OzE7",
                    "setID": "gold-pixel-heart---together-for-good-24",
                    "version": "1",
                    "title": "ゴールドのピクセルハート - Together For Good '24",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/52c90eac-b7ec-4e24-b500-8fceecfe91e8/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/52c90eac-b7ec-4e24-b500-8fceecfe91e8/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/52c90eac-b7ec-4e24-b500-8fceecfe91e8/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "Z29uZS1iYW5hbmFzOzE7",
                    "setID": "gone-bananas",
                    "version": "1",
                    "title": "「Gone Bananas」バッジ",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/e2ba99f4-6079-44d1-8c07-4ca6b58de61f/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/e2ba99f4-6079-44d1-8c07-4ca6b58de61f/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/e2ba99f4-6079-44d1-8c07-4ca6b58de61f/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "http://blog.twitch.tv/2025/04/01/april-fools-day/",
                    "__typename": "Badge"
                },
                {
                    "id": "Z3AtZXhwbG9yZXItMzsxOw==",
                    "setID": "gp-explorer-3",
                    "version": "1",
                    "title": "GP Explorer 3",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/1e3b6965-2224-44d1-a67a-6d186c1fb17d/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/1e3b6965-2224-44d1-a67a-6d186c1fb17d/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/1e3b6965-2224-44d1-a67a-6d186c1fb17d/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "aGVhdnktYnVsbGV0c18xOzE7",
                    "setID": "heavy-bullets_1",
                    "version": "1",
                    "title": "Heavy Bullets",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/fc83b76b-f8b2-4519-9f61-6faf84eef4cd/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/fc83b76b-f8b2-4519-9f61-6faf84eef4cd/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/fc83b76b-f8b2-4519-9f61-6faf84eef4cd/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/Heavy%20Bullets/details",
                    "__typename": "Badge"
                },
                {
                    "id": "aGVsbG9fbmVpZ2hib3JfMTsxOw==",
                    "setID": "hello_neighbor_1",
                    "version": "1",
                    "title": "Hello Neighbor",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/030cab2c-5d14-11e7-8d91-43a5a4306286/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/030cab2c-5d14-11e7-8d91-43a5a4306286/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/030cab2c-5d14-11e7-8d91-43a5a4306286/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/Hello%20Neighbor/details",
                    "__typename": "Badge"
                },
                {
                    "id": "aG9ybmV0OzE7",
                    "setID": "hornet",
                    "version": "1",
                    "title": "Hornet",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/4dc7b047-8c59-4522-97f2-24fb63147f56/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/4dc7b047-8c59-4522-97f2-24fb63147f56/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/4dc7b047-8c59-4522-97f2-24fb63147f56/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "aHVudC1jcm9zc2VzOzE7",
                    "setID": "hunt-crosses",
                    "version": "1",
                    "title": "Hunt Crosses",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/b1e77273-2fc0-4d36-873a-67a7c1647efe/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/b1e77273-2fc0-4d36-873a-67a7c1647efe/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/b1e77273-2fc0-4d36-873a-67a7c1647efe/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "aHlwZS10cmFpbjsyOw==",
                    "setID": "hype-train",
                    "version": "2",
                    "title": "ハイプトレインの元車掌",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/9c8d038a-3a29-45ea-96d4-5031fb1a7a81/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/9c8d038a-3a29-45ea-96d4-5031fb1a7a81/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/9c8d038a-3a29-45ea-96d4-5031fb1a7a81/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://help.twitch.tv/s/article/hype-train-guide",
                    "__typename": "Badge"
                },
                {
                    "id": "aHlwZS10cmFpbjsxOw==",
                    "setID": "hype-train",
                    "version": "1",
                    "title": "ハイプトレインの現車掌",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/fae4086c-3190-44d4-83c8-8ef0cbe1a515/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/fae4086c-3190-44d4-83c8-8ef0cbe1a515/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/fae4086c-3190-44d4-83c8-8ef0cbe1a515/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://help.twitch.tv/s/article/hype-train-guide",
                    "__typename": "Badge"
                },
                {
                    "id": "aW5uZXJzcGFjZV8xOzE7",
                    "setID": "innerspace_1",
                    "version": "1",
                    "title": "Innerspace",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/97532ccd-6a07-42b5-aecf-3458b6b3ebea/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/97532ccd-6a07-42b5-aecf-3458b6b3ebea/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/97532ccd-6a07-42b5-aecf-3458b6b3ebea/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/Innerspace/details",
                    "__typename": "Badge"
                },
                {
                    "id": "aW5uZXJzcGFjZV8yOzE7",
                    "setID": "innerspace_2",
                    "version": "1",
                    "title": "Innerspace",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/fc7d6018-657a-40e4-9246-0acdc85886d1/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/fc7d6018-657a-40e4-9246-0acdc85886d1/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/fc7d6018-657a-40e4-9246-0acdc85886d1/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/Innerspace/details",
                    "__typename": "Badge"
                },
                {
                    "id": "amFja2JveC1wYXJ0eS1wYWNrXzE7MTs=",
                    "setID": "jackbox-party-pack_1",
                    "version": "1",
                    "title": "Jackbox Party Pack",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/0f964fc1-f439-485f-a3c0-905294ee70e8/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/0f964fc1-f439-485f-a3c0-905294ee70e8/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/0f964fc1-f439-485f-a3c0-905294ee70e8/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/The%20Jackbox%20Party%20Pack/details",
                    "__typename": "Badge"
                },
                {
                    "id": "amVmZi10aGUtbGFuZC1zaGFyazsxOw==",
                    "setID": "jeff-the-land-shark",
                    "version": "1",
                    "title": "陸ザメのジェフ",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/ad51bd4c-2621-4d7c-8580-89243002bc8b/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/ad51bd4c-2621-4d7c-8580-89243002bc8b/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/ad51bd4c-2621-4d7c-8580-89243002bc8b/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "azRzZW4tY29uLTIwMjU7MTs=",
                    "setID": "k4sen-con-2025",
                    "version": "1",
                    "title": "The K4SEN Con 2025",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/6ea537aa-bb9a-4410-a330-5820b7dd8a24/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/6ea537aa-bb9a-4410-a330-5820b7dd8a24/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/6ea537aa-bb9a-4410-a330-5820b7dd8a24/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "a2luZ2RvbS1uZXctbGFuZHNfMTsxOw==",
                    "setID": "kingdom-new-lands_1",
                    "version": "1",
                    "title": "Kingdom: New Lands",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/e3c2a67e-ef80-4fe3-ae41-b933cd11788a/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/e3c2a67e-ef80-4fe3-ae41-b933cd11788a/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/e3c2a67e-ef80-4fe3-ae41-b933cd11788a/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/Kingdom:%20New%20Lands/details",
                    "__typename": "Badge"
                },
                {
                    "id": "bGEtdmVsYWRhLWl2OzE7",
                    "setID": "la-velada-iv",
                    "version": "1",
                    "title": "La Velada del Año IV",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/655dac77-0b2f-4b62-8871-6ae21f82b34e/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/655dac77-0b2f-4b62-8871-6ae21f82b34e/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/655dac77-0b2f-4b62-8871-6ae21f82b34e/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "bGEtdmVsYWRhLXYtYmFkZ2U7MTs=",
                    "setID": "la-velada-v-badge",
                    "version": "1",
                    "title": "La Velada Vバッジ",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/3f728095-b84d-4e7e-9eee-541ea02ddea0/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/3f728095-b84d-4e7e-9eee-541ea02ddea0/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/3f728095-b84d-4e7e-9eee-541ea02ddea0/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "bGVhZF9tb2RlcmF0b3I7MTs=",
                    "setID": "lead_moderator",
                    "version": "1",
                    "title": "モデレーター責任者",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/0822047b-65e0-46f2-94a9-d1091d685d33/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/0822047b-65e0-46f2-94a9-d1091d685d33/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/0822047b-65e0-46f2-94a9-d1091d685d33/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "bGVhZ3VlLW9mLWxlZ2VuZHMtbWlkLXNlYXNvbi1pbnZpdGF0aW9uYWwtMjAyNS0tLWdyZXk7MTs=",
                    "setID": "league-of-legends-mid-season-invitational-2025---grey",
                    "version": "1",
                    "title": "League of Legends Mid Season Invitational 2025 ストリーマー応援キャンペーン",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/18a0b4ba-5f62-4e94-b6f1-481731608602/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/18a0b4ba-5f62-4e94-b6f1-481731608602/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/18a0b4ba-5f62-4e94-b6f1-481731608602/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "bGVhZ3VlLW9mLWxlZ2VuZHMtbWlkLXNlYXNvbi1pbnZpdGF0aW9uYWwtMjAyNS0tLXB1cnBsZTsxOw==",
                    "setID": "league-of-legends-mid-season-invitational-2025---purple",
                    "version": "1",
                    "title": "League of Legends Mid Season Invitational 2025",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/0a99ba23-5e1a-46b7-8ff2-efbb9a6ea54c/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/0a99ba23-5e1a-46b7-8ff2-efbb9a6ea54c/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/0a99ba23-5e1a-46b7-8ff2-efbb9a6ea54c/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "bGVnZW5kdXM7MTs=",
                    "setID": "legendus",
                    "version": "1",
                    "title": "LEGENDUS",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/55c355cf-ddbf-4f12-8369-6554a1f78b6f/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/55c355cf-ddbf-4f12-8369-6554a1f78b6f/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/55c355cf-ddbf-4f12-8369-6554a1f78b6f/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "bG9sLXdvcmxkcy0yMDI1OzE7",
                    "setID": "lol-worlds-2025",
                    "version": "1",
                    "title": "LoL Worlds 2025",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/4545b0b5-c825-487e-8958-ce5512eb6f84/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/4545b0b5-c825-487e-8958-ce5512eb6f84/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/4545b0b5-c825-487e-8958-ce5512eb6f84/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "bG93OzE7",
                    "setID": "low",
                    "version": "1",
                    "title": "低",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/58d48669-bfee-46e7-a83c-b65a30783400/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/58d48669-bfee-46e7-a83c-b65a30783400/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/58d48669-bfee-46e7-a83c-b65a30783400/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "bWFyYXRob24tcmV2ZWFsLXJ1bm5lcjsxOw==",
                    "setID": "marathon-reveal-runner",
                    "version": "1",
                    "title": "『Marathon』初公開ランナー",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/ae1c6c62-c057-4fad-a1d4-663bf988701f/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/ae1c6c62-c057-4fad-a1d4-663bf988701f/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/ae1c6c62-c057-4fad-a1d4-663bf988701f/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "bWVsOzE7",
                    "setID": "mel",
                    "version": "1",
                    "title": "メル",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/8a7c1d1d-12bb-40d9-9be8-0a4fdf0d870c/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/8a7c1d1d-12bb-40d9-9be8-0a4fdf0d870c/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/8a7c1d1d-12bb-40d9-9be8-0a4fdf0d870c/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "bWluZWNyYWZ0LTE1dGgtYW5uaXZlcnNhcnktY2VsZWJyYXRpb247MTs=",
                    "setID": "minecraft-15th-anniversary-celebration",
                    "version": "1",
                    "title": "『Minecraft』15周年記念セレブレーション",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/178077b2-8b86-4f8d-927c-66ed6c1b025f/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/178077b2-8b86-4f8d-927c-66ed6c1b025f/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/178077b2-8b86-4f8d-927c-66ed6c1b025f/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://twitch-web.app.link/e/vkOhfCa7nJb",
                    "__typename": "Badge"
                },
                {
                    "id": "bW9kZXJhdG9yOzE7",
                    "setID": "moderator",
                    "version": "1",
                    "title": "モデレーター",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/3267646d-33f0-4b17-b3df-f923a41db1d0/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/3267646d-33f0-4b17-b3df-f923a41db1d0/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/3267646d-33f0-4b17-b3df-f923a41db1d0/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "bW9tZW50czsxNDs=",
                    "setID": "moments",
                    "version": "14",
                    "title": "Moments Badge - Tier 14",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/cb40eb03-1015-45ba-8793-51c66a24a3d5/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/cb40eb03-1015-45ba-8793-51c66a24a3d5/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/cb40eb03-1015-45ba-8793-51c66a24a3d5/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "bW9tZW50czs4Ow==",
                    "setID": "moments",
                    "version": "8",
                    "title": "Moments Badge - Tier 8",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/f22286cd-6aa3-42ce-b3fb-10f5d18c4aa0/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/f22286cd-6aa3-42ce-b3fb-10f5d18c4aa0/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/f22286cd-6aa3-42ce-b3fb-10f5d18c4aa0/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "bW9tZW50czsxMTs=",
                    "setID": "moments",
                    "version": "11",
                    "title": "Moments Badge - Tier 11",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/7573e7a2-0f1f-4508-b833-d822567a1e03/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/7573e7a2-0f1f-4508-b833-d822567a1e03/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/7573e7a2-0f1f-4508-b833-d822567a1e03/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "bW9tZW50czs2Ow==",
                    "setID": "moments",
                    "version": "6",
                    "title": "Moments Badge - Tier 6",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/f9e3b4e4-200e-4045-bd71-3a6b480c23ae/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/f9e3b4e4-200e-4045-bd71-3a6b480c23ae/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/f9e3b4e4-200e-4045-bd71-3a6b480c23ae/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "bW9tZW50czsxOTs=",
                    "setID": "moments",
                    "version": "19",
                    "title": "Moments Badge - Tier 19",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/6100cc6f-6b4b-4a3d-a55b-a5b34edb5ea1/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/6100cc6f-6b4b-4a3d-a55b-a5b34edb5ea1/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/6100cc6f-6b4b-4a3d-a55b-a5b34edb5ea1/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "bW9tZW50czsxNzs=",
                    "setID": "moments",
                    "version": "17",
                    "title": "Moments Badge - Tier 17",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/3b08c1ee-0f77-451b-9226-b5b22d7f023c/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/3b08c1ee-0f77-451b-9226-b5b22d7f023c/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/3b08c1ee-0f77-451b-9226-b5b22d7f023c/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "bW9tZW50czsxMzs=",
                    "setID": "moments",
                    "version": "13",
                    "title": "Moments Badge - Tier 13",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/35eb3395-a1d3-4170-969a-86402ecfb11a/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/35eb3395-a1d3-4170-969a-86402ecfb11a/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/35eb3395-a1d3-4170-969a-86402ecfb11a/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "bW9tZW50czsxOw==",
                    "setID": "moments",
                    "version": "1",
                    "title": "Moments Badge - Tier 1",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/bf370830-d79a-497b-81c6-a365b2b60dda/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/bf370830-d79a-497b-81c6-a365b2b60dda/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/bf370830-d79a-497b-81c6-a365b2b60dda/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "bW9tZW50czsxODs=",
                    "setID": "moments",
                    "version": "18",
                    "title": "Moments Badge - Tier 18",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/14057e75-080c-42da-a412-6232c6f33b68/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/14057e75-080c-42da-a412-6232c6f33b68/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/14057e75-080c-42da-a412-6232c6f33b68/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "bW9tZW50czsxMjs=",
                    "setID": "moments",
                    "version": "12",
                    "title": "Moments Badge - Tier 12",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/f2c91d14-85c8-434b-a6c0-6d7930091150/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/f2c91d14-85c8-434b-a6c0-6d7930091150/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/f2c91d14-85c8-434b-a6c0-6d7930091150/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "bW9tZW50czsxNjs=",
                    "setID": "moments",
                    "version": "16",
                    "title": "Moments Badge - Tier 16",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/5684d1bc-8132-4a4f-850c-18d3c5bd04f3/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/5684d1bc-8132-4a4f-850c-18d3c5bd04f3/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/5684d1bc-8132-4a4f-850c-18d3c5bd04f3/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "bW9tZW50czsyMDs=",
                    "setID": "moments",
                    "version": "20",
                    "title": "Moments Badge - Tier 20",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/43399796-e74c-4741-a975-56202f0af30e/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/43399796-e74c-4741-a975-56202f0af30e/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/43399796-e74c-4741-a975-56202f0af30e/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "bW9tZW50czs1Ow==",
                    "setID": "moments",
                    "version": "5",
                    "title": "Moments Badge - Tier 5",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/c8a0d95a-856e-4097-9fc0-7765300a4f58/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/c8a0d95a-856e-4097-9fc0-7765300a4f58/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/c8a0d95a-856e-4097-9fc0-7765300a4f58/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "bW9tZW50czsyOw==",
                    "setID": "moments",
                    "version": "2",
                    "title": "Moments Badge - Tier 2",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/fc46b10c-5b45-43fd-81ad-d5cb0de6d2f4/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/fc46b10c-5b45-43fd-81ad-d5cb0de6d2f4/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/fc46b10c-5b45-43fd-81ad-d5cb0de6d2f4/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "bW9tZW50czsxNTs=",
                    "setID": "moments",
                    "version": "15",
                    "title": "Moments Badge - Tier 15",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/b241d667-280b-4183-96ae-2d0053631186/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/b241d667-280b-4183-96ae-2d0053631186/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/b241d667-280b-4183-96ae-2d0053631186/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "bW9tZW50czszOw==",
                    "setID": "moments",
                    "version": "3",
                    "title": "Moments Badge - Tier 3",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/d08658d7-205f-4f75-ad44-8c6e0acd8ef6/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/d08658d7-205f-4f75-ad44-8c6e0acd8ef6/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/d08658d7-205f-4f75-ad44-8c6e0acd8ef6/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "bW9tZW50czsxMDs=",
                    "setID": "moments",
                    "version": "10",
                    "title": "Moments Badge - Tier 10",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/9c13f2b6-69cd-4537-91b4-4a8bd8b6b1fd/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/9c13f2b6-69cd-4537-91b4-4a8bd8b6b1fd/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/9c13f2b6-69cd-4537-91b4-4a8bd8b6b1fd/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "bW9tZW50czs5Ow==",
                    "setID": "moments",
                    "version": "9",
                    "title": "Moments Badge - Tier 9",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/5cb2e584-1efd-469b-ab1d-4d1b59a944e7/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/5cb2e584-1efd-469b-ab1d-4d1b59a944e7/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/5cb2e584-1efd-469b-ab1d-4d1b59a944e7/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "bW9tZW50czs3Ow==",
                    "setID": "moments",
                    "version": "7",
                    "title": "Moments Badge - Tier 7",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/a90a26a4-fdf7-4ac3-a782-76a413da16c1/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/a90a26a4-fdf7-4ac3-a782-76a413da16c1/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/a90a26a4-fdf7-4ac3-a782-76a413da16c1/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "bW9tZW50czs0Ow==",
                    "setID": "moments",
                    "version": "4",
                    "title": "Moments Badge - Tier 4",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/fe5b5ddc-93e7-4aaf-9b3e-799cd41808b1/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/fe5b5ddc-93e7-4aaf-9b3e-799cd41808b1/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/fe5b5ddc-93e7-4aaf-9b3e-799cd41808b1/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "bm9fYXVkaW87MTs=",
                    "setID": "no_audio",
                    "version": "1",
                    "title": "音声なしで視聴中",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/aef2cd08-f29b-45a1-8c12-d44d7fd5e6f0/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/aef2cd08-f29b-45a1-8c12-d44d7fd5e6f0/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/aef2cd08-f29b-45a1-8c12-d44d7fd5e6f0/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "bm9fdmlkZW87MTs=",
                    "setID": "no_video",
                    "version": "1",
                    "title": "音声のみ",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/199a0dba-58f3-494e-a7fc-1fa0a1001fb8/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/199a0dba-58f3-494e-a7fc-1fa0a1001fb8/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/199a0dba-58f3-494e-a7fc-1fa0a1001fb8/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "b2tobG9zXzE7MTs=",
                    "setID": "okhlos_1",
                    "version": "1",
                    "title": "Okhlos",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/dc088bd6-8965-4907-a1a2-c0ba83874a7d/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/dc088bd6-8965-4907-a1a2-c0ba83874a7d/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/dc088bd6-8965-4907-a1a2-c0ba83874a7d/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/Okhlos/details",
                    "__typename": "Badge"
                },
                {
                    "id": "b3ZlcndhdGNoLWxlYWd1ZS1pbnNpZGVyXzE7MTs=",
                    "setID": "overwatch-league-insider_1",
                    "version": "1",
                    "title": "OWL All-Access Pass 2018",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/51e9e0aa-12e3-48ce-b961-421af0787dad/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/51e9e0aa-12e3-48ce-b961-421af0787dad/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/51e9e0aa-12e3-48ce-b961-421af0787dad/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/overwatchleague",
                    "__typename": "Badge"
                },
                {
                    "id": "b3ZlcndhdGNoLWxlYWd1ZS1pbnNpZGVyXzIwMThCOzE7",
                    "setID": "overwatch-league-insider_2018B",
                    "version": "1",
                    "title": "OWL All-Access Pass 2018",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/34ec1979-d9bb-4706-ad15-464de814a79d/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/34ec1979-d9bb-4706-ad15-464de814a79d/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/34ec1979-d9bb-4706-ad15-464de814a79d/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/overwatchleague",
                    "__typename": "Badge"
                },
                {
                    "id": "b3ZlcndhdGNoLWxlYWd1ZS1pbnNpZGVyXzIwMTlBOzI7",
                    "setID": "overwatch-league-insider_2019A",
                    "version": "2",
                    "title": "OWL All-Access Pass 2019",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/ab7fa7a7-c2d9-403f-9f33-215b29b43ce4/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/ab7fa7a7-c2d9-403f-9f33-215b29b43ce4/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/ab7fa7a7-c2d9-403f-9f33-215b29b43ce4/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/overwatchleague",
                    "__typename": "Badge"
                },
                {
                    "id": "b3ZlcndhdGNoLWxlYWd1ZS1pbnNpZGVyXzIwMTlBOzE7",
                    "setID": "overwatch-league-insider_2019A",
                    "version": "1",
                    "title": "OWL All-Access Pass 2019",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/ca980da1-3639-48a6-95a3-a03b002eb0e5/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/ca980da1-3639-48a6-95a3-a03b002eb0e5/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/ca980da1-3639-48a6-95a3-a03b002eb0e5/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/overwatchleague",
                    "__typename": "Badge"
                },
                {
                    "id": "b3ZlcndhdGNoLWxlYWd1ZS1pbnNpZGVyXzIwMTlCOzU7",
                    "setID": "overwatch-league-insider_2019B",
                    "version": "5",
                    "title": "OWL All-Access Pass 2019",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/be87fd6d-1560-4e33-9ba4-2401b58d901f/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/be87fd6d-1560-4e33-9ba4-2401b58d901f/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/be87fd6d-1560-4e33-9ba4-2401b58d901f/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/overwatchleague",
                    "__typename": "Badge"
                },
                {
                    "id": "b3ZlcndhdGNoLWxlYWd1ZS1pbnNpZGVyXzIwMTlCOzQ7",
                    "setID": "overwatch-league-insider_2019B",
                    "version": "4",
                    "title": "OWL All-Access Pass 2019",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/a8ae0ccd-783d-460d-93ee-57c485c558a6/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/a8ae0ccd-783d-460d-93ee-57c485c558a6/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/a8ae0ccd-783d-460d-93ee-57c485c558a6/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/overwatchleague",
                    "__typename": "Badge"
                },
                {
                    "id": "b3ZlcndhdGNoLWxlYWd1ZS1pbnNpZGVyXzIwMTlCOzM7",
                    "setID": "overwatch-league-insider_2019B",
                    "version": "3",
                    "title": "OWL All-Access Pass 2019",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/765a0dcf-2a94-43ff-9b9c-ef6c209b90cd/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/765a0dcf-2a94-43ff-9b9c-ef6c209b90cd/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/765a0dcf-2a94-43ff-9b9c-ef6c209b90cd/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/overwatchleague",
                    "__typename": "Badge"
                },
                {
                    "id": "b3ZlcndhdGNoLWxlYWd1ZS1pbnNpZGVyXzIwMTlCOzI7",
                    "setID": "overwatch-league-insider_2019B",
                    "version": "2",
                    "title": "OWL All-Access Pass 2019",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/75f05d4b-3042-415c-8b0b-e87620a24daf/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/75f05d4b-3042-415c-8b0b-e87620a24daf/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/75f05d4b-3042-415c-8b0b-e87620a24daf/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/overwatchleague",
                    "__typename": "Badge"
                },
                {
                    "id": "b3ZlcndhdGNoLWxlYWd1ZS1pbnNpZGVyXzIwMTlCOzE7",
                    "setID": "overwatch-league-insider_2019B",
                    "version": "1",
                    "title": "OWL All-Access Pass 2019",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/c5860811-d714-4413-9433-d6b1c9fc803c/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/c5860811-d714-4413-9433-d6b1c9fc803c/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/c5860811-d714-4413-9433-d6b1c9fc803c/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/overwatchleague",
                    "__typename": "Badge"
                },
                {
                    "id": "cGFydG5lcjsxOw==",
                    "setID": "partner",
                    "version": "1",
                    "title": "認証済み",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/d12a2e27-16f6-41d0-ab77-b780518f00a3/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/d12a2e27-16f6-41d0-ab77-b780518f00a3/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/d12a2e27-16f6-41d0-ab77-b780518f00a3/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://blog.twitch.tv/2017/04/24/the-verified-badge-is-here-13381bc05735",
                    "__typename": "Badge"
                },
                {
                    "id": "cGF0aC1vZi1leGlsZS0yLWJhZGdlOzE7",
                    "setID": "path-of-exile-2-badge",
                    "version": "1",
                    "title": "Chaos Orb",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/8bebe4ce-6c15-4746-8c33-42312c250ceb/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/8bebe4ce-6c15-4746-8c33-42312c250ceb/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/8bebe4ce-6c15-4746-8c33-42312c250ceb/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "cG9rZW1vbi1sZWdlbmRzLXotYS1jaGlrb3JpdGE7MTs=",
                    "setID": "pokemon-legends-z-a-chikorita",
                    "version": "1",
                    "title": "Pokémon Legends: Z-A チコリータ",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/f479e945-987b-423a-a901-7b1c3b3003fb/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/f479e945-987b-423a-a901-7b1c3b3003fb/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/f479e945-987b-423a-a901-7b1c3b3003fb/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "cG9rZW1vbi1sZWdlbmRzLXotYS10ZXBpZzsxOw==",
                    "setID": "pokemon-legends-z-a-tepig",
                    "version": "1",
                    "title": "Pokémon Legends: Z-A ポカブ",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/34799f2e-e165-42a4-ae79-1c9c06cf1d55/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/34799f2e-e165-42a4-ae79-1c9c06cf1d55/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/34799f2e-e165-42a4-ae79-1c9c06cf1d55/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "cG9rZW1vbi1sZWdlbmRzLXotYS10b3RvZGlsZTsxOw==",
                    "setID": "pokemon-legends-z-a-totodile",
                    "version": "1",
                    "title": "Pokémon Legends: Z-A ワニノコ",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/0bdb6906-ba8f-4fb6-bc9e-3aca04d3d501/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/0bdb6906-ba8f-4fb6-bc9e-3aca04d3d501/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/0bdb6906-ba8f-4fb6-bc9e-3aca04d3d501/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "cG93ZXItcmFuZ2VyczszOw==",
                    "setID": "power-rangers",
                    "version": "3",
                    "title": "Pink Ranger",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/5c58cb40-9028-4d16-af67-5bc0c18b745e/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/5c58cb40-9028-4d16-af67-5bc0c18b745e/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/5c58cb40-9028-4d16-af67-5bc0c18b745e/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "cG93ZXItcmFuZ2VyczsyOw==",
                    "setID": "power-rangers",
                    "version": "2",
                    "title": "Green Ranger",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/21bbcd6d-1751-4d28-a0c3-0b72453dd823/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/21bbcd6d-1751-4d28-a0c3-0b72453dd823/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/21bbcd6d-1751-4d28-a0c3-0b72453dd823/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "cG93ZXItcmFuZ2VyczsxOw==",
                    "setID": "power-rangers",
                    "version": "1",
                    "title": "Blue Ranger",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/1eeae8fe-5bc6-44ed-9c88-fb84d5e0df52/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/1eeae8fe-5bc6-44ed-9c88-fb84d5e0df52/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/1eeae8fe-5bc6-44ed-9c88-fb84d5e0df52/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "cG93ZXItcmFuZ2VyczswOw==",
                    "setID": "power-rangers",
                    "version": "0",
                    "title": "Black Ranger",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/9edf3e7f-62e4-40f5-86ab-7a646b10f1f0/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/9edf3e7f-62e4-40f5-86ab-7a646b10f1f0/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/9edf3e7f-62e4-40f5-86ab-7a646b10f1f0/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "cG93ZXItcmFuZ2Vyczs2Ow==",
                    "setID": "power-rangers",
                    "version": "6",
                    "title": "Yellow Ranger",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/d6dca630-1ca4-48de-94e7-55ed0a24d8d1/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/d6dca630-1ca4-48de-94e7-55ed0a24d8d1/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/d6dca630-1ca4-48de-94e7-55ed0a24d8d1/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "cG93ZXItcmFuZ2Vyczs1Ow==",
                    "setID": "power-rangers",
                    "version": "5",
                    "title": "White Ranger",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/06c85e34-477e-4939-9537-fd9978976042/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/06c85e34-477e-4939-9537-fd9978976042/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/06c85e34-477e-4939-9537-fd9978976042/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "cG93ZXItcmFuZ2Vyczs0Ow==",
                    "setID": "power-rangers",
                    "version": "4",
                    "title": "Red Ranger",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/8843d2de-049f-47d5-9794-b6517903db61/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/8843d2de-049f-47d5-9794-b6517903db61/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/8843d2de-049f-47d5-9794-b6517903db61/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "cHJlZGljdGlvbnM7cGluay0yOw==",
                    "setID": "predictions",
                    "version": "pink-2",
                    "title": "ピンク(2)を予想",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/4b76d5f2-91cc-4400-adf2-908a1e6cfd1e/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/4b76d5f2-91cc-4400-adf2-908a1e6cfd1e/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/4b76d5f2-91cc-4400-adf2-908a1e6cfd1e/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "cHJlZGljdGlvbnM7Ymx1ZS05Ow==",
                    "setID": "predictions",
                    "version": "blue-9",
                    "title": "青(9)を予想",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/fc74bd90-2b74-4f56-8e42-04d405e10fae/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/fc74bd90-2b74-4f56-8e42-04d405e10fae/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/fc74bd90-2b74-4f56-8e42-04d405e10fae/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "cHJlZGljdGlvbnM7Ymx1ZS04Ow==",
                    "setID": "predictions",
                    "version": "blue-8",
                    "title": "青(8)を予想",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/4416dfd7-db97-44a0-98e7-40b4e250615e/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/4416dfd7-db97-44a0-98e7-40b4e250615e/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/4416dfd7-db97-44a0-98e7-40b4e250615e/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "cHJlZGljdGlvbnM7Ymx1ZS02Ow==",
                    "setID": "predictions",
                    "version": "blue-6",
                    "title": "青(6)を予想",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/46b1537e-d8b0-4c0d-8fba-a652e57b9df0/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/46b1537e-d8b0-4c0d-8fba-a652e57b9df0/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/46b1537e-d8b0-4c0d-8fba-a652e57b9df0/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "cHJlZGljdGlvbnM7Ymx1ZS0xMDs=",
                    "setID": "predictions",
                    "version": "blue-10",
                    "title": "青(10)を予想",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/072ae906-ecf7-44f1-ac69-a5b2261d8892/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/072ae906-ecf7-44f1-ac69-a5b2261d8892/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/072ae906-ecf7-44f1-ac69-a5b2261d8892/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "cHJlZGljdGlvbnM7cGluay0xOw==",
                    "setID": "predictions",
                    "version": "pink-1",
                    "title": "ピンク(1)を予想",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/75e27613-caf7-4585-98f1-cb7363a69a4a/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/75e27613-caf7-4585-98f1-cb7363a69a4a/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/75e27613-caf7-4585-98f1-cb7363a69a4a/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "cHJlZGljdGlvbnM7Z3JheS0yOw==",
                    "setID": "predictions",
                    "version": "gray-2",
                    "title": "Predicted Gray (2)",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/097a4b14-b458-47eb-91b6-fe74d3dbb3f5/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/097a4b14-b458-47eb-91b6-fe74d3dbb3f5/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/097a4b14-b458-47eb-91b6-fe74d3dbb3f5/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "cHJlZGljdGlvbnM7Z3JheS0xOw==",
                    "setID": "predictions",
                    "version": "gray-1",
                    "title": "Predicted Gray (1)",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/144f77a2-e324-4a6b-9c17-9304fa193a27/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/144f77a2-e324-4a6b-9c17-9304fa193a27/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/144f77a2-e324-4a6b-9c17-9304fa193a27/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "cHJlZGljdGlvbnM7Ymx1ZS03Ow==",
                    "setID": "predictions",
                    "version": "blue-7",
                    "title": "青(7)を予想",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/07cd34b2-c6a1-45f5-8d8a-131e3c8b2279/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/07cd34b2-c6a1-45f5-8d8a-131e3c8b2279/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/07cd34b2-c6a1-45f5-8d8a-131e3c8b2279/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "cHJlZGljdGlvbnM7Ymx1ZS01Ow==",
                    "setID": "predictions",
                    "version": "blue-5",
                    "title": "青(5)を予想",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/88758be8-de09-479b-9383-e3bb6d9e6f06/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/88758be8-de09-479b-9383-e3bb6d9e6f06/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/88758be8-de09-479b-9383-e3bb6d9e6f06/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "cHJlZGljdGlvbnM7Ymx1ZS00Ow==",
                    "setID": "predictions",
                    "version": "blue-4",
                    "title": "青(4)を予想",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/df95317d-9568-46de-a421-a8520edb9349/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/df95317d-9568-46de-a421-a8520edb9349/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/df95317d-9568-46de-a421-a8520edb9349/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "cHJlZGljdGlvbnM7Ymx1ZS0zOw==",
                    "setID": "predictions",
                    "version": "blue-3",
                    "title": "青(3)を予想",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/f2ab9a19-8ef7-4f9f-bd5d-9cf4e603f845/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/f2ab9a19-8ef7-4f9f-bd5d-9cf4e603f845/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/f2ab9a19-8ef7-4f9f-bd5d-9cf4e603f845/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "cHJlZGljdGlvbnM7Ymx1ZS0yOw==",
                    "setID": "predictions",
                    "version": "blue-2",
                    "title": "青(2)を予想",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/ffdda3fe-8012-4db3-981e-7a131402b057/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/ffdda3fe-8012-4db3-981e-7a131402b057/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/ffdda3fe-8012-4db3-981e-7a131402b057/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "cHJlZGljdGlvbnM7Ymx1ZS0xOw==",
                    "setID": "predictions",
                    "version": "blue-1",
                    "title": "青(1)を予想",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/e33d8b46-f63b-4e67-996d-4a7dcec0ad33/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/e33d8b46-f63b-4e67-996d-4a7dcec0ad33/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/e33d8b46-f63b-4e67-996d-4a7dcec0ad33/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "cHJlbWl1bTsxOw==",
                    "setID": "premium",
                    "version": "1",
                    "title": "Prime Gaming",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/bbbe0db0-a598-423e-86d0-f9fb98ca1933/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/bbbe0db0-a598-423e-86d0-f9fb98ca1933/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/bbbe0db0-a598-423e-86d0-f9fb98ca1933/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://gaming.amazon.com",
                    "__typename": "Badge"
                },
                {
                    "id": "cHN5Y2hvbmF1dHNfMTsxOw==",
                    "setID": "psychonauts_1",
                    "version": "1",
                    "title": "Psychonauts",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/a9811799-dce3-475f-8feb-3745ad12b7ea/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/a9811799-dce3-475f-8feb-3745ad12b7ea/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/a9811799-dce3-475f-8feb-3745ad12b7ea/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/Psychonauts/details",
                    "__typename": "Badge"
                },
                {
                    "id": "cHVycGxlLW5vb2I7MTs=",
                    "setID": "purple-noob",
                    "version": "1",
                    "title": "パープル Noob",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/a1fb3f16-14e8-4e2b-84f8-55e2b86878c8/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/a1fb3f16-14e8-4e2b-84f8-55e2b86878c8/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/a1fb3f16-14e8-4e2b-84f8-55e2b86878c8/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "cHVycGxlLXBpeGVsLWhlYXJ0LS0tdG9nZXRoZXItZm9yLWdvb2QtMjQ7MTs=",
                    "setID": "purple-pixel-heart---together-for-good-24",
                    "version": "1",
                    "title": "パープルのピクセルハート - Together For Good '24",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/1afb4b76-8c34-4b7b-8beb-75f7e5d2a1ab/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/1afb4b76-8c34-4b7b-8beb-75f7e5d2a1ab/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/1afb4b76-8c34-4b7b-8beb-75f7e5d2a1ab/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "cmFnaW5nLXdvbGYtaGVsbTsxOw==",
                    "setID": "raging-wolf-helm",
                    "version": "1",
                    "title": "Raging Wolf Helm",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/3ff668be-59a3-4e3e-96af-e6b2908b3171/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/3ff668be-59a3-4e3e-96af-e6b2908b3171/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/3ff668be-59a3-4e3e-96af-e6b2908b3171/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "cmFpZGVuLXYtZGlyZWN0b3JzLWN1dF8xOzE7",
                    "setID": "raiden-v-directors-cut_1",
                    "version": "1",
                    "title": "Raiden V",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/441b50ae-a2e3-11e7-8a3e-6bff0c840878/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/441b50ae-a2e3-11e7-8a3e-6bff0c840878/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/441b50ae-a2e3-11e7-8a3e-6bff0c840878/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/Raiden%20V/details",
                    "__typename": "Badge"
                },
                {
                    "id": "cmFpZGVyLWljb24tYmFkZ2U7MTs=",
                    "setID": "raider-icon-badge",
                    "version": "1",
                    "title": "Raiderアイコン",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/5007f3e0-41d4-4bda-a605-8f72cfe8c2d4/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/5007f3e0-41d4-4bda-a605-8f72cfe8c2d4/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/5007f3e0-41d4-4bda-a605-8f72cfe8c2d4/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "cmFpbmJvdy1zaXgtc2llZ2UteC0xMHRoLWFubml2ZXJzYXJ5OzE7",
                    "setID": "rainbow-six-siege-x-10th-anniversary",
                    "version": "1",
                    "title": "レインボーシックス シージエックス 10周年",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/00fc50e1-dea9-47a4-9db5-ff087e91dd0d/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/00fc50e1-dea9-47a4-9db5-ff087e91dd0d/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/00fc50e1-dea9-47a4-9db5-ff087e91dd0d/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "cmV2ZWR0di1zdHJlYW0tYXdhcmRzLTIwMjU7MTs=",
                    "setID": "revedtv-stream-awards-2025",
                    "version": "1",
                    "title": "RevedTV StreamAwards 2025",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/03cb38fd-00cc-4ed8-8d24-ce53189ee1de/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/03cb38fd-00cc-4ed8-8d24-ce53189ee1de/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/03cb38fd-00cc-4ed8-8d24-ce53189ee1de/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "cmlmdF8xOzE7",
                    "setID": "rift_1",
                    "version": "1",
                    "title": "RIFT",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/f939686b-2892-46a4-9f0d-5f582578173e/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/f939686b-2892-46a4-9f0d-5f582578173e/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/f939686b-2892-46a4-9f0d-5f582578173e/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/Rift/details",
                    "__typename": "Badge"
                },
                {
                    "id": "cnBsYWNlLTIwMjM7MTs=",
                    "setID": "rplace-2023",
                    "version": "1",
                    "title": "r/place 2023 ケーキ",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/e33e0c67-c380-4241-828a-099c46e51c66/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/e33e0c67-c380-4241-828a-099c46e51c66/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/e33e0c67-c380-4241-828a-099c46e51c66/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.reddit.com/r/place/",
                    "__typename": "Badge"
                },
                {
                    "id": "cnVieS1waXhlbC1oZWFydC0tLXRvZ2V0aGVyLWZvci1nb29kLTI0OzE7",
                    "setID": "ruby-pixel-heart---together-for-good-24",
                    "version": "1",
                    "title": "ルビーのピクセルハート - Together For Good '24",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/ca0aa8ce-b2a8-4582-a5de-4d5b6915dc47/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/ca0aa8ce-b2a8-4582-a5de-4d5b6915dc47/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/ca0aa8ce-b2a8-4582-a5de-4d5b6915dc47/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "cnVzdG1hcy0yMDI1OzE7",
                    "setID": "rustmas-2025",
                    "version": "1",
                    "title": "Rustmas 2025",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/5ec1bcd1-82fa-4c42-abf1-be5b64cb63ed/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/5ec1bcd1-82fa-4c42-abf1-be5b64cb63ed/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/5ec1bcd1-82fa-4c42-abf1-be5b64cb63ed/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c2FqYW0tc2xhbS1iYWRnZTsxOw==",
                    "setID": "sajam-slam-badge",
                    "version": "1",
                    "title": "Sajam Slamバッジ",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/9fd798d6-3d67-4458-a916-9fd5d7286159/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/9fd798d6-3d67-4458-a916-9fd5d7286159/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/9fd798d6-3d67-4458-a916-9fd5d7286159/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c2FtdXNvZmZlcl9iZXRhOzA7",
                    "setID": "samusoffer_beta",
                    "version": "0",
                    "title": "beta_title1",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/aa960159-a7b8-417e-83c1-035e4bc2deb5/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/aa960159-a7b8-417e-83c1-035e4bc2deb5/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/aa960159-a7b8-417e-83c1-035e4bc2deb5/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://twitch.amazon.com/prime",
                    "__typename": "Badge"
                },
                {
                    "id": "c2hhcmUtdGhlLWxvdmU7MTs=",
                    "setID": "share-the-love",
                    "version": "1",
                    "title": "愛をシェア",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/2de71f4f-b152-4308-a426-127a4cf8003a/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/2de71f4f-b152-4308-a426-127a4cf8003a/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/2de71f4f-b152-4308-a426-127a4cf8003a/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://blog.twitch.tv/2025/02/14/share-the-love-this-valentine-s-day/",
                    "__typename": "Badge"
                },
                {
                    "id": "c29jaWFsLXNoYXJpbmc7MTs=",
                    "setID": "social-sharing",
                    "version": "1",
                    "title": "ソーシャルメディアバッジ",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/d2030c7e-c400-4605-a2cf-ce32bd06af8f/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/d2030c7e-c400-4605-a2cf-ce32bd06af8f/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/d2030c7e-c400-4605-a2cf-ce32bd06af8f/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c29jaWFsLXNoYXJpbmc7Mzs=",
                    "setID": "social-sharing",
                    "version": "3",
                    "title": "ソーシャルメディアバッジ",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/590698dd-2bc4-4401-817a-17c641f5e881/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/590698dd-2bc4-4401-817a-17c641f5e881/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/590698dd-2bc4-4401-817a-17c641f5e881/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c29jaWFsLXNoYXJpbmc7Mjs=",
                    "setID": "social-sharing",
                    "version": "2",
                    "title": "ソーシャルメディアバッジ",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/fcca0804-1da5-4d00-ab06-7677676d4d8e/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/fcca0804-1da5-4d00-ab06-7677676d4d8e/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/fcca0804-1da5-4d00-ab06-7677676d4d8e/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c29uaWMtcmFjaW5nLWNyb3Nzd29ybGRzOzE7",
                    "setID": "sonic-racing-crossworlds",
                    "version": "1",
                    "title": "ソニックレーシング",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/3c5a5ea0-714f-46da-b764-5e7ceba59fca/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/3c5a5ea0-714f-46da-b764-5e7ceba59fca/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/3c5a5ea0-714f-46da-b764-5e7ceba59fca/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c3BlZWRvbnMtNS1iYWRnZTsxOw==",
                    "setID": "speedons-5-badge",
                    "version": "1",
                    "title": "Speedons 5 Badge",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/81d89508-850c-45ae-b0e2-dbbe6e531b8d/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/81d89508-850c-45ae-b0e2-dbbe6e531b8d/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/81d89508-850c-45ae-b0e2-dbbe6e531b8d/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c3RhZmY7MTs=",
                    "setID": "staff",
                    "version": "1",
                    "title": "スタッフ",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/d97c37bd-a6f5-4c38-8f57-4e4bef88af34/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/d97c37bd-a6f5-4c38-8f57-4e4bef88af34/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/d97c37bd-a6f5-4c38-8f57-4e4bef88af34/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/jobs?ref=chat_badge",
                    "__typename": "Badge"
                },
                {
                    "id": "c3RhcmJvdW5kXzE7MTs=",
                    "setID": "starbound_1",
                    "version": "1",
                    "title": "Starbound",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/e838e742-0025-4646-9772-18a87ba99358/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/e838e742-0025-4646-9772-18a87ba99358/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/e838e742-0025-4646-9772-18a87ba99358/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/Starbound/details",
                    "__typename": "Badge"
                },
                {
                    "id": "c3RyYWZlXzE7MTs=",
                    "setID": "strafe_1",
                    "version": "1",
                    "title": "Strafe",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/0051508d-2d42-4e4b-a328-c86b04510ca4/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/0051508d-2d42-4e4b-a328-c86b04510ca4/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/0051508d-2d42-4e4b-a328-c86b04510ca4/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/strafe/details",
                    "__typename": "Badge"
                },
                {
                    "id": "c3RyZWFtLWZvci1odW1hbml0eS0yLTIwMjU7MTs=",
                    "setID": "stream-for-humanity-2-2025",
                    "version": "1",
                    "title": "Stream For Humanity 2",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/c02fbad3-aa4b-46d0-93a6-661719a19f1c/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/c02fbad3-aa4b-46d0-93a6-661719a19f1c/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/c02fbad3-aa4b-46d0-93a6-661719a19f1c/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c3RyZWFtZXItYXdhcmRzLTIwMjQ7MTs=",
                    "setID": "streamer-awards-2024",
                    "version": "1",
                    "title": "Streamer Awards 2024",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/efc07d3d-46e4-4738-827b-a5bf3508983a/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/efc07d3d-46e4-4738-827b-a5bf3508983a/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/efc07d3d-46e4-4738-827b-a5bf3508983a/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://thestreamerawards.com/home",
                    "__typename": "Badge"
                },
                {
                    "id": "c3RyZWFtZXItYXdhcmRzLXR1eDsxOw==",
                    "setID": "streamer-awards-tux",
                    "version": "1",
                    "title": "Streamer Awards Tux",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/d52bd174-3509-460b-80ac-4a8d5840194b/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/d52bd174-3509-460b-80ac-4a8d5840194b/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/d52bd174-3509-460b-80ac-4a8d5840194b/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c3ViLWdpZnQtbGVhZGVyOzM7",
                    "setID": "sub-gift-leader",
                    "version": "3",
                    "title": "Gifter Leader 3",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/4c6e4497-eed9-4dd3-ac64-e0599d0a63e5/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/4c6e4497-eed9-4dd3-ac64-e0599d0a63e5/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/4c6e4497-eed9-4dd3-ac64-e0599d0a63e5/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c3ViLWdpZnQtbGVhZGVyOzI7",
                    "setID": "sub-gift-leader",
                    "version": "2",
                    "title": "Gifter Leader 2",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/0d9fe96b-97b7-4215-b5f3-5328ebad271c/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/0d9fe96b-97b7-4215-b5f3-5328ebad271c/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/0d9fe96b-97b7-4215-b5f3-5328ebad271c/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c3ViLWdpZnQtbGVhZGVyOzE7",
                    "setID": "sub-gift-leader",
                    "version": "1",
                    "title": "Gifter Leader 1",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/21656088-7da2-4467-acd2-55220e1f45ad/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/21656088-7da2-4467-acd2-55220e1f45ad/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/21656088-7da2-4467-acd2-55220e1f45ad/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c3ViLWdpZnRlcjs3MDA7",
                    "setID": "sub-gifter",
                    "version": "700",
                    "title": "700件のサブスクギフト",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/4a9acdc7-30be-4dd1-9898-fc9e42b3d304/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/4a9acdc7-30be-4dd1-9898-fc9e42b3d304/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/4a9acdc7-30be-4dd1-9898-fc9e42b3d304/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c3ViLWdpZnRlcjs1MDA7",
                    "setID": "sub-gifter",
                    "version": "500",
                    "title": "500件のサブスクギフト",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/60e9504c-8c3d-489f-8a74-314fb195ad8d/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/60e9504c-8c3d-489f-8a74-314fb195ad8d/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/60e9504c-8c3d-489f-8a74-314fb195ad8d/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c3ViLWdpZnRlcjszMDAwOw==",
                    "setID": "sub-gifter",
                    "version": "3000",
                    "title": "3000件のサブスクギフト",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/b18852ba-65d2-4b84-97d2-aeb6c44a0956/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/b18852ba-65d2-4b84-97d2-aeb6c44a0956/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/b18852ba-65d2-4b84-97d2-aeb6c44a0956/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c3ViLWdpZnRlcjs0MDA7",
                    "setID": "sub-gifter",
                    "version": "400",
                    "title": "400件のサブスクギフト",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/6f4cab6b-def9-4d99-ad06-90b0013b28c8/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/6f4cab6b-def9-4d99-ad06-90b0013b28c8/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/6f4cab6b-def9-4d99-ad06-90b0013b28c8/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c3ViLWdpZnRlcjsxOw==",
                    "setID": "sub-gifter",
                    "version": "1",
                    "title": "サブスクギフター",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/a5ef6c17-2e5b-4d8f-9b80-2779fd722414/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/a5ef6c17-2e5b-4d8f-9b80-2779fd722414/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/a5ef6c17-2e5b-4d8f-9b80-2779fd722414/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c3ViLWdpZnRlcjs1Ow==",
                    "setID": "sub-gifter",
                    "version": "5",
                    "title": "5件のサブスクギフト",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/ee113e59-c839-4472-969a-1e16d20f3962/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/ee113e59-c839-4472-969a-1e16d20f3962/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/ee113e59-c839-4472-969a-1e16d20f3962/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c3ViLWdpZnRlcjsxMDs=",
                    "setID": "sub-gifter",
                    "version": "10",
                    "title": "10件のサブスクギフト",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/d333288c-65d7-4c7b-b691-cdd7b3484bf8/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/d333288c-65d7-4c7b-b691-cdd7b3484bf8/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/d333288c-65d7-4c7b-b691-cdd7b3484bf8/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c3ViLWdpZnRlcjs5MDA7",
                    "setID": "sub-gifter",
                    "version": "900",
                    "title": "900件のサブスクギフト",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/193d86f6-83e1-428c-9638-d6ca9e408166/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/193d86f6-83e1-428c-9638-d6ca9e408166/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/193d86f6-83e1-428c-9638-d6ca9e408166/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c3ViLWdpZnRlcjs4NTA7",
                    "setID": "sub-gifter",
                    "version": "850",
                    "title": "850件のサブスクギフト",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/cc924aaf-dfd4-4f3f-822a-f5a87eb24069/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/cc924aaf-dfd4-4f3f-822a-f5a87eb24069/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/cc924aaf-dfd4-4f3f-822a-f5a87eb24069/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c3ViLWdpZnRlcjs2MDA7",
                    "setID": "sub-gifter",
                    "version": "600",
                    "title": "600件のサブスクギフト",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/3ecc3aab-09bf-4823-905e-3a4647171fc1/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/3ecc3aab-09bf-4823-905e-3a4647171fc1/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/3ecc3aab-09bf-4823-905e-3a4647171fc1/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c3ViLWdpZnRlcjs1MDs=",
                    "setID": "sub-gifter",
                    "version": "50",
                    "title": "50件のサブスクギフト",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/c4a29737-e8a5-4420-917a-314a447f083e/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/c4a29737-e8a5-4420-917a-314a447f083e/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/c4a29737-e8a5-4420-917a-314a447f083e/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c3ViLWdpZnRlcjsxMDA7",
                    "setID": "sub-gifter",
                    "version": "100",
                    "title": "100件のサブスクギフト",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/8343ada7-3451-434e-91c4-e82bdcf54460/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/8343ada7-3451-434e-91c4-e82bdcf54460/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/8343ada7-3451-434e-91c4-e82bdcf54460/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c3ViLWdpZnRlcjs3NTA7",
                    "setID": "sub-gifter",
                    "version": "750",
                    "title": "750件のサブスクギフト",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/ca17277c-53e5-422b-8bb4-7c5dcdb0ac67/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/ca17277c-53e5-422b-8bb4-7c5dcdb0ac67/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/ca17277c-53e5-422b-8bb4-7c5dcdb0ac67/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c3ViLWdpZnRlcjs0MDAwOw==",
                    "setID": "sub-gifter",
                    "version": "4000",
                    "title": "4000件のサブスクギフト",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/efbf3c93-ecfa-4b67-8d0a-1f732fb07397/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/efbf3c93-ecfa-4b67-8d0a-1f732fb07397/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/efbf3c93-ecfa-4b67-8d0a-1f732fb07397/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c3ViLWdpZnRlcjsyNTs=",
                    "setID": "sub-gifter",
                    "version": "25",
                    "title": "25件のサブスクギフト",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/052a5d41-f1cc-455c-bc7b-fe841ffaf17f/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/052a5d41-f1cc-455c-bc7b-fe841ffaf17f/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/052a5d41-f1cc-455c-bc7b-fe841ffaf17f/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c3ViLWdpZnRlcjsyMDA7",
                    "setID": "sub-gifter",
                    "version": "200",
                    "title": "200件のサブスクギフト",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/c6b1893e-8059-4024-b93c-39c84b601732/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/c6b1893e-8059-4024-b93c-39c84b601732/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/c6b1893e-8059-4024-b93c-39c84b601732/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c3ViLWdpZnRlcjszNTA7",
                    "setID": "sub-gifter",
                    "version": "350",
                    "title": "350件のサブスクギフト",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/6c4783cd-0aba-4e75-a7a4-f48a70b665b0/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/6c4783cd-0aba-4e75-a7a4-f48a70b665b0/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/6c4783cd-0aba-4e75-a7a4-f48a70b665b0/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c3ViLWdpZnRlcjszMDA7",
                    "setID": "sub-gifter",
                    "version": "300",
                    "title": "300件のサブスクギフト",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/9e1bb24f-d238-4078-871a-ac401b76ecf2/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/9e1bb24f-d238-4078-871a-ac401b76ecf2/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/9e1bb24f-d238-4078-871a-ac401b76ecf2/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c3ViLWdpZnRlcjs4MDA7",
                    "setID": "sub-gifter",
                    "version": "800",
                    "title": "800件のサブスクギフト",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/9c1fb96d-0579-43d7-ba94-94672eaef63a/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/9c1fb96d-0579-43d7-ba94-94672eaef63a/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/9c1fb96d-0579-43d7-ba94-94672eaef63a/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c3ViLWdpZnRlcjs2NTA7",
                    "setID": "sub-gifter",
                    "version": "650",
                    "title": "650件のサブスクギフト",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/eeabf43c-8e4c-448d-9790-4c2172c57944/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/eeabf43c-8e4c-448d-9790-4c2172c57944/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/eeabf43c-8e4c-448d-9790-4c2172c57944/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c3ViLWdpZnRlcjsyNTA7",
                    "setID": "sub-gifter",
                    "version": "250",
                    "title": "250件のサブスクギフト",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/cd479dc0-4a15-407d-891f-9fd2740bddda/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/cd479dc0-4a15-407d-891f-9fd2740bddda/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/cd479dc0-4a15-407d-891f-9fd2740bddda/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c3ViLWdpZnRlcjsxMDAwOw==",
                    "setID": "sub-gifter",
                    "version": "1000",
                    "title": "1000件のサブスクギフト",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/bfb7399a-c632-42f7-8d5f-154610dede81/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/bfb7399a-c632-42f7-8d5f-154610dede81/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/bfb7399a-c632-42f7-8d5f-154610dede81/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c3ViLWdpZnRlcjs5NTA7",
                    "setID": "sub-gifter",
                    "version": "950",
                    "title": "950件のサブスクギフト",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/7ce130bd-6f55-40cc-9231-e2a4cb712962/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/7ce130bd-6f55-40cc-9231-e2a4cb712962/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/7ce130bd-6f55-40cc-9231-e2a4cb712962/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c3ViLWdpZnRlcjs1NTA7",
                    "setID": "sub-gifter",
                    "version": "550",
                    "title": "550件のサブスクギフト",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/024d2563-1794-43ed-b8dc-33df3efae900/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/024d2563-1794-43ed-b8dc-33df3efae900/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/024d2563-1794-43ed-b8dc-33df3efae900/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c3ViLWdpZnRlcjs1MDAwOw==",
                    "setID": "sub-gifter",
                    "version": "5000",
                    "title": "5000件のサブスクギフト",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/d775275d-fd19-4914-b63a-7928a22135c3/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/d775275d-fd19-4914-b63a-7928a22135c3/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/d775275d-fd19-4914-b63a-7928a22135c3/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c3ViLWdpZnRlcjsxNTA7",
                    "setID": "sub-gifter",
                    "version": "150",
                    "title": "150件のサブスクギフト",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/514845ba-0fc3-4771-bce1-14d57e91e621/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/514845ba-0fc3-4771-bce1-14d57e91e621/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/514845ba-0fc3-4771-bce1-14d57e91e621/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c3ViLWdpZnRlcjsyMDAwOw==",
                    "setID": "sub-gifter",
                    "version": "2000",
                    "title": "2000件のサブスクギフト",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/4e8b3a32-1513-44ad-8a12-6c90232c77f9/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/4e8b3a32-1513-44ad-8a12-6c90232c77f9/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/4e8b3a32-1513-44ad-8a12-6c90232c77f9/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c3ViLWdpZnRlcjs0NTA7",
                    "setID": "sub-gifter",
                    "version": "450",
                    "title": "450件のサブスクギフト",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/b593d68a-f8fb-4516-a09a-18cce955402c/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/b593d68a-f8fb-4516-a09a-18cce955402c/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/b593d68a-f8fb-4516-a09a-18cce955402c/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c3Vic2NyaWJlcjs1Ow==",
                    "setID": "subscriber",
                    "version": "5",
                    "title": "9-Month Subscriber",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/b4e6b13a-a76f-4c56-87e1-9375a7aaa610/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/b4e6b13a-a76f-4c56-87e1-9375a7aaa610/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/b4e6b13a-a76f-4c56-87e1-9375a7aaa610/3",
                    "clickAction": "SUBSCRIBE",
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c3Vic2NyaWJlcjs0Ow==",
                    "setID": "subscriber",
                    "version": "4",
                    "title": "6-Month Subscriber",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/2d2485f6-d19b-4daa-8393-9493b019156b/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/2d2485f6-d19b-4daa-8393-9493b019156b/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/2d2485f6-d19b-4daa-8393-9493b019156b/3",
                    "clickAction": "SUBSCRIBE",
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c3Vic2NyaWJlcjszOw==",
                    "setID": "subscriber",
                    "version": "3",
                    "title": "3ヵ月サブスクライバー",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/e8984705-d091-4e54-8241-e53b30a84b0e/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/e8984705-d091-4e54-8241-e53b30a84b0e/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/e8984705-d091-4e54-8241-e53b30a84b0e/3",
                    "clickAction": "SUBSCRIBE",
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c3Vic2NyaWJlcjsyOw==",
                    "setID": "subscriber",
                    "version": "2",
                    "title": "2ヶ月のサブスクライバー",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/25a03e36-2bb2-4625-bd37-d6d9d406238d/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/25a03e36-2bb2-4625-bd37-d6d9d406238d/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/25a03e36-2bb2-4625-bd37-d6d9d406238d/3",
                    "clickAction": "SUBSCRIBE",
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c3Vic2NyaWJlcjsxOw==",
                    "setID": "subscriber",
                    "version": "1",
                    "title": "サブスクライバー",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/5d9f2208-5dd8-11e7-8513-2ff4adfae661/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/5d9f2208-5dd8-11e7-8513-2ff4adfae661/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/5d9f2208-5dd8-11e7-8513-2ff4adfae661/3",
                    "clickAction": "SUBSCRIBE",
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c3Vic2NyaWJlcjswOw==",
                    "setID": "subscriber",
                    "version": "0",
                    "title": "サブスクライバー",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/5d9f2208-5dd8-11e7-8513-2ff4adfae661/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/5d9f2208-5dd8-11e7-8513-2ff4adfae661/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/5d9f2208-5dd8-11e7-8513-2ff4adfae661/3",
                    "clickAction": "SUBSCRIBE",
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c3Vic2NyaWJlcjs2Ow==",
                    "setID": "subscriber",
                    "version": "6",
                    "title": "6ヵ月サブスクライバー",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/ed51a614-2c44-4a60-80b6-62908436b43a/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/ed51a614-2c44-4a60-80b6-62908436b43a/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/ed51a614-2c44-4a60-80b6-62908436b43a/3",
                    "clickAction": "SUBSCRIBE",
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c3VidGVtYmVyLTIwMjQ7MTs=",
                    "setID": "subtember-2024",
                    "version": "1",
                    "title": "SUBtember 2024",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/4149750c-9582-4515-9e22-da7d5437643b/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/4149750c-9582-4515-9e22-da7d5437643b/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/4149750c-9582-4515-9e22-da7d5437643b/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://link.twitch.tv/subtember2024",
                    "__typename": "Badge"
                },
                {
                    "id": "c3VidGVtYmVyLTIwMjU7MTs=",
                    "setID": "subtember-2025",
                    "version": "1",
                    "title": "SUBtember 2025",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/a9c01f28-179e-486d-a4c7-2277e4f6adb4/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/a9c01f28-179e-486d-a4c7-2277e4f6adb4/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/a9c01f28-179e-486d-a4c7-2277e4f6adb4/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c3VwZXJob3RfMTsxOw==",
                    "setID": "superhot_1",
                    "version": "1",
                    "title": "Superhot",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/c5a06922-83b5-40cb-885f-bcffd3cd6c68/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/c5a06922-83b5-40cb-885f-bcffd3cd6c68/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/c5a06922-83b5-40cb-885f-bcffd3cd6c68/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/superhot/details",
                    "__typename": "Badge"
                },
                {
                    "id": "c3VwZXJ1bHRyYWNvbWJvLTIwMjM7MTs=",
                    "setID": "superultracombo-2023",
                    "version": "1",
                    "title": "SuperUltraCombo 2023",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/5864739a-5e58-4623-9450-a2c0555ef90b/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/5864739a-5e58-4623-9450-a2c0555ef90b/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/5864739a-5e58-4623-9450-a2c0555ef90b/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "c3VwcG9ydC1hLXN0cmVhbWVyLWhvMjYtYmFkZ2U7MTs=",
                    "setID": "support-a-streamer-ho26-badge",
                    "version": "1",
                    "title": "ストリーマー応援キャンペーン HO'26",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/fe0814c3-87f9-40cb-95b5-d1e7453f289d/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/fe0814c3-87f9-40cb-95b5-d1e7453f289d/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/fe0814c3-87f9-40cb-95b5-d1e7453f289d/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://worldoftanks.eu/en/news/general-news/wot-monthly-december-2025/",
                    "__typename": "Badge"
                },
                {
                    "id": "c3Vydml2YWwtY3VwLTQ7MTs=",
                    "setID": "survival-cup-4",
                    "version": "1",
                    "title": "Survival Cup 4",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/9ff55f50-2c2a-40c2-9863-158d5ac2d5fd/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/9ff55f50-2c2a-40c2-9863-158d5ac2d5fd/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/9ff55f50-2c2a-40c2-9863-158d5ac2d5fd/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "dGZ0LXBhcmlzLW9wZW47MTs=",
                    "setID": "tft-paris-open",
                    "version": "1",
                    "title": "TFTパリオープン",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/c5688799-c50c-4878-b451-de78f3ef6a56/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/c5688799-c50c-4878-b451-de78f3ef6a56/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/c5688799-c50c-4878-b451-de78f3ef6a56/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "dGhlLWZpcnN0LWRlc2NlbmRhbnQtYmFkZ2U7MTs=",
                    "setID": "the-first-descendant-badge",
                    "version": "1",
                    "title": "The First Descendantバッジ",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/a56ef091-e8cd-49bd-9de9-7b342c9a7e7e/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/a56ef091-e8cd-49bd-9de9-7b342c9a7e7e/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/a56ef091-e8cd-49bd-9de9-7b342c9a7e7e/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "dGhlLWdhbWUtYXdhcmRzLTIwMjM7MTs=",
                    "setID": "the-game-awards-2023",
                    "version": "1",
                    "title": "The Game Awards 2023",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/10cf46de-61e7-4a42-807a-7898408ce352/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/10cf46de-61e7-4a42-807a-7898408ce352/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/10cf46de-61e7-4a42-807a-7898408ce352/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://blog.twitch.tv/2023/11/30/the-2023-game-awards-is-live-on-twitch-december-7th/",
                    "__typename": "Badge"
                },
                {
                    "id": "dGhlLWdvbGRlbi1wcmVkaWN0b3Itb2YtdGhlLWdhbWUtYXdhcmRzLTIwMjM7MTs=",
                    "setID": "the-golden-predictor-of-the-game-awards-2023",
                    "version": "1",
                    "title": "The Game Awards 2023のゴールデン予想者",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/c84c4dd7-9318-4e8b-9f01-1612d3f83dae/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/c84c4dd7-9318-4e8b-9f01-1612d3f83dae/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/c84c4dd7-9318-4e8b-9f01-1612d3f83dae/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://blog.twitch.tv/2023/11/30/the-2023-game-awards-is-live-on-twitch-december-7th/",
                    "__typename": "Badge"
                },
                {
                    "id": "dGhlLW1hbi13aXRob3V0LWZlYXI7MTs=",
                    "setID": "the-man-without-fear",
                    "version": "1",
                    "title": "The Man Without Fear",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/4ca893e7-6ee3-4437-acdd-071340913d3c/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/4ca893e7-6ee3-4437-acdd-071340913d3c/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/4ca893e7-6ee3-4437-acdd-071340913d3c/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "dGhlLW9ucnlvcy1tYXNrOzE7",
                    "setID": "the-onryos-mask",
                    "version": "1",
                    "title": "怨霊の仮面",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/e0d21894-8d58-4266-9127-b1bd61177899/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/e0d21894-8d58-4266-9127-b1bd61177899/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/e0d21894-8d58-4266-9127-b1bd61177899/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "dGhlLXN1cmdlXzE7MTs=",
                    "setID": "the-surge_1",
                    "version": "1",
                    "title": "The Surge",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/c9f69d89-31c8-41aa-843b-fee956dfbe23/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/c9f69d89-31c8-41aa-843b-fee956dfbe23/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/c9f69d89-31c8-41aa-843b-fee956dfbe23/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/The%20Surge/details",
                    "__typename": "Badge"
                },
                {
                    "id": "dGhlLXN1cmdlXzI7MTs=",
                    "setID": "the-surge_2",
                    "version": "1",
                    "title": "The Surge",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/2c4d7e95-e138-4dde-a783-7956a8ecc408/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/2c4d7e95-e138-4dde-a783-7956a8ecc408/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/2c4d7e95-e138-4dde-a783-7956a8ecc408/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/The%20Surge/details",
                    "__typename": "Badge"
                },
                {
                    "id": "dGhlLXN1cmdlXzM7MTs=",
                    "setID": "the-surge_3",
                    "version": "1",
                    "title": "The Surge",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/0a8fc2d4-3125-4ccb-88db-e970dfbee189/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/0a8fc2d4-3125-4ccb-88db-e970dfbee189/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/0a8fc2d4-3125-4ccb-88db-e970dfbee189/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/The%20Surge/details",
                    "__typename": "Badge"
                },
                {
                    "id": "dGhpcy13YXItb2YtbWluZV8xOzE7",
                    "setID": "this-war-of-mine_1",
                    "version": "1",
                    "title": "This War of Mine",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/6a20f814-cb2c-414e-89cc-f8dd483e1785/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/6a20f814-cb2c-414e-89cc-f8dd483e1785/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/6a20f814-cb2c-414e-89cc-f8dd483e1785/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/This%20War%20of%20Mine/details",
                    "__typename": "Badge"
                },
                {
                    "id": "dGl0YW4tc291bHNfMTsxOw==",
                    "setID": "titan-souls_1",
                    "version": "1",
                    "title": "Titan Souls",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/092a7ce2-709c-434f-8df4-a6b075ef867d/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/092a7ce2-709c-434f-8df4-a6b075ef867d/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/092a7ce2-709c-434f-8df4-a6b075ef867d/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/Titan%20Souls/details",
                    "__typename": "Badge"
                },
                {
                    "id": "dG9nZXRoZXItZm9yLWdvb2QtMjUtLS1nb29kLWJhZGdlOzE7",
                    "setID": "together-for-good-25---good-badge",
                    "version": "1",
                    "title": "Together for Good '25 - Goodバッジ",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/192fb627-82b3-46e8-95d3-ac9feba4b1bc/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/192fb627-82b3-46e8-95d3-ac9feba4b1bc/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/192fb627-82b3-46e8-95d3-ac9feba4b1bc/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "dG9nZXRoZXItZm9yLWdvb2QtMjUtLS1nb29kZXItYmFkZ2U7MTs=",
                    "setID": "together-for-good-25---gooder-badge",
                    "version": "1",
                    "title": "Together for Good '25 - Gooderバッジ",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/84a37e81-9d61-4c29-970e-64a32ec040c7/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/84a37e81-9d61-4c29-970e-64a32ec040c7/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/84a37e81-9d61-4c29-970e-64a32ec040c7/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "dG9nZXRoZXItZm9yLWdvb2QtMjUtLS1nb29kZXN0LWJhZGdlOzE7",
                    "setID": "together-for-good-25---goodest-badge",
                    "version": "1",
                    "title": "Together for Good '25 - Goodestバッジ",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/928b8033-3777-49cb-a056-230135a08a62/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/928b8033-3777-49cb-a056-230135a08a62/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/928b8033-3777-49cb-a056-230135a08a62/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "dG9nZXRoZXItZm9yLWdvb2QtMjUtLS13aWNrZWQtZHViLWJhZGdlOzE7",
                    "setID": "together-for-good-25---wicked-dub-badge",
                    "version": "1",
                    "title": "Together for Good '25 - Wicked Dubバッジ",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/9ddae219-6674-4fbc-add9-6e4e6572ea8e/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/9ddae219-6674-4fbc-add9-6e4e6572ea8e/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/9ddae219-6674-4fbc-add9-6e4e6572ea8e/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "dG90YWwtd2FyLWFubml2ZXJzYXJ5OzE7",
                    "setID": "total-war-anniversary",
                    "version": "1",
                    "title": "Total Warアニバーサリー",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/17604bfd-aeb6-427d-bd0e-cae93bed6f36/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/17604bfd-aeb6-427d-bd0e-cae93bed6f36/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/17604bfd-aeb6-427d-bd0e-cae93bed6f36/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "dG91Y2gtZ3Jhc3M7MTs=",
                    "setID": "touch-grass",
                    "version": "1",
                    "title": "Touch Grass",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/51f536c1-96ca-495b-bc11-150c857a6d54/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/51f536c1-96ca-495b-bc11-150c857a6d54/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/51f536c1-96ca-495b-bc11-150c857a6d54/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://help.twitch.tv/s/article/twitch-chat-badges-guide",
                    "__typename": "Badge"
                },
                {
                    "id": "dHJlYXN1cmUtYWR2ZW50dXJlLXdvcmxkXzE7MTs=",
                    "setID": "treasure-adventure-world_1",
                    "version": "1",
                    "title": "Treasure Adventure World",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/59810027-2988-4b0d-b88d-fc414c751305/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/59810027-2988-4b0d-b88d-fc414c751305/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/59810027-2988-4b0d-b88d-fc414c751305/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/Treasure%20Adventure%20World/details",
                    "__typename": "Badge"
                },
                {
                    "id": "dHVyYm87MTs=",
                    "setID": "turbo",
                    "version": "1",
                    "title": "Turbo",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/bd444ec6-8f34-4bf9-91f4-af1e3428d80f/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/bd444ec6-8f34-4bf9-91f4-af1e3428d80f/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/bd444ec6-8f34-4bf9-91f4-af1e3428d80f/3",
                    "clickAction": "GET_TURBO",
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "dHdpdGNoLWRqOzE7",
                    "setID": "twitch-dj",
                    "version": "1",
                    "title": "Twitch DJ",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/cf91bbc0-0332-413a-a7f3-e36bac08b624/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/cf91bbc0-0332-413a-a7f3-e36bac08b624/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/cf91bbc0-0332-413a-a7f3-e36bac08b624/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/dj-program",
                    "__typename": "Badge"
                },
                {
                    "id": "dHdpdGNoLWludGVybi0yMDIyOzE7",
                    "setID": "twitch-intern-2022",
                    "version": "1",
                    "title": "Twitchインターン2022",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/91ed38ea-32fe-4f14-8db1-852537d19aa5/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/91ed38ea-32fe-4f14-8db1-852537d19aa5/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/91ed38ea-32fe-4f14-8db1-852537d19aa5/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/jobs/early-career/",
                    "__typename": "Badge"
                },
                {
                    "id": "dHdpdGNoLWludGVybi0yMDIzOzE7",
                    "setID": "twitch-intern-2023",
                    "version": "1",
                    "title": "Twitch Intern 2023",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/e239e7e0-e373-4fdf-b95e-3469aec28485/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/e239e7e0-e373-4fdf-b95e-3469aec28485/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/e239e7e0-e373-4fdf-b95e-3469aec28485/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/jobs/early-career/",
                    "__typename": "Badge"
                },
                {
                    "id": "dHdpdGNoLWludGVybi0yMDI0OzE7",
                    "setID": "twitch-intern-2024",
                    "version": "1",
                    "title": "Twitchインターン2024",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/ae96ce48-e764-4232-aa48-d9abf9a5fdab/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/ae96ce48-e764-4232-aa48-d9abf9a5fdab/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/ae96ce48-e764-4232-aa48-d9abf9a5fdab/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/jobs/early-career/",
                    "__typename": "Badge"
                },
                {
                    "id": "dHdpdGNoLXJlY2FwLTIwMjM7MTs=",
                    "setID": "twitch-recap-2023",
                    "version": "1",
                    "title": "Twitch Recap 2023",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/4d9e9812-ba9b-48a6-8690-13f3f338ee65/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/4d9e9812-ba9b-48a6-8690-13f3f338ee65/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/4d9e9812-ba9b-48a6-8690-13f3f338ee65/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://twitch-web.app.link/e/twitch-recap",
                    "__typename": "Badge"
                },
                {
                    "id": "dHdpdGNoLXJlY2FwLTIwMjQ7MTs=",
                    "setID": "twitch-recap-2024",
                    "version": "1",
                    "title": "Twitch Recap 2024",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/72f2a6ac-3d9b-4406-b9e9-998b27182f61/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/72f2a6ac-3d9b-4406-b9e9-998b27182f61/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/72f2a6ac-3d9b-4406-b9e9-998b27182f61/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/annual-recap",
                    "__typename": "Badge"
                },
                {
                    "id": "dHdpdGNoLXJlY2FwLTIwMjU7MTs=",
                    "setID": "twitch-recap-2025",
                    "version": "1",
                    "title": "Twitch Recap 2025",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/48b26ab3-c9f1-4f16-b02d-fe877be389fd/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/48b26ab3-c9f1-4f16-b02d-fe877be389fd/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/48b26ab3-c9f1-4f16-b02d-fe877be389fd/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/annual-recap",
                    "__typename": "Badge"
                },
                {
                    "id": "dHdpdGNoYm90OzI7",
                    "setID": "twitchbot",
                    "version": "2",
                    "title": "AutoMod",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/8dbdfef5-0901-457f-a644-afa77ba176e5/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/8dbdfef5-0901-457f-a644-afa77ba176e5/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/8dbdfef5-0901-457f-a644-afa77ba176e5/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "dHdpdGNoYm90OzE7",
                    "setID": "twitchbot",
                    "version": "1",
                    "title": "AutoMod",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/df9095f6-a8a0-4cc2-bb33-d908c0adffb8/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/df9095f6-a8a0-4cc2-bb33-d908c0adffb8/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/df9095f6-a8a0-4cc2-bb33-d908c0adffb8/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "http://link.twitch.tv/automod_blog",
                    "__typename": "Badge"
                },
                {
                    "id": "dHdpdGNoY29uLTIwMjQtLS1yb3R0ZXJkYW07MTs=",
                    "setID": "twitchcon-2024---rotterdam",
                    "version": "1",
                    "title": "TwitchCon 2024 - Rotterdam",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/95b10c66-775c-4652-9b86-10bd3a709422/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/95b10c66-775c-4652-9b86-10bd3a709422/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/95b10c66-775c-4652-9b86-10bd3a709422/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://twitchcon.com/rotterdam-2024/?utm_source=twitch\u0026utm_medium=chat-badge\u0026utm_campaign=tceu24-chat-badge",
                    "__typename": "Badge"
                },
                {
                    "id": "dHdpdGNoY29uLTIwMjQtLS1zYW4tZGllZ287MTs=",
                    "setID": "twitchcon-2024---san-diego",
                    "version": "1",
                    "title": "TwitchCon 2024 - San Diego",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/6575f0d1-2dc2-4f45-a13f-a1a969dcf8fa/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/6575f0d1-2dc2-4f45-a13f-a1a969dcf8fa/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/6575f0d1-2dc2-4f45-a13f-a1a969dcf8fa/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://twitchcon.com/san-diego-2024/?utm_source=twitch\u0026utm_medium=chat-badge\u0026utm_campaign=tcsd24-chat-badge",
                    "__typename": "Badge"
                },
                {
                    "id": "dHdpdGNoY29uLTIwMjUtLS1yb3R0ZXJkYW07MTs=",
                    "setID": "twitchcon-2025---rotterdam",
                    "version": "1",
                    "title": "TwitchCon 2025",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/f4d97fd0-437f-4d8d-b4d3-4b6d18e4705b/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/f4d97fd0-437f-4d8d-b4d3-4b6d18e4705b/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/f4d97fd0-437f-4d8d-b4d3-4b6d18e4705b/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitchcon.com/rotterdam-2025/",
                    "__typename": "Badge"
                },
                {
                    "id": "dHdpdGNoY29uLXJlZmVycmFsLXByb2dyYW0tMjAyNS1ibGVlZHB1cnBsZTsxOw==",
                    "setID": "twitchcon-referral-program-2025-bleedpurple",
                    "version": "1",
                    "title": "TwitchCon紹介プログラム2025 (bleedPurple)",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/81952c7b-cfec-479c-a8f6-2bccc296786c/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/81952c7b-cfec-479c-a8f6-2bccc296786c/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/81952c7b-cfec-479c-a8f6-2bccc296786c/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://twitchcon.com/rotterdam-2025/referral-program/",
                    "__typename": "Badge"
                },
                {
                    "id": "dHdpdGNoY29uLXJlZmVycmFsLXByb2dyYW0tMjAyNS1jaHJvbWUtc3RhcjsxOw==",
                    "setID": "twitchcon-referral-program-2025-chrome-star",
                    "version": "1",
                    "title": "TwitchCon紹介プログラム2025 (Chrome Star)",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/d139bccf-8184-4fec-a970-cd8d81a7f51a/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/d139bccf-8184-4fec-a970-cd8d81a7f51a/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/d139bccf-8184-4fec-a970-cd8d81a7f51a/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://twitchcon.com/rotterdam-2025/referral-program/",
                    "__typename": "Badge"
                },
                {
                    "id": "dHdpdGNoY29uMjAxNzsxOw==",
                    "setID": "twitchcon2017",
                    "version": "1",
                    "title": "TwitchCon 2017 - Long Beach",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/0964bed0-5c31-11e7-a90b-0739918f1d9b/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/0964bed0-5c31-11e7-a90b-0739918f1d9b/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/0964bed0-5c31-11e7-a90b-0739918f1d9b/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitchcon.com/",
                    "__typename": "Badge"
                },
                {
                    "id": "dHdpdGNoY29uMjAxODsxOw==",
                    "setID": "twitchcon2018",
                    "version": "1",
                    "title": "TwitchCon 2018 - San Jose",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/e68164e4-087d-4f62-81da-d3557efae3cb/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/e68164e4-087d-4f62-81da-d3557efae3cb/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/e68164e4-087d-4f62-81da-d3557efae3cb/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitchcon.com/?utm_source=twitch-chat\u0026utm_medium=badge\u0026utm_campaign=tc18",
                    "__typename": "Badge"
                },
                {
                    "id": "dHdpdGNoY29uQW1zdGVyZGFtMjAyMDsxOw==",
                    "setID": "twitchconAmsterdam2020",
                    "version": "1",
                    "title": "TwitchCon 2020 - Amsterdam",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/ed917c9a-1a45-4340-9c64-ca8be4348c51/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/ed917c9a-1a45-4340-9c64-ca8be4348c51/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/ed917c9a-1a45-4340-9c64-ca8be4348c51/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitchcon.com/amsterdam/?utm_source=twitch-chat\u0026utm_medium=badge\u0026utm_campaign=tcamsterdam20",
                    "__typename": "Badge"
                },
                {
                    "id": "dHdpdGNoY29uRVUyMDE5OzE7",
                    "setID": "twitchconEU2019",
                    "version": "1",
                    "title": "TwitchCon 2019 - Berlin",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/590eee9e-f04d-474c-90e7-b304d9e74b32/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/590eee9e-f04d-474c-90e7-b304d9e74b32/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/590eee9e-f04d-474c-90e7-b304d9e74b32/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://europe.twitchcon.com/?utm_source=twitch-chat\u0026utm_medium=badge\u0026utm_campaign=tceu19",
                    "__typename": "Badge"
                },
                {
                    "id": "dHdpdGNoY29uRVUyMDIyOzE7",
                    "setID": "twitchconEU2022",
                    "version": "1",
                    "title": "TwitchCon 2022 - Amsterdam",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/e4744003-50b7-4eb8-9b47-a7b1616a30c6/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/e4744003-50b7-4eb8-9b47-a7b1616a30c6/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/e4744003-50b7-4eb8-9b47-a7b1616a30c6/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitchcon.com/amsterdam-2022/?utm_source=twitch-chat\u0026utm_medium=badge\u0026utm_campaign=tceu22",
                    "__typename": "Badge"
                },
                {
                    "id": "dHdpdGNoY29uRVUyMDIzOzE7",
                    "setID": "twitchconEU2023",
                    "version": "1",
                    "title": "TwitchCon 2023 - Paris",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/a8f2084e-46b9-4bb9-ae5e-00d594aafc64/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/a8f2084e-46b9-4bb9-ae5e-00d594aafc64/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/a8f2084e-46b9-4bb9-ae5e-00d594aafc64/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitchcon.com/paris-2023/?utm_source=chat_badge",
                    "__typename": "Badge"
                },
                {
                    "id": "dHdpdGNoY29uTkEyMDE5OzE7",
                    "setID": "twitchconNA2019",
                    "version": "1",
                    "title": "TwitchCon 2019 - San Diego",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/569c829d-c216-4f56-a191-3db257ed657c/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/569c829d-c216-4f56-a191-3db257ed657c/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/569c829d-c216-4f56-a191-3db257ed657c/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitchcon.com/?utm_source=twitch-chat\u0026utm_medium=badge\u0026utm_campaign=tcna19",
                    "__typename": "Badge"
                },
                {
                    "id": "dHdpdGNoY29uTkEyMDIwOzE7",
                    "setID": "twitchconNA2020",
                    "version": "1",
                    "title": "TwitchCon 2020 - North America",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/ed917c9a-1a45-4340-9c64-ca8be4348c51/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/ed917c9a-1a45-4340-9c64-ca8be4348c51/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/ed917c9a-1a45-4340-9c64-ca8be4348c51/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitchcon.com/?utm_source=twitch-chat\u0026utm_medium=badge\u0026utm_campaign=tcna20",
                    "__typename": "Badge"
                },
                {
                    "id": "dHdpdGNoY29uTkEyMDIyOzE7",
                    "setID": "twitchconNA2022",
                    "version": "1",
                    "title": "TwitchCon 2022 - San Diego",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/344d429a-0b34-48e5-a84c-14a1b5772a3a/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/344d429a-0b34-48e5-a84c-14a1b5772a3a/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/344d429a-0b34-48e5-a84c-14a1b5772a3a/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitchcon.com/san-diego-2022/?utm_source=twitch-chat\u0026utm_medium=badge\u0026utm_campaign=tcna22",
                    "__typename": "Badge"
                },
                {
                    "id": "dHdpdGNoY29uTkEyMDIzOzE7",
                    "setID": "twitchconNA2023",
                    "version": "1",
                    "title": "TwitchCon 2023 - Las Vegas",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/c90a753f-ab20-41bc-9c42-ede062485d2c/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/c90a753f-ab20-41bc-9c42-ede062485d2c/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/c90a753f-ab20-41bc-9c42-ede062485d2c/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitchcon.com/en/las-vegas-2023/",
                    "__typename": "Badge"
                },
                {
                    "id": "dHlyYW5ueV8xOzE7",
                    "setID": "tyranny_1",
                    "version": "1",
                    "title": "Tyranny",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/0c79afdf-28ce-4b0b-9e25-4f221c30bfde/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/0c79afdf-28ce-4b0b-9e25-4f221c30bfde/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/0c79afdf-28ce-4b0b-9e25-4f221c30bfde/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/game/Tyranny/details",
                    "__typename": "Badge"
                },
                {
                    "id": "dWdseS1zd2VhdGVyOzE7",
                    "setID": "ugly-sweater",
                    "version": "1",
                    "title": "Ugly Sweater",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/8eddf4ab-68f8-4ee8-a07d-9d7e8520463e/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/8eddf4ab-68f8-4ee8-a07d-9d7e8520463e/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/8eddf4ab-68f8-4ee8-a07d-9d7e8520463e/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://help.twitch.tv/s/article/how-to-use-badges?language=en_US",
                    "__typename": "Badge"
                },
                {
                    "id": "dW1icmVsbGEtY29ycG9yYXRpb247MTs=",
                    "setID": "umbrella-corporation",
                    "version": "1",
                    "title": "アンブレラ社",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/995ff00f-c16c-4782-86ba-f2d7668dc6a2/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/995ff00f-c16c-4782-86ba-f2d7668dc6a2/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/995ff00f-c16c-4782-86ba-f2d7668dc6a2/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "dXNlci1hbm5pdmVyc2FyeTsxOw==",
                    "setID": "user-anniversary",
                    "version": "1",
                    "title": "Twitchiversary Badge",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/ccbbedaa-f4db-4d0b-9c2a-375de7ad947c/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/ccbbedaa-f4db-4d0b-9c2a-375de7ad947c/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/ccbbedaa-f4db-4d0b-9c2a-375de7ad947c/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "dmN0LXBhcmlzLTIwMjU7MTs=",
                    "setID": "vct-paris-2025",
                    "version": "1",
                    "title": "VCT Paris 2025",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/63c91cf1-53d1-4ee9-821f-b4d6e4144e8e/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/63c91cf1-53d1-4ee9-821f-b4d6e4144e8e/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/63c91cf1-53d1-4ee9-821f-b4d6e4144e8e/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "dmdhLWNoYW1wLTIwMTc7MTs=",
                    "setID": "vga-champ-2017",
                    "version": "1",
                    "title": "2017 VGA Champ",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/03dca92e-dc69-11e7-ac5b-9f942d292dc7/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/03dca92e-dc69-11e7-ac5b-9f942d292dc7/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/03dca92e-dc69-11e7-ac5b-9f942d292dc7/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://blog.twitch.tv/watch-and-co-stream-the-game-awards-this-thursday-on-twitch-3d8e34d2345d",
                    "__typename": "Badge"
                },
                {
                    "id": "dmlkZW8tZ2FtZXMtZGF5OzE7",
                    "setID": "video-games-day",
                    "version": "1",
                    "title": "ビデオゲームの日",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/34a57a67-b058-45a9-b088-da681aebc83e/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/34a57a67-b058-45a9-b088-da681aebc83e/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/34a57a67-b058-45a9-b088-da681aebc83e/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://www.twitch.tv/directory/gaming",
                    "__typename": "Badge"
                },
                {
                    "id": "dmlwOzE7",
                    "setID": "vip",
                    "version": "1",
                    "title": "VIP",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/b817aba4-fad8-49e2-b88a-7cc744dfa6ec/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/b817aba4-fad8-49e2-b88a-7cc744dfa6ec/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/b817aba4-fad8-49e2-b88a-7cc744dfa6ec/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "https://help.twitch.tv/customer/en/portal/articles/659115-twitch-chat-badges-guide",
                    "__typename": "Badge"
                },
                {
                    "id": "d2FyY3JhZnQ7YWxsaWFuY2U7",
                    "setID": "warcraft",
                    "version": "alliance",
                    "title": "Alliance",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/c4816339-bad4-4645-ae69-d1ab2076a6b0/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/c4816339-bad4-4645-ae69-d1ab2076a6b0/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/c4816339-bad4-4645-ae69-d1ab2076a6b0/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "http://warcraftontwitch.tv/",
                    "__typename": "Badge"
                },
                {
                    "id": "d2FyY3JhZnQ7aG9yZGU7",
                    "setID": "warcraft",
                    "version": "horde",
                    "title": "Horde",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/de8b26b6-fd28-4e6c-bc89-3d597343800d/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/de8b26b6-fd28-4e6c-bc89-3d597343800d/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/de8b26b6-fd28-4e6c-bc89-3d597343800d/3",
                    "clickAction": "VISIT_URL",
                    "clickURL": "http://warcraftontwitch.tv/",
                    "__typename": "Badge"
                },
                {
                    "id": "emV2ZW50LTIwMjQ7MTs=",
                    "setID": "zevent-2024",
                    "version": "1",
                    "title": "ZEVENT 2024",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/2040d479-b815-4617-8a55-9aed027e30d0/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/2040d479-b815-4617-8a55-9aed027e30d0/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/2040d479-b815-4617-8a55-9aed027e30d0/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                },
                {
                    "id": "emV2ZW50MjU7MTs=",
                    "setID": "zevent25",
                    "version": "1",
                    "title": "ZEVENT25",
                    "image1x": "https://static-cdn.jtvnw.net/badges/v1/7c39aa87-4659-4e8f-abaf-c29614cd8a29/1",
                    "image2x": "https://static-cdn.jtvnw.net/badges/v1/7c39aa87-4659-4e8f-abaf-c29614cd8a29/2",
                    "image4x": "https://static-cdn.jtvnw.net/badges/v1/7c39aa87-4659-4e8f-abaf-c29614cd8a29/3",
                    "clickAction": null,
                    "clickURL": null,
                    "__typename": "Badge"
                }
            ],
            "cheerConfig": {
                "displayConfig": {
                    "backgrounds": [
                        "light",
                        "dark"
                    ],
                    "colors": [
                        {
                            "bits": 1,
                            "color": "#979797",
                            "__typename": "CheermoteColorConfig"
                        },
                        {
                            "bits": 100,
                            "color": "#9c3ee8",
                            "__typename": "CheermoteColorConfig"
                        },
                        {
                            "bits": 1000,
                            "color": "#1db2a5",
                            "__typename": "CheermoteColorConfig"
                        },
                        {
                            "bits": 5000,
                            "color": "#0099fe",
                            "__typename": "CheermoteColorConfig"
                        },
                        {
                            "bits": 10000,
                            "color": "#f43021",
                            "__typename": "CheermoteColorConfig"
                        },
                        {
                            "bits": 100000,
                            "color": "#f3a71a",
                            "__typename": "CheermoteColorConfig"
                        }
                    ],
                    "order": [
                        "SPONSORED",
                        "DEFAULT",
                        "CUSTOM",
                        "CHARITY",
                        "FIRST_PARTY",
                        "THIRD_PARTY",
                        "ANONYMOUS",
                        "DISPLAY_ONLY"
                    ],
                    "scales": [
                        "1",
                        "1.5",
                        "2",
                        "3",
                        "4"
                    ],
                    "types": [
                        {
                            "animation": "static",
                            "extension": "png",
                            "__typename": "CheermoteDisplayType"
                        },
                        {
                            "animation": "animated",
                            "extension": "gif",
                            "__typename": "CheermoteDisplayType"
                        }
                    ],
                    "__typename": "CheermoteDisplayConfig"
                },
                "groups": [
                    {
                        "templateURL": "https://d3aqoihi2n8ty8.cloudfront.net/actions/PREFIX/BACKGROUND/ANIMATION/TIER/SCALE.EXTENSION",
                        "nodes": [
                            {
                                "id": "Cheer",
                                "prefix": "Cheer",
                                "type": "DEFAULT",
                                "campaign": null,
                                "tiers": [
                                    {
                                        "id": "Cheer;1",
                                        "bits": 1,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Cheer;100",
                                        "bits": 100,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Cheer;1000",
                                        "bits": 1000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Cheer;5000",
                                        "bits": 5000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Cheer;10000",
                                        "bits": 10000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Cheer;100000",
                                        "bits": 100000,
                                        "canShowInBitsCard": false,
                                        "__typename": "CheermoteTier"
                                    }
                                ],
                                "__typename": "Cheermote"
                            },
                            {
                                "id": "cheerwhal",
                                "prefix": "cheerwhal",
                                "type": "FIRST_PARTY",
                                "campaign": null,
                                "tiers": [
                                    {
                                        "id": "cheerwhal;1",
                                        "bits": 1,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "cheerwhal;100",
                                        "bits": 100,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "cheerwhal;1000",
                                        "bits": 1000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "cheerwhal;5000",
                                        "bits": 5000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "cheerwhal;10000",
                                        "bits": 10000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    }
                                ],
                                "__typename": "Cheermote"
                            },
                            {
                                "id": "Corgo",
                                "prefix": "Corgo",
                                "type": "FIRST_PARTY",
                                "campaign": null,
                                "tiers": [
                                    {
                                        "id": "Corgo;1",
                                        "bits": 1,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Corgo;100",
                                        "bits": 100,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Corgo;1000",
                                        "bits": 1000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Corgo;5000",
                                        "bits": 5000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Corgo;10000",
                                        "bits": 10000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    }
                                ],
                                "__typename": "Cheermote"
                            },
                            {
                                "id": "Scoops",
                                "prefix": "Scoops",
                                "type": "DISPLAY_ONLY",
                                "campaign": null,
                                "tiers": [
                                    {
                                        "id": "Scoops;1",
                                        "bits": 1,
                                        "canShowInBitsCard": false,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Scoops;100",
                                        "bits": 100,
                                        "canShowInBitsCard": false,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Scoops;1000",
                                        "bits": 1000,
                                        "canShowInBitsCard": false,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Scoops;5000",
                                        "bits": 5000,
                                        "canShowInBitsCard": false,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Scoops;10000",
                                        "bits": 10000,
                                        "canShowInBitsCard": false,
                                        "__typename": "CheermoteTier"
                                    }
                                ],
                                "__typename": "Cheermote"
                            },
                            {
                                "id": "uni",
                                "prefix": "uni",
                                "type": "FIRST_PARTY",
                                "campaign": null,
                                "tiers": [
                                    {
                                        "id": "uni;1",
                                        "bits": 1,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "uni;100",
                                        "bits": 100,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "uni;1000",
                                        "bits": 1000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "uni;5000",
                                        "bits": 5000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "uni;10000",
                                        "bits": 10000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    }
                                ],
                                "__typename": "Cheermote"
                            },
                            {
                                "id": "ShowLove",
                                "prefix": "ShowLove",
                                "type": "FIRST_PARTY",
                                "campaign": null,
                                "tiers": [
                                    {
                                        "id": "ShowLove;1",
                                        "bits": 1,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "ShowLove;100",
                                        "bits": 100,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "ShowLove;1000",
                                        "bits": 1000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "ShowLove;5000",
                                        "bits": 5000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "ShowLove;10000",
                                        "bits": 10000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    }
                                ],
                                "__typename": "Cheermote"
                            },
                            {
                                "id": "Party",
                                "prefix": "Party",
                                "type": "FIRST_PARTY",
                                "campaign": null,
                                "tiers": [
                                    {
                                        "id": "Party;1",
                                        "bits": 1,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Party;100",
                                        "bits": 100,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Party;1000",
                                        "bits": 1000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Party;5000",
                                        "bits": 5000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Party;10000",
                                        "bits": 10000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    }
                                ],
                                "__typename": "Cheermote"
                            },
                            {
                                "id": "SeemsGood",
                                "prefix": "SeemsGood",
                                "type": "FIRST_PARTY",
                                "campaign": null,
                                "tiers": [
                                    {
                                        "id": "SeemsGood;1",
                                        "bits": 1,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "SeemsGood;100",
                                        "bits": 100,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "SeemsGood;1000",
                                        "bits": 1000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "SeemsGood;5000",
                                        "bits": 5000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "SeemsGood;10000",
                                        "bits": 10000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    }
                                ],
                                "__typename": "Cheermote"
                            },
                            {
                                "id": "Pride",
                                "prefix": "Pride",
                                "type": "FIRST_PARTY",
                                "campaign": null,
                                "tiers": [
                                    {
                                        "id": "Pride;1",
                                        "bits": 1,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Pride;100",
                                        "bits": 100,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Pride;1000",
                                        "bits": 1000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Pride;5000",
                                        "bits": 5000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Pride;10000",
                                        "bits": 10000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    }
                                ],
                                "__typename": "Cheermote"
                            },
                            {
                                "id": "Kappa",
                                "prefix": "Kappa",
                                "type": "FIRST_PARTY",
                                "campaign": null,
                                "tiers": [
                                    {
                                        "id": "Kappa;1",
                                        "bits": 1,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Kappa;100",
                                        "bits": 100,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Kappa;1000",
                                        "bits": 1000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Kappa;5000",
                                        "bits": 5000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Kappa;10000",
                                        "bits": 10000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    }
                                ],
                                "__typename": "Cheermote"
                            },
                            {
                                "id": "FrankerZ",
                                "prefix": "FrankerZ",
                                "type": "FIRST_PARTY",
                                "campaign": null,
                                "tiers": [
                                    {
                                        "id": "FrankerZ;1",
                                        "bits": 1,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "FrankerZ;100",
                                        "bits": 100,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "FrankerZ;1000",
                                        "bits": 1000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "FrankerZ;5000",
                                        "bits": 5000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "FrankerZ;10000",
                                        "bits": 10000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    }
                                ],
                                "__typename": "Cheermote"
                            },
                            {
                                "id": "HeyGuys",
                                "prefix": "HeyGuys",
                                "type": "FIRST_PARTY",
                                "campaign": null,
                                "tiers": [
                                    {
                                        "id": "HeyGuys;1",
                                        "bits": 1,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "HeyGuys;100",
                                        "bits": 100,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "HeyGuys;1000",
                                        "bits": 1000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "HeyGuys;5000",
                                        "bits": 5000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "HeyGuys;10000",
                                        "bits": 10000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    }
                                ],
                                "__typename": "Cheermote"
                            },
                            {
                                "id": "DansGame",
                                "prefix": "DansGame",
                                "type": "FIRST_PARTY",
                                "campaign": null,
                                "tiers": [
                                    {
                                        "id": "DansGame;1",
                                        "bits": 1,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "DansGame;100",
                                        "bits": 100,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "DansGame;1000",
                                        "bits": 1000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "DansGame;5000",
                                        "bits": 5000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "DansGame;10000",
                                        "bits": 10000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    }
                                ],
                                "__typename": "Cheermote"
                            },
                            {
                                "id": "TriHard",
                                "prefix": "TriHard",
                                "type": "FIRST_PARTY",
                                "campaign": null,
                                "tiers": [
                                    {
                                        "id": "TriHard;1",
                                        "bits": 1,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "TriHard;100",
                                        "bits": 100,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "TriHard;1000",
                                        "bits": 1000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "TriHard;5000",
                                        "bits": 5000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "TriHard;10000",
                                        "bits": 10000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    }
                                ],
                                "__typename": "Cheermote"
                            },
                            {
                                "id": "Kreygasm",
                                "prefix": "Kreygasm",
                                "type": "FIRST_PARTY",
                                "campaign": null,
                                "tiers": [
                                    {
                                        "id": "Kreygasm;1",
                                        "bits": 1,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Kreygasm;100",
                                        "bits": 100,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Kreygasm;1000",
                                        "bits": 1000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Kreygasm;5000",
                                        "bits": 5000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Kreygasm;10000",
                                        "bits": 10000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    }
                                ],
                                "__typename": "Cheermote"
                            },
                            {
                                "id": "4Head",
                                "prefix": "4Head",
                                "type": "FIRST_PARTY",
                                "campaign": null,
                                "tiers": [
                                    {
                                        "id": "4Head;1",
                                        "bits": 1,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "4Head;100",
                                        "bits": 100,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "4Head;1000",
                                        "bits": 1000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "4Head;5000",
                                        "bits": 5000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "4Head;10000",
                                        "bits": 10000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    }
                                ],
                                "__typename": "Cheermote"
                            },
                            {
                                "id": "SwiftRage",
                                "prefix": "SwiftRage",
                                "type": "FIRST_PARTY",
                                "campaign": null,
                                "tiers": [
                                    {
                                        "id": "SwiftRage;1",
                                        "bits": 1,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "SwiftRage;100",
                                        "bits": 100,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "SwiftRage;1000",
                                        "bits": 1000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "SwiftRage;5000",
                                        "bits": 5000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "SwiftRage;10000",
                                        "bits": 10000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    }
                                ],
                                "__typename": "Cheermote"
                            },
                            {
                                "id": "NotLikeThis",
                                "prefix": "NotLikeThis",
                                "type": "FIRST_PARTY",
                                "campaign": null,
                                "tiers": [
                                    {
                                        "id": "NotLikeThis;1",
                                        "bits": 1,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "NotLikeThis;100",
                                        "bits": 100,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "NotLikeThis;1000",
                                        "bits": 1000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "NotLikeThis;5000",
                                        "bits": 5000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "NotLikeThis;10000",
                                        "bits": 10000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    }
                                ],
                                "__typename": "Cheermote"
                            },
                            {
                                "id": "FailFish",
                                "prefix": "FailFish",
                                "type": "FIRST_PARTY",
                                "campaign": null,
                                "tiers": [
                                    {
                                        "id": "FailFish;1",
                                        "bits": 1,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "FailFish;100",
                                        "bits": 100,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "FailFish;1000",
                                        "bits": 1000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "FailFish;5000",
                                        "bits": 5000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "FailFish;10000",
                                        "bits": 10000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    }
                                ],
                                "__typename": "Cheermote"
                            },
                            {
                                "id": "VoHiYo",
                                "prefix": "VoHiYo",
                                "type": "FIRST_PARTY",
                                "campaign": null,
                                "tiers": [
                                    {
                                        "id": "VoHiYo;1",
                                        "bits": 1,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "VoHiYo;100",
                                        "bits": 100,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "VoHiYo;1000",
                                        "bits": 1000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "VoHiYo;5000",
                                        "bits": 5000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "VoHiYo;10000",
                                        "bits": 10000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    }
                                ],
                                "__typename": "Cheermote"
                            },
                            {
                                "id": "PJSalt",
                                "prefix": "PJSalt",
                                "type": "FIRST_PARTY",
                                "campaign": null,
                                "tiers": [
                                    {
                                        "id": "PJSalt;1",
                                        "bits": 1,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "PJSalt;100",
                                        "bits": 100,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "PJSalt;1000",
                                        "bits": 1000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "PJSalt;5000",
                                        "bits": 5000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "PJSalt;10000",
                                        "bits": 10000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    }
                                ],
                                "__typename": "Cheermote"
                            },
                            {
                                "id": "MrDestructoid",
                                "prefix": "MrDestructoid",
                                "type": "FIRST_PARTY",
                                "campaign": null,
                                "tiers": [
                                    {
                                        "id": "MrDestructoid;1",
                                        "bits": 1,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "MrDestructoid;100",
                                        "bits": 100,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "MrDestructoid;1000",
                                        "bits": 1000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "MrDestructoid;5000",
                                        "bits": 5000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "MrDestructoid;10000",
                                        "bits": 10000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    }
                                ],
                                "__typename": "Cheermote"
                            },
                            {
                                "id": "bday",
                                "prefix": "bday",
                                "type": "FIRST_PARTY",
                                "campaign": null,
                                "tiers": [
                                    {
                                        "id": "bday;1",
                                        "bits": 1,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "bday;100",
                                        "bits": 100,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "bday;1000",
                                        "bits": 1000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "bday;5000",
                                        "bits": 5000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "bday;10000",
                                        "bits": 10000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    }
                                ],
                                "__typename": "Cheermote"
                            },
                            {
                                "id": "RIPCheer",
                                "prefix": "RIPCheer",
                                "type": "FIRST_PARTY",
                                "campaign": null,
                                "tiers": [
                                    {
                                        "id": "RIPCheer;1",
                                        "bits": 1,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "RIPCheer;100",
                                        "bits": 100,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "RIPCheer;1000",
                                        "bits": 1000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "RIPCheer;5000",
                                        "bits": 5000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "RIPCheer;10000",
                                        "bits": 10000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    }
                                ],
                                "__typename": "Cheermote"
                            },
                            {
                                "id": "Shamrock",
                                "prefix": "Shamrock",
                                "type": "FIRST_PARTY",
                                "campaign": null,
                                "tiers": [
                                    {
                                        "id": "Shamrock;1",
                                        "bits": 1,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Shamrock;100",
                                        "bits": 100,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Shamrock;1000",
                                        "bits": 1000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Shamrock;5000",
                                        "bits": 5000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Shamrock;10000",
                                        "bits": 10000,
                                        "canShowInBitsCard": true,
                                        "__typename": "CheermoteTier"
                                    }
                                ],
                                "__typename": "Cheermote"
                            },
                            {
                                "id": "DoodleCheer",
                                "prefix": "DoodleCheer",
                                "type": "THIRD_PARTY",
                                "campaign": null,
                                "tiers": [
                                    {
                                        "id": "DoodleCheer;1",
                                        "bits": 1,
                                        "canShowInBitsCard": false,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "DoodleCheer;100",
                                        "bits": 100,
                                        "canShowInBitsCard": false,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "DoodleCheer;1000",
                                        "bits": 1000,
                                        "canShowInBitsCard": false,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "DoodleCheer;5000",
                                        "bits": 5000,
                                        "canShowInBitsCard": false,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "DoodleCheer;10000",
                                        "bits": 10000,
                                        "canShowInBitsCard": false,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "DoodleCheer;100000",
                                        "bits": 100000,
                                        "canShowInBitsCard": false,
                                        "__typename": "CheermoteTier"
                                    }
                                ],
                                "__typename": "Cheermote"
                            },
                            {
                                "id": "BitBoss",
                                "prefix": "BitBoss",
                                "type": "THIRD_PARTY",
                                "campaign": null,
                                "tiers": [
                                    {
                                        "id": "BitBoss;1",
                                        "bits": 1,
                                        "canShowInBitsCard": false,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "BitBoss;100",
                                        "bits": 100,
                                        "canShowInBitsCard": false,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "BitBoss;1000",
                                        "bits": 1000,
                                        "canShowInBitsCard": false,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "BitBoss;5000",
                                        "bits": 5000,
                                        "canShowInBitsCard": false,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "BitBoss;10000",
                                        "bits": 10000,
                                        "canShowInBitsCard": false,
                                        "__typename": "CheermoteTier"
                                    }
                                ],
                                "__typename": "Cheermote"
                            },
                            {
                                "id": "Streamlabs",
                                "prefix": "Streamlabs",
                                "type": "THIRD_PARTY",
                                "campaign": null,
                                "tiers": [
                                    {
                                        "id": "Streamlabs;1",
                                        "bits": 1,
                                        "canShowInBitsCard": false,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Streamlabs;100",
                                        "bits": 100,
                                        "canShowInBitsCard": false,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Streamlabs;1000",
                                        "bits": 1000,
                                        "canShowInBitsCard": false,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Streamlabs;5000",
                                        "bits": 5000,
                                        "canShowInBitsCard": false,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Streamlabs;10000",
                                        "bits": 10000,
                                        "canShowInBitsCard": false,
                                        "__typename": "CheermoteTier"
                                    }
                                ],
                                "__typename": "Cheermote"
                            },
                            {
                                "id": "Muxy",
                                "prefix": "Muxy",
                                "type": "THIRD_PARTY",
                                "campaign": null,
                                "tiers": [
                                    {
                                        "id": "Muxy;1",
                                        "bits": 1,
                                        "canShowInBitsCard": false,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Muxy;100",
                                        "bits": 100,
                                        "canShowInBitsCard": false,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Muxy;1000",
                                        "bits": 1000,
                                        "canShowInBitsCard": false,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Muxy;5000",
                                        "bits": 5000,
                                        "canShowInBitsCard": false,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Muxy;10000",
                                        "bits": 10000,
                                        "canShowInBitsCard": false,
                                        "__typename": "CheermoteTier"
                                    }
                                ],
                                "__typename": "Cheermote"
                            },
                            {
                                "id": "HolidayCheer",
                                "prefix": "HolidayCheer",
                                "type": "THIRD_PARTY",
                                "campaign": null,
                                "tiers": [
                                    {
                                        "id": "HolidayCheer;1",
                                        "bits": 1,
                                        "canShowInBitsCard": false,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "HolidayCheer;100",
                                        "bits": 100,
                                        "canShowInBitsCard": false,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "HolidayCheer;1000",
                                        "bits": 1000,
                                        "canShowInBitsCard": false,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "HolidayCheer;5000",
                                        "bits": 5000,
                                        "canShowInBitsCard": false,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "HolidayCheer;10000",
                                        "bits": 10000,
                                        "canShowInBitsCard": false,
                                        "__typename": "CheermoteTier"
                                    }
                                ],
                                "__typename": "Cheermote"
                            },
                            {
                                "id": "Goal",
                                "prefix": "Goal",
                                "type": "THIRD_PARTY",
                                "campaign": null,
                                "tiers": [
                                    {
                                        "id": "Goal;1",
                                        "bits": 1,
                                        "canShowInBitsCard": false,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Goal;100",
                                        "bits": 100,
                                        "canShowInBitsCard": false,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Goal;1000",
                                        "bits": 1000,
                                        "canShowInBitsCard": false,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Goal;5000",
                                        "bits": 5000,
                                        "canShowInBitsCard": false,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Goal;10000",
                                        "bits": 10000,
                                        "canShowInBitsCard": false,
                                        "__typename": "CheermoteTier"
                                    }
                                ],
                                "__typename": "Cheermote"
                            },
                            {
                                "id": "Anon",
                                "prefix": "Anon",
                                "type": "DISPLAY_ONLY",
                                "campaign": null,
                                "tiers": [
                                    {
                                        "id": "Anon;1",
                                        "bits": 1,
                                        "canShowInBitsCard": false,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Anon;100",
                                        "bits": 100,
                                        "canShowInBitsCard": false,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Anon;1000",
                                        "bits": 1000,
                                        "canShowInBitsCard": false,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Anon;5000",
                                        "bits": 5000,
                                        "canShowInBitsCard": false,
                                        "__typename": "CheermoteTier"
                                    },
                                    {
                                        "id": "Anon;10000",
                                        "bits": 10000,
                                        "canShowInBitsCard": false,
                                        "__typename": "CheermoteTier"
                                    }
                                ],
                                "__typename": "Cheermote"
                            }
                        ],
                        "__typename": "CheermoteGroup"
                    }
                ],
                "__typename": "GlobalCheerConfig"
            },
            "__typename": "Query",
            "currentUser": {
                "id": "115620888",
                "roles": {
                    "isStaff": null,
                    "isSiteAdmin": null,
                    "isGlobalMod": null,
                    "__typename": "UserRoles"
                },
                "blockedUsers": [
                    {
                        "id": "1180224968",
                        "__typename": "User"
                    },
                    {
                        "id": "1012471429",
                        "__typename": "User"
                    },
                    {
                        "id": "845114989",
                        "__typename": "User"
                    },
                    null,
                    {
                        "id": "964133177",
                        "__typename": "User"
                    },
                    {
                        "id": "771645615",
                        "__typename": "User"
                    },
                    {
                        "id": "927313042",
                        "__typename": "User"
                    },
                    {
                        "id": "897566495",
                        "__typename": "User"
                    },
                    {
                        "id": "677326237",
                        "__typename": "User"
                    }
                ],
                "__typename": "User"
            },
            "video": {
                "id": "2669748313",
                "broadcastType": "ARCHIVE",
                "lengthSeconds": 18087,
                "owner": {
                    "id": "1016942766",
                    "login": "kyoyu_shiroya",
                    "broadcastBadges": [
                        {
                            "id": "c3Vic2NyaWJlcjsyMDAwOzEwMTY5NDI3NjY=",
                            "setID": "subscriber",
                            "version": "2000",
                            "title": "Subscriber",
                            "image1x": "https://static-cdn.jtvnw.net/badges/v1/4a66a744-115f-49aa-87af-94739e7e0b84/1",
                            "image2x": "https://static-cdn.jtvnw.net/badges/v1/4a66a744-115f-49aa-87af-94739e7e0b84/2",
                            "image4x": "https://static-cdn.jtvnw.net/badges/v1/4a66a744-115f-49aa-87af-94739e7e0b84/3",
                            "clickAction": "SUBSCRIBE",
                            "clickURL": null,
                            "__typename": "Badge"
                        },
                        {
                            "id": "c3Vic2NyaWJlcjszMDI0OzEwMTY5NDI3NjY=",
                            "setID": "subscriber",
                            "version": "3024",
                            "title": "2-Year Subscriber",
                            "image1x": "https://static-cdn.jtvnw.net/badges/v1/a59c8bf1-75d0-4d5b-873f-f933eb964ac1/1",
                            "image2x": "https://static-cdn.jtvnw.net/badges/v1/a59c8bf1-75d0-4d5b-873f-f933eb964ac1/2",
                            "image4x": "https://static-cdn.jtvnw.net/badges/v1/a59c8bf1-75d0-4d5b-873f-f933eb964ac1/3",
                            "clickAction": "SUBSCRIBE",
                            "clickURL": null,
                            "__typename": "Badge"
                        },
                        {
                            "id": "c3Vic2NyaWJlcjsxMjsxMDE2OTQyNzY2",
                            "setID": "subscriber",
                            "version": "12",
                            "title": "1年サブスクライバー",
                            "image1x": "https://static-cdn.jtvnw.net/badges/v1/d50fd146-7c3f-4a0d-af85-273726285596/1",
                            "image2x": "https://static-cdn.jtvnw.net/badges/v1/d50fd146-7c3f-4a0d-af85-273726285596/2",
                            "image4x": "https://static-cdn.jtvnw.net/badges/v1/d50fd146-7c3f-4a0d-af85-273726285596/3",
                            "clickAction": "SUBSCRIBE",
                            "clickURL": null,
                            "__typename": "Badge"
                        },
                        {
                            "id": "c3Vic2NyaWJlcjs2OzEwMTY5NDI3NjY=",
                            "setID": "subscriber",
                            "version": "6",
                            "title": "6ヵ月サブスクライバー",
                            "image1x": "https://static-cdn.jtvnw.net/badges/v1/1e5807fa-110f-423a-b5c3-9dc114ca9719/1",
                            "image2x": "https://static-cdn.jtvnw.net/badges/v1/1e5807fa-110f-423a-b5c3-9dc114ca9719/2",
                            "image4x": "https://static-cdn.jtvnw.net/badges/v1/1e5807fa-110f-423a-b5c3-9dc114ca9719/3",
                            "clickAction": "SUBSCRIBE",
                            "clickURL": null,
                            "__typename": "Badge"
                        },
                        {
                            "id": "c3Vic2NyaWJlcjsyMDAyOzEwMTY5NDI3NjY=",
                            "setID": "subscriber",
                            "version": "2002",
                            "title": "2-Month Subscriber",
                            "image1x": "https://static-cdn.jtvnw.net/badges/v1/8f158a7f-03a5-46e5-a7af-7e1659a095de/1",
                            "image2x": "https://static-cdn.jtvnw.net/badges/v1/8f158a7f-03a5-46e5-a7af-7e1659a095de/2",
                            "image4x": "https://static-cdn.jtvnw.net/badges/v1/8f158a7f-03a5-46e5-a7af-7e1659a095de/3",
                            "clickAction": "SUBSCRIBE",
                            "clickURL": null,
                            "__typename": "Badge"
                        },
                        {
                            "id": "c3Vic2NyaWJlcjsyMDA2OzEwMTY5NDI3NjY=",
                            "setID": "subscriber",
                            "version": "2006",
                            "title": "6-Month Subscriber",
                            "image1x": "https://static-cdn.jtvnw.net/badges/v1/7e682939-c170-4178-b4af-43be77cd57b3/1",
                            "image2x": "https://static-cdn.jtvnw.net/badges/v1/7e682939-c170-4178-b4af-43be77cd57b3/2",
                            "image4x": "https://static-cdn.jtvnw.net/badges/v1/7e682939-c170-4178-b4af-43be77cd57b3/3",
                            "clickAction": "SUBSCRIBE",
                            "clickURL": null,
                            "__typename": "Badge"
                        },
                        {
                            "id": "c3Vic2NyaWJlcjswOzEwMTY5NDI3NjY=",
                            "setID": "subscriber",
                            "version": "0",
                            "title": "サブスクライバー",
                            "image1x": "https://static-cdn.jtvnw.net/badges/v1/d9d49cc6-e6e7-4573-9dfd-a98cee640a36/1",
                            "image2x": "https://static-cdn.jtvnw.net/badges/v1/d9d49cc6-e6e7-4573-9dfd-a98cee640a36/2",
                            "image4x": "https://static-cdn.jtvnw.net/badges/v1/d9d49cc6-e6e7-4573-9dfd-a98cee640a36/3",
                            "clickAction": "SUBSCRIBE",
                            "clickURL": null,
                            "__typename": "Badge"
                        },
                        {
                            "id": "c3Vic2NyaWJlcjsyOzEwMTY5NDI3NjY=",
                            "setID": "subscriber",
                            "version": "2",
                            "title": "2ヶ月のサブスクライバー",
                            "image1x": "https://static-cdn.jtvnw.net/badges/v1/56cc05de-182b-4530-838e-101cf5c476ba/1",
                            "image2x": "https://static-cdn.jtvnw.net/badges/v1/56cc05de-182b-4530-838e-101cf5c476ba/2",
                            "image4x": "https://static-cdn.jtvnw.net/badges/v1/56cc05de-182b-4530-838e-101cf5c476ba/3",
                            "clickAction": "SUBSCRIBE",
                            "clickURL": null,
                            "__typename": "Badge"
                        },
                        {
                            "id": "c3Vic2NyaWJlcjszMDAyOzEwMTY5NDI3NjY=",
                            "setID": "subscriber",
                            "version": "3002",
                            "title": "2-Month Subscriber",
                            "image1x": "https://static-cdn.jtvnw.net/badges/v1/90e282e9-f2dc-4ecb-9880-58944b387132/1",
                            "image2x": "https://static-cdn.jtvnw.net/badges/v1/90e282e9-f2dc-4ecb-9880-58944b387132/2",
                            "image4x": "https://static-cdn.jtvnw.net/badges/v1/90e282e9-f2dc-4ecb-9880-58944b387132/3",
                            "clickAction": "SUBSCRIBE",
                            "clickURL": null,
                            "__typename": "Badge"
                        },
                        {
                            "id": "c3Vic2NyaWJlcjszMDEyOzEwMTY5NDI3NjY=",
                            "setID": "subscriber",
                            "version": "3012",
                            "title": "1-Year Subscriber",
                            "image1x": "https://static-cdn.jtvnw.net/badges/v1/e8b3ec33-d6dd-47ea-af85-123b7666e0af/1",
                            "image2x": "https://static-cdn.jtvnw.net/badges/v1/e8b3ec33-d6dd-47ea-af85-123b7666e0af/2",
                            "image4x": "https://static-cdn.jtvnw.net/badges/v1/e8b3ec33-d6dd-47ea-af85-123b7666e0af/3",
                            "clickAction": "SUBSCRIBE",
                            "clickURL": null,
                            "__typename": "Badge"
                        },
                        {
                            "id": "c3Vic2NyaWJlcjszMDAwOzEwMTY5NDI3NjY=",
                            "setID": "subscriber",
                            "version": "3000",
                            "title": "Subscriber",
                            "image1x": "https://static-cdn.jtvnw.net/badges/v1/0c470ba1-29e9-4c85-afff-373d686d83fa/1",
                            "image2x": "https://static-cdn.jtvnw.net/badges/v1/0c470ba1-29e9-4c85-afff-373d686d83fa/2",
                            "image4x": "https://static-cdn.jtvnw.net/badges/v1/0c470ba1-29e9-4c85-afff-373d686d83fa/3",
                            "clickAction": "SUBSCRIBE",
                            "clickURL": null,
                            "__typename": "Badge"
                        },
                        {
                            "id": "c3Vic2NyaWJlcjsyMDEyOzEwMTY5NDI3NjY=",
                            "setID": "subscriber",
                            "version": "2012",
                            "title": "1-Year Subscriber",
                            "image1x": "https://static-cdn.jtvnw.net/badges/v1/0207814e-2a0c-4a33-8358-a7606ba4d2ff/1",
                            "image2x": "https://static-cdn.jtvnw.net/badges/v1/0207814e-2a0c-4a33-8358-a7606ba4d2ff/2",
                            "image4x": "https://static-cdn.jtvnw.net/badges/v1/0207814e-2a0c-4a33-8358-a7606ba4d2ff/3",
                            "clickAction": "SUBSCRIBE",
                            "clickURL": null,
                            "__typename": "Badge"
                        },
                        {
                            "id": "c3Vic2NyaWJlcjsyNDsxMDE2OTQyNzY2",
                            "setID": "subscriber",
                            "version": "24",
                            "title": "2年サブスクライバー",
                            "image1x": "https://static-cdn.jtvnw.net/badges/v1/431cf4c9-b072-4f1c-89d1-941813b9d4b2/1",
                            "image2x": "https://static-cdn.jtvnw.net/badges/v1/431cf4c9-b072-4f1c-89d1-941813b9d4b2/2",
                            "image4x": "https://static-cdn.jtvnw.net/badges/v1/431cf4c9-b072-4f1c-89d1-941813b9d4b2/3",
                            "clickAction": "SUBSCRIBE",
                            "clickURL": null,
                            "__typename": "Badge"
                        },
                        {
                            "id": "c3Vic2NyaWJlcjszMDA2OzEwMTY5NDI3NjY=",
                            "setID": "subscriber",
                            "version": "3006",
                            "title": "6-Month Subscriber",
                            "image1x": "https://static-cdn.jtvnw.net/badges/v1/063c3cc6-531e-484d-a63a-b428b0be12f6/1",
                            "image2x": "https://static-cdn.jtvnw.net/badges/v1/063c3cc6-531e-484d-a63a-b428b0be12f6/2",
                            "image4x": "https://static-cdn.jtvnw.net/badges/v1/063c3cc6-531e-484d-a63a-b428b0be12f6/3",
                            "clickAction": "SUBSCRIBE",
                            "clickURL": null,
                            "__typename": "Badge"
                        },
                        {
                            "id": "c3Vic2NyaWJlcjsyMDI0OzEwMTY5NDI3NjY=",
                            "setID": "subscriber",
                            "version": "2024",
                            "title": "2-Year Subscriber",
                            "image1x": "https://static-cdn.jtvnw.net/badges/v1/6a8c9426-a97a-471f-99ac-3facf717b82f/1",
                            "image2x": "https://static-cdn.jtvnw.net/badges/v1/6a8c9426-a97a-471f-99ac-3facf717b82f/2",
                            "image4x": "https://static-cdn.jtvnw.net/badges/v1/6a8c9426-a97a-471f-99ac-3facf717b82f/3",
                            "clickAction": "SUBSCRIBE",
                            "clickURL": null,
                            "__typename": "Badge"
                        }
                    ],
                    "cheer": {
                        "id": "1016942766",
                        "cheerGroups": [
                            {
                                "templateURL": "https://d3aqoihi2n8ty8.cloudfront.net/actions/PREFIX/BACKGROUND/ANIMATION/TIER/SCALE.EXTENSION",
                                "nodes": [
                                    {
                                        "id": "Charity",
                                        "prefix": "Charity",
                                        "type": "DISPLAY_ONLY",
                                        "campaign": null,
                                        "tiers": [
                                            {
                                                "id": "Charity;1",
                                                "bits": 1,
                                                "canShowInBitsCard": false,
                                                "__typename": "CheermoteTier"
                                            },
                                            {
                                                "id": "Charity;100",
                                                "bits": 100,
                                                "canShowInBitsCard": false,
                                                "__typename": "CheermoteTier"
                                            },
                                            {
                                                "id": "Charity;1000",
                                                "bits": 1000,
                                                "canShowInBitsCard": false,
                                                "__typename": "CheermoteTier"
                                            },
                                            {
                                                "id": "Charity;5000",
                                                "bits": 5000,
                                                "canShowInBitsCard": false,
                                                "__typename": "CheermoteTier"
                                            },
                                            {
                                                "id": "Charity;10000",
                                                "bits": 10000,
                                                "canShowInBitsCard": false,
                                                "__typename": "CheermoteTier"
                                            }
                                        ],
                                        "__typename": "Cheermote"
                                    }
                                ],
                                "__typename": "CheermoteGroup"
                            }
                        ],
                        "__typename": "CheerInfo"
                    },
                    "__typename": "User",
                    "self": {
                        "isModerator": false,
                        "__typename": "UserSelfConnection"
                    }
                },
                "__typename": "Video"
            },
            "requestInfo": {
                "countryCode": "JP",
                "__typename": "RequestInfo"
            }
        },
        "extensions": {
            "durationMilliseconds": 49,
            "operationName": "VideoComments",
            "requestID": "01KF73A8ZPCJG0HR3WP7PCNGXG"
        }
    }
```

### VideoCommentsByOffsetOrCursor
payload

offset
```
{
  "operationName": "VideoCommentsByOffsetOrCursor",
  "variables": {
    "videoID": "2669748313",
    "contentOffsetSeconds": 13649
  },
  "extensions": {
    "persistedQuery": {
      "version": 1,
      "sha256Hash": "b70a3591ff0f4e0313d126c6a1502d79a1c02baebb288227c582044aa76adf6a"
    }
  }
}
```
cursor
```
{
  "operationName": "VideoCommentsByOffsetOrCursor",
  "variables": {
    "videoID": "2669748313",
    "cursor": "eyJpZCI6IjMxZGFmYjc2LTliZDYtNDM1ZC1iYTFmLTVlM2IwYzlmMmFlYyIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBSVpKVEp1WUFZaXFMbnR1WE9nQSJ9"
  },
  "extensions": {
    "persistedQuery": {
      "version": 1,
      "sha256Hash": "b70a3591ff0f4e0313d126c6a1502d79a1c02baebb288227c582044aa76adf6a"
    }
  }
}
```

response
```
{
    "data": {
        "video": {
            "id": "2669748313",
            "creator": {
                "id": "1016942766",
                "channel": {
                    "id": "1016942766",
                    "__typename": "Channel"
                },
                "__typename": "User"
            },
            "comments": {
                "edges": [
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "dc8317e7-72f0-4cda-9783-e9822901c0b1",
                            "commenter": {
                                "id": "994734543",
                                "login": "eriko_________",
                                "displayName": "eriko_________",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13570,
                            "createdAt": "2026-01-14T16:47:42.612Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "www",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "c3Vic2NyaWJlcjs2Ow==",
                                        "setID": "subscriber",
                                        "version": "6",
                                        "__typename": "Badge"
                                    },
                                    {
                                        "id": "Yml0czsxMDAwOw==",
                                        "setID": "bits",
                                        "version": "1000",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": "#FF69B4",
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "a33f3481-dbf3-45ac-8571-3a28aa266569",
                            "commenter": {
                                "id": "976192223",
                                "login": "itu_hal",
                                "displayName": "itu_hal",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13572,
                            "createdAt": "2026-01-14T16:47:43.942Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "んこｗｗ",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "c3Vic2NyaWJlcjs2Ow==",
                                        "setID": "subscriber",
                                        "version": "6",
                                        "__typename": "Badge"
                                    },
                                    {
                                        "id": "Yml0czsxMDA7",
                                        "setID": "bits",
                                        "version": "100",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": null,
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "cb89b07f-370c-4bb8-8d5d-5e2b5e84881b",
                            "commenter": {
                                "id": "462358546",
                                "login": "pon2149",
                                "displayName": "ぽんぽんぺ",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13573,
                            "createdAt": "2026-01-14T16:47:44.757Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "トナカイ、一瞬も口癖やろw",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "c3Vic2NyaWJlcjs2Ow==",
                                        "setID": "subscriber",
                                        "version": "6",
                                        "__typename": "Badge"
                                    },
                                    {
                                        "id": "bG93OzE7",
                                        "setID": "low",
                                        "version": "1",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": "#FF69B4",
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "c1dadf59-a293-45ae-b280-91644eb7c3a8",
                            "commenter": {
                                "id": "1019157982",
                                "login": "doragon_fry",
                                "displayName": "ずぶずぶタートル",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13573,
                            "createdAt": "2026-01-14T16:47:45.289Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "コルトピー！",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "Ozs=",
                                        "setID": "",
                                        "version": "",
                                        "__typename": "Badge"
                                    },
                                    {
                                        "id": "cHJlbWl1bTsxOw==",
                                        "setID": "premium",
                                        "version": "1",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": null,
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "aacb6967-5afb-4ea0-ab2a-8b64fdb57d5a",
                            "commenter": {
                                "id": "1060885953",
                                "login": "azukinodaizu",
                                "displayName": "azukinodaizu",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13577,
                            "createdAt": "2026-01-14T16:47:49.545Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "コルトピw",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "c3Vic2NyaWJlcjs2Ow==",
                                        "setID": "subscriber",
                                        "version": "6",
                                        "__typename": "Badge"
                                    },
                                    {
                                        "id": "dHdpdGNoLXJlY2FwLTIwMjQ7MTs=",
                                        "setID": "twitch-recap-2024",
                                        "version": "1",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": "#0000FF",
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "fdf9deac-c3b6-4ac9-8e6d-2519a9758811",
                            "commenter": {
                                "id": "1070751031",
                                "login": "thiunn",
                                "displayName": "thiunn",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13592,
                            "createdAt": "2026-01-14T16:48:04.109Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "トニーの一瞬長くなりがち",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "c3Vic2NyaWJlcjs2Ow==",
                                        "setID": "subscriber",
                                        "version": "6",
                                        "__typename": "Badge"
                                    },
                                    {
                                        "id": "dHVyYm87MTs=",
                                        "setID": "turbo",
                                        "version": "1",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": null,
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "4dd90dc9-2f81-446c-a4b0-1534c1319eea",
                            "commenter": {
                                "id": "462358546",
                                "login": "pon2149",
                                "displayName": "ぽんぽんぺ",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13602,
                            "createdAt": "2026-01-14T16:48:14.295Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "一瞬って言った時に一瞬だった試しが無いぞw",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "c3Vic2NyaWJlcjs2Ow==",
                                        "setID": "subscriber",
                                        "version": "6",
                                        "__typename": "Badge"
                                    },
                                    {
                                        "id": "bG93OzE7",
                                        "setID": "low",
                                        "version": "1",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": "#FF69B4",
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "12921c87-4340-4fe0-96bd-39e051ff8ba6",
                            "commenter": {
                                "id": "800969388",
                                "login": "nya_cat2022",
                                "displayName": "にゃかご",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13609,
                            "createdAt": "2026-01-14T16:48:20.911Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": {
                                            "id": "emotesv2_3847cdcd19cb483390a58bb4ed868d83;0;9",
                                            "emoteID": "emotesv2_3847cdcd19cb483390a58bb4ed868d83",
                                            "from": 0,
                                            "__typename": "EmbeddedEmote"
                                        },
                                        "text": "kyoyusNiya",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "c3Vic2NyaWJlcjsyOw==",
                                        "setID": "subscriber",
                                        "version": "2",
                                        "__typename": "Badge"
                                    },
                                    {
                                        "id": "cHJlbWl1bTsxOw==",
                                        "setID": "premium",
                                        "version": "1",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": null,
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "c79ad10e-4398-4260-bf82-e37de47d8cb2",
                            "commenter": {
                                "id": "927279021",
                                "login": "sakusannorusein",
                                "displayName": "sakusannorusein",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13609,
                            "createdAt": "2026-01-14T16:48:21.237Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "コルトピ?",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "c3Vic2NyaWJlcjs2Ow==",
                                        "setID": "subscriber",
                                        "version": "6",
                                        "__typename": "Badge"
                                    },
                                    {
                                        "id": "Yml0czsxMDA7",
                                        "setID": "bits",
                                        "version": "100",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": "#00FF7F",
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "9a3eca19-f3d4-44d6-9a86-7d695ca9a3a1",
                            "commenter": {
                                "id": "755778774",
                                "login": "fountain31",
                                "displayName": "fountain31",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13611,
                            "createdAt": "2026-01-14T16:48:23.379Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "やめてぇw",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "c3Vic2NyaWJlcjsyOw==",
                                        "setID": "subscriber",
                                        "version": "2",
                                        "__typename": "Badge"
                                    },
                                    {
                                        "id": "c3ViLWdpZnRlcjsxMDs=",
                                        "setID": "sub-gifter",
                                        "version": "10",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": "#1E90FF",
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "3e73b655-e701-4487-a723-b8b469a93900",
                            "commenter": {
                                "id": "143769207",
                                "login": "noi_lol",
                                "displayName": "のい",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13612,
                            "createdAt": "2026-01-14T16:48:24.355Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "確かにいつも一瞬って言うよな離れる時w",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "c3Vic2NyaWJlcjsyOw==",
                                        "setID": "subscriber",
                                        "version": "2",
                                        "__typename": "Badge"
                                    },
                                    {
                                        "id": "dHdpdGNoLXJlY2FwLTIwMjM7MTs=",
                                        "setID": "twitch-recap-2023",
                                        "version": "1",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": "#9ACD32",
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "8aebf253-0fb3-4a7a-98f4-d9686f946c88",
                            "commenter": {
                                "id": "714510069",
                                "login": "hoshi_jump",
                                "displayName": "hoshi_jump",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13639,
                            "createdAt": "2026-01-14T16:48:51.462Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "www",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "c3Vic2NyaWJlcjs2Ow==",
                                        "setID": "subscriber",
                                        "version": "6",
                                        "__typename": "Badge"
                                    },
                                    {
                                        "id": "c3VidGVtYmVyLTIwMjU7MTs=",
                                        "setID": "subtember-2025",
                                        "version": "1",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": "#FF69B4",
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "97d85f3f-cf5b-4321-9a5b-365edf696f35",
                            "commenter": {
                                "id": "768326248",
                                "login": "rerto1114",
                                "displayName": "微助っ人",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13645,
                            "createdAt": "2026-01-14T16:48:57.327Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "裏でアレ連呼して欲求を",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "dHVyYm87MTs=",
                                        "setID": "turbo",
                                        "version": "1",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": null,
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "4f6eb983-2d2d-4f76-bb75-c225a715af2a",
                            "commenter": {
                                "id": "558739650",
                                "login": "hikage000",
                                "displayName": "hikage000",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13646,
                            "createdAt": "2026-01-14T16:48:57.993Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "ポンポンペインか?",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [],
                                "userColor": null,
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "6cbb485a-55b0-4a29-a36a-850bee3b59dc",
                            "commenter": {
                                "id": "720088631",
                                "login": "mayugenekohachi",
                                "displayName": "眉毛猫八",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13647,
                            "createdAt": "2026-01-14T16:48:59.047Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "心配にもなるレベルですね",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "c3Vic2NyaWJlcjs2Ow==",
                                        "setID": "subscriber",
                                        "version": "6",
                                        "__typename": "Badge"
                                    },
                                    {
                                        "id": "cHJlbWl1bTsxOw==",
                                        "setID": "premium",
                                        "version": "1",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": "#FF69B4",
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "fecf44d5-a642-45c5-8ee3-d520b547d598",
                            "commenter": {
                                "id": "709903122",
                                "login": "ichi84_",
                                "displayName": "一条の端くれ",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13651,
                            "createdAt": "2026-01-14T16:49:03.291Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "家広すぎか",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "Ozs=",
                                        "setID": "",
                                        "version": "",
                                        "__typename": "Badge"
                                    },
                                    {
                                        "id": "Yml0czsyNTAwMDs=",
                                        "setID": "bits",
                                        "version": "25000",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": "#FF6600",
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "56d011fa-2546-4c96-a655-14e45cad525f",
                            "commenter": {
                                "id": "671229309",
                                "login": "kzdenki",
                                "displayName": "kzdenki",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13667,
                            "createdAt": "2026-01-14T16:49:18.806Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "配慮がねwww",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [],
                                "userColor": null,
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "b377d73d-9151-4de5-ae0a-54352fe6328d",
                            "commenter": {
                                "id": "527081708",
                                "login": "boccoro",
                                "displayName": "boccoro",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13677,
                            "createdAt": "2026-01-14T16:49:29.141Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "なら仕方ない",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "c3Vic2NyaWJlcjsyOw==",
                                        "setID": "subscriber",
                                        "version": "2",
                                        "__typename": "Badge"
                                    },
                                    {
                                        "id": "dHVyYm87MTs=",
                                        "setID": "turbo",
                                        "version": "1",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": "#1E90FF",
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "d0bf1e18-2e44-4670-91a3-ab4b92fdae2b",
                            "commenter": {
                                "id": "795098931",
                                "login": "chemical_73",
                                "displayName": "chemical_73",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13678,
                            "createdAt": "2026-01-14T16:49:30.258Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "普通にw",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "Ozs=",
                                        "setID": "",
                                        "version": "",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": "#8A2BE2",
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "0a661d98-027f-473e-a959-71536d2c9f59",
                            "commenter": {
                                "id": "967596020",
                                "login": "menma_m_",
                                "displayName": "menma_m_",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13683,
                            "createdAt": "2026-01-14T16:49:35.143Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "ほなしゃーない",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "Ozs=",
                                        "setID": "",
                                        "version": "",
                                        "__typename": "Badge"
                                    },
                                    {
                                        "id": "Yml0czsxMDAwOw==",
                                        "setID": "bits",
                                        "version": "1000",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": "#B22222",
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "66f1c9cb-6d77-4167-b5ad-8e1c1aafbc89",
                            "commenter": {
                                "id": "177216447",
                                "login": "0825_tono",
                                "displayName": "るーとの",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13684,
                            "createdAt": "2026-01-14T16:49:35.942Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "生理現象はしゃーない",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "c3Vic2NyaWJlcjs2Ow==",
                                        "setID": "subscriber",
                                        "version": "6",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": null,
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "f8219654-2032-4fa4-8e9d-3449d3697939",
                            "commenter": {
                                "id": "720088631",
                                "login": "mayugenekohachi",
                                "displayName": "眉毛猫八",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13684,
                            "createdAt": "2026-01-14T16:49:36.316Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "許されました",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "c3Vic2NyaWJlcjs2Ow==",
                                        "setID": "subscriber",
                                        "version": "6",
                                        "__typename": "Badge"
                                    },
                                    {
                                        "id": "cHJlbWl1bTsxOw==",
                                        "setID": "premium",
                                        "version": "1",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": "#FF69B4",
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "a6502ed3-942c-4148-b920-91108bd1830d",
                            "commenter": {
                                "id": "820299420",
                                "login": "lyncis_2525",
                                "displayName": "更待りんくす",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13685,
                            "createdAt": "2026-01-14T16:49:36.971Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "許されたｗｗ",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "c3Vic2NyaWJlcjsyOw==",
                                        "setID": "subscriber",
                                        "version": "2",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": "#B22222",
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "f7e24584-730e-45ae-a71d-d2f16f13ab7d",
                            "commenter": {
                                "id": "984850013",
                                "login": "cheeeeetra319",
                                "displayName": "チートラ",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13686,
                            "createdAt": "2026-01-14T16:49:37.94Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "あれだったんだよ、あれ！って期待してたのに",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "c3Vic2NyaWJlcjswOw==",
                                        "setID": "subscriber",
                                        "version": "0",
                                        "__typename": "Badge"
                                    },
                                    {
                                        "id": "Y2xpcHMtbGVhZGVyOzE7",
                                        "setID": "clips-leader",
                                        "version": "1",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": "#FF4500",
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "4499d2e4-f8b8-4443-ae06-6639496edbf6",
                            "commenter": {
                                "id": "1029776657",
                                "login": "meimei51",
                                "displayName": "meimei51",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13686,
                            "createdAt": "2026-01-14T16:49:38.06Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "仕方ないw",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "Ozs=",
                                        "setID": "",
                                        "version": "",
                                        "__typename": "Badge"
                                    },
                                    {
                                        "id": "Yml0czszMDAwMDA7",
                                        "setID": "bits",
                                        "version": "300000",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": "#FF4500",
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "0e351eb1-b838-4c65-ae82-d1cb43f76f4c",
                            "commenter": {
                                "id": "1233719684",
                                "login": "myu_mdg_3",
                                "displayName": "美有希",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13694,
                            "createdAt": "2026-01-14T16:49:46.523Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "痛かったならしゃーないw",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "c3Vic2NyaWJlcjs2Ow==",
                                        "setID": "subscriber",
                                        "version": "6",
                                        "__typename": "Badge"
                                    },
                                    {
                                        "id": "dHdpdGNoLXJlY2FwLTIwMjU7MTs=",
                                        "setID": "twitch-recap-2025",
                                        "version": "1",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": "#FF69B4",
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "aa945124-d42c-4ddb-b79b-eb99f4a6ff23",
                            "commenter": {
                                "id": "462358546",
                                "login": "pon2149",
                                "displayName": "ぽんぽんぺ",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13705,
                            "createdAt": "2026-01-14T16:49:57.426Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "うんこは仕方ない",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "c3Vic2NyaWJlcjs2Ow==",
                                        "setID": "subscriber",
                                        "version": "6",
                                        "__typename": "Badge"
                                    },
                                    {
                                        "id": "bG93OzE7",
                                        "setID": "low",
                                        "version": "1",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": "#FF69B4",
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "7e6d5031-35a5-4742-ad09-34ec6a866ff2",
                            "commenter": {
                                "id": "143769207",
                                "login": "noi_lol",
                                "displayName": "のい",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13711,
                            "createdAt": "2026-01-14T16:50:02.987Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "www",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "c3Vic2NyaWJlcjsyOw==",
                                        "setID": "subscriber",
                                        "version": "2",
                                        "__typename": "Badge"
                                    },
                                    {
                                        "id": "dHdpdGNoLXJlY2FwLTIwMjM7MTs=",
                                        "setID": "twitch-recap-2023",
                                        "version": "1",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": "#9ACD32",
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "ff869b44-4ff7-4e9d-9382-e15dc03f7091",
                            "commenter": {
                                "id": "671229309",
                                "login": "kzdenki",
                                "displayName": "kzdenki",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13718,
                            "createdAt": "2026-01-14T16:50:10.473Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "ウンコマンwww",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [],
                                "userColor": null,
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "5b7c93ee-ebc0-4f59-a2f0-44d467f1b161",
                            "commenter": {
                                "id": "428061267",
                                "login": "you__ren",
                                "displayName": "you__ren",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13729,
                            "createdAt": "2026-01-14T16:50:21.652Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "普通にお手洗いな",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "Ozs=",
                                        "setID": "",
                                        "version": "",
                                        "__typename": "Badge"
                                    },
                                    {
                                        "id": "dHVyYm87MTs=",
                                        "setID": "turbo",
                                        "version": "1",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": "#FF69B4",
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "aab59de8-6fe9-4d26-ac5c-6c48ec915dd7",
                            "commenter": {
                                "id": "177216447",
                                "login": "0825_tono",
                                "displayName": "るーとの",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13763,
                            "createdAt": "2026-01-14T16:50:55.417Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "なんなら連呼してたww",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "c3Vic2NyaWJlcjs2Ow==",
                                        "setID": "subscriber",
                                        "version": "6",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": null,
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "b9973ed5-cccc-4977-8e12-29ce91e6d71a",
                            "commenter": {
                                "id": "961256224",
                                "login": "yoshimaru440794",
                                "displayName": "よしまる794",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13763,
                            "createdAt": "2026-01-14T16:50:55.592Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "普通にお手洗いかーw",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "c3Vic2NyaWJlcjs2Ow==",
                                        "setID": "subscriber",
                                        "version": "6",
                                        "__typename": "Badge"
                                    },
                                    {
                                        "id": "Yml0czsxMDA7",
                                        "setID": "bits",
                                        "version": "100",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": "#DAA520",
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "9cae9397-1748-4516-95e5-ec541374ddb4",
                            "commenter": {
                                "id": "961256224",
                                "login": "yoshimaru440794",
                                "displayName": "よしまる794",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13777,
                            "createdAt": "2026-01-14T16:51:08.937Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "あ",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "c3Vic2NyaWJlcjs2Ow==",
                                        "setID": "subscriber",
                                        "version": "6",
                                        "__typename": "Badge"
                                    },
                                    {
                                        "id": "Yml0czsxMDA7",
                                        "setID": "bits",
                                        "version": "100",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": "#DAA520",
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "f688b7b8-eb13-4f32-be02-307845f3a2d9",
                            "commenter": {
                                "id": "671229309",
                                "login": "kzdenki",
                                "displayName": "kzdenki",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13782,
                            "createdAt": "2026-01-14T16:51:14.298Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "アレの方行ってくるでも",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [],
                                "userColor": null,
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "27ca6c55-39bd-4c0f-82c3-812adcfdcc32",
                            "commenter": {
                                "id": "462358546",
                                "login": "pon2149",
                                "displayName": "ぽんぽんぺ",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13786,
                            "createdAt": "2026-01-14T16:51:18.165Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "うんこで恥ずかしがるのは中学までや",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "c3Vic2NyaWJlcjs2Ow==",
                                        "setID": "subscriber",
                                        "version": "6",
                                        "__typename": "Badge"
                                    },
                                    {
                                        "id": "bG93OzE7",
                                        "setID": "low",
                                        "version": "1",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": "#FF69B4",
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "75f55544-0fb4-4fa2-bc60-a463fcbecefe",
                            "commenter": {
                                "id": "961256224",
                                "login": "yoshimaru440794",
                                "displayName": "よしまる794",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13836,
                            "createdAt": "2026-01-14T16:52:07.727Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "もうアウトだよw",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "c3Vic2NyaWJlcjs2Ow==",
                                        "setID": "subscriber",
                                        "version": "6",
                                        "__typename": "Badge"
                                    },
                                    {
                                        "id": "Yml0czsxMDA7",
                                        "setID": "bits",
                                        "version": "100",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": "#DAA520",
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "e69fab53-fb6d-4291-8342-e49f6da2828f",
                            "commenter": {
                                "id": "1019157982",
                                "login": "doragon_fry",
                                "displayName": "ずぶずぶタートル",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13843,
                            "createdAt": "2026-01-14T16:52:15.047Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "これは単純に位置が悪かったね",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "Ozs=",
                                        "setID": "",
                                        "version": "",
                                        "__typename": "Badge"
                                    },
                                    {
                                        "id": "cHJlbWl1bTsxOw==",
                                        "setID": "premium",
                                        "version": "1",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": null,
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "5e32492c-678a-4fbd-a67b-b0ae9728a0e9",
                            "commenter": {
                                "id": "984850013",
                                "login": "cheeeeetra319",
                                "displayName": "チートラ",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13855,
                            "createdAt": "2026-01-14T16:52:27.609Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "家族で言うてたけどな普通に、かな",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "c3Vic2NyaWJlcjswOw==",
                                        "setID": "subscriber",
                                        "version": "0",
                                        "__typename": "Badge"
                                    },
                                    {
                                        "id": "Y2xpcHMtbGVhZGVyOzE7",
                                        "setID": "clips-leader",
                                        "version": "1",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": "#FF4500",
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "9f6fbccc-c1e9-48c4-b556-17394436bce0",
                            "commenter": {
                                "id": "794838438",
                                "login": "salmom_syake",
                                "displayName": "salmom_syake",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13857,
                            "createdAt": "2026-01-14T16:52:29.146Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "あ",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [],
                                "userColor": null,
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "e003af06-b1c5-48c7-a307-ee7f8801ac30",
                            "commenter": {
                                "id": "1025124902",
                                "login": "gondoana",
                                "displayName": "gondoana",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13861,
                            "createdAt": "2026-01-14T16:52:32.897Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "普通",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "c3Vic2NyaWJlcjs2Ow==",
                                        "setID": "subscriber",
                                        "version": "6",
                                        "__typename": "Badge"
                                    },
                                    {
                                        "id": "cHJlbWl1bTsxOw==",
                                        "setID": "premium",
                                        "version": "1",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": null,
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "a3e22168-e5fb-4f4d-8324-a28734b0e558",
                            "commenter": {
                                "id": "1227822175",
                                "login": "hajime2728",
                                "displayName": "Hajime2728",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13862,
                            "createdAt": "2026-01-14T16:52:34.291Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "普通のはOKなのかw",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "c3Vic2NyaWJlcjs2Ow==",
                                        "setID": "subscriber",
                                        "version": "6",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": null,
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "ca6cbdd7-2cc9-480b-8c8b-ae93d9d5869e",
                            "commenter": {
                                "id": "961756174",
                                "login": "dendendeen",
                                "displayName": "dendendeen",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13880,
                            "createdAt": "2026-01-14T16:52:52.135Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "それはないw",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [],
                                "userColor": null,
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "6c29b8a2-cbd6-4671-90b7-ff3b826985ca",
                            "commenter": {
                                "id": "768326248",
                                "login": "rerto1114",
                                "displayName": "微助っ人",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13915,
                            "createdAt": "2026-01-14T16:53:27.16Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "アレは普通に全然使わないよ",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "dHVyYm87MTs=",
                                        "setID": "turbo",
                                        "version": "1",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": null,
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "5f15f63a-509a-487f-b1a9-f918850a8991",
                            "commenter": {
                                "id": "716440414",
                                "login": "mrboy9029",
                                "displayName": "mrboy9029",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13922,
                            "createdAt": "2026-01-14T16:53:34.49Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "ほほう",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "Ozs=",
                                        "setID": "",
                                        "version": "",
                                        "__typename": "Badge"
                                    },
                                    {
                                        "id": "dHVyYm87MTs=",
                                        "setID": "turbo",
                                        "version": "1",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": "#00FF7F",
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "ee45eb6e-66aa-49ca-9463-60328c886fed",
                            "commenter": {
                                "id": "1002244357",
                                "login": "marytop777",
                                "displayName": "MaryTop777",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13984,
                            "createdAt": "2026-01-14T16:54:36.549Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "ｗｗｗ",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "azRzZW4tY29uLTIwMjU7MTs=",
                                        "setID": "k4sen-con-2025",
                                        "version": "1",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": "#00445D",
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "c03d1510-95b4-42a2-bbc9-c638b9c7d96e",
                            "commenter": {
                                "id": "709903122",
                                "login": "ichi84_",
                                "displayName": "一条の端くれ",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13987,
                            "createdAt": "2026-01-14T16:54:38.918Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "wwww",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "Ozs=",
                                        "setID": "",
                                        "version": "",
                                        "__typename": "Badge"
                                    },
                                    {
                                        "id": "Yml0czsyNTAwMDs=",
                                        "setID": "bits",
                                        "version": "25000",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": "#FF6600",
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "d1e6a1d1-128d-46b8-847a-e5408629e720",
                            "commenter": {
                                "id": "578645998",
                                "login": "k4m1kami",
                                "displayName": "かみ_",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13988,
                            "createdAt": "2026-01-14T16:54:40.138Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "Gを感じるｗ",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "c3Vic2NyaWJlcjswOw==",
                                        "setID": "subscriber",
                                        "version": "0",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": "#1E90FF",
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "d318286a-6716-4fbe-a258-f87128698e9f",
                            "commenter": {
                                "id": "795098931",
                                "login": "chemical_73",
                                "displayName": "chemical_73",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13988,
                            "createdAt": "2026-01-14T16:54:40.385Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "www",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "Ozs=",
                                        "setID": "",
                                        "version": "",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": "#8A2BE2",
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "3a714e53-1ee4-4bc5-9363-4e97d2799fb3",
                            "commenter": {
                                "id": "1063412818",
                                "login": "m1009y",
                                "displayName": "m1009y",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13991,
                            "createdAt": "2026-01-14T16:54:43.01Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "www",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "Ozs=",
                                        "setID": "",
                                        "version": "",
                                        "__typename": "Badge"
                                    },
                                    {
                                        "id": "dHdpdGNoLXJlY2FwLTIwMjU7MTs=",
                                        "setID": "twitch-recap-2025",
                                        "version": "1",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": "#DAA520",
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "42496f3f-70fc-4e57-9a72-700a0894678c",
                            "commenter": {
                                "id": "522949032",
                                "login": "sironowani",
                                "displayName": "白のワニ",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13991,
                            "createdAt": "2026-01-14T16:54:43.209Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "アシモ",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "c3Vic2NyaWJlcjs2Ow==",
                                        "setID": "subscriber",
                                        "version": "6",
                                        "__typename": "Badge"
                                    },
                                    {
                                        "id": "bGVnZW5kdXM7MTs=",
                                        "setID": "legendus",
                                        "version": "1",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": "#DAA520",
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "810b601a-ae81-4ad3-bd75-5c5fdad2c3aa",
                            "commenter": {
                                "id": "1320859354",
                                "login": "tokumu5choh",
                                "displayName": "特務伍長",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13991,
                            "createdAt": "2026-01-14T16:54:43.477Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "wwww",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "c3Vic2NyaWJlcjs2Ow==",
                                        "setID": "subscriber",
                                        "version": "6",
                                        "__typename": "Badge"
                                    },
                                    {
                                        "id": "dHdpdGNoLXJlY2FwLTIwMjU7MTs=",
                                        "setID": "twitch-recap-2025",
                                        "version": "1",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": null,
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "d0fbda13-7bdd-498d-a9bb-9bde75100a2e",
                            "commenter": {
                                "id": "782284678",
                                "login": "sourapple716",
                                "displayName": "塩林檎だよ",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13992,
                            "createdAt": "2026-01-14T16:54:44.155Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "そろりそろりwww",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "c3Vic2NyaWJlcjs2Ow==",
                                        "setID": "subscriber",
                                        "version": "6",
                                        "__typename": "Badge"
                                    },
                                    {
                                        "id": "dHVyYm87MTs=",
                                        "setID": "turbo",
                                        "version": "1",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": "#B22222",
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "bb3ef7be-8304-4bc4-9524-1f8d08126a73",
                            "commenter": {
                                "id": "671229309",
                                "login": "kzdenki",
                                "displayName": "kzdenki",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 13994,
                            "createdAt": "2026-01-14T16:54:45.874Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "車消えるとアレだしね",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [],
                                "userColor": null,
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "9ccbefd3-e6fe-403c-bed5-c9d097151d81",
                            "commenter": {
                                "id": "801891916",
                                "login": "akitake4006",
                                "displayName": "akitake4006",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 14151,
                            "createdAt": "2026-01-14T16:57:22.706Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "車くん頑張ってくれてるね",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "c3Vic2NyaWJlcjs2Ow==",
                                        "setID": "subscriber",
                                        "version": "6",
                                        "__typename": "Badge"
                                    },
                                    {
                                        "id": "Yml0czsxMDAwOw==",
                                        "setID": "bits",
                                        "version": "1000",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": null,
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "44323dab-f5c5-496e-8414-719774421d4a",
                            "commenter": {
                                "id": "1019525174",
                                "login": "ayanagi_",
                                "displayName": "彩凪_",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 14192,
                            "createdAt": "2026-01-14T16:58:04.031Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "こんな修理しなくて動くもんなんだね",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "c3Vic2NyaWJlcjs2Ow==",
                                        "setID": "subscriber",
                                        "version": "6",
                                        "__typename": "Badge"
                                    },
                                    {
                                        "id": "dHdpdGNoLXJlY2FwLTIwMjU7MTs=",
                                        "setID": "twitch-recap-2025",
                                        "version": "1",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": "#1E90FF",
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "54313f44-b4b3-426a-9b46-a60e7b08eb3a",
                            "commenter": {
                                "id": "1330810569",
                                "login": "yuuuchan0125",
                                "displayName": "yuuuchan0125",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 14236,
                            "createdAt": "2026-01-14T16:58:47.716Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "ワクワク",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "c3Vic2NyaWJlcjs2Ow==",
                                        "setID": "subscriber",
                                        "version": "6",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": "#9ACD32",
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "8c1f317b-51ad-4a99-992e-570c7129234f",
                            "commenter": {
                                "id": "1019525174",
                                "login": "ayanagi_",
                                "displayName": "彩凪_",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 14245,
                            "createdAt": "2026-01-14T16:58:57.395Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "おおー！",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "c3Vic2NyaWJlcjs2Ow==",
                                        "setID": "subscriber",
                                        "version": "6",
                                        "__typename": "Badge"
                                    },
                                    {
                                        "id": "dHdpdGNoLXJlY2FwLTIwMjU7MTs=",
                                        "setID": "twitch-recap-2025",
                                        "version": "1",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": "#1E90FF",
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "e43111e7-e055-4f9b-b882-d184bd3d0a90",
                            "commenter": {
                                "id": "522949032",
                                "login": "sironowani",
                                "displayName": "白のワニ",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 14246,
                            "createdAt": "2026-01-14T16:58:57.709Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "おしゃれ",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "c3Vic2NyaWJlcjs2Ow==",
                                        "setID": "subscriber",
                                        "version": "6",
                                        "__typename": "Badge"
                                    },
                                    {
                                        "id": "bGVnZW5kdXM7MTs=",
                                        "setID": "legendus",
                                        "version": "1",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": "#DAA520",
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    },
                    {
                        "cursor": "eyJpZCI6IjY4OGRiMmUxLTM4MWUtNDMwZi1iMDRkLTllYWZkMGFjMjQ4ZCIsImhrIjoiYnJvYWRjYXN0OjMxNTM0OTU3ODczOCIsInNrIjoiQUFBTTlvaFlxVUFZaXFkNXFuUy1RQSJ9",
                        "node": {
                            "id": "688db2e1-381e-430f-b04d-9eafd0ac248d",
                            "commenter": {
                                "id": "1002244357",
                                "login": "marytop777",
                                "displayName": "MaryTop777",
                                "__typename": "User"
                            },
                            "contentOffsetSeconds": 14252,
                            "createdAt": "2026-01-14T16:59:04.689Z",
                            "message": {
                                "fragments": [
                                    {
                                        "emote": null,
                                        "text": "うええええええ",
                                        "__typename": "VideoCommentMessageFragment"
                                    }
                                ],
                                "userBadges": [
                                    {
                                        "id": "azRzZW4tY29uLTIwMjU7MTs=",
                                        "setID": "k4sen-con-2025",
                                        "version": "1",
                                        "__typename": "Badge"
                                    }
                                ],
                                "userColor": "#00445D",
                                "__typename": "VideoCommentMessage"
                            },
                            "__typename": "VideoComment"
                        },
                        "__typename": "VideoCommentEdge"
                    }
                ],
                "pageInfo": {
                    "hasNextPage": true,
                    "hasPreviousPage": true,
                    "__typename": "PageInfo"
                },
                "__typename": "VideoCommentConnection"
            },
            "__typename": "Video"
        }
    },
    "extensions": {
        "durationMilliseconds": 86,
        "operationName": "VideoCommentsByOffsetOrCursor",
        "requestID": "01KF7SRDZ769E4VE6WC1KX839J"
    }
}
```
