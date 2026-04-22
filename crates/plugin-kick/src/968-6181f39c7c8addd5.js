"use strict";
(self.webpackChunk_N_E = self.webpackChunk_N_E || []).push([[968], {
    794: (e, s, l) => {
        l.d(s, {
            n: () => o
        });
        var t = l(61873)
            , i = l(15833)
            , n = l(90901)
            , r = l(66321)
            , a = l(92441)
            , h = l(64815);
        let d = e => {
            let { badges: s, emotes: l, status: n } = e
                , d = (0,
                    i.c3)("SubscribePopover");
            return "error" !== n && ("success" !== n || s.length || l.length) ? (0,
                t.jsxs)("div", {
                    className: "bg-surface-base flex flex-col gap-2.5",
                    children: [("pending" === n || l.length > 0) && (0,
                        t.jsxs)("div", {
                            className: "grid grid-cols-8 gap-y-2",
                            children: ["pending" === n ? (0,
                                t.jsx)(a.r5, {
                                    className: "col-span-8 h-5 w-[150px] text-sm"
                                }) : (0,
                                    t.jsx)("span", {
                                        className: "col-span-8 w-fit text-sm font-semibold",
                                        children: d("flex_sub_emotes", {
                                            emoteCount: l.length
                                        })
                                    }), "pending" === n ? Array.from({
                                        length: 10
                                    }).map((e, s) => (0,
                                        t.jsx)(a.n, {
                                            className: "size-8"
                                        }, s)) : l.map(e => (0,
                                            t.jsx)(h.m, {
                                                text: e.name,
                                                children: (0,
                                                    t.jsx)("img", {
                                                        src: (0,
                                                            r.i3)(e.id),
                                                        alt: e.name,
                                                        className: "size-8"
                                                    })
                                            }, e.name))]
                        }), ("pending" === n || s.length > 0) && (0,
                            t.jsxs)("div", {
                                className: "grid grid-cols-6 gap-y-2",
                                children: [(0,
                                    t.jsx)("span", {
                                        className: "col-span-6 w-fit text-sm font-semibold",
                                        children: d("flex_sub_badges")
                                    }), "pending" === n ? Array.from({
                                        length: 10
                                    }).map((e, s) => (0,
                                        t.jsx)(a.n, {
                                            className: "size-8"
                                        }, s)) : s.map(e => {
                                            var s, l;
                                            let i = e.months > 11 ? d("subs_badge_n_year", {
                                                count: e.months / 12
                                            }) : d("subs_badge_n_months", {
                                                count: e.months
                                            });
                                            return (0,
                                                t.jsxs)("div", {
                                                    className: "flex flex-col items-center justify-center gap-1",
                                                    children: [(0,
                                                        t.jsx)("img", {
                                                            src: null != (l = null == (s = e.badge_image) ? void 0 : s.src) ? l : void 0,
                                                            alt: i,
                                                            className: "size-8"
                                                        }), (0,
                                                            t.jsx)("span", {
                                                                className: "text-surface-onSurfaceSecondary text-xs",
                                                                children: i
                                                            })]
                                                }, e.id)
                                        }
                                        )]
                            })]
                }) : null
        }
            ;
        d.displayName = "SubscribeButton.EmotesAndBadges";
        let x = e => {
            let { status: s, badges: l, emotes: i } = e
                , r = (0,
                    n.useMemo)(() => i.filter(e => e.subscribers_only) || [], [i])
                , a = (0,
                    n.useMemo)(() => l || [], [l]);
            return (0,
                t.jsx)(d, {
                    emotes: r,
                    badges: a,
                    status: s
                })
        }
            ;
        x.displayName = "SubscribeButton.Content";
        let c = () => (0,
            t.jsx)(a.n, {
                className: "h-8 w-20 grow rounded lg:h-10 lg:w-[124px] lg:grow-0"
            });
        c.displayName = "SubscribeButton.Skeleton";
        let o = {
            Content: x,
            Skeleton: c,
            EmotesAndBadges: d
        }
    }
    ,
    4472: (e, s, l) => {
        l.d(s, {
            _: () => B
        });
        var t = l(61873)
            , i = l(15833)
            , n = l(90901)
            , r = l(29274)
            , a = l(39689)
            , h = l(14662)
            , d = l(29436)
            , x = l(81951)
            , c = l(94493)
            , o = l(1002)
            , j = l(80628)
            , p = l(50036)
            , f = l(56711)
            , u = l(70248)
            , g = l(72367)
            , H = l(12953)
            , w = l(73284)
            , m = l(31581)
            , L = l(57003)
            , V = l(53871)
            , v = l(48799)
            , C = l(77885)
            , b = l(30342)
            , F = l(57935);
        let _ = e => {
            let { children: s } = e;
            return (0,
                t.jsx)("div", {
                    className: "flex flex-col gap-2",
                    children: s
                })
        }
            ;
        _.displayName = "GiftSubButton.Root";
        let M = e => {
            let { children: s } = e;
            return (0,
                t.jsx)("div", {
                    className: "bg-surface-highest flex items-center gap-2 rounded p-3",
                    children: s
                })
        }
            ;
        M.displayName = "GiftSubButton.OptionContainer";
        let Z = e => {
            let { children: s } = e;
            return (0,
                t.jsx)("span", {
                    className: "grow text-sm font-semibold",
                    children: s
                })
        }
            ;
        Z.displayName = "GiftSubButton.OptionName";
        let y = e => {
            let { onSelection: s } = e
                , l = (0,
                    i.c3)("GiftSubPopover")
                , [r, a] = (0,
                    n.useState)(p.Xo)
                , [h, d] = (0,
                    n.useState)(String(p.Xo));
            return (0,
                t.jsxs)(M, {
                    children: [(0,
                        t.jsx)(F.bL, {
                            value: h,
                            onInput: e => {
                                let s = e.currentTarget.value;
                                if ("" === (s = s.replace(/[^0-9]/g, ""))) {
                                    d(""),
                                        a(p.Xo);
                                    return
                                }
                                let l = parseInt(s, 10);
                                l < p.Xo ? s = String(p.Xo) : l > p.hs && (s = String(p.hs)),
                                    e.currentTarget.value = s,
                                    d(s),
                                    a(parseInt(s, 10))
                            }
                            ,
                            onBlur: () => {
                                "" === h && (d(String(p.Xo)),
                                    a(p.Xo))
                            }
                            ,
                            type: "text",
                            cType: "number",
                            inputMode: "numeric",
                            cSize: "sm",
                            min: p.Xo,
                            max: p.hs,
                            className: "w-[48px]"
                        }), (0,
                            t.jsxs)("div", {
                                className: "flex grow flex-col",
                                children: [(0,
                                    t.jsx)(Z, {
                                        children: l("custom_quantity")
                                    }), (0,
                                        t.jsx)("span", {
                                            className: "text-subtle text-xs leading-none",
                                            children: l("maximum_quantity", {
                                                quantity: p.hs
                                            })
                                        })]
                            }), (0,
                                t.jsxs)(g.$n, {
                                    size: "sm",
                                    onClick: () => s(r),
                                    children: ["$", (r * p.Gq).toFixed(2)]
                                })]
                })
        }
            ;
        y.displayName = "GiftSubButton.CustomOption";
        let N = {
            Root: _,
            Item: M,
            CustomItem: y,
            Name: Z
        }
            , B = e => {
                let { buttonClassName: s, channelSlug: l, channelId: F, chatroomId: _, isHighlighted: M, withText: Z = !0, size: y, onClick: B } = e
                    , E = (0,
                        i.c3)("GiftSubPopover")
                    , D = (0,
                        i.c3)("GiftSubButton")
                    , { openAuthenticationLogIn: P } = (0,
                        o.l)()
                    , z = "authenticated" === (0,
                        a.w)().status
                    , S = (0,
                        j.al)()
                    , [A, k] = (0,
                        n.useState)(!1)
                    , { status: U } = (0,
                        c.y)({
                            channelId: F,
                            channelSlug: l
                        })
                    , { data: $, status: q } = (0,
                        C.useQuery)(r.w.info(l))
                    , G = (0,
                        n.useMemo)(() => "error" === q || z && "error" === U ? "error" : "pending" === q || z && "pending" === U ? "pending" : "success", [q, U, z])
                    , R = e => {
                        k(e)
                    }
                    , I = (e, s) => {
                        if (!z) {
                            (0,
                                x.J)({
                                    gaEvent: f.fq6,
                                    gaEventParams: {
                                        button: f.dCL,
                                        channel_slug: l
                                    }
                                }),
                                P();
                            return
                        }
                        (0,
                            x.J)({
                                gaEvent: f.dCL,
                                gaEventParams: {
                                    channel_slug: l,
                                    quantity: e,
                                    custom: s
                                }
                            }),
                            (0,
                                u.X)({
                                    type: "gift",
                                    channelSlug: l,
                                    channelId: F,
                                    chatroomId: _,
                                    gift: {
                                        quantity: e,
                                        tier: 1
                                    }
                                })
                    }
                    , O = y || (S ? "sm" : "md")
                    , X = A ? "secondary" : M ? "primary" : "secondary";
                return "pending" === G ? (0,
                    t.jsx)(H.k_, {
                        size: O,
                        className: (0,
                            v.cn)("w-[120px] grow", s)
                    }) : "error" !== G && $ ? S ? (0,
                        t.jsxs)(w.bL, {
                            open: A,
                            onOpenChange: R,
                            children: [(0,
                                t.jsx)(w.l9, {
                                    asChild: !0,
                                    children: Z ? (0,
                                        t.jsxs)(g.$n, {
                                            variant: X,
                                            size: O,
                                            className: s,
                                            onClick: B,
                                            children: [(0,
                                                t.jsx)(d.o, {}), (0,
                                                    t.jsx)("span", {
                                                        children: D("gift_sub")
                                                    })]
                                        }) : (0,
                                            t.jsx)(g.a2, {
                                                variant: X,
                                                size: O,
                                                className: s,
                                                "aria-label": D("gift_sub"),
                                                onClick: B,
                                                children: (0,
                                                    t.jsx)(d.o, {})
                                            })
                                }), (0,
                                    t.jsxs)(w.UC, {
                                        children: [(0,
                                            t.jsxs)(w.Y9, {
                                                className: "flex flex-row items-center justify-between",
                                                children: [(0,
                                                    t.jsx)(w.hE, {
                                                        children: E("gift_to_community")
                                                    }), (0,
                                                        t.jsx)(w.bm, {})]
                                            }), (0,
                                                t.jsx)("p", {
                                                    className: "text-surface-onSurfaceSecondary text-xs",
                                                    children: E("gift_sub_description", {
                                                        username: $.user.username
                                                    })
                                                }), (0,
                                                    t.jsxs)(N.Root, {
                                                        children: [p.JW.map(e => (0,
                                                            t.jsxs)(N.Item, {
                                                                children: [(0,
                                                                    t.jsx)(b.w, {
                                                                        quantity: e,
                                                                        className: "size-5 shrink-0"
                                                                    }), (0,
                                                                        t.jsx)(N.Name, {
                                                                            children: E("gift_subs", {
                                                                                count: e
                                                                            })
                                                                        }), (0,
                                                                            t.jsxs)(g.$n, {
                                                                                size: "sm",
                                                                                onClick: () => {
                                                                                    k(!1),
                                                                                        I(e, !1)
                                                                                }
                                                                                ,
                                                                                children: ["$", (e * p.Gq).toFixed(2)]
                                                                            })]
                                                            }, e)), (0,
                                                                t.jsx)(N.CustomItem, {
                                                                    onSelection: e => I(e, !0)
                                                                })]
                                                    }), (0,
                                                        t.jsx)("p", {
                                                            className: "text-surface-onSurfaceSecondary text-xs",
                                                            children: E("unlock_gifter_badge")
                                                        })]
                                    })]
                        }) : (0,
                            t.jsxs)(m.bL, {
                                open: A,
                                onOpenChange: R,
                                children: [(0,
                                    t.jsx)(m.l9, {
                                        asChild: !0,
                                        children: Z ? (0,
                                            t.jsxs)(g.$n, {
                                                variant: X,
                                                size: O,
                                                className: s,
                                                onClick: B,
                                                "data-testid": "gift-sub-button",
                                                children: [A ? (0,
                                                    t.jsx)(h.U, {}) : (0,
                                                        t.jsx)(d.o, {}), (0,
                                                            t.jsx)("span", {
                                                                children: D(A ? "close" : "gift_sub")
                                                            })]
                                            }) : (0,
                                                t.jsx)(g.a2, {
                                                    variant: X,
                                                    size: O,
                                                    className: s,
                                                    "aria-label": D(A ? "close" : "gift_sub"),
                                                    onClick: B,
                                                    children: A ? (0,
                                                        t.jsx)(h.U, {}) : (0,
                                                            t.jsx)(d.o, {})
                                                })
                                    }), (0,
                                        t.jsxs)(m.UC, {
                                            className: "gap-2 p-4 lg:w-[408px]",
                                            align: "end",
                                            children: [(0,
                                                t.jsxs)("div", {
                                                    className: "flex items-center justify-between",
                                                    children: [(0,
                                                        t.jsx)(L.H2, {
                                                            size: "md",
                                                            className: "font-bold",
                                                            children: E("gift_to_community")
                                                        }), (0,
                                                            t.jsx)(m.bm, {
                                                                asChild: !0,
                                                                children: (0,
                                                                    t.jsx)(g.a2, {
                                                                        variant: "secondary",
                                                                        size: "sm",
                                                                        "aria-label": "Close",
                                                                        children: (0,
                                                                            t.jsx)(h.U, {})
                                                                    })
                                                            })]
                                                }), (0,
                                                    t.jsx)("p", {
                                                        className: "text-surface-onSurfaceSecondary text-xs",
                                                        children: E("gift_sub_description", {
                                                            username: $.user.username
                                                        })
                                                    }), (0,
                                                        t.jsx)(V.F, {
                                                            className: "h-32 lg:h-[302px]",
                                                            type: "always",
                                                            children: (0,
                                                                t.jsxs)(N.Root, {
                                                                    children: [p.JW.map(e => (0,
                                                                        t.jsxs)(N.Item, {
                                                                            children: [(0,
                                                                                t.jsx)(b.w, {
                                                                                    quantity: e,
                                                                                    className: "size-5 shrink-0"
                                                                                }), (0,
                                                                                    t.jsx)(N.Name, {
                                                                                        children: E("gift_subs", {
                                                                                            count: e
                                                                                        })
                                                                                    }), (0,
                                                                                        t.jsxs)(g.$n, {
                                                                                            size: "sm",
                                                                                            onClick: () => I(e, !1),
                                                                                            children: ["$", (e * p.Gq).toFixed(2)]
                                                                                        })]
                                                                        }, e)), (0,
                                                                            t.jsx)(N.CustomItem, {
                                                                                onSelection: e => I(e, !0)
                                                                            })]
                                                                })
                                                        }), (0,
                                                            t.jsx)("p", {
                                                                className: "text-surface-onSurfaceSecondary text-xs",
                                                                children: E("unlock_gifter_badge")
                                                            })]
                                        })]
                            }) : null
            }
    }
    ,
    5400: (e, s, l) => {
        l.d(s, {
            h: () => r
        });
        var t = l(9682)
            , i = (l(59281),
                l(56519));
        let n = {
            index: e => (0,
                i.A6)("/emotes", null, {
                    method: "GET",
                    signal: e
                }),
            channelEmotes: (e, s) => (0,
                i.A6)("/emotes/".concat(s), null, {
                    method: "GET",
                    signal: e
                })
        }
            , r = {
                index: e => (0,
                    t.j)({
                        queryKey: ["Emote", "index"],
                        queryFn: e => {
                            let { signal: s } = e;
                            return n.index(s)
                        }
                        ,
                        enabled: e
                    }),
                channelEmotes: e => (0,
                    t.j)({
                        queryKey: ["Emote", "channelEmotes", e],
                        queryFn: s => {
                            let { signal: l } = s;
                            return n.channelEmotes(l, e)
                        }
                    })
            }
    }
    ,
    8649: (e, s, l) => {
        l.d(s, {
            f: () => t
        });
        let t = (e, s) => {
            let l = new Set;
            return e.filter(e => {
                let t = s(e);
                return !l.has(t) && (l.add(t),
                    !0)
            }
            )
        }
    }
    ,
    10765: (e, s, l) => {
        l.d(s, {
            C: () => i
        });
        var t = l(61873);
        let i = e => {
            let { ...s } = e;
            return (0,
                t.jsxs)("svg", {
                    width: "32",
                    height: "32",
                    viewBox: "0 0 32 32",
                    fill: "none",
                    xmlns: "http://www.w3.org/2000/svg",
                    ...s,
                    children: [(0,
                        t.jsx)("g", {
                            clipPath: "url(#clip0_614_6275)",
                            children: (0,
                                t.jsx)("path", {
                                    d: "M30.8598 19.2368C30.1977 18.2069 29.5356 17.2138 28.8736 16.1839C28.7264 15.9632 28.7264 15.8161 28.8736 15.5954C29.5356 14.6023 30.1609 13.6092 30.823 12.6161C31.5954 11.4391 31.1908 10.2989 29.8667 9.82069C28.7632 9.41609 27.6598 8.97471 26.5563 8.57012C26.3356 8.49656 26.2253 8.34943 26.2253 8.09196C26.1885 6.87816 26.1149 5.66437 26.0414 4.48736C25.9678 3.2 24.9747 2.46437 23.7241 2.7954C22.5471 3.08966 21.3701 3.42069 20.2299 3.75173C19.9724 3.82529 19.8253 3.75173 19.6414 3.56782C18.9057 2.61149 18.1333 1.69195 17.3977 0.772414C16.5885 -0.257472 15.3379 -0.257472 14.492 0.772414C13.7563 1.69195 12.9839 2.61149 12.2851 3.53103C12.1012 3.7885 11.9172 3.82529 11.623 3.75173C10.4828 3.42069 9.34253 3.12644 8.53334 2.90575C6.95173 2.53793 5.99541 3.16322 5.92184 4.48736C5.84828 5.70115 5.77472 6.91495 5.73794 8.16552C5.73794 8.42299 5.62759 8.53333 5.4069 8.64368C4.26667 9.08506 3.12644 9.52644 1.98621 9.96782C0.809203 10.446 0.441387 11.5862 1.14023 12.6529C1.8023 13.6828 2.46437 14.6759 3.12644 15.7057C3.27356 15.9264 3.27356 16.0736 3.12644 16.331C2.42759 17.3609 1.76552 18.3908 1.10345 19.4575C0.478165 20.4506 0.882759 21.6276 1.98621 22.069C3.12644 22.5104 4.30345 22.9517 5.44368 23.3931C5.70115 23.4667 5.77471 23.6138 5.77471 23.8713C5.81149 25.0483 5.95862 26.1885 5.95862 27.3655C5.95862 28.5425 6.9885 29.6092 8.42298 29.1678C9.56321 28.8 10.7034 28.5425 11.8437 28.2115C12.0644 28.1379 12.2115 28.1747 12.3586 28.3954C13.131 29.3517 13.8667 30.2713 14.6391 31.2276C15.485 32.2575 16.6988 32.2575 17.508 31.2276C18.2805 30.2713 19.0161 29.3517 19.7885 28.3954C19.9356 28.2115 20.046 28.1379 20.3034 28.2115C21.4804 28.5425 22.6575 28.8368 23.8345 29.1678C25.0483 29.4988 26.0781 28.7632 26.1149 27.5126C26.1885 26.2989 26.2621 25.0851 26.2988 23.8345C26.2988 23.5402 26.446 23.4299 26.6667 23.3563C27.7701 22.9517 28.9103 22.5104 30.0138 22.069C31.1908 21.4805 31.5586 20.3034 30.8598 19.2368ZM22.069 13.2046L14.7127 20.5609C14.5287 20.7448 14.2713 20.892 14.0138 20.9287C13.9402 20.9287 13.8299 20.9655 13.7563 20.9655C13.4253 20.9655 13.0575 20.8184 12.8 20.5609L9.78392 17.5448C9.26898 17.0299 9.26898 16.1839 9.78392 15.669C10.2989 15.154 11.1448 15.154 11.6598 15.669L13.7196 17.7287L20.1196 11.3287C20.6345 10.8138 21.4805 10.8138 21.9954 11.3287C22.5839 11.8437 22.5839 12.6897 22.069 13.2046Z",
                                    fill: "url(#paint0_linear_614_6275)"
                                })
                        }), (0,
                            t.jsxs)("defs", {
                                children: [(0,
                                    t.jsxs)("linearGradient", {
                                        id: "paint0_linear_614_6275",
                                        x1: "8.14138",
                                        y1: "32.3591",
                                        x2: "24.4968",
                                        y2: "0.904884",
                                        gradientUnits: "userSpaceOnUse",
                                        children: [(0,
                                            t.jsx)("stop", {
                                                stopColor: "#1EFF00"
                                            }), (0,
                                                t.jsx)("stop", {
                                                    offset: "0.99",
                                                    stopColor: "#00FF8C"
                                                })]
                                    }), (0,
                                        t.jsx)("clipPath", {
                                            id: "clip0_614_6275",
                                            children: (0,
                                                t.jsx)("rect", {
                                                    width: "32",
                                                    height: "32",
                                                    fill: "white"
                                                })
                                        })]
                            })]
                })
        }
    }
    ,
    17379: (e, s, l) => {
        l.d(s, {
            ge: () => E,
            aL: () => y,
            xQ: () => N,
            Yc: () => D,
            A$: () => B
        });
        var t = l(61873)
            , i = l(15833)
            , n = l(90901)
            , r = l(29274)
            , a = l(5400)
            , h = l(39689)
            , d = l(14662)
            , x = l(95082)
            , c = l(99742)
            , o = l(81951)
            , j = l(94493)
            , p = l(1002)
            , f = l(80628)
            , u = l(56711)
            , g = l(70248)
            , H = l(8649)
            , w = l(72367)
            , m = l(73284)
            , L = l(31581)
            , V = l(57003)
            , v = l(64815)
            , C = l(48799)
            , b = l(77885)
            , F = l(42177)
            , _ = l(12953);
        let M = {
            Part: e => {
                let { children: s, className: l } = e;
                return (0,
                    t.jsx)("div", {
                        className: (0,
                            C.cn)("betterhover:group-hover:bg-[#3AD305] flex h-full items-center gap-1 bg-green-500 px-2 py-1.5 group-focus-visible:bg-green-500 group-disabled:bg-[#21650A]", l),
                        children: s
                    })
            }
            ,
            Root: (0,
                n.forwardRef)((e, s) => {
                    let { children: l, className: i, ...n } = e;
                    return (0,
                        t.jsx)(_.$n, {
                            ref: s,
                            ...n,
                            variant: "default",
                            className: (0,
                                C.cn)("l!eading-none group gap-0.5 overflow-hidden !bg-[#21650A] !p-0 !text-sm disabled:!bg-transparent lg:!text-base", i),
                            children: l
                        })
                }
                )
        };
        var Z = l(794);
        let y = e => {
            let { buttonClassName: s, channelSlug: l, channelId: c, isHighlighted: o, withText: p, size: u, open: g, onOpenChange: H } = e
                , v = (0,
                    i.c3)("SubscribePopover")
                , C = (0,
                    i.c3)("SubscribeButton")
                , F = "authenticated" === (0,
                    h.w)().status
                , _ = (0,
                    f.al)()
                , { data: M, status: y } = (0,
                    b.useQuery)(r.w.info(l))
                , { data: P, status: z } = (0,
                    j.y)({
                        channelId: c,
                        channelSlug: l
                    })
                , { data: S, status: A } = (0,
                    b.useQuery)(a.h.channelEmotes(l))
                , k = (0,
                    n.useMemo)(() => {
                        if (S) {
                            var e;
                            return (null == (e = S.find(e => e.slug === l)) ? void 0 : e.emotes) || []
                        }
                        return []
                    }
                        , [S, l])
                , U = (0,
                    n.useMemo)(() => "error" === y || F && "error" === z || "error" === A ? "error" : "pending" === y || F && "pending" === z || "pending" === A ? "pending" : "success", [y, z, A, F])
                , $ = u || _ ? "sm" : "md"
                , q = g ? "secondary" : o ? "primary" : "secondary";
            return "success" !== U ? (0,
                t.jsx)(Z.n.Skeleton, {}) : (null == P ? void 0 : P.subscription) ? (0,
                    t.jsx)(N, {
                        size: $,
                        expiresAt: P.subscription.expires_at
                    }) : M ? _ ? (0,
                        t.jsxs)(m.bL, {
                            open: g,
                            onOpenChange: H,
                            children: [(0,
                                t.jsx)(m.l9, {
                                    asChild: !0,
                                    children: p ? (0,
                                        t.jsxs)(w.$n, {
                                            variant: q,
                                            size: $,
                                            className: s,
                                            children: [(0,
                                                t.jsx)(x.Q, {}), (0,
                                                    t.jsx)("span", {
                                                        children: C("subscribe")
                                                    })]
                                        }) : (0,
                                            t.jsx)(w.a2, {
                                                variant: q,
                                                size: $,
                                                className: s,
                                                "aria-label": C("subscribe"),
                                                children: (0,
                                                    t.jsx)(x.Q, {})
                                            })
                                }), (0,
                                    t.jsxs)(m.UC, {
                                        children: [(0,
                                            t.jsxs)(m.Y9, {
                                                className: "flex flex-row items-center justify-between",
                                                children: [(0,
                                                    t.jsx)(m.hE, {
                                                        children: v("subscribe_to", {
                                                            username: M.user.username
                                                        })
                                                    }), (0,
                                                        t.jsx)(m.bm, {})]
                                            }), (0,
                                                t.jsx)(B, {
                                                    username: M.user.username,
                                                    status: U,
                                                    badges: M.subscriber_badges,
                                                    emotes: k
                                                }), (0,
                                                    t.jsx)(D, {
                                                        children: (0,
                                                            t.jsx)(E, {
                                                                channelSlug: M.slug,
                                                                channelId: M.id
                                                            })
                                                    })]
                                    })]
                        }) : (0,
                            t.jsxs)(L.bL, {
                                open: g,
                                onOpenChange: H,
                                children: [(0,
                                    t.jsx)(L.l9, {
                                        asChild: !0,
                                        children: p ? (0,
                                            t.jsxs)(w.$n, {
                                                variant: q,
                                                size: $,
                                                className: s,
                                                "data-testid": "sub-button",
                                                children: [g ? (0,
                                                    t.jsx)(d.U, {}) : (0,
                                                        t.jsx)(x.Q, {}), (0,
                                                            t.jsx)("span", {
                                                                children: C(g ? "close" : "subscribe")
                                                            })]
                                            }) : (0,
                                                t.jsx)(w.a2, {
                                                    variant: q,
                                                    size: $,
                                                    className: s,
                                                    "aria-label": C(g ? "close" : "subscribe"),
                                                    children: g ? (0,
                                                        t.jsx)(d.U, {}) : (0,
                                                            t.jsx)(x.Q, {})
                                                })
                                    }), (0,
                                        t.jsxs)(L.UC, {
                                            className: "gap-2 p-4 lg:w-[408px]",
                                            align: "end",
                                            children: [(0,
                                                t.jsxs)("div", {
                                                    className: "flex items-center justify-between",
                                                    children: [(0,
                                                        t.jsx)(V.H2, {
                                                            size: "md",
                                                            className: "font-bold",
                                                            children: v("subscribe_to", {
                                                                username: M.user.username
                                                            })
                                                        }), (0,
                                                            t.jsx)(L.bm, {
                                                                asChild: !0,
                                                                children: (0,
                                                                    t.jsx)(w.a2, {
                                                                        variant: "text",
                                                                        size: "sm",
                                                                        "aria-label": "Close",
                                                                        children: (0,
                                                                            t.jsx)(d.U, {})
                                                                    })
                                                            })]
                                                }), (0,
                                                    t.jsx)(B, {
                                                        username: M.user.username,
                                                        status: U,
                                                        badges: M.subscriber_badges,
                                                        emotes: k
                                                    }), (0,
                                                        t.jsx)(D, {
                                                            children: (0,
                                                                t.jsx)(E, {
                                                                    channelSlug: M.slug,
                                                                    channelId: M.id
                                                                })
                                                        })]
                                        })]
                            }) : null
        }
            , N = e => {
                let { variant: s = "highlight", size: l, expiresAt: n, className: r, children: a } = e
                    , h = (0,
                        i.c3)("SubscribePopover");
                return (0,
                    t.jsx)(v.m, {
                        text: h("subscribed_until", {
                            expirationAt: new Date(n)
                        }),
                        children: a ? (0,
                            t.jsxs)(w.$n, {
                                variant: s,
                                className: (0,
                                    C.cn)("flex grow-0", r),
                                size: l,
                                "aria-label": h("subscribed_until", {
                                    expirationAt: new Date(n)
                                }),
                                children: [(0,
                                    t.jsx)(c.Y, {}), a]
                            }) : (0,
                                t.jsx)(w.a2, {
                                    variant: s,
                                    className: (0,
                                        C.cn)("flex grow-0", r),
                                    size: l,
                                    "aria-label": h("subscribed_until", {
                                        expirationAt: new Date(n)
                                    }),
                                    children: (0,
                                        t.jsx)(c.Y, {})
                                })
                    })
            }
            , B = e => {
                let { username: s, status: l, badges: r, emotes: a } = e
                    , h = (0,
                        i.c3)("SubscribePopover")
                    , d = (0,
                        n.useMemo)(() => r.length ? (0,
                            H.f)(r, e => e.months) : [], [r]);
                return (0,
                    t.jsxs)(n.Fragment, {
                        children: [(0,
                            t.jsx)("p", {
                                className: "text-surface-onSurfaceSecondary text-xs",
                                children: h("subscribe_description", {
                                    username: s
                                })
                            }), (!!a.length || !!d.length) && (0,
                                t.jsxs)(n.Fragment, {
                                    children: [(0,
                                        t.jsx)(F.c, {
                                            className: "w-full grow-0"
                                        }), (0,
                                            t.jsx)(V.H3, {
                                                size: "md",
                                                className: "font-bold text-green-500",
                                                children: h("as_a_thank_you", {
                                                    username: s
                                                })
                                            }), (0,
                                                t.jsx)(Z.n.Content, {
                                                    status: l,
                                                    badges: d,
                                                    emotes: a
                                                })]
                                })]
                    })
            }
            , E = e => {
                let { size: s = "sm", channelSlug: l, channelId: n, onClick: r, ...a } = e
                    , d = "authenticated" === (0,
                        h.w)().status
                    , { openAuthenticationLogIn: c } = (0,
                        p.l)()
                    , j = (0,
                        i.c3)("SubscribePopover");
                return (0,
                    t.jsxs)(M.Root, {
                        onClick: e => {
                            if (r && r(e),
                                !d) {
                                (0,
                                    o.J)({
                                        gaEvent: u.fq6,
                                        gaEventParams: {
                                            button: u.fLz,
                                            channel_slug: l
                                        }
                                    }),
                                    c();
                                return
                            }
                            (0,
                                o.J)({
                                    gaEvent: u.fLz,
                                    gaEventParams: {
                                        channel_slug: l
                                    }
                                }),
                                (0,
                                    g.X)({
                                        type: "subscription",
                                        channelSlug: l,
                                        channelId: n,
                                        subscription: {
                                            tier: 1,
                                            monthCount: 1
                                        }
                                    })
                        }
                        ,
                        size: s,
                        ...a,
                        children: [(0,
                            t.jsxs)(M.Part, {
                                children: [(0,
                                    t.jsx)(x.Q, {
                                        className: "fill-current"
                                    }), (0,
                                        t.jsx)("span", {
                                            children: j("subscribe")
                                        })]
                            }), (0,
                                t.jsx)(M.Part, {
                                    children: "$4.99"
                                })]
                    })
            }
            , D = e => {
                let { children: s } = e;
                return (0,
                    t.jsx)("div", {
                        className: "flex flex-row items-center justify-end gap-1.5 pt-3",
                        children: s
                    })
            }
    }
    ,
    29436: (e, s, l) => {
        l.d(s, {
            o: () => i
        });
        var t = l(61873);
        let i = e => {
            let { fill: s, ...l } = e;
            return (0,
                t.jsxs)("svg", {
                    width: "40",
                    height: "40",
                    viewBox: "0 0 40 40",
                    xmlns: "http://www.w3.org/2000/svg",
                    fill: s || "white",
                    ...l,
                    children: [(0,
                        t.jsxs)("g", {
                            clipPath: "url(#clip0_93_826)",
                            children: [(0,
                                t.jsx)("path", {
                                    d: "M28.75 7.5L33.75 0H23.75L20 5.625L16.25 0H6.25L11.25 7.5H0V15H40V7.5H28.75Z",
                                    fill: "current"
                                }), (0,
                                    t.jsx)("path", {
                                        d: "M17.5 20H2.5V40H17.5V20Z",
                                        fill: "current"
                                    }), (0,
                                        t.jsx)("path", {
                                            d: "M37.5 20H22.5V40H37.5V20Z",
                                            fill: "current"
                                        })]
                        }), (0,
                            t.jsx)("defs", {
                                children: (0,
                                    t.jsx)("clipPath", {
                                        id: "clip0_93_826",
                                        children: (0,
                                            t.jsx)("rect", {
                                                width: "40",
                                                height: "40",
                                                fill: "current"
                                            })
                                    })
                            })]
                })
        }
    }
    ,
    30342: (e, s, l) => {
        l.d(s, {
            w: () => D,
            a: () => E
        });
        var t = l(61873);
        let i = e => {
            let { ...s } = e;
            return (0,
                t.jsxs)("svg", {
                    width: "32",
                    height: "32",
                    viewBox: "0 0 32 32",
                    fill: "none",
                    xmlns: "http://www.w3.org/2000/svg",
                    ...s,
                    children: [(0,
                        t.jsx)("g", {
                            clipPath: "url(#clip0_162_470)",
                            children: (0,
                                t.jsxs)("g", {
                                    clipPath: "url(#clip1_162_470)",
                                    children: [(0,
                                        t.jsx)("path", {
                                            d: "M22.34 9.5L26 4H18L16 7L14 4H6L9.66 9.5H4V15.1H28V9.5H22.34Z",
                                            fill: "#53FC18"
                                        }), (0,
                                            t.jsx)("path", {
                                                d: "M26.0799 19.0996H5.8999V28.4996H26.0799V19.0996Z",
                                                fill: "#53FC18"
                                            }), (0,
                                                t.jsx)("path", {
                                                    d: "M26.0799 15.0996H5.8999V19.0996H26.0799V15.0996Z",
                                                    fill: "#32970E"
                                                })]
                                })
                        }), (0,
                            t.jsxs)("defs", {
                                children: [(0,
                                    t.jsx)("clipPath", {
                                        id: "clip0_162_470",
                                        children: (0,
                                            t.jsx)("rect", {
                                                width: "24",
                                                height: "24.5",
                                                fill: "white",
                                                transform: "translate(4 4)"
                                            })
                                    }), (0,
                                        t.jsx)("clipPath", {
                                            id: "clip1_162_470",
                                            children: (0,
                                                t.jsx)("rect", {
                                                    width: "24",
                                                    height: "24.5",
                                                    fill: "white",
                                                    transform: "translate(4 4)"
                                                })
                                        })]
                            })]
                })
        }
            , n = e => {
                let { ...s } = e;
                return (0,
                    t.jsxs)("svg", {
                        width: "32",
                        height: "32",
                        viewBox: "0 0 32 32",
                        fill: "none",
                        xmlns: "http://www.w3.org/2000/svg",
                        ...s,
                        children: [(0,
                            t.jsxs)("g", {
                                clipPath: "url(#clip0_31_3199)",
                                children: [(0,
                                    t.jsx)("path", {
                                        d: "M22.34 9.5L26 4H18L16 7L14 4H6L9.66 9.5H4V15.1H28V9.5H22.34Z",
                                        fill: "#DEB2FF"
                                    }), (0,
                                        t.jsx)("path", {
                                            d: "M26.08 19.1001H5.90002V28.5001H26.08V19.1001Z",
                                            fill: "#DEB2FF"
                                        }), (0,
                                            t.jsx)("path", {
                                                d: "M26.08 15.1001H5.90002V19.1001H26.08V15.1001Z",
                                                fill: "#BC66FF"
                                            })]
                            }), (0,
                                t.jsx)("defs", {
                                    children: (0,
                                        t.jsx)("clipPath", {
                                            id: "clip0_31_3199",
                                            children: (0,
                                                t.jsx)("rect", {
                                                    width: "24",
                                                    height: "24.5",
                                                    fill: "white",
                                                    transform: "translate(4 4)"
                                                })
                                        })
                                })]
                    })
            }
            , r = e => {
                let { ...s } = e;
                return (0,
                    t.jsxs)("svg", {
                        width: "32",
                        height: "32",
                        viewBox: "0 0 32 32",
                        fill: "none",
                        xmlns: "http://www.w3.org/2000/svg",
                        ...s,
                        children: [(0,
                            t.jsx)("path", {
                                d: "M30 10H2V28H30V10Z",
                                fill: "#DDFED1"
                            }), (0,
                                t.jsx)("path", {
                                    d: "M12 10H10L6 4H14L16 7L18 4H26L22 10H20V28H12V10Z",
                                    fill: "#53FC18"
                                })]
                    })
            }
            , a = e => {
                let { ...s } = e;
                return (0,
                    t.jsxs)("svg", {
                        width: "32",
                        height: "32",
                        viewBox: "0 0 32 32",
                        fill: "none",
                        xmlns: "http://www.w3.org/2000/svg",
                        ...s,
                        children: [(0,
                            t.jsxs)("g", {
                                clipPath: "url(#clip0_31_3220)",
                                children: [(0,
                                    t.jsx)("path", {
                                        d: "M0.02 6H32.02V14H0.02V6Z",
                                        fill: "#53FC18"
                                    }), (0,
                                        t.jsx)("path", {
                                            d: "M0.02 18H32.02V32H0.02V18Z",
                                            fill: "#53FC18"
                                        }), (0,
                                            t.jsx)("path", {
                                                d: "M30.02 14H2.02V18H30.02V14Z",
                                                fill: "#32970E"
                                            }), (0,
                                                t.jsx)("path", {
                                                    d: "M22.02 6H20.02V32H12.02V6H14.02L18.02 0H26.02L22.02 6Z",
                                                    fill: "#BC66FF"
                                                }), (0,
                                                    t.jsx)("path", {
                                                        d: "M14.02 6L16.02 3L14.02 0H6.02L10.02 6H12.02H14.02Z",
                                                        fill: "white"
                                                    }), (0,
                                                        t.jsx)("path", {
                                                            d: "M3.02 9L2.14 7.16C2.14 6.98 1.84 6.88 1.84 6.88L0 6L1.84 5.12L2.14 4.82L3.02 2.98L3.9 4.82C3.9 4.92 4 5.02 4.2 5.12L6.04 5.9L4.2 6.78C4 6.86 3.9 6.96 3.9 7.16L3.02 9Z",
                                                            fill: "white"
                                                        }), (0,
                                                            t.jsx)("path", {
                                                                d: "M30.02 16L29.44 14.78C29.44 14.64 29.24 14.58 29.24 14.58L28.02 14L29.24 13.42L29.44 13.22L30.02 12L30.6 13.22C30.6 13.22 30.68 13.36 30.8 13.42L32.02 13.94L30.8 14.52C30.66 14.58 30.6 14.66 30.6 14.78L30.02 16Z",
                                                                fill: "white"
                                                            })]
                            }), (0,
                                t.jsx)("defs", {
                                    children: (0,
                                        t.jsx)("clipPath", {
                                            id: "clip0_31_3220",
                                            children: (0,
                                                t.jsx)("rect", {
                                                    width: "32",
                                                    height: "32",
                                                    fill: "white"
                                                })
                                        })
                                })]
                    })
            }
            , h = e => {
                let { ...s } = e;
                return (0,
                    t.jsxs)("svg", {
                        width: "32",
                        height: "32",
                        viewBox: "0 0 32 32",
                        fill: "none",
                        xmlns: "http://www.w3.org/2000/svg",
                        ...s,
                        children: [(0,
                            t.jsx)("path", {
                                d: "M30 10H2V28H30V10Z",
                                fill: "#93EBE0"
                            }), (0,
                                t.jsx)("path", {
                                    d: "M12 10H10L6 4H14L16 7L18 4H26L22 10H20V28H12V10Z",
                                    fill: "#00CCB3"
                                })]
                    })
            }
            , d = e => {
                let { ...s } = e;
                return (0,
                    t.jsxs)("svg", {
                        width: "32",
                        height: "32",
                        viewBox: "0 0 32 32",
                        fill: "none",
                        xmlns: "http://www.w3.org/2000/svg",
                        ...s,
                        children: [(0,
                            t.jsx)("path", {
                                d: "M30 10H2V28H30V10Z",
                                fill: "#B9D6F6"
                            }), (0,
                                t.jsx)("path", {
                                    d: "M12 10H10L6 4H14L16 7L18 4H26L22 10H20V28H12V10Z",
                                    fill: "#72ACED"
                                })]
                    })
            }
            , x = e => {
                let { ...s } = e;
                return (0,
                    t.jsxs)("svg", {
                        width: "32",
                        height: "32",
                        viewBox: "0 0 32 32",
                        fill: "none",
                        xmlns: "http://www.w3.org/2000/svg",
                        ...s,
                        children: [(0,
                            t.jsxs)("g", {
                                clipPath: "url(#clip0_31_3221)",
                                children: [(0,
                                    t.jsx)("path", {
                                        d: "M0.02 6H32.02V14H0.02V6Z",
                                        fill: "#00CCB3"
                                    }), (0,
                                        t.jsx)("path", {
                                            d: "M0.02 18H32.02V32H0.02V18Z",
                                            fill: "#00CCB3"
                                        }), (0,
                                            t.jsx)("path", {
                                                d: "M30.02 14H2.02V18H30.02V14Z",
                                                fill: "#00A18D"
                                            }), (0,
                                                t.jsx)("path", {
                                                    d: "M22.02 6H20.02V32H12.02V6H14.02L18.02 0H26.02L22.02 6Z",
                                                    fill: "#F2708A"
                                                }), (0,
                                                    t.jsx)("path", {
                                                        d: "M14.02 6L16.02 3L14.02 0H6.02L10.02 6H12.02H14.02Z",
                                                        fill: "white"
                                                    }), (0,
                                                        t.jsx)("path", {
                                                            d: "M3.02 9L2.14 7.16C2.14 6.98 1.84 6.88 1.84 6.88L0 6L1.84 5.12L2.14 4.82L3.02 2.98L3.9 4.82C3.9 4.92 4 5.02 4.2 5.12L6.04 5.9L4.2 6.78C4 6.86 3.9 6.96 3.9 7.16L3.02 9Z",
                                                            fill: "white"
                                                        }), (0,
                                                            t.jsx)("path", {
                                                                d: "M30.02 16L29.44 14.78C29.44 14.64 29.24 14.58 29.24 14.58L28.02 14L29.24 13.42L29.44 13.22L30.02 12L30.6 13.22C30.6 13.22 30.68 13.36 30.8 13.42L32.02 13.94L30.8 14.52C30.66 14.58 30.6 14.66 30.6 14.78L30.02 16Z",
                                                                fill: "white"
                                                            })]
                            }), (0,
                                t.jsx)("defs", {
                                    children: (0,
                                        t.jsx)("clipPath", {
                                            id: "clip0_31_3221",
                                            children: (0,
                                                t.jsx)("rect", {
                                                    width: "32",
                                                    height: "32",
                                                    fill: "white"
                                                })
                                        })
                                })]
                    })
            }
            , c = e => {
                let { ...s } = e;
                return (0,
                    t.jsxs)("svg", {
                        width: "32",
                        height: "32",
                        viewBox: "0 0 32 32",
                        fill: "none",
                        xmlns: "http://www.w3.org/2000/svg",
                        ...s,
                        children: [(0,
                            t.jsxs)("g", {
                                clipPath: "url(#clip0_31_3201)",
                                children: [(0,
                                    t.jsx)("path", {
                                        d: "M22.34 9.5L26 4H18L16 7L14 4H6L9.66 9.5H4V15.1H28V9.5H22.34Z",
                                        fill: "#FBCFD8"
                                    }), (0,
                                        t.jsx)("path", {
                                            d: "M26.08 19.1001H5.90002V28.5001H26.08V19.1001Z",
                                            fill: "#FBCFD8"
                                        }), (0,
                                            t.jsx)("path", {
                                                d: "M26.08 15.1001H5.90002V19.1001H26.08V15.1001Z",
                                                fill: "#F2708A"
                                            })]
                            }), (0,
                                t.jsx)("defs", {
                                    children: (0,
                                        t.jsx)("clipPath", {
                                            id: "clip0_31_3201",
                                            children: (0,
                                                t.jsx)("rect", {
                                                    width: "24",
                                                    height: "24.5",
                                                    fill: "white",
                                                    transform: "translate(4 4)"
                                                })
                                        })
                                })]
                    })
            }
            , o = e => {
                let { ...s } = e;
                return (0,
                    t.jsxs)("svg", {
                        width: "32",
                        height: "32",
                        viewBox: "0 0 32 32",
                        fill: "none",
                        xmlns: "http://www.w3.org/2000/svg",
                        ...s,
                        children: [(0,
                            t.jsx)("path", {
                                d: "M30 10H2V28H30V10Z",
                                fill: "#DEB2FF"
                            }), (0,
                                t.jsx)("path", {
                                    d: "M12 10H10L6 4H14L16 7L18 4H26L22 10H20V28H12V10Z",
                                    fill: "#BC66FF"
                                })]
                    })
            }
            , j = e => {
                let { ...s } = e;
                return (0,
                    t.jsxs)("svg", {
                        width: "32",
                        height: "32",
                        viewBox: "0 0 32 32",
                        fill: "none",
                        xmlns: "http://www.w3.org/2000/svg",
                        ...s,
                        children: [(0,
                            t.jsx)("path", {
                                d: "M30 10H2V28H30V10Z",
                                fill: "#FBCFD8"
                            }), (0,
                                t.jsx)("path", {
                                    d: "M12 10H10L6 4H14L16 7L18 4H26L22 10H20V28H12V10Z",
                                    fill: "#F2708A"
                                })]
                    })
            }
            , p = e => {
                let { ...s } = e;
                return (0,
                    t.jsxs)("svg", {
                        width: "32",
                        height: "32",
                        viewBox: "0 0 32 32",
                        fill: "none",
                        xmlns: "http://www.w3.org/2000/svg",
                        ...s,
                        children: [(0,
                            t.jsxs)("g", {
                                clipPath: "url(#clip0_31_3222)",
                                children: [(0,
                                    t.jsx)("path", {
                                        d: "M0.02 6H32.02V14H0.02V6Z",
                                        fill: "#DEB2FF"
                                    }), (0,
                                        t.jsx)("path", {
                                            d: "M0.02 18H32.02V32H0.02V18Z",
                                            fill: "#DEB2FF"
                                        }), (0,
                                            t.jsx)("path", {
                                                d: "M30.02 14H2.02V18H30.02V14Z",
                                                fill: "#BC66FF"
                                            }), (0,
                                                t.jsx)("path", {
                                                    d: "M22.02 6H20.02V32H12.02V6H14.02L18.02 0H26.02L22.02 6Z",
                                                    fill: "#1475E1"
                                                }), (0,
                                                    t.jsx)("path", {
                                                        d: "M14.02 6L16.02 3L14.02 0H6.02L10.02 6H12.02H14.02Z",
                                                        fill: "white"
                                                    }), (0,
                                                        t.jsx)("path", {
                                                            d: "M3.02 9L2.14 7.16C2.14 6.98 1.84 6.88 1.84 6.88L0 6L1.84 5.12L2.14 4.82L3.02 2.98L3.9 4.82C3.9 4.92 4 5.02 4.2 5.12L6.04 5.9L4.2 6.78C4 6.86 3.9 6.96 3.9 7.16L3.02 9Z",
                                                            fill: "white"
                                                        }), (0,
                                                            t.jsx)("path", {
                                                                d: "M30.02 16L29.44 14.78C29.44 14.64 29.24 14.58 29.24 14.58L28.02 14L29.24 13.42L29.44 13.22L30.02 12L30.6 13.22C30.6 13.22 30.68 13.36 30.8 13.42L32.02 13.94L30.8 14.52C30.66 14.58 30.6 14.66 30.6 14.78L30.02 16Z",
                                                                fill: "white"
                                                            })]
                            }), (0,
                                t.jsx)("defs", {
                                    children: (0,
                                        t.jsx)("clipPath", {
                                            id: "clip0_31_3222",
                                            children: (0,
                                                t.jsx)("rect", {
                                                    width: "32",
                                                    height: "32",
                                                    fill: "white"
                                                })
                                        })
                                })]
                    })
            }
            , f = e => {
                let { ...s } = e;
                return (0,
                    t.jsxs)("svg", {
                        width: "32",
                        height: "32",
                        viewBox: "0 0 32 32",
                        fill: "none",
                        xmlns: "http://www.w3.org/2000/svg",
                        ...s,
                        children: [(0,
                            t.jsx)("path", {
                                d: "M30 10H2V28H30V10Z",
                                fill: "#FFD899"
                            }), (0,
                                t.jsx)("path", {
                                    d: "M12 10H10L6 4H14L16 7L18 4H26L22 10H20V28H12V10Z",
                                    fill: "#FF9D00"
                                })]
                    })
            }
            , u = e => {
                let { ...s } = e;
                return (0,
                    t.jsxs)("svg", {
                        width: "32",
                        height: "32",
                        viewBox: "0 0 32 32",
                        fill: "none",
                        xmlns: "http://www.w3.org/2000/svg",
                        ...s,
                        children: [(0,
                            t.jsx)("path", {
                                d: "M30 10H2V28H30V10Z",
                                fill: "#53FC18"
                            }), (0,
                                t.jsx)("path", {
                                    d: "M12 10H10L6 4H14L16 7L18 4H26L22 10H20V28H12V10Z",
                                    fill: "#BC66FF"
                                })]
                    })
            }
            , g = e => {
                let { ...s } = e;
                return (0,
                    t.jsxs)("svg", {
                        width: "32",
                        height: "32",
                        viewBox: "0 0 32 32",
                        fill: "none",
                        xmlns: "http://www.w3.org/2000/svg",
                        ...s,
                        children: [(0,
                            t.jsxs)("g", {
                                clipPath: "url(#clip0_116_1156)",
                                children: [(0,
                                    t.jsx)("path", {
                                        d: "M0.02 6H32.02V14H0.02V6Z",
                                        fill: "#F2708A"
                                    }), (0,
                                        t.jsx)("path", {
                                            d: "M0.02 18H32.02V32H0.02V18Z",
                                            fill: "#F2708A"
                                        }), (0,
                                            t.jsx)("path", {
                                                d: "M30.02 14H2.02V18H30.02V14Z",
                                                fill: "#E9113C"
                                            }), (0,
                                                t.jsx)("path", {
                                                    d: "M22.02 6H20.02V32H12.02V6H14.02L18.02 0H26.02L22.02 6Z",
                                                    fill: "#53FC18"
                                                }), (0,
                                                    t.jsx)("path", {
                                                        d: "M14.02 6L16.02 3L14.02 0H6.02L10.02 6H12.02H14.02Z",
                                                        fill: "white"
                                                    }), (0,
                                                        t.jsx)("path", {
                                                            d: "M3.02 9L2.14 7.16C2.14 6.98 1.84 6.88 1.84 6.88L0 6L1.84 5.12L2.14 4.82L3.02 2.98L3.9 4.82C3.9 4.92 4 5.02 4.2 5.12L6.04 5.9L4.2 6.78C4 6.86 3.9 6.96 3.9 7.16L3.02 9Z",
                                                            fill: "white"
                                                        }), (0,
                                                            t.jsx)("path", {
                                                                d: "M30.02 16L29.44 14.78C29.44 14.64 29.24 14.58 29.24 14.58L28.02 14L29.24 13.42L29.44 13.22L30.02 12L30.6 13.22C30.6 13.22 30.68 13.36 30.8 13.42L32.02 13.94L30.8 14.52C30.66 14.58 30.6 14.66 30.6 14.78L30.02 16Z",
                                                                fill: "white"
                                                            })]
                            }), (0,
                                t.jsx)("defs", {
                                    children: (0,
                                        t.jsx)("clipPath", {
                                            id: "clip0_116_1156",
                                            children: (0,
                                                t.jsx)("rect", {
                                                    width: "32",
                                                    height: "32",
                                                    fill: "white"
                                                })
                                        })
                                })]
                    })
            }
            , H = e => {
                let { ...s } = e;
                return (0,
                    t.jsxs)("svg", {
                        width: "32",
                        height: "32",
                        viewBox: "0 0 32 32",
                        fill: "none",
                        xmlns: "http://www.w3.org/2000/svg",
                        ...s,
                        children: [(0,
                            t.jsx)("path", {
                                d: "M30 10H2V28H30V10Z",
                                fill: "#53FC18"
                            }), (0,
                                t.jsx)("path", {
                                    d: "M12 10H10L6 4H14L16 7L18 4H26L22 10H20V28H12V10Z",
                                    fill: "#FBC900"
                                })]
                    })
            }
            , w = e => {
                let { ...s } = e;
                return (0,
                    t.jsxs)("svg", {
                        width: "32",
                        height: "32",
                        viewBox: "0 0 32 32",
                        fill: "none",
                        xmlns: "http://www.w3.org/2000/svg",
                        ...s,
                        children: [(0,
                            t.jsxs)("g", {
                                clipPath: "url(#clip0_31_3197)",
                                children: [(0,
                                    t.jsx)("path", {
                                        d: "M22.34 9.5L26 4H18L16 7L14 4H6L9.66 9.5H4V15.1H28V9.5H22.34Z",
                                        fill: "#2EFAD1"
                                    }), (0,
                                        t.jsx)("path", {
                                            d: "M26.08 19.1001H5.90002V28.5001H26.08V19.1001Z",
                                            fill: "#2EFAD1"
                                        }), (0,
                                            t.jsx)("path", {
                                                d: "M26.08 15.1001H5.90002V19.1001H26.08V15.1001Z",
                                                fill: "#00A18D"
                                            })]
                            }), (0,
                                t.jsx)("defs", {
                                    children: (0,
                                        t.jsx)("clipPath", {
                                            id: "clip0_31_3197",
                                            children: (0,
                                                t.jsx)("rect", {
                                                    width: "24",
                                                    height: "24.5",
                                                    fill: "white",
                                                    transform: "translate(4 4)"
                                                })
                                        })
                                })]
                    })
            }
            , m = e => {
                let { ...s } = e;
                return (0,
                    t.jsxs)("svg", {
                        width: "32",
                        height: "32",
                        viewBox: "0 0 32 32",
                        fill: "none",
                        xmlns: "http://www.w3.org/2000/svg",
                        ...s,
                        children: [(0,
                            t.jsxs)("g", {
                                clipPath: "url(#clip0_31_3196)",
                                children: [(0,
                                    t.jsx)("path", {
                                        d: "M22.34 9.5L26 4H18L16 7L14 4H6L9.66 9.5H4V15.1H28V9.5H22.34Z",
                                        fill: "#FFD899"
                                    }), (0,
                                        t.jsx)("path", {
                                            d: "M26.08 19.1001H5.90002V28.5001H26.08V19.1001Z",
                                            fill: "#FFD899"
                                        }), (0,
                                            t.jsx)("path", {
                                                d: "M26.08 15.1001H5.90002V19.1001H26.08V15.1001Z",
                                                fill: "#FF9D00"
                                            })]
                            }), (0,
                                t.jsx)("defs", {
                                    children: (0,
                                        t.jsx)("clipPath", {
                                            id: "clip0_31_3196",
                                            children: (0,
                                                t.jsx)("rect", {
                                                    width: "24",
                                                    height: "24.5",
                                                    fill: "white",
                                                    transform: "translate(4 4)"
                                                })
                                        })
                                })]
                    })
            }
            , L = e => {
                let { ...s } = e;
                return (0,
                    t.jsxs)("svg", {
                        width: "32",
                        height: "32",
                        viewBox: "0 0 32 32",
                        fill: "none",
                        xmlns: "http://www.w3.org/2000/svg",
                        ...s,
                        children: [(0,
                            t.jsx)("path", {
                                d: "M30 18H2V32H30V18Z",
                                fill: "#DDFED1"
                            }), (0,
                                t.jsx)("path", {
                                    d: "M30 8H2V14H30V8Z",
                                    fill: "#DDFED1"
                                }), (0,
                                    t.jsx)("path", {
                                        d: "M10 8H12.5V14H4V18H12.5V32H19.5V18H28V14H19.5V8H22L26 2H18L16 5L14 2H6L10 8Z",
                                        fill: "#53FC18"
                                    })]
                    })
            }
            , V = e => {
                let { ...s } = e;
                return (0,
                    t.jsxs)("svg", {
                        width: "32",
                        height: "32",
                        viewBox: "0 0 32 32",
                        fill: "none",
                        xmlns: "http://www.w3.org/2000/svg",
                        ...s,
                        children: [(0,
                            t.jsxs)("g", {
                                clipPath: "url(#clip0_116_1166)",
                                children: [(0,
                                    t.jsx)("path", {
                                        d: "M0.02 6H32.02V14H0.02V6Z",
                                        fill: "#FFC466"
                                    }), (0,
                                        t.jsx)("path", {
                                            d: "M0.02 18H32.02V32H0.02V18Z",
                                            fill: "#FFC466"
                                        }), (0,
                                            t.jsx)("path", {
                                                d: "M30.02 14H2.02V18H30.02V14Z",
                                                fill: "#FF9D00"
                                            }), (0,
                                                t.jsx)("path", {
                                                    d: "M22.02 6H20.02V32H12.02V6H14.02L18.02 0H26.02L22.02 6Z",
                                                    fill: "#53FC18"
                                                }), (0,
                                                    t.jsx)("path", {
                                                        d: "M14.02 6L16.02 3L14.02 0H6.02L10.02 6H12.02H14.02Z",
                                                        fill: "white"
                                                    }), (0,
                                                        t.jsx)("path", {
                                                            d: "M3.02 9L2.14 7.16C2.14 6.98 1.84 6.88 1.84 6.88L0 6L1.84 5.12L2.14 4.82001L3.02 2.98L3.9 4.82001C3.9 4.92001 4 5.02 4.2 5.12L6.04 5.89999L4.2 6.78C4 6.86 3.9 6.96 3.9 7.16L3.02 9Z",
                                                            fill: "white"
                                                        }), (0,
                                                            t.jsx)("path", {
                                                                d: "M30.02 16L29.44 14.78C29.44 14.64 29.24 14.58 29.24 14.58L28.02 14L29.24 13.42L29.44 13.22L30.02 12L30.6 13.22C30.6 13.22 30.68 13.36 30.8 13.42L32.02 13.94L30.8 14.52C30.66 14.58 30.6 14.66 30.6 14.78L30.02 16Z",
                                                                fill: "white"
                                                            })]
                            }), (0,
                                t.jsx)("defs", {
                                    children: (0,
                                        t.jsx)("clipPath", {
                                            id: "clip0_116_1166",
                                            children: (0,
                                                t.jsx)("rect", {
                                                    width: "32",
                                                    height: "32",
                                                    fill: "white"
                                                })
                                        })
                                })]
                    })
            }
            , v = e => {
                let { ...s } = e;
                return (0,
                    t.jsxs)("svg", {
                        width: "32",
                        height: "32",
                        viewBox: "0 0 32 32",
                        fill: "none",
                        xmlns: "http://www.w3.org/2000/svg",
                        ...s,
                        children: [(0,
                            t.jsx)("path", {
                                d: "M30 18H2V32H30V18Z",
                                fill: "#93EBE0"
                            }), (0,
                                t.jsx)("path", {
                                    d: "M30 8H2V14H30V8Z",
                                    fill: "#93EBE0"
                                }), (0,
                                    t.jsx)("path", {
                                        d: "M10 8H12.5V14H4V18H12.5V32H19.5V18H28V14H19.5V8H22L26 2H18L16 5L14 2H6L10 8Z",
                                        fill: "#00CCB3"
                                    })]
                    })
            }
            , C = e => {
                let { ...s } = e;
                return (0,
                    t.jsxs)("svg", {
                        width: "32",
                        height: "32",
                        viewBox: "0 0 32 32",
                        fill: "none",
                        xmlns: "http://www.w3.org/2000/svg",
                        ...s,
                        children: [(0,
                            t.jsx)("path", {
                                d: "M30 18H2V32H30V18Z",
                                fill: "#DEB2FF"
                            }), (0,
                                t.jsx)("path", {
                                    d: "M30 8H2V14H30V8Z",
                                    fill: "#DEB2FF"
                                }), (0,
                                    t.jsx)("path", {
                                        d: "M10 8H12.5V14H4V18H12.5V32H19.5V18H28V14H19.5V8H22L26 2H18L16 5L14 2H6L10 8Z",
                                        fill: "#BC66FF"
                                    })]
                    })
            }
            , b = e => {
                let { ...s } = e;
                return (0,
                    t.jsxs)("svg", {
                        width: "32",
                        height: "32",
                        viewBox: "0 0 32 32",
                        fill: "none",
                        xmlns: "http://www.w3.org/2000/svg",
                        ...s,
                        children: [(0,
                            t.jsx)("path", {
                                d: "M30 18H2V32H30V18Z",
                                fill: "#FBCFD8"
                            }), (0,
                                t.jsx)("path", {
                                    d: "M30 8H2V14H30V8Z",
                                    fill: "#FBCFD8"
                                }), (0,
                                    t.jsx)("path", {
                                        d: "M10 8H12.5V14H4V18H12.5V32H19.5V18H28V14H19.5V8H22L26 2H18L16 5L14 2H6L10 8Z",
                                        fill: "#F2708A"
                                    })]
                    })
            }
            , F = e => {
                let { ...s } = e;
                return (0,
                    t.jsxs)("svg", {
                        width: "32",
                        height: "32",
                        viewBox: "0 0 32 32",
                        fill: "none",
                        xmlns: "http://www.w3.org/2000/svg",
                        ...s,
                        children: [(0,
                            t.jsx)("path", {
                                d: "M30 18H2V32H30V18Z",
                                fill: "#FFD899"
                            }), (0,
                                t.jsx)("path", {
                                    d: "M30 8H2V14H30V8Z",
                                    fill: "#FFD899"
                                }), (0,
                                    t.jsx)("path", {
                                        d: "M10 8H12.5V14H4V18H12.5V32H19.5V18H28V14H19.5V8H22L26 2H18L16 5L14 2H6L10 8Z",
                                        fill: "#FF9D00"
                                    })]
                    })
            }
            , _ = e => {
                let { ...s } = e;
                return (0,
                    t.jsxs)("svg", {
                        width: "32",
                        height: "32",
                        viewBox: "0 0 32 32",
                        fill: "none",
                        xmlns: "http://www.w3.org/2000/svg",
                        ...s,
                        children: [(0,
                            t.jsxs)("g", {
                                clipPath: "url(#clip0_31_3214)",
                                children: [(0,
                                    t.jsx)("path", {
                                        d: "M0 6H32V12H0V6Z",
                                        fill: "#53FC18"
                                    }), (0,
                                        t.jsx)("path", {
                                            d: "M2 16H30V32H2V16Z",
                                            fill: "#53FC18"
                                        }), (0,
                                            t.jsx)("path", {
                                                d: "M30 12H2V16H30V12Z",
                                                fill: "#32970E"
                                            }), (0,
                                                t.jsx)("path", {
                                                    d: "M14 6L16 3L14 0H6L10 6H12H14Z",
                                                    fill: "#53FC18"
                                                }), (0,
                                                    t.jsx)("path", {
                                                        d: "M22 6H20V32H12V6H14L18 0H26L22 6Z",
                                                        fill: "#BC66FF"
                                                    })]
                            }), (0,
                                t.jsx)("defs", {
                                    children: (0,
                                        t.jsx)("clipPath", {
                                            id: "clip0_31_3214",
                                            children: (0,
                                                t.jsx)("rect", {
                                                    width: "32",
                                                    height: "32",
                                                    fill: "white"
                                                })
                                        })
                                })]
                    })
            }
            , M = e => {
                let { ...s } = e;
                return (0,
                    t.jsxs)("svg", {
                        width: "32",
                        height: "32",
                        viewBox: "0 0 32 32",
                        fill: "none",
                        xmlns: "http://www.w3.org/2000/svg",
                        ...s,
                        children: [(0,
                            t.jsxs)("g", {
                                clipPath: "url(#clip0_31_3215)",
                                children: [(0,
                                    t.jsx)("path", {
                                        d: "M0 6H32V12H0V6Z",
                                        fill: "#93EBE0"
                                    }), (0,
                                        t.jsx)("path", {
                                            d: "M2 16H30V32H2V16Z",
                                            fill: "#93EBE0"
                                        }), (0,
                                            t.jsx)("path", {
                                                d: "M30 12H2V16H30V12Z",
                                                fill: "#31D6C2"
                                            }), (0,
                                                t.jsx)("path", {
                                                    d: "M14 6L16 3L14 0H6L10 6H12H14Z",
                                                    fill: "#93EBE0"
                                                }), (0,
                                                    t.jsx)("path", {
                                                        d: "M22 6H20V32H12V6H14L18 0H26L22 6Z",
                                                        fill: "#F2708A"
                                                    })]
                            }), (0,
                                t.jsx)("defs", {
                                    children: (0,
                                        t.jsx)("clipPath", {
                                            id: "clip0_31_3215",
                                            children: (0,
                                                t.jsx)("rect", {
                                                    width: "32",
                                                    height: "32",
                                                    fill: "white"
                                                })
                                        })
                                })]
                    })
            }
            , Z = e => {
                let { ...s } = e;
                return (0,
                    t.jsxs)("svg", {
                        width: "32",
                        height: "32",
                        viewBox: "0 0 32 32",
                        fill: "none",
                        xmlns: "http://www.w3.org/2000/svg",
                        ...s,
                        children: [(0,
                            t.jsxs)("g", {
                                clipPath: "url(#clip0_31_3216)",
                                children: [(0,
                                    t.jsx)("path", {
                                        d: "M0 6H32V12H0V6Z",
                                        fill: "#DEB2FF"
                                    }), (0,
                                        t.jsx)("path", {
                                            d: "M2 16H30V32H2V16Z",
                                            fill: "#DEB2FF"
                                        }), (0,
                                            t.jsx)("path", {
                                                d: "M30 12H2V16H30V12Z",
                                                fill: "#BC66FF"
                                            }), (0,
                                                t.jsx)("path", {
                                                    d: "M14 6L16 3L14 0H6L10 6H12H14Z",
                                                    fill: "#DEB2FF"
                                                }), (0,
                                                    t.jsx)("path", {
                                                        d: "M22 6H20V32H12V6H14L18 0H26L22 6Z",
                                                        fill: "#1475E1"
                                                    })]
                            }), (0,
                                t.jsx)("defs", {
                                    children: (0,
                                        t.jsx)("clipPath", {
                                            id: "clip0_31_3216",
                                            children: (0,
                                                t.jsx)("rect", {
                                                    width: "32",
                                                    height: "32",
                                                    fill: "white"
                                                })
                                        })
                                })]
                    })
            }
            , y = e => {
                let { ...s } = e;
                return (0,
                    t.jsxs)("svg", {
                        width: "32",
                        height: "32",
                        viewBox: "0 0 32 32",
                        fill: "none",
                        xmlns: "http://www.w3.org/2000/svg",
                        ...s,
                        children: [(0,
                            t.jsxs)("g", {
                                clipPath: "url(#clip0_31_3217)",
                                children: [(0,
                                    t.jsx)("path", {
                                        d: "M0 6H32V12H0V6Z",
                                        fill: "#F2708A"
                                    }), (0,
                                        t.jsx)("path", {
                                            d: "M2 16H30V32H2V16Z",
                                            fill: "#F2708A"
                                        }), (0,
                                            t.jsx)("path", {
                                                d: "M30 12H2V16H30V12Z",
                                                fill: "#E9113C"
                                            }), (0,
                                                t.jsx)("path", {
                                                    d: "M14 6L16 3L14 0H6L10 6H12H14Z",
                                                    fill: "#F2708A"
                                                }), (0,
                                                    t.jsx)("path", {
                                                        d: "M22 6H20V32H12V6H14L18 0H26L22 6Z",
                                                        fill: "#31D6C2"
                                                    })]
                            }), (0,
                                t.jsx)("defs", {
                                    children: (0,
                                        t.jsx)("clipPath", {
                                            id: "clip0_31_3217",
                                            children: (0,
                                                t.jsx)("rect", {
                                                    width: "32",
                                                    height: "32",
                                                    fill: "white"
                                                })
                                        })
                                })]
                    })
            }
            , N = e => {
                let { ...s } = e;
                return (0,
                    t.jsxs)("svg", {
                        width: "32",
                        height: "32",
                        viewBox: "0 0 32 32",
                        fill: "none",
                        xmlns: "http://www.w3.org/2000/svg",
                        ...s,
                        children: [(0,
                            t.jsxs)("g", {
                                clipPath: "url(#clip0_31_3218)",
                                children: [(0,
                                    t.jsx)("path", {
                                        d: "M0 6H32V12H0V6Z",
                                        fill: "#FFC466"
                                    }), (0,
                                        t.jsx)("path", {
                                            d: "M2 16H30V32H2V16Z",
                                            fill: "#FFC466"
                                        }), (0,
                                            t.jsx)("path", {
                                                d: "M30 12H2V16H30V12Z",
                                                fill: "#FF9D00"
                                            }), (0,
                                                t.jsx)("path", {
                                                    d: "M14 6L16 3L14 0H6L10 6H12H14Z",
                                                    fill: "#FFC466"
                                                }), (0,
                                                    t.jsx)("path", {
                                                        d: "M22 6H20V32H12V6H14L18 0H26L22 6Z",
                                                        fill: "#00AD93"
                                                    })]
                            }), (0,
                                t.jsx)("defs", {
                                    children: (0,
                                        t.jsx)("clipPath", {
                                            id: "clip0_31_3218",
                                            children: (0,
                                                t.jsx)("rect", {
                                                    width: "32",
                                                    height: "32",
                                                    fill: "white"
                                                })
                                        })
                                })]
                    })
            }
            ;
        var B = l(64815);
        let E = e => {
            if (e > 0 && e < 5)
                return "#53FC18";
            if (e >= 5 && e < 10)
                return "#2EFAD1";
            if (e >= 10 && e < 25)
                return "#C070FF";
            if (e >= 25 && e < 50)
                return "#FF50A8";
            if (e >= 50 && e < 100)
                return "#FFC466";
            else if (e >= 100 && e < 150)
                return "#81FD54";
            else if (e >= 150 && e < 200)
                return "#2EFAD1";
            else if (e >= 200 && e < 250)
                return "#72ACED";
            else if (e >= 250 && e < 300)
                return "#C070FF";
            else if (e >= 300 && e < 350)
                return "#FF50A8";
            else if (e >= 350 && e < 400)
                return "#FFC466";
            else if (e >= 400 && e < 450)
                return "#81FD54";
            else if (e >= 450 && e < 500)
                return "#81FD54";
            else if (e >= 500 && e < 550)
                return "#81FD54";
            else if (e >= 550 && e < 600)
                return "#2EFAD1";
            else if (e >= 600 && e < 650)
                return "#C070FF";
            else if (e >= 650 && e < 700)
                return "#FF50A8";
            else if (e >= 700 && e < 750)
                return "#FFC466";
            else if (e >= 750 && e < 800)
                return "#81FD54";
            else if (e >= 800 && e < 850)
                return "#2EFAD1";
            else if (e >= 850 && e < 900)
                return "#C070FF";
            else if (e >= 900 && e < 950)
                return "#FFC466";
            return "#52FC18"
        }
            , D = e => {
                let s, { quantity: l, tooltipText: E, tooltipSide: D = "top", tooltipAlign: P = "center", ...z } = e;
                return (l > 0 && l < 5 ? s = (0,
                    t.jsx)(i, {
                        ...z
                    }) : l >= 5 && l < 10 ? s = (0,
                        t.jsx)(w, {
                            ...z
                        }) : l >= 10 && l < 25 ? s = (0,
                            t.jsx)(n, {
                                ...z
                            }) : l >= 25 && l < 50 ? s = (0,
                                t.jsx)(c, {
                                    ...z
                                }) : l >= 50 && l < 100 ? s = (0,
                                    t.jsx)(m, {
                                        ...z
                                    }) : l >= 100 && l < 150 ? s = (0,
                                        t.jsx)(r, {
                                            ...z
                                        }) : l >= 150 && l < 200 ? s = (0,
                                            t.jsx)(h, {
                                                ...z
                                            }) : l >= 200 && l < 250 ? s = (0,
                                                t.jsx)(d, {
                                                    ...z
                                                }) : l >= 250 && l < 300 ? s = (0,
                                                    t.jsx)(o, {
                                                        ...z
                                                    }) : l >= 300 && l < 350 ? s = (0,
                                                        t.jsx)(j, {
                                                            ...z
                                                        }) : l >= 350 && l < 400 ? s = (0,
                                                            t.jsx)(f, {
                                                                ...z
                                                            }) : l >= 400 && l < 450 ? s = (0,
                                                                t.jsx)(u, {
                                                                    ...z
                                                                }) : l >= 450 && l < 500 ? s = (0,
                                                                    t.jsx)(H, {
                                                                        ...z
                                                                    }) : l >= 500 && l < 550 ? s = (0,
                                                                        t.jsx)(L, {
                                                                            ...z
                                                                        }) : l >= 550 && l < 600 ? s = (0,
                                                                            t.jsx)(v, {
                                                                                ...z
                                                                            }) : l >= 600 && l < 650 ? s = (0,
                                                                                t.jsx)(C, {
                                                                                    ...z
                                                                                }) : l >= 650 && l < 700 ? s = (0,
                                                                                    t.jsx)(b, {
                                                                                        ...z
                                                                                    }) : l >= 700 && l < 750 ? s = (0,
                                                                                        t.jsx)(F, {
                                                                                            ...z
                                                                                        }) : l >= 750 && l < 800 ? s = (0,
                                                                                            t.jsx)(_, {
                                                                                                ...z
                                                                                            }) : l >= 800 && l < 850 ? s = (0,
                                                                                                t.jsx)(M, {
                                                                                                    ...z
                                                                                                }) : l >= 850 && l < 900 ? s = (0,
                                                                                                    t.jsx)(Z, {
                                                                                                        ...z
                                                                                                    }) : l >= 900 && l < 950 ? s = (0,
                                                                                                        t.jsx)(y, {
                                                                                                            ...z
                                                                                                        }) : l >= 950 && l < 1e3 ? s = (0,
                                                                                                            t.jsx)(N, {
                                                                                                                ...z
                                                                                                            }) : l >= 1e3 && l < 2e3 ? s = (0,
                                                                                                                t.jsx)(a, {
                                                                                                                    ...z
                                                                                                                }) : l >= 2e3 && l < 3e3 ? s = (0,
                                                                                                                    t.jsx)(x, {
                                                                                                                        ...z
                                                                                                                    }) : l >= 3e3 && l < 4e3 ? s = (0,
                                                                                                                        t.jsx)(p, {
                                                                                                                            ...z
                                                                                                                        }) : l >= 4e3 && l < 5e3 ? s = (0,
                                                                                                                            t.jsx)(g, {
                                                                                                                                ...z
                                                                                                                            }) : l >= 5e3 && (s = (0,
                                                                                                                                t.jsx)(V, {
                                                                                                                                    ...z
                                                                                                                                })),
                    E) ? (0,
                        t.jsx)(B.m, {
                            disabled: !0,
                            side: D,
                            align: P,
                            text: E,
                            children: (0,
                                t.jsx)("div", {
                                    children: s
                                })
                        }) : s
            }
    }
    ,
    31581: (e, s, l) => {
        l.d(s, {
            Mz: () => h,
            UC: () => o,
            ZL: () => x,
            bL: () => a,
            bm: () => d,
            l9: () => c
        });
        var t = l(61873)
            , i = l(90901)
            , n = l(77351)
            , r = l(48799);
        let a = n.bL
            , h = n.Mz
            , d = n.bm
            , x = n.ZL
            , c = i.forwardRef((e, s) => (0,
                t.jsx)(n.l9, {
                    size: "sm",
                    variant: "highlight",
                    ref: s,
                    ...e
                }))
            , o = i.forwardRef((e, s) => {
                let { className: l, align: i = "center", sideOffset: a = 4, ...h } = e;
                return (0,
                    t.jsx)(n.ZL, {
                        children: (0,
                            t.jsx)(n.UC, {
                                ref: s,
                                align: i,
                                sideOffset: a,
                                className: (0,
                                    r.cn)("z-dropdown bg-surface-base flex h-fit min-w-fit flex-col gap-1 rounded p-2 text-sm shadow-lg", "data-[side=bottom]:animate-slideUpAndFade data-[side=left]:animate-slideRightAndFade data-[side=right]:animate-slideLeftAndFade data-[side=top]:animate-slideDownAndFade will-change-[opacity,transform]", l),
                                ...h
                            })
                    })
            }
            );
        o.displayName = n.UC.displayName
    }
    ,
    42177: (e, s, l) => {
        l.d(s, {
            c: () => d
        });
        var t = l(61873)
            , i = l(52737)
            , n = l(48799);
        let r = (0,
            i.tv)({
                base: "",
                variants: {
                    orientation: {
                        horizontal: "h-px w-full",
                        vertical: "w-px h-full"
                    },
                    variant: {
                        primary: "bg-surface-onSurfacePrimary",
                        secondary: "bg-outline-decorative"
                    }
                },
                defaultVariants: {
                    orientation: "horizontal",
                    variant: "secondary"
                }
            })
            , a = (0,
                i.tv)({
                    base: "hidden lg:flex items-center gap-4",
                    variants: {
                        orientation: {
                            horizontal: "flex-row",
                            vertical: "flex-col"
                        }
                    },
                    defaultVariants: {
                        orientation: "horizontal"
                    }
                })
            , h = (0,
                i.tv)({
                    base: "text-surface-onSurfaceSecondary shrink-0 text-sm font-medium",
                    variants: {
                        orientation: {
                            horizontal: "px-3",
                            vertical: "py-3"
                        }
                    },
                    defaultVariants: {
                        orientation: "horizontal"
                    }
                })
            , d = e => {
                let { className: s, orientation: l, variant: i, label: d, ...x } = e
                    , c = "secondary" === i ? d ? "bg-outline-region" : "bg-outline-decorative" : "bg-surface-primary";
                return d ? (0,
                    t.jsxs)("div", {
                        className: (0,
                            n.cn)(a({
                                orientation: l
                            }), s),
                        ...x,
                        children: [(0,
                            t.jsx)("div", {
                                className: (0,
                                    n.cn)(r({
                                        orientation: l,
                                        variant: i
                                    }), "grow", "secondary" === i && "bg-outline-region")
                            }), (0,
                                t.jsx)("span", {
                                    className: h({
                                        orientation: l
                                    }),
                                    children: d
                                }), (0,
                                    t.jsx)("div", {
                                        className: (0,
                                            n.cn)(r({
                                                orientation: l,
                                                variant: i
                                            }), "grow", "secondary" === i && "bg-outline-region")
                                    })]
                    }) : (0,
                        t.jsx)("div", {
                            className: (0,
                                n.cn)(r({
                                    orientation: l,
                                    variant: i
                                }), c, s),
                            ...x
                        })
            }
    }
    ,
    57003: (e, s, l) => {
        l.d(s, {
            H1: () => n,
            H2: () => r,
            H3: () => a
        });
        var t = l(61873);
        let i = (0,
            l(52737).tv)({
                base: "font-semibold",
                variants: {
                    variant: {
                        primary: "text-white",
                        secondary: "text-neutral-200"
                    },
                    size: {
                        sm: "text-sm",
                        md: "text-base",
                        lg: "text-lg",
                        xl: "text-xl",
                        "2xl": "text-base lg:text-2xl leading-[1.2] font-bold",
                        "3xl": "text-3xl",
                        "4xl": "text-4xl"
                    }
                }
            })
            , n = e => {
                let { className: s, variant: l, size: n, ...r } = e;
                return (0,
                    t.jsx)("h1", {
                        className: i({
                            variant: null != l ? l : "primary",
                            size: null != n ? n : "4xl",
                            className: s
                        }),
                        ...r,
                        children: r.children
                    })
            }
            , r = e => {
                let { className: s, variant: l, size: n, ...r } = e;
                return (0,
                    t.jsx)("h2", {
                        className: i({
                            variant: null != l ? l : "primary",
                            size: null != n ? n : "2xl",
                            className: s
                        }),
                        ...r,
                        children: r.children
                    })
            }
            , a = e => {
                let { className: s, variant: l, size: n, ...r } = e;
                return (0,
                    t.jsx)("h3", {
                        className: i({
                            variant: null != l ? l : "primary",
                            size: null != n ? n : "lg",
                            className: s
                        }),
                        ...r,
                        children: r.children
                    })
            }
    }
    ,
    64815: (e, s, l) => {
        l.d(s, {
            m: () => r
        });
        var t = l(61873)
            , i = l(48799)
            , n = l(78515);
        let r = e => {
            let { text: s, content: l, children: r, disabled: a, side: h = "top", align: d = "center", sideOffset: x = 5, withArrow: c = !0, contentClass: o, rootProps: j = {}, wrapperAttributes: p = {} } = e;
            return (0,
                t.jsxs)(n.m_, {
                    disableHoverableContent: a || !s && !l,
                    ...j,
                    children: [(0,
                        t.jsx)(n.k$, {
                            asChild: !0,
                            children: r
                        }), (0,
                            t.jsx)(n.KL, {
                                children: (0,
                                    t.jsx)(n.ZI, {
                                        className: (0,
                                            i.cn)("z-tooltip", o),
                                        sideOffset: x,
                                        side: h,
                                        align: d,
                                        asChild: !0,
                                        children: (0,
                                            t.jsxs)("div", {
                                                ...p,
                                                className: "pointer-events-none size-full",
                                                children: [l || s, c && (0,
                                                    t.jsx)(n.PR, {
                                                        className: "fill-white"
                                                    })]
                                            })
                                    })
                            })]
                })
        }
    }
    ,
    78515: (e, s, l) => {
        l.d(s, {
            m_: () => d,
            PR: () => o,
            ZI: () => h,
            KL: () => j,
            TooltipProvider: () => x,
            k$: () => c
        });
        var t = l(23653)
            , i = l(61873)
            , n = l(90901)
            , r = l(48799);
        let a = (0,
            n.forwardRef)((e, s) => {
                let { className: l, children: n, sideOffset: a, align: h, ...d } = e;
                return (0,
                    i.jsx)(t.UC, {
                        ref: s,
                        sideOffset: a,
                        align: h,
                        className: (0,
                            r.cn)("z-tooltip select-none rounded-md bg-white p-[5px] text-sm font-medium leading-5 text-black", "data-[state=delayed-open]:data-[side=bottom]:animate-slideUpAndFade data-[state=delayed-open]:data-[side=left]:animate-slideRightAndFade data-[state=delayed-open]:data-[side=right]:animate-slideLeftAndFade data-[state=delayed-open]:data-[side=top]:animate-slideDownAndFade will-change-[transform,opacity]", l),
                        ...d,
                        children: n
                    })
            }
            );
        a.displayName = t.UC.displayName;
        let h = a
            , d = t.bL
            , x = t.Kq
            , c = t.l9
            , o = t.i3
            , j = t.ZL
    }
    ,
    99742: (e, s, l) => {
        l.d(s, {
            G: () => n,
            Y: () => i
        });
        var t = l(61873);
        let i = e => {
            let { fill: s, ...l } = e;
            return (0,
                t.jsxs)("svg", {
                    width: "32",
                    height: "32",
                    viewBox: "0 0 32 32",
                    fill: s || "white",
                    xmlns: "http://www.w3.org/2000/svg",
                    ...l,
                    children: [(0,
                        t.jsx)("path", {
                            d: "M5.00215 17.5057L12.6433 13.9772C13.2297 13.7058 13.7024 13.233 13.9737 12.6464L17.5011 5.00286L21.0285 12.6464C21.2998 13.233 21.7724 13.7058 22.3589 13.9772L30 17.5057L22.3589 21.0342C21.7724 21.3056 21.2998 21.7784 21.0285 22.365L17.5011 30.0085L13.9737 22.365C13.7024 21.7784 13.2297 21.3056 12.6433 21.0342L4.9934 17.5057H5.00215Z"
                        }), (0,
                            t.jsx)("path", {
                                d: "M2 7.37587L5.29104 5.86117C5.54487 5.74735 5.74618 5.54597 5.85997 5.29207L7.37419 2L8.88842 5.29207C9.0022 5.54597 9.20352 5.74735 9.45735 5.86117L12.7484 7.37587L9.45735 8.89057C9.20352 9.0044 9.0022 9.20577 8.88842 9.45968L7.37419 12.7517L5.85997 9.46844C5.74618 9.21453 5.54487 9.01315 5.29104 8.89933L2 7.38463V7.37587Z"
                            })]
                })
        }
            , n = e => {
                let { fill: s, ...l } = e;
                return (0,
                    t.jsxs)("svg", {
                        xmlns: "http://www.w3.org/2000/svg",
                        width: "32",
                        height: "32",
                        viewBox: "0 0 32 32",
                        fill: s || "white",
                        ...l,
                        children: [(0,
                            t.jsxs)("g", {
                                clipPath: "url(#clip0_232_3499)",
                                children: [(0,
                                    t.jsx)("path", {
                                        d: "M22.94 15.06L19 6L15.06 15.06L6 19L15.06 22.94L19 32L22.94 22.94L32 19L22.94 15.06Z",
                                        fill: "current"
                                    }), (0,
                                        t.jsx)("path", {
                                            d: "M9.12 9.12L14 7L9.12 4.88L7 0L4.88 4.88L0 7L4.88 9.12L7 14L9.12 9.12Z",
                                            fill: "current"
                                        })]
                            }), (0,
                                t.jsx)("defs", {
                                    children: (0,
                                        t.jsx)("clipPath", {
                                            id: "clip0_232_3499",
                                            children: (0,
                                                t.jsx)("rect", {
                                                    width: "32",
                                                    height: "32",
                                                    fill: "current"
                                                })
                                        })
                                })]
                    })
            }
    }
}]);
