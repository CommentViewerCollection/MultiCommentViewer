(self.__LOADABLE_LOADED_CHUNKS__ = self.__LOADABLE_LOADED_CHUNKS__ || []).push([[1436], {
    71141: (e, t, i) => {
        "use strict";
        i.d(t, {
            K: () => o,
            v: () => r
        });
        var s = i(67406);
        function o() {
            return Promise.resolve({
                status: 0,
                message: "",
                data: (0,
                    s.A)().urls.chat.host || ""
            })
        }
        function r() {
            return Promise.resolve({
                status: 0,
                message: "",
                data: (0,
                    s.A)().urls.viewers.host || ""
            })
        }
    }
    ,
    4105: (e, t, i) => {
        "use strict";
        i.d(t, {
            QL: () => o,
            cR: () => a,
            f5: () => r,
            oT: () => n
        });
        var s = i(87324);
        function o(e, t) {
            return (0,
                s.A)("DELETE", `/movies/${e}/chat-moderators/${t}`, {
                    query: void 0,
                    body: void 0
                })
        }
        function r(e, t) {
            return (0,
                s.A)("POST", `/movies/${e}/chat-moderators`, {
                    query: void 0,
                    body: t
                })
        }
        function a(e, t, i = {}) {
            return (0,
                s.A)("PUT", `/movies/${e}/chat-moderators/${t}`, {
                    query: void 0,
                    body: i
                })
        }
        function n(e, t = {}) {
            return (0,
                s.A)("PUT", `/movies/${e}/chat-moderators`, {
                    query: void 0,
                    body: t
                })
        }
    }
    ,
    98013: (e, t, i) => {
        "use strict";
        i.d(t, {
            QY: () => r,
            f6: () => o
        });
        var s = i(87324);
        function o() {
            return (0,
                s.A)("GET", "/users/me/chat-setting", {
                    query: void 0,
                    body: void 0
                })
        }
        function r(e = {}) {
            return (0,
                s.A)("PUT", "/users/me/chat-setting", {
                    query: void 0,
                    body: e
                })
        }
    }
    ,
    89056: (e, t, i) => {
        "use strict";
        i.d(t, {
            J: () => o,
            M: () => r
        });
        var s = i(87324);
        function o(e, t) {
            return (0,
                s.A)("DELETE", `/movies/${e}/chats/${t}`, {
                    query: void 0,
                    body: void 0
                })
        }
        function r(e, t) {
            return (0,
                s.A)("POST", `/movies/${e}/chats`, {
                    query: void 0,
                    body: t
                })
        }
    }
    ,
    13087: (e, t, i) => {
        "use strict";
        i.d(t, {
            a8: () => o
        });
        var s = i(87324);
        function o(e, t = {}) {
            return (0,
                s.A)("PUT", `/users/me/dashboard_broadcasts/${e}`, {
                    query: void 0,
                    body: t
                })
        }
    }
    ,
    89844: (e, t, i) => {
        "use strict";
        i.d(t, {
            G: () => r,
            V: () => o
        });
        var s = i(78432);
        function o(e, t) {
            return (0,
                s.A)("DELETE", `/movies/${e}/chats/${t}`, {
                    query: void 0,
                    body: void 0
                })
        }
        function r(e, t) {
            return (0,
                s.A)("POST", `/movies/${e}/chats`, {
                    query: void 0,
                    body: t
                })
        }
    }
    ,
    9720: (e, t, i) => {
        "use strict";
        i.d(t, {
            k: () => o
        });
        var s = i(23700);
        function o(e, t = {}) {
            return (0,
                s.A)("GET", `/movies/${e}/chats`, {
                    query: t,
                    body: void 0
                })
        }
    }
    ,
    43203: (e, t, i) => {
        "use strict";
        i.d(t, {
            J: () => o
        });
        var s = i(23700);
        function o() {
            return (0,
                s.A)("GET", "/fixed-phrases", {
                    query: void 0,
                    body: void 0
                })
        }
    }
    ,
    92915: (e, t, i) => {
        "use strict";
        i.d(t, {
            P: () => o
        });
        var s = i(23700);
        function o(e) {
            return (0,
                s.A)("GET", "/league-ranks", {
                    query: e,
                    body: void 0
                })
        }
    }
    ,
    26504: (e, t, i) => {
        "use strict";
        i.d(t, {
            Q: () => o
        });
        var s = i(23700);
        function o(e = {}) {
            return (0,
                s.A)("GET", "/reaction-products", {
                    query: e,
                    body: void 0
                })
        }
    }
    ,
    4571: (e, t, i) => {
        "use strict";
        i.d(t, {
            A: () => s
        });
        const s = {
            all: () => ["movie"],
            one: () => ["movie", "one"],
            oneOf: (e, ...t) => ["movie", "one", e, ...t],
            list: () => ["movie", "list"],
            listOf: (e, ...t) => ["movie", "list", e, ...t]
        }
    }
    ,
    98932: (e, t, i) => {
        "use strict";
        i.d(t, {
            A: () => s
        });
        const s = {
            all: () => ["my", "movieDetail"],
            one: () => ["my", "movieDetail", "one"],
            oneOf: (e, ...t) => ["my", "movieDetail", "one", e, ...t]
        }
    }
    ,
    14418: (e, t, i) => {
        "use strict";
        i.d(t, {
            A: () => n
        });
        var s = i(31370)
            , o = i(96540)
            , r = i(92568)
            , a = i(84881);
        const n = (0,
            s.WQ)("localeStore")((0,
                s.PA)(e => {
                    const t = e.localeStore
                        , { className: i, reaction: s, size: r = "m" } = e;
                    return o.createElement(d, {
                        alt: t.getString(s.label),
                        className: i,
                        src: s.reactionFile,
                        size: r
                    })
                }
                ))
            , l = a.Ej({
                m: (0,
                    r.AH)(["width:1.6rem;height:1.6rem;"]),
                l: (0,
                    r.AH)(["width:2rem;height:2rem;"]),
                xl: (0,
                    r.AH)(["width:3rem;height:3rem;"])
            })
            , d = r.Ay.img.withConfig({
                componentId: "sc-6ep55y-0"
            })(["", ""], l)
    }
    ,
    20656: (e, t, i) => {
        "use strict";
        i.d(t, {
            A: () => h,
            H: () => u
        });
        var s = i(96540)
            , o = i(92568)
            , r = i(14418)
            , a = i(46564)
            , n = i(77914)
            , l = i(84881);
        const d = e => {
            const { reaction: t, size: i = "m", className: o, onClick: r } = e
                , [a, n] = s.useState(!1)
                , l = s.useMemo(() => {
                    if (r)
                        return () => {
                            return e = function* () {
                                yield r(t),
                                    n(!0)
                            }
                                ,
                                new Promise((t, i) => {
                                    var s = t => {
                                        try {
                                            r(e.next(t))
                                        } catch (e) {
                                            i(e)
                                        }
                                    }
                                        , o = t => {
                                            try {
                                                r(e.throw(t))
                                            } catch (e) {
                                                i(e)
                                            }
                                        }
                                        , r = e => e.done ? t(e.value) : Promise.resolve(e.value).then(s, o);
                                    r((e = e.apply(void 0, null)).next())
                                }
                                );
                            var e
                        }
                }
                    , [r, t])
                , d = t.count > 0 && s.createElement(b, {
                    size: i
                }, t.count.toLocaleString());
            return s.createElement(y, {
                className: o,
                animationEnabled: a,
                isReaction: t.isReaction,
                size: i,
                onClick: l
            }, s.createElement(g, {
                reaction: t,
                size: "m"
            }), d)
        }
            , h = d
            , u = (0,
                a.d)(d)
            , c = (0,
                l.Ej)({
                    s: "1.6rem",
                    m: "1.6rem",
                    l: "2.4rem"
                })
            , p = (0,
                l.Ej)({
                    s: "0",
                    m: "0",
                    l: "0.4rem"
                })
            , m = (0,
                l.Ej)({
                    s: "0",
                    m: "0",
                    l: "0.1rem"
                })
            , y = o.Ay.span.withConfig({
                componentId: "sc-1vivtkm-0"
            })(["display:inline-flex;align-items:center;justify-content:center;padding-right:", ";padding-left:", ";height:", ";border:", " solid transparent;border-radius:0.4rem;", " ", " ", ""], p, p, c, m, ({ onClick: e }) => !!e && (0,
                o.AH)(["border-color:var(--or_theme_frame);", " overflow:visible;"], (0,
                    n.vY)("button")), ({ isReaction: e }) => e && (0,
                        o.AH)(["&&{border-color:var(--or_theme_accent);}"]), ({ animationEnabled: e, isReaction: t }) => e && t && (0,
                            o.AH)(["> ", "{animation-name:stamp;}"], g))
            , g = (0,
                o.Ay)(r.A).withConfig({
                    componentId: "sc-1vivtkm-1"
                })(["animation-duration:0.2s;animation-direction:alternate;animation-iteration-count:2;@keyframes stamp{0%{transform:scale(1.0) rotate(0deg);}100%{transform:scale(2.2) rotate(-10deg);}}"])
            , v = (0,
                l.Ej)({
                    s: "0.2rem",
                    m: "0.4rem",
                    l: "0.4rem"
                })
            , S = (0,
                l.Ej)({
                    s: "1rem",
                    m: "1.2rem",
                    l: "1.4rem"
                })
            , b = o.Ay.span.withConfig({
                componentId: "sc-1vivtkm-2"
            })(["margin-left:", ";font-size:", ";font-weight:normal;color:var(--or_theme_text-base);"], v, S)
    }
    ,
    3644: (e, t, i) => {
        "use strict";
        i.d(t, {
            A: () => d,
            c: () => h
        });
        var s = i(31370)
            , o = i(96540)
            , r = i(92568)
            , a = i(46564)
            , n = i(84881);
        const l = (0,
            s.WQ)("localeStore")((0,
                s.PA)(e => {
                    const t = e.localeStore
                        , { shareCount: i, badgeImage: s, size: r, className: a } = e;
                    return o.createElement(y, {
                        className: a,
                        size: r
                    }, o.createElement(g, {
                        src: s,
                        size: r,
                        alt: ""
                    }), t.assign(t.dictionary.subscription.shareCount, i.toLocaleString()))
                }
                ))
            , d = l
            , h = (0,
                a.d)(l)
            , u = n.Ej({
                s: "0.4rem",
                m: "0.4rem",
                l: "0.8rem"
            })
            , c = n.Ej({
                s: "1.4rem",
                m: "1.6rem",
                l: "2.4rem"
            })
            , p = n.Ej({
                s: "1rem",
                m: "1rem",
                l: "1.4rem"
            })
            , m = n.Ej({
                s: "1.2rem",
                m: "1.4rem",
                l: "1.6rem"
            })
            , y = r.Ay.span.withConfig({
                componentId: "sc-1d0hcuy-0"
            })(["display:inline-flex;justify-content:center;align-items:center;gap:0.2rem;font-size:", ";height:", ";color:var(--or_theme_text-base);background-color:var(--or_theme_alpha-2);padding:0 ", ";font-weight:bold;"], p, c, u)
            , g = r.Ay.img.withConfig({
                componentId: "sc-1d0hcuy-1"
            })(["width:", ";height:", ";"], m, m)
    }
    ,
    6174: (e, t, i) => {
        "use strict";
        i.d(t, {
            o: () => s
        });
        const s = 5
    }
    ,
    70755: (e, t, i) => {
        "use strict";
        i.d(t, {
            A: () => n
        });
        class s {
            constructor(e, t, i, s, o, r, a) {
                this.id = e,
                    this.chatId = t,
                    this.position = i,
                    this.message = s,
                    this.stamp = o,
                    this.yell = r,
                    this.toUser = a
            }
        }
        var o = i(10310)
            , r = i(88316)
            , a = i(72226);
        class n {
            static createModel(e) {
                return n._isIChatHighlight(e) ? n._createModelByIChatHighlight(e) : n._createModelByChatHighlight(e)
            }
            static _isIChatHighlight(e) {
                return "chatId" in e
            }
            static _createModelByIChatHighlight(e) {
                const t = e.yell ? a.A.createModel(e.yell) : null
                    , i = e.stamp ? o.A.createModel(e.stamp) : null
                    , n = e.toUser ? r.A.createModel(e.toUser) : null;
                return new s(e.chatId || 0, e.chatId || 0, e.position || 0, e.message || "", i, t, n)
            }
            static _createModelByChatHighlight(e) {
                return new s(e.id, e.chatId, e.position, e.message, e.stamp, e.yell, e.toUser)
            }
        }
    }
    ,
    46202: (e, t, i) => {
        "use strict";
        i.d(t, {
            A: () => r
        });
        class s {
            constructor(e, t, i) {
                this.id = e,
                    this.label = t,
                    this.reactionFile = i
            }
        }
        var o = i(95217);
        class r {
            static createModel(e) {
                return r._isIReactionProductExternal(e) ? r._createModelByIReactionProductExternal(e) : r._createModelByReaction(e)
            }
            static _createModelByIReactionProductExternal(e) {
                const t = o.A.createModel(e.label);
                return new s(e.id || "", t, e.reactionFile || "")
            }
            static _createModelByReaction(e) {
                const t = o.A.createModel(e.label);
                return new s(e.id, t, e.reactionFile)
            }
            static _isIReactionProductExternal(e) {
                return !0
            }
        }
    }
    ,
    72226: (e, t, i) => {
        "use strict";
        i.d(t, {
            A: () => a
        });
        class s {
            constructor(e, t, i, s, o, r, a) {
                this.id = e,
                    this.imageUrl = t,
                    this.pointCurrency = i,
                    this.yells = s,
                    this.tickerSeconds = o,
                    this.subsShareCount = r,
                    this.subsBadge = a
            }
        }
        var o = i(62795)
            , r = i(92146);
        class a {
            static createModel(e) {
                return a._isIYellProduct(e) ? a._createModelByIYellProduct(e) : a._isSocketYellProduct(e) ? a._createModelByISocketYell(e) : a._createModelByYellProduct(e)
            }
            static _isIYellProduct(e) {
                return "animationUrl" in e
            }
            static _isSocketYellProduct(e) {
                return "yell_id" in e
            }
            static _createModelByIYellProduct(e) {
                var t, i, a;
                const n = o.A.createModel({
                    type: "point",
                    amount: e.points || 0
                })
                    , l = (null == (t = e.subsShare) ? void 0 : t.subsProduct.subsBadges) && e.subsShare.subsProduct.subsBadges.length > 0 ? r.A.createModel(null == (i = e.subsShare) ? void 0 : i.subsProduct.subsBadges[0]) : void 0;
                return new s(e.id || 0, e.imageUrl || "", n, e.yells || 0, e.tickerSeconds || 0, (null == (a = e.subsShare) ? void 0 : a.shareCount) || 0, l)
            }
            static _createModelByISocketYell(e) {
                const t = o.A.createModel({
                    type: "point",
                    amount: Number(e.points) || 0
                });
                return new s(Number(e.yell_id) || 0, e.image_url, t, Number(e.yells) || 0, Number(e.ticker_seconds) || 0)
            }
            static _createModelByYellProduct(e) {
                const t = o.A.createModel(e.pointCurrency)
                    , i = e.subsBadge ? r.A.createModel(e.subsBadge) : void 0;
                return new s(e.id || 0, e.imageUrl || "", t, e.yells || 0, e.tickerSeconds || 0, e.subsShareCount, i)
            }
        }
    }
    ,
    90327: (e, t, i) => {
        "use strict";
        i.d(t, {
            A: () => S
        });
        var s = i(59324)
            , o = i(4571)
            , r = i(1694)
            , a = i(31634)
            , n = i(20983)
            , l = i(27813)
            , d = i(16980)
            , h = i(53179)
            , u = i(80585)
            , c = Object.defineProperty
            , p = Object.getOwnPropertyDescriptor
            , m = (e, t, i, s) => {
                for (var o, r = s > 1 ? void 0 : s ? p(t, i) : t, a = e.length - 1; a >= 0; a--)
                    (o = e[a]) && (r = (s ? o(t, i, r) : o(r)) || r);
                return s && r && c(t, i, r),
                    r
            }
            ;
        class y extends d.A {
            constructor() {
                super(),
                    this._channelGameMovies = [],
                    this._channelMovies = [],
                    this._gameLiveMovies = [],
                    this._tagLiveMovies = [],
                    this._popularLiveMovies = [],
                    (0,
                        u.H)(this, {
                            movie: l.sH
                        })
            }
            get validChannelGameMovies() {
                const e = this._getMovieIds([this.movie]);
                return this._channelGameMovies.filter(t => !e.includes(t.id))
            }
            get validChannelMovies() {
                const e = this._getMovieIds([this.movie, ...this.validChannelGameMovies]);
                return this._channelMovies.filter(t => !e.includes(t.id))
            }
            get validGameLiveMovies() {
                const e = this._getMovieIds([this.movie, ...this.validChannelGameMovies, ...this.validChannelMovies]);
                return this._gameLiveMovies.filter(t => !e.includes(t.id))
            }
            get validTagLiveMovies() {
                const e = this._getMovieIds([this.movie, ...this.validChannelGameMovies, ...this.validChannelMovies, ...this.validGameLiveMovies]);
                return this._tagLiveMovies.filter(t => !e.includes(t.id))
            }
            get validPopularLiveMovies() {
                const e = this._getMovieIds([this.movie, ...this.validChannelGameMovies, ...this.validChannelMovies, ...this.validGameLiveMovies, ...this.validTagLiveMovies]);
                return this._popularLiveMovies.filter(t => !e.includes(t.id)).filter(e => !(e.isViewersHidden || e.isHidden))
            }
            setMovieData(e, t) {
                this.movie = e,
                    this.deepMovie = t
            }
            setRelatedMovies(e, t, i, s, o) {
                this._channelGameMovies = e,
                    this._channelMovies = t,
                    this._gameLiveMovies = i,
                    this._tagLiveMovies = s,
                    this._popularLiveMovies = o
            }
            _getMovieIds(e) {
                return e.filter(h.d).map(e => e.id)
            }
        }
        m([l.sH], y.prototype, "_channelGameMovies", 2),
            m([l.sH], y.prototype, "_channelMovies", 2),
            m([l.sH], y.prototype, "_gameLiveMovies", 2),
            m([l.sH], y.prototype, "_tagLiveMovies", 2),
            m([l.sH], y.prototype, "_popularLiveMovies", 2),
            m([l.XI], y.prototype, "setMovieData", 1),
            m([l.XI], y.prototype, "setRelatedMovies", 1);
        var g = i(51202)
            , v = (e, t, i) => new Promise((s, o) => {
                var r = e => {
                    try {
                        n(i.next(e))
                    } catch (e) {
                        o(e)
                    }
                }
                    , a = e => {
                        try {
                            n(i.throw(e))
                        } catch (e) {
                            o(e)
                        }
                    }
                    , n = e => e.done ? s(e.value) : Promise.resolve(e.value).then(r, a);
                n((i = i.apply(e, t)).next())
            }
            );
        class S extends n.A {
            constructor() {
                super(...arguments),
                    this.archiveCreatingDialogId = "movie-archive-creating",
                    this.breakTimeAdStartDialogId = "movie-break-time-ad-start",
                    this.castFollowDialogId = "movie-cast-follow",
                    this.pollStartConfirmDialogId = "movie-poll-start-confirm",
                    this.titleEditDialogId = "movie-title-edit",
                    this.storeName = "moviePageStore"
            }
            willLoadOnServer(e, t, i, s) {
                this._willLoadOnAnywhere(e, t, i),
                    this.state.loadStart(),
                    this._restoreFromFallback(s.fallback),
                    this.state.setMovieData(this.movie, this.deepMovie),
                    this.state.loadEnd(),
                    this.extensionStore = i.extensionStore
            }
            willLoad(e, t, i) {
                return v(this, null, function* () {
                    this._willLoadOnAnywhere(e, t, i),
                        this.state.loadStart(),
                        this._restoreFromFallback(window.or.fb) || (yield this._fetch()),
                        this.state.setMovieData(this.movie, this.deepMovie),
                        this.state.loadEnd(),
                        this.extensionStore = i.extensionStore
                })
            }
            willUnload() {
                this.state.destroy()
            }
            _willLoadOnAnywhere(e, t, i) {
                this.state = this.state || new y,
                    this.movieId = t.ids.movieId,
                    this.secretKey = "string" == typeof t.searchParams.secret_key ? t.searchParams.secret_key : void 0
            }
            _restoreFromFallback(e) {
                var t, i;
                const n = (0,
                    s.WI)(o.A.oneOf("byId", this.movieId, this.secretKey))
                    , [l, d] = null != (t = null == e ? void 0 : e[n]) ? t : [];
                return !(!l || !d || (this.movieId = null != (i = l.id) ? i : "",
                    this.movie = a.A.createModel(l),
                    this.deepMovie = r.A.createModel(d),
                    0))
            }
            _fetch() {
                return v(this, null, function* () {
                    try {
                        const { movie: e, deepMovie: t } = yield g.Ay.getById(this.movieId, this.secretKey);
                        this.movieId = e.id,
                            this.movie = e,
                            this.deepMovie = t
                    } catch (e) {
                        this.movie = void 0
                    }
                })
            }
        }
        S.TelopDialogId = "movie-telop",
            S.TelopFinishConfirmId = "movie-telop-finish-confirm"
    }
    ,
    35206: (e, t, i) => {
        "use strict";
        i.d(t, {
            A: () => g
        });
        var s = i(18987)
            , o = i(57698)
            , r = i.n(o)
            , a = i(48497)
            , n = i(37148)
            , l = i.n(n)
            , d = i(67406)
            , h = i(71141)
            , u = i(89056)
            , c = i(9720)
            , p = i(32605)
            , m = i(6174);
        const y = class e {
            connectSocketDeprecated(e, t) {
                const i = (0,
                    d.A)().urls.chat.host || ""
                    , o = {
                        query: {
                            movieId: e,
                            broadcastType: t,
                            uuid: s.A.get(p.b0)
                        },
                        transports: ["websocket"]
                    };
                return r()(i, o)
            }
            generateViewersSocket(e, t) {
                return i = this,
                    o = function* () {
                        const i = yield (0,
                            h.v)();
                        if (i.message)
                            throw new Error(i.message);
                        const o = i.data
                            , r = {
                                movieId: String(e),
                                uuid: s.A.get(p.b0),
                                openrecAuthorization: "",
                                connectAt: String(Math.floor(Date.now() / 1e3)),
                                connectionId: t.connectionId
                            }
                            , n = {
                                autoConnect: !1,
                                query: (l = r,
                                    Object.keys(l).filter(e => void 0 === l[e]).forEach(e => {
                                        delete l[e]
                                    }
                                    ),
                                    l),
                                transports: ["websocket"],
                                reconnectionAttempts: m.o,
                                withCredentials: !0
                            };
                        var l;
                        return (0,
                            a.io)(o, n)
                    }
                    ,
                    new Promise((e, t) => {
                        var s = e => {
                            try {
                                a(o.next(e))
                            } catch (e) {
                                t(e)
                            }
                        }
                            , r = e => {
                                try {
                                    a(o.throw(e))
                                } catch (e) {
                                    t(e)
                                }
                            }
                            , a = t => t.done ? e(t.value) : Promise.resolve(t.value).then(s, r);
                        a((o = o.apply(i, null)).next())
                    }
                    );
                var i, o
            }
            listTo(e, t) {
                const i = "string" == typeof t ? t : void 0
                    , s = "number" == typeof t ? t : void 0;
                return (0,
                    c.k)(e, {
                        toCreatedAt: i,
                        toChatId: s,
                        isIncludingSystemMessage: !1
                    })
            }
            postMessage(e, t, i) {
                if (e && t)
                    return (0,
                        u.M)(e, {
                            message: t,
                            qualityType: i
                        })
            }
            postStamp(e, t, i) {
                if (e)
                    return (0,
                        u.M)(e, {
                            message: "",
                            qualityType: i,
                            stampId: t.id
                        })
            }
            postYell(e, t, i, s) {
                if (e)
                    return (0,
                        u.M)(e, {
                            message: t,
                            qualityType: s,
                            yellId: i.id
                        })
            }
            postEffect(e, t, i, s) {
                if (e)
                    return (0,
                        u.M)(e, {
                            message: "",
                            qualityType: s,
                            useType: t,
                            itemProductKey: i.id,
                            itemProductType: i.type
                        })
            }
            setChatPositionWithFullscreenMode(t) {
                l().set(e.keyChatPositionWithFullscreenMode, t)
            }
            getChatPositionWithFullscreenMode() {
                const t = l().get(e.keyChatPositionWithFullscreenMode);
                return t ? "right" === t ? "right" : "left" : null
            }
            setIsDisplayedChannelStarAlert() {
                l().set(e.keyIsDisplayedChannelStarAlert, !0)
            }
            getIsDisplayedChannelStarAlert() {
                return !!l().get(e.keyIsDisplayedChannelStarAlert)
            }
        }
            ;
        y.keyChatPositionWithFullscreenMode = "or:chat:position-with-fullscreen-mode",
            y.keyIsDisplayedChannelStarAlert = "or:chat:is-displayed-channel-star-alert";
        const g = new y
    }
    ,
    50054: (e, t, i) => {
        "use strict";
        i.d(t, {
            A: () => a
        });
        var s = i(50907)
            , o = i(98013)
            , r = (e, t, i) => new Promise((s, o) => {
                var r = e => {
                    try {
                        n(i.next(e))
                    } catch (e) {
                        o(e)
                    }
                }
                    , a = e => {
                        try {
                            n(i.throw(e))
                        } catch (e) {
                            o(e)
                        }
                    }
                    , n = e => e.done ? s(e.value) : Promise.resolve(e.value).then(r, a);
                n((i = i.apply(e, t)).next())
            }
            );
        const a = new class {
            get() {
                return r(this, null, function* () {
                    const e = yield (0,
                        o.f6)();
                    return (0,
                        s.L)(e),
                        e.data.items[0]
                })
            }
            putMutedWarnedUser(e) {
                return (0,
                    o.QY)({
                        mutedWarnedUser: e
                    })
            }
            putMutedFreshUser(e) {
                return (0,
                    o.QY)({
                        mutedFreshUser: e
                    })
            }
            putMutedUnauthenticatedUser(e) {
                return (0,
                    o.QY)({
                        mutedUnauthenticatedUser: e
                    })
            }
            putMutedBannedWord(e) {
                return (0,
                    o.QY)({
                        mutedBannedWord: e
                    })
            }
            putPostSetting(e, t, i, a, n, l, d, h, u, c, p) {
                return r(this, null, function* () {
                    const r = yield (0,
                        o.QY)({
                            chatRule: e,
                            limitedContinuousChat: t,
                            limitedUnfollowerChat: i,
                            limitedFreshUserChat: a,
                            limitedTemporaryBlacklist: n,
                            continuousChatThreshold: h,
                            unfollowerChatThreshold: u,
                            freshUserChatThreshold: c,
                            temporaryBlacklistThreshold: p,
                            limitedUnsubsMemberChat: l,
                            limitedWarnedUserChat: d
                        });
                    return (0,
                        s.L)(r),
                        r.data.items[0]
                })
            }
        }
    }
    ,
    60683: (e, t, i) => {
        "use strict";
        i.d(t, {
            A: () => h
        });
        var s = i(85947)
            , o = i(59161)
            , r = i(50907)
            , a = i(87324)
            , n = (e, t, i) => new Promise((s, o) => {
                var r = e => {
                    try {
                        n(i.next(e))
                    } catch (e) {
                        o(e)
                    }
                }
                    , a = e => {
                        try {
                            n(i.throw(e))
                        } catch (e) {
                            o(e)
                        }
                    }
                    , n = e => e.done ? s(e.value) : Promise.resolve(e.value).then(r, a);
                n((i = i.apply(e, t)).next())
            }
            );
        const l = new class {
            post(e, t, i, s) {
                return n(this, null, function* () {
                    const o = yield function (e, t) {
                        return (0,
                            a.A)("POST", `/movies/${e}/chapters`, {
                                query: void 0,
                                body: t
                            })
                    }(e, {
                        title: i,
                        chapteredAt: t,
                        gameId: s
                    });
                    return (0,
                        r.L)(o),
                        o.data.items[0]
                })
            }
            delete(e, t) {
                return n(this, null, function* () {
                    const i = yield function (e, t) {
                        return (0,
                            a.A)("DELETE", `/movies/${e}/chapters/${t}`, {
                                query: void 0,
                                body: void 0
                            })
                    }(e, t);
                    (0,
                        r.L)(i)
                })
            }
            edit(e, t, i, s, o) {
                return n(this, null, function* () {
                    const n = yield function (e, t, i = {}) {
                        return (0,
                            a.A)("PUT", `/movies/${e}/chapters/${t}`, {
                                query: void 0,
                                body: i
                            })
                    }(e, t, {
                        title: s,
                        chapteredAt: i,
                        gameId: o
                    });
                    return (0,
                        r.L)(n),
                        n.data.items[0]
                })
            }
            list(e) {
                return n(this, null, function* () {
                    const t = yield function (e) {
                        return (0,
                            a.A)("GET", `/movies/${e}/chapters`, {
                                query: void 0,
                                body: void 0
                            })
                    }(e);
                    return (0,
                        r.j)(t),
                        t.data.items
                })
            }
        }
            ;
        var d = (e, t, i) => new Promise((s, o) => {
            var r = e => {
                try {
                    n(i.next(e))
                } catch (e) {
                    o(e)
                }
            }
                , a = e => {
                    try {
                        n(i.throw(e))
                    } catch (e) {
                        o(e)
                    }
                }
                , n = e => e.done ? s(e.value) : Promise.resolve(e.value).then(r, a);
            n((i = i.apply(e, t)).next())
        }
        );
        const h = new class {
            postChapter(e, t, i, s) {
                return d(this, null, function* () {
                    const r = yield l.post(e, t, i, s);
                    return o.A.createModel(r)
                })
            }
            deleteChapter(e, t) {
                return d(this, null, function* () {
                    return yield l.delete(e, t)
                })
            }
            editChapter(e, t, i, s, r) {
                return d(this, null, function* () {
                    const a = yield l.edit(e, t, i, s, r);
                    return o.A.createModel(a)
                })
            }
            getChapter(e) {
                return d(this, null, function* () {
                    const t = yield l.list(e);
                    return s.A.createMapBy(t, o.A.createModel)
                })
            }
        }
    }
    ,
    25221: (e, t, i) => {
        "use strict";
        i.d(t, {
            Ay: () => P,
            pw: () => M
        });
        var s = i(96540)
            , o = i(36038)
            , r = i(89602)
            , a = i(24708);
        class n {
            constructor(e) {
                this.id = e
            }
            isMessageChat() {
                return !!this.message && !this.yellProduct
            }
            isStampChat() {
                return !!this.stamp
            }
            isYellChat() {
                return !!this.yellProduct
            }
            isItemChat() {
                return !!this.itemProduct
            }
            isPostSettingChat() {
                return !!this.postSetting
            }
            isSystemMessageChat() {
                return !!this.systemMessage
            }
        }
        var l = i(10550);
        class d extends n {
            constructor(e, t, i, s) {
                super(e),
                    this.id = e,
                    this.user = t,
                    this.message = i,
                    this.itemProduct = s,
                    this.message = l.pl(i)
            }
        }
        class h extends n {
            constructor(e, t, i) {
                super(e),
                    this.id = e,
                    this.user = t,
                    this.message = i,
                    this.message = l.pl(i)
            }
        }
        class u extends n {
            constructor(e, t, i) {
                super(e),
                    this.user = t,
                    this.stamp = i
            }
        }
        class c extends n {
            constructor(e, t, i, s) {
                super(e),
                    this.id = e,
                    this.user = t,
                    this.message = i,
                    this.yellProduct = s,
                    this.message = l.pl(i)
            }
        }
        class p {
            get isEffect() {
                return "effect" === this.type
            }
            get isTicket() {
                return "ticket" === this.type
            }
            get isSuperdoor() {
                return "superdoor" === this.type
            }
            get isSuperhoop() {
                return "superhoop" === this.type
            }
            constructor(e, t, i, s, o, r, a, n) {
                this.id = e,
                    this.name = t,
                    this.type = i,
                    this.imageUrl = s,
                    this.stoneCurrency = o,
                    this.pointCurrency = r,
                    this.baseCount = a,
                    this.availableDays = n
            }
            getPayableCurrency(e, t) {
                return this._isStonePayable(e) ? this.stoneCurrency : this._isPointPayable(t) ? this.pointCurrency : null
            }
            _isStonePayable(e) {
                return this.stoneCurrency.amount < e.amount
            }
            _isPointPayable(e) {
                return this.pointCurrency.amount < e.amount
            }
        }
        var m = i(62795)
            , y = i(95217);
        class g {
            static createModel(e) {
                return g._isIItemProduct(e) ? g._createModelByIItemProduct(e) : g._createModelByISocketItem(e)
            }
            static _createModelByIItemProduct(e) {
                const t = y.A.createModel(e.name)
                    , i = m.A.createModel({
                        type: "stone",
                        amount: e.stones || 0
                    })
                    , s = m.A.createModel({
                        type: "point",
                        amount: e.points || 0
                    });
                return new p(e.itemProductKey || "", t, e.itemType || "", e.itemFileUrl || "", i, s, e.baseItemCount || 0, e.availableDays || 0)
            }
            static _createModelByISocketItem(e) {
                const t = y.A.createModel(e.name)
                    , i = m.A.createModel({
                        type: "stone",
                        amount: e.stones
                    })
                    , s = m.A.createModel({
                        type: "point",
                        amount: e.points
                    });
                return new p(e.item_product_key, t, e.item_type, e.item_file_url, i, s, e.base_item_count || 0, e.available_days || 0)
            }
            static _isIItemProduct(e) {
                return "string" == typeof e.itemProductKey
            }
        }
        var v = i(10310)
            , S = i(88316)
            , b = i(72226);
        class f {
            static createModel(e) {
                return f._isIChat(e) ? f._createModelByIChat(e) : f._createModelByISocketChat(e)
            }
            static _isIChat(e) {
                return "number" == typeof e.id
            }
            static _createModelByIChat(e) {
                if (!e.user)
                    throw new Error("Can not be found user.");
                const t = S.A.createModel(e.user)
                    , i = e.stamp ? v.A.createModel(e.stamp) : null
                    , s = e.yell ? b.A.createModel(e.yell) : null
                    , o = e.itemProduct ? g.createModel(e.itemProduct) : null;
                return f._createModel(e.id || 0, t, e.message || "", i, s, o)
            }
            static _createModelByISocketChat(e) {
                const t = S.A.createModel(e)
                    , i = e.stamp ? v.A.createModel(e.stamp) : null
                    , s = e.yell ? b.A.createModel(e.yell) : null
                    , o = e.item_data ? g.createModel(e.item_data) : null;
                return f._createModel(Number(e.chat_id) || 0, t, e.message || "", i, s, o)
            }
            static _createModel(e, t, i, s, o, r) {
                return f._isStampChat(s) ? new u(e, t, s) : f._isYellChat(o) ? new c(e, t, i, o) : f._isItemChat(r) ? new d(e, t, i, r) : new h(e, t, i)
            }
            static _isStampChat(e) {
                return !!e
            }
            static _isYellChat(e) {
                return !!e
            }
            static _isItemChat(e) {
                return !!e
            }
        }
        var C = i(35206)
            , I = i(71120)
            , A = i(3134)
            , _ = (e, t, i) => new Promise((s, o) => {
                var r = e => {
                    try {
                        n(i.next(e))
                    } catch (e) {
                        o(e)
                    }
                }
                    , a = e => {
                        try {
                            n(i.throw(e))
                        } catch (e) {
                            o(e)
                        }
                    }
                    , n = e => e.done ? s(e.value) : Promise.resolve(e.value).then(r, a);
                n((i = i.apply(e, t)).next())
            }
            );
        const P = new class {
            connectViewersSocket() {
                return _(this, arguments, function* (...[e, t]) {
                    const i = yield C.A.generateViewersSocket(e, t);
                    return i.connect(),
                        i
                })
            }
            listToCreatedAt(e, t) {
                return _(this, null, function* () {
                    return (yield C.A.listTo(e, t)).map(f.createModel)
                })
            }
            listToChat(e, t) {
                return _(this, null, function* () {
                    return (yield C.A.listTo(e, t)).map(f.createModel)
                })
            }
            verifyPermission(e, t, i) {
                try {
                    return this.verifyPermissionSend(i.user),
                        !0
                } catch (i) {
                    return i instanceof a.A && this.execSendErrorAction(i, e, t),
                        !1
                }
            }
            verifyPermissionSend(e) {
                if (!e.isLogined)
                    throw new a.A(a.A.Type.RequireLoggedIn);
                if (!e.userKey)
                    throw new a.A(a.A.Type.RequireUserKey);
                if (!e.isRegisteredMailAddress && !e.isApproved)
                    throw new a.A(a.A.Type.RequireMailAddressConfirmation)
            }
            execSendErrorAction(e, t, i) {
                e.typeIs(a.A.Type.RequireLoggedIn) ? r.A.show(I.A.bifurcateSignupAndSigninDialogId) : e.typeIs(a.A.Type.RequireUserKey) ? o.A.addMessage({
                    variant: "warning",
                    text: t.dictionary.common.userIdIsRequired,
                    unlimited: !0,
                    buttonLabel: t.dictionary.common.toUserInformationSettings,
                    onButtonClick() {
                        A.A.to(i, "/profile/profile_user_information")
                    }
                }) : e.typeIs(a.A.Type.RequireMailAddressConfirmation) && o.A.addMessage({
                    variant: "warning",
                    text: t.dictionary.common.emailAddressConfirmationIsRequired,
                    unlimited: !0,
                    buttonLabel: t.dictionary.common.toUserInformationSettings,
                    onButtonClick() {
                        A.A.to(i, "/profile/profile_user_information")
                    }
                })
            }
            setChatPositionWithFullscreenMode(e) {
                C.A.setChatPositionWithFullscreenMode(e)
            }
            getChatPositionWithFullscreenMode() {
                return C.A.getChatPositionWithFullscreenMode()
            }
        }
            , M = () => (0,
                s.useMemo)(() => w({}), [])
            , w = ({ }) => ({
                verifyPermission(e, t, i) {
                    return _(this, null, function* () {
                        try {
                            return (e => {
                                if (!e.isLogined)
                                    throw new a.A(a.A.Type.RequireLoggedIn);
                                if (!e.userKey)
                                    throw new a.A(a.A.Type.RequireUserKey);
                                if (!e.isRegisteredMailAddress && !e.isApproved)
                                    throw new a.A(a.A.Type.RequireMailAddressConfirmation)
                            }
                            )(i.user),
                                !0
                        } catch (i) {
                            return i instanceof a.A && (n = e,
                                l = t,
                                (s = i).typeIs(a.A.Type.RequireLoggedIn) ? r.A.show(I.A.bifurcateSignupAndSigninDialogId) : s.typeIs(a.A.Type.RequireUserKey) ? o.A.addMessage({
                                    variant: "warning",
                                    text: n.dictionary.common.userIdIsRequired,
                                    unlimited: !0,
                                    buttonLabel: n.dictionary.common.toUserInformationSettings,
                                    onButtonClick() {
                                        A.A.to(l, "/profile/profile_user_information")
                                    }
                                }) : s.typeIs(a.A.Type.RequireMailAddressConfirmation) && o.A.addMessage({
                                    variant: "warning",
                                    text: n.dictionary.common.emailAddressConfirmationIsRequired,
                                    unlimited: !0,
                                    buttonLabel: n.dictionary.common.toUserInformationSettings,
                                    onButtonClick() {
                                        A.A.to(l, "/profile/profile_user_information")
                                    }
                                })),
                                !1
                        }
                        var s, n, l
                    })
                }
            })
    }
    ,
    77581: (e, t, i) => {
        "use strict";
        i.d(t, {
            v1: () => l,
            At: () => d,
            Bt: () => n
        });
        var s = i(4105)
            , o = i(37437)
            , r = (e, t, i) => new Promise((s, o) => {
                var r = e => {
                    try {
                        n(i.next(e))
                    } catch (e) {
                        o(e)
                    }
                }
                    , a = e => {
                        try {
                            n(i.throw(e))
                        } catch (e) {
                            o(e)
                        }
                    }
                    , n = e => e.done ? s(e.value) : Promise.resolve(e.value).then(r, a);
                n((i = i.apply(e, t)).next())
            }
            )
            , a = (e, t, i) => new Promise((s, o) => {
                var r = e => {
                    try {
                        n(i.next(e))
                    } catch (e) {
                        o(e)
                    }
                }
                    , a = e => {
                        try {
                            n(i.throw(e))
                        } catch (e) {
                            o(e)
                        }
                    }
                    , n = e => e.done ? s(e.value) : Promise.resolve(e.value).then(r, a);
                n((i = i.apply(e, t)).next())
            }
            );
        function n(e, t, i, n) {
            return a(this, null, function* () {
                yield function (e, t, i, a) {
                    return r(this, null, function* () {
                        const { linkUrl: r, messagedAt: n } = a
                            , l = yield (0,
                                s.f5)(e, {
                                    message: t,
                                    qualityType: i,
                                    linkUrl: r,
                                    messagedAt: n
                                });
                        (0,
                            o.s)(l)
                    })
                }(e, t, i, n)
            })
        }
        function l(e, t, i) {
            return a(this, null, function* () {
                yield function (e, t, i) {
                    return r(this, null, function* () {
                        const r = yield (0,
                            s.cR)(e, t, i);
                        (0,
                            o.s)(r)
                    })
                }(e, t, i)
            })
        }
        function d(e) {
            return a(this, null, function* () {
                yield function (e) {
                    return r(this, null, function* () {
                        const t = yield (0,
                            s.oT)(e, {
                                isHidden: !0
                            });
                        (0,
                            o.s)(t)
                    })
                }(e)
            })
        }
    }
    ,
    65516: (e, t, i) => {
        "use strict";
        i.d(t, {
            A: () => l
        });
        var s = i(96735)
            , o = i(25595)
            , r = i(67406)
            , a = i(86254);
        const n = {
            openUserSubscription(e, t = {}) {
                const i = n.createWindowsFeatures({
                    width: 800,
                    height: (0,
                        o.A)().outerHeight,
                    menubar: "no",
                    toolbar: "no",
                    scrollbars: "yes"
                }, t);
                return (0,
                    o.A)().open(`${(0,
                        r.A)().urls.tvDomain}/subscription/user/${e}`, "_blank", i)
            },
            openPpv(e, t, i = {}) {
                const s = n.createWindowsFeatures({
                    width: 800,
                    height: (0,
                        o.A)().outerHeight,
                    menubar: "no",
                    toolbar: "no",
                    scrollbars: "yes"
                }, i)
                    , a = t ? `?movie_id=${t}` : "";
                return (0,
                    o.A)().open(`${(0,
                        r.A)().urls.tvDomain}/ppv/${e}${a}`, "_blank", s)
            },
            openPurchase(e = {}) {
                const t = n.createWindowsFeatures({
                    width: 1e3,
                    height: 1200,
                    menubar: "no",
                    toolbar: "no",
                    scrollbars: "yes"
                }, e);
                return (0,
                    o.A)().open(`${(0,
                        r.A)().urls.tvDomain}/point`, "_blank", t)
            },
            openPollEdit(e, t = {}) {
                const i = (0,
                    s.Ay)().getPollPopoutSize()
                    , a = ((0,
                        o.A)().screen.height - i.height) / 2
                    , l = ((0,
                        o.A)().screen.width - i.width) / 2
                    , d = n.createWindowsFeatures({
                        top: a,
                        left: l,
                        width: i.width,
                        height: i.height,
                        menubar: "no",
                        toolbar: "no",
                        scrollbars: "yes"
                    }, t);
                return (0,
                    o.A)().open(`${(0,
                        r.A)().urls.tvDomain}/live/${e}/poll/edit`, "_blank", d)
            },
            onclose(e, t) {
                if (!e)
                    return () => { }
                        ;
                let i = (0,
                    o.A)().setInterval(() => {
                        if (e.closed)
                            return t(),
                                (0,
                                    o.A)().clearInterval(i),
                                void (i = void 0)
                    }
                        , 1e3);
                return () => {
                    (0,
                        o.A)().clearInterval(i),
                        i = void 0
                }
            },
            createWindowsFeatures(e, t = {}) {
                const i = (0,
                    a.extendDeepWith)(e, t);
                let s = "";
                return Object.keys(i).forEach(e => {
                    s += `${e}=${i[e]},`
                }
                ),
                    s && s.slice(0, -1)
            }
        }
            , l = () => n
    }
    ,
    3910: (e, t, i) => {
        "use strict";
        i.d(t, {
            A: () => y
        });
        var s = i(96540)
            , o = i(92568)
            , r = i(35225)
            , a = i(61889)
            , n = i(29463)
            , l = i(83279)
            , d = Object.defineProperty
            , h = Object.getOwnPropertySymbols
            , u = Object.prototype.hasOwnProperty
            , c = Object.prototype.propertyIsEnumerable
            , p = (e, t, i) => t in e ? d(e, t, {
                enumerable: !0,
                configurable: !0,
                writable: !0,
                value: i
            }) : e[t] = i
            , m = (e, t) => {
                for (var i in t || (t = {}))
                    u.call(t, i) && p(e, i, t[i]);
                if (h)
                    for (var i of h(t))
                        c.call(t, i) && p(e, i, t[i]);
                return e
            }
            ;
        const y = e => {
            const t = {
                backgroundColor: e.backgroundColor,
                color: e.color,
                borderRadius: e.borderRadius
            };
            return s.createElement(b, {
                style: t,
                theme: n.vt(e),
                className: e.className
            }, !!e.prefix && s.createElement(r.A, m({}, e.prefix)), !!e.text && e.text, !!e.suffix && s.createElement(r.A, m({}, e.suffix)))
        }
            , g = n.Ej({
                xSmall: a.SG.XXXXS,
                small: a.SG.XXXS,
                medium: a.SG.XS,
                large: a.SG.M
            })
            , v = n.Ej({
                xSmall: "1.4rem",
                small: "1.6rem",
                medium: "1.8rem",
                large: "2rem"
            })
            , S = n.Ej({
                xSmall: "0 0.3rem",
                small: "0 0.4rem",
                medium: "0 0.5rem",
                large: "0 0.6rem"
            })
            , b = o.Ay.span.withConfig({
                componentId: "sc-4g9g4b-0"
            })(["align-items:center;display:inline-flex;white-space:nowrap;font-weight:", ";font-size:", "rem;height:", ";padding:", ";", ""], a.Y.MEDIUM, g, v, S, e => (0,
                o.AH)(["background-color:", ";color:", ";"], (0,
                    l.getTheme)(`atoms-textLabel-background_${e.theme.variant}`), (0,
                        l.getTheme)(`atoms-textLabel_${e.theme.variant}`)))
    }
    ,
    64349: (e, t, i) => {
        "use strict";
        i.d(t, {
            A: () => h
        });
        var s = i(31370)
            , o = i(96540)
            , r = i(92568)
            , a = i(61889)
            , n = i(29463)
            , l = i(83279)
            , d = i(11456);
        Object.defineProperty,
            Object.getOwnPropertyDescriptor;
        let h = class extends o.Component {
            render() {
                const e = this.props.langStore
                    , t = n.vt(this.props)
                    , i = [].concat(this.props.className || []);
                return o.createElement(m, {
                    theme: t,
                    className: i.join(" ")
                }, o.createElement(y, {
                    theme: t
                }, d.localeString(this.props.quantity), o.createElement(g, {
                    theme: t
                }, e.yell.yell)))
            }
        }
            ;
        h = ((e, t) => {
            for (var i, s = t, o = e.length - 1; o >= 0; o--)
                (i = e[o]) && (s = i(s) || s);
            return s
        }
        )([(0,
            s.WQ)("langStore"), s.PA], h);
        const u = n.Ej({
            xsmall: a.SG.XXXS,
            small: a.SG.XS,
            medium: a.SG.M,
            large: a.SG.L,
            xlarge: a.SG.XXXL
        })
            , c = n.Ej({
                xsmall: 1.6,
                small: 1.6,
                medium: 2.4,
                large: 2.4,
                xlarge: 3.2
            })
            , p = n.Ej({
                xsmall: "0 0.4rem",
                small: "0 0.4rem",
                medium: "0 0.6rem",
                large: "0 0.6rem",
                xlarge: "0 0.6rem"
            })
            , m = r.Ay.span.withConfig({
                componentId: "sc-z3zz27-0"
            })(["display:inline-flex;flex-flow:column;justify-content:center;height:", "rem;padding:", ";color:", ";background-color:", ";"], c, p, (0,
                l.getTheme)("text-base"), (0,
                    l.getTheme)("alpha-2"))
            , y = r.Ay.span.withConfig({
                componentId: "sc-z3zz27-1"
            })(["align-items:flex-end;display:flex;font-weight:", ";font-size:", "rem;"], a.Y.MEDIUM, u)
            , g = r.Ay.span.withConfig({
                componentId: "sc-z3zz27-2"
            })(["margin-left:0.2rem;font-size:", "rem;"], u)
    }
    ,
    90660: (e, t, i) => {
        "use strict";
        i.d(t, {
            A: () => u
        });
        var s = i(5205)
            , o = i(96540)
            , r = i(40961)
            , a = i(92568)
            , n = i(25595)
            , l = Object.defineProperty
            , d = Object.getOwnPropertyDescriptor
            , h = (e, t, i, s) => {
                for (var o, r = s > 1 ? void 0 : s ? d(t, i) : t, a = e.length - 1; a >= 0; a--)
                    (o = e[a]) && (r = (s ? o(t, i, r) : o(r)) || r);
                return s && r && l(t, i, r),
                    r
            }
            ;
        class u extends o.Component {
            constructor(e) {
                super(e),
                    this._observer = null,
                    this._propSrc = e.src,
                    this.state = {
                        src: "",
                        isLoaded: !1
                    }
            }
            componentDidMount() {
                this._observe()
            }
            UNSAFE_componentWillReceiveProps(e) {
                this._disconnect(),
                    this._propSrc = e.src,
                    this._observe()
            }
            componentWillUnmount() {
                this._disconnect()
            }
            render() {
                const e = [];
                return this.props.className && e.push(this.props.className),
                    this.state.isLoaded || e.push("is-hidden"),
                    o.createElement(c, {
                        src: this.state.src,
                        className: e.join(" "),
                        alt: this.props.alt || "No images",
                        onLoad: this._handleLoad
                    })
            }
            _disconnect() {
                this._observer && (this._observer.disconnect(),
                    delete this._observer)
            }
            _handleLoad() {
                this.setState({
                    isLoaded: !0
                })
            }
            _handleIntersect(e) {
                e.forEach(e => {
                    e.isIntersecting && (this._observer.unobserve(e.target),
                        this.setState({
                            src: this._propSrc
                        }))
                }
                )
            }
            _observe() {
                const e = (0,
                    n.A)().IntersectionObserver;
                this._observer = new e(this._handleIntersect),
                    this._observer.observe(r.findDOMNode(this), {
                        rootMargin: "200px 0px"
                    })
            }
        }
        h([s.A], u.prototype, "_disconnect", 1),
            h([s.A], u.prototype, "_handleLoad", 1),
            h([s.A], u.prototype, "_handleIntersect", 1),
            h([s.A], u.prototype, "_observe", 1);
        const c = a.Ay.img.withConfig({
            componentId: "sc-j397m9-0"
        })(["object-fit:contain;height:100%;width:100%;pointer-events:none;"])
    }
    ,
    87325: (e, t, i) => {
        "use strict";
        i.d(t, {
            A: () => c
        });
        var s = i(5205)
            , o = i(31370)
            , r = i(96540)
            , a = i(92568)
            , n = i(25595)
            , l = i(91460)
            , d = Object.defineProperty
            , h = Object.getOwnPropertyDescriptor
            , u = (e, t, i, s) => {
                for (var o, r = s > 1 ? void 0 : s ? h(t, i) : t, a = e.length - 1; a >= 0; a--)
                    (o = e[a]) && (r = (s ? o(t, i, r) : o(r)) || r);
                return s && r && d(t, i, r),
                    r
            }
            ;
        let c = class extends r.PureComponent {
            get _enabled() {
                const e = this.props.appStore;
                return !!this.props.pc && !e.isMobile || !!this.props.mobile && e.isMobile || !this.props.pc && !this.props.mobile
            }
            render() {
                return this._enabled ? r.createElement(p, {
                    className: this.props.className,
                    role: "button",
                    href: this.props.to,
                    onClick: this._handleClick
                }, this.props.children) : r.createElement(r.Fragment, null, this.props.children)
            }
            _handleClick(e) {
                e.metaKey || e.altKey || e.ctrlKey || e.shiftKey || (e.preventDefault(),
                    "_blank" !== this.props.target ? l.Oo(this.props.to) ? this.props.appStore.toPage(this.props.to) : location.assign(this.props.to) : (0,
                        n.A)().open(this.props.to, this.props.target))
            }
        }
            ;
        u([s.A], c.prototype, "_handleClick", 1),
            c = u([(0,
                o.WQ)("appStore")], c);
        const p = a.Ay.a.withConfig({
            componentId: "sc-1z03q9k-0"
        })(["color:inherit;cursor:pointer;"])
    }
    ,
    38201: (e, t, i) => {
        "use strict";
        i.d(t, {
            Ay: () => v,
            S8: () => S
        });
        var s = i(31370)
            , o = i(96540)
            , r = i(92568)
            , a = i(35225)
            , n = i(3910)
            , l = i(90660)
            , d = i(87325)
            , h = i(74070)
            , u = i(69083)
            , c = i(17306)
            , p = i(61889)
            , m = i(97401)
            , y = i(83279)
            , g = i(11456);
        Object.defineProperty,
            Object.getOwnPropertyDescriptor;
        let v = class extends o.Component {
            constructor() {
                super(...arguments),
                    this._renderLabel = (e = -1) => {
                        const t = this.props.langStore;
                        if (!t.completedSync)
                            return null;
                        if (m.comingup(e) || m.liveStreaming(e)) {
                            const i = m.comingup(e) ? t.common.comingUp : ""
                                , s = m.comingup(e) ? "primary" : "secondary"
                                , r = m.comingup(e) ? null : {
                                    icon: a.P.live,
                                    width: 2.6
                                };
                            return o.createElement(n.A, {
                                size: "large",
                                text: i,
                                variant: s,
                                prefix: r
                            })
                        }
                        return null
                    }
            }
            render() {
                this.props.langStore.completedSync;
                const e = ["text-hover"];
                return this.props.isResponsive && e.push("width100", "height100"),
                    o.createElement(b, null, o.createElement(C, null, o.createElement(h.A, {
                        href: `${c.A.USER}/${this.props.userId}`,
                        userName: this.props.userName,
                        userIconImageUrl: this.props.userIconImageUrl,
                        size: "small",
                        isOfficial: !!this.props.isOfficial
                    })), o.createElement(d.A, {
                        to: `${this.props.isUploadedMovie ? c.A.MOVIE : c.A.LIVE}/${this.props.movieId}`
                    }, o.createElement(f, null, o.createElement(I, {
                        className: e.join(" ")
                    }, o.createElement(_, null, this._renderMemberLabel(), this._renderLabel(this.props.onairStatus)), this._renderTrialLabel(), this._renderMobileLabel(), !!this.props.playTimeSec && o.createElement(P, null, g.degitalTime(this.props.playTimeSec)), o.createElement(l.A, {
                        src: this.props.thumbnailUrl
                    })), o.createElement(A, {
                        className: "text-hover",
                        title: this.props.title
                    }, this.props.title))), o.createElement(T, null, this.props.gameId && this.props.gameTitle ? o.createElement(d.A, {
                        className: "text-hover",
                        to: `${c.A.GAME}/${this.props.gameId}`
                    }, this.props.gameTitle) : "　"), o.createElement(E, null, this._renderViewers(), o.createElement(U, null, this._getDateString())))
            }
            _renderMemberLabel() {
                var e;
                const t = this.props.langStore;
                if (this.props.publicType === u.Oq.all)
                    return null;
                const i = null == (e = this.props.permissions) ? void 0 : e.find(e => !!e.ppvTicketId);
                let s = "";
                s = this.props.publicType === u.Oq.premium ? "SP" : i ? "PPV" : t.subscription.memberOnly;
                const r = i ? "ppv" : "premium";
                return o.createElement(n.A, {
                    size: "large",
                    text: s,
                    backgroundColor: (0,
                        y.getTheme)(r)
                })
            }
            _renderMobileLabel() {
                const e = this.props.langStore;
                return this.props.isMobile ? this.props.publicType === u.Oq.memberTrial ? null : o.createElement(M, null, e.video.mobileLabel) : null
            }
            _renderTrialLabel() {
                var e;
                const t = this.props.langStore;
                if (this.props.publicType !== u.Oq.memberTrial)
                    return null;
                if (!m.liveStreaming(this.props.onairStatus))
                    return null;
                const i = null == (e = this.props.permissions) ? void 0 : e.find(e => !!e.ppvTicketId);
                return o.createElement(w, {
                    $isPpv: !!i
                }, t.subscription.freeTrialOnlyNow)
            }
            _renderViewers() {
                const e = this.props.langStore;
                return this.props.isViewersHidden ? null : m.liveStreaming(this.props.onairStatus) ? o.createElement(U, null, o.createElement(L, null, g.localeString(this.props.liveViews), e.common.liveViews)) : o.createElement(U, null, e.common.viewCount, " ", g.localeString(this.props.totalViews))
            }
            _getDateString() {
                const e = this.props.langStore;
                return this._isComingup() ? `${g.formatDate(this.props.willStartAt || "", "YYYY/M/D")}` : this.props.startedAt ? `${g.elapsedTime(e, this.props.startedAt, {
                    dateFormatThreshold: 7
                })}` : `${g.formatDate(this.props.createdAt || "", "YYYY/M/D")}`
            }
            _isComingup() {
                return m.num(this.props.onairStatus) && m.comingup(this.props.onairStatus)
            }
        }
            ;
        v = ((e, t) => {
            for (var i, s = t, o = e.length - 1; o >= 0; o--)
                (i = e[o]) && (s = i(s) || s);
            return s
        }
        )([(0,
            s.WQ)("langStore"), s.PA], v);
        const S = e => {
            var t, i, s, o;
            return {
                gameId: e.game && e.game.id || "",
                gameTitle: e.game && e.game.title || "",
                movieId: e.id || "",
                thumbnailUrl: e.sThumbnailUrl || c.A.IMAGES.MOVIE,
                title: e.title || "",
                userId: (null == (t = e.channel) ? void 0 : t.id) || "",
                userName: (null == (i = e.channel) ? void 0 : i.nickname) || "",
                userIconImageUrl: (null == (s = e.channel) ? void 0 : s.iconImageUrl) || "",
                createdAt: e.createdAt || "",
                startedAt: e.startedAt || "",
                willStartAt: e.willStartAt || "",
                isOfficial: !!(null == (o = e.channel) ? void 0 : o.isOfficial),
                playTimeSec: e.playTime || 0,
                totalViews: e.totalViews || 0,
                liveViews: e.liveViews || 0,
                onairStatus: void 0 === e.onairStatus ? null : e.onairStatus,
                isMobile: !!e.isMobile,
                isUploadedMovie: !e.isLive,
                publicType: e.publicType || u.GJ.all,
                isViewersHidden: !!e.isViewersHidden,
                permissions: e.permissions ? g.changeCaseKey("camel", e.permissions) : void 0
            }
        }
            , b = r.Ay.div.withConfig({
                componentId: "sc-1d35bq3-0"
            })([""])
            , f = r.Ay.div.withConfig({
                componentId: "sc-1d35bq3-1"
            })(["margin-top:", "rem;"], p.jS.XXS)
            , C = r.Ay.div.withConfig({
                componentId: "sc-1d35bq3-2"
            })(["padding-right:", "rem;"], p.jS.XXS)
            , I = r.Ay.div.withConfig({
                componentId: "sc-1d35bq3-3"
            })(["position:relative;height:11.8rem;width:21rem;"])
            , A = r.Ay.p.withConfig({
                componentId: "sc-1d35bq3-4"
            })(["overflow:hidden;word-break:break-all;opacity:1;font-size:", "rem;font-weight:", ";line-height:", "rem;margin:", "rem 0 0;padding-right:", "rem;height:", "rem;color:", ";"], p.SG.M, p.Y.MEDIUM, 1.8, p.jS.XXS, p.jS.S, 1.8 * p.SG.M * 1.4, (0,
                y.getTheme)("text-strong"))
            , _ = r.Ay.div.withConfig({
                componentId: "sc-1d35bq3-5"
            })(["position:absolute;top:0.4rem;left:0.4rem;> *:not(:last-child){margin-right:0.4rem;}"])
            , P = r.Ay.span.withConfig({
                componentId: "sc-1d35bq3-6"
            })(["align-items:center;display:inline-flex;font-weight:", ";background-color:", ";position:absolute;bottom:0.4rem;right:0.4rem;height:1.6rem;padding:0 0.4rem;border-radius:2px;"], p.Y.MEDIUM, g.rgba(p.lm.BLACK, p.EQ.HIGH))
            , M = (0,
                r.Ay)(P).withConfig({
                    componentId: "sc-1d35bq3-7"
                })(["position:absolute;bottom:0;right:0;font-size:", "rem;width:100%;padding:0 0.8rem;border-radius:0;"], p.SG.XXS)
            , w = (0,
                r.Ay)(M).withConfig({
                    componentId: "sc-1d35bq3-8"
                })(["background:", ";color:", ";justify-content:center;"], ({ $isPpv: e }) => e ? (0,
                    y.getTheme)("ppv") : (0,
                        y.getTheme)("premium"), (0,
                            y.getColor)("white"))
            , T = r.Ay.p.withConfig({
                componentId: "sc-1d35bq3-9"
            })(["overflow:hidden;text-overflow:ellipsis;white-space:nowrap;padding-right:", "rem;margin:", "rem 0 0;color:", ";"], .6, p.jS.XS, (0,
                y.getTheme)("text-base"))
            , E = r.Ay.ul.withConfig({
                componentId: "sc-1d35bq3-10"
            })(["display:flex;align-items:center;list-style-type:none;padding:0;margin:", "rem 0 0;"], p.jS.XXXS)
            , U = r.Ay.li.withConfig({
                componentId: "sc-1d35bq3-11"
            })(["color:", ";&:not(:last-of-type)::after{content:'・';}"], (0,
                y.getTheme)("text-base"))
            , L = r.Ay.strong.withConfig({
                componentId: "sc-1d35bq3-12"
            })(["color:", ";font-weight:", ";"], (0,
                y.getTheme)("live"), p.Y.MEDIUM)
    }
    ,
    7750: (e, t, i) => {
        "use strict";
        i.d(t, {
            A: () => l
        });
        var s = i(96540)
            , o = i(92568)
            , r = i(35225)
            , a = i(17306)
            , n = i(29463);
        const l = e => {
            const t = n.vt(e)
                , i = {
                    backgroundImage: `url(${e.src || a.A.IMAGES.PROFILE})`
                };
            return s.createElement(p, {
                theme: t,
                style: i
            }, !!e.isLiveStreaming && s.createElement(c, {
                theme: t
            }, s.createElement(r.A, {
                icon: r.P.onair
            })))
        }
            , d = n.Ej({
                xxsmall: 2.2,
                xsmall: 2.4,
                small: 3.2,
                medium: 4,
                large: 4.8,
                xlarge: 6.4
            })
            , h = n.Ej({
                xxsmall: 0,
                xsmall: 0,
                small: 0,
                medium: 0,
                large: .2,
                xlarge: .4
            })
            , u = n.Ej({
                xxsmall: 0,
                xsmall: 0,
                small: 0,
                medium: 0,
                large: .2,
                xlarge: .4
            })
            , c = o.Ay.div.withConfig({
                componentId: "sc-h48kgw-0"
            })(["position:absolute;top:", "rem;left:", "rem;"], h, u)
            , p = o.Ay.div.withConfig({
                componentId: "sc-h48kgw-1"
            })(["position:relative;background-position:center;background-repeat:no-repeat;border-radius:50%;background-size:cover;height:", "rem;width:", "rem;"], d, d)
    }
    ,
    47268: (e, t, i) => {
        "use strict";
        i.d(t, {
            Ay: () => S,
            BB: () => b
        });
        var s = i(5205)
            , o = i(31370)
            , r = i(96540)
            , a = i(92568)
            , n = i(83564)
            , l = i(35225)
            , d = i(61889)
            , h = i(86254)
            , u = i(83279)
            , c = i(54240)
            , p = i(11456)
            , m = i(35416)
            , y = Object.defineProperty
            , g = Object.getOwnPropertyDescriptor
            , v = (e, t, i, s) => {
                for (var o, r = s > 1 ? void 0 : s ? g(t, i) : t, a = e.length - 1; a >= 0; a--)
                    (o = e[a]) && (r = (s ? o(t, i, r) : o(r)) || r);
                return s && r && y(t, i, r),
                    r
            }
            ;
        let S = class extends r.Component {
            componentWillUnmount() {
                delete this._subsBadgeEl,
                    delete this._subsTierEl,
                    delete this._membershipCardBadgeEl
            }
            render() {
                const e = this.props.appStore
                    , t = this.props.langStore
                    , i = [];
                this.props.isNewLine || i.push("text-ellipsis"),
                    this.props.onNameClick && i.push("text-hover", "cursor-pointer");
                const s = () => this.props.userColor ? this.props.isLightModeUserColor ? p.rgba(this.props.userColor, 1, e.themeMode) : p.rgba(this.props.userColor) : ""
                    , o = p.color(s())
                    , a = (o ? p.grayscale(o.r, o.g, o.b) : 0) >= .5825
                    , d = s()
                    , h = a ? (0,
                        u.getColor)("text-strong_light") : (0,
                            u.getColor)("text-strong_dark")
                    , y = () => !!this.props.subsBadge && !!this.props.subsBadge.imageUrl
                    , g = () => {
                        if (!this.props.subsBadge)
                            return null;
                        if (this.props.isSubsBadgeHidden && this.props.isSubsDurationHidden)
                            return null;
                        const e = this.props.subsBadge.tier || 0
                            , i = `${this.props.subsBadge.months || ""}`
                            , s = `${this.props.subsBadge.label || ""}`
                            , o = e ? [...Array(e)].map((e, t) => r.createElement(n.A, {
                                key: e || t,
                                icon: n.u.subsStar,
                                color: h,
                                width: 10,
                                height: 10
                            })) : null;
                        return r.createElement(_, {
                            key: "subs-badge"
                        }, !this.props.isSubsBadgeHidden && r.createElement(P, {
                            ref: e => {
                                this._subsBadgeEl = e
                            }
                            ,
                            src: this.props.subsBadge.imageUrl,
                            onMouseOver: () => {
                                this._handleMouseOver(this._subsBadgeEl, t.subscription.member)
                            }
                            ,
                            onMouseLeave: this._handleMouseLeave
                        }), !this.props.isSubsDurationHidden && r.createElement(M, {
                            ref: e => {
                                this._subsTierEl = e
                            }
                            ,
                            backgroundColor: d,
                            color: h,
                            onMouseOver: () => {
                                this._handleMouseOver(this._subsTierEl, `${s} ${"1" === i ? t.common.firstMonth : t.common.nthMonth.replace("%s", i)}`)
                            }
                            ,
                            onMouseLeave: this._handleMouseLeave
                        }, o, r.createElement(w, null, i)))
                    }
                    ;
                return r.createElement(C, {
                    onClick: this.props.onNameClick,
                    ref: this.props.innerRef,
                    style: this.props.style,
                    isNewLine: !!this.props.isNewLine
                }, r.createElement(I, {
                    theme: {
                        color: s()
                    },
                    className: i.join(" ")
                }, (0,
                    c.convertBlankStringToEmWhiteSpace)(this.props.userName || "")), (() => {
                        var e, i;
                        const s = ((null == (e = this.props.userInChannel) ? void 0 : e.membershipCardUrl) || this.props.isShowMembershipCard) && !(null == (i = this.props.chatCommentAppearanceSetting) ? void 0 : i.isSubsMembershipCardHidden) && r.createElement(T, {
                            key: "membershipCard",
                            ref: e => {
                                this._membershipCardBadgeEl = e
                            }
                            ,
                            onMouseOver: () => {
                                this._handleMouseOver(this._membershipCardBadgeEl, t.channelStarCollection.membershipCard)
                            }
                            ,
                            onMouseLeave: this._handleMouseLeave
                        }, r.createElement(m.A, {
                            id: "24/membershipCard",
                            width: "14",
                            height: "14",
                            fill: d,
                            fillSecondary: h
                        }))
                            , o = [this.props.displayOfficialIcon && this.props.isOfficial && !this.props.isOfficialHidden && r.createElement(l.A, {
                                key: "official",
                                icon: l.P.official
                            }), this.props.isPremium && !this.props.isPremiumHidden && r.createElement(l.A, {
                                key: "premium",
                                icon: l.P.premium
                            }), this.props.isModerator && r.createElement(l.A, {
                                key: "moderator",
                                icon: l.P.moderator
                            }), this.props.isWarned && r.createElement(l.A, {
                                key: "warned",
                                icon: l.P.warned
                            }), this.props.isFresh && !this.props.hideFreshIcon && r.createElement(l.A, {
                                key: "begginer",
                                icon: l.P.begginer
                            }), y() && g(), s].filter(Boolean);
                        return o.length ? r.createElement(A, null, o) : null
                    }
                    )())
            }
            _handleMouseOver(e, t) {
                e && this.props.appStore.showTooltip(e, t)
            }
            _handleMouseLeave() {
                this.props.appStore.hideTooltip()
            }
        }
            ;
        v([s.A], S.prototype, "_handleMouseOver", 1),
            v([s.A], S.prototype, "_handleMouseLeave", 1),
            S = v([(0,
                o.WQ)("appStore", "langStore"), o.PA], S);
        const b = (e, t, i, s) => ((0,
            h.extendDeepWith)(e, f(t, i, s)),
            e)
            , f = (e, t, i) => ({
                userName: (null == e ? void 0 : e.nickname) || "",
                isOfficial: !!(null == e ? void 0 : e.isOfficial),
                isOfficialHidden: !!(null == t ? void 0 : t.isOfficialHidden),
                isPremium: !!(null == e ? void 0 : e.isPremium),
                isFresh: !!(null == e ? void 0 : e.isFresh),
                isWarned: !!(null == e ? void 0 : e.isWarned),
                userColor: (null == t ? void 0 : t.nameColor) || "",
                isPremiumHidden: !!(null == t ? void 0 : t.isPremiumHidden),
                isSubsBadgeHidden: !!(null == t ? void 0 : t.isSubsBadgeHidden),
                isSubsDurationHidden: !!(null == t ? void 0 : t.isSubsDurationHidden),
                toUser: {
                    userIconImageUrl: (null == i ? void 0 : i.iconImageUrl) || ""
                }
            })
            , C = a.Ay.span.withConfig({
                componentId: "sc-1i0rd20-0"
            })(["align-items:center;display:inline-flex;font-weight:", ";word-break:break-all;max-width:100%;text-decoration:none;", ""], d.Y.MEDIUM, e => e.isNewLine && (0,
                a.AH)(["display:inline;"]))
            , I = a.Ay.span.withConfig({
                componentId: "sc-1i0rd20-1"
            })(["min-width:0;color:", ";"], e => e.theme.color ? e.theme.color : (0,
                u.getTheme)("text-strong"))
            , A = a.Ay.span.withConfig({
                componentId: "sc-1i0rd20-2"
            })(["align-items:center;flex:none;display:inline-flex;margin-left:", "rem;& >:not(:last-child){margin-right:", "rem;}"], d.jS.XXXS, d.jS.XXXS)
            , _ = a.Ay.span.withConfig({
                componentId: "sc-1i0rd20-3"
            })(["display:inline-flex;align-items:center;text-shadow:none;& > *:not(:last-child){margin-right:0.6rem;}"])
            , P = a.Ay.img.withConfig({
                componentId: "sc-1i0rd20-4"
            })(["width:1.4rem;height:1.4rem;"])
            , M = a.Ay.span.withConfig({
                componentId: "sc-1i0rd20-5"
            })(["position:relative;padding:0.2rem;height:1.4rem;display:flex;align-items:center;border-radius:0.2rem;font-size:1rem;font-weight:bold;", ' > *:not(:last-child){margin-right:0.1rem;}&:before{content:"";position:absolute;top:50%;left:-0.6rem;transform:translateY(-50%);border:0.3rem solid transparent;border-right:0.3rem solid;', "}"], e => (0,
                a.AH)(["background:", ";color:", ";"], e.backgroundColor, e.color), e => (0,
                    a.AH)(["border-right-color:", ";"], e.backgroundColor))
            , w = a.Ay.span.withConfig({
                componentId: "sc-1i0rd20-6"
            })([""])
            , T = a.Ay.div.withConfig({
                componentId: "sc-1i0rd20-7"
            })([""])
    }
    ,
    74070: (e, t, i) => {
        "use strict";
        i.d(t, {
            A: () => _
        });
        var s = i(5205)
            , o = i(31370)
            , r = i(96540)
            , a = i(92568)
            , n = i(10786)
            , l = i(27979)
            , d = i(87325)
            , h = i(7750)
            , u = i(47268)
            , c = i(61889)
            , p = i(29463)
            , m = i(83279)
            , y = i(89602)
            , g = i(71120)
            , v = Object.defineProperty
            , S = Object.getOwnPropertyDescriptor
            , b = Object.getOwnPropertySymbols
            , f = Object.prototype.hasOwnProperty
            , C = Object.prototype.propertyIsEnumerable
            , I = (e, t, i) => t in e ? v(e, t, {
                enumerable: !0,
                configurable: !0,
                writable: !0,
                value: i
            }) : e[t] = i
            , A = (e, t, i, s) => {
                for (var o, r = s > 1 ? void 0 : s ? S(t, i) : t, a = e.length - 1; a >= 0; a--)
                    (o = e[a]) && (r = (s ? o(t, i, r) : o(r)) || r);
                return s && r && v(t, i, r),
                    r
            }
            ;
        let _ = class extends r.Component {
            render() {
                const e = p.vt(this.props);
                return r.createElement(T, null, this._renderUserImage(), r.createElement(E, {
                    theme: e
                }, this._renderUserName(), this._renderFollowButton()))
            }
            _renderUserImage() {
                const e = p.vt(this.props)
                    , t = this.props.href ? "text-hover cursor-pointer" : ""
                    , i = this.props.target || "_self"
                    , s = r.createElement(L, {
                        theme: e,
                        className: t
                    }, r.createElement(h.A, {
                        src: this.props.userIconImageUrl,
                        size: this.props.size,
                        isLiveStreaming: this.props.isLiveStreaming
                    }));
                return this.props.href ? r.createElement(d.A, {
                    to: this.props.href,
                    target: i
                }, s) : s
            }
            _renderUserName() {
                const e = p.vt(this.props)
                    , t = this.props.href ? "text-hover cursor-pointer" : ""
                    , i = this.props.target || "_self"
                    , s = r.createElement(r.Fragment, null, r.createElement(U, {
                        theme: e,
                        className: t
                    }, r.createElement(u.Ay, ((e, t) => {
                        for (var i in t || (t = {}))
                            f.call(t, i) && I(e, i, t[i]);
                        if (b)
                            for (var i of b(t))
                                C.call(t, i) && I(e, i, t[i]);
                        return e
                    }
                    )({}, this.props))), this.props.description && r.createElement(k, null, " ", this.props.description, " "));
                return this.props.href ? r.createElement(d.A, {
                    to: this.props.href,
                    target: i
                }, s) : s
            }
            _renderFollowButton() {
                const e = this.props.langStore
                    , t = this.props.userStore
                    , i = this.props.moviePageStore.movieStore;
                return this.props.isVisibleFollowButton && i.isCompletedFetchMovieDetail ? this.props.userId === t.user.id ? null : this.props.isFollowing ? r.createElement(H, {
                    className: "text-hover",
                    onClick: e => this._handleFollowClick(e),
                    label: e.user.following,
                    variant: "primary",
                    size: "small"
                }) : r.createElement(O, {
                    onClick: e => this._handleFollowClick(e),
                    label: `${e.user.follow}`,
                    variant: "primary",
                    size: "small"
                }) : null
            }
            _handleFollowClick(e) {
                return t = this,
                    i = function* () {
                        const t = this.props.moviePageStore.movieStore
                            , i = this.props.userStore;
                        if (e.stopPropagation(),
                            this.props.userId)
                            if (i.isLogined)
                                try {
                                    this.props.isFollowing ? yield i.unfollow(this.props.userId) : yield i.follow(this.props.userId),
                                        t.updateCastFollow(this.props.userId, !this.props.isFollowing)
                                } catch (e) { }
                            else
                                y.A.show(g.A.bifurcateSignupAndSigninDialogId)
                    }
                    ,
                    new Promise((e, s) => {
                        var o = e => {
                            try {
                                a(i.next(e))
                            } catch (e) {
                                s(e)
                            }
                        }
                            , r = e => {
                                try {
                                    a(i.throw(e))
                                } catch (e) {
                                    s(e)
                                }
                            }
                            , a = t => t.done ? e(t.value) : Promise.resolve(t.value).then(o, r);
                        a((i = i.apply(t, null)).next())
                    }
                    );
                var t, i
            }
        }
            ;
        A([s.A], _.prototype, "_handleFollowClick", 1),
            _ = A([(0,
                o.WQ)("langStore", "moviePageStore", "userStore"), o.PA], _);
        const P = p.Ej({
            small: c.jS.XXS,
            medium: c.jS.XXS,
            large: c.jS.XS
        })
            , M = p.Ej({
                small: c.SG.S,
                medium: c.SG.M,
                large: c.SG.L
            })
            , w = p.Ej({
                small: c.SG.XXS,
                medium: c.SG.XXS,
                large: c.SG.XS
            })
            , T = a.Ay.div.withConfig({
                componentId: "sc-f45l3i-0"
            })(["display:flex;align-items:center;opacity:1;cursor:", ";"], e => e.onClick && "pointer")
            , E = a.Ay.div.withConfig({
                componentId: "sc-f45l3i-1"
            })(["margin-left:", "rem;min-width:0;"], P)
            , U = a.Ay.div.withConfig({
                componentId: "sc-f45l3i-2"
            })(["font-size:", "rem;"], M)
            , L = a.Ay.div.withConfig({
                componentId: "sc-f45l3i-3"
            })(["align-self:flex-start;flex:none;"])
            , k = a.Ay.div.withConfig({
                componentId: "sc-f45l3i-4"
            })(["font-size:", "rem;margin-top:", "rem;color:", ";"], w, c.jS.XXXS, (0,
                m.getTheme)("text-base"))
            , H = (0,
                a.Ay)(l.A).withConfig({
                    componentId: "sc-f45l3i-5"
                })(["border:0.1rem solid ", ";margin-top:0.8rem;min-width:7.8rem;"], (0,
                    m.getTheme)("frame"))
            , O = (0,
                a.Ay)(n.A).withConfig({
                    componentId: "sc-f45l3i-6"
                })(["margin-top:0.8rem;min-width:7.8rem;"])
    }
    ,
    52401: (e, t, i) => {
        "use strict";
        i.d(t, {
            A: () => ds
        });
        var s = i(5205)
            , o = i(27813)
            , r = i(59324)
            , a = i(54011)
            , n = i(38201)
            , l = i(80767)
            , d = Object.defineProperty
            , h = Object.getOwnPropertyDescriptor
            , u = (e, t, i, s) => {
                for (var o, r = s > 1 ? void 0 : s ? h(t, i) : t, a = e.length - 1; a >= 0; a--)
                    (o = e[a]) && (r = (s ? o(t, i, r) : o(r)) || r);
                return s && r && d(t, i, r),
                    r
            }
            ;
        class c {
            constructor() {
                this.stores = null,
                    (0,
                        o.Gn)(this)
            }
            willLoad(e) {
                return t = this,
                    i = function* () {
                        this.stores = e
                    }
                    ,
                    new Promise((e, s) => {
                        var o = e => {
                            try {
                                a(i.next(e))
                            } catch (e) {
                                s(e)
                            }
                        }
                            , r = e => {
                                try {
                                    a(i.throw(e))
                                } catch (e) {
                                    s(e)
                                }
                            }
                            , a = t => t.done ? e(t.value) : Promise.resolve(t.value).then(o, r);
                        a((i = i.apply(t, null)).next())
                    }
                    );
                var t, i
            }
            willUnload() {
                this.stores = null
            }
            get adjustLink() {
                return this.stores ? (0,
                    l.y)("OPENREC_SP_Web", "movie", this._deeplinkParams, this._adjustParams) : ""
            }
            get adjustStoreLink() {
                return this.stores ? (0,
                    l.y)("OPENREC_SP_Web", "movie", this._deeplinkParams, this._adjustParams, {
                        isDeeplinkExcluded: !0
                    }) : ""
            }
            get _deeplinkParams() {
                if (!this.stores)
                    return {};
                const e = this.stores.appStore;
                return {
                    id: this.stores.moviePageStore.movieStore.movieId,
                    seekTo: parseInt(e.currentQueryInfo.t || "0", 10) || void 0,
                    secretKey: e.currentQueryInfo.secret_key || void 0
                }
            }
            get _adjustParams() {
                if (!this.stores)
                    return {};
                const e = this.stores.moviePageStore.movieStore;
                return {
                    campaign: (e.isUploadedMovie ? "movie" : e.isArchive && "archive") || e.isComingUp && "comingup" || "live",
                    adgroup: e.channel.id,
                    creative: e.id
                }
            }
        }
        u([o.sH.ref], c.prototype, "stores", 2),
            u([o.XI], c.prototype, "willLoad", 1),
            u([o.XI], c.prototype, "willUnload", 1),
            u([o.EW], c.prototype, "adjustLink", 1),
            u([o.EW], c.prototype, "adjustStoreLink", 1),
            u([o.EW], c.prototype, "_deeplinkParams", 1),
            u([o.EW], c.prototype, "_adjustParams", 1);
        var p = i(96735)
            , m = i(7342)
            , y = i(25595)
            , g = i(67406)
            , v = i(69083)
            , S = i(43462)
            , b = i(44820)
            , f = i(55449)
            , C = i(97401)
            , I = i(28493)
            , A = i(11456)
            , _ = i(50907)
            , P = i(15773)
            , M = i(84507)
            , w = i(89056)
            , T = i(50187)
            , E = i(76080)
            , U = i(89844)
            , L = i(28139)
            , k = i(23700)
            , H = i(9720)
            , O = i(43203)
            , D = i(26504)
            , x = i(36038)
            , F = i(21928)
            , R = i(18644)
            , B = i(46202)
            , X = i(88316)
            , W = i(35206)
            , N = i(14408)
            , V = i(774)
            , Y = i(8766)
            , Q = i(43011)
            , q = i(80585)
            , j = Object.defineProperty
            , z = Object.defineProperties
            , G = Object.getOwnPropertyDescriptor
            , K = Object.getOwnPropertyDescriptors
            , $ = Object.getOwnPropertySymbols
            , Z = Object.prototype.hasOwnProperty
            , J = Object.prototype.propertyIsEnumerable
            , ee = (e, t, i) => t in e ? j(e, t, {
                enumerable: !0,
                configurable: !0,
                writable: !0,
                value: i
            }) : e[t] = i
            , te = (e, t) => {
                for (var i in t || (t = {}))
                    Z.call(t, i) && ee(e, i, t[i]);
                if ($)
                    for (var i of $(t))
                        J.call(t, i) && ee(e, i, t[i]);
                return e
            }
            , ie = (e, t) => z(e, K(t))
            , se = (e, t, i, s) => {
                for (var o, r = s > 1 ? void 0 : s ? G(t, i) : t, a = e.length - 1; a >= 0; a--)
                    (o = e[a]) && (r = (s ? o(t, i, r) : o(r)) || r);
                return s && r && j(t, i, r),
                    r
            }
            , oe = (e, t, i) => new Promise((s, o) => {
                var r = e => {
                    try {
                        n(i.next(e))
                    } catch (e) {
                        o(e)
                    }
                }
                    , a = e => {
                        try {
                            n(i.throw(e))
                        } catch (e) {
                            o(e)
                        }
                    }
                    , n = e => e.done ? s(e.value) : Promise.resolve(e.value).then(r, a);
                n((i = i.apply(e, t)).next())
            }
            );
        class re {
            constructor(e) {
                this.chats = o.sH.array(),
                    this.latestChat = null,
                    this.chatQue = [],
                    this.chatlistQue = [],
                    this.archiveChatQue = [],
                    this.isReproducingChatInDvr = !1,
                    this.blacklist = [],
                    this.moderator = [],
                    this.bannedWords = [],
                    this.chatScrollEnable = !1,
                    this.isLastPage = !1,
                    this.isPopout = !1,
                    this.isTracing = !1,
                    this.isChatVisible = !1,
                    this._isChatTransitioning = !1,
                    this.fixedPhrases = [],
                    this.isFixedPhraseChatVisible = !1,
                    this.isFixedPhraseListVisible = !1,
                    this.currentYellGroupId = "",
                    this.chatLoading = !0,
                    this.archiveChatLoading = !1,
                    this.nextArchiveChatExists = !0,
                    this.forcedScrollBottom = !0,
                    this.isScrolledByAuto = !1,
                    this.forcedAutoScrollCount = 0,
                    this.enabledChatInDvr = !1,
                    this.isScrollingByAuto = !1,
                    this.recentlyDisplayedSelfChatIdList = [],
                    this.yellReactionHistory = [],
                    this._preLastChatCell = null,
                    this._preFirstChatCellId = null,
                    this._firstFetchedWhenChatListMode = !1,
                    this._systemChatId = 0,
                    this._extensionIds = [],
                    this._hideSubsAppealTimeoutId = 0,
                    (0,
                        q.H)(this, {
                            points: o.sH,
                            scrollListEl: o.sH
                        }),
                    this.chatSettingStore = e
            }
            get scrollEndByTheaterMode() {
                return this.scrollEnd + 110
            }
            setStores(e) {
                this.stores = e
            }
            setScrollTop(e) {
                this.scrollTop = e
            }
            setScrollEnd(e) {
                this.preScrollEnd = this.scrollEnd,
                    this.scrollEnd = e
            }
            syncScrollEnd() {
                if (!this.scrollListEl)
                    return;
                const e = f.getScrollInfo(this.scrollListEl);
                this.setScrollEnd(e.end)
            }
            isScrollBottom() {
                return this.scrollEnd - this.scrollTop < v.oN
            }
            isPreScrollBottom() {
                return this.preScrollEnd - this.scrollTop < v.oN
            }
            scrollToEnd(e = {}) {
                const t = e.scrollEnd || this.scrollEnd
                    , i = this.scrollListEl;
                if (i) {
                    if (this._cancelScroll && (this._cancelScroll(),
                        delete this._cancelScroll),
                        this.startScrollByAuto(),
                        !e.enabledAnimation || (0,
                            y.A)().fps < v.uE || C.ie())
                        return this.setScrollTop(t),
                            this.markAsReadLatestChatLabel(),
                            void (0,
                                y.A)().requestAnimationFrame(() => {
                                    i.scrollTop = t,
                                        this.finishScrollByAuto()
                                }
                                );
                    this._cancelScroll = S.scrollTo(i, t, v.ap, b.easeOutCubic, () => {
                        this.setScrollTop(t),
                            this.markAsReadLatestChatLabel(),
                            this.finishScrollByAuto()
                    }
                    )
                }
            }
            setScrollListEl(e) {
                this.scrollListEl = e
            }
            willLoad(e) {
                return oe(this, null, function* () {
                    this.stores = e;
                    const t = (0,
                        p.Ay)().getMoviePagePlayMode();
                    this.isChatVisible = t.isChatVisible,
                        this._setChatRuleAdd(e),
                        this._setAppealChat(e),
                        this._setFollowAppealChat(e),
                        this.fetchReactionProducts(),
                        this.getYellReactionHistory()
                })
            }
            willUnload() {
                (0,
                    y.A)().clearInterval(this._chatAddIntervalId),
                    (0,
                        y.A)().clearTimeout(this._subsAppealTimeoutId),
                    (0,
                        y.A)().clearTimeout(this._followAppealTimeoutId),
                    (0,
                        y.A)().clearInterval(this._delayChatlistQueIntervalId),
                    (0,
                        y.A)().cancelAnimationFrame(this._chatQueAnimationFrame),
                    this._chatAddIntervalId = 0,
                    this._subsAppealTimeoutId = 0,
                    this._followAppealTimeoutId = 0,
                    this._chatQueAnimationFrame = 0,
                    this.scrollListEl = null,
                    this._firstFetchedWhenChatListMode = !1,
                    this.resetChats(),
                    delete this.stores,
                    this._disposeAppealChat && (this._disposeAppealChat(),
                        this._disposeAppealChat = void 0),
                    this._disposeChatRuleAdd && (this._disposeChatRuleAdd(),
                        this._disposeChatRuleAdd = void 0),
                    this._disposeFollowAppealChat && (this._disposeFollowAppealChat(),
                        this._disposeFollowAppealChat = void 0)
            }
            sendChat(e, t) {
                return oe(this, null, function* () {
                    var i, s, o;
                    try {
                        this.stores && this.stores.moviePageStore.movieStore.isLiveStreaming && (t.messagedAt = this.stores.appStore.currentPlayerInfo.programDateTime);
                        const r = {
                            yellId: t.yellId,
                            stampId: t.stampId,
                            message: t.message,
                            qualityType: "lowLatency" === (null == (i = t.level) ? void 0 : i.type) ? 2 : 0,
                            messagedAt: t.messagedAt,
                            leagueKey: t.leagueKey || "",
                            toUserId: t.toUserId || "",
                            consentedChatTerms: !!t.consentedChatTerms
                        }
                            , a = yield (0,
                                w.M)(e, r);
                        if (a.message) {
                            if (a.status === v.bo.CHAT_USER_KEY_NOT_EXISTS) {
                                if (this.stores && this.stores.langStore.chat.chatIdRequired) {
                                    const e = String(this.stores.langStore.chat.chatIdRequired).replace(/%s/, `${(0,
                                        g.A)().urls.tvDomain}/profile/profile_user_information`);
                                    return void this._showErrorMessage(e, "warning")
                                }
                                return void this._showErrorMessage(a.message, "warning")
                            }
                            if (a.status === v.bo.COMMON)
                                return void this._showErrorMessage(a.message, "warning");
                            a.status === v.bo.CHAT_SETTING && (this._showErrorMessage(a.message, "warning"),
                                this.chatSettingStore.setMenuType("rule"),
                                this.chatSettingStore.showMenu()),
                                a.status === v.bo.CHAT_TERM_CONSENT && this._showErrorMessage(a.message, "warning"),
                                a.status === v.bo.CHAT_STAMP_NOT_EXISTS && (this._showErrorMessage(a.message, "warning"),
                                    this.stores && this.stores.userStore.user.recxuserId && t.stampId && (0,
                                        p.Ay)().deleteStampHistory(this.stores.userStore.user.recxuserId, t.stampId))
                        } else {
                            const e = null == (o = null == (s = a.data) ? void 0 : s.items) ? void 0 : o[0];
                            if (e) {
                                const t = this._toChatProps(e)
                                    , { stamp: i } = e;
                                if (this.addChatAndUpdateYellInfo(t),
                                    i && this.stores) {
                                    const e = this.stores.userStore
                                        , t = (0,
                                            p.Ay)().getStampHistories(e.user.recxuserId || 0)
                                        , s = t.findIndex(e => e.id === i.id);
                                    if (-1 !== s) {
                                        const o = [...t];
                                        o[s] = {
                                            id: i.id,
                                            imageUrl: i.imageUrl,
                                            groupId: i.groupId,
                                            stampGroup: o[s].stampGroup,
                                            subsProduct: i.subsProduct
                                        },
                                            (0,
                                                p.Ay)().updateStampHistories(e.user.recxuserId || 0, o)
                                    }
                                }
                            }
                            t.yellId && (this.fetchMyPointAndUpdate(),
                                e && e.channelStar && this.stores && x.A.addMessage({
                                    variant: "info",
                                    text: this.stores.langStore.channelStarCollection.youEarnedStarsBySending.replace("{type}", this.stores.langStore.yell.yell).replace("{star}", (e.channelStar.earnedStars || 0).toLocaleString()),
                                    iconImageUrl: e.channelStar.iconImageUrl || "",
                                    unlimited: !W.A.getIsDisplayedChannelStarAlert(),
                                    onCloseButtonClick: W.A.setIsDisplayedChannelStarAlert
                                }))
                        }
                    } catch (e) {
                        console.error(e),
                            this._showErrorMessage(e.message)
                    }
                })
            }
            sendChatAfterChatTermConsenet(e, t) {
                return oe(this, null, function* () {
                    if (!this.stores)
                        return;
                    const i = this.stores.dialogStore
                        , s = this.stores.moviePageStore.movieStore;
                    !s.isOwner && s.settings.isChatTermsRequired ? t.message ? i.showDialog({
                        type: "chatTerm",
                        onSubmit: () => {
                            this.sendChat(e, te({
                                consentedChatTerms: !0
                            }, t)),
                                i.hideDialog()
                        }
                        ,
                        chatTerm: {
                            message: t.message
                        }
                    }) : (t.consentedChatTerms = !0,
                        this.sendChat(e, te({
                            consentedChatTerms: !0
                        }, t))) : this.sendChat(e, t)
                })
            }
            deleteChat(e, t) {
                return oe(this, null, function* () {
                    try {
                        yield (0,
                            w.J)(e, t.id),
                            (0,
                                o.h5)(() => {
                                    const e = this.chats.find(e => e.id === t.id);
                                    e && this.chats.remove(e)
                                }
                                )
                    } catch (e) {
                        this._showErrorMessage("チャットの削除に失敗しました")
                    }
                })
            }
            updateChat(e, t) {
                return oe(this, null, function* () {
                    const i = this._toChatPropsFromLegacy(e);
                    i.id = t;
                    const s = this.chats.map(e => e.id === t ? i : e);
                    s.sort((e, t) => {
                        const i = I.parse(e.postedAt || 0).getTime()
                            , s = I.parse(t.postedAt || 0).getTime();
                        return i < s ? -1 : i > s ? 1 : e.id < t.id ? -1 : e.id > t.id ? 1 : 0
                    }
                    ),
                        this.chats.replace(s)
                })
            }
            fetchChatAndReset(e) {
                return oe(this, arguments, function* (e, t = {}) {
                    if (!this.stores)
                        return;
                    const i = this.stores.moviePageStore.movieStore
                        , s = new Date;
                    try {
                        yield this.fetchChatsAndUpdate(e, {
                            toCreatedAt: t.toCreatedAt || (0,
                                Q.wO)(s),
                            isIncludingSystemMessage: !0
                        }, {
                            isReset: !0,
                            c: i.settings.timestamp
                        })
                    } catch (e) { }
                })
            }
            updateModeratedChat(e, t) {
                return oe(this, null, function* () {
                    const i = this.chats.map(i => (i.userId === e && (i.isModerator = t,
                        i.userInChannel = ie(te({}, i.userInChannel), {
                            isModerating: t
                        })),
                        i));
                    this.chats.replace(i)
                })
            }
            resetChats() {
                return oe(this, null, function* () {
                    this.chats.clear(),
                        this.archiveChatQue = [],
                        this.chatQue = [],
                        this.forcedScrollBottom = !0,
                        this.nextArchiveChatExists = !0
                })
            }
            traceChats(e) {
                return oe(this, null, function* () {
                    if (this.chatLoading || this.isLastPage)
                        return;
                    const t = this.chats.length && this.chats[0].id || 0;
                    t ? (this.isTracing = !0,
                        yield this.fetchChatsAndUpdate(e, {
                            toChatId: t,
                            isIncludingSystemMessage: !0
                        })) : this.fetchChatAndReset(e)
                })
            }
            fetchChatsAndUpdate(e, t) {
                return oe(this, arguments, function* (e, t, i = {}) {
                    let s;
                    this.chatLoading = !0;
                    try {
                        s = yield (0,
                            k.r)(() => (0,
                                H.k)(e, ie(te({}, t), {
                                    chatSecretKey: this.stores && this.stores.moviePageStore.movieStore.chatSecretKey
                                })), {
                                c: i.c ? i.c : void 0
                            })
                    } catch (e) {
                        return void (0,
                            o.h5)(() => {
                                console.error(e),
                                    i.isReset && this.resetChats(),
                                    this.chatLoading = !1
                            }
                            )
                    }
                    (0,
                        o.h5)(() => {
                            if (this.chatLoading = !1,
                                i.isReset && this.resetChats(),
                                !s.length)
                                return void (this.isLastPage = !0);
                            s.length >= 40 && (this.isLastPage = !1);
                            let e = s.map(e => (this._updateExtensionIds(e),
                                this._toChatProps(e)));
                            if (t.toCreatedAt) {
                                e = e.filter(e => e.postedAt);
                                const s = new Date(t.toCreatedAt).getTime()
                                    , o = this.stores && this.stores.moviePageStore.movieStore.isLiveStreaming
                                    , r = this.stores && this.stores.appStore.currentPlayerInfo.level && "lowLatency" !== this.stores.appStore.currentPlayerInfo.level.type
                                    , a = o && r && this.chatSettingStore.delayChat
                                    , n = e.filter(e => new Date(e.postedAt).getTime() + (a && e.isLowLatency ? v._ : 0) <= s)
                                    , l = e.filter(e => !n.includes(e));
                                i.isReset ? (this.chats.replace(n),
                                    this.stores && this.stores.moviePageStore.movieStore.isArchive ? this.archiveChatQue = l : this.chatQue = l) : (this.chats.replace(n.concat(this.chats.slice())),
                                        this.stores && this.stores.moviePageStore.movieStore.isArchive ? this.archiveChatQue = this.archiveChatQue.concat(l) : this.updateChatQue(l))
                            } else
                                i.isReset ? this.chats.replace(e) : this.chats.replace(e.concat(this.chats.slice()));
                            if (this._extensionIds.length > 0 && this.stores) {
                                const e = this.stores.moviePageStore.movieStore;
                                this._extensionIds.forEach((t, i) => {
                                    e.updateDisplayExtension(t),
                                        this._extensionIds.splice(i, 1)
                                }
                                )
                            }
                        }
                        )
                })
            }
            fetchChatsAndUpdateChatlistQue(e, t) {
                return oe(this, null, function* () {
                    if (!this.stores)
                        return;
                    const i = this.stores.userStore;
                    let s;
                    try {
                        s = yield (0,
                            H.k)(e, ie(te({}, t), {
                                chatSecretKey: this.stores && this.stores.moviePageStore.movieStore.chatSecretKey
                            }))
                    } catch (e) {
                        return
                    }
                    (0,
                        o.h5)(() => {
                            this.chatlistQue = [];
                            let e = s.filter(e => {
                                var t;
                                return i.user.id !== (null == (t = e.user) ? void 0 : t.id)
                            }
                            ).map(e => this._toChatProps(e));
                            this._firstFetchedWhenChatListMode || (e = e.filter(e => this.chats.every(t => e.id !== t.id)),
                                this._firstFetchedWhenChatListMode = !0);
                            const { normalChats: o, delayChats: r } = this._separateByChatType(e)
                                , a = Math.floor((t.limit || v.Ln) / v.Ln)
                                , n = Y.i(o, Math.ceil(o.length / a));
                            this.chatlistQue.push(...n),
                                this._delayChatlistQueIntervalId = (0,
                                    y.A)().setTimeout(() => {
                                        const e = Y.i(r, Math.ceil(r.length / a));
                                        this.chatlistQue.push(...e)
                                    }
                                        , v._)
                        }
                        )
                })
            }
            fetchArchiveChatsAndUpdate(e, t) {
                return oe(this, arguments, function* (e, t, i = {}) {
                    let s;
                    if (!this.archiveChatLoading) {
                        this.archiveChatLoading = !0;
                        try {
                            s = yield (0,
                                H.k)(e, ie(te({}, t), {
                                    chatSecretKey: this.stores && this.stores.moviePageStore.movieStore.chatSecretKey
                                }))
                        } catch (e) {
                            return console.error(e),
                                void (0,
                                    o.h5)(() => {
                                        this.archiveChatLoading = !1,
                                            i.isReset && this.resetChats()
                                    }
                                    )
                        }
                        (0,
                            o.h5)(() => {
                                if (i.isReset && this.resetChats(),
                                    this.archiveChatLoading = !1,
                                    0 === s.length)
                                    return void (this.nextArchiveChatExists = !1);
                                const e = s.map(e => this._toChatProps(e));
                                i.isReset ? (this.nextArchiveChatExists = !0,
                                    this.archiveChatQue = e) : this.archiveChatQue = this.archiveChatQue.concat(e)
                            }
                            )
                    }
                })
            }
            fetchFixedPhrases(e) {
                return oe(this, null, function* () {
                    try {
                        const t = yield (0,
                            O.J)();
                        (0,
                            o.h5)(() => {
                                this.fixedPhrases = t.map(e => {
                                    var t, i, s, o;
                                    return {
                                        id: e.id || "",
                                        phrase: {
                                            ja: (null == (t = e.phrase) ? void 0 : t.ja) || "",
                                            en: (null == (i = e.phrase) ? void 0 : i.en) || "",
                                            ko: (null == (s = e.phrase) ? void 0 : s.ko) || "",
                                            zh: (null == (o = e.phrase) ? void 0 : o.zh) || ""
                                        }
                                    }
                                }
                                ),
                                    (0,
                                        p.Ay)().getShowedFixedPhraseChatMovieIds().includes(e) || this.chatSettingStore.isFixedPhraseHidden || !this.stores || this.stores.appStore.isMobile || this.showFixedPhraseChat()
                            }
                            )
                    } catch (e) {
                        return
                    }
                })
            }
            sendFixedPhraseChat(e, t) {
                return oe(this, null, function* () {
                    var i, s;
                    try {
                        const o = yield (0,
                            U.G)(e, t);
                        if (o.message) {
                            if (o.status === v.bo.CHAT_USER_KEY_NOT_EXISTS) {
                                if (this.stores && this.stores.langStore.chat.chatIdRequired) {
                                    const e = String(this.stores.langStore.chat.chatIdRequired).replace(/%s/, `${(0,
                                        g.A)().urls.tvDomain}/profile/profile_user_information`);
                                    return void this._showErrorMessage(e, "warning")
                                }
                                return void this._showErrorMessage(o.message, "warning")
                            }
                            if (o.status === v.bo.COMMON)
                                return void this._showErrorMessage(o.message, "warning");
                            o.status === v.bo.CHAT_SETTING && (this._showErrorMessage(o.message, "warning"),
                                this.chatSettingStore.setMenuType("rule"),
                                this.chatSettingStore.showMenu())
                        } else {
                            const e = null == (s = null == (i = o.data) ? void 0 : i.items) ? void 0 : s[0];
                            if (e) {
                                const t = this._toChatProps(e);
                                this.addChatAndUpdateYellInfo(t)
                            }
                        }
                    } catch (e) {
                        console.error(e),
                            this._showErrorMessage(e.message)
                    }
                })
            }
            deleteFixedPhraseChat(e, t) {
                return oe(this, null, function* () {
                    try {
                        (0,
                            U.V)(e, t.id),
                            (0,
                                o.h5)(() => {
                                    const e = this.chats.find(e => e.id === t.id);
                                    e && this.chats.remove(e)
                                }
                                )
                    } catch (e) {
                        this._showErrorMessage("チャットの削除に失敗しました")
                    }
                })
            }
            postIPBan(e, t) {
                return oe(this, null, function* () {
                    if (this.stores && (this.stores.moviePageStore.movieStore.isOwner || this.stores.moviePageStore.movieStore.channel.isModerating))
                        try {
                            return void (yield (0,
                                P.x)({
                                    movieId: t,
                                    recxuserId: e
                                }))
                        } catch (e) {
                            return this._showErrorMessage(e.message),
                                void console.error(e)
                        }
                })
            }
            fetchReactionProducts() {
                return oe(this, null, function* () {
                    try {
                        const e = yield (0,
                            D.Q)({
                                targetType: "chat"
                            });
                        if (!e || 0 === e.length)
                            return;
                        this.reactionProduct = B.A.createModel(e[0])
                    } catch (e) {
                        return
                    }
                })
            }
            getYellReactionHistory() {
                this.yellReactionHistory = N.A.getYellReactionHistory()
            }
            doYellReaction(e) {
                return oe(this, null, function* () {
                    this.reactionProduct && (this.yellReactionHistory.includes(e.toString()) ? yield this.deleteYellReaction(e, this.reactionProduct.id) : yield this.postYellReaction(e, this.reactionProduct.id))
                })
            }
            postYellReaction(e, t) {
                return oe(this, null, function* () {
                    const i = this.stores && this.stores.moviePageStore.yellStore;
                    try {
                        const s = yield (0,
                            L.Pn)({
                                targetId: e,
                                targetType: "chat",
                                reactionId: t
                            });
                        (0,
                            _.L)(s);
                        const r = R.A.createModel(s.data.items[0]);
                        (0,
                            o.h5)(() => {
                                this.yellReactionHistory = N.A.addYellReactionHistory(e),
                                    this.updateYellReaction(Number(e), r),
                                    i && i.updateYellReaction(Number(e), r)
                            }
                            )
                    } catch (e) {
                        x.A.addMessage({
                            variant: "danger",
                            text: e instanceof Error ? e.message : ""
                        })
                    }
                })
            }
            deleteYellReaction(e, t) {
                return oe(this, null, function* () {
                    const i = this.stores && this.stores.moviePageStore.yellStore;
                    try {
                        const s = yield (0,
                            L.QN)({
                                targetId: e,
                                targetType: "chat",
                                reactionId: t
                            });
                        (0,
                            _.L)(s);
                        const r = R.A.createModel(s.data.items[0]);
                        (0,
                            o.h5)(() => {
                                this.yellReactionHistory = N.A.deleteYellReactionHistory(e),
                                    this.updateYellReaction(Number(e), r),
                                    i && i.updateYellReaction(Number(e), r)
                            }
                            )
                    } catch (e) {
                        x.A.addMessage({
                            variant: "danger",
                            text: e instanceof Error ? e.message : ""
                        })
                    }
                })
            }
            handleChatIconClick(e) {
                (this.isChatVisible && !e || !this.isChatVisible && e) && this.toggleChatDisplay()
            }
            updateMovieDetail(e) {
                var t;
                (null == e ? void 0 : e.blacklist) && this.setBlacklist(null != (t = e.blacklist) ? t : [])
            }
            showBlacklistedChat(e) {
                const t = this.chats.map(t => (t.id === e && (t.isBlacklist = !1),
                    t));
                this.chats.replace(t)
            }
            popArchiveChatQue(e) {
                if (!this.canPopChatQue())
                    return;
                if (!this.archiveChatQue.length)
                    return;
                const t = this.archiveChatQue.filter(t => {
                    if (!this.stores)
                        return !1;
                    if (!t.postedAt)
                        return !1;
                    const i = new Date(t.postedAt).getTime();
                    return this.stores.moviePageStore.movieStore.isLiveStreaming && !this.stores.moviePageStore.movieStore.isLowLatency ? i - v.D8 < e : i < e
                }
                );
                if (!t.length)
                    return;
                const i = t.map(e => e.id);
                this.archiveChatQue = this.archiveChatQue.filter(e => !i.includes(e.id));
                const s = []
                    , o = this.chats.concat(t).filter(e => !s.includes(e.id) && (s.push(e.id),
                        !0))
                    , r = this.stores && this.stores.moviePageStore.yellStore;
                if (r) {
                    const e = r.newSupporterQueue.map(e => e.id);
                    t.filter(t => !!t.yell && !(r.newSupporter.supporter && r.newSupporter.supporter.id === t.id || e.includes(t.id))).map(e => r.toYellTickerFromChat(e, "new")).forEach(r.addNewSupporterQueue, r)
                }
                const a = o[o.length - 1];
                if (this.isScrollBottom())
                    this.chats.replace(o.slice(-v.H5));
                else {
                    const e = o.splice(0, v.o_);
                    this.chats.replace(e),
                        this.archiveChatQue = o.concat(this.archiveChatQue)
                }
                this._setLatestChat(a)
            }
            setEnabledChatInDvr(e) {
                this.enabledChatInDvr = e,
                    this.scrollToEnd()
            }
            setReproducingChatsInDvr(e) {
                this.isReproducingChatInDvr = e
            }
            closePopout() {
                this.isPopout = !1
            }
            setForcedScrollBottom(e) {
                this.forcedScrollBottom = e
            }
            setScrolledByAuto(e) {
                this.isScrolledByAuto = e
            }
            setForcedAutoScrollCount(e) {
                this.forcedAutoScrollCount = e
            }
            setChatTransitioning(e) {
                this._isChatTransitioning = e
            }
            canPopChatQue() {
                return !(this.forcedScrollBottom || this.stores && this.stores.moviePageStore.isTheaterMode && this._isChatTransitioning || this.isScrollingByAuto)
            }
            startScrollByAuto() {
                this.isScrollingByAuto = !0
            }
            finishScrollByAuto() {
                this.isScrollingByAuto = !1
            }
            addChatAndUpdateYellInfo(e) {
                if (!this.stores)
                    return;
                const t = this.stores.userStore
                    , i = this.stores.moviePageStore
                    , s = i.movieStore;
                if (t.user.id === e.userId || !t.user.id && !e.isAuthenticated) {
                    if (this.recentlyDisplayedSelfChatIdList.includes(e.id))
                        return;
                    this.recentlyDisplayedSelfChatIdList.unshift(e.id),
                        this.recentlyDisplayedSelfChatIdList.splice(10)
                }
                i.chatStore.addChats(s.id, [e]),
                    e.yell && (s.leagueId && i.leagueStore.fetchLeagueRankAndUpdate({
                        leagueId: s.leagueId,
                        c: String(e.id)
                    }),
                        i.yellStore.addNewSupporter(e),
                        i.splitViewStore.addNotificationChat(e))
            }
            addChats(e, t) {
                if (!this.stores)
                    return;
                const i = this.stores.moviePageStore.movieStore
                    , { normalChats: s, delayChats: o } = this._separateByChatType(t);
                t.length && i.isLiveStreaming && this.isReproducingChatInDvr && !this.nextArchiveChatExists ? this.archiveChatQue = this.archiveChatQue.concat(t) : (o.length && setTimeout(() => {
                    this.stores && this.stores.moviePageStore.movieStore.id !== e || this.updateChatQue(o)
                }
                    , v._),
                    s.length && this.updateChatQue(s))
            }
            addSystemChatToChatQue(e) {
                if (!e)
                    return;
                const t = C.array(e) ? e : [e];
                this.updateChatQue(t)
            }
            updateChatQue(e) {
                this.chatQue = this.chatQue.concat(e)
            }
            popChatQue() {
                if (!this.stores)
                    return;
                if (!this.canPopChatQue())
                    return;
                if (this.stores.moviePageStore.movieStore.isLiveStreaming && this.isReproducingChatInDvr && !this.stores.moviePageStore.isChatPopoutPage)
                    return;
                if (!this.chatQue.length)
                    return;
                const e = this.chats.concat(this.chatQue)
                    , t = e[e.length - 1];
                if (this.isScrollBottom()) {
                    const t = e.splice(-v.H5);
                    this.chats.replace(t),
                        this.chats.length >= v.H5 && (this.isLastPage = !1),
                        this.chatQue = []
                } else {
                    const t = e.splice(0, v.o_);
                    this.chats.replace(t),
                        this.chatQue = e.slice(-v.Nz)
                }
                this._setLatestChat(t)
            }
            transferFromChatlistQueToChatQue() {
                const e = this.chatlistQue.shift();
                e && this.updateChatQue(e)
            }
            setBlacklist(e) {
                this.blacklist = e.map(e => {
                    var t, i, s;
                    return {
                        id: (null == (t = e.user) ? void 0 : t.id) || "",
                        nickname: (null == (i = e.user) ? void 0 : i.nickname) || "",
                        isOfficial: !!(null == (s = e.user) ? void 0 : s.isOfficial),
                        isMuted: !!e.isMuted
                    }
                }
                )
            }
            fetchBlacklistAndUpdate() {
                return oe(this, null, function* () {
                    try {
                        const e = yield (0,
                            M.P7)();
                        (0,
                            _.j)(e),
                            this.setBlacklist(e.data.items)
                    } catch (e) {
                        console.error(e)
                    }
                })
            }
            addBlacklist(e, t = !1, i) {
                return oe(this, null, function* () {
                    if (this.stores && e === this.stores.moviePageStore.movieStore.channel.id)
                        return this._showErrorMessage(this.stores.langStore.error.blackListRefuse),
                            Promise.reject("配信者はブロックリストに追加できません");
                    try {
                        const s = yield this._postBlacklist(e, t, i);
                        (0,
                            _.L)(s);
                        const r = s.data.items[0];
                        return (0,
                            o.h5)(() => {
                                var e, t, s;
                                this.blacklist.push({
                                    id: (null == (e = r.user) ? void 0 : e.id) || "",
                                    nickname: (null == (t = r.user) ? void 0 : t.nickname) || "",
                                    isOfficial: (null == (s = r.user) ? void 0 : s.isOfficial) || !1,
                                    isMuted: i || !1
                                });
                                const o = this.chats.map(e => {
                                    var t;
                                    return e.userId === (null == (t = r.user) ? void 0 : t.id) && (e.isBlacklist = !0,
                                        this.stores && e.userId && this.stores.moviePageStore.splitViewStore.addBlackList(e.userId)),
                                        e
                                }
                                );
                                if (this.chats.replace(o),
                                    this.stores) {
                                    const e = this.stores.userStore;
                                    e.setBlacklistCount(e.blacklistCount + 1)
                                }
                            }
                            ),
                            Promise.resolve("")
                    } catch (e) {
                        return this._showErrorMessage(e.message),
                            Promise.reject(e.message)
                    }
                })
            }
            addOwnerBlacklist(e) {
                if (e === this.stores.userStore.user.recxuserId)
                    return;
                const t = this.chats.map(t => (t.recxuserId === e && this.stores && this.stores.userStore.user.id !== t.userId && (t.isBlacklist = !0),
                    t));
                this.chats.replace(t)
            }
            deleteOwnerBlacklist(e) {
                const t = this.chats.map(t => (t.recxuserId === e && (t.isBlacklist = !1),
                    t));
                this.chats.replace(t)
            }
            addModerator(e) {
                const t = this.chats.map(t => (t.recxuserId === e && (t.isModerator = !0),
                    t));
                this.chats.replace(t),
                    this.stores && this.stores.moviePageStore.movieStore.channel.id !== this.stores.userStore.user.id && e === this.stores.userStore.user.recxuserId && (this.stores.moviePageStore.movieStore.channel.isModerating = !0)
            }
            deleteModerator(e) {
                const t = this.chats.map(t => (t.recxuserId === e && (t.isModerator = !1),
                    t));
                this.chats.replace(t),
                    this.stores && this.stores.moviePageStore.movieStore.channel.id !== this.stores.userStore.user.id && e === this.stores.userStore.user.recxuserId && (this.stores.moviePageStore.movieStore.channel.isModerating = !1)
            }
            deleteBlacklist(e) {
                return oe(this, null, function* () {
                    try {
                        const t = this.stores.moviePageStore.movieStore.channel;
                        t.isModerating ? yield (0,
                            M.Wx)(t.id, e) : yield (0,
                                M.KZ)(e),
                            this.fetchBlacklistAndUpdate(),
                            (0,
                                o.h5)(() => {
                                    const t = this.chats.map(t => (t.userId === e && (t.isBlacklist = !1,
                                        this.stores && t.userId && this.stores.moviePageStore.splitViewStore.deleteBlackList(t.userId)),
                                        t));
                                    if (this.chats.replace(t),
                                        this.stores) {
                                        const e = this.stores.userStore;
                                        e.setBlacklistCount(e.blacklistCount - 1)
                                    }
                                }
                                )
                    } catch (e) {
                        this._showErrorMessage("ブロックリストの削除に失敗しました")
                    }
                })
            }
            updateBlacklist(e, t, i) {
                (0,
                    o.h5)(() => {
                        const s = this.chats.map(i => (i.userId === e.id && (i.isBlacklist = t,
                            this.stores && i.userId && this.stores.moviePageStore.splitViewStore.deleteBlackList(i.userId)),
                            i));
                        if (this.chats.replace(s),
                            t ? this.blacklist.unshift({
                                id: (null == e ? void 0 : e.id) || "",
                                nickname: (null == e ? void 0 : e.name) || "",
                                isOfficial: (null == e ? void 0 : e.isOfficial) || !1,
                                isMuted: i || !1
                            }) : this.blacklist = this.blacklist.filter(t => t.id !== e.id),
                            this.stores) {
                            const e = this.stores.userStore
                                , i = t ? e.blacklistCount + 1 : e.blacklistCount - 1;
                            e.setBlacklistCount(i)
                        }
                    }
                    )
            }
            initModerator() {
                return oe(this, null, function* () {
                    var e;
                    let t;
                    try {
                        const i = yield (0,
                            T.VM)();
                        t = (null == (e = i.data) ? void 0 : e.items) || [],
                            (0,
                                o.h5)(() => {
                                    t.map(e => {
                                        this.moderator.push({
                                            id: e.id || "",
                                            name: e.nickname || ""
                                        })
                                    }
                                    )
                                }
                                )
                    } catch (e) {
                        console.error(e)
                    }
                })
            }
            fetchMyPointAndUpdate() {
                return oe(this, null, function* () {
                    var e, t;
                    try {
                        const i = null == (t = null == (e = (yield (0,
                            E.E)({
                                deviceType: 1
                            })).data) ? void 0 : e.items) ? void 0 : t[0];
                        (0,
                            o.h5)(() => {
                                this.points = i && i.points || 0
                            }
                            )
                    } catch (e) {
                        x.A.addMessage({
                            variant: "danger",
                            text: "ポイントの取得に失敗しました"
                        }),
                            console.error(e)
                    }
                })
            }
            showChat() {
                this.isChatVisible = !0,
                    (0,
                        p.Ay)().setMoviePagePlayMode({
                            isChatVisible: !0
                        })
            }
            hideChat() {
                this.isChatVisible = !1,
                    (0,
                        p.Ay)().setMoviePagePlayMode({
                            isChatVisible: !1
                        })
            }
            toggleChatDisplay() {
                this.isChatVisible ? this.hideChat() : this.showChat()
            }
            resetTracing() {
                this.isTracing = !1
            }
            scrollByAuto() {
                if (this.scrollListEl) {
                    if (this.forcedScrollBottom)
                        return this.scrollListEl.scrollTop = this.scrollEnd,
                            void this.setForcedScrollBottom(!1);
                    if (this.hasBottomChatListDiffrence()) {
                        if (this.forcedAutoScrollCount)
                            return this.setForcedAutoScrollCount(this.forcedAutoScrollCount - 1),
                                void this.scrollToEnd({
                                    enabledAnimation: !0
                                });
                        if (this.isPreScrollBottom())
                            return this.isScrolledByAuto || (this.setScrolledByAuto(!0),
                                this.setForcedAutoScrollCount(2),
                                setTimeout(() => {
                                    this.setForcedAutoScrollCount(0)
                                }
                                    , 1500)),
                                void this.scrollToEnd({
                                    enabledAnimation: !0
                                });
                        this.setScrolledByAuto(!1)
                    }
                    return this.isTracing && this._hasTopChatListDiffrence() ? (this.resetTracing(),
                        void (this.preScrollEnd && (this.scrollListEl.scrollTop = this.scrollEnd - this.preScrollEnd))) : void 0
                }
            }
            markAsReadLatestChatLabel() {
                this.latestChat = null
            }
            setPreFirstChatCellId(e) {
                this._preFirstChatCellId = e
            }
            setPreLastChatCell(e) {
                this._preLastChatCell = e
            }
            hasBottomChatListDiffrence() {
                const e = this.chats[this.chats.length - 1];
                return !!(e && this._preLastChatCell && (this._preLastChatCell.id !== e.id || this._preLastChatCell.isSystemMessage && e.isSystemMessage && this._preLastChatCell.postedAt !== e.postedAt))
            }
            startIntervalToPopChatQue() {
                let e = v.ap + v.vt;
                (C.ie() || C.safari()) && (e *= 4),
                    this._chatAddIntervalId = (0,
                        y.A)().setInterval(() => {
                            this._chatQueAnimationFrame && ((0,
                                y.A)().cancelAnimationFrame(this._chatQueAnimationFrame),
                                this._chatQueAnimationFrame = 0),
                                this._chatQueAnimationFrame = (0,
                                    y.A)().requestAnimationFrame(() => {
                                        this.transferFromChatlistQueToChatQue(),
                                            this.popChatQue()
                                    }
                                    )
                        }
                            , e)
            }
            addSystemChatForLevelToChatQue(e, t, i) {
                if (!this.stores)
                    return;
                const s = this.stores.moviePageStore
                    , o = s.movieStore;
                e.forEach(e => {
                    switch (e) {
                        case "playUll":
                            "lowLatency" === t.type && o.media.urlUll && "hlsjs" === s.currentPlayerType && o.isLiveStreaming && this.addSystemChatToChatQue(this.createSystemChat("playUll"));
                            break;
                        case "highBitrate":
                            !(0,
                                m.A)().isHighBitrateLevel(t) || i && (0,
                                    m.A)().isHighBitrateLevel(i) || o.isComingUp || this.addSystemChatToChatQue(this.createSystemChat("highBitrate"))
                    }
                }
                )
            }
            createSystemChat(e, t, i) {
                if (!this.stores)
                    return null;
                const s = this.stores.langStore
                    , o = this.stores.moviePageStore.movieStore;
                let r, a = !1;
                switch (e) {
                    case "ullSelectable":
                        r = s.video.lowLatencyToast,
                            this._systemChatId--;
                        break;
                    case "playUll":
                        r = s.video.lowlatencySystemMessage,
                            this._systemChatId--;
                        break;
                    case "highBitrate":
                        r = s.video.highBitrateSystemMessage,
                            this._systemChatId--;
                        break;
                    case "memberOnly":
                    case "memberOnlyStart":
                        r = o.isPpvEnabled ? o.isUploadedMovie ? s.ppv.canSeeCommentWhenPurchaseTicket : s.ppv.canSeeChatWhenPurchaseTicket : o.isLowestSubsProductPermitted ? o.isUploadedMovie ? s.subscription.seeComment : s.subscription.seeChat : o.isUploadedMovie ? s.subscription.canSeeCommentOnHigherPlan : s.subscription.canSeeChatOnHigherPlan,
                            this._systemChatId--;
                        break;
                    case "chatRule":
                        r = null != t ? t : "",
                            this._systemChatId--,
                            a = !0;
                        break;
                    case "custom":
                    case "extension":
                        r = null != t ? t : "",
                            this._systemChatId--
                }
                return r ? {
                    id: this._systemChatId,
                    message: r,
                    postedAt: (new Date).toISOString(),
                    isSystemMessage: !0,
                    isChatRule: a,
                    deletable: !1,
                    disabled: !1,
                    onSystemChatClick: i
                } : null
            }
            handleLevelChange(e, t) {
                this.addSystemChatForLevelToChatQue(["highBitrate"], e, t)
            }
            showFixedPhraseChat() {
                this.isFixedPhraseChatVisible = !0
            }
            hideFixedPhraseChat() {
                this.isFixedPhraseChatVisible = !1
            }
            showFixedPhraseList() {
                this.isFixedPhraseListVisible = !0
            }
            hideFixedPhraseList() {
                this.isFixedPhraseListVisible = !1
            }
            addSystemChatForPurchasedToChatQue(e, t) {
                const i = [{
                    message: t,
                    id: this._systemChatId--,
                    isNotificationOfSubs: "subs" === e,
                    isNotificationOfPpvTicketPurchase: "ppv" === e,
                    postedAt: (new Date).toISOString(),
                    deletable: !1,
                    disabled: !1
                }];
                this.updateChatQue(i)
            }
            setCurrentYellGroupId(e) {
                this.currentYellGroupId = e
            }
            updateAppealChat() {
                if (this.stores)
                    if (this.stores && this.stores.moviePageStore.isTheaterMode) {
                        if (this._hideSubsAppealTimeoutId)
                            return;
                        const e = this.chats.find(e => e.isMemberAppeal);
                        if (!e)
                            return;
                        this._hideSubsAppealTimeoutId = (0,
                            y.A)().setTimeout((0,
                                o.XI)(() => {
                                    this.chats.remove(e)
                                }
                                ), 5400)
                    } else
                        (0,
                            y.A)().clearTimeout(this._hideSubsAppealTimeoutId),
                            this._hideSubsAppealTimeoutId = 0
            }
            updateYellReaction(e, t) {
                const i = this.chats.find(t => t.id === e);
                i && i.yell && (i.yell.reaction = t)
            }
            _separateByChatType(e) {
                const t = e => {
                    var t, i;
                    return "lowLatency" !== (null == (i = null == (t = this.stores) ? void 0 : t.appStore.currentPlayerInfo.level) ? void 0 : i.type) && e.isLowLatency && this.chatSettingStore.delayChat
                }
                    ;
                return {
                    normalChats: e.filter(e => !t(e)),
                    delayChats: e.filter(t)
                }
            }
            _hasTopChatListDiffrence() {
                const e = this.chats.length && this.chats[0];
                return !!e && this._preFirstChatCellId !== e.id
            }
            _toChatPropsFromLegacy(e) {
                var t;
                const i = this.stores && this.stores.moviePageStore.movieStore
                    , s = e.user || {}
                    , o = e.to_user
                    , r = e.chat_setting || {}
                    , a = (e.badges || [])[0] || {}
                    , n = a.subscription || {}
                    , l = {
                        id: e.id || 0,
                        message: e.message || "",
                        postedAt: e.posted_at || "",
                        userId: s.id || "",
                        recxuserId: s.recxuser_id || 0,
                        userIconImageUrl: s.icon_image_url || "",
                        userCoverImageUrl: s.cover_image_url || "",
                        userName: s.nickname || "",
                        userColor: r.name_color || "",
                        isOfficial: !!s.is_official,
                        isOfficialHidden: !!r.is_official_hidden,
                        isFresh: !!s.is_fresh,
                        isPremium: !!s.is_premium,
                        isPremiumHidden: !!r.is_premium_hidden,
                        isSubsBadgeHidden: !!r.is_subs_badge_hidden,
                        isSubsDurationHidden: !!r.is_subs_duration_hidden,
                        isWarned: !!s.is_warned,
                        isModerator: !!e.is_moderating,
                        isLowLatency: !!i && i.isLowLatency && !!e.quality_type && e.quality_type === v.lT.LOW_LATENCY,
                        isAuthenticated: !!e.is_authenticated,
                        isTopSupporter: !!e.is_top_supporter,
                        isMuted: !!e.is_muted,
                        hasBannedWord: !!e.has_banned_word,
                        subsBadge: {
                            id: a.id || 0,
                            imageUrl: a.image_url || "",
                            months: n.months || 0,
                            tier: n && n.tier || 0,
                            label: a.label || ""
                        },
                        userInChannel: {
                            isModerating: !!e.is_moderating,
                            membershipCardUrl: e.membership_card_url || ""
                        }
                    };
                if (e.yell) {
                    if (l.yell = {
                        id: e.yell.id || 0,
                        imageUrl: e.yell.image_url || "",
                        quantity: e.yell.yells || 0,
                        seconds: e.yell.ticker_seconds || 10,
                        toUser: o ? {
                            userName: o.nickname || "",
                            userIconImageUrl: o.icon_image_url || ""
                        } : void 0
                    },
                        e.yell.subs_share) {
                        const i = null == (t = e.yell.subs_share.subs_product) ? void 0 : t.subs_badges;
                        l.yell.subsShareCount = e.yell.subs_share.share_count || void 0,
                            l.yell.subsShareBadge = (null == i ? void 0 : i[0]) ? {
                                id: i[0].id || 0,
                                imageUrl: i[0].image_url || ""
                            } : void 0
                    }
                    i && (l.yell.isLeague = i.leagueId && i.enabledYell && i.channel.isLeagueYell || !1,
                        l.yell.isCastYell = i.enabledCastYell)
                }
                if (e.stamp && (l.stamp = {
                    id: e.stamp.id || 0,
                    imageUrl: e.stamp.image_url || "",
                    groupId: e.stamp.group_id || 0
                }),
                    e.to_user && (l.toUser = {
                        userId: e.to_user.id || "",
                        userName: e.to_user.nickname || "",
                        userIconImageUrl: e.to_user.icon_image_url || ""
                    }),
                    this.stores && (l.disabled = this.stores.moviePageStore.isDisabledChat(l) || this.isBannedWordChat(l)),
                    e.capture && e.capture.capture && e.capture.capture_channel && (l.isSystemMessage = !0,
                        l.capture = {
                            capture: {
                                id: e.capture.capture.id || "",
                                thumbnail_url: e.capture.capture.thumbnail_url || "",
                                title: e.capture.capture.title || ""
                            },
                            captureChannel: {
                                id: e.capture.capture_channel.id || "",
                                nickname: e.capture.capture_channel.nickname || ""
                            }
                        }),
                    e.system_message && (l.isSystemMessage = !0,
                        l.systemMessage = {
                            style: e.system_message.style || void 0,
                            extensionKey: e.system_message.extension_key_info && e.system_message.extension_key_info.extension_key ? e.system_message.extension_key_info.extension_key : void 0
                        },
                        i && e.system_message.extension_key_info && e.system_message.extension_key_info.extension_key && (l.onSystemChatClick = () => {
                            var t, s, o, r, a, n;
                            return i.setDisplayExtension({
                                id: (null == (s = null == (t = e.system_message) ? void 0 : t.extension_key_info) ? void 0 : s.extension_key) || "",
                                installationId: (null == (r = null == (o = e.system_message) ? void 0 : o.extension_key_info) ? void 0 : r.installation_key) || "",
                                matchType: (null == (n = null == (a = e.system_message) ? void 0 : a.extension_key_info) ? void 0 : n.match_type) || "extension_key"
                            })
                        }
                        ),
                        e.system_message.poll)) {
                    const t = e.system_message.poll;
                    l.poll = {
                        id: e.system_message.poll.id || "",
                        status: (e.system_message.type || "poll_start").split("_")[1],
                        pollThumbnailUrl: t.poll_thumbnail_url || "",
                        voteThumbnailUrl: t.vote_thumbnail_url || ""
                    }
                }
                return e.chat_setting && (l.chatCommentAppearanceSetting = F.A.createModel(A.changeCaseKey("camel", e.chat_setting))),
                    l
            }
            _toChatProps(e) {
                var t, i, s, o, r, a, n, l, d, h, u, c, p, m, y, g, S, b, f, C, I, A, _, P, M, w, T, E;
                const U = this.stores && this.stores.moviePageStore.movieStore
                    , L = {
                        id: e.id || 0,
                        message: e.message || "",
                        postedAt: e.postedAt || "",
                        linkUrl: e.linkUrl || "",
                        userId: (null == (t = e.user) ? void 0 : t.id) || "",
                        recxuserId: (null == (i = e.user) ? void 0 : i.recxuserId) || 0,
                        userIconImageUrl: (null == (s = e.user) ? void 0 : s.iconImageUrl) || "",
                        userCoverImageUrl: (null == (o = e.user) ? void 0 : o.coverImageUrl) || "",
                        userName: (null == (r = e.user) ? void 0 : r.nickname) || "",
                        userColor: (null == (a = e.chatSetting) ? void 0 : a.nameColor) || "",
                        isOfficial: !!(null == (n = e.user) ? void 0 : n.isOfficial),
                        isOfficialHidden: !!(null == (l = e.chatSetting) ? void 0 : l.isOfficialHidden),
                        isFresh: !!(null == (d = e.user) ? void 0 : d.isFresh),
                        isPremium: !!(null == (h = e.user) ? void 0 : h.isPremium),
                        isPremiumHidden: !!(null == (u = e.chatSetting) ? void 0 : u.isPremiumHidden),
                        isSubsBadgeHidden: !!(null == (c = e.chatSetting) ? void 0 : c.isSubsBadgeHidden),
                        isSubsDurationHidden: !!(null == (p = e.chatSetting) ? void 0 : p.isSubsDurationHidden),
                        isWarned: !!(null == (m = e.user) ? void 0 : m.isWarned),
                        isModerator: !!e.isModerating,
                        isLowLatency: !!U && U.isLowLatency && !!e.qualityType && e.qualityType === v.lT.LOW_LATENCY,
                        isAuthenticated: !!e.isAuthenticated,
                        isTopSupporter: !!e.isTopSupporter,
                        isMuted: !!e.isMuted,
                        hasBannedWord: !!e.hasBannedWord,
                        subsBadge: {
                            id: (null == (g = null == (y = e.badges) ? void 0 : y[0]) ? void 0 : g.id) || 0,
                            imageUrl: (null == (b = null == (S = e.badges) ? void 0 : S[0]) ? void 0 : b.imageUrl) || "",
                            months: (null == (I = null == (C = null == (f = e.badges) ? void 0 : f[0]) ? void 0 : C.subscription) ? void 0 : I.months) || 0,
                            tier: (null == (P = null == (_ = null == (A = e.badges) ? void 0 : A[0]) ? void 0 : _.subscription) ? void 0 : P.tier) || 0,
                            label: (null == (w = null == (M = e.badges) ? void 0 : M[0]) ? void 0 : w.label) || ""
                        },
                        userInChannel: {
                            isModerating: e.isModerating || !1,
                            membershipCardUrl: e.membershipCardUrl || ""
                        }
                    };
                if (e.yell) {
                    if (L.yell = {
                        id: e.yell.id || 0,
                        imageUrl: e.yell.imageUrl || "",
                        quantity: e.yell.yells || 0,
                        seconds: e.yell.tickerSeconds || 10,
                        toUser: e.toUser ? {
                            userName: e.toUser.nickname || "",
                            userIconImageUrl: e.toUser.iconImageUrl || ""
                        } : void 0
                    },
                        U && (L.yell.isLeague = U.leagueId && U.enabledYell && U.channel.isLeagueYell || !1,
                            L.yell.isCastYell = U.enabledCastYell),
                        e.yell.subsShare) {
                        const t = null == (T = e.yell.subsShare.subsProduct) ? void 0 : T.subsBadges;
                        L.yell.subsShareCount = e.yell.subsShare.shareCount || void 0,
                            L.yell.subsShareBadge = (null == t ? void 0 : t[0]) ? {
                                id: t[0].id || 0,
                                imageUrl: t[0].imageUrl || ""
                            } : void 0
                    }
                    e.reactionStatsList && e.reactionStatsList.length > 0 && (L.yell.reaction = R.A.createModel(e.reactionStatsList[0]))
                }
                if (e.channelStar && (L.channelStar = {
                    earnedStars: e.channelStar.earnedStars || 0,
                    iconImageUrl: e.channelStar.iconImageUrl || ""
                }),
                    e.stamp && (L.stamp = {
                        id: e.stamp.id || 0,
                        imageUrl: e.stamp.imageUrl || "",
                        groupId: e.stamp.groupId || 0
                    }),
                    e.toUser && (L.toUser = {
                        userId: e.toUser.id || "",
                        userName: e.toUser.nickname || "",
                        userIconImageUrl: e.toUser.iconImageUrl || ""
                    }),
                    this.stores && (L.disabled = this.stores.moviePageStore.isDisabledChat(L) || this.isBannedWordChat(L)),
                    e.capture && e.capture.capture && e.capture.captureChannel && (L.isSystemMessage = !0,
                        L.capture = {
                            capture: {
                                id: e.capture.capture.id || "",
                                thumbnail_url: e.capture.capture.thumbnailUrl || "",
                                title: e.capture.capture.title || ""
                            },
                            captureChannel: {
                                id: e.capture.captureChannel.id || "",
                                nickname: e.capture.captureChannel.nickname || ""
                            }
                        }),
                    e.systemMessage) {
                    if (L.isSystemMessage = !0,
                        L.systemMessage = {
                            style: e.systemMessage.style || void 0,
                            extensionKey: e.systemMessage.extensionKeyInfo && e.systemMessage.extensionKeyInfo.extensionKey ? e.systemMessage.extensionKeyInfo.extensionKey : void 0
                        },
                        U && e.systemMessage.extensionKeyInfo && e.systemMessage.extensionKeyInfo.extensionKey && (L.onSystemChatClick = () => {
                            var t, i, s, o, r, a;
                            return U.setDisplayExtension({
                                id: (null == (i = null == (t = e.systemMessage) ? void 0 : t.extensionKeyInfo) ? void 0 : i.extensionKey) || "",
                                installationId: (null == (o = null == (s = e.systemMessage) ? void 0 : s.extensionKeyInfo) ? void 0 : o.installationKey) || "",
                                matchType: (null == (a = null == (r = e.systemMessage) ? void 0 : r.extensionKeyInfo) ? void 0 : a.matchType) || "extension_key"
                            })
                        }
                        ),
                        e.systemMessage.poll) {
                        const t = e.systemMessage.poll;
                        L.poll = {
                            id: e.systemMessage.poll.id || "",
                            status: (e.systemMessage.type || "poll_start").split("_")[1],
                            pollThumbnailUrl: t.pollThumbnailUrl || "",
                            voteThumbnailUrl: t.voteThumbnailUrl || ""
                        }
                    }
                    (null == (E = e.systemMessage.subsBadges) ? void 0 : E[0]) && (L.systemMessage.subsBadgeImage = e.systemMessage.subsBadges[0].imageUrl || "")
                }
                return e.user && (L.v8User = X.A.createModel(e.user)),
                    e.chatSetting && (L.chatCommentAppearanceSetting = F.A.createModel(e.chatSetting)),
                    L
            }
            isBannedWordChat(e) {
                if (!this.stores)
                    return !1;
                const t = this.stores.moviePageStore.movieStore
                    , i = this.stores.userStore;
                return (!i || i.user.id !== e.userId) && !!(this.chatSettingStore.muteForbiddenWord && e.message && t && t.bannedWords.length > 0) && t.bannedWords.some(t => {
                    var i;
                    return 0 === t.type ? t.words.toLowerCase() === (null == (i = e.message) ? void 0 : i.toLowerCase()) : e.message.toLowerCase().indexOf(t.words.toLowerCase()) > -1
                }
                )
            }
            _setLatestChat(e) {
                e && (e.disabled || this.stores && this.stores.moviePageStore.isDisabledChat(e) || e.isMemberAppeal || (this.isScrollBottom() ? this.markAsReadLatestChatLabel() : this.latestChat = e))
            }
            _setAppealChat(e) {
                const t = e.userStore
                    , i = e.moviePageStore.movieStore;
                this._disposeAppealChat = (0,
                    o.z7)(() => !this.chatLoading && (i.isPpvEnabled ? !(!i.hasPpvTicketProducts && i.isSubsEnabled && i.hasSubscribedGreaterThanOrEqualToPermittedSubsProduct) && (i.hasPpvTicketProducts ? !(!i.isCompletedFetchMovie || !i.channel.enabledFanletter || i.isOwner) : !(!t.isCompletedInitialFetch || t.isLogined) || i.isCompletedFetchMovieDetail) : i.openedSubscription ? !i.isOwner && (!(!t.isCompletedInitialFetch || t.isLogined) || i.isCompletedFetchMovieDetail) : !(!i.isCompletedFetchMovie || !i.channel.enabledFanletter || i.isOwner)), () => {
                        (0,
                            y.A)().clearTimeout(this._subsAppealTimeoutId),
                            i.isMemberOnly || i.isMemberTrial ? i.isPpvEnabled && i.hasPpvTicketProducts || this._addSystemChatForMemberAppealToChatQue() : i.isPpvEnabled || i.openedSubscription && !i.hasSubscribed && 0 === Math.floor(2 * Math.random()) && (this._subsAppealTimeoutId = (0,
                                y.A)().setTimeout(() => {
                                    const e = (0,
                                        p.Ay)().getSubsAppealChatHistory(i.channel.id);
                                    0 === Math.floor(Math.random() * (e.impressions + 1)) && (this._addSystemChatForMemberAppealToChatQue(),
                                        (0,
                                            p.Ay)().setSubsAppealChatHistoriy(i.channel.id),
                                        (0,
                                            y.A)().setTimeout(() => {
                                                this.updateAppealChat()
                                            }
                                                , v.ap + v.vt))
                                }
                                    , v.Rl))
                    }
                    )
            }
            _setFollowAppealChat(e) {
                const t = e.userStore
                    , i = e.moviePageStore.movieStore;
                this._disposeFollowAppealChat = (0,
                    o.z7)(() => !(!t.isLogined || !i.isCompletedFetchMovieDetail || i.channel.id === t.user.id || i.channel.isFollowing || !i.isLiveStreaming && !i.isArchive), () => {
                        (0,
                            y.A)().clearTimeout(this._followAppealTimeoutId),
                            this._followAppealTimeoutId = (0,
                                y.A)().setTimeout(() => {
                                    const e = (0,
                                        p.Ay)().getFollowAppealChatHistory(i.channel.id);
                                    0 === Math.floor(Math.random() * (e.impressions + 1)) && (this.addSystemChatForFollowAppealToChatQue(),
                                        (0,
                                            p.Ay)().setFollowAppealChatHistoriy(i.channel.id))
                                }
                                    , 5 * V.pY)
                    }
                    )
            }
            _setChatRuleAdd(e) {
                const t = e.moviePageStore.movieStore;
                this._disposeChatRuleAdd = (0,
                    o.z7)(() => {
                        if (!e.userStore.isCompletedInitialFetch)
                            return !1;
                        if (e.userStore.isLogined && !t.isCompletedFetchMovieDetail)
                            return !1;
                        if (this.chatLoading)
                            return !1;
                        if (!t.id)
                            return !1;
                        const i = t.settings.chatRule || "";
                        return !(!i || !i.trim() || t.isUploadedMovie || t.isArchive || t.isOwner || t.channel.isModerating)
                    }
                        , () => {
                            const t = e.moviePageStore.movieStore.settings.chatRule || "";
                            this.addSystemChatToChatQue(this.createSystemChat("chatRule", t))
                        }
                    )
            }
            _addSystemChatForMemberAppealToChatQue() {
                const e = [{
                    id: this._systemChatId--,
                    isMemberAppeal: !0,
                    postedAt: (new Date).toISOString(),
                    deletable: !1,
                    isSystemMessage: !0,
                    disabled: !1
                }];
                this.updateChatQue(e)
            }
            addSystemChatForFollowAppealToChatQue() {
                const e = this.chats.slice(-1)[0];
                if (e && e.isFollowAppeal)
                    return;
                const t = [{
                    id: this._systemChatId--,
                    isFollowAppeal: !0,
                    postedAt: (new Date).toISOString(),
                    deletable: !1,
                    isSystemMessage: !0,
                    disabled: !1
                }];
                this.updateChatQue(t)
            }
            _postBlacklist(e, t = !1, i) {
                const s = this.stores.moviePageStore.movieStore.channel;
                return s.isModerating ? (0,
                    M.HV)(s.id, e, {}, {
                        isIpBanned: t,
                        isMuted: i
                    }) : (0,
                        M.vm)(e, {}, {
                            isIpBanned: t,
                            isMuted: i
                        })
            }
            _updateExtensionIds(e) {
                if (!this.stores)
                    return;
                if (!e.systemMessage)
                    return;
                const t = this.stores.moviePageStore.movieStore;
                e.systemMessage.extensionKeyInfo && e.systemMessage.extensionKeyInfo.extensionKey && ([...t.extensions, ...t.notInstallExtensions].some(t => {
                    var i, s;
                    return t.id === (null == (s = null == (i = e.systemMessage) ? void 0 : i.extensionKeyInfo) ? void 0 : s.extensionKey)
                }
                ) || this._extensionIds.includes(e.systemMessage.extensionKeyInfo.extensionKey) || this._extensionIds.push(e.systemMessage.extensionKeyInfo.extensionKey))
            }
            _showErrorMessage(e = "", t) {
                x.A.addMessage({
                    variant: t || "danger",
                    text: e
                })
            }
        }
        se([o.sH], re.prototype, "chats", 2),
            se([o.sH], re.prototype, "latestChat", 2),
            se([o.sH], re.prototype, "isReproducingChatInDvr", 2),
            se([o.sH], re.prototype, "blacklist", 2),
            se([o.sH], re.prototype, "moderator", 2),
            se([o.sH], re.prototype, "bannedWords", 2),
            se([o.sH], re.prototype, "chatScrollEnable", 2),
            se([o.sH], re.prototype, "isLastPage", 2),
            se([o.sH], re.prototype, "isPopout", 2),
            se([o.sH], re.prototype, "isTracing", 2),
            se([o.sH], re.prototype, "isChatVisible", 2),
            se([o.sH], re.prototype, "_isChatTransitioning", 2),
            se([o.sH], re.prototype, "fixedPhrases", 2),
            se([o.sH], re.prototype, "isFixedPhraseChatVisible", 2),
            se([o.sH], re.prototype, "isFixedPhraseListVisible", 2),
            se([o.sH], re.prototype, "currentYellGroupId", 2),
            se([o.EW], re.prototype, "scrollEndByTheaterMode", 1),
            se([o.sH], re.prototype, "chatLoading", 2),
            se([o.sH], re.prototype, "enabledChatInDvr", 2),
            se([o.sH], re.prototype, "isScrollingByAuto", 2),
            se([o.sH], re.prototype, "yellReactionHistory", 2),
            se([o.XI], re.prototype, "willLoad", 1),
            se([o.XI], re.prototype, "willUnload", 1),
            se([o.XI], re.prototype, "sendChat", 1),
            se([o.XI], re.prototype, "sendChatAfterChatTermConsenet", 1),
            se([o.XI.bound], re.prototype, "deleteChat", 1),
            se([o.XI.bound], re.prototype, "updateChat", 1),
            se([o.XI], re.prototype, "fetchChatAndReset", 1),
            se([o.XI.bound], re.prototype, "updateModeratedChat", 1),
            se([o.XI.bound], re.prototype, "resetChats", 1),
            se([o.XI.bound], re.prototype, "traceChats", 1),
            se([o.XI], re.prototype, "fetchChatsAndUpdate", 1),
            se([o.XI], re.prototype, "fetchChatsAndUpdateChatlistQue", 1),
            se([o.XI.bound], re.prototype, "fetchArchiveChatsAndUpdate", 1),
            se([o.XI], re.prototype, "fetchFixedPhrases", 1),
            se([o.XI], re.prototype, "sendFixedPhraseChat", 1),
            se([o.XI.bound], re.prototype, "deleteFixedPhraseChat", 1),
            se([o.XI], re.prototype, "postIPBan", 1),
            se([o.XI], re.prototype, "fetchReactionProducts", 1),
            se([o.XI], re.prototype, "getYellReactionHistory", 1),
            se([o.XI.bound], re.prototype, "postYellReaction", 1),
            se([o.XI.bound], re.prototype, "deleteYellReaction", 1),
            se([o.XI.bound], re.prototype, "handleChatIconClick", 1),
            se([o.XI.bound], re.prototype, "updateMovieDetail", 1),
            se([o.XI.bound], re.prototype, "showBlacklistedChat", 1),
            se([o.XI], re.prototype, "popArchiveChatQue", 1),
            se([o.XI.bound], re.prototype, "setEnabledChatInDvr", 1),
            se([o.XI.bound], re.prototype, "setReproducingChatsInDvr", 1),
            se([o.XI], re.prototype, "closePopout", 1),
            se([o.XI], re.prototype, "setForcedScrollBottom", 1),
            se([o.XI.bound], re.prototype, "startScrollByAuto", 1),
            se([o.XI.bound], re.prototype, "finishScrollByAuto", 1),
            se([o.XI], re.prototype, "addChatAndUpdateYellInfo", 1),
            se([o.XI], re.prototype, "addChats", 1),
            se([o.XI], re.prototype, "addSystemChatToChatQue", 1),
            se([o.XI], re.prototype, "updateChatQue", 1),
            se([o.XI.bound], re.prototype, "popChatQue", 1),
            se([o.XI.bound], re.prototype, "transferFromChatlistQueToChatQue", 1),
            se([o.XI], re.prototype, "setBlacklist", 1),
            se([o.XI], re.prototype, "fetchBlacklistAndUpdate", 1),
            se([o.XI], re.prototype, "addBlacklist", 1),
            se([o.XI], re.prototype, "addOwnerBlacklist", 1),
            se([o.XI], re.prototype, "deleteOwnerBlacklist", 1),
            se([o.XI], re.prototype, "addModerator", 1),
            se([o.XI], re.prototype, "deleteModerator", 1),
            se([o.XI], re.prototype, "deleteBlacklist", 1),
            se([o.XI.bound], re.prototype, "updateBlacklist", 1),
            se([o.XI], re.prototype, "initModerator", 1),
            se([o.XI.bound], re.prototype, "fetchMyPointAndUpdate", 1),
            se([o.XI.bound], re.prototype, "showChat", 1),
            se([o.XI.bound], re.prototype, "hideChat", 1),
            se([o.XI.bound], re.prototype, "toggleChatDisplay", 1),
            se([o.XI], re.prototype, "resetTracing", 1),
            se([o.XI.bound], re.prototype, "markAsReadLatestChatLabel", 1),
            se([o.XI.bound], re.prototype, "handleLevelChange", 1),
            se([o.XI.bound], re.prototype, "showFixedPhraseChat", 1),
            se([o.XI.bound], re.prototype, "hideFixedPhraseChat", 1),
            se([o.XI.bound], re.prototype, "showFixedPhraseList", 1),
            se([o.XI.bound], re.prototype, "hideFixedPhraseList", 1),
            se([o.XI.bound], re.prototype, "setCurrentYellGroupId", 1),
            se([o.XI.bound], re.prototype, "updateAppealChat", 1);
        var ae = i(4105)
            , ne = Object.defineProperty
            , le = Object.getOwnPropertyDescriptor
            , de = Object.getOwnPropertySymbols
            , he = Object.prototype.hasOwnProperty
            , ue = Object.prototype.propertyIsEnumerable
            , ce = (e, t, i) => t in e ? ne(e, t, {
                enumerable: !0,
                configurable: !0,
                writable: !0,
                value: i
            }) : e[t] = i
            , pe = (e, t, i, s) => {
                for (var o, r = s > 1 ? void 0 : s ? le(t, i) : t, a = e.length - 1; a >= 0; a--)
                    (o = e[a]) && (r = (s ? o(t, i, r) : o(r)) || r);
                return s && r && ne(t, i, r),
                    r
            }
            , me = (e, t, i) => new Promise((s, o) => {
                var r = e => {
                    try {
                        n(i.next(e))
                    } catch (e) {
                        o(e)
                    }
                }
                    , a = e => {
                        try {
                            n(i.throw(e))
                        } catch (e) {
                            o(e)
                        }
                    }
                    , n = e => e.done ? s(e.value) : Promise.resolve(e.value).then(r, a);
                n((i = i.apply(e, t)).next())
            }
            );
        class ye {
            constructor(e) {
                this.chatModerators = o.sH.array(),
                    this.latestTelop = {
                        id: 0,
                        message: null,
                        linkUrl: ""
                    },
                    this.chatModeratorQue = [],
                    this.moderators = [],
                    this.chatScrollEnable = !1,
                    this.isLastPage = !1,
                    this.isChatVisible = !1,
                    this.isCompletedInicialFetch = !1,
                    this.chatLoading = !0,
                    this.scrollTop = 0,
                    this.scrollEnd = 0,
                    this.preScrollEnd = 0,
                    this.forcedScrollBottom = !0,
                    this.isScrolledByAuto = !1,
                    this.forcedAutoScrollCount = 0,
                    this.enabledChatInDvr = !1,
                    this.isScrollingByAuto = !1,
                    this._isLoadedChatModerator = !1,
                    this._preLastChatCellId = null,
                    (0,
                        q.H)(this, {
                            scrollListEl: o.sH
                        }),
                    this.chatSettingStore = e
            }
            setScrollTop(e) {
                this.scrollTop = e
            }
            setScrollEnd(e) {
                this.preScrollEnd = this.scrollEnd,
                    this.scrollEnd = e
            }
            isScrollBottom() {
                return this.scrollEnd - this.scrollTop < v.oN
            }
            scrollToEnd(e = {}) {
                const t = e.scrollEnd || this.scrollEnd
                    , i = this.scrollListEl;
                if (i) {
                    if (this._cancelScroll && (this._cancelScroll(),
                        delete this._cancelScroll),
                        this.startScrollByAuto(),
                        !e.enabledAnimation || (0,
                            y.A)().fps < v.uE || C.ie())
                        return this.setScrollTop(t),
                            void (0,
                                y.A)().requestAnimationFrame(() => {
                                    i.scrollTop = t,
                                        this.finishScrollByAuto()
                                }
                                );
                    this._cancelScroll = S.scrollTo(i, t, v.ap, b.easeOutCubic, () => {
                        this.setScrollTop(t),
                            this.finishScrollByAuto()
                    }
                    )
                }
            }
            setScrollListEl(e) {
                this.scrollListEl = e
            }
            willLoad(e) {
                return me(this, null, function* () {
                    this._startIntervalToPopChatQue(),
                        this.stores = e;
                    const t = (0,
                        p.Ay)().getMoviePagePlayMode();
                    this.isChatVisible = t.isChatVisible || !1
                })
            }
            willUnload() {
                (0,
                    y.A)().clearInterval(this._chatAddIntervalId),
                    (0,
                        y.A)().cancelAnimationFrame(this._chatQueAnimationFrame),
                    this.scrollListEl = null,
                    this._chatAddIntervalId = 0,
                    this._chatQueAnimationFrame = 0,
                    this._isLoadedChatModerator = !1,
                    this.resetChatModerators(),
                    delete this.stores
            }
            sendChatModerator(e, t) {
                return me(this, null, function* () {
                    let i;
                    try {
                        const s = ((e, t) => {
                            for (var i in t || (t = {}))
                                he.call(t, i) && ce(e, i, t[i]);
                            if (de)
                                for (var i of de(t))
                                    ue.call(t, i) && ce(e, i, t[i]);
                            return e
                        }
                        )({}, t);
                        this.stores && this.stores.moviePageStore.movieStore.isLiveStreaming && (s.messagedAt = this.stores.appStore.currentPlayerInfo.programDateTime),
                            i = yield (0,
                                ae.f5)(e, s)
                    } catch (e) {
                        console.error(e),
                            this._showErrorMessage(e.message)
                    }
                    if (i && i.message) {
                        if (i.status === v.bo.CHAT_USER_KEY_NOT_EXISTS) {
                            if (this.stores && this.stores.langStore.chat.chatIdRequired) {
                                const e = this.stores.langStore.chat.chatIdRequired.replace(/%s/, `${(0,
                                    g.A)().urls.tvDomain}/profile/profile_user_information`);
                                return void this._showErrorMessage(e, "warning")
                            }
                            return void this._showErrorMessage(i.message, "warning")
                        }
                        if (i.status === v.bo.COMMON)
                            return void this._showErrorMessage(i.message, "warning");
                        i.status === v.bo.CHAT_SETTING && (this._showErrorMessage(i.message, "warning"),
                            this.chatSettingStore.setMenuType("rule"),
                            this.chatSettingStore.showMenu())
                    }
                })
            }
            updateChatModerator(e, t, i) {
                return me(this, null, function* () {
                    let s;
                    try {
                        if (s = yield (0,
                            ae.cR)(e, t, i),
                            s && s.message) {
                            if (s.status === v.bo.CHAT_USER_KEY_NOT_EXISTS) {
                                if (this.stores && this.stores.langStore.chat.chatIdRequired) {
                                    const e = this.stores.langStore.chat.chatIdRequired.replace(/%s/, `${(0,
                                        g.A)().urls.tvDomain}/profile/profile_user_information`);
                                    return this._showErrorMessage(e, "warning"),
                                        s
                                }
                                this._showErrorMessage(s.message, "warning")
                            } else
                                s.status === v.bo.COMMON ? this._showErrorMessage(s.message, "warning") : s.status === v.bo.CHAT_SETTING && (this._showErrorMessage(s.message, "warning"),
                                    this.chatSettingStore.setMenuType("rule"),
                                    this.chatSettingStore.showMenu());
                            return s
                        }
                        if (this.stores && this.stores.moviePageStore.movieStore.isArchive) {
                            const e = s.data
                                , t = this._toChatPropsFromLegacy(e.items[0]);
                            return this.updatedChatModerator(t),
                                s
                        }
                    } catch (e) {
                        this.stores && this._showErrorMessage(this.stores.langStore.chat.chatModeratorError)
                    }
                    return s
                })
            }
            updatedChatModerator(e) {
                return me(this, null, function* () {
                    let t = !0;
                    const i = this.chatModerators.map(i => {
                        if (i.id !== e.id)
                            return i;
                        const s = i.message === e.message
                            , o = I.parse(i.postedAt || 0).getTime() === I.parse(e.postedAt || 0).getTime();
                        return s && o && e.isMessageHidden ? (t = !1,
                            i) : e
                    }
                    );
                    return !!t && (i.sort((e, t) => {
                        const i = I.parse(e.postedAt || 0).getTime()
                            , s = I.parse(t.postedAt || 0).getTime();
                        return i < s ? 1 : i > s ? -1 : e.id < t.id ? 1 : e.id > t.id ? -1 : 0
                    }
                    ),
                        this.chatModerators.replace(i),
                        !0)
                })
            }
            updateHiddenChatModerator(e) {
                return me(this, null, function* () {
                    let t;
                    try {
                        const i = {
                            isHidden: !0
                        };
                        if (t = yield (0,
                            ae.oT)(e, i),
                            t && t.message)
                            return t.status === v.bo.COMMON ? this._showErrorMessage(t.message, "warning") : t.status === v.bo.CHAT_SETTING && (this._showErrorMessage(t.message, "warning"),
                                this.chatSettingStore.setMenuType("rule"),
                                this.chatSettingStore.showMenu()),
                                t
                    } catch (e) {
                        this.stores && this._showErrorMessage(this.stores.langStore.chat.chatModeratorEndedError)
                    }
                    return t
                })
            }
            deleteChatModerator(e, t) {
                return me(this, null, function* () {
                    try {
                        yield (0,
                            ae.QL)(e, t.id)
                    } catch (e) {
                        this.stores && this._showErrorMessage(this.stores.langStore.chat.chatModeratorDeleteError)
                    }
                })
            }
            deleteChatModeratorByChatModeratorId(e) {
                return me(this, null, function* () {
                    const t = this.chatModerators.find(t => t.id === e);
                    t && this.chatModerators.remove(t)
                })
            }
            deleteChatModeratorFromDeleteChat(e) {
                const t = this.chatModerators.find(t => t.chatId === e);
                t && this.chatModerators.remove(t)
            }
            resetChatModerators() {
                return me(this, null, function* () {
                    this.chatModerators.clear(),
                        this.chatModeratorQue = [],
                        this.forcedScrollBottom = !0,
                        this._isLoadedChatModerator || this.setLatestMessage(null)
                })
            }
            fetchChatsAndUpdate(e) {
                return me(this, arguments, function* (e, t = {}) {
                    let i;
                    this.chatLoading = !0;
                    try {
                        i = yield (0,
                            k.r)(() => function (e, t = {}) {
                                return (0,
                                    k.A)("GET", `/movies/${e}/chat-moderators`, {
                                        query: t,
                                        body: void 0
                                    })
                            }(e, {
                                isLatest: t.isLatest
                            }), {
                                c: t.cacheBuster ? t.cacheBuster : void 0
                            })
                    } catch (e) {
                        return void (0,
                            o.h5)(() => {
                                console.error(e),
                                    t.isReset && this.resetChatModerators(),
                                    this.chatLoading = !1
                            }
                            )
                    }
                    (0,
                        o.h5)(() => {
                            this.isCompletedInicialFetch = !0;
                            const e = !!this.latestTelop.message;
                            t.isReset && this.resetChatModerators();
                            const s = i.map(e => this._toChatProps(e));
                            if (this.chatLoading = !1,
                                t.isReset ? this.chatModerators.replace(s) : this.chatModerators.replace(s.concat(this.chatModerators)),
                                this._isLoadedChatModerator && !e)
                                return;
                            const o = s[0];
                            this._setLatestChatModerator(o),
                                this._isLoadedChatModerator = !0
                        }
                        )
                })
            }
            showedChatModerator() {
                this.setLatestMessage(null)
            }
            showBlacklistedChatModerator(e) {
                this.chatModerators.forEach(t => {
                    t.id === e && (t.isBlacklist = !1)
                }
                )
            }
            setLatestMessage(e, t, i) {
                this.latestTelop.message = e,
                    this.latestTelop.id = t || 0,
                    this.latestTelop.linkUrl = i,
                    this.stores && this.stores.moviePageStore.changeChatModeratorForTheaterMode()
            }
            setForcedScrollBottom(e) {
                this.forcedScrollBottom = e
            }
            setScrolledByAuto(e) {
                this.isScrolledByAuto = e
            }
            setForcedAutoScrollCount(e) {
                this.forcedAutoScrollCount = e
            }
            canPopChatQue() {
                return !(this.forcedScrollBottom || this.stores && this.stores.moviePageStore.isTheaterMode || this.isScrollingByAuto)
            }
            startScrollByAuto() {
                this.isScrollingByAuto = !0
            }
            finishScrollByAuto() {
                this.isScrollingByAuto = !1
            }
            addChatModeratorQue(e) {
                this.chatModeratorQue = e.concat(this.chatModeratorQue)
            }
            popChatQue() {
                if (!this.chatModeratorQue.length)
                    return;
                const e = this.chatModeratorQue.concat(this.chatModerators.slice());
                if (this.isScrollBottom()) {
                    const t = e.splice(0, v.o_)
                        , i = e.filter(e => !!e.yell || this._isOwnerChat(e));
                    this.chatModerators.replace(i.concat(t)),
                        this.chatModeratorQue = []
                } else {
                    const t = e.splice(-v.H5);
                    this.chatModerators.replace(t),
                        this.chatModeratorQue = e.slice(-v.Nz)
                }
            }
            scrollByAuto() {
                if (this.scrollListEl)
                    return this.forcedScrollBottom ? (this.scrollListEl.scrollTop = this.scrollEnd,
                        void this.setForcedScrollBottom(!1)) : this.hasBottomChatListDiffrence() ? this.forcedAutoScrollCount ? (this.setForcedAutoScrollCount(this.forcedAutoScrollCount - 1),
                            void this.scrollToEnd({
                                enabledAnimation: !0
                            })) : (this.isScrolledByAuto || (this.setScrolledByAuto(!0),
                                this.setForcedAutoScrollCount(2),
                                setTimeout(() => {
                                    this.setForcedAutoScrollCount(0)
                                }
                                    , 1500)),
                                void this.scrollToEnd({
                                    enabledAnimation: !0
                                })) : void this.setScrolledByAuto(!1)
            }
            setPreLastChatCellId(e) {
                this._preLastChatCellId = e
            }
            hasBottomChatListDiffrence() {
                const e = this.chatModerators[this.chatModerators.length - 1];
                return !!e && this._preLastChatCellId !== e.id
            }
            _startIntervalToPopChatQue() {
                let e = v.ap + v.vt;
                (C.ie() || C.safari()) && (e *= 4),
                    this._chatAddIntervalId = (0,
                        y.A)().setInterval(() => {
                            this._chatQueAnimationFrame && ((0,
                                y.A)().cancelAnimationFrame(this._chatQueAnimationFrame),
                                this._chatQueAnimationFrame = 0),
                                this._chatQueAnimationFrame = (0,
                                    y.A)().requestAnimationFrame(() => {
                                        this.popChatQue()
                                    }
                                    )
                        }
                            , e)
            }
            _toChatPropsFromLegacy(e) {
                const t = e.user || {}
                    , i = e.chat_setting || {};
                return {
                    id: e.id || 0,
                    message: e.message || "",
                    postedAt: e.posted_at || "",
                    chatId: e.chat_id || 0,
                    userId: t.id || "",
                    recxuserId: t.recxuser_id || 0,
                    userIconImageUrl: t.icon_image_url || "",
                    userCoverImageUrl: t.cover_image_url || "",
                    userName: t.nickname || "",
                    userColor: i.name_color || "",
                    isOfficial: !!t.is_official,
                    isFresh: !!t.is_fresh,
                    isPremium: !!t.is_premium,
                    isPremiumHidden: !!i.is_premium_hidden,
                    isSubsBadgeHidden: !!i.is_subs_badge_hidden,
                    isSubsDurationHidden: !!i.is_subs_duration_hidden,
                    isWarned: !!t.is_warned,
                    isModerator: !!e.is_moderating,
                    isLowLatency: !!e.quality_type && e.quality_type === v.lT.LOW_LATENCY,
                    isMessageHidden: !!e.is_hidden
                }
            }
            _toChatProps(e) {
                var t, i, s, o, r, a, n, l, d, h, u, c, p;
                return {
                    id: e.id || 0,
                    message: e.message || "",
                    postedAt: e.postedAt || "",
                    chatId: e.chatId || 0,
                    linkUrl: e.linkUrl || "",
                    userId: (null == (t = e.user) ? void 0 : t.id) || "",
                    recxuserId: (null == (i = e.user) ? void 0 : i.recxuserId) || 0,
                    userIconImageUrl: (null == (s = e.user) ? void 0 : s.iconImageUrl) || "",
                    userCoverImageUrl: (null == (o = e.user) ? void 0 : o.coverImageUrl) || "",
                    userName: (null == (r = e.user) ? void 0 : r.nickname) || "",
                    userColor: (null == (a = e.chatSetting) ? void 0 : a.nameColor) || "",
                    isOfficial: !!(null == (n = e.user) ? void 0 : n.isOfficial),
                    isFresh: !!(null == (l = e.user) ? void 0 : l.isFresh),
                    isPremium: !!(null == (d = e.user) ? void 0 : d.isPremium),
                    isPremiumHidden: !!(null == (h = e.chatSetting) ? void 0 : h.isPremiumHidden),
                    isSubsBadgeHidden: !!(null == (u = e.chatSetting) ? void 0 : u.isSubsBadgeHidden),
                    isSubsDurationHidden: !!(null == (c = e.chatSetting) ? void 0 : c.isSubsDurationHidden),
                    isWarned: !!(null == (p = e.user) ? void 0 : p.isWarned),
                    isModerator: !!e.isModerating,
                    isLowLatency: !!e.qualityType && e.qualityType === v.lT.LOW_LATENCY,
                    isMessageHidden: !!e.isHidden
                }
            }
            _isOwnerChat(e) {
                return !!this.stores && this.stores.moviePageStore.movieStore.channel.id === e.userId
            }
            _setLatestChatModerator(e) {
                e && (e.isMessageHidden ? this.setLatestMessage(null) : this.setLatestMessage(e.message || null, e.chatId, e.linkUrl))
            }
            _showErrorMessage(e = "", t) {
                x.A.addMessage({
                    variant: t || "danger",
                    text: e
                })
            }
        }
        pe([o.sH], ye.prototype, "chatModerators", 2),
            pe([o.sH], ye.prototype, "latestTelop", 2),
            pe([o.sH], ye.prototype, "moderators", 2),
            pe([o.sH], ye.prototype, "chatScrollEnable", 2),
            pe([o.sH], ye.prototype, "isLastPage", 2),
            pe([o.sH], ye.prototype, "isChatVisible", 2),
            pe([o.sH], ye.prototype, "isCompletedInicialFetch", 2),
            pe([o.sH], ye.prototype, "enabledChatInDvr", 2),
            pe([o.sH], ye.prototype, "isScrollingByAuto", 2),
            pe([o.XI], ye.prototype, "willLoad", 1),
            pe([o.XI], ye.prototype, "willUnload", 1),
            pe([o.XI], ye.prototype, "sendChatModerator", 1),
            pe([o.XI.bound], ye.prototype, "updateChatModerator", 1),
            pe([o.XI], ye.prototype, "updatedChatModerator", 1),
            pe([o.XI.bound], ye.prototype, "updateHiddenChatModerator", 1),
            pe([o.XI.bound], ye.prototype, "deleteChatModerator", 1),
            pe([o.XI], ye.prototype, "deleteChatModeratorByChatModeratorId", 1),
            pe([o.XI], ye.prototype, "deleteChatModeratorFromDeleteChat", 1),
            pe([o.XI.bound], ye.prototype, "resetChatModerators", 1),
            pe([o.XI], ye.prototype, "fetchChatsAndUpdate", 1),
            pe([o.XI.bound], ye.prototype, "showedChatModerator", 1),
            pe([o.XI.bound], ye.prototype, "showBlacklistedChatModerator", 1),
            pe([o.XI], ye.prototype, "setLatestMessage", 1),
            pe([o.XI], ye.prototype, "setForcedScrollBottom", 1),
            pe([o.XI.bound], ye.prototype, "startScrollByAuto", 1),
            pe([o.XI.bound], ye.prototype, "finishScrollByAuto", 1),
            pe([o.XI], ye.prototype, "addChatModeratorQue", 1),
            pe([o.XI.bound], ye.prototype, "popChatQue", 1);
        var ge = i(86254)
            , ve = i(83279)
            , Se = i(98013)
            , be = i(45452)
            , fe = Object.defineProperty
            , Ce = Object.defineProperties
            , Ie = Object.getOwnPropertyDescriptor
            , Ae = Object.getOwnPropertyDescriptors
            , _e = Object.getOwnPropertySymbols
            , Pe = Object.prototype.hasOwnProperty
            , Me = Object.prototype.propertyIsEnumerable
            , we = (e, t, i) => t in e ? fe(e, t, {
                enumerable: !0,
                configurable: !0,
                writable: !0,
                value: i
            }) : e[t] = i
            , Te = (e, t, i, s) => {
                for (var o, r = s > 1 ? void 0 : s ? Ie(t, i) : t, a = e.length - 1; a >= 0; a--)
                    (o = e[a]) && (r = (s ? o(t, i, r) : o(r)) || r);
                return s && r && fe(t, i, r),
                    r
            }
            , Ee = (e, t, i) => new Promise((s, o) => {
                var r = e => {
                    try {
                        n(i.next(e))
                    } catch (e) {
                        o(e)
                    }
                }
                    , a = e => {
                        try {
                            n(i.throw(e))
                        } catch (e) {
                            o(e)
                        }
                    }
                    , n = e => e.done ? s(e.value) : Promise.resolve(e.value).then(r, a);
                n((i = i.apply(e, t)).next())
            }
            );
        class Ue {
            constructor() {
                this.menuType = "default",
                    this.menuOption = {},
                    this.menuVisible = !1,
                    this.delayChat = !1,
                    this.muteFreshUser = !1,
                    this.muteWarnedUser = !1,
                    this.muteForbiddenWord = !1,
                    this.muteUnAuthenticatedUser = !1,
                    this.isPremiumHidden = !1,
                    this.isOfficialHidden = !1,
                    this.userColor = "",
                    this.limitedContinuousChat = !1,
                    this.continuousChatThreshold = 0,
                    this.limitedUnfollowerChat = !1,
                    this.unfollowerChatThreshold = 0,
                    this.limitedFreshUserChat = !1,
                    this.freshUserChatThreshold = 0,
                    this.limitedTemporaryBlacklist = !1,
                    this.temporaryBlacklistThreshold = 0,
                    this.limitedWarnedUserChat = !1,
                    this.isChatTermsRequired = !1,
                    this.isMutedBannedWord = !1,
                    this.chatRule = "",
                    this.isSmallSizeStamp = !1,
                    this.isFixedPhraseHidden = !1,
                    this.isDeclinedSubsShare = !1,
                    this.isSubsBadgeHidden = !1,
                    this.isSubsDurationHidden = !1,
                    this.isSubsMembershipCardHidden = !1,
                    this.limitedUnsubsMemberChat = !1,
                    (0,
                        o.Gn)(this)
            }
            willLoad(e) {
                return Ee(this, null, function* () {
                    this.setChatTermsRequired(!!e.moviePageStore.movieStore.settings.isChatTermsRequired)
                })
            }
            willUnload() {
                this.hideMenu()
            }
            updateMovieDetail(e, t) {
                (null == t ? void 0 : t.chatSetting) ? this.updateChatSetting(this.toChatSettings(t.chatSetting)) : e.userStore.user.recxuserId && this.updateUserColor((0,
                    ve.toUserColor)(e.userStore.user.recxuserId))
            }
            updateChatSetting(e) {
                this.limitedContinuousChat = e.limitedContinuousChat,
                    this.limitedFreshUserChat = e.limitedFreshUserChat,
                    this.limitedTemporaryBlacklist = e.limitedTemporaryBlacklist,
                    this.limitedUnfollowerChat = e.limitedUnfollowerChat,
                    this.limitedWarnedUserChat = e.limitedWarnedUserChat,
                    this.unfollowerChatThreshold = e.unfollowerChatThreshold,
                    this.continuousChatThreshold = e.continuousChatThreshold,
                    this.freshUserChatThreshold = e.freshUserChatThreshold,
                    this.isSmallSizeStamp = !!e.isSmallSizeStamp,
                    this.temporaryBlacklistThreshold = e.temporaryBlacklistThreshold,
                    this.chatRule = e.chatRule,
                    this.delayChat = !!e.delayChat,
                    this.isPremiumHidden = !!e.isPremiumHidden,
                    this.isOfficialHidden = !!e.isOfficialHidden,
                    this.isSubsBadgeHidden = !!e.isSubsBadgeHidden,
                    this.isSubsDurationHidden = !!e.isSubsDurationHidden,
                    this.isSubsMembershipCardHidden = !!e.isSubsMembershipCardHidden,
                    this.isSmallSizeStamp = !!e.isSmallSizeStamp,
                    this.isFixedPhraseHidden = !!e.isFixedPhraseHidden,
                    this.userColor = e.userColor || "",
                    this.muteFreshUser = !!e.muteFreshUser,
                    this.muteWarnedUser = !!e.muteWarnedUser,
                    this.muteForbiddenWord = !!e.muteForbiddenWord,
                    this.muteUnAuthenticatedUser = !!e.muteUnAuthenticatedUser,
                    this.limitedUnsubsMemberChat = !!e.limitedUnsubsMemberChat,
                    this.isDeclinedSubsShare = !!e.declinedSubsShare,
                    this._preChatSetting = e
            }
            toChatSettings(e) {
                return {
                    delayChat: !!e.adjustChatDelay,
                    muteFreshUser: !!e.mutedFreshUser,
                    muteWarnedUser: !!e.mutedWarnedUser,
                    muteForbiddenWord: !!e.mutedBannedWord,
                    muteUnAuthenticatedUser: !!e.mutedUnauthenticatedUser,
                    limitedContinuousChat: !!e.limitedContinuousChat,
                    limitedFreshUserChat: !!e.limitedFreshUserChat,
                    limitedUnsubsMemberChat: !!e.limitedUnsubsMemberChat,
                    limitedTemporaryBlacklist: !!e.limitedTemporaryBlacklist,
                    limitedUnfollowerChat: !!e.limitedUnfollowerChat,
                    limitedWarnedUserChat: !!e.limitedWarnedUserChat,
                    isPremiumHidden: !!e.isPremiumHidden,
                    isOfficialHidden: !!e.isOfficialHidden,
                    userColor: e.nameColor ? e.nameColor : "",
                    chatRule: e.chatRule ? e.chatRule : "",
                    unfollowerChatThreshold: e.unfollowerChatThreshold || v.qe.ALL,
                    continuousChatThreshold: e.continuousChatThreshold || v.qe.ONE_MINUTES,
                    freshUserChatThreshold: e.freshUserChatThreshold || v.qe.TEN_MINUITES,
                    isSmallSizeStamp: !!e.isSmallSizeStamp,
                    temporaryBlacklistThreshold: e.temporaryBlacklistThreshold || v.qe.TEN_MINUITES,
                    isFixedPhraseHidden: !!e.isFixedPhraseHidden,
                    isSubsBadgeHidden: !!e.isSubsBadgeHidden,
                    isSubsDurationHidden: !!e.isSubsDurationHidden,
                    isSubsMembershipCardHidden: !!e.isSubsMembershipCardHidden,
                    declinedSubsShare: !!e.declinedSubsShare
                }
            }
            setMenuType(e, t = {}) {
                this.menuType = e,
                    this.menuOption = t
            }
            showMenu() {
                this.menuVisible = !0
            }
            hideMenu() {
                this.menuVisible = !1
            }
            updateChatRules() {
                return Ee(this, null, function* () {
                    const e = {
                        adjustChatDelay: this.delayChat,
                        mutedFreshUser: this.muteFreshUser,
                        mutedWarnedUser: this.muteWarnedUser,
                        mutedBannedWord: this.muteForbiddenWord,
                        limitedContinuousChat: this.limitedContinuousChat,
                        limitedFreshUserChat: this.limitedFreshUserChat,
                        limitedTemporaryBlacklist: this.limitedTemporaryBlacklist,
                        limitedUnfollowerChat: this.limitedUnfollowerChat,
                        limitedUnsubsMemberChat: this.limitedUnsubsMemberChat,
                        limitedWarnedUserChat: this.limitedWarnedUserChat,
                        isPremiumHidden: this.isPremiumHidden,
                        isOfficialHidden: this.isOfficialHidden,
                        isSubsBadgeHidden: this.isSubsBadgeHidden,
                        isSubsDurationHidden: this.isSubsDurationHidden,
                        isSubsMembershipCardHidden: this.isSubsMembershipCardHidden,
                        chatRule: this.chatRule,
                        unfollowerChatThreshold: this.unfollowerChatThreshold,
                        continuousChatThreshold: this.continuousChatThreshold,
                        freshUserChatThreshold: this.freshUserChatThreshold,
                        isSmallSizeStamp: this.isSmallSizeStamp,
                        temporaryBlacklistThreshold: this.temporaryBlacklistThreshold,
                        declinedSubsShare: this.isDeclinedSubsShare
                    };
                    var t, i;
                    this.userColor && (e.nameColor = this.userColor),
                        (0,
                            Se.QY)(e),
                        this.updateChatSetting((t = ((e, t) => {
                            for (var i in t || (t = {}))
                                Pe.call(t, i) && we(e, i, t[i]);
                            if (_e)
                                for (var i of _e(t))
                                    Me.call(t, i) && we(e, i, t[i]);
                            return e
                        }
                        )({}, e),
                            i = {
                                userColor: this.userColor
                            },
                            Ce(t, Ae(i))))
                })
            }
            updateChatSettings() {
                return Ee(this, null, function* () {
                    try {
                        yield be.A.updateChatSettings(!!this.isChatTermsRequired, !!this.isMutedBannedWord)
                    } catch (e) {
                        (0,
                            o.h5)(() => {
                                this.isChatTermsRequired = !this.isChatTermsRequired,
                                    this.isMutedBannedWord = !this.isMutedBannedWord
                            }
                            )
                    }
                })
            }
            setChatTermsRequired(e) {
                return Ee(this, null, function* () {
                    this.isChatTermsRequired = e
                })
            }
            setMutedBannedWord(e) {
                return Ee(this, null, function* () {
                    this.isMutedBannedWord = e
                })
            }
            setChatRules(e) {
                (0,
                    ge.extendDeepWith)(this, e)
            }
            revertChatSetting() {
                this._preChatSetting && this.updateChatSetting(this._preChatSetting)
            }
            updateChatRuleText(e) {
                this.chatRule = e
            }
            setMuteWarnedUser() {
                return Ee(this, null, function* () {
                    this.muteWarnedUser = !0;
                    try {
                        yield (0,
                            Se.QY)({
                                mutedWarnedUser: !0
                            })
                    } catch (e) {
                        (0,
                            o.h5)(() => {
                                this.muteWarnedUser = !1
                            }
                            )
                    }
                })
            }
            unsetMuteWarnedUser() {
                return Ee(this, null, function* () {
                    this.muteWarnedUser = !1;
                    try {
                        yield (0,
                            Se.QY)({
                                mutedWarnedUser: !1
                            })
                    } catch (e) {
                        (0,
                            o.h5)(() => {
                                this.muteWarnedUser = !0
                            }
                            )
                    }
                })
            }
            setMuteFreshUser() {
                return Ee(this, null, function* () {
                    this.muteFreshUser = !0;
                    try {
                        yield (0,
                            Se.QY)({
                                mutedFreshUser: !0
                            })
                    } catch (e) {
                        (0,
                            o.h5)(() => {
                                this.muteWarnedUser = !1
                            }
                            )
                    }
                })
            }
            unsetMuteFreshUser() {
                return Ee(this, null, function* () {
                    this.muteFreshUser = !1;
                    try {
                        yield (0,
                            Se.QY)({
                                mutedFreshUser: !1
                            })
                    } catch (e) {
                        (0,
                            o.h5)(() => {
                                this.muteFreshUser = !0
                            }
                            )
                    }
                })
            }
            setMuteForbiddenWord() {
                return Ee(this, null, function* () {
                    this.muteForbiddenWord = !0;
                    try {
                        yield (0,
                            Se.QY)({
                                mutedBannedWord: !0
                            })
                    } catch (e) {
                        (0,
                            o.h5)(() => {
                                this.muteForbiddenWord = !1
                            }
                            )
                    }
                })
            }
            unsetMuteForbiddenWord() {
                return Ee(this, null, function* () {
                    this.muteForbiddenWord = !1;
                    try {
                        yield (0,
                            Se.QY)({
                                mutedBannedWord: !1
                            })
                    } catch (e) {
                        (0,
                            o.h5)(() => {
                                this.muteForbiddenWord = !0
                            }
                            )
                    }
                })
            }
            setDelayChat() {
                return Ee(this, null, function* () {
                    this.delayChat = !0;
                    try {
                        yield (0,
                            Se.QY)({
                                adjustChatDelay: !0
                            })
                    } catch (e) {
                        (0,
                            o.h5)(() => {
                                this.delayChat = !1
                            }
                            )
                    }
                })
            }
            unsetDelayChat() {
                return Ee(this, null, function* () {
                    this.delayChat = !1;
                    try {
                        yield (0,
                            Se.QY)({
                                adjustChatDelay: !1
                            })
                    } catch (e) {
                        (0,
                            o.h5)(() => {
                                this.delayChat = !0
                            }
                            )
                    }
                })
            }
            setSmallSizeStamp() {
                return Ee(this, null, function* () {
                    this.isSmallSizeStamp = !0;
                    try {
                        yield (0,
                            Se.QY)({
                                isSmallSizeStamp: !0
                            })
                    } catch (e) {
                        (0,
                            o.h5)(() => {
                                this.isSmallSizeStamp = !1
                            }
                            )
                    }
                })
            }
            unsetSmallSizeStamp() {
                return Ee(this, null, function* () {
                    this.isSmallSizeStamp = !1;
                    try {
                        yield (0,
                            Se.QY)({
                                isSmallSizeStamp: !1
                            })
                    } catch (e) {
                        (0,
                            o.h5)(() => {
                                this.isSmallSizeStamp = !0
                            }
                            )
                    }
                })
            }
            setMuteUnAuthenticatedUser() {
                return Ee(this, null, function* () {
                    this.muteUnAuthenticatedUser = !0;
                    try {
                        yield (0,
                            Se.QY)({
                                mutedUnauthenticatedUser: !0
                            })
                    } catch (e) {
                        (0,
                            o.h5)(() => {
                                this.muteForbiddenWord = !1
                            }
                            )
                    }
                })
            }
            unsetMuteUnAuthenticatedUser() {
                return Ee(this, null, function* () {
                    this.muteUnAuthenticatedUser = !1;
                    try {
                        yield (0,
                            Se.QY)({
                                mutedUnauthenticatedUser: !1
                            })
                    } catch (e) {
                        (0,
                            o.h5)(() => {
                                this.muteForbiddenWord = !0
                            }
                            )
                    }
                })
            }
            setHiddenFixedPhrase() {
                return Ee(this, null, function* () {
                    this.isFixedPhraseHidden = !0;
                    try {
                        yield (0,
                            Se.QY)({
                                isFixedPhraseHidden: !0
                            })
                    } catch (e) {
                        (0,
                            o.h5)(() => {
                                this.isFixedPhraseHidden = !1
                            }
                            )
                    }
                })
            }
            unsetHiddenFixedPhrase() {
                return Ee(this, null, function* () {
                    this.isFixedPhraseHidden = !1;
                    try {
                        yield (0,
                            Se.QY)({
                                isFixedPhraseHidden: !1
                            })
                    } catch (e) {
                        (0,
                            o.h5)(() => {
                                this.isFixedPhraseHidden = !0
                            }
                            )
                    }
                })
            }
            setDeclinedSubsShare() {
                return Ee(this, null, function* () {
                    this.isDeclinedSubsShare = !0;
                    try {
                        yield (0,
                            Se.QY)({
                                declinedSubsShare: !0
                            })
                    } catch (e) {
                        (0,
                            o.h5)(() => {
                                this.isDeclinedSubsShare = !1
                            }
                            )
                    }
                })
            }
            unsetDeclinedSubsShare() {
                return Ee(this, null, function* () {
                    this.isDeclinedSubsShare = !1;
                    try {
                        yield (0,
                            Se.QY)({
                                declinedSubsShare: !1
                            })
                    } catch (e) {
                        (0,
                            o.h5)(() => {
                                this.isDeclinedSubsShare = !1
                            }
                            )
                    }
                })
            }
            showSubsBadge() {
                this.isSubsBadgeHidden = !1
            }
            hideSubsBadge() {
                this.isSubsBadgeHidden = !0
            }
            showSubsDuration() {
                this.isSubsDurationHidden = !1
            }
            hideSubsDuration() {
                this.isSubsDurationHidden = !0
            }
            showSubsMembersCard() {
                this.isSubsMembershipCardHidden = !1
            }
            hideSubsMembersCard() {
                this.isSubsMembershipCardHidden = !0
            }
            showOfficial() {
                this.isOfficialHidden = !1
            }
            hideOfficial() {
                this.isOfficialHidden = !0
            }
            updateUserColor(e) {
                this.userColor = e
            }
            hidePremium() {
                this.isPremiumHidden = !0
            }
            showPremium() {
                this.isPremiumHidden = !1
            }
            setLimitedUnsubsMemberChat(e) {
                return Ee(this, null, function* () {
                    this.limitedUnsubsMemberChat = e
                })
            }
        }
        Te([o.sH], Ue.prototype, "menuType", 2),
            Te([o.sH], Ue.prototype, "menuOption", 2),
            Te([o.sH], Ue.prototype, "menuVisible", 2),
            Te([o.sH], Ue.prototype, "delayChat", 2),
            Te([o.sH], Ue.prototype, "muteFreshUser", 2),
            Te([o.sH], Ue.prototype, "muteWarnedUser", 2),
            Te([o.sH], Ue.prototype, "muteForbiddenWord", 2),
            Te([o.sH], Ue.prototype, "muteUnAuthenticatedUser", 2),
            Te([o.sH], Ue.prototype, "isPremiumHidden", 2),
            Te([o.sH], Ue.prototype, "isOfficialHidden", 2),
            Te([o.sH], Ue.prototype, "userColor", 2),
            Te([o.sH], Ue.prototype, "limitedContinuousChat", 2),
            Te([o.sH], Ue.prototype, "continuousChatThreshold", 2),
            Te([o.sH], Ue.prototype, "limitedUnfollowerChat", 2),
            Te([o.sH], Ue.prototype, "unfollowerChatThreshold", 2),
            Te([o.sH], Ue.prototype, "limitedFreshUserChat", 2),
            Te([o.sH], Ue.prototype, "freshUserChatThreshold", 2),
            Te([o.sH], Ue.prototype, "limitedTemporaryBlacklist", 2),
            Te([o.sH], Ue.prototype, "temporaryBlacklistThreshold", 2),
            Te([o.sH], Ue.prototype, "limitedWarnedUserChat", 2),
            Te([o.sH], Ue.prototype, "isChatTermsRequired", 2),
            Te([o.sH], Ue.prototype, "isMutedBannedWord", 2),
            Te([o.sH], Ue.prototype, "chatRule", 2),
            Te([o.sH], Ue.prototype, "isSmallSizeStamp", 2),
            Te([o.sH], Ue.prototype, "isFixedPhraseHidden", 2),
            Te([o.sH], Ue.prototype, "isDeclinedSubsShare", 2),
            Te([o.sH], Ue.prototype, "isSubsBadgeHidden", 2),
            Te([o.sH], Ue.prototype, "isSubsDurationHidden", 2),
            Te([o.sH], Ue.prototype, "isSubsMembershipCardHidden", 2),
            Te([o.sH], Ue.prototype, "limitedUnsubsMemberChat", 2),
            Te([o.XI], Ue.prototype, "willLoad", 1),
            Te([o.XI], Ue.prototype, "willUnload", 1),
            Te([o.XI.bound], Ue.prototype, "updateMovieDetail", 1),
            Te([o.XI.bound], Ue.prototype, "updateChatSetting", 1),
            Te([o.XI.bound], Ue.prototype, "toChatSettings", 1),
            Te([o.XI.bound], Ue.prototype, "setMenuType", 1),
            Te([o.XI.bound], Ue.prototype, "showMenu", 1),
            Te([o.XI.bound], Ue.prototype, "hideMenu", 1),
            Te([o.XI.bound], Ue.prototype, "updateChatRules", 1),
            Te([o.XI.bound], Ue.prototype, "updateChatSettings", 1),
            Te([o.XI.bound], Ue.prototype, "setChatTermsRequired", 1),
            Te([o.XI.bound], Ue.prototype, "setMutedBannedWord", 1),
            Te([o.XI.bound], Ue.prototype, "setChatRules", 1),
            Te([o.XI.bound], Ue.prototype, "revertChatSetting", 1),
            Te([o.XI.bound], Ue.prototype, "updateChatRuleText", 1),
            Te([o.XI.bound], Ue.prototype, "setMuteWarnedUser", 1),
            Te([o.XI.bound], Ue.prototype, "unsetMuteWarnedUser", 1),
            Te([o.XI.bound], Ue.prototype, "setMuteFreshUser", 1),
            Te([o.XI.bound], Ue.prototype, "unsetMuteFreshUser", 1),
            Te([o.XI.bound], Ue.prototype, "setMuteForbiddenWord", 1),
            Te([o.XI.bound], Ue.prototype, "unsetMuteForbiddenWord", 1),
            Te([o.XI.bound], Ue.prototype, "setDelayChat", 1),
            Te([o.XI.bound], Ue.prototype, "unsetDelayChat", 1),
            Te([o.XI.bound], Ue.prototype, "setSmallSizeStamp", 1),
            Te([o.XI.bound], Ue.prototype, "unsetSmallSizeStamp", 1),
            Te([o.XI.bound], Ue.prototype, "setMuteUnAuthenticatedUser", 1),
            Te([o.XI.bound], Ue.prototype, "unsetMuteUnAuthenticatedUser", 1),
            Te([o.XI.bound], Ue.prototype, "setHiddenFixedPhrase", 1),
            Te([o.XI.bound], Ue.prototype, "unsetHiddenFixedPhrase", 1),
            Te([o.XI.bound], Ue.prototype, "setDeclinedSubsShare", 1),
            Te([o.XI.bound], Ue.prototype, "unsetDeclinedSubsShare", 1),
            Te([o.XI.bound], Ue.prototype, "showSubsBadge", 1),
            Te([o.XI.bound], Ue.prototype, "hideSubsBadge", 1),
            Te([o.XI.bound], Ue.prototype, "showSubsDuration", 1),
            Te([o.XI.bound], Ue.prototype, "hideSubsDuration", 1),
            Te([o.XI.bound], Ue.prototype, "showSubsMembersCard", 1),
            Te([o.XI.bound], Ue.prototype, "hideSubsMembersCard", 1),
            Te([o.XI.bound], Ue.prototype, "showOfficial", 1),
            Te([o.XI.bound], Ue.prototype, "hideOfficial", 1),
            Te([o.XI.bound], Ue.prototype, "updateUserColor", 1),
            Te([o.XI.bound], Ue.prototype, "hidePremium", 1),
            Te([o.XI.bound], Ue.prototype, "showPremium", 1),
            Te([o.XI.bound], Ue.prototype, "setLimitedUnsubsMemberChat", 1);
        var Le = i(93921)
            , ke = i(92505)
            , He = i(11175)
            , Oe = Object.defineProperty
            , De = Object.getOwnPropertyDescriptor
            , xe = Object.getOwnPropertySymbols
            , Fe = Object.prototype.hasOwnProperty
            , Re = Object.prototype.propertyIsEnumerable
            , Be = (e, t, i) => t in e ? Oe(e, t, {
                enumerable: !0,
                configurable: !0,
                writable: !0,
                value: i
            }) : e[t] = i
            , Xe = (e, t) => {
                for (var i in t || (t = {}))
                    Fe.call(t, i) && Be(e, i, t[i]);
                if (xe)
                    for (var i of xe(t))
                        Re.call(t, i) && Be(e, i, t[i]);
                return e
            }
            , We = (e, t, i, s) => {
                for (var o, r = s > 1 ? void 0 : s ? De(t, i) : t, a = e.length - 1; a >= 0; a--)
                    (o = e[a]) && (r = (s ? o(t, i, r) : o(r)) || r);
                return s && r && Oe(t, i, r),
                    r
            }
            , Ne = (e, t, i) => new Promise((s, o) => {
                var r = e => {
                    try {
                        n(i.next(e))
                    } catch (e) {
                        o(e)
                    }
                }
                    , a = e => {
                        try {
                            n(i.throw(e))
                        } catch (e) {
                            o(e)
                        }
                    }
                    , n = e => e.done ? s(e.value) : Promise.resolve(e.value).then(r, a);
                n((i = i.apply(e, t)).next())
            }
            );
        class Ve {
            constructor() {
                this.comments = o.sH.array(),
                    this.commentCount = 0,
                    this._createIComment = e => {
                        var t, i, s;
                        const o = e.user
                            , r = e.chatSetting
                            , a = this.stores && this.stores.moviePageStore
                            , n = a && a.movieStore
                            , l = {
                                id: e.id || 0,
                                replyId: e.replyId || 0,
                                message: e.message || "",
                                createdAt: e.createdAt || "",
                                isMuted: !!e.isMuted,
                                isOfficial: !!(null == o ? void 0 : o.isOfficial),
                                isOfficialHidden: !!(null == r ? void 0 : r.isOfficialHidden),
                                isPremium: !!(null == o ? void 0 : o.isPremium),
                                isFresh: !!(null == o ? void 0 : o.isFresh),
                                isWarned: !!(null == o ? void 0 : o.isWarned),
                                isPremiumHidden: !!(null == r ? void 0 : r.isPremiumHidden),
                                isSubsBadgeHidden: !!(null == r ? void 0 : r.isSubsBadgeHidden),
                                isSubsDurationHidden: !!(null == r ? void 0 : r.isSubsDurationHidden),
                                isAuthenticated: !0,
                                isModerator: !!e.isModerating,
                                hasBannedWord: !!e.hasBannedWord,
                                userId: (null == o ? void 0 : o.id) || "",
                                userName: (null == o ? void 0 : o.nickname) || "",
                                userIconImageUrl: (null == o ? void 0 : o.iconImageUrl) || "",
                                userCoverImageUrl: (null == o ? void 0 : o.coverImageUrl) || "",
                                userColor: (null == r ? void 0 : r.nameColor) || ""
                            };
                        if (e.stamp) {
                            const t = e.stamp;
                            l.stamp = {
                                id: t.id || 0,
                                imageUrl: t.imageUrl || "",
                                groupId: t.groupId || 0
                            }
                        }
                        if (e.yell) {
                            const o = e.yell;
                            l.yell = {
                                id: o.id || 0,
                                imageUrl: o.imageUrl || "",
                                quantity: o.yells || 0,
                                isLeague: (null == n ? void 0 : n.leagueId) && (null == n ? void 0 : n.enabledYell) && (null == n ? void 0 : n.channel.isLeagueYell) || !1,
                                isCastYell: null == n ? void 0 : n.enabledCastYell,
                                toUser: {
                                    userId: (null == (t = e.toUser) ? void 0 : t.id) || void 0,
                                    userName: (null == (i = e.toUser) ? void 0 : i.nickname) || void 0,
                                    userIconImageUrl: (null == (s = e.toUser) ? void 0 : s.iconImageUrl) || void 0
                                }
                            }
                        }
                        if (e.badges && e.badges[0]) {
                            const t = e.badges[0]
                                , i = t.subscription;
                            l.subsBadge = {
                                id: t.id || 0,
                                imageUrl: t.imageUrl || "",
                                months: (null == i ? void 0 : i.months) || 0,
                                tier: (null == i ? void 0 : i.tier) || 0,
                                label: t.label || ""
                            }
                        }
                        return o && (l.v8User = X.A.createModel(o)),
                            e.chatSetting && (l.chatCommentAppearanceSetting = F.A.createModel(e.chatSetting)),
                            l.userInChannel = {
                                isModerating: e.isModerating || !1,
                                membershipCardUrl: e.membershipCardUrl || ""
                            },
                            l
                    }
                    ,
                    (0,
                        q.H)(this, {
                            commentLoading: o.sH,
                            isLastPage: o.sH,
                            preFirstCommentId: o.sH,
                            openedPalette: o.sH
                        })
            }
            willLoad(e) {
                return Ne(this, null, function* () {
                    this.stores = e
                })
            }
            willUnload() {
                this.comments.clear(),
                    delete this.stores
            }
            _sendComment(e, t) {
                return Ne(this, null, function* () {
                    var i, s;
                    let r;
                    try {
                        const o = yield (0,
                            Le.T9)(e, t);
                        if (o.message)
                            return x.A.addMessage({
                                variant: "danger",
                                text: o.message || ""
                            }),
                                void (o.status === v.bo.CHAT_STAMP_NOT_EXISTS && this.stores && this.stores.userStore.user.recxuserId && t.stampId && (0,
                                    p.Ay)().deleteStampHistory(this.stores.userStore.user.recxuserId, t.stampId));
                        r = null == (s = null == (i = o.data) ? void 0 : i.items) ? void 0 : s[0];
                        const { stamp: a } = null != r ? r : {};
                        if (a && this.stores) {
                            const e = this.stores.userStore
                                , t = (0,
                                    p.Ay)().getStampHistories(e.user.recxuserId || 0)
                                , i = t.findIndex(e => e.id === a.id);
                            if (-1 !== i) {
                                const s = [...t];
                                s[i] = {
                                    id: a.id,
                                    imageUrl: a.imageUrl,
                                    groupId: a.groupId,
                                    stampGroup: s[i].stampGroup,
                                    subsProduct: a.subsProduct
                                },
                                    (0,
                                        p.Ay)().updateStampHistories(e.user.recxuserId || 0, s)
                            }
                        }
                        this.updateCommentCount(this.commentCount + 1)
                    } catch (e) {
                        return e instanceof Error && x.A.addMessage({
                            variant: "danger",
                            text: e.message
                        }),
                            void console.error(e)
                    }
                    (0,
                        o.h5)(() => {
                            this.updateFirstCommentId(),
                                r && this.comments.unshift(this._createIComment(r))
                        }
                        )
                })
            }
            sendCommentAfterCommentTermConsenet(e, t) {
                return Ne(this, null, function* () {
                    if (!this.stores)
                        return;
                    const i = this.stores.dialogStore
                        , s = this.stores.moviePageStore.movieStore;
                    !s.isOwner && s.settings.isChatTermsRequired ? t.message ? i.showDialog({
                        type: "chatTerm",
                        onSubmit: () => {
                            this._sendComment(e, Xe({
                                consentedCommentTerms: !0
                            }, t)),
                                i.hideDialog()
                        }
                        ,
                        chatTerm: {
                            message: t.message || ""
                        }
                    }) : this._sendComment(e, Xe({
                        consentedCommentTerms: !0
                    }, t)) : this._sendComment(e, t)
                })
            }
            deleteComment(e, t) {
                return Ne(this, null, function* () {
                    if (t.replyId)
                        this.deleteReply(e, t);
                    else {
                        try {
                            yield (0,
                                Le.g7)(e, t.id);
                            const i = this.comments.filter(e => e.id === t.id);
                            this.updateCommentCount(this.commentCount - i.length)
                        } catch (e) {
                            return e instanceof Error && x.A.addMessage({
                                variant: "danger",
                                text: e.message
                            }),
                                void console.error(e)
                        }
                        (0,
                            o.h5)(() => {
                                const e = this.comments.filter(e => e.id !== t.id);
                                this.comments.replace(e),
                                    this.updateFirstCommentId()
                            }
                            )
                    }
                })
            }
            sendReply(e, t, i) {
                return Ne(this, null, function* () {
                    if (!this.stores)
                        return;
                    const s = this.stores.dialogStore
                        , r = this.stores.moviePageStore.movieStore
                        , a = () => Ne(this, null, function* () {
                            try {
                                const s = i.stampId ? yield He.A.replyStamp(e, t, i.stampId, !0) : yield He.A.replyMessage(e, t, i.message || "", !0);
                                (0,
                                    o.h5)(() => {
                                        const e = this.comments.slice().reverse().findIndex(e => s.id === e.id)
                                            , t = this.comments.length - e;
                                        this.comments.splice(t, 0, this._createIComment(s)),
                                            this.updateFirstCommentId(),
                                            this.updateCommentCount(this.commentCount + 1)
                                    }
                                    )
                            } catch (e) {
                                e instanceof Error && x.A.addMessage({
                                    variant: "danger",
                                    text: e.message
                                })
                            }
                        });
                    !r.isOwner && r.settings.isChatTermsRequired && i.message ? s.showDialog({
                        type: "chatTerm",
                        onSubmit: () => {
                            a(),
                                s.hideDialog()
                        }
                        ,
                        chatTerm: {
                            message: i.message || ""
                        }
                    }) : a()
                })
            }
            deleteReply(e, t) {
                return Ne(this, null, function* () {
                    if (t.replyId) {
                        try {
                            yield (0,
                                Le.ny)(e, t.id, t.replyId),
                                this.updateCommentCount(this.commentCount - 1)
                        } catch (e) {
                            return void console.error(e)
                        }
                        (0,
                            o.h5)(() => {
                                this.comments.remove(t),
                                    this.updateFirstCommentId()
                            }
                            )
                    }
                })
            }
            fetchLatestCommentAndReset(e) {
                return Ne(this, null, function* () {
                    this._fetchCommentsAndUpdate(e, {}, {
                        isReset: !0
                    })
                })
            }
            traceComments(e) {
                return Ne(this, null, function* () {
                    if (this.commentLoading || this.isLastPage)
                        return;
                    const t = this.comments[this.comments.length - 1];
                    let i = 0;
                    t && (i = t.id),
                        i ? this._fetchCommentsAndUpdate(e, {
                            toCommentId: i
                        }) : this.fetchLatestCommentAndReset(e)
                })
            }
            _fetchCommentsAndUpdate(e, t) {
                return Ne(this, arguments, function* (e, t, i = {}) {
                    let s;
                    try {
                        s = yield (0,
                            ke.i)(e, t)
                    } catch (e) {
                        return void (0,
                            o.h5)(() => {
                                console.error(e),
                                    i.isReset && this.comments.clear(),
                                    this.commentLoading = !1
                            }
                            )
                    }
                    (0,
                        o.h5)(() => {
                            if (!s.length)
                                return i.isReset && this.comments.clear(),
                                    void (this.isLastPage = !0);
                            const e = s.map(e => this._createIComment(e));
                            this.commentLoading = !1,
                                i.isReset ? (this.comments.replace(e),
                                    this.updateFirstCommentId()) : (this.updateFirstCommentId(),
                                        this.comments.replace(this.comments.concat(e)))
                        }
                        )
                })
            }
            updateFirstCommentId() {
                const e = this.comments.length && this.comments[0];
                e && (this.preFirstCommentId = e.id)
            }
            openPalette() {
                this.openedPalette = !0
            }
            closePalette() {
                this.openedPalette = !1
            }
            updateCommentCount(e) {
                this.commentCount = e
            }
        }
        We([o.sH], Ve.prototype, "comments", 2),
            We([o.sH], Ve.prototype, "commentCount", 2),
            We([o.XI], Ve.prototype, "willLoad", 1),
            We([o.XI], Ve.prototype, "willUnload", 1),
            We([o.XI], Ve.prototype, "_sendComment", 1),
            We([o.XI], Ve.prototype, "sendCommentAfterCommentTermConsenet", 1),
            We([o.XI], Ve.prototype, "deleteComment", 1),
            We([o.XI], Ve.prototype, "sendReply", 1),
            We([o.XI], Ve.prototype, "deleteReply", 1),
            We([o.XI], Ve.prototype, "fetchLatestCommentAndReset", 1),
            We([o.XI.bound], Ve.prototype, "traceComments", 1),
            We([o.XI], Ve.prototype, "_fetchCommentsAndUpdate", 1),
            We([o.XI], Ve.prototype, "updateFirstCommentId", 1),
            We([o.XI], Ve.prototype, "openPalette", 1),
            We([o.XI], Ve.prototype, "closePalette", 1),
            We([o.XI], Ve.prototype, "updateCommentCount", 1);
        const Ye = {
            CHAT_ADD: 0,
            VIEWERS_COUNT: 1,
            STREAM_START: 5,
            STREAM_END: 3,
            BLACKLIST_ADD: 6,
            BLACKLIST_DELETE: 7,
            MODERATOR_ADD: 8,
            MODERATOR_DELETE: 9,
            DASHBOARD_BROADCAST: 10,
            SYSTEM_MESSAGE_V2: 11,
            CHATMODERATOR_ADD: 12,
            CHATMODERATOR_UPDATE: 13,
            CHATMODERATOR_DELETE: 14,
            CHATMODERATOR_SHOWPLAYER: 15,
            SUBSCRIBED: 27,
            POLL_START: 29,
            POLL_PROGRESS: 30,
            POLL_FINISH: 31,
            WATCHING_STOP: 32,
            PPV_TICKET_PURCHASED: 33,
            CHATLIST_MODE: 34,
            EXTENSION_DISPLAY: 38,
            EXTENSION_NOTIFY: 43,
            MOVIE_SWITCH_TO_BACKUP: 44,
            YELL_REACTION: 45,
            PLAYER_REFRESH: 46,
            BREAK_TIME_START: 47
        };
        var Qe = i(17306)
            , qe = i(43435)
            , je = i(92915)
            , ze = Object.defineProperty
            , Ge = Object.getOwnPropertyDescriptor
            , Ke = Object.getOwnPropertySymbols
            , $e = Object.prototype.hasOwnProperty
            , Ze = Object.prototype.propertyIsEnumerable
            , Je = (e, t, i, s) => {
                for (var o, r = s > 1 ? void 0 : s ? Ge(t, i) : t, a = e.length - 1; a >= 0; a--)
                    (o = e[a]) && (r = (s ? o(t, i, r) : o(r)) || r);
                return s && r && ze(t, i, r),
                    r
            }
            , et = (e, t, i) => new Promise((s, o) => {
                var r = e => {
                    try {
                        n(i.next(e))
                    } catch (e) {
                        o(e)
                    }
                }
                    , a = e => {
                        try {
                            n(i.throw(e))
                        } catch (e) {
                            o(e)
                        }
                    }
                    , n = e => e.done ? s(e.value) : Promise.resolve(e.value).then(r, a);
                n((i = i.apply(e, t)).next())
            }
            );
        class tt {
            constructor() {
                this.stores = null,
                    this.toUserHistory = o.sH.array(),
                    this.leagueRanks = o.sH.array(),
                    this.leagueMembers = o.sH.array(),
                    this.isEnabledTeam = !0,
                    (0,
                        o.Gn)(this)
            }
            get toUser() {
                if (this.leagueMembers.length && this.toUserHistory.length) {
                    const e = this.leagueMembers
                        , t = this.toUserHistory.find(t => e.some(e => e.id === t.userId));
                    return t || null
                }
                return null
            }
            willLoad(e) {
                return et(this, null, function* () {
                    this.stores = e;
                    const t = this.stores.moviePageStore.movieStore.leagueId;
                    t && (this.fetchLeagueRankAndUpdate({
                        leagueId: t
                    }),
                        this.fetchLeagueMembersAndUpdate(t))
                })
            }
            fetchLeagueRankAndUpdate(e) {
                return et(this, null, function* () {
                    let t;
                    try {
                        const i = e
                            , { c: s } = i
                            , o = ((e, t) => {
                                var i = {};
                                for (var s in e)
                                    $e.call(e, s) && t.indexOf(s) < 0 && (i[s] = e[s]);
                                if (null != e && Ke)
                                    for (var s of Ke(e))
                                        t.indexOf(s) < 0 && Ze.call(e, s) && (i[s] = e[s]);
                                return i
                            }
                            )(i, ["c"]);
                        t = yield (0,
                            k.r)(() => (0,
                                je.P)(o), {
                                c: s
                            })
                    } catch (e) {
                        return void (0,
                            o.h5)(() => {
                                console.error(e)
                            }
                            )
                    }
                    (0,
                        o.h5)(() => {
                            if (!t.length)
                                return;
                            const e = t.map(e => this._toLeagueRankFromApiv5Response(e));
                            this.leagueRanks.replace(e)
                        }
                        )
                })
            }
            fetchLeagueMembersAndUpdate(e) {
                return et(this, null, function* () {
                    let t;
                    try {
                        t = yield function (e) {
                            return (0,
                                k.A)("GET", `/leagues/${e}/members`, {
                                    query: void 0,
                                    body: void 0
                                })
                        }(e)
                    } catch (e) {
                        return void (0,
                            o.h5)(() => {
                                console.error(e)
                            }
                            )
                    }
                    (0,
                        o.h5)(() => {
                            if (!t.length)
                                return;
                            const e = this.stores && this.stores.moviePageStore.movieStore.movieId || 0
                                , i = this.stores && this.stores.userStore.user.id || ""
                                , s = t.filter(e => e.id !== i).sort((t, i) => {
                                    const s = Number(t.openrecUserId || 0) + e
                                        , o = Number(i.openrecUserId || 0) + e;
                                    return (qe.A.math.seedRandom(s) || 0) < (qe.A.math.seedRandom(o) || 0) ? 1 : -1
                                }
                                ).map(this._toLeagueMemberFromApiv5Response);
                            this.leagueMembers.replace(s)
                        }
                        )
                })
            }
            updateToUserHistory(e) {
                if (!this.stores || !e)
                    return;
                if (!this.stores.moviePageStore.movieStore.isLeague)
                    return;
                if (!this.leagueMembers.length)
                    return;
                const t = this.leagueMembers.find(t => t.id === e);
                if (!t)
                    return;
                const i = {
                    userId: t.id,
                    userName: t.name,
                    userIconImageUrl: t.iconImageUrl
                };
                this.toUserHistory.unshift(i),
                    this.isEnabledTeam = !1,
                    this.toUserHistory.length > 100 && this.toUserHistory.pop()
            }
            _toLeagueRankFromApiv5Response(e) {
                var t, i, s, o, r, a, n, l, d, h, u, c;
                return {
                    rank: e.rank || 0,
                    totalYells: e.totalYells || 0,
                    userId: (null == (t = e.leagueMember) ? void 0 : t.id) || "",
                    recxuserId: (null == (i = e.leagueMember) ? void 0 : i.recxuserId) || 0,
                    userIconImageUrl: (null == (s = e.leagueMember) ? void 0 : s.iconImageUrl) || Qe.A.IMAGES.PROFILE,
                    userLIconImageUrl: (null == (o = e.leagueMember) ? void 0 : o.lIconImageUrl) || Qe.A.IMAGES.PROFILE,
                    userCoverImageUrl: (null == (r = e.leagueMember) ? void 0 : r.coverImageUrl) || Qe.A.IMAGES.PROFILE_COVER,
                    userName: (null == (a = e.leagueMember) ? void 0 : a.nickname) || "",
                    isOfficial: !!(null == (n = e.leagueMember) ? void 0 : n.isOfficial),
                    isFresh: !!(null == (l = e.leagueMember) ? void 0 : l.isFresh),
                    isPremium: !!(null == (d = e.leagueMember) ? void 0 : d.isPremium),
                    isWarned: !!(null == (h = e.leagueMember) ? void 0 : h.isWarned),
                    isTeam: !!(null == (u = e.leagueMember) ? void 0 : u.isTeam),
                    isLeagueYell: !!(null == (c = e.leagueMember) ? void 0 : c.isLeagueYell)
                }
            }
            _toLeagueMemberFromApiv5Response(e) {
                return {
                    id: e.id || "",
                    name: e.nickname || "",
                    iconImageUrl: e.lIconImageUrl || Qe.A.IMAGES.PROFILE
                }
            }
        }
        Je([o.sH], tt.prototype, "toUserHistory", 2),
            Je([o.sH], tt.prototype, "leagueRanks", 2),
            Je([o.sH], tt.prototype, "leagueMembers", 2),
            Je([o.sH], tt.prototype, "isEnabledTeam", 2),
            Je([o.EW], tt.prototype, "toUser", 1),
            Je([o.XI], tt.prototype, "willLoad", 1),
            Je([o.XI], tt.prototype, "fetchLeagueRankAndUpdate", 1),
            Je([o.XI], tt.prototype, "fetchLeagueMembersAndUpdate", 1),
            Je([o.XI], tt.prototype, "updateToUserHistory", 1);
        const it = (e, t = {
            mutedWarnedUser: null,
            mutedFreshUser: null,
            mutedUnauthenticatedUser: null,
            mutedBannedWord: null,
            mutedRoomNotification: null,
            adjustChatDelay: null,
            nameColor: null,
            isFixedPhraseHidden: null,
            isPremiumHidden: null,
            isOfficialHidden: null,
            isSmallSizeStamp: null,
            isRoomNotification: null,
            isSubsBadgeHidden: null,
            isSubsDurationHidden: null,
            isSubsMembershipCardHidden: null,
            limitedUnsubsMemberChat: null,
            limitedContinuousChat: null,
            continuousChatThreshold: null,
            limitedUnfollowerChat: null,
            unfollowerChatThreshold: null,
            limitedFreshUserChat: null,
            freshUserChatThreshold: null,
            limitedTemporaryBlacklist: null,
            temporaryBlacklistThreshold: null,
            limitedWarnedUserChat: null,
            chatRule: null,
            yellReplyTemplate: null,
            enabledSharingTwitter: null,
            declinedSubsShare: null
        }) => ({
            userId: e.id || null,
            recxuserId: e.recxuserId || 0,
            openrecUserId: e.openrecUserId || 0,
            userName: e.nickname || "",
            userIconImageUrl: e.iconImageUrl || "",
            isOfficial: !!e.isOfficial,
            isPremium: !!e.isPremium,
            isFresh: !!e.isFresh,
            isWarned: !!e.isWarned,
            isModerator: !1,
            isLiveStreaming: !!e.isLive,
            userColor: t.nameColor || "",
            isPremiumHidden: !!t.isPremiumHidden,
            isSubsBadgeHidden: !!t.isSubsBadgeHidden,
            isSubsDurationHidden: !!t.isSubsDurationHidden
        })
            , st = e => {
                var t, i, s, o, r, a, n, l, d, h, u, c, p, m, y, g, v, S;
                return {
                    capture: {
                        id: (null == (t = e.capture) ? void 0 : t.id) || "",
                        title: (null == (i = e.capture) ? void 0 : i.title) || "",
                        thumbnailUrl: (null == (s = e.capture) ? void 0 : s.thumbnailUrl) || "",
                        createdAt: (null == (o = e.capture) ? void 0 : o.createdAt) || "",
                        startedAt: (null == (r = e.capture) ? void 0 : r.publishedAt) || "",
                        isBan: (null == (a = e.capture) ? void 0 : a.isBan) || !1,
                        isPublic: (null == (n = e.capture) ? void 0 : n.isPublic) || !1
                    },
                    captureChannel: {
                        userId: (null == (l = e.captureChannel) ? void 0 : l.id) || "",
                        userName: (null == (d = e.captureChannel) ? void 0 : d.nickname) || ""
                    },
                    game: {
                        id: (null == (h = e.game) ? void 0 : h.id) || "",
                        title: (null == (u = e.game) ? void 0 : u.title) || ""
                    },
                    movie: {
                        id: (null == (c = e.movie) ? void 0 : c.id) || "",
                        userId: (null == (m = null == (p = e.movie) ? void 0 : p.channel) ? void 0 : m.id) || "",
                        userName: (null == (g = null == (y = e.movie) ? void 0 : y.channel) ? void 0 : g.nickname) || "",
                        userIconImageUrl: (null == (S = null == (v = e.movie) ? void 0 : v.channel) ? void 0 : S.iconImageUrl) || ""
                    },
                    reactions: (e.reactionStatsList || []).map(e => ({
                        reaction: {
                            id: e.id || "",
                            label: {
                                ja: e.label && e.label.ja || "",
                                en: e.label && e.label.en || "",
                                ko: e.label && e.label.ko || "",
                                zh: e.label && e.label.zh || ""
                            },
                            reactionFile: e.reactionFile || ""
                        },
                        count: e.count || 0
                    }))
                }
            }
            ;
        var ot = i(87324)
            , rt = i(13087)
            , at = i(41433)
            , nt = i(1418)
            , lt = i(78838)
            , dt = i(37437)
            , ht = i(43526)
            , ut = i(79061)
            , ct = i(98932)
            , pt = i(89602)
            , mt = i(92926)
            , yt = i(85947)
            , gt = i(92288)
            , vt = i(59161)
            , St = i(70755)
            , bt = i(32128)
            , ft = i(11150)
            , Ct = i(28252)
            , It = i(18494)
            , At = i(23358)
            , _t = i(59881)
            , Pt = i(86709)
            , Mt = i(71120)
            , wt = i(60683)
            , Tt = i(17638)
            , Et = i(17605)
            , Ut = i(74211)
            , Lt = i(4075)
            , kt = Object.defineProperty
            , Ht = Object.getOwnPropertyDescriptor
            , Ot = Object.getOwnPropertySymbols
            , Dt = Object.prototype.hasOwnProperty
            , xt = Object.prototype.propertyIsEnumerable
            , Ft = (e, t, i) => t in e ? kt(e, t, {
                enumerable: !0,
                configurable: !0,
                writable: !0,
                value: i
            }) : e[t] = i
            , Rt = (e, t) => {
                for (var i in t || (t = {}))
                    Dt.call(t, i) && Ft(e, i, t[i]);
                if (Ot)
                    for (var i of Ot(t))
                        xt.call(t, i) && Ft(e, i, t[i]);
                return e
            }
            , Bt = (e, t, i, s) => {
                for (var o, r = s > 1 ? void 0 : s ? Ht(t, i) : t, a = e.length - 1; a >= 0; a--)
                    (o = e[a]) && (r = (s ? o(t, i, r) : o(r)) || r);
                return s && r && kt(t, i, r),
                    r
            }
            , Xt = (e, t, i) => new Promise((s, o) => {
                var r = e => {
                    try {
                        n(i.next(e))
                    } catch (e) {
                        o(e)
                    }
                }
                    , a = e => {
                        try {
                            n(i.throw(e))
                        } catch (e) {
                            o(e)
                        }
                    }
                    , n = e => e.done ? s(e.value) : Promise.resolve(e.value).then(r, a);
                n((i = i.apply(e, t)).next())
            }
            );
        class Wt {
            constructor() {
                this.stores = null,
                    this.isCompletedFetchMovie = !0,
                    this.isCompletedFetchMovieDetail = !1,
                    this.isCompletedFetchSubsProduct = !1,
                    this.isCompletedFetchVoteResult = !1,
                    this.id = "",
                    this.movieId = 0,
                    this.leagueId = "",
                    this.title = "",
                    this.introduction = "",
                    this.thumbnailUrl = Qe.A.IMAGES.MOVIE,
                    this._casts = o.sH.array(),
                    this._castsDetail = o.sH.array(),
                    this.tags = o.sH.array(),
                    this.next = null,
                    this.spriteImage = {
                        url: "",
                        interval: 0,
                        width: 0,
                        height: 0,
                        cols: 0,
                        rows: 0,
                        startPage: 0,
                        ext: ""
                    },
                    this.totalViews = 0,
                    this.liveViews = 0,
                    this.totalYells = 0,
                    this.monetizeStatus = 0,
                    this.onairStatus = -1,
                    this.enabledAd = !1,
                    this.enabledYell = !1,
                    this.width = 0,
                    this.height = 0,
                    this.playPostion = 0,
                    this.orientation = 0,
                    this.dvrOrientation = 0,
                    this.deviceType = 0,
                    this.movieType = 0,
                    this.platformType = "",
                    this.encryptionType = 0,
                    this.connectCount = null,
                    this.fixedScreen = void 0,
                    this._media = {
                        url: "",
                        urlDvr: "",
                        urlUll: "",
                        urlTrailer: ""
                    },
                    this._mediaDetail = void 0,
                    this.game = ft.A.createDefaultModel(),
                    this.ceroRating = 0,
                    this.makerId = void 0,
                    this.chapters = [],
                    this.chapterPosition = void 0,
                    this.channel = {
                        id: "",
                        openrecUserId: 0,
                        recxuserId: 0,
                        name: "",
                        followers: 0,
                        teams: [],
                        coverImageUrl: "",
                        iconImageUrl: "",
                        isModerating: !1,
                        isFollowing: void 0,
                        isPushEnabled: !1,
                        isStatsHidden: !1
                    },
                    this.blacklist = [],
                    this.ad = {
                        webStream: ""
                    },
                    this.viewsLimit = {
                        hasPermission: !1,
                        remain: 0,
                        restrictionMethod: "",
                        secondsRemaining: 0
                    },
                    this.settings = {
                        limitedContinuousChat: !1,
                        continuousChatThreshold: 0,
                        limitedUnfollowerChat: !1,
                        unfollowerChatThreshold: 0,
                        limitedFreshUserChat: !1,
                        freshUserChatThreshold: 0,
                        limitedTemporaryBlacklist: !1,
                        temporaryBlacklistThreshold: 0,
                        limitedWarnedUserChat: !1,
                        chatRule: "",
                        userColor: "",
                        isOfficialHidden: !1,
                        isPremiumHidden: !1,
                        isSubsBadgeHidden: !1,
                        isSubsDurationHidden: !1,
                        isChatTermsRequired: !1,
                        limitedUnsubsMemberChat: !1,
                        timestamp: void 0
                    },
                    this.playableChannelIdsOnMoblie = [],
                    this.fesEntries = [],
                    this.poll = Ct.A.createDefaultModel(),
                    this.pollUseTarget = "none",
                    this.voteResult = It.A.createModel(),
                    this.castYellMembers = o.sH.array(),
                    this.castYellHistory = o.sH.array(),
                    this.isSelectedCast = !1,
                    this.isMemberTrialMessageVisible = !1,
                    this.isFesEventIconClosed = !1,
                    this.isEnabledInitialExtension = !1,
                    this.displayExtensionNotifications = o.sH.array(),
                    this.isExtensionSystemChatClick = !1,
                    this.bannedWords = [],
                    this._releasedExtensions = [],
                    this._testExtensions = [],
                    this.notInstallExtensions = [],
                    this.breakTimeAdCooldown = void 0,
                    this._cooldownTimeoutId = 0,
                    this._backupMedias = [],
                    this._backupMediasDetail = void 0,
                    this._tempIsFixedPhrase = !1,
                    (0,
                        q.H)(this, {
                            createdAt: o.sH,
                            publishedAt: o.sH,
                            startedAt: o.sH,
                            endedAt: o.sH,
                            willStartAt: o.sH,
                            willEndAt: o.sH,
                            startTime: o.sH,
                            playTime: o.sH,
                            notFound: o.sH,
                            publicType: o.sH,
                            chatPublicType: o.sH,
                            uploadStatus: o.sH,
                            isLowLatency: o.sH,
                            isUploadedMovie: o.sH,
                            isMobile: o.sH,
                            isDvr: o.sH,
                            isCaputure: o.sH,
                            isPremiere: o.sH,
                            finishedLiveStreaming: o.sH,
                            enabledOwnerYell: o.sH,
                            enabledCastYell: o.sH,
                            chatPostedPosition: o.sH,
                            isFixedPhrase: o.sH,
                            isViewersHidden: o.sH,
                            exceedsDeviceLimit: o.sH,
                            interceptsDeviceLimit: o.sH,
                            movieResponseMessage: o.sH,
                            selectedChapter: o.sH,
                            subsChannel: o.sH,
                            subsProducts: o.sH.ref,
                            memberShip: o.sH,
                            v8Membership: o.sH,
                            ppvEvent: o.sH.ref,
                            myPpvTicketProducts: o.sH.ref,
                            permissions: o.sH.ref,
                            displayExtension: o.sH,
                            isAutoPlay: o.sH,
                            _isFinishedMedia: o.sH
                        })
            }
            get casts() {
                return this._casts.map(e => [e, this._castsDetail.find(t => t.userId === e.userId)]).map(([e, t]) => Rt(Rt({}, e), t))
            }
            get media() {
                return this._mediaDetail || this._media
            }
            get backupMedias() {
                return this._backupMediasDetail || this._backupMedias
            }
            get canPlayOnMobile() {
                return !!(this.playableChannelIdsOnMoblie.find(e => this.channel.id === e) || this.isMemberOnly || this.isMemberOnlyPlayable || this.hasPermissionForMemberOnly)
            }
            get toCastYellUser() {
                if (this.castYellMembers.length && this.castYellHistory.length) {
                    const e = this.castYellMembers
                        , t = this.castYellHistory.find(t => e.some(e => e.userId === t.userId));
                    return t || null
                }
                return null
            }
            get isArchivePlayable() {
                return this.isArchive && this.isPlayable
            }
            get isArchive() {
                return qe.A.is.archive(this.onairStatus)
            }
            get isComingUp() {
                return qe.A.is.comingup(this.onairStatus)
            }
            get isLiveStreaming() {
                return qe.A.is.liveStreaming(this.onairStatus)
            }
            get isFinishedMedia() {
                return !!this._isFinishedMedia
            }
            get isUserChatPublic() {
                return this.publicType === v.Oq.all || this.publicType === v.Oq.memberTrial || this.chatPublicType === v.GJ.all
            }
            get isChatPublicTypeMember() {
                return this.chatPublicType === v.GJ.member
            }
            get isUserChatReceivable() {
                return !!this.isOwner || !!this.isUserChatPublic || !this.isMemberOnly || this.isMemberOnlyChatPostable
            }
            get isPlayable() {
                return !!this.isOwner || !this.exceedsDeviceLimit && !!this.media.url
            }
            get isSecret() {
                return this.isComingUp && !!this.media.url
            }
            get isArchiveCreating() {
                return "SB" === this.uploadStatus && 2 === this.onairStatus
            }
            get isConverting() {
                return "SB" === this.uploadStatus || "SR" === this.uploadStatus || "SU" === this.uploadStatus
            }
            get failedUpload() {
                return "SP" === this.uploadStatus
            }
            get isCeroZ() {
                return 99 === this.ceroRating
            }
            get isLeague() {
                return !!this.leagueId && !!this.enabledYell && !!this.channel.isLeagueYell
            }
            get primaryTag() {
                return this.tags.length ? this.tags[0] : ""
            }
            get userIds() {
                return [this.channel.id, ...this.casts.map(e => e.userId).filter(e => !!e)]
            }
            get existVoteResult() {
                return this.voteResult.index >= 0
            }
            get isAes() {
                return 1 === this.encryptionType
            }
            get membershipMap() {
                const e = {};
                return this.memberShip && (e[this.channel.id] = this.memberShip),
                    this.casts.reduce((e, t) => (t.userId && t.membership && (e[t.userId] = t.membership),
                        e), e)
            }
            get v8MembershipMap() {
                const e = {};
                return this.v8Membership && (e[this.channel.id] = this.v8Membership),
                    this.casts.reduce((e, t) => (t.userId && t.v8Membership && (e[t.userId] = t.v8Membership),
                        e), e)
            }
            get currentChapter() {
                if (!this.stores)
                    return;
                const e = this.stores.moviePageStore;
                return qe.A.is.num(e.currentTime) ? this._getChapter(e.currentTime) : void 0
            }
            get chapterOnFirstPlay() {
                if (this.stores)
                    return this.isLiveStreaming ? qe.A.is.num(this.playPositionOnFirstPlay) ? this._getChapter(this.playPositionOnFirstPlay) : this.chapters[this.chapters.length - 1] : this._getChapter(this.playPositionOnFirstPlay || 0)
            }
            get currentGame() {
                if (this.currentChapter) {
                    if (this.currentChapter.game)
                        return this.currentChapter.game;
                    {
                        const e = [...this.chapters].reverse()
                            , t = e.findIndex(e => {
                                var t;
                                return e.id === (null == (t = this.currentChapter) ? void 0 : t.id)
                            }
                            )
                            , i = e.find((e, i) => i > t && e.game);
                        return null == i ? void 0 : i.game
                    }
                }
            }
            get playPositionOnFirstPlay() {
                if (!this.stores)
                    return;
                const e = this.stores.appStore
                    , t = this.stores.userStore
                    , i = e.currentQueryInfo;
                return this.isLiveStreaming ? t.user.isPremium && this.isDvr && i.t ? qe.A.to.playTimeToSec(i.t) : void 0 : i.t ? qe.A.to.playTimeToSec(decodeURIComponent(i.t)) : this.playPostion ? this.playPostion : this.startTime || 0
            }
            get isMemberTrial() {
                return this.publicType === v.Oq.memberTrial
            }
            get isMemberOnly() {
                return this.publicType === v.Oq.member
            }
            get isMemberOnlyPlayable() {
                return this.isMemberOnly && this.isPlayable
            }
            get isMemberOnlyChatPostable() {
                return !!this.isMemberOnly && (!(!this.isComingUp || !this.hasPermissionForMemberOnly) || this.isPlayable)
            }
            get isMemberOnlyCommentPostable() {
                return this.isMemberOnly && this.isPlayable
            }
            get hasPermissionForMemberOnly() {
                return !!this.isOwner || !(!this.isPpvEnabled || !this.hasPpvTicketProducts) || !(!this.isSubsEnabled || !this.hasSubscribedGreaterThanOrEqualToPermittedSubsProduct)
            }
            get isSpecial() {
                return !!this.media.urlTrailer
            }
            get isSpecialPlayable() {
                return this.isSpecial && this.isPlayable
            }
            get isTrailer() {
                return !this.isSecret && (this.isSpecial && !this.isSpecialPlayable || this.isSpecial && this.isComingUp)
            }
            get openedSubscription() {
                return !!this.subsChannel && !!this.subsChannel.id
            }
            get isSubsEnabled() {
                var e, t;
                return !(!this.isMemberOnly && !this.isMemberTrial || 0 !== (null == (e = this.permissions) ? void 0 : e.length) && !(null == (t = this.permissions) ? void 0 : t.find(e => !!e.subsProductId)))
            }
            get lowestSubsProduct() {
                var e;
                return null == (e = this.subsProducts) ? void 0 : e.sort((e, t) => e.tier - t.tier)[0]
            }
            get isLowestSubsProductPermitted() {
                return !!this.lowestSubsProduct && !!this.permittedSubsProduct && this.lowestSubsProduct.tier === this.permittedSubsProduct.tier
            }
            get permittedSubsProduct() {
                var e, t;
                if (!this.isSubsEnabled)
                    return;
                const i = null == (e = this.permissions) ? void 0 : e.find(e => !!e.subsProductId);
                return (null == (t = this.subsProducts) ? void 0 : t.find(e => (null == i ? void 0 : i.subsProductId) === e.id)) || this.lowestSubsProduct
            }
            get subsProductsGreaterThanOrEqualToPermittedSubsProduct() {
                var e;
                return null == (e = this.subsProducts) ? void 0 : e.filter(e => !!this.permittedSubsProduct && this.permittedSubsProduct.tier <= e.tier)
            }
            get hasSubscribed() {
                return !!this.memberShip && this.memberShip.isActive
            }
            get hasSubscribedGreaterThanOrEqualToPermittedSubsProduct() {
                return !!this.hasSubscribed && (!!this.isLowestSubsProductPermitted || !!this.permittedSubsProduct && !!this.memberShip && this.permittedSubsProduct.tier <= this.memberShip.subsProduct.tier)
            }
            get isPpvEnabled() {
                var e;
                return !(!this.isMemberOnly && !this.isMemberTrial || !(null == (e = this.permissions) ? void 0 : e.find(e => !!e.ppvTicketId)))
            }
            get hasPpvTicketProducts() {
                var e;
                return !!(null == (e = this.myPpvTicketProducts) ? void 0 : e.length)
            }
            get isFesEventIconVisible() {
                if (!this.stores)
                    return !1;
                const e = this.stores.moviePageStore;
                return !this.isFesEventIconClosed && !e.isTheaterMode && !(!this.fesEntries.length || !this.isLiveStreaming && !this.isComingUp)
            }
            get extensions() {
                return [...this._releasedExtensions, ...this._testExtensions]
            }
            willLoadOnServer(e) {
                this.stores = e
            }
            willLoad(e) {
                return Xt(this, null, function* () {
                    this.stores = e,
                        this.fetchTeamsAndUpdate(),
                        this.fetchFesEntriesAndUpdate()
                })
            }
            willUnload() {
                this.isCompletedFetchMovie = !1,
                    this.isCompletedFetchMovieDetail = !1,
                    this.isCompletedFetchSubsProduct = !1,
                    this.id = "",
                    this.movieId = 0,
                    this.leagueId = "",
                    this.title = "",
                    this.introduction = "",
                    this.thumbnailUrl = Qe.A.IMAGES.MOVIE,
                    this._casts.clear(),
                    this.tags.clear(),
                    this.next = null,
                    this.spriteImage = {
                        url: "",
                        interval: 0,
                        width: 0,
                        height: 0,
                        cols: 0,
                        rows: 0,
                        startPage: 0,
                        ext: ""
                    },
                    this.totalViews = 0,
                    this.liveViews = 0,
                    this.totalYells = 0,
                    this.createdAt = void 0,
                    this.publishedAt = void 0,
                    this.startedAt = void 0,
                    this.endedAt = void 0,
                    this.willStartAt = void 0,
                    this.willEndAt = void 0,
                    this.startTime = void 0,
                    this.playTime = void 0,
                    this.notFound = void 0,
                    this.publicType = void 0,
                    this.chatPublicType = void 0,
                    this.monetizeStatus = 0,
                    this.onairStatus = -1,
                    this.uploadStatus = void 0,
                    this.isLowLatency = void 0,
                    this.isUploadedMovie = void 0,
                    this.isMobile = void 0,
                    this.isDvr = void 0,
                    this.isCaputure = void 0,
                    this.isPremiere = void 0,
                    this.finishedLiveStreaming = void 0,
                    this.enabledAd = !1,
                    this.enabledYell = !1,
                    this.enabledOwnerYell = void 0,
                    this.enabledCastYell = void 0,
                    this.width = 0,
                    this.height = 0,
                    this.playPostion = 0,
                    this.orientation = 0,
                    this.dvrOrientation = 0,
                    this.deviceType = 0,
                    this.movieType = 0,
                    this.platformType = "",
                    this.encryptionType = 0,
                    this.connectCount = null,
                    this.chatPostedPosition = void 0,
                    this.isFixedPhrase = void 0,
                    this.exceedsDeviceLimit = void 0,
                    this.interceptsDeviceLimit = void 0,
                    this.fixedScreen = void 0,
                    this.chatSecretKey = void 0,
                    this.resetMedia(),
                    this.game = ft.A.createDefaultModel(),
                    this.ceroRating = 0,
                    this.makerId = void 0,
                    this.chapters = [],
                    this.chapterPosition = void 0,
                    this.channel = {
                        id: "",
                        openrecUserId: 0,
                        recxuserId: 0,
                        name: "",
                        followers: 0,
                        teams: [],
                        coverImageUrl: "",
                        iconImageUrl: "",
                        isModerating: !1,
                        isFollowing: void 0,
                        isPushEnabled: !1,
                        enabledChannelStamp: !1,
                        enabledFanletter: !1,
                        recentFanletters: 0
                    },
                    this.v8Channel = void 0,
                    this.subsChannel = void 0,
                    this.memberShip = void 0,
                    this.v8Membership = void 0,
                    this.ppvEvent = void 0,
                    this.myPpvTicketProducts = void 0,
                    this.permissions = void 0,
                    this.blacklist = [],
                    this.ad = {
                        webStream: ""
                    },
                    this.viewsLimit = {
                        hasPermission: !1,
                        remain: 0,
                        restrictionMethod: "",
                        secondsRemaining: 0
                    },
                    this.settings = {
                        limitedContinuousChat: !1,
                        continuousChatThreshold: 0,
                        limitedUnfollowerChat: !1,
                        unfollowerChatThreshold: 0,
                        limitedFreshUserChat: !1,
                        freshUserChatThreshold: 0,
                        limitedTemporaryBlacklist: !1,
                        temporaryBlacklistThreshold: 0,
                        limitedWarnedUserChat: !1,
                        chatRule: "",
                        userColor: "",
                        isPremiumHidden: !1,
                        isSubsBadgeHidden: !1,
                        isSubsDurationHidden: !1,
                        limitedUnsubsMemberChat: !1,
                        timestamp: void 0
                    },
                    this.fesEntries = [],
                    this.isFesEventIconClosed = !1,
                    this.initPoll(),
                    this.pollUseTarget = "none",
                    this.isCompletedFetchVoteResult = !1,
                    this.castYellMembers.clear(),
                    this.castYellHistory.clear(),
                    this.isSelectedCast = !1,
                    this.hideMemberTrialMessage(),
                    this._isFinishedMedia = void 0,
                    this._tempIsFixedPhrase = !1,
                    this.stores = null,
                    this._releasedExtensions = [],
                    this._testExtensions = [],
                    this.isEnabledInitialExtension = !1,
                    this.breakTimeAdCooldown = void 0,
                    window.clearTimeout(this._cooldownTimeoutId),
                    this._backupMedias = [],
                    this._backupMediasDetail = [],
                    this._disposeLimitDialogShow(),
                    this._disposeStores(),
                    this._disposeMovies()
            }
            resetForTransition() { }
            handlePlay(e) {
                (this.isArchive || this.isUploadedMovie) && e > v.U0 && this.fetchMovieDetailAndUpdateDeviceLimit(),
                    this.isFinishedMedia && this.startMedia()
            }
            handleVideoEnded() {
                if (!this.stores)
                    return;
                const e = this.stores.appStore
                    , t = this.stores.dialogStore
                    , i = this.stores.moviePageStore
                    , s = this.stores.langStore
                    , o = this.stores.userStore;
                if (!this.isMemberTrial || this.hasPermissionForMemberOnly ? i.postViewingTimeLog(o, -1) : "number" == typeof i.currentTime && i.postViewingTimeLog(o, i.currentTime),
                    this.next)
                    e.toPage(`${this.next.isUploadedMovie ? Qe.A.MOVIE : Qe.A.LIVE}/${this.next.movieId}`);
                else if (this.finishMedia(),
                    this.isComingUp && this.isSpecial) {
                    if (o.user.isPremium)
                        return;
                    t.showDialog({
                        type: "premiumAppeal",
                        premiumAppeal: {
                            message: s.premium.special
                        }
                    })
                }
            }
            handleImaSdkEvent(e, t) {
                if (!this.stores)
                    return;
                const i = this.stores.moviePageStore
                    , s = this.stores.userStore;
                i.postAdViewingTimeLog(e, t, s)
            }
            handleViewingTimeUpdate(e, t) {
                if (!this.stores)
                    return;
                const i = this.stores.moviePageStore
                    , s = this.stores.userStore;
                e && Math.floor(e) % v.b === 0 && i.postViewingTimeLog(s, t)
            }
            _updateMovie(e) {
                var t, i, s, r, a, l, d, h, u, c, p, m, y, g, S, b, f, C, I, A, _, P, M, w, T, E, U, L, k, H, O, D, x, F, R, B, X, W, N, V, Y, Q, q, j, z, G;
                const K = (e.tags || []).filter(e => !!e)
                    , $ = (e.casts || []).map(e => it(e));
                this.notFound = !1,
                    this._isFinishedMedia = !1,
                    this.id = e.id || "",
                    this.movieId = e.movieId || 0,
                    this.leagueId = e.leagueId || "",
                    this.title = e.title || "",
                    this.introduction = e.introduction || "",
                    this.thumbnailUrl = e.lThumbnailUrl || Qe.A.IMAGES.MOVIE,
                    this.spriteImage.url = (null == (t = e.spriteImage) ? void 0 : t.url) || "",
                    this.spriteImage.interval = (null == (i = e.spriteImage) ? void 0 : i.interval) || 0,
                    this.spriteImage.width = (null == (s = e.spriteImage) ? void 0 : s.width) || 0,
                    this.spriteImage.height = (null == (r = e.spriteImage) ? void 0 : r.height) || 0,
                    this.spriteImage.cols = (null == (a = e.spriteImage) ? void 0 : a.cols) || 0,
                    this.spriteImage.rows = (null == (l = e.spriteImage) ? void 0 : l.rows) || 0,
                    this.spriteImage.startPage = (null == (d = e.spriteImage) ? void 0 : d.startPage) || 0,
                    this.spriteImage.ext = (null == (h = e.spriteImage) ? void 0 : h.ext) || "",
                    this.tags.replace(K),
                    this._casts.replace($),
                    this.updateCastYellMembers(),
                    this.totalViews = e.totalViews || 0,
                    this.liveViews = e.liveViews || 0,
                    this.createdAt = e.createdAt || "",
                    this.publishedAt = e.publishedAt || "",
                    this.startedAt = e.startedAt || "",
                    this.willStartAt = e.willStartAt || "",
                    this.willEndAt = e.willEndAt || "",
                    this.startTime = e.startTime || 0,
                    this.playTime = e.playTime || 0,
                    this.publicType = e.publicType || v.Oq.all,
                    this.chatPublicType = e.chatPublicType || v.GJ.all,
                    this.onairStatus = void 0 === e.onairStatus ? null : e.onairStatus,
                    this.monetizeStatus = e.monetizeStatus || 0,
                    this.uploadStatus = e.uploadStatus || "SK",
                    this.isLowLatency = !!e.isLowLatency,
                    this.enabledYell = !!e.enabledYell,
                    this.enabledOwnerYell = !!e.enabledOwnerYell,
                    this.enabledCastYell = !!e.enabledCastYell,
                    this.isMobile = !!e.isMobile,
                    this.isDvr = !!e.isDvr,
                    this.isCaputure = !!e.isCapture,
                    this.isPremiere = !!e.isPremiere,
                    this.deviceType = e.deviceType || 0,
                    this.orientation = e.orientation || 0,
                    this.enabledAd = !!e.enabledAd,
                    this.isUploadedMovie = !e.isLive,
                    this.next = e.next ? (0,
                        n.S8)(e.next) : null,
                    this.width = e.width || 0,
                    this.height = e.height || 0,
                    this.movieType = Number(e.movieType) || 0,
                    this.platformType = e.platformType || "",
                    this.encryptionType = e.encryptionType || 0,
                    this.connectCount = null === e.connectCount ? null : e.connectCount || 0,
                    this.isFixedPhrase = !!e.isFixedPhrase,
                    this.isViewersHidden = !!e.isViewersHidden,
                    this.fixedScreen = e.fixedScreen,
                    e.publicType === v.Oq.memberTrial && this.showMemberTrialMessage(),
                    this._updateMedia(e.media, e.subsTrialMedia),
                    e.backupMedia && this._updateBackupMedias(e.backupMedia),
                    this.ad.webStream = e.ad && e.ad.webStream || "",
                    this.game = e.game ? ft.A.createModel(e.game) : ft.A.createDefaultModel(),
                    this.ceroRating = e.game && e.game.ceroRating || 0,
                    this.makerId = (null == (c = null == (u = e.game) ? void 0 : u.maker) ? void 0 : c.id) || void 0,
                    this.updateChapters(e.chapters),
                    this.channel.id = (null == (p = e.channel) ? void 0 : p.id) || "",
                    this.channel.openrecUserId = (null == (m = e.channel) ? void 0 : m.openrecUserId) || 0,
                    this.channel.recxuserId = (null == (y = e.channel) ? void 0 : y.recxuserId) || 0,
                    this.channel.name = (null == (g = e.channel) ? void 0 : g.nickname) || "",
                    this.channel.coverImageUrl = (null == (S = e.channel) ? void 0 : S.coverImageUrl) || Qe.A.IMAGES.PROFILE_COVER,
                    this.channel.iconImageUrl = (null == (b = e.channel) ? void 0 : b.iconImageUrl) || "",
                    this.channel.followers = (null == (f = e.channel) ? void 0 : f.followers) || 0,
                    this.channel.isPremium = !!(null == (C = e.channel) ? void 0 : C.isPremium),
                    this.channel.isOfficial = !!(null == (I = e.channel) ? void 0 : I.isOfficial),
                    this.channel.isFresh = !!(null == (A = e.channel) ? void 0 : A.isFresh),
                    this.channel.isWarned = !!(null == (_ = e.channel) ? void 0 : _.isWarned),
                    this.channel.isLeagueYell = !!(null == (P = e.channel) ? void 0 : P.isLeagueYell),
                    this.channel.twitterScreenName = (null == (M = e.channel) ? void 0 : M.twitterScreenName) || "",
                    this.channel.gaTrackingId = (null == (w = e.channel) ? void 0 : w.gaTrackingId) || "",
                    this.channel.enabledChannelStamp = (null == (T = e.channel) ? void 0 : T.enabledChannelStamp) || !1,
                    this.channel.enabledFanletter = (null == (E = e.channel) ? void 0 : E.enabledFanletter) || !1,
                    this.channel.isStatsHidden = (null == (U = e.channel) ? void 0 : U.isStatsHidden) || !1,
                    this.channel.recentFanletters = (null == (L = e.channel) ? void 0 : L.recentFanletters) || 0,
                    this.channel.channelStarSetting = (null == (k = e.channel) ? void 0 : k.channelStarSetting) || void 0,
                    e.channel && (this.v8Channel = gt.A.createModel(e.channel)),
                    e.subsChannel && (this.subsChannel = this._toSubsChannel(e.subsChannel)),
                    e.ppvEvent && (this.ppvEvent = At.A.createModel(e.ppvEvent)),
                    e.permissions && this.updatePermissions(e.permissions);
                const Z = (null == (H = e.channel) ? void 0 : H.blacklist) || [];
                this.blacklist = Z.map(e => {
                    var t;
                    return {
                        id: (null == (t = e.user) ? void 0 : t.id) || "",
                        nickname: ""
                    }
                }
                ),
                    this.settings.limitedContinuousChat = !!(null == (O = e.chatSetting) ? void 0 : O.limitedContinuousChat),
                    this.settings.continuousChatThreshold = (null == (D = e.chatSetting) ? void 0 : D.continuousChatThreshold) || 0,
                    this.settings.limitedUnfollowerChat = !!(null == (x = e.chatSetting) ? void 0 : x.limitedUnfollowerChat),
                    this.settings.unfollowerChatThreshold = (null == (F = e.chatSetting) ? void 0 : F.unfollowerChatThreshold) || 0,
                    this.settings.limitedFreshUserChat = !!(null == (R = e.chatSetting) ? void 0 : R.limitedFreshUserChat),
                    this.settings.freshUserChatThreshold = (null == (B = e.chatSetting) ? void 0 : B.freshUserChatThreshold) || 0,
                    this.settings.limitedTemporaryBlacklist = !!(null == (X = e.chatSetting) ? void 0 : X.limitedTemporaryBlacklist),
                    this.settings.temporaryBlacklistThreshold = (null == (W = e.chatSetting) ? void 0 : W.temporaryBlacklistThreshold) || 0,
                    this.settings.limitedWarnedUserChat = !!(null == (N = e.chatSetting) ? void 0 : N.limitedWarnedUserChat),
                    this.settings.chatRule = (null == (V = e.chatSetting) ? void 0 : V.chatRule) || "",
                    this.settings.userColor = (null == (Y = e.chatSetting) ? void 0 : Y.nameColor) || "",
                    this.settings.isPremiumHidden = !!(null == (Q = e.chatSetting) ? void 0 : Q.isPremiumHidden),
                    this.settings.isChatTermsRequired = !!(null == (q = e.chatSetting) ? void 0 : q.chatTermsRequired),
                    this.settings.limitedUnsubsMemberChat = !!(null == (j = e.chatSetting) ? void 0 : j.limitedUnsubsMemberChat),
                    this.settings.timestamp = null != (G = null == (z = e.chatSetting) ? void 0 : z.timestamp) ? G : void 0,
                    this._tempIsFixedPhrase = !!e.isFixedPhrase,
                    this.pollUseTarget = e.pollUseTarget || "none",
                    e.poll && this.updatePoll(e.poll),
                    this._releasedExtensions = yt.A.createMapBy(e.componentExtensions || [], bt.A.createModel),
                    this.isEnabledInitialExtension = e.isEnabledInitialExtension || !1,
                    this._disposeStores = (0,
                        o.z7)(() => !!this.stores, () => {
                            this.stores && (this.stores.moviePageStore.commentStore.updateCommentCount(e.commentCount || 0),
                                this._toggleMovieAlert())
                        }
                        ),
                    this._disposeMovies = (0,
                        o.z7)(() => !!this.stores && !!this.movieId && !!this.uploadStatus, () => {
                            "SB" === this.uploadStatus && this.stores && pt.A.show(this.stores.moviePageStore.v8.archiveCreatingDialogId)
                        }
                        )
            }
            fetchMovieAndUpdate(e) {
                return Xt(this, arguments, function* (e, t = {}) {
                    const i = this.id || Nt();
                    this.id = i;
                    try {
                        const s = yield (0,
                            k.r)(() => (0,
                                ht.q0)(i), t.cacheBuster ? {
                                    c: t.cacheBuster
                                } : e.currentQueryInfo);
                        this.updateMovie(e, s)
                    } catch (t) {
                        this.updateMovie(e, t)
                    }
                })
            }
            updateMovie(e, t) {
                try {
                    const e = t.statusCode;
                    if (400 <= e && e <= 599)
                        throw t;
                    (0,
                        dt.s)(t),
                        this._updateMovie(t)
                } catch (t) {
                    if (404 === t.statusCode)
                        return e.updateStatusCode(404),
                            void (0,
                                o.h5)(() => {
                                    this.notFound = !0,
                                        -6 === t.status && (this.movieResponseMessage = t.message)
                                }
                                );
                    t.message && x.A.addMessage({
                        variant: "danger",
                        text: t.message
                    })
                }
                (0,
                    o.h5)(() => {
                        this.isCompletedFetchMovie = !0
                    }
                    );
                const i = new RegExp(`${this.isUploadedMovie ? Qe.A.LIVE : Qe.A.MOVIE}(/[0-9a-zA-Z-_]+)`)
                    , s = e.currentPath.match(i);
                s && (e.location = `${this.isUploadedMovie ? Qe.A.MOVIE : Qe.A.LIVE}${s[1]}`,
                    e.httpStatusCode = 301)
            }
            changeMoviesGameAndExtension() {
                return Xt(this, arguments, function* (e = {}) {
                    const t = (0,
                        o.XI)(e => {
                            this.monetizeStatus = e.monetizeStatus || 0,
                                this.enabledYell = !!e.enabledYell,
                                this.enabledAd = !!e.enabledAd,
                                this.game = e.game ? ft.A.createModel(e.game) : ft.A.createDefaultModel(),
                                this.ceroRating = e.game && e.game.ceroRating || 0,
                                this.updateChapters(e.chapters)
                        }
                        )
                        , i = (0,
                            o.XI)(e => {
                                const t = qe.A.to.changeCaseKey("camel", e.componentExtensions || []);
                                this._releasedExtensions = yt.A.createMapBy(t, bt.A.createModel)
                            }
                            );
                    try {
                        const s = e.c
                            , o = yield (0,
                                k.r)(() => (0,
                                    ht.q0)(this.id), {
                                    c: s
                                });
                        t(o),
                            i(o)
                    } catch (e) { }
                })
            }
            revalidateMovieDetail(e) {
                (0,
                    r.Tk)(ct.A.oneOf("byId", this.id, e.appStore.currentQueryInfo.secret_key))
            }
            fetchMovieDetailAndUpdate(e) {
                return Xt(this, null, function* () {
                    if (!e.userStore.isLogined)
                        return;
                    const t = (0,
                        lt.g)(this.id, {
                            secretKey: e.appStore.currentQueryInfo.secret_key
                        });
                    return t.then(e => {
                        var t, i;
                        const s = null == (i = null == (t = e.data) ? void 0 : t.items) ? void 0 : i[0];
                        this.updateMovieDetail(s)
                    }
                    ).catch(() => {
                        this._showErrorMessage(e.langStore.error.errorOccurred)
                    }
                    ),
                        t
                })
            }
            updateMovieDetail(e) {
                var t, i, s;
                this.isCompletedFetchMovieDetail = !0,
                    this.exceedsDeviceLimit = !!(null == e ? void 0 : e.exceedsDeviceLimit),
                    this.playPostion = -1 === (null == e ? void 0 : e.playPosition) ? 0 : (null == e ? void 0 : e.playPosition) || 0,
                    this.channel.isFollowing = !!(null == e ? void 0 : e.isFollowing),
                    this.channel.isModerating = !!(null == e ? void 0 : e.isModerating),
                    this.channel.isPushEnabled = !!(null == e ? void 0 : e.pushEnabled);
                const o = (null == e ? void 0 : e.chatHighlights) || [];
                if (this.channel.chatHistories = yt.A.createMapBy(o, St.A.createModel),
                    this._updateMediaDetail(null != (t = null == e ? void 0 : e.media) ? t : null, null != (i = null == e ? void 0 : e.subsTrialMedia) ? i : null),
                    (null == e ? void 0 : e.backupMedia) && this._updateBackupMediasDetail(null == e ? void 0 : e.backupMedia),
                    null == e ? void 0 : e.membership) {
                    const t = this._toMembership(e.membership)
                        , i = Pt.A.createModel(e.membership);
                    this.setMembership(t, i)
                }
                const r = null == e ? void 0 : e.viewsLimit;
                if (r && (this.viewsLimit.hasPermission = !!r.hasPermission,
                    this.viewsLimit.remain = r.remain || 0,
                    this.viewsLimit.restrictionMethod = r.restrictionMethod || "",
                    this.viewsLimit.secondsRemaining = r.secondsRemaining || 0),
                    this.stores && (null == e ? void 0 : e.yellReply) && (this.stores.moviePageStore.yellStore.yellReply.message = (null == e ? void 0 : e.yellReply.message) || "",
                        this.stores.moviePageStore.yellStore.yellReply.createdAt = (null == e ? void 0 : e.yellReply.createdAt) || "",
                        this.stores.moviePageStore.yellStore.yellReply.id = (null == e ? void 0 : e.yellReply.id) || 0),
                    null == e ? void 0 : e.casts) {
                    const t = e.casts.map(e => ({
                        userId: e.userId,
                        isFollowing: !!e.isFollowing,
                        membership: e.membership ? this._toMembership(e.membership) : void 0,
                        v8Membership: e.membership ? Pt.A.createModel(e.membership) : void 0
                    }));
                    this._castsDetail.replace(t)
                }
                (null == e ? void 0 : e.ppvTicketProducts) && (this.myPpvTicketProducts = yt.A.createMapBy(e.ppvTicketProducts, _t.A.createModel)),
                    this._testExtensions = yt.A.createMapBy(null != (s = null == e ? void 0 : e.testComponentExtensions) ? s : [], bt.A.createModel),
                    (null == e ? void 0 : e.bannedWords) && (this.bannedWords = e.bannedWords.map(e => this._toBannedWord(e))),
                    this.stores && (null == e ? void 0 : e.userSetting) && this.stores.moviePageStore.updateUserSetting(!!e.userSetting.adHidden, !!e.userSetting.subsAdHidden),
                    (null == e ? void 0 : e.chatSecretKey) && (this.chatSecretKey = e.chatSecretKey);
                const a = "number" == typeof (null == e ? void 0 : e.breaktimeAdCooldownRemainingSeconds) ? e.breaktimeAdCooldownRemainingSeconds * V.Z2 : void 0;
                "number" == typeof a && a >= 0 && this.setBreakTimeAdCooldown(a)
            }
            fetchMovieDetailAndUpdateDeviceLimit() {
                return Xt(this, null, function* () {
                    if (!this.stores)
                        return;
                    if (!this.stores.userStore.isLogined)
                        return;
                    const e = this.stores.appStore;
                    try {
                        const t = yield (0,
                            lt.g)(this.id, {
                                secretKey: e.currentQueryInfo.secret_key
                            });
                        (0,
                            o.h5)(() => {
                                var e, i;
                                const s = null == (i = null == (e = t.data) ? void 0 : e.items) ? void 0 : i[0];
                                this.exceedsDeviceLimit = !!(null == s ? void 0 : s.exceedsDeviceLimit)
                            }
                            )
                    } catch (e) { }
                })
            }
            fetchFesEntriesAndUpdate(e) {
                return Xt(this, null, function* () {
                    try {
                        const t = yield Et.Ay.listOpened(this.channel.id, e);
                        (0,
                            o.h5)(() => {
                                this.fesEntries = t
                            }
                            )
                    } catch (e) {
                        console.log(e)
                    }
                })
            }
            fetchSubsProductsOfChannelAndUpdate() {
                return Xt(this, null, function* () {
                    var e;
                    try {
                        const e = yield Ut.A.listByUserId(this.channel.id);
                        (0,
                            o.h5)(() => {
                                this.subsProducts = e,
                                    this.isCompletedFetchSubsProduct = !0
                            }
                            )
                    } catch (t) {
                        this._showErrorMessage(null == (e = this.stores) ? void 0 : e.langStore.common.tryAgainForCommunicationError)
                    }
                })
            }
            switchToWatchingOnThisDevice() {
                return Xt(this, null, function* () {
                    var e, t;
                    try {
                        yield (t = this.id,
                            (0,
                                ot.A)("POST", `/chat-rooms/${t}/switch-device`, {
                                    query: void 0,
                                    body: void 0
                                })),
                            (0,
                                o.h5)(() => {
                                    var e;
                                    null == (e = this.stores) || e.moviePageStore.resetPostedViewLog(),
                                        this.exceedsDeviceLimit = !1,
                                        this.interceptsDeviceLimit = !1
                                }
                                )
                    } catch (t) {
                        this._showErrorMessage(null == (e = this.stores) ? void 0 : e.langStore.common.tryAgainForCommunicationError)
                    }
                })
            }
            stopToWatchingOnThisDevice() {
                return Xt(this, null, function* () {
                    this.exceedsDeviceLimit = !0,
                        this.interceptsDeviceLimit = !0
                })
            }
            watchArchive() {
                return Xt(this, null, function* () {
                    try {
                        yield (e = {
                            movieId: this.id
                        },
                            (0,
                                ot.A)("POST", "/users/me/views-limit", {
                                    query: void 0,
                                    body: e
                                })),
                            this.stores && this.fetchMovieDetailAndUpdate(this.stores)
                    } catch (e) {
                        this._showErrorMessage(e.message)
                    }
                    var e
                })
            }
            fetchTeamsAndUpdate() {
                return Xt(this, null, function* () {
                    if (!this.channel.id)
                        return;
                    let e = [];
                    try {
                        e = yield (0,
                            ut.X)(this.channel.id)
                    } catch (e) { }
                    (0,
                        o.h5)(() => {
                            this.channel.teams = e.map(e => it(e))
                        }
                        )
                })
            }
            fetchVoteResultAndUpdate() {
                return Xt(this, null, function* () {
                    if ("start" === this.poll.status)
                        try {
                            const e = yield mt.A.voteResult(this.poll.votes, this.id, this.poll.id);
                            (0,
                                o.h5)(() => {
                                    this.updateVoteResult(e || {
                                        index: -1
                                    }),
                                        this.isCompletedFetchVoteResult = !0
                                }
                                )
                        } catch (e) {
                            Lt.A.error(e),
                                (0,
                                    o.h5)(() => {
                                        this.updateVoteResult({
                                            index: -1
                                        }),
                                            this.isCompletedFetchVoteResult = !0
                                    }
                                    ),
                                this._showErrorMessage(e.message)
                        }
                    else
                        this.isCompletedFetchVoteResult = !0
                })
            }
            updateCastYellHistory(e) {
                if (!this.stores || !e)
                    return;
                if (!this.enabledCastYell)
                    return;
                if (!this.castYellMembers.length)
                    return;
                const t = this.castYellMembers.find(t => t.userId === e);
                if (!t)
                    return;
                const i = {
                    userId: t.userId,
                    userName: t.userName,
                    userIconImageUrl: t.userIconImageUrl
                };
                this.castYellHistory.unshift(i),
                    this.isSelectedCast = !0,
                    this.castYellHistory.length > 100 && this.castYellHistory.pop()
            }
            updateDisplayExtension(e) {
                return Xt(this, null, function* () {
                    const t = yield Tt.A.getByExtensionId(e, "")
                        , i = {
                            id: t.id,
                            installationId: "",
                            name: t.name,
                            iconImageUrl: t.iconImageUrl,
                            contentsUrl: "",
                            sort: 1,
                            isOfficial: !1
                        };
                    (0,
                        o.h5)(() => {
                            this.notInstallExtensions.push(i)
                        }
                        )
                })
            }
            follow() {
                return Xt(this, null, function* () {
                    this.channel.followers++,
                        this.channel.isFollowing = !0,
                        this.channel.isPushEnabled = !0;
                    const e = yield (0,
                        at.$Q)(this.channel.id);
                    return e.message && (this._showErrorMessage(e.message),
                        (0,
                            o.h5)(() => {
                                this.channel.followers--,
                                    this.channel.isFollowing = !1,
                                    this.channel.isPushEnabled = !1
                            }
                            )),
                        e
                })
            }
            unfollow() {
                return Xt(this, null, function* () {
                    this.channel.followers--,
                        this.channel.isFollowing = !1;
                    const e = yield (0,
                        at.kW)(this.channel.id);
                    return (null == e ? void 0 : e.message) && (this._showErrorMessage(e.message),
                        (0,
                            o.h5)(() => {
                                this.channel.followers++,
                                    this.channel.isFollowing = !0
                            }
                            )),
                        e
                })
            }
            updateFollowing(e) {
                this.channel.isFollowing = e,
                    e ? this.channel.followers++ : this.channel.followers--
            }
            enablePush() {
                return Xt(this, null, function* () {
                    const e = yield (0,
                        at.E4)(this.channel.id, {
                            isNotified: !0
                        });
                    (null == e ? void 0 : e.message) ? this._showErrorMessage(e.message) : (this.stores && this.stores.appStore.hideTooltip(),
                        (0,
                            o.h5)(() => {
                                this.channel.isPushEnabled = !0
                            }
                            ))
                })
            }
            diablePush() {
                return Xt(this, null, function* () {
                    const e = yield (0,
                        at.E4)(this.channel.id, {
                            isNotified: !1
                        });
                    (null == e ? void 0 : e.message) ? this._showErrorMessage(e.message) : (this.stores && this.stores.appStore.hideTooltip(),
                        (0,
                            o.h5)(() => {
                                this.channel.isPushEnabled = !1
                            }
                            ))
                })
            }
            fetchMembershipsAndUpdate() {
                return Xt(this, null, function* () {
                    var e, t;
                    if (this.channel.id && this.stores && this.stores.userStore.isLogined)
                        try {
                            const i = null == (t = null == (e = (yield (0,
                                nt.Cr)(this.channel.id)).data) ? void 0 : e.items) ? void 0 : t[0];
                            (0,
                                o.h5)(() => {
                                    this.memberShip = i && this._toMembership(i)
                                }
                                )
                        } catch (e) {
                            this._showErrorMessage(e.message)
                        }
                })
            }
            updateViews(e, t) {
                this.totalViews = e < 0 ? 0 : e,
                    this.liveViews = t < 0 ? 0 : t
            }
            updateOnairStatus(e) {
                this.onairStatus = e
            }
            updateChatPostedPosition(e) {
                this.chatPostedPosition = e,
                    setTimeout((0,
                        o.XI)(() => {
                            this.chatPostedPosition = v.tv
                        }
                        ), 1e3)
            }
            finishMedia() {
                this._isFinishedMedia = !0
            }
            startMedia() {
                this._isFinishedMedia = !1
            }
            updateBroadcastInfo(e) {
                return Xt(this, null, function* () {
                    yield (0,
                        rt.a8)(this.movieId, e)
                })
            }
            updateReceivedBroadcastInfo(e) {
                return Xt(this, null, function* () {
                    this.title = e.title ? e.title : this.title,
                        this.introduction = qe.A.is.str(e.introduction) ? e.introduction : this.introduction,
                        this.publicType = e.public_type ? e.public_type : this.publicType,
                        this.chatPublicType = e.chat_public_type ? e.chat_public_type : this.chatPublicType,
                        e.permissions && this.updatePermissionsFromLegacy(e.permissions),
                        this._toggleMovieAlert()
                })
            }
            updateTitle(e) {
                this.title = e
            }
            updateIntroduction(e) {
                this.introduction = e
            }
            updatePreIsFixedPhrase(e) {
                return Xt(this, null, function* () {
                    this._tempIsFixedPhrase = e
                })
            }
            updateIsFixedPhrase() {
                return Xt(this, null, function* () {
                    this.isFixedPhrase = this._tempIsFixedPhrase
                })
            }
            updateCastFollow(e, t) {
                const i = this.casts.slice()
                    , s = i.findIndex(t => t.userId === e);
                i[s].isFollowing = t,
                    this._casts.replace(i)
            }
            rotatePlayer(e) {
                this.orientation = e
            }
            rotatePlayerWhenRotateButtonClick(e) {
                this.stores && (this.stores.moviePageStore.isLatest ? this.orientation = e : this.dvrOrientation = e)
            }
            updatePpv(e) {
                const t = qe.A.to.changeCaseKey("camel", e);
                this.ppvEvent = At.A.createModel(t)
            }
            updatePermissionsFromLegacy(e) {
                this.permissions = qe.A.to.changeCaseKey("camel", e)
            }
            updatePermissions(e) {
                this.permissions = e.map(e => {
                    var t, i;
                    return {
                        subsProductId: null != (t = e.subsProductId) ? t : void 0,
                        ppvTicketId: null != (i = e.ppvTicketId) ? i : void 0
                    }
                }
                )
            }
            updatePollFromLegacy(e) {
                const t = qe.A.to.changeCaseKey("camel", e);
                this.poll = Ct.A.createModel(t)
            }
            updatePoll(e) {
                this.poll = Ct.A.createModel(e)
            }
            updateVoteResult(e) {
                this.voteResult = It.A.createModel(e)
            }
            initPoll() {
                this.poll = Ct.A.createDefaultModel(),
                    this.voteResult = It.A.createModel()
            }
            updateIsViewersHidden(e) {
                this.isViewersHidden = e
            }
            updateChaptersFromLegacy(e) {
                if (!e)
                    return void (this.chapters = []);
                const t = qe.A.to.changeCaseKey("camel", e);
                this.chapters = t.map(vt.A.createModel)
            }
            updateChapters(e) {
                this.chapters = e ? e.map(vt.A.createModel) : []
            }
            selectChapterManually(e) {
                this.stores && (this.isLiveStreaming && !this.stores.userStore.isLogined && pt.A.show(Mt.A.bifurcateSignupAndSigninDialogId),
                    this.stores.moviePageStore.updateWhetherToSeekPlayer(!0),
                    this.startedAt && (this.chapterPosition = qe.A.moment.substract(new Date(this.startedAt), e.chapterAt.toDate()),
                        setTimeout((0,
                            o.XI)(() => {
                                this.chapterPosition = v.tv
                            }
                            ), 1e3)))
            }
            deleteChapter(e) {
                const t = this.chapters.filter(t => t.id !== e);
                this.chapters = t
            }
            getChapter(e) {
                return Xt(this, null, function* () {
                    try {
                        const t = yield wt.A.getChapter(e);
                        (0,
                            o.h5)(() => {
                                this.chapters = t
                            }
                            )
                    } catch (e) {
                        e instanceof Error && x.A.addMessage({
                            variant: "danger",
                            text: e.message
                        })
                    }
                })
            }
            updateSelectedChapter(e) {
                this.selectedChapter = e
            }
            closeFesEventIcon() {
                this.isFesEventIconClosed = !0
            }
            addDisplayExtension(e) {
                const t = this.displayExtensionNotifications.some(t => "extension_key" === t.matchType ? t.id === e.id : t.id === e.id && t.installationId === e.installationId);
                this.displayExtension = e,
                    t || this.displayExtensionNotifications.push(e)
            }
            setDisplayExtension(e) {
                this.displayExtension = e,
                    this.setIsExtensionSystemChatClick(!0)
            }
            removeDisplayExtensin() {
                this.displayExtension = void 0
            }
            removeDisplayExtensionNotification(e, t) {
                const i = this.displayExtensionNotifications.filter(i => i.id !== e || i.installationId !== t);
                this.displayExtensionNotifications.replace(i)
            }
            setIsExtensionSystemChatClick(e) {
                this.isExtensionSystemChatClick = e
            }
            setPlayPosition(e) {
                this.playPostion = e
            }
            setIsAutoPlay(e) {
                this.isAutoPlay = e
            }
            setMembership(e, t) {
                this.memberShip = e,
                    this.v8Membership = t
            }
            startLiveStreaming() {
                return Xt(this, null, function* () {
                    if (this.stores)
                        try {
                            yield this.fetchMovieAndUpdate(this.stores.appStore, {
                                cacheBuster: `${this.id || "start"}`
                            });
                            {
                                const e = Math.floor(5e3 * Math.random());
                                setTimeout(() => {
                                    this.stores && this.stores.userStore.isLogined && (this.isPpvEnabled || this.isSubsEnabled || this.stores.userStore.user.isPremium) && this.fetchMovieDetailAndUpdate(this.stores)
                                }
                                    , e)
                            }
                        } catch (e) {
                            this._showErrorMessage(this.stores.langStore.error.errorOccurred)
                        }
                })
            }
            updateCastYellMembers() {
                if (!this.enabledCastYell)
                    return;
                const e = this.movieId || 0
                    , t = this.stores && this.stores.userStore.user.id || ""
                    , i = this.casts.slice().filter(e => e.userId !== t).sort((t, i) => {
                        const s = Number(t.recxuserId || 0) + e
                            , o = Number(i.recxuserId || 0) + e;
                        return (qe.A.math.seedRandom(s) || 0) < (qe.A.math.seedRandom(o) || 0) ? 1 : -1
                    }
                    );
                if (this.enabledOwnerYell) {
                    const e = {
                        userId: this.channel.id,
                        recxuserId: this.channel.recxuserId,
                        userName: this.channel.name,
                        userIconImageUrl: this.channel.iconImageUrl
                    };
                    i.unshift(e)
                }
                this.castYellMembers.replace(i)
            }
            finishLiveStreaming() {
                this.finishMedia()
            }
            showMemberTrialMessage() {
                this.isMemberTrialMessageVisible = !0
            }
            hideMemberTrialMessage() {
                this.isMemberTrialMessageVisible = !1
            }
            showViewsLimitDialog() {
                if (!this.stores)
                    return;
                const e = this.viewsLimit || {};
                this.stores.dialogStore.showDialog({
                    type: "viewsLimit",
                    viewsLimit: qe.A.object.extendDeepWith({}, e),
                    onSubmit: this.watchArchive
                })
            }
            resetMedia() {
                this._media = {
                    url: "",
                    urlDvr: "",
                    urlUll: "",
                    urlTrailer: "",
                    urlSource: ""
                },
                    this._mediaDetail = void 0
            }
            isLiveViewersVisible() {
                if (!this.stores)
                    return !1;
                const e = this.stores.moviePageStore
                    , t = e.movieStore
                    , i = this.stores.appStore;
                return !(e.isOffline || (!t.isLiveStreaming || !t.liveViews) && !(t.isComingUp && t.liveViews && i.currentQueryInfo.secret_key))
            }
            isSplitViewVisible() {
                if (!this.stores)
                    return !1;
                const e = this.stores.userStore
                    , t = this.stores.moviePageStore
                    , i = t.movieStore;
                return !(!e.user.id || e.user.id !== i.channel.id || t.isTheaterMode || t.isExtensionPopoutPage || !i.isLiveStreaming || t.isOffline)
            }
            _isMemberInfoVisible() {
                if (!this.stores)
                    return !1;
                const e = this.stores.userStore;
                if (!e.isCompletedInitialFetch)
                    return !1;
                if (this.isMemberOnly || this.isMemberTrial || this.isPpvEnabled || this.openedSubscription) {
                    if (this.isPpvEnabled) {
                        if (this.isOwner)
                            return !1
                    } else if (this.openedSubscription && this.isOwner)
                        return !1;
                    if (!e.isLogined)
                        return !0;
                    if (this.isCompletedFetchMovieDetail)
                        return !0
                }
                return !1
            }
            isMemberAppealVisible() {
                return this.isPpvEnabled ? this.hasPpvTicketProducts ? (!this.isArchive || !this.isMemberTrial) && this._isMemberInfoVisible() : (!this.isSubsEnabled || !this.hasSubscribedGreaterThanOrEqualToPermittedSubsProduct) && this._isMemberInfoVisible() : !!this.openedSubscription && (this.hasSubscribed ? !(!this.isArchive || !this.isMemberTrial || this.hasSubscribedGreaterThanOrEqualToPermittedSubsProduct) : this._isMemberInfoVisible())
            }
            switchToBackupMedia(e) {
                var t;
                const i = this._backupMedias.find(t => t.id === e);
                if (i) {
                    const { media: e, subsTrialMedia: t } = i
                        , s = this.publicType === v.Oq.memberTrial ? t : void 0;
                    this._media.url = e.url || (null == s ? void 0 : s.url) || "",
                        this._media.urlDvr = e.urlDvr || (null == s ? void 0 : s.urlDvr) || "",
                        this._media.urlUll = e.urlUll || (null == s ? void 0 : s.urlUll) || "",
                        this._media.urlTrailer = e.urlTrailer || "",
                        this._media.urlSource = e.urlSource || ""
                } else
                    this._media = {
                        url: "",
                        urlDvr: "",
                        urlUll: "",
                        urlTrailer: ""
                    };
                const s = (null != (t = this._backupMediasDetail) ? t : []).find(t => t.id === e);
                if (s) {
                    const { media: e, subsTrialMedia: t } = s
                        , i = this.publicType === v.Oq.memberTrial ? t : void 0;
                    this._mediaDetail = {
                        url: e.url || (null == i ? void 0 : i.url) || "",
                        urlDvr: e.urlDvr || (null == i ? void 0 : i.urlDvr) || "",
                        urlUll: e.urlUll || (null == i ? void 0 : i.urlUll) || "",
                        urlTrailer: e.urlTrailer || "",
                        urlSource: e.urlSource || ""
                    }
                } else
                    this._mediaDetail = void 0
            }
            get isOwner() {
                return !!this.stores && this.channel.id === this.stores.userStore.user.id
            }
            fetchBannedWordsAndUpdate() {
                return Xt(this, null, function* () {
                    try {
                        const e = yield (0,
                            ot.A)("GET", "/users/me/banned-words", {
                                query: void 0,
                                body: void 0
                            });
                        (0,
                            _.j)(e);
                        const t = e.data.items;
                        (0,
                            o.h5)(() => {
                                this.bannedWords = t.map(e => this._toBannedWord(e))
                            }
                            )
                    } catch (e) {
                        e instanceof Error && x.A.addMessage({
                            variant: "danger",
                            text: e.message
                        })
                    }
                })
            }
            addBannedWord(e) {
                return Xt(this, null, function* () {
                    try {
                        const t = yield function (e = {}) {
                            return (0,
                                ot.A)("POST", "/users/me/banned-words", {
                                    query: void 0,
                                    body: e
                                })
                        }({
                            bannedWord: e,
                            matchType: 1
                        });
                        (0,
                            _.L)(t),
                            this.fetchBannedWordsAndUpdate()
                    } catch (e) {
                        e instanceof Error && x.A.addMessage({
                            variant: "danger",
                            text: e.message
                        })
                    }
                })
            }
            deleteBannedWord(e) {
                return Xt(this, null, function* () {
                    try {
                        const i = yield (t = e,
                            (0,
                                ot.A)("DELETE", `/users/me/banned-words/${t}`, {
                                    query: void 0,
                                    body: void 0
                                }));
                        (0,
                            _.L)(i),
                            this.fetchBannedWordsAndUpdate()
                    } catch (e) {
                        e instanceof Error && x.A.addMessage({
                            variant: "danger",
                            text: e.message
                        })
                    }
                    var t
                })
            }
            setBreakTimeAdCooldown(e) {
                this.breakTimeAdCooldown = e,
                    e > 0 && (window.clearTimeout(this._cooldownTimeoutId),
                        this._cooldownTimeoutId = window.setTimeout((0,
                            o.XI)(() => {
                                this.breakTimeAdCooldown = 0
                            }
                            ), e))
            }
            _getChapter(e) {
                const t = this.startedAt;
                if (t)
                    return [...this.chapters].reverse().find(i => qe.A.moment.substract(new Date(t), i.chapterAt.toDate()) <= e) || this.chapters[this.chapters.length - 1]
            }
            _showErrorMessage(e = "", t) {
                x.A.addMessage({
                    variant: t || "danger",
                    text: e
                })
            }
            _toSubsChannel(e) {
                var t, i;
                return {
                    id: (null == e ? void 0 : e.id) || "",
                    description: (null == e ? void 0 : e.description) || "",
                    bannerImageUrl: (null == e ? void 0 : e.bannerImageUrl) || "",
                    defaultBadgeImageUrl: (null == e ? void 0 : e.defaultBadgeImageUrl) || "",
                    status: (null == e ? void 0 : e.status) || "",
                    owner: {
                        id: (null == (t = null == e ? void 0 : e.owner) ? void 0 : t.id) || "",
                        nickname: (null == (i = null == e ? void 0 : e.owner) ? void 0 : i.nickname) || ""
                    },
                    payableWeb: (null == e ? void 0 : e.payableWeb) || !1,
                    payableIos: (null == e ? void 0 : e.payableIos) || !1,
                    payableAndroid: (null == e ? void 0 : e.payableAndroid) || !1
                }
            }
            _updateMedia(e, t) {
                this._media.url = (null == e ? void 0 : e.url) || (null == t ? void 0 : t.url) || "",
                    this._media.urlDvr = (null == e ? void 0 : e.urlDvr) || (null == t ? void 0 : t.urlDvr) || "",
                    this._media.urlUll = (null == e ? void 0 : e.urlUll) || (null == t ? void 0 : t.urlUll) || "",
                    this._media.urlTrailer = (null == e ? void 0 : e.urlTrailer) || "",
                    this._media.urlSource = (null == e ? void 0 : e.urlSource) || ""
            }
            _updateMediaDetail(e, t) {
                this._mediaDetail = {
                    url: ""
                },
                    this._mediaDetail.url = (null == e ? void 0 : e.url) || (null == t ? void 0 : t.url) || "",
                    this._mediaDetail.urlDvr = (null == e ? void 0 : e.urlDvr) || (null == t ? void 0 : t.urlDvr) || "",
                    this._mediaDetail.urlUll = (null == e ? void 0 : e.urlUll) || (null == t ? void 0 : t.urlUll) || "",
                    this._mediaDetail.urlTrailer = (null == e ? void 0 : e.urlTrailer) || "",
                    this._mediaDetail.urlSource = (null == e ? void 0 : e.urlSource) || ""
            }
            _updateBackupMedias(e) {
                this._backupMedias = e.map(e => {
                    var t, i, s, o, r, a, n, l, d;
                    return {
                        id: e.id || "",
                        media: {
                            url: (null == (t = e.media) ? void 0 : t.url) || "",
                            urlUll: (null == (i = e.media) ? void 0 : i.urlUll) || "",
                            urlDvr: (null == (s = e.media) ? void 0 : s.urlDvr) || "",
                            urlPublic: (null == (o = e.media) ? void 0 : o.urlPublic) || "",
                            urlTrailer: (null == (r = e.media) ? void 0 : r.urlTrailer) || "",
                            urlSource: (null == (a = e.media) ? void 0 : a.urlSource) || ""
                        },
                        subsTrialMedia: {
                            url: (null == (n = e.subsTrialMedia) ? void 0 : n.url) || "",
                            urlUll: (null == (l = e.subsTrialMedia) ? void 0 : l.urlUll) || "",
                            urlDvr: (null == (d = e.subsTrialMedia) ? void 0 : d.urlDvr) || ""
                        }
                    }
                }
                )
            }
            _updateBackupMediasDetail(e) {
                this._backupMediasDetail = e.map(e => {
                    var t, i, s, o, r, a, n, l, d;
                    return {
                        id: e.id || "",
                        media: {
                            url: (null == (t = e.media) ? void 0 : t.url) || "",
                            urlUll: (null == (i = e.media) ? void 0 : i.urlUll) || "",
                            urlDvr: (null == (s = e.media) ? void 0 : s.urlDvr) || "",
                            urlPublic: (null == (o = e.media) ? void 0 : o.urlPublic) || "",
                            urlTrailer: (null == (r = e.media) ? void 0 : r.urlTrailer) || "",
                            urlSource: (null == (a = e.media) ? void 0 : a.urlSource) || ""
                        },
                        subsTrialMedia: {
                            url: (null == (n = e.subsTrialMedia) ? void 0 : n.url) || "",
                            urlUll: (null == (l = e.subsTrialMedia) ? void 0 : l.urlUll) || "",
                            urlDvr: (null == (d = e.subsTrialMedia) ? void 0 : d.urlDvr) || ""
                        }
                    }
                }
                )
            }
            _toMembership(e) {
                var t, i, s, o, r, a, n, l, d, h, u, c, p, m, y, g, v;
                return {
                    isActive: !!e.isActive,
                    startedAt: e.startedAt || "",
                    subsChannel: this._toSubsChannel(e.subsChannel),
                    subsProduct: {
                        productId: (null == (t = e.subsProduct) ? void 0 : t.id) || "",
                        name: (null == (i = e.subsProduct) ? void 0 : i.name) || "",
                        tier: "number" == typeof (null == (s = e.subsProduct) ? void 0 : s.tier) ? null == (o = e.subsProduct) ? void 0 : o.tier : -1
                    },
                    badge: {
                        id: (null == (r = e.badge) ? void 0 : r.id) || 0,
                        imageUrl: (null == (a = e.badge) ? void 0 : a.imageUrl) || "",
                        months: (null == (l = null == (n = e.badge) ? void 0 : n.subscription) ? void 0 : l.months) || 0,
                        tier: (null == (h = null == (d = e.badge) ? void 0 : d.subscription) ? void 0 : h.tier) || 0,
                        label: (null == (u = e.badge) ? void 0 : u.label) || ""
                    },
                    charge: {
                        status: (null == (c = e.charge) ? void 0 : c.status) || "",
                        trialStartedAt: (null == (p = e.charge) ? void 0 : p.trialStartedAt) || "",
                        trialEndedAt: (null == (m = e.charge) ? void 0 : m.trialEndedAt) || "",
                        startedAt: (null == (y = e.charge) ? void 0 : y.trialStartedAt) || "",
                        endedAt: (null == (g = e.charge) ? void 0 : g.endedAt) || "",
                        autoRenewal: !!(null == (v = e.charge) ? void 0 : v.autoRenewal)
                    }
                }
            }
            _toBannedWord(e) {
                return {
                    id: e.id || 0,
                    words: e.word || "",
                    type: e.matchType || 0
                }
            }
            _toggleMovieAlert() {
                var e, t;
                this.isMemberOnly && !this.hasPermissionForMemberOnly ? null == (e = this.stores) || e.moviePageStore.showMovieAlert() : this.isMemberTrial && (null == (t = this.stores) || t.moviePageStore.hideMovieAlert())
            }
            _disposeLimitDialogShow() { }
            _disposeStores() { }
            _disposeMovies() { }
            isTestMode(e) {
                return !!this._testExtensions.find(t => t.id === e.id && t.installationId === e.installationId)
            }
        }
        Bt([o.sH.ref], Wt.prototype, "stores", 2),
            Bt([o.sH], Wt.prototype, "isCompletedFetchMovie", 2),
            Bt([o.sH], Wt.prototype, "isCompletedFetchMovieDetail", 2),
            Bt([o.sH], Wt.prototype, "isCompletedFetchSubsProduct", 2),
            Bt([o.sH], Wt.prototype, "isCompletedFetchVoteResult", 2),
            Bt([o.sH], Wt.prototype, "id", 2),
            Bt([o.sH], Wt.prototype, "movieId", 2),
            Bt([o.sH], Wt.prototype, "leagueId", 2),
            Bt([o.sH], Wt.prototype, "title", 2),
            Bt([o.sH], Wt.prototype, "introduction", 2),
            Bt([o.sH], Wt.prototype, "thumbnailUrl", 2),
            Bt([o.sH], Wt.prototype, "_casts", 2),
            Bt([o.sH], Wt.prototype, "_castsDetail", 2),
            Bt([o.EW], Wt.prototype, "casts", 1),
            Bt([o.sH], Wt.prototype, "tags", 2),
            Bt([o.sH], Wt.prototype, "next", 2),
            Bt([o.sH], Wt.prototype, "spriteImage", 2),
            Bt([o.sH], Wt.prototype, "totalViews", 2),
            Bt([o.sH], Wt.prototype, "liveViews", 2),
            Bt([o.sH], Wt.prototype, "totalYells", 2),
            Bt([o.sH], Wt.prototype, "monetizeStatus", 2),
            Bt([o.sH], Wt.prototype, "onairStatus", 2),
            Bt([o.sH], Wt.prototype, "enabledAd", 2),
            Bt([o.sH], Wt.prototype, "enabledYell", 2),
            Bt([o.sH], Wt.prototype, "width", 2),
            Bt([o.sH], Wt.prototype, "height", 2),
            Bt([o.sH], Wt.prototype, "playPostion", 2),
            Bt([o.sH], Wt.prototype, "orientation", 2),
            Bt([o.sH], Wt.prototype, "dvrOrientation", 2),
            Bt([o.sH], Wt.prototype, "deviceType", 2),
            Bt([o.sH], Wt.prototype, "movieType", 2),
            Bt([o.sH], Wt.prototype, "platformType", 2),
            Bt([o.sH], Wt.prototype, "encryptionType", 2),
            Bt([o.sH], Wt.prototype, "connectCount", 2),
            Bt([o.sH], Wt.prototype, "fixedScreen", 2),
            Bt([o.sH], Wt.prototype, "_media", 2),
            Bt([o.sH], Wt.prototype, "_mediaDetail", 2),
            Bt([o.EW], Wt.prototype, "media", 1),
            Bt([o.sH], Wt.prototype, "game", 2),
            Bt([o.sH], Wt.prototype, "ceroRating", 2),
            Bt([o.sH], Wt.prototype, "makerId", 2),
            Bt([o.sH.ref], Wt.prototype, "chapters", 2),
            Bt([o.sH], Wt.prototype, "chapterPosition", 2),
            Bt([o.sH], Wt.prototype, "channel", 2),
            Bt([o.sH], Wt.prototype, "blacklist", 2),
            Bt([o.sH], Wt.prototype, "ad", 2),
            Bt([o.sH], Wt.prototype, "viewsLimit", 2),
            Bt([o.sH], Wt.prototype, "settings", 2),
            Bt([o.sH.ref], Wt.prototype, "playableChannelIdsOnMoblie", 2),
            Bt([o.sH.ref], Wt.prototype, "fesEntries", 2),
            Bt([o.sH.ref], Wt.prototype, "poll", 2),
            Bt([o.sH], Wt.prototype, "pollUseTarget", 2),
            Bt([o.sH.ref], Wt.prototype, "voteResult", 2),
            Bt([o.sH], Wt.prototype, "castYellMembers", 2),
            Bt([o.sH], Wt.prototype, "castYellHistory", 2),
            Bt([o.sH], Wt.prototype, "isSelectedCast", 2),
            Bt([o.sH], Wt.prototype, "isMemberTrialMessageVisible", 2),
            Bt([o.sH], Wt.prototype, "isFesEventIconClosed", 2),
            Bt([o.sH], Wt.prototype, "isEnabledInitialExtension", 2),
            Bt([o.sH], Wt.prototype, "displayExtensionNotifications", 2),
            Bt([o.sH], Wt.prototype, "isExtensionSystemChatClick", 2),
            Bt([o.sH], Wt.prototype, "bannedWords", 2),
            Bt([o.sH], Wt.prototype, "_releasedExtensions", 2),
            Bt([o.sH], Wt.prototype, "_testExtensions", 2),
            Bt([o.sH], Wt.prototype, "notInstallExtensions", 2),
            Bt([o.sH], Wt.prototype, "breakTimeAdCooldown", 2),
            Bt([o.EW], Wt.prototype, "backupMedias", 1),
            Bt([o.EW], Wt.prototype, "canPlayOnMobile", 1),
            Bt([o.EW], Wt.prototype, "toCastYellUser", 1),
            Bt([o.EW], Wt.prototype, "isArchivePlayable", 1),
            Bt([o.EW], Wt.prototype, "isArchive", 1),
            Bt([o.EW], Wt.prototype, "isComingUp", 1),
            Bt([o.EW], Wt.prototype, "isLiveStreaming", 1),
            Bt([o.EW], Wt.prototype, "isFinishedMedia", 1),
            Bt([o.EW], Wt.prototype, "isUserChatPublic", 1),
            Bt([o.EW], Wt.prototype, "isChatPublicTypeMember", 1),
            Bt([o.EW], Wt.prototype, "isUserChatReceivable", 1),
            Bt([o.EW], Wt.prototype, "isPlayable", 1),
            Bt([o.EW], Wt.prototype, "isSecret", 1),
            Bt([o.EW], Wt.prototype, "isArchiveCreating", 1),
            Bt([o.EW], Wt.prototype, "isConverting", 1),
            Bt([o.EW], Wt.prototype, "failedUpload", 1),
            Bt([o.EW], Wt.prototype, "isCeroZ", 1),
            Bt([o.EW], Wt.prototype, "isLeague", 1),
            Bt([o.EW], Wt.prototype, "primaryTag", 1),
            Bt([o.EW], Wt.prototype, "userIds", 1),
            Bt([o.EW], Wt.prototype, "existVoteResult", 1),
            Bt([o.EW], Wt.prototype, "isAes", 1),
            Bt([o.EW], Wt.prototype, "membershipMap", 1),
            Bt([o.EW], Wt.prototype, "v8MembershipMap", 1),
            Bt([o.EW], Wt.prototype, "currentChapter", 1),
            Bt([o.EW], Wt.prototype, "chapterOnFirstPlay", 1),
            Bt([o.EW], Wt.prototype, "currentGame", 1),
            Bt([o.EW], Wt.prototype, "playPositionOnFirstPlay", 1),
            Bt([o.EW], Wt.prototype, "isMemberTrial", 1),
            Bt([o.EW], Wt.prototype, "isMemberOnly", 1),
            Bt([o.EW], Wt.prototype, "isMemberOnlyPlayable", 1),
            Bt([o.EW], Wt.prototype, "isMemberOnlyChatPostable", 1),
            Bt([o.EW], Wt.prototype, "isMemberOnlyCommentPostable", 1),
            Bt([o.EW], Wt.prototype, "hasPermissionForMemberOnly", 1),
            Bt([o.EW], Wt.prototype, "isSpecial", 1),
            Bt([o.EW], Wt.prototype, "isSpecialPlayable", 1),
            Bt([o.EW], Wt.prototype, "isTrailer", 1),
            Bt([o.EW], Wt.prototype, "openedSubscription", 1),
            Bt([o.EW], Wt.prototype, "isSubsEnabled", 1),
            Bt([o.EW], Wt.prototype, "lowestSubsProduct", 1),
            Bt([o.EW], Wt.prototype, "isLowestSubsProductPermitted", 1),
            Bt([o.EW], Wt.prototype, "permittedSubsProduct", 1),
            Bt([o.EW], Wt.prototype, "subsProductsGreaterThanOrEqualToPermittedSubsProduct", 1),
            Bt([o.EW], Wt.prototype, "hasSubscribed", 1),
            Bt([o.EW], Wt.prototype, "hasSubscribedGreaterThanOrEqualToPermittedSubsProduct", 1),
            Bt([o.EW], Wt.prototype, "isPpvEnabled", 1),
            Bt([o.EW], Wt.prototype, "hasPpvTicketProducts", 1),
            Bt([o.EW], Wt.prototype, "isFesEventIconVisible", 1),
            Bt([o.EW], Wt.prototype, "extensions", 1),
            Bt([o.XI], Wt.prototype, "willLoadOnServer", 1),
            Bt([o.XI], Wt.prototype, "willLoad", 1),
            Bt([o.XI], Wt.prototype, "willUnload", 1),
            Bt([o.XI], Wt.prototype, "resetForTransition", 1),
            Bt([o.XI], Wt.prototype, "handlePlay", 1),
            Bt([s.A], Wt.prototype, "handleVideoEnded", 1),
            Bt([o.XI], Wt.prototype, "handleImaSdkEvent", 1),
            Bt([o.XI.bound], Wt.prototype, "handleViewingTimeUpdate", 1),
            Bt([o.XI], Wt.prototype, "_updateMovie", 1),
            Bt([o.XI], Wt.prototype, "fetchMovieAndUpdate", 1),
            Bt([o.XI], Wt.prototype, "revalidateMovieDetail", 1),
            Bt([o.XI], Wt.prototype, "fetchMovieDetailAndUpdate", 1),
            Bt([o.XI.bound], Wt.prototype, "updateMovieDetail", 1),
            Bt([o.XI], Wt.prototype, "fetchMovieDetailAndUpdateDeviceLimit", 1),
            Bt([o.XI], Wt.prototype, "fetchFesEntriesAndUpdate", 1),
            Bt([o.XI], Wt.prototype, "fetchSubsProductsOfChannelAndUpdate", 1),
            Bt([o.XI.bound], Wt.prototype, "switchToWatchingOnThisDevice", 1),
            Bt([o.XI.bound], Wt.prototype, "stopToWatchingOnThisDevice", 1),
            Bt([o.XI.bound], Wt.prototype, "watchArchive", 1),
            Bt([o.XI.bound], Wt.prototype, "fetchTeamsAndUpdate", 1),
            Bt([o.XI.bound], Wt.prototype, "fetchVoteResultAndUpdate", 1),
            Bt([o.XI], Wt.prototype, "updateCastYellHistory", 1),
            Bt([o.XI], Wt.prototype, "updateDisplayExtension", 1),
            Bt([o.XI], Wt.prototype, "follow", 1),
            Bt([o.XI], Wt.prototype, "unfollow", 1),
            Bt([o.XI.bound], Wt.prototype, "updateFollowing", 1),
            Bt([o.XI.bound], Wt.prototype, "enablePush", 1),
            Bt([o.XI.bound], Wt.prototype, "diablePush", 1),
            Bt([o.XI.bound], Wt.prototype, "fetchMembershipsAndUpdate", 1),
            Bt([o.XI.bound], Wt.prototype, "updateViews", 1),
            Bt([o.XI], Wt.prototype, "updateOnairStatus", 1),
            Bt([o.XI], Wt.prototype, "updateChatPostedPosition", 1),
            Bt([o.XI], Wt.prototype, "finishMedia", 1),
            Bt([o.XI], Wt.prototype, "startMedia", 1),
            Bt([o.XI.bound], Wt.prototype, "updateBroadcastInfo", 1),
            Bt([o.XI.bound], Wt.prototype, "updateReceivedBroadcastInfo", 1),
            Bt([o.XI.bound], Wt.prototype, "updateTitle", 1),
            Bt([o.XI.bound], Wt.prototype, "updateIntroduction", 1),
            Bt([o.XI.bound], Wt.prototype, "updatePreIsFixedPhrase", 1),
            Bt([o.XI.bound], Wt.prototype, "updateIsFixedPhrase", 1),
            Bt([o.XI.bound], Wt.prototype, "updateCastFollow", 1),
            Bt([o.XI.bound], Wt.prototype, "rotatePlayer", 1),
            Bt([o.XI.bound], Wt.prototype, "rotatePlayerWhenRotateButtonClick", 1),
            Bt([o.XI.bound], Wt.prototype, "updatePpv", 1),
            Bt([o.XI.bound], Wt.prototype, "updatePermissionsFromLegacy", 1),
            Bt([o.XI.bound], Wt.prototype, "updatePermissions", 1),
            Bt([o.XI.bound], Wt.prototype, "updatePollFromLegacy", 1),
            Bt([o.XI.bound], Wt.prototype, "updatePoll", 1),
            Bt([o.XI.bound], Wt.prototype, "updateVoteResult", 1),
            Bt([o.XI.bound], Wt.prototype, "initPoll", 1),
            Bt([o.XI.bound], Wt.prototype, "updateIsViewersHidden", 1),
            Bt([o.XI.bound], Wt.prototype, "updateChaptersFromLegacy", 1),
            Bt([o.XI.bound], Wt.prototype, "updateChapters", 1),
            Bt([o.XI.bound], Wt.prototype, "selectChapterManually", 1),
            Bt([o.XI.bound], Wt.prototype, "deleteChapter", 1),
            Bt([o.XI.bound], Wt.prototype, "getChapter", 1),
            Bt([o.XI], Wt.prototype, "updateSelectedChapter", 1),
            Bt([o.XI.bound], Wt.prototype, "closeFesEventIcon", 1),
            Bt([o.XI.bound], Wt.prototype, "addDisplayExtension", 1),
            Bt([o.XI.bound], Wt.prototype, "setDisplayExtension", 1),
            Bt([o.XI.bound], Wt.prototype, "removeDisplayExtensin", 1),
            Bt([o.XI.bound], Wt.prototype, "removeDisplayExtensionNotification", 1),
            Bt([o.XI.bound], Wt.prototype, "setIsExtensionSystemChatClick", 1),
            Bt([o.XI.bound], Wt.prototype, "setPlayPosition", 1),
            Bt([o.XI.bound], Wt.prototype, "setIsAutoPlay", 1),
            Bt([o.XI.bound], Wt.prototype, "setMembership", 1),
            Bt([o.XI], Wt.prototype, "startLiveStreaming", 1),
            Bt([o.XI], Wt.prototype, "updateCastYellMembers", 1),
            Bt([o.XI.bound], Wt.prototype, "showMemberTrialMessage", 1),
            Bt([o.XI.bound], Wt.prototype, "hideMemberTrialMessage", 1),
            Bt([o.XI.bound], Wt.prototype, "showViewsLimitDialog", 1),
            Bt([o.XI.bound], Wt.prototype, "resetMedia", 1),
            Bt([o.XI], Wt.prototype, "switchToBackupMedia", 1),
            Bt([o.EW], Wt.prototype, "isOwner", 1),
            Bt([o.XI.bound], Wt.prototype, "fetchBannedWordsAndUpdate", 1),
            Bt([o.XI.bound], Wt.prototype, "addBannedWord", 1),
            Bt([o.XI.bound], Wt.prototype, "deleteBannedWord", 1),
            Bt([o.XI.bound], Wt.prototype, "setBreakTimeAdCooldown", 1),
            Bt([o.XI.bound], Wt.prototype, "_toSubsChannel", 1),
            Bt([o.XI.bound], Wt.prototype, "_toMembership", 1),
            Bt([o.XI.bound], Wt.prototype, "_toBannedWord", 1),
            Bt([o.XI.bound], Wt.prototype, "_toggleMovieAlert", 1);
        const Nt = () => location.pathname.replace(/\/live\//, "").replace(/\/movie\//, "");
        var Vt = i(16980)
            , Yt = i(98665)
            , Qt = Object.defineProperty
            , qt = Object.getOwnPropertyDescriptor
            , jt = (e, t, i) => new Promise((s, o) => {
                var r = e => {
                    try {
                        n(i.next(e))
                    } catch (e) {
                        o(e)
                    }
                }
                    , a = e => {
                        try {
                            n(i.throw(e))
                        } catch (e) {
                            o(e)
                        }
                    }
                    , n = e => e.done ? s(e.value) : Promise.resolve(e.value).then(r, a);
                n((i = i.apply(e, t)).next())
            }
            );
        class zt extends Vt.A {
            constructor() {
                super(),
                    (0,
                        q.H)(this, {
                            playlist: o.sH
                        })
            }
            willLoad(e) {
                return jt(this, null, function* () {
                    yield this.updatePlaylist(e.appStore.currentQueryInfo.playlist_id),
                        this.when(() => e.userStore.isLogined, () => this.updateMyPlaylist(e.appStore.currentQueryInfo.playlist_id))
                })
            }
            willUnload() {
                this.playlistId = void 0,
                    this.playlist = void 0,
                    super.destroy()
            }
            updatePlaylist(e) {
                return jt(this, null, function* () {
                    if (this._setPlaylistId(e),
                        e)
                        try {
                            const t = yield Yt.A.getById(e);
                            this._setPlaylist(t)
                        } catch (e) { }
                    else
                        this._setPlaylist(void 0)
                })
            }
            updateMyPlaylist(e) {
                return jt(this, null, function* () {
                    if (this._setPlaylistId(e),
                        e)
                        try {
                            const t = yield Yt.A.getUsersById(e);
                            this._setPlaylist(t)
                        } catch (e) { }
                    else
                        this._setPlaylist(void 0)
                })
            }
            _setPlaylistId(e) {
                this.playlistId = e
            }
            _setPlaylist(e) {
                this.playlist = e
            }
        }
        ((e, t, i) => {
            for (var s, o = qt(t, i), r = e.length - 1; r >= 0; r--)
                (s = e[r]) && (o = s(t, i, o) || o);
            o && Qt(t, i, o)
        }
        )([o.XI], zt.prototype, "_setPlaylist");
        var Gt = i(65516)
            , Kt = Object.defineProperty
            , $t = Object.getOwnPropertyDescriptor
            , Zt = (e, t, i, s) => {
                for (var o, r = s > 1 ? void 0 : s ? $t(t, i) : t, a = e.length - 1; a >= 0; a--)
                    (o = e[a]) && (r = (s ? o(t, i, r) : o(r)) || r);
                return s && r && Kt(t, i, r),
                    r
            }
            ;
        class Jt {
            constructor() {
                this.stores = null,
                    this.isClosed = !1,
                    this.isOpenedPopout = !1,
                    (0,
                        o.Gn)(this)
            }
            willLoad(e) {
                return t = this,
                    i = function* () {
                        this.stores = e
                    }
                    ,
                    new Promise((e, s) => {
                        var o = e => {
                            try {
                                a(i.next(e))
                            } catch (e) {
                                s(e)
                            }
                        }
                            , r = e => {
                                try {
                                    a(i.throw(e))
                                } catch (e) {
                                    s(e)
                                }
                            }
                            , a = t => t.done ? e(t.value) : Promise.resolve(t.value).then(o, r);
                        a((i = i.apply(t, null)).next())
                    }
                    );
                var t, i
            }
            willUnload() {
                this.isClosed = !1
            }
            show() {
                this.isClosed = !1
            }
            hide() {
                this.isClosed = !0
            }
            openPollEditPopout() {
                if (!this.stores)
                    return;
                const e = this.stores.moviePageStore
                    , t = e.movieStore
                    , i = (0,
                        Gt.A)().openPollEdit(t.id)
                    , s = (0,
                        Gt.A)().onclose(i, this.handlePopoutClose);
                e.aborts.push(s),
                    this.isOpenedPopout = !0
            }
            handlePopoutClose() {
                this.isOpenedPopout = !1
            }
            handleVoteSelect(e) {
                if (!this.stores)
                    return;
                const t = this.stores.moviePageStore.movieStore
                    , i = t.poll.votes.find(t => t.index === e);
                i && t.updateVoteResult(i)
            }
            appealLogin() {
                if (!this.stores)
                    return;
                const e = this.stores.dialogStore
                    , t = this.stores.langStore
                    , i = [{
                        label: t.common.cancel,
                        variant: "cancel",
                        onClick: e.hideDialog
                    }, {
                        label: t.common.login,
                        onClick: () => {
                            e.hideDialog(),
                                pt.A.show(Mt.A.socialLoginDialogId)
                        }
                    }];
                e.showDialog({
                    type: "system",
                    system: {
                        message: t.poll.messageConfirmPollLogin,
                        buttons: i
                    }
                })
            }
            appealFollow(e) {
                if (!this.stores)
                    return;
                const t = this.stores.dialogStore
                    , i = this.stores.langStore
                    , s = [{
                        label: i.common.cancel,
                        variant: "cancel",
                        onClick: t.hideDialog
                    }, {
                        label: i.user.follow,
                        onClick: () => {
                            t.hideDialog(),
                                e()
                        }
                    }];
                t.showDialog({
                    type: "system",
                    system: {
                        message: i.poll.messageConfirmPollFollow,
                        buttons: s
                    }
                })
            }
            appealSubscription() {
                if (!this.stores)
                    return;
                const e = this.stores.dialogStore
                    , t = this.stores.langStore
                    , i = [{
                        label: t.common.cancel,
                        variant: "cancel",
                        onClick: e.hideDialog
                    }, {
                        label: t.subscription.subscribe,
                        onClick: () => {
                            e.hideDialog(),
                                this._openUserSubscription()
                        }
                    }];
                e.showDialog({
                    type: "system",
                    system: {
                        message: t.poll.messageConfirmPollSubscription,
                        buttons: i
                    }
                })
            }
            _openUserSubscription() {
                if (!this.stores)
                    return;
                const e = this.stores.moviePageStore
                    , t = e.movieStore
                    , i = (0,
                        Gt.A)().openUserSubscription(t.channel.id)
                    , s = (0,
                        Gt.A)().onclose(i, t.fetchMembershipsAndUpdate);
                e.aborts.push(s)
            }
        }
        Zt([o.sH], Jt.prototype, "isClosed", 2),
            Zt([o.sH], Jt.prototype, "isOpenedPopout", 2),
            Zt([o.XI], Jt.prototype, "willLoad", 1),
            Zt([o.XI], Jt.prototype, "willUnload", 1),
            Zt([o.XI.bound], Jt.prototype, "show", 1),
            Zt([o.XI.bound], Jt.prototype, "hide", 1),
            Zt([o.XI], Jt.prototype, "openPollEditPopout", 1),
            Zt([o.XI.bound], Jt.prototype, "handlePopoutClose", 1),
            Zt([s.A], Jt.prototype, "handleVoteSelect", 1),
            Zt([s.A], Jt.prototype, "appealLogin", 1),
            Zt([s.A], Jt.prototype, "appealFollow", 1),
            Zt([s.A], Jt.prototype, "appealSubscription", 1),
            Zt([s.A], Jt.prototype, "_openUserSubscription", 1);
        var ei = i(18987)
            , ti = i(57698)
            , ii = i.n(ti)
            , si = i(71141)
            , oi = i(25221)
            , ri = i(32605)
            , ai = i(6174)
            , ni = Object.defineProperty
            , li = Object.getOwnPropertyDescriptor
            , di = (e, t, i, s) => {
                for (var o, r = s > 1 ? void 0 : s ? li(t, i) : t, a = e.length - 1; a >= 0; a--)
                    (o = e[a]) && (r = (s ? o(t, i, r) : o(r)) || r);
                return s && r && ni(t, i, r),
                    r
            }
            , hi = (e, t, i) => new Promise((s, o) => {
                var r = e => {
                    try {
                        n(i.next(e))
                    } catch (e) {
                        o(e)
                    }
                }
                    , a = e => {
                        try {
                            n(i.throw(e))
                        } catch (e) {
                            o(e)
                        }
                    }
                    , n = e => e.done ? s(e.value) : Promise.resolve(e.value).then(r, a);
                n((i = i.apply(e, t)).next())
            }
            );
        class ui {
            constructor() {
                this.stores = null,
                    this.chatHost = "",
                    this.parentWindow = null,
                    this.messages = o.sH.array(),
                    this._currentSocket = null,
                    this._viewersSocket = null,
                    (0,
                        o.Gn)(this)
            }
            willLoad(e) {
                return hi(this, null, function* () {
                    this.stores = e;
                    const t = e.moviePageStore;
                    (0,
                        ve.isSameDomain)((0,
                            y.A)().document.referrer) && (0,
                                y.A)().opener && (t.isChatPopoutPage || t.isExtensionPopoutPage || t.isSplitViewPopoutPage) && (this.parentWindow = (0,
                                    y.A)().opener,
                                    this._addParentWindowEvents(),
                                    e.appStore.registerSocketHandler(this._handleSocketMessageFromParentWindow)),
                        this._disposeConnectionSwitchingAutorun = this._setConnectionSwitchingAutorun()
                })
            }
            willUnload() {
                var e;
                this._disconnectSocket(),
                    this._disposeConnectionSwitchingAutorun(),
                    null == (e = this.stores) || e.appStore.unregisterSocketHandler(this._handleSocketMessageFromParentWindow),
                    this._removeParentWindowEvents(),
                    this.parentWindow = null,
                    this.messages.clear(),
                    this.stores = null
            }
            get isConnecting() {
                return !!this._currentSocket && !!this._viewersSocket
            }
            get canConnect() {
                return "available" === this.connectionStatus
            }
            get connectionStatus() {
                const e = this.stores;
                if (!e)
                    return "none";
                const t = e.userStore
                    , i = e.moviePageStore
                    , s = i.movieStore;
                return s.movieId && (s.isLiveStreaming || s.isComingUp) ? (i.isChatPopoutPage || i.isExtensionPopoutPage || i.isSplitViewPopoutPage) && this.parentWindow ? "synchronizableParentWindow" : t.isCompletedInitialFetch ? t.isLogined ? s.isCompletedFetchMovieDetail ? s.interceptsDeviceLimit ? "interceptsDeviceLimit" : s.exceedsDeviceLimit ? "exceedsDeviceLimit" : "available" : "none" : "available" : "none" : "unavailable"
            }
            _initSocket(e, t) {
                return hi(this, null, function* () {
                    var i;
                    const s = e.moviePageStore.movieStore;
                    this.chatHost || (yield this._fetchChatHostAndUpdate());
                    const o = {
                        movieId: s.movieId,
                        uuid: ei.A.get(ri.b0),
                        referrer: (null == (i = this.stores) ? void 0 : i.appStore.currentUrl) || (0,
                            y.A)().location.href,
                        connectAt: Math.floor(Date.now() / 1e3),
                        connectionId: t,
                        isExcludeLiveViewers: !0
                    }
                        , r = ei.A.get(ri.y6);
                    r && (o.accessToken = r),
                        s.chatSecretKey && (o.chatSecretKey = s.chatSecretKey);
                    const a = {
                        query: o,
                        transports: ["websocket"],
                        reconnectionAttempts: ai.o
                    };
                    return ii()(this.chatHost, a)
                })
            }
            _connectSocket() {
                return hi(this, null, function* () {
                    if (!this.stores)
                        return;
                    if (!this.canConnect)
                        return;
                    const { movieStore: e } = this.stores.moviePageStore;
                    this._disconnectSocket();
                    const t = (0,
                        a.A)();
                    this._currentSocket = yield this._initSocket(this.stores, t),
                        this._currentSocket.on("message", this.stores.moviePageStore.handleSocketMessage),
                        this._currentSocket.on("error", this._handleSocketError),
                        this._currentSocket.on("reconnect_failed", () => this._handleSocketReconnectFailed(() => {
                            var e;
                            null == (e = this._currentSocket) || e.connect()
                        }
                        )),
                        this._viewersSocket = yield oi.Ay.connectViewersSocket(e.movieId, {
                            connectionId: t
                        }),
                        this._viewersSocket.io.on("reconnect_failed", () => this._handleSocketReconnectFailed(() => {
                            var e;
                            null == (e = this._viewersSocket) || e.connect()
                        }
                        )),
                        ["message", "error"].forEach(e => {
                            var t;
                            null == (t = this._currentSocket) || t.on(e, this._emitSocketToChildWindows.bind(this, e))
                        }
                        )
                })
            }
            _disconnectSocket() {
                this._currentSocket && (this._currentSocket.close(),
                    this._currentSocket = null),
                    this._viewersSocket && (this._viewersSocket.close(),
                        this._viewersSocket = null)
            }
            _setConnectionSwitchingAutorun() {
                return (0,
                    o.fm)(() => {
                        this.canConnect ? this._connectSocket() : this._disconnectSocket()
                    }
                    )
            }
            _addParentWindowEvents() {
                var e;
                try {
                    null == (e = this.parentWindow) || e.addEventListener("unload", this._handleParentWindowUnload)
                } catch (e) {
                    this._removeParentWindow()
                }
            }
            _removeParentWindowEvents() {
                var e;
                try {
                    null == (e = this.parentWindow) || e.removeEventListener("unload", this._handleParentWindowUnload)
                } catch (e) {
                    (0,
                        o.h5)(() => {
                            (0,
                                y.A)().opener = null,
                                this.parentWindow = null
                        }
                        )
                }
            }
            _removeParentWindow() {
                this._removeParentWindowEvents(),
                    (0,
                        y.A)().opener = null,
                    this.parentWindow = null
            }
            _emitSocketToChildWindows(e, t) {
                if (!this.stores)
                    return;
                const i = this.stores.moviePageStore;
                [i.chatPopoutWindow, i.exntensionPopoutWindow, i.splitViewStore.splitViewPopoutWindow].filter(e => e).forEach(i => i.onSocket(e, t))
            }
            _fetchChatHostAndUpdate() {
                return hi(this, null, function* () {
                    try {
                        const e = yield (0,
                            si.K)();
                        (0,
                            o.h5)(() => {
                                this.chatHost = e.data || ""
                            }
                            )
                    } catch (e) { }
                })
            }
            setSocketMessage(e) {
                if (0 === e.type) {
                    if (!("yell" in e.data && e.data.yell || "capture" in e.data && e.data.capture))
                        return
                } else if (3 === e.type && this.messages.find(({ socketMessage: e }) => 3 === e.type))
                    return;
                const t = this.messages.concat({
                    id: Date.now(),
                    socketMessage: e
                }).slice(-10);
                this.messages.replace(t)
            }
            _handleSocketError(e) {
                console.error(e)
            }
            _handleSocketReconnectFailed(e) {
                this.stores && x.A.addMessage({
                    variant: "danger",
                    text: this.stores.langStore.error.occurred,
                    unlimited: !0,
                    buttonLabel: this.stores.langStore.common.reDo,
                    onButtonClick() {
                        e()
                    }
                })
            }
            _handleSocketMessageFromParentWindow(e, t) {
                const i = this.stores;
                if (!i)
                    return;
                const s = i.moviePageStore;
                if (s.isChatPopoutPage || s.isExtensionPopoutPage || s.isSplitViewPopoutPage)
                    switch (e) {
                        case "message":
                            return s.handleSocketMessage(t || "");
                        case "error":
                            return this._handleSocketError(t || "")
                    }
            }
            _handleParentWindowUnload() {
                this._removeParentWindow()
            }
            _disposeConnectionSwitchingAutorun() { }
        }
        di([o.sH.ref], ui.prototype, "stores", 2),
            di([o.sH.ref], ui.prototype, "parentWindow", 2),
            di([o.sH], ui.prototype, "messages", 2),
            di([o.sH.ref], ui.prototype, "_currentSocket", 2),
            di([o.sH], ui.prototype, "_viewersSocket", 2),
            di([o.XI], ui.prototype, "willLoad", 1),
            di([o.XI], ui.prototype, "willUnload", 1),
            di([o.EW], ui.prototype, "isConnecting", 1),
            di([o.EW], ui.prototype, "canConnect", 1),
            di([o.EW], ui.prototype, "connectionStatus", 1),
            di([o.XI], ui.prototype, "_connectSocket", 1),
            di([s.A], ui.prototype, "_setConnectionSwitchingAutorun", 1),
            di([s.A], ui.prototype, "_addParentWindowEvents", 1),
            di([s.A], ui.prototype, "_removeParentWindowEvents", 1),
            di([o.XI.bound], ui.prototype, "_removeParentWindow", 1),
            di([s.A], ui.prototype, "_emitSocketToChildWindows", 1),
            di([o.XI], ui.prototype, "_fetchChatHostAndUpdate", 1),
            di([s.A], ui.prototype, "setSocketMessage", 1),
            di([s.A], ui.prototype, "_handleSocketError", 1),
            di([s.A], ui.prototype, "_handleSocketReconnectFailed", 1),
            di([s.A], ui.prototype, "_handleSocketMessageFromParentWindow", 1),
            di([o.XI.bound], ui.prototype, "_handleParentWindowUnload", 1);
        var ci = Object.defineProperty
            , pi = Object.getOwnPropertyDescriptor
            , mi = (e, t, i, s) => {
                for (var o, r = s > 1 ? void 0 : s ? pi(t, i) : t, a = e.length - 1; a >= 0; a--)
                    (o = e[a]) && (r = (s ? o(t, i, r) : o(r)) || r);
                return s && r && ci(t, i, r),
                    r
            }
            ;
        class yi {
            constructor() {
                this.notifications = o.sH.array(),
                    this.isOpenedSplitView = !1,
                    this.notificationQue = [],
                    this.notificationPool = [],
                    this.isScrolledByAuto = !1,
                    this.isScrollingByAuto = !1,
                    this.isLastPage = !1,
                    this.isTracing = !1,
                    this.splitViewPopoutWindow = null,
                    this._preLastNotificationCell = null,
                    this._preFirstNotificationCellId = null,
                    this._systemChatId = 0,
                    this._everOpenedSplitView = !1,
                    (0,
                        q.H)(this, {
                            scrollListEl: o.sH
                        })
            }
            setScrollTop(e) {
                this.scrollTop = e
            }
            setScrollEnd(e) {
                this.preScrollEnd = this.scrollEnd,
                    this.scrollEnd = e
            }
            isScrollBottom() {
                return this.scrollEnd - this.scrollTop < v.oN
            }
            isPreScrollBottom() {
                return this.preScrollEnd - this.scrollTop < v.oN
            }
            scrollToEnd(e = {}) {
                const t = e.scrollEnd || this.scrollEnd
                    , i = this.scrollListEl;
                if (i) {
                    if (this._cancelScroll && (this._cancelScroll(),
                        delete this._cancelScroll),
                        this.startScrollByAuto(),
                        !e.enabledAnimation || (0,
                            y.A)().fps < v.uE || C.ie())
                        return this.setScrollTop(t),
                            void (0,
                                y.A)().requestAnimationFrame(() => {
                                    i.scrollTop = t,
                                        this.finishScrollByAuto()
                                }
                                );
                    this._cancelScroll = S.scrollTo(i, t, v.ap, b.easeOutCubic, () => {
                        this.setScrollTop(t),
                            this.finishScrollByAuto()
                    }
                    )
                }
            }
            setScrollListEl(e) {
                this.scrollListEl = e
            }
            setSplitViewPopoutWindow(e) {
                this.splitViewPopoutWindow = e
            }
            closeSplitViewPopoutWindow() {
                this.splitViewPopoutWindow && (this.splitViewPopoutWindow.closed || this.splitViewPopoutWindow.close(),
                    this.splitViewPopoutWindow = null)
            }
            willLoad(e) {
                return t = this,
                    i = function* () {
                        this.stores = e
                    }
                    ,
                    new Promise((e, s) => {
                        var o = e => {
                            try {
                                a(i.next(e))
                            } catch (e) {
                                s(e)
                            }
                        }
                            , r = e => {
                                try {
                                    a(i.throw(e))
                                } catch (e) {
                                    s(e)
                                }
                            }
                            , a = t => t.done ? e(t.value) : Promise.resolve(t.value).then(o, r);
                        a((i = i.apply(t, null)).next())
                    }
                    );
                var t, i
            }
            willUnload() {
                (0,
                    y.A)().clearInterval(this._notificationAddIntervalId),
                    (0,
                        y.A)().cancelAnimationFrame(this._notificationQueAnimationFrame),
                    this._notificationAddIntervalId = 0,
                    this._notificationQueAnimationFrame = 0,
                    this.notifications.clear(),
                    this.notificationQue = [],
                    this.notificationPool = [],
                    delete this.stores
            }
            popNotificationQue() {
                if (!this.canPopNotificationQue())
                    return;
                if (!this.notificationQue.length)
                    return;
                const e = this.notifications.concat(this.notificationQue);
                if (this.isScrollBottom()) {
                    const t = e.splice(-v.H5);
                    this.notifications.replace(t),
                        this.notifications.length >= v.H5 && (this.isLastPage = !1),
                        this.notificationQue = []
                } else {
                    const t = e.splice(0, v.o_);
                    this.notifications.replace(t),
                        this.notificationQue = e.slice(-v.Nz)
                }
                this._firstOpenSplitview()
            }
            traceNotifications() {
                if (this.isLastPage)
                    return;
                const e = this.notificationPool.slice(-this.notifications.length - v.H5);
                this.notifications.length !== e.length ? (this.isTracing = !0,
                    this.notifications.replace(e)) : this.isLastPage = !0
            }
            toggleSplitView() {
                this.isOpenedSplitView = !this.isOpenedSplitView
            }
            openSplitView() {
                this.isOpenedSplitView = !0
            }
            addNotificationChat(e) {
                this.notificationQue.push(e),
                    this.notificationPool.push(e)
            }
            addPurchaseNotification(e, t) {
                const i = {
                    message: t,
                    id: this._systemChatId--,
                    isNotificationOfSubs: "subs" === e,
                    isNotificationOfPpvTicketPurchase: "ppv" === e,
                    postedAt: (new Date).toISOString(),
                    deletable: !1,
                    disabled: !1
                };
                this.notificationQue.push(i),
                    this.notificationPool.push(i)
            }
            canPopNotificationQue() {
                return !this.isScrollingByAuto
            }
            resetTracing() {
                this.isTracing = !1
            }
            scrollByAuto() {
                if (this.scrollListEl) {
                    if (this.hasBottomNotificationListDiffrence()) {
                        if (this.isPreScrollBottom())
                            return this.isScrolledByAuto || this.setScrolledByAuto(!0),
                                void this.scrollToEnd({
                                    enabledAnimation: !0
                                });
                        this.setScrolledByAuto(!1)
                    }
                    return this.isTracing && this._hasTopNotificationListDiffrence() ? (this.resetTracing(),
                        void (this.preScrollEnd && (this.scrollListEl.scrollTop = this.scrollEnd - this.preScrollEnd))) : void 0
                }
            }
            startScrollByAuto() {
                this.isScrollingByAuto = !0
            }
            finishScrollByAuto() {
                this.isScrollingByAuto = !1
            }
            addBlackList(e) {
                const t = this.notifications.map(t => (t.userId === e && (t.isBlacklist = !0),
                    t));
                this.notifications.replace(t)
            }
            deleteBlackList(e) {
                const t = this.notifications.map(t => (t.userId === e && (t.isBlacklist = !1),
                    t));
                this.notifications.replace(t)
            }
            hasBottomNotificationListDiffrence() {
                const e = this.notifications[this.notifications.length - 1];
                return !!(e && this._preLastNotificationCell && (this._preLastNotificationCell.id !== e.id || this._preLastNotificationCell.isSystemMessage && e.isSystemMessage && this._preLastNotificationCell.postedAt !== e.postedAt))
            }
            setScrolledByAuto(e) {
                this.isScrolledByAuto = e
            }
            setPreLastNotificationCell(e) {
                this._preLastNotificationCell = e
            }
            setPreFirstNotificationCellId(e) {
                this._preFirstNotificationCellId = e
            }
            startIntervalToPopNotificationQue() {
                let e = v.ap + v.vt;
                (C.ie() || C.safari()) && (e *= 4),
                    this._notificationAddIntervalId = (0,
                        y.A)().setInterval(() => {
                            this._notificationQueAnimationFrame && ((0,
                                y.A)().cancelAnimationFrame(this._notificationQueAnimationFrame),
                                this._notificationQueAnimationFrame = 0),
                                this._notificationQueAnimationFrame = (0,
                                    y.A)().requestAnimationFrame(() => {
                                        this.popNotificationQue()
                                    }
                                    )
                        }
                            , e)
            }
            _firstOpenSplitview() {
                this._everOpenedSplitView || this.isOpenedSplitView || 0 !== this.notifications.length && (this.toggleSplitView(),
                    this._everOpenedSplitView = !0)
            }
            _hasTopNotificationListDiffrence() {
                const e = this.notifications.length && this.notifications[0];
                return !!e && this._preFirstNotificationCellId !== e.id
            }
        }
        mi([o.sH], yi.prototype, "notifications", 2),
            mi([o.sH], yi.prototype, "isOpenedSplitView", 2),
            mi([o.sH], yi.prototype, "isScrollingByAuto", 2),
            mi([o.sH], yi.prototype, "isLastPage", 2),
            mi([o.sH], yi.prototype, "isTracing", 2),
            mi([o.sH], yi.prototype, "splitViewPopoutWindow", 2),
            mi([o.XI.bound], yi.prototype, "setSplitViewPopoutWindow", 1),
            mi([o.XI.bound], yi.prototype, "closeSplitViewPopoutWindow", 1),
            mi([o.XI], yi.prototype, "willLoad", 1),
            mi([o.XI], yi.prototype, "willUnload", 1),
            mi([o.XI.bound], yi.prototype, "popNotificationQue", 1),
            mi([o.XI.bound], yi.prototype, "traceNotifications", 1),
            mi([o.XI.bound], yi.prototype, "toggleSplitView", 1),
            mi([o.XI.bound], yi.prototype, "openSplitView", 1),
            mi([o.XI.bound], yi.prototype, "addNotificationChat", 1),
            mi([o.XI.bound], yi.prototype, "addPurchaseNotification", 1),
            mi([o.XI], yi.prototype, "resetTracing", 1),
            mi([o.XI.bound], yi.prototype, "startScrollByAuto", 1),
            mi([o.XI.bound], yi.prototype, "finishScrollByAuto", 1),
            mi([o.XI.bound], yi.prototype, "addBlackList", 1),
            mi([o.XI.bound], yi.prototype, "deleteBlackList", 1);
        var gi = i(70322)
            , vi = Object.defineProperty
            , Si = Object.defineProperties
            , bi = Object.getOwnPropertyDescriptor
            , fi = Object.getOwnPropertyDescriptors
            , Ci = Object.getOwnPropertySymbols
            , Ii = Object.prototype.hasOwnProperty
            , Ai = Object.prototype.propertyIsEnumerable
            , _i = (e, t, i) => t in e ? vi(e, t, {
                enumerable: !0,
                configurable: !0,
                writable: !0,
                value: i
            }) : e[t] = i
            , Pi = (e, t, i, s) => {
                for (var o, r = s > 1 ? void 0 : s ? bi(t, i) : t, a = e.length - 1; a >= 0; a--)
                    (o = e[a]) && (r = (s ? o(t, i, r) : o(r)) || r);
                return s && r && vi(t, i, r),
                    r
            }
            , Mi = (e, t, i) => new Promise((s, o) => {
                var r = e => {
                    try {
                        n(i.next(e))
                    } catch (e) {
                        o(e)
                    }
                }
                    , a = e => {
                        try {
                            n(i.throw(e))
                        } catch (e) {
                            o(e)
                        }
                    }
                    , n = e => e.done ? s(e.value) : Promise.resolve(e.value).then(r, a);
                n((i = i.apply(e, t)).next())
            }
            );
        class wi {
            constructor() {
                this.userCard = {
                    element: null,
                    blacklistElement: null,
                    ipBanElement: null,
                    userId: "",
                    userName: "",
                    followed: !1,
                    blacklisted: !1,
                    moderated: !1,
                    userIconImageUrl: "",
                    userCoverImageUrl: "",
                    recxuserId: 0,
                    isAuthenticated: !1,
                    isFresh: !1
                },
                    this.v8UserCard = {
                        displayingUser: void 0,
                        subsBadge: void 0,
                        chatCommentAppearanceSetting: void 0,
                        userInChannel: void 0
                    },
                    this.isOpenBlacklistCard = !1,
                    (0,
                        o.Gn)(this)
            }
            willLoad(e) {
                return Mi(this, null, function* () {
                    this.stores = e
                })
            }
            willUnload() {
                this.hideUserCard(),
                    this.hideIPBanCard(),
                    delete this.stores
            }
            showUserCard(e, t, i) {
                var s, o;
                const r = this.stores;
                if (i && r.appStore.isMobile)
                    return;
                this.userCard.userId = e.userId,
                    this.userCard.userName = e.userName,
                    this.userCard.userIconImageUrl = e.userIconImageUrl || "",
                    this.userCard.userCoverImageUrl = e.userCoverImageUrl || "",
                    this.userCard.recxuserId = e.recxuserId || 0,
                    this.userCard.isAuthenticated = !!e.isAuthenticated,
                    this.userCard.isFresh = e.isFresh,
                    this.userCard.element = t;
                const a = {
                    displayingUser: e.v8User,
                    chatCommentAppearanceSetting: e.chatCommentAppearanceSetting,
                    userInChannel: {
                        isModerating: (null == (s = e.userInChannel) ? void 0 : s.isModerating) || !1,
                        membershipCardUrl: (null == (o = e.userInChannel) ? void 0 : o.membershipCardUrl) || ""
                    },
                    subsBadge: e.subsBadge && "" !== e.subsBadge.imageUrl ? (0,
                        gi.sD)(e.subsBadge.id, e.subsBadge.imageUrl, e.subsBadge.label, "subscription", {
                            months: e.subsBadge.months,
                            tier: e.subsBadge.tier
                        }) : void 0
                };
                this.v8UserCard = a
            }
            showBlacklistCard(e) {
                this.userCard.blacklistElement = e
            }
            closeBlacklistCard() {
                this.userCard.blacklistElement = null
            }
            showIpBanCard(e) {
                this.userCard.ipBanElement = e
            }
            closeIpBanCard() {
                this.userCard.ipBanElement = null
            }
            hideUserCard() {
                this.userCard.element = null,
                    this.userCard.blacklistElement = null,
                    this.userCard.userId = "",
                    this.userCard.userName = "",
                    this.userCard.followed = !1,
                    this.userCard.blacklisted = !1,
                    this.userCard.moderated = !1,
                    this.userCard.isFresh = !1,
                    this.v8UserCard.displayingUser = void 0,
                    this.v8UserCard.subsBadge = void 0,
                    this.v8UserCard.chatCommentAppearanceSetting = void 0,
                    this.v8UserCard.userInChannel = void 0
            }
            hideIPBanCard() {
                this.userCard.ipBanElement = null
            }
            updateUserCardFollowed() {
                this.userCard.followed = !this.userCard.followed
            }
            updateUserCardBlacklisted(e) {
                this.userCard.blacklisted = "boolean" == typeof e ? e : !this.userCard.blacklisted
            }
            updateUserCardModerated() {
                this.userCard.moderated = !this.userCard.moderated
            }
            updateIsModerating() {
                var e, t;
                this.v8UserCard.userInChannel && (this.v8UserCard.userInChannel = (e = ((e, t) => {
                    for (var i in t || (t = {}))
                        Ii.call(t, i) && _i(e, i, t[i]);
                    if (Ci)
                        for (var i of Ci(t))
                            Ai.call(t, i) && _i(e, i, t[i]);
                    return e
                }
                )({}, this.v8UserCard.userInChannel),
                    t = {
                        isModerating: !this.v8UserCard.userInChannel.isModerating
                    },
                    Si(e, fi(t))))
            }
            fetchOptionalUserCard(e, t) {
                return Mi(this, null, function* () { })
            }
            userFollowAndUpdate(e) {
                return Mi(this, null, function* () {
                    if (this.stores) {
                        try {
                            let t;
                            if (t = this.stores.moviePageStore.movieStore.channel.id === e ? yield this.stores.moviePageStore.movieStore.follow() : yield this.stores.userStore.follow(e),
                                t.message)
                                return
                        } catch (e) {
                            return console.error(e),
                                void this._showErrorMessage(e.message)
                        }
                        this.updateUserCardFollowed()
                    }
                })
            }
            userUnfollowAndUpdate(e) {
                return Mi(this, null, function* () {
                    if (this.stores) {
                        try {
                            this.stores.moviePageStore.movieStore.channel.id === e ? yield this.stores.moviePageStore.movieStore.unfollow() : yield this.stores.userStore.unfollow(e)
                        } catch (e) {
                            return console.error(e),
                                void this._showErrorMessage(e.message)
                        }
                        this.updateUserCardFollowed()
                    }
                })
            }
            _showErrorMessage(e = "") {
                x.A.addMessage({
                    variant: "danger",
                    text: e
                })
            }
        }
        Pi([o.sH], wi.prototype, "userCard", 2),
            Pi([o.sH.shallow], wi.prototype, "v8UserCard", 2),
            Pi([o.sH], wi.prototype, "isOpenBlacklistCard", 2),
            Pi([o.XI], wi.prototype, "willLoad", 1),
            Pi([o.XI], wi.prototype, "willUnload", 1),
            Pi([o.XI.bound], wi.prototype, "showUserCard", 1),
            Pi([o.XI.bound], wi.prototype, "showBlacklistCard", 1),
            Pi([o.XI.bound], wi.prototype, "closeBlacklistCard", 1),
            Pi([o.XI.bound], wi.prototype, "showIpBanCard", 1),
            Pi([o.XI.bound], wi.prototype, "closeIpBanCard", 1),
            Pi([o.XI.bound], wi.prototype, "hideUserCard", 1),
            Pi([o.XI.bound], wi.prototype, "hideIPBanCard", 1),
            Pi([o.XI.bound], wi.prototype, "updateUserCardFollowed", 1),
            Pi([o.XI.bound], wi.prototype, "updateUserCardBlacklisted", 1),
            Pi([o.XI.bound], wi.prototype, "updateUserCardModerated", 1),
            Pi([o.XI.bound], wi.prototype, "updateIsModerating", 1),
            Pi([o.XI.bound], wi.prototype, "fetchOptionalUserCard", 1),
            Pi([o.XI], wi.prototype, "userFollowAndUpdate", 1),
            Pi([o.XI], wi.prototype, "userUnfollowAndUpdate", 1);
        var Ti = i(47268)
            , Ei = i(64780)
            , Ui = i(2842)
            , Li = i(54334)
            , ki = Object.defineProperty
            , Hi = Object.defineProperties
            , Oi = Object.getOwnPropertyDescriptor
            , Di = Object.getOwnPropertyDescriptors
            , xi = Object.getOwnPropertySymbols
            , Fi = Object.prototype.hasOwnProperty
            , Ri = Object.prototype.propertyIsEnumerable
            , Bi = (e, t, i) => t in e ? ki(e, t, {
                enumerable: !0,
                configurable: !0,
                writable: !0,
                value: i
            }) : e[t] = i
            , Xi = (e, t) => {
                for (var i in t || (t = {}))
                    Fi.call(t, i) && Bi(e, i, t[i]);
                if (xi)
                    for (var i of xi(t))
                        Ri.call(t, i) && Bi(e, i, t[i]);
                return e
            }
            , Wi = (e, t, i, s) => {
                for (var o, r = s > 1 ? void 0 : s ? Oi(t, i) : t, a = e.length - 1; a >= 0; a--)
                    (o = e[a]) && (r = (s ? o(t, i, r) : o(r)) || r);
                return s && r && ki(t, i, r),
                    r
            }
            , Ni = (e, t, i) => new Promise((s, o) => {
                var r = e => {
                    try {
                        n(i.next(e))
                    } catch (e) {
                        o(e)
                    }
                }
                    , a = e => {
                        try {
                            n(i.throw(e))
                        } catch (e) {
                            o(e)
                        }
                    }
                    , n = e => e.done ? s(e.value) : Promise.resolve(e.value).then(r, a);
                n((i = i.apply(e, t)).next())
            }
            );
        class Vi {
            constructor() {
                this.currentTab = "ranking",
                    this.newSupporter = {
                        currentTime: 0,
                        duration: 0,
                        supporter: null
                    },
                    this.newSupporterQueue = o.sH.array(),
                    this.newYells = o.sH.array(),
                    this.yellRanks = o.sH.array(),
                    this.yellRanksOfMonthly = o.sH.array(),
                    this.pageOfNewYells = 0,
                    this.pageOfYellRanks = 0,
                    this.pageOfMonthlyYellRanks = 0,
                    this.isLastPageOfNewYells = !1,
                    this.isLastPageOfYellRanks = !1,
                    this.isLastPageOfMonthlyYellRanks = !1,
                    this.isLoading = !1,
                    this.isYellLogVisible = !1,
                    this.yellReply = {
                        id: 0,
                        message: "",
                        createdAt: ""
                    },
                    this._isNewYellReplyVisible = !0,
                    this._isYellReplyVisible = !0,
                    this.toYellTickerFromRank = e => {
                        var t, i, s, o, r, a, n, l, d, h;
                        const u = {
                            userId: e.user && e.user.id,
                            message: e.lastMessage,
                            isAuthenticated: !0,
                            isModerator: e.isModerating,
                            subsBadge: {
                                imageUrl: (null == (i = null == (t = e.badges) ? void 0 : t[0]) ? void 0 : i.imageUrl) || "",
                                months: (null == (r = null == (o = null == (s = e.badges) ? void 0 : s[0]) ? void 0 : o.subscription) ? void 0 : r.months) || 0,
                                tier: (null == (n = null == (a = e.badges) ? void 0 : a[0]) ? void 0 : n.subscription) && (null == (h = null == (d = null == (l = e.badges) ? void 0 : l[0]) ? void 0 : d.subscription) ? void 0 : h.tier) || 0
                            }
                        }
                            , c = (0,
                                Ti.BB)(u, e.user, e.chatSetting, e.toUser);
                        return c.isDisabled = this.stores && this.stores.moviePageStore.chatStore.isBannedWordChat(c),
                            c
                    }
                    ,
                    this.toYellTickerFromChat = (e, t = "top") => {
                        const i = {
                            id: e.id,
                            userId: e.userId,
                            userColor: e.userColor,
                            userName: e.userName,
                            message: e.message,
                            toUser: e.toUser,
                            isAuthenticated: !!e.isAuthenticated,
                            isOfficial: !!e.isOfficial,
                            isOfficialHidden: !!e.isOfficialHidden,
                            isPremium: !!e.isPremium,
                            isPremiumHidden: !!e.isPremiumHidden,
                            isSubsBadgeHidden: !!e.isSubsBadgeHidden,
                            isSubsDurationHidden: !!e.isSubsDurationHidden,
                            isWarned: !!e.isWarned,
                            isModerator: !!e.isModerator,
                            subsBadge: e.subsBadge,
                            yell: e.yell,
                            type: t
                        };
                        return e.yell && (i.yells = String(e.yell.quantity),
                            i.duration = e.yell.seconds,
                            i.iconImageUrl = e.yell.imageUrl),
                            i.isDisabled = this.stores && this.stores.moviePageStore.chatStore.isBannedWordChat(i),
                            i
                    }
                    ,
                    (0,
                        q.H)(this, {
                            topSupporter: o.sH
                        })
            }
            willLoad(e) {
                return Ni(this, null, function* () {
                    this.stores = e
                })
            }
            willUnload() {
                this.hideYellDisplay(),
                    delete this.stores
            }
            updateTab(e) {
                this.currentTab = e
            }
            startLoading() {
                this.isLoading = !0
            }
            finishLoading() {
                this.isLoading = !1
            }
            updatePageInfo(e, t, i = {}) {
                switch (e) {
                    case "ranking":
                        this.pageOfYellRanks = t,
                            this.isLastPageOfYellRanks = !!i.isLastPage;
                        break;
                    case "monthly_ranking":
                        this.pageOfMonthlyYellRanks = t,
                            this.isLastPageOfMonthlyYellRanks = !!i.isLastPage;
                        break;
                    case "new":
                        this.pageOfNewYells = t,
                            this.isLastPageOfNewYells = !!i.isLastPage
                }
            }
            clear(e) {
                switch (e) {
                    case "ranking":
                        this.yellRanks.clear();
                        break;
                    case "monthly_ranking":
                        this.yellRanksOfMonthly.clear();
                        break;
                    case "new":
                        this.newYells.clear()
                }
            }
            addNewSupporter(e) {
                this.stores && (this.stores.moviePageStore.chatStore.isReproducingChatInDvr || (e.isTopSupporter && (this.topSupporter = this.toYellTickerFromChat(e)),
                    this.addNewSupporterQueue(this.toYellTickerFromChat(e, "new"))))
            }
            addNewSupporterQueue(e) {
                this.newSupporterQueue.push(e),
                    this.toNextNewSupporter()
            }
            toNextNewSupporter() {
                if (this._intervalId)
                    return;
                const e = this.newSupporterQueue.shift();
                if (!e)
                    return this.newSupporter.currentTime = 0,
                        this.newSupporter.duration = 0,
                        void (this.newSupporter.supporter = null);
                this.newSupporter.currentTime = 0,
                    this.newSupporter.duration = e.duration || 0,
                    this.newSupporter.supporter = e,
                    this._intervalId = (0,
                        y.A)().setInterval(() => {
                            (0,
                                o.h5)(() => {
                                    this.newSupporter.currentTime < this.newSupporter.duration ? this.newSupporter.currentTime += .01 : ((0,
                                        y.A)().clearInterval(this._intervalId),
                                        delete this._intervalId,
                                        this.toNextNewSupporter())
                                }
                                )
                        }
                            , 10)
            }
            fetchTopSupporterAndUpdate(e) {
                return Ni(this, null, function* () {
                    let t;
                    try {
                        t = yield (0,
                            Li.k)(Xi({
                                page: 1
                            }, e))
                    } catch (e) {
                        return void console.error(e)
                    }
                    (0,
                        o.h5)(() => {
                            t && t[0] && (this.topSupporter = this.toYellTickerFromRank(t[0]))
                        }
                        )
                })
            }
            fetchYellRankingAndUpdate(e, t) {
                return Ni(this, null, function* () {
                    const i = t.page || 1;
                    i < 2 && this.clear(e),
                        this.startLoading(),
                        this.updatePageInfo(e, i, {
                            isLastPage: !1
                        });
                    let s = null;
                    try {
                        s = yield (0,
                            Li.k)(t)
                    } catch (e) {
                        return console.error(e),
                            void this.finishLoading()
                    }
                    (0,
                        o.h5)(() => {
                            if (!s || !s.length)
                                return void this.updatePageInfo(e, i, {
                                    isLastPage: !0
                                });
                            const t = s.filter(e => e && e.rank).map(e => (0,
                                Ei.cN)(e));
                            let o, r;
                            "monthly_ranking" === e ? (o = this.yellRanksOfMonthly.slice().concat(t),
                                this.yellRanksOfMonthly.replace(o)) : (r = this.yellRanks.slice().concat(t),
                                    this.yellRanks.replace(r))
                        }
                        ),
                        this.finishLoading()
                })
            }
            fetchNewYellsAndUpdate(e) {
                return Ni(this, null, function* () {
                    const t = e.page || 1;
                    t < 2 && this.clear("new"),
                        this.startLoading(),
                        this.updatePageInfo("new", t, {
                            isLastPage: !1
                        });
                    let i = null;
                    try {
                        i = yield (0,
                            Ui.c)(e)
                    } catch (e) {
                        return console.error(e),
                            void this.finishLoading()
                    }
                    (0,
                        o.h5)(() => {
                            if (!i || !i.length)
                                return void this.updatePageInfo("new", t, {
                                    isLastPage: !0
                                });
                            const e = i.map(e => (0,
                                Ei.gb)(e))
                                , s = this.newYells.slice().concat(e);
                            this.newYells.replace(s)
                        }
                        ),
                        this.finishLoading()
                })
            }
            showYellDisplay() {
                this.isYellLogVisible = !0
            }
            hideYellDisplay() {
                this.isYellLogVisible = !1
            }
            hideNewYellReply() {
                this._isNewYellReplyVisible = !1
            }
            showNewYellReply() {
                this._isNewYellReplyVisible = !0
            }
            hideYellReply() {
                this._isYellReplyVisible = !1
            }
            showYellReply() {
                this._isYellReplyVisible = !0
            }
            updateYellRanks(e) {
                this.yellRanks.replace(e)
            }
            updateNewYells(e) {
                this.newYells.replace(e)
            }
            updateYellRanksOfMonthly(e) {
                this.yellRanksOfMonthly.replace(e)
            }
            updateTopSupporter(e) {
                this.topSupporter = e
            }
            updateNewSupporter(e) {
                this.newSupporter = e
            }
            updateNewSupporterQueue(e) {
                this.newSupporterQueue.replace(e)
            }
            updateYellReaction(e, t) {
                const i = this.newYells.map(i => {
                    return i.chatId === e ? (s = Xi({}, i),
                        Hi(s, Di({
                            reaction: t
                        }))) : i;
                    var s
                }
                );
                this.newYells.replace(i)
            }
            get supporter() {
                return this.newSupporter.supporter ? this.newSupporter.supporter : this.topSupporter
            }
            get isEmpty() {
                switch (this.currentTab) {
                    case "ranking":
                        return !this.yellRanks.length;
                    case "monthly_ranking":
                        return !this.yellRanksOfMonthly.length;
                    case "new":
                        return !this.newYells.length
                }
                return !1
            }
            get isLastPage() {
                switch (this.currentTab) {
                    case "ranking":
                        return this.isLastPageOfYellRanks;
                    case "monthly_ranking":
                        return this.isLastPageOfMonthlyYellRanks;
                    case "new":
                        return this.isLastPageOfNewYells
                }
                return !1
            }
            get isDisabled() {
                return !!this.stores && (!!this.supporter.isDisabled || this.stores.moviePageStore.isDisabledChat(this.supporter))
            }
            get isYellReplyVisible() {
                return !this.isNewYellReplyVisible && !!this._isYellReplyVisible && !!this.yellReply.id
            }
            get isNewYellReplyVisible() {
                if (!this.yellReply.id)
                    return !1;
                if (!this.stores)
                    return !1;
                if (!this._isNewYellReplyVisible)
                    return !1;
                const e = this.stores.moviePageStore;
                return !(0,
                    p.Ay)().getReadYellReplies().some(t => t.channelId === e.movieStore.channel.id && t.replyId === this.yellReply.id) && ((0,
                        p.Ay)().setReadYellReplies({
                            channelId: e.movieStore.channel.id,
                            replyId: Number(this.yellReply.id),
                            readAt: new Date
                        }),
                        !0)
            }
        }
        Wi([o.sH], Vi.prototype, "currentTab", 2),
            Wi([o.sH], Vi.prototype, "newSupporter", 2),
            Wi([o.sH], Vi.prototype, "newSupporterQueue", 2),
            Wi([o.sH], Vi.prototype, "newYells", 2),
            Wi([o.sH], Vi.prototype, "yellRanks", 2),
            Wi([o.sH], Vi.prototype, "yellRanksOfMonthly", 2),
            Wi([o.sH], Vi.prototype, "pageOfNewYells", 2),
            Wi([o.sH], Vi.prototype, "pageOfYellRanks", 2),
            Wi([o.sH], Vi.prototype, "pageOfMonthlyYellRanks", 2),
            Wi([o.sH], Vi.prototype, "isLastPageOfNewYells", 2),
            Wi([o.sH], Vi.prototype, "isLastPageOfYellRanks", 2),
            Wi([o.sH], Vi.prototype, "isLastPageOfMonthlyYellRanks", 2),
            Wi([o.sH], Vi.prototype, "isLoading", 2),
            Wi([o.sH], Vi.prototype, "isYellLogVisible", 2),
            Wi([o.sH], Vi.prototype, "yellReply", 2),
            Wi([o.sH], Vi.prototype, "_isNewYellReplyVisible", 2),
            Wi([o.sH], Vi.prototype, "_isYellReplyVisible", 2),
            Wi([o.XI], Vi.prototype, "willLoad", 1),
            Wi([o.XI], Vi.prototype, "willUnload", 1),
            Wi([o.XI], Vi.prototype, "updateTab", 1),
            Wi([o.XI], Vi.prototype, "startLoading", 1),
            Wi([o.XI], Vi.prototype, "finishLoading", 1),
            Wi([o.XI], Vi.prototype, "updatePageInfo", 1),
            Wi([o.XI], Vi.prototype, "clear", 1),
            Wi([o.XI.bound], Vi.prototype, "addNewSupporter", 1),
            Wi([o.XI.bound], Vi.prototype, "toNextNewSupporter", 1),
            Wi([o.XI.bound], Vi.prototype, "fetchTopSupporterAndUpdate", 1),
            Wi([o.XI.bound], Vi.prototype, "fetchYellRankingAndUpdate", 1),
            Wi([o.XI.bound], Vi.prototype, "fetchNewYellsAndUpdate", 1),
            Wi([o.XI.bound], Vi.prototype, "showYellDisplay", 1),
            Wi([o.XI.bound], Vi.prototype, "hideYellDisplay", 1),
            Wi([o.XI.bound], Vi.prototype, "hideNewYellReply", 1),
            Wi([o.XI.bound], Vi.prototype, "showNewYellReply", 1),
            Wi([o.XI.bound], Vi.prototype, "hideYellReply", 1),
            Wi([o.XI.bound], Vi.prototype, "showYellReply", 1),
            Wi([o.XI], Vi.prototype, "updateYellRanks", 1),
            Wi([o.XI], Vi.prototype, "updateNewYells", 1),
            Wi([o.XI], Vi.prototype, "updateYellRanksOfMonthly", 1),
            Wi([o.XI], Vi.prototype, "updateTopSupporter", 1),
            Wi([o.XI], Vi.prototype, "updateNewSupporter", 1),
            Wi([o.XI], Vi.prototype, "updateNewSupporterQueue", 1),
            Wi([o.EW], Vi.prototype, "supporter", 1),
            Wi([o.EW], Vi.prototype, "isEmpty", 1),
            Wi([o.EW], Vi.prototype, "isLastPage", 1),
            Wi([o.EW], Vi.prototype, "isDisabled", 1),
            Wi([o.EW], Vi.prototype, "isYellReplyVisible", 1),
            Wi([o.EW], Vi.prototype, "isNewYellReplyVisible", 1);
        var Yi = i(72909)
            , Qi = i(61889)
            , qi = i(92420)
            , ji = i(13528)
            , zi = i(51148)
            , Gi = i(61372)
            , Ki = i(87361)
            , $i = i(90327)
            , Zi = i(51202);
        const Ji = new class {
            createStore() {
                return new $i.A
            }
            updateRelatedMovies(e, t) {
                return i = this,
                    s = function* () {
                        yield e.waitToLoad();
                        const { movie: i, deepMovie: s } = e
                            , o = (e, t) => e.filter(e => !t.hiddenChannelList.includesUserId(e.channel.user.id));
                        if (i && s)
                            try {
                                const [r, a, n, l, d] = yield Promise.all([Zi.Ay.listByUserIdAndGameId(i.channel.user.id, i.game.id), i.isUploaded() ? Zi.Ay.listUploadByUserId(i.channel.user.id) : Zi.Ay.listStreamByUserId(i.channel.user.id), Zi.Ay.listLiveByGameId(i.game.id), s.categoryTags.length > 0 ? Zi.Ay.listLiveByTag(s.categoryTags[0].content) : [], Zi.Ay.listPopularLive()]);
                                e.setRelatedMovies(r, a, o(n, t), o(l, t), o(d, t))
                            } catch (e) { }
                    }
                    ,
                    new Promise((e, t) => {
                        var o = e => {
                            try {
                                a(s.next(e))
                            } catch (e) {
                                t(e)
                            }
                        }
                            , r = e => {
                                try {
                                    a(s.throw(e))
                                } catch (e) {
                                    t(e)
                                }
                            }
                            , a = t => t.done ? e(t.value) : Promise.resolve(t.value).then(o, r);
                        a((s = s.apply(i, null)).next())
                    }
                    );
                var i, s
            }
        }
            ;
        var es = i(5701)
            , ts = i(50054)
            , is = i(17283)
            , ss = i(77581)
            , os = i(77872)
            , rs = Object.defineProperty
            , as = Object.getOwnPropertyDescriptor
            , ns = (e, t, i, s) => {
                for (var o, r = s > 1 ? void 0 : s ? as(t, i) : t, a = e.length - 1; a >= 0; a--)
                    (o = e[a]) && (r = (s ? o(t, i, r) : o(r)) || r);
                return s && r && rs(t, i, r),
                    r
            }
            , ls = (e, t, i) => new Promise((s, o) => {
                var r = e => {
                    try {
                        n(i.next(e))
                    } catch (e) {
                        o(e)
                    }
                }
                    , a = e => {
                        try {
                            n(i.throw(e))
                        } catch (e) {
                            o(e)
                        }
                    }
                    , n = e => e.done ? s(e.value) : Promise.resolve(e.value).then(r, a);
                n((i = i.apply(e, t)).next())
            }
            );
        class ds {
            constructor() {
                this.storeName = "moviePageStore",
                    this.stores = null,
                    this.aborts = [],
                    this.currentPlayerType = "",
                    this.playerTypeUpdateIntervalId = 0,
                    this.movieArticle = null,
                    this.hasAlreadyPostedViewLog = !1,
                    this.isCompletedFetchRelatedMovies = !1,
                    this.videoWidth = 0,
                    this.videoHeight = 0,
                    this.chatArticleWidth = 10 * Qi.F0.CHAT_WIDTH,
                    this.relatedMovieStreams = [],
                    this.isFullscreenMode = !1,
                    this.chatPopoutWindow = null,
                    this.exntensionPopoutWindow = null,
                    this.pdt = null,
                    this.isCompletedPrapareDvr = !1,
                    this.isCapturePremiumDialogVisible = !1,
                    this.isCapturePopupVisible = !1,
                    this.isPolling = !1,
                    this.captureResponse = {
                        captureId: "",
                        captureThumbnail: ""
                    },
                    this.hasPpvPermission = !1,
                    this.videoContainer = null,
                    this.isVisibleMovieAlert = !1,
                    this.chatPositionWithFullscreenMode = "left",
                    this.refreshTime = void 0,
                    this.isVisibleRectangleAd = !1,
                    this.vpos = void 0,
                    this.mridx = 0,
                    this._capturesOfMovie = [],
                    this._loadArchiveChatTimeoutID = null,
                    this._midRollStartTime = 0,
                    this._remainingMidRollTime = v.mZ,
                    this._isCaptureTimeout = !1,
                    this._sentContinuousBufferStalledErrorMessage = !1,
                    this._sessionId = "",
                    this._playingVideobitrate = 0,
                    this._playingVideoWidth = 0,
                    this._playingVideoHeight = 0,
                    this._startedDate = void 0,
                    this._elapsedTime = 0,
                    this._userSetting = {
                        adHidden: !1,
                        subsAdHidden: !1
                    },
                    this._membershipFetchCount = 0,
                    this.RELATED_MOVIE_LIMIT = 8,
                    this.RELATED_MOVIE_SHOW_LIMIT = 6,
                    this._processAfterLogin = e => ls(this, null, function* () {
                        var t, i, s, r;
                        if (this._disposeLoginCallback(),
                            this.chatSettingStore.hideMenu(),
                            this.userCardStore.hideUserCard(),
                            !this.movieStore.notFound) {
                            try {
                                const o = yield this.movieStore.fetchMovieDetailAndUpdate(e)
                                    , a = null == (i = null == (t = null == o ? void 0 : o.data) ? void 0 : t.items) ? void 0 : i[0];
                                if (this.chatSettingStore.updateMovieDetail(e, a),
                                    this.chatStore.updateMovieDetail(a),
                                    this.movieStore.isOwner) {
                                    const [e, t] = yield Promise.all([ts.A.get(), es.A.get()]);
                                    this.chatSettingStore.updateChatSetting(this.chatSettingStore.toChatSettings(e)),
                                        this.chatSettingStore.setChatTermsRequired(null != (s = null == t ? void 0 : t.chatTermsRequired) && s),
                                        this.chatSettingStore.setMutedBannedWord(null != (r = null == t ? void 0 : t.mutedBannedWord) && r)
                                }
                            } catch (t) {
                                return console.error(t),
                                    void x.A.addMessage({
                                        variant: "danger",
                                        text: e.langStore.error.errorOccurred
                                    })
                            }
                            if (this.movieStore.isUploadedMovie ? this._willLoadForComment(e) : this._willLoadForChat(e),
                                (() => {
                                    ls(this, null, function* () {
                                        yield (0,
                                            o.z7)(() => this.movieStore.isCompletedFetchMovieDetail),
                                            this.movieStore.hasPpvTicketProducts || (this._disposePpvTicketPurchaseCallback = (0,
                                                o.z7)(() => this.movieStore.hasPpvTicketProducts, () => {
                                                    (0,
                                                        y.A)().location.reload()
                                                }
                                                ))
                                    })
                                }
                                )(),
                                this._disposeReceivedIsPlayableWhen = (0,
                                    o.z7)(() => this.movieStore.isCompletedFetchMovieDetail && !this.movieStore.isPlayable, () => {
                                        if (this.movieStore.isArchive && this.movieStore.startedAt) {
                                            const t = Ki.A.createModel(this.movieStore.startedAt).add(e.appStore.currentQueryInfo.t ? Number(e.appStore.currentQueryInfo.t) : this.movieStore.playPostion || 0, "SECOND").toDate()
                                                , i = (0,
                                                    Q.wO)(t);
                                            this.refetchAllArchiveChats({
                                                dateStr: i
                                            })
                                        }
                                    }
                                    ),
                                this.movieStore.isOwner) {
                                const e = yield this._fetchUsersMe();
                                e && (0,
                                    o.h5)(() => {
                                        this.hasPpvPermission = !!e.hasPpvPermission
                                    }
                                    )
                            }
                        }
                    }),
                    this._toChatFromSocketChat = e => {
                        const t = this.stores && this.stores.moviePageStore.movieStore
                            , i = this.stores && this.stores.moviePageStore.chatStore
                            , s = {
                                id: Number(e.chat_id || 0),
                                message: e.message || "",
                                postedAt: e.cre_dt,
                                userId: e.user_key || "",
                                recxuserId: e.user_id,
                                userIconImageUrl: e.user_icon || "",
                                userName: e.user_name || "",
                                userColor: e.user_color || (0,
                                    ve.toUserColor)(e.user_id),
                                isOfficial: "1" === String(e.user_type),
                                isOfficialHidden: !!e.is_official_hidden,
                                isFresh: !!e.is_fresh,
                                isPremium: !!e.is_premium,
                                isPremiumHidden: !!e.is_premium_hidden,
                                isSubsBadgeHidden: !!e.is_subs_badge_hidden,
                                isSubsDurationHidden: !!e.is_subs_duration_hidden,
                                isWarned: !!e.is_warned,
                                isModerator: !!e.is_moderator,
                                isLowLatency: !!e.quality_type && String(e.quality_type) === v.lT.LOW_LATENCY.toString(),
                                isAuthenticated: !!e.is_authenticated,
                                isTopSupporter: 1 === e.supporter_rank,
                                isMuted: 1 === e.is_muted,
                                hasBannedWord: 1 === e.has_banned_word,
                                linkUrl: e.link_url || ""
                            };
                        if (e.stamp && (s.stamp = {
                            id: Number(e.stamp.stamp_id || 0),
                            groupId: Number(e.stamp.group_id || 0),
                            imageUrl: e.stamp.image_url
                        }),
                            e.yell) {
                            if (s.yell = {
                                id: Number(e.yell.yell_id || 0),
                                userName: e.yell.name || "",
                                label: e.yell.label || "",
                                quantity: Number(e.yell.yells || 0),
                                points: Number(e.yell.points || 0),
                                imageUrl: e.yell.image_url || "",
                                toUser: e.to_user ? {
                                    userName: e.to_user.nickname || "",
                                    userIconImageUrl: e.to_user.icon_image_url || ""
                                } : void 0,
                                seconds: Number(e.yell.ticker_seconds)
                            },
                                e.yell.subs_share) {
                                const t = e.yell.subs_share.subs_product.subs_badges[0];
                                s.yell.subsShareCount = e.yell.subs_share.share_count,
                                    s.yell.subsShareBadge = {
                                        id: t.id || 0,
                                        imageUrl: t.image_url || ""
                                    }
                            }
                            t && (s.yell.isLeague = t.leagueId && t.enabledYell && t.channel.isLeagueYell || !1,
                                s.yell.isCastYell = t.enabledCastYell)
                        }
                        if (e.to_user && (s.toUser = {
                            userId: e.to_user.id || "",
                            userName: e.to_user.nickname || "",
                            userIconImageUrl: e.to_user.icon_image_url || ""
                        }),
                            e.badges) {
                            const t = (e.badges || [])[0] || {}
                                , i = t.subscription || {};
                            s.subsBadge = {
                                id: t.id || 0,
                                imageUrl: t.image_url || "",
                                months: i.months || 0,
                                tier: i.tier || 0,
                                label: t.label || ""
                            }
                        }
                        return s.disabled = this.isDisabledChat(s) || (null == i ? void 0 : i.isBannedWordChat(s)),
                            s.v8User = X.A.createModel({
                                user_id: e.user_id,
                                user_key: e.user_key,
                                user_identify_id: e.user_identify_id,
                                openrec_user_id: e.openrec_user_id,
                                user_name: e.user_name,
                                user_icon: e.user_icon,
                                user_type: e.user_type,
                                is_premium: e.is_premium,
                                is_fresh: e.is_fresh,
                                is_warned: e.is_warned
                            }),
                            s.chatCommentAppearanceSetting = F.A.createModel({
                                nameColor: e.user_color || null,
                                isPremiumHidden: 1 === e.is_premium_hidden,
                                isOfficialHidden: 1 === e.is_official_hidden,
                                isSubsBadgeHidden: 1 === e.is_subs_badge_hidden,
                                isSubsDurationHidden: 1 === e.is_subs_duration_hidden,
                                isSubsMembershipCardHidden: 1 === e.is_subs_membership_card_hidden
                            }),
                            s.userInChannel = {
                                isModerating: 1 === e.is_moderator,
                                membershipCardUrl: e.membership_card_url
                            },
                            s
                    }
                    ,
                    this._toSystemMessageFromSocketChat = e => {
                        var t, i, s;
                        const o = this.movieStore
                            , { message: r, message_en: a, message_ko: n, message_zh: l, cre_dt: d, chat_id: h, system_message: u } = e
                            , c = u.type
                            , p = {
                                style: u.style,
                                extensionKey: null == (t = u.extension_key_info) ? void 0 : t.extension_key,
                                subsBadgeImage: null == (s = null == (i = u.subs_badges) ? void 0 : i[0]) ? void 0 : s.image_url
                            }
                            , m = u.poll ? {
                                id: u.poll.id,
                                status: "poll_start" === c ? "start" : "poll_finish" === c ? "finish" : "ready",
                                pollThumbnailUrl: u.poll.poll_thumbnail_url,
                                voteThumbnailUrl: u.poll.vote_thumbnail_url
                            } : void 0
                            , y = u.extension_key_info ? () => {
                                var e, t, i;
                                return o.setDisplayExtension({
                                    id: (null == (e = u.extension_key_info) ? void 0 : e.extension_key) || "",
                                    installationId: (null == (t = u.extension_key_info) ? void 0 : t.installation_key) || "",
                                    matchType: (null == (i = u.extension_key_info) ? void 0 : i.match_type) || "extension_key"
                                })
                            }
                                : void 0;
                        return {
                            id: Number(h) || 0,
                            message: this._toLangMessageFromSocketChat(e) || "",
                            postedAt: d,
                            isSystemMessage: !!(r && a && n && l),
                            deletable: !1,
                            disabled: !1,
                            systemMessage: p,
                            poll: m,
                            onSystemChatClick: y
                        }
                    }
                    ,
                    this._toChatFromSocketCapture = e => {
                        const t = {
                            id: Number(e.chat_id || 0),
                            userId: e.user_key,
                            message: e.message || "",
                            userName: e.user_name || "",
                            userColor: e.user_color || "",
                            userCoverImageUrl: e.capture.capture_channel.cover_image_url || "",
                            userIconImageUrl: e.capture.capture_channel.icon_image_url || "",
                            isAuthenticated: !!e.is_authenticated,
                            isOfficial: !!e.capture.capture_channel.is_official,
                            isOfficialHidden: !!e.is_official_hidden,
                            isFresh: !!e.is_fresh,
                            isPremium: !!e.is_premium,
                            isPremiumHidden: !!e.is_premium_hidden,
                            isSubsBadgeHidden: !!e.is_subs_badge_hidden,
                            isSubsDurationHidden: !!e.is_subs_duration_hidden,
                            isWarned: !!e.is_warned,
                            isModerator: !!e.is_moderating,
                            capture: {
                                capture: {
                                    id: e.capture.capture.id || "",
                                    thumbnail_url: e.capture.capture.thumbnail_url || "",
                                    title: e.capture.capture.title || ""
                                },
                                captureChannel: {
                                    id: e.capture.capture_channel.id || "",
                                    nickname: e.capture.capture_channel.nickname || ""
                                }
                            },
                            isSystemMessage: !0
                        };
                        if (e.badges) {
                            const i = (e.badges || [])[0] || {}
                                , s = i.subscription || {};
                            t.subsBadge = {
                                id: i.id || 0,
                                imageUrl: i.image_url || "",
                                months: s.months || 0,
                                tier: s.tier || 0,
                                label: i.label || ""
                            }
                        }
                        return t.disabled = this.isDisabledChat(t),
                            t
                    }
                    ,
                    this._isUllSelectable = () => {
                        if (!this.stores)
                            return !1;
                        const e = this.stores.appStore
                            , t = this.stores.moviePageStore
                            , i = t.movieStore;
                        return !(!i.media.urlUll || C.ie() || "nativeplayer" === t.currentPlayerType || !i.isLiveStreaming || !e.currentPlayerInfo.level || "lowLatency" === e.currentPlayerInfo.level.type)
                    }
                    ,
                    this.isStaff = () => {
                        const e = !(!this.stores || this.stores.userStore.user.id !== this.movieStore.channel.id)
                            , t = !!this.movieStore.channel.isModerating;
                        return e || t
                    }
                    ,
                    this._toChatModeratorFromSocketChat = e => ({
                        id: Number(e.id || 0),
                        message: e.message || "",
                        chatId: Number(e.chat_id || 0),
                        userId: e.user_key || "",
                        recxuserId: e.user_id,
                        userIconImageUrl: e.user_icon || "",
                        userName: e.user_name || "",
                        userColor: e.user_color || (0,
                            ve.toUserColor)(e.user_id),
                        postedAt: e.message_dt,
                        linkUrl: e.link_url || "",
                        isOfficial: "1" === String(e.user_type),
                        isFresh: !!e.is_fresh,
                        isPremium: !!e.is_premium,
                        isPremiumHidden: !!e.is_premium_hidden,
                        isSubsBadgeHidden: !!e.is_subs_badge_hidden,
                        isSubsDurationHidden: !!e.is_subs_duration_hidden,
                        isWarned: !!e.is_warned,
                        isModerator: !!e.is_moderator,
                        isLowLatency: Number(e.quality_type) === v.lT.LOW_LATENCY,
                        isMessageHidden: !!e.hidden_flg
                    }),
                    (0,
                        q.H)(this, {
                            yellStore: o.sH,
                            splitViewStore: o.sH,
                            isOffline: o.sH,
                            isTheaterMode: o.sH,
                            isPrevTheaterMode: o.sH,
                            isChatPopoutPage: o.sH,
                            isExtensionPopoutPage: o.sH,
                            isSplitViewPopoutPage: o.sH,
                            isPlayableAd: o.sH,
                            enabledCeroZ: o.sH,
                            currentTime: o.sH,
                            requireToSeekPlayer: o.sH,
                            isLatest: o.sH,
                            isNonLinearAd: o.sH,
                            now: o.sH,
                            isPaused: o.sH,
                            isManualPaused: o.sH,
                            volume: o.sH,
                            muted: o.sH,
                            vpos: o.sH
                        }),
                    this.v8 = Ji.createStore(),
                    this.movieStore = new Wt,
                    this.chatSettingStore = new Ue,
                    this.chatStore = new re(this.chatSettingStore),
                    this.chatModeratorStore = new ye(this.chatSettingStore),
                    this.commentStore = new Ve,
                    this.userCardStore = new wi,
                    this.yellStore = new Vi,
                    this.leagueStore = new tt,
                    this.splitViewStore = new yi,
                    this.pollStore = new Jt,
                    this.playlistStore = new zt,
                    this.socketStore = new ui,
                    this.adjustStore = new c
            }
            activate() {
                this._inactiveTimer ? ((0,
                    y.A)().clearTimeout(this._inactiveTimer),
                    delete this._inactiveTimer) : (this.chatStore.setChatTransitioning(!0),
                        (this.chatStore.isScrollBottom() || this.chatStore.isScrollingByAuto) && this.chatStore.scrollToEnd({
                            scrollEnd: this.chatStore.scrollEndByTheaterMode
                        })),
                    (0,
                        y.A)().document.body.classList.add("movie-page-user-active"),
                    (0,
                        y.A)().document.body.classList.remove("movie-page-user-inactive"),
                    this._inactiveTimer = (0,
                        y.A)().setTimeout(() => {
                            this.inactivate()
                        }
                            , 3e3)
            }
            inactivate() {
                this.chatStore.setChatTransitioning(!0),
                    (this.chatStore.isScrollBottom() || this.chatStore.isScrollingByAuto) && this.chatStore.scrollToEnd({
                        scrollEnd: this.chatStore.scrollEndByTheaterMode
                    }),
                    (0,
                        y.A)().document.body.classList.add("movie-page-user-inactive"),
                    (0,
                        y.A)().document.body.classList.remove("movie-page-user-active"),
                    this._inactiveTimer && ((0,
                        y.A)().clearTimeout(this._inactiveTimer),
                        delete this._inactiveTimer)
            }
            enterScrollTop() {
                (0,
                    y.A)().document.body.classList.add("is-movie-page-top")
            }
            exitScrollTop() {
                (0,
                    y.A)().document.body.classList.remove("is-movie-page-top")
            }
            postViewingTimeLog(e, t) {
                return ls(this, null, function* () {
                    var i, s;
                    const o = this.movieStore
                        , r = this.playlistStore;
                    var a;
                    if ((!o.isSpecial || o.isSpecialPlayable) && this.stores)
                        try {
                            const n = this.stores.appStore.currentPlayerInfo.level
                                , l = (() => {
                                    if (n)
                                        return this.movieStore.isArchive ? 3 : "normal" === n.type ? 0 : "lowLatency" === n.type ? 1 : 2
                                }
                                )()
                                , d = yield (a = {
                                    openrecUserId: e.user.openrecUserId || 0,
                                    recxuserId: e.user.recxuserId || 0,
                                    isPremium: e.user.isPremium ? 1 : 0,
                                    creatorRecxuserId: this.movieStore.channel.recxuserId,
                                    creatorOpenrecUserId: this.movieStore.channel.openrecUserId,
                                    movieId: o.movieId,
                                    gameId: (null == (i = o.currentGame) ? void 0 : i.gameId) || o.game.gameId,
                                    movieType: o.movieType || 0,
                                    position: Math.floor(t),
                                    seconds: v.b,
                                    chapterKey: null == (s = o.currentChapter) ? void 0 : s.id,
                                    playListKey: r.playlistId,
                                    sessionId: this._sessionId,
                                    visibility: "visible" === document.visibilityState ? 1 : 0,
                                    bitrate: this._playingVideobitrate,
                                    quality: n ? "auto" === n.name ? "auto" : `${n.width}x${n.height}` : void 0,
                                    videoHeight: this._playingVideoHeight,
                                    videoWidth: this._playingVideoWidth,
                                    videoAreaHeight: this.videoHeight,
                                    videoAreaWidth: this.videoWidth,
                                    latency: this.movieStore.isLiveStreaming && this.isLatest && this.pdt ? Date.now() - this.pdt : void 0,
                                    viewMode: l
                                },
                                    (0,
                                        zi.A)("POST", "/user/viewing_time/update", {
                                            query: void 0,
                                            body: void 0,
                                            form: {
                                                creatorRecxuserId: a.creatorRecxuserId,
                                                creatorOpenrecUserId: a.creatorOpenrecUserId,
                                                openrecUserId: a.openrecUserId,
                                                recxuserId: a.recxuserId,
                                                isPremium: a.isPremium,
                                                movieType: a.movieType,
                                                movieId: a.movieId,
                                                gameId: a.gameId,
                                                playListKey: a.playListKey,
                                                chapterKey: a.chapterKey,
                                                playListId: a.playListId,
                                                os: a.os,
                                                osVersion: a.osVersion,
                                                model: a.model,
                                                bundleId: a.bundleId,
                                                position: a.position,
                                                seconds: a.seconds,
                                                archiveUnlimited: a.archiveUnlimited,
                                                sessionId: a.sessionId,
                                                visibility: a.visibility,
                                                bitrate: a.bitrate,
                                                quality: a.quality,
                                                videoHeight: a.videoHeight,
                                                videoWidth: a.videoWidth,
                                                videoAreaHeight: a.videoAreaHeight,
                                                videoAreaWidth: a.videoAreaWidth,
                                                latency: a.latency,
                                                bufferingRatio: a.bufferingRatio,
                                                startupTime: a.startupTime,
                                                viewMode: a.viewMode
                                            }
                                        }));
                            d.errorMessage && console.error(d.errorMessage)
                        } catch (e) {
                            console.error(e)
                        }
                })
            }
            postAdViewingTimeLog(e, t, i) {
                return ls(this, null, function* () {
                    var s;
                    const o = this.movieStore
                        , r = this.playlistStore
                        , a = t.getAd();
                    if (!a)
                        return;
                    if (![e.AdEvent.Type.STARTED, e.AdEvent.Type.FIRST_QUARTILE, e.AdEvent.Type.MIDPOINT, e.AdEvent.Type.THIRD_QUARTILE, e.AdEvent.Type.COMPLETE, e.AdEvent.Type.SKIPPED, e.AdEvent.Type.CLICK].includes(t.type))
                        return;
                    const n = {};
                    n[e.AdEvent.Type.STARTED] = 0,
                        n[e.AdEvent.Type.FIRST_QUARTILE] = .25,
                        n[e.AdEvent.Type.MIDPOINT] = .5,
                        n[e.AdEvent.Type.THIRD_QUARTILE] = .75,
                        n[e.AdEvent.Type.COMPLETE] = 1;
                    try {
                        const e = yield (l = {
                            openrecUserId: i.user.openrecUserId || 0,
                            recxuserId: i.user.recxuserId || 0,
                            isPremium: i.user.isPremium ? 1 : 0,
                            creatorRecxuserId: this.movieStore.channel.recxuserId,
                            creatorOpenrecUserId: this.movieStore.channel.openrecUserId,
                            movieId: o.movieId,
                            movieType: o.movieType || 0,
                            playListKey: r.playlistId,
                            chapterKey: null == (s = o.chapterOnFirstPlay) ? void 0 : s.id,
                            percentage: n[t.type],
                            adEventName: t.type,
                            adSystem: a.getAdSystem(),
                            advertiserName: a.getAdvertiserName(),
                            skippable: Number(a.isSkippable()),
                            liner: Number(a.isLinear()),
                            duration: a.getDuration(),
                            adTitle: a.getTitle(),
                            adDescription: a.getDescription()
                        },
                            (0,
                                zi.A)("POST", "/user/ad_viewing_time/update", {
                                    query: void 0,
                                    body: void 0,
                                    form: {
                                        creatorRecxuserId: l.creatorRecxuserId,
                                        creatorOpenrecUserId: l.creatorOpenrecUserId,
                                        openrecUserId: l.openrecUserId,
                                        recxuserId: l.recxuserId,
                                        isPremium: l.isPremium,
                                        movieType: l.movieType,
                                        movieId: l.movieId,
                                        gameId: l.gameId,
                                        playListKey: l.playListKey,
                                        chapterKey: l.chapterKey,
                                        playListId: l.playListId,
                                        os: l.os,
                                        osVersion: l.osVersion,
                                        model: l.model,
                                        bundleId: l.bundleId,
                                        seconds: l.seconds,
                                        percentage: l.percentage,
                                        adEventName: l.adEventName,
                                        adSystem: l.adSystem,
                                        advertiserName: l.advertiserName,
                                        skippable: l.skippable,
                                        liner: l.liner,
                                        duration: l.duration,
                                        adTitle: l.adTitle,
                                        adDescription: l.adDescription
                                    }
                                }));
                        e.errorMessage && console.error(e.errorMessage)
                    } catch (e) {
                        console.error(e)
                    }
                    var l
                })
            }
            postShareLog(e) {
                return ls(this, null, function* () {
                    e.isLogined && is.A.post(this.movieStore.id)
                })
            }
            willLoadOnServer(e, t, i, s) {
                var a, n, l;
                this.v8.willLoadOnServer(e, t, i, s);
                const d = t.ids;
                this.movieStore.id = null != (a = d.movieId) ? a : "";
                const h = t.opts || {};
                this.isChatPopoutPage = !!h.isChatPopoutPage,
                    this.isExtensionPopoutPage = !!h.isExtensionPopoutPage,
                    this.isSplitViewPopoutPage = !!h.isSplitViewPopoutPage;
                const u = (0,
                    r.WI)(["__iMovie__", t.ids.movieId, t.searchParams.secret_key]);
                if (!(null == (n = s.fallback) ? void 0 : n[u]))
                    return e.appStore.updateStatusCode(404),
                        void (0,
                            o.h5)(() => {
                                this.movieStore.notFound = !0
                            }
                            );
                this.movieStore.updateMovie(e.appStore, null == (l = s.fallback) ? void 0 : l[u]),
                    this.movieStore.willLoadOnServer(e)
            }
            willLoad(e, t, i) {
                return ls(this, null, function* () {
                    var s;
                    const r = this.v8.willLoad(e, t, i);
                    this._intervalIdPerSeconds = (0,
                        y.A)().setInterval(() => {
                            (0,
                                o.h5)(() => {
                                    this.now = new Date
                                }
                                )
                        }
                            , 1e3),
                        this.stores = e,
                        this.page = t;
                    const a = t.ids;
                    this.movieStore.id = null != (s = a.movieId) ? s : "";
                    const n = t.opts || {};
                    this.isChatPopoutPage = !!n.isChatPopoutPage,
                        this.isExtensionPopoutPage = !!n.isExtensionPopoutPage,
                        this.isSplitViewPopoutPage = !!n.isSplitViewPopoutPage,
                        (0,
                            p.Ay)().getMoviePagePlayMode().isTheaterMode && this.enterTheaterMode(),
                        this.isTheaterMode && (0,
                            y.A)().document.body.classList.add("is-movie-page-theater-mode"),
                        this._disposeTheaterModeAvailableReaction = (0,
                            o.mJ)(() => this.isTheaterModeAvailable, e => {
                                e ? (0,
                                    p.Ay)().getMoviePagePlayMode().isTheaterMode && this.enterTheaterMode() : this.exitTheaterMode({
                                        canceledLocalStorage: !0
                                    })
                            }
                            ),
                        this.enterScrollTop(),
                        this.activate(),
                        this.movieArticle && S.scrollTo(this.movieArticle, 0, 200, b.easeOutCubic);
                    const l = (0,
                        y.A)().pageStore;
                    if (l && "moviePageStore" === l.storeName)
                        delete l.v8,
                            this.reset(l),
                            delete (0,
                                y.A)().pageStore;
                    else {
                        this.reset();
                        try {
                            yield this.movieStore.fetchMovieAndUpdate(e.appStore)
                        } catch (e) {
                            console.error(e)
                        }
                    }
                    if (this.chatStore.setStores(e),
                        this.movieStore.fetchSubsProductsOfChannelAndUpdate(),
                        e.appStore.isMobile && (0,
                            y.A)().document.body.classList.add("feature-sp-chat"),
                        this._disposeInitialFetchCallback = (0,
                            o.z7)(() => e.userStore.isCompletedInitialFetch, () => {
                                e.userStore.isLogined || (this.movieStore.isUploadedMovie ? this._willLoadForComment(e) : this._willLoadForChat(e)),
                                    this.movieStore.fetchVoteResultAndUpdate(),
                                    this._remainingMidRollTime = v.mZ,
                                    this._startPreRoll(),
                                    this.startMidRolltimer(),
                                    e.userStore.isLogined || !this.movieStore.isArchive && !this.movieStore.isUploadedMovie || this.startRectangleAd()
                            }
                            ),
                        this._disposeMovieDetailFetchCallback = (0,
                            o.z7)(() => this.movieStore.isCompletedFetchMovieDetail, () => {
                                (this.movieStore.isArchive || this.movieStore.isUploadedMovie) && this.startRectangleAd()
                            }
                            ),
                        this._disposeLoginCallback = (0,
                            o.z7)(() => e.userStore.isLogined, () => {
                                this._processAfterLogin(e)
                            }
                            ),
                        this._disposeMediaEndCallback = (0,
                            o.mJ)(() => this.movieStore.isFinishedMedia, () => {
                                this.movieStore.isFinishedMedia && (this.fetchFinishedRelatedMovieAndUpdate(),
                                    this.movieStore.setPlayPosition(0),
                                    this.movieStore.setIsAutoPlay(!1),
                                    e.userStore.isLogined && !this.movieStore.channel.isFollowing && this.movieStore.channel.id !== e.userStore.user.id && (this.movieStore.isLiveStreaming || this.movieStore.isArchive) && this.chatStore.addSystemChatForFollowAppealToChatQue(),
                                    this.showMovieAlert())
                            }
                            ),
                        this._disposeLevelUpdateToChildWindow = (0,
                            o.fm)(() => {
                                e.appStore.currentPlayerInfo.level && e.appStore.currentPlayerInfo.level.type && this.chatPopoutWindow && this.chatPopoutWindow.updateLevel && this.chatPopoutWindow.updateLevel(e.appStore.currentPlayerInfo.level)
                            }
                            ),
                        this._disposeDVRChangedReaction = (0,
                            o.mJ)(() => !!this.isLatest, e => {
                                if (!this.movieStore.isLiveStreaming)
                                    return;
                                const t = this.chatStore.isReproducingChatInDvr
                                    , i = !e;
                                this.chatStore.setReproducingChatsInDvr(i),
                                    t && !i && (this.chatStore.fetchChatAndReset(this.movieStore.id),
                                        this.yellStore.fetchTopSupporterAndUpdate({
                                            movieId: this.movieStore.id
                                        }),
                                        this.chatStore.scrollToEnd()),
                                    !t && i && this.refetchAllArchiveChats()
                            }
                            ),
                        this._disposeViewLogPostAutorun = (0,
                            o.fm)(() => {
                                if (this.needsToPostViewLog) {
                                    const e = this.movieStore
                                        , t = Date.now();
                                    os.A.addMovieViewHistory({
                                        movieId: e.id,
                                        userId: e.channel.id,
                                        gameId: e.game.id,
                                        viewedAt: t
                                    }),
                                        this._postViewLog()
                                }
                            }
                            ),
                        this._disposeReceivedPdtWhen = (0,
                            o.z7)(() => null !== this.pdt, () => {
                                if (this.movieStore.isArchive && this.movieStore.startedAt) {
                                    const e = this.pdt ? new Date(this.pdt) : Ki.A.createModel(this.movieStore.startedAt).add(this.currentTime || 0, "SECOND").toDate()
                                        , t = (0,
                                            Q.wO)(e);
                                    this.refetchAllArchiveChats({
                                        dateStr: t,
                                        offset: v.g6
                                    })
                                }
                            }
                            ),
                        (0,
                            C.ios)() && this.movieStore.isArchive && this.movieStore.startedAt) {
                        const e = Ki.A.createModel(this.movieStore.startedAt).add(this.currentTime || 0, "SECOND").toDate()
                            , t = (0,
                                Q.wO)(e);
                        this.refetchAllArchiveChats({
                            dateStr: t,
                            offset: v.g6
                        })
                    }
                    const d = (0,
                        y.A)().document;
                    Yi.A.FULLSCREENCHANGES.forEach(e => {
                        d.addEventListener(e, this._handleFullScreenChange)
                    }
                    ),
                        this.isChatPopoutPage ? e.appStore.updatePlayerInfo({
                            level: (0,
                                p.Ay)().getLiveVideoLevel(this.movieStore.isLowLatency ? "lowLatency" : "normal")
                        }) : this._disposeLoadMovieWhen = (0,
                            o.z7)(() => this.movieStore.movieId > 0, () => {
                                this.fetchCapturesAndUpdate(),
                                    Ji.updateRelatedMovies(this.v8.state, i.userStore)
                            }
                            );
                    const h = oi.Ay.getChatPositionWithFullscreenMode();
                    return h && (this.chatPositionWithFullscreenMode = h),
                        this._adIntervalTime = v.J,
                        this._createSessionId(),
                        this.chatStore.willLoad(e),
                        this.chatSettingStore.willLoad(e),
                        this.chatModeratorStore.willLoad(e),
                        this.commentStore.willLoad(e),
                        this.movieStore.willLoad(e),
                        this.userCardStore.willLoad(e),
                        this.leagueStore.willLoad(e),
                        this.yellStore.willLoad(e),
                        this.pollStore.willLoad(e),
                        this.playlistStore.willLoad(e),
                        this.socketStore.willLoad(e),
                        this.adjustStore.willLoad(e),
                        this.splitViewStore.willLoad(e),
                        r
                })
            }
            willUnload() {
                this.v8.willUnload(),
                    this._intervalIdPerSeconds && ((0,
                        y.A)().clearInterval(this._intervalIdPerSeconds),
                        this._intervalIdPerSeconds = 0),
                    this._viewsLimitAdIntervalId && ((0,
                        y.A)().clearInterval(this._viewsLimitAdIntervalId),
                        this._viewsLimitAdIntervalId = void 0),
                    this._rectangleAdIntervalId && ((0,
                        y.A)().clearInterval(this._rectangleAdIntervalId),
                        this._rectangleAdIntervalId = void 0),
                    this._rectangleAdTimeoutId && ((0,
                        y.A)().clearInterval(this._rectangleAdTimeoutId),
                        this._rectangleAdTimeoutId = void 0),
                    this._midRollTimeoutId && ((0,
                        y.A)().clearTimeout(this._midRollTimeoutId),
                        this._midRollTimeoutId = void 0),
                    this._inactiveTimer && ((0,
                        y.A)().clearTimeout(this._inactiveTimer),
                        delete this._inactiveTimer),
                    this._fetchMembershipIntervalId && ((0,
                        y.A)().clearTimeout(this._fetchMembershipIntervalId),
                        delete this._fetchMembershipIntervalId),
                    this.currentTime = 0,
                    this.hasAlreadyPostedViewLog = !1,
                    this.isCompletedFetchRelatedMovies = !1,
                    this.isNonLinearAd = void 0,
                    this.pdt = null,
                    this.pdtPlaybackTime = 0,
                    this._sessionId = "",
                    this._playingVideobitrate = 0,
                    this._playingVideoWidth = 0,
                    this._playingVideoHeight = 0,
                    this._membershipFetchCount = 0,
                    this.isVisibleRectangleAd = !1,
                    this.mridx = 0,
                    this._sentContinuousBufferStalledErrorMessage = !1,
                    this.stopAd(),
                    this._disposeDVRChangedReaction(),
                    this._disposeLevelUpdateToChildWindow(),
                    this._disposeInitialFetchCallback(),
                    this._disposeMovieDetailFetchCallback(),
                    this._disposeLoginCallback(),
                    this._disposeTheaterModeAvailableReaction(),
                    this._disposeSubscribedCallback(),
                    this._disposePpvTicketPurchaseCallback(),
                    this._disposeViewLogPostAutorun(),
                    this._disposeLoadMovieWhen(),
                    this._disposeReceivedPdtWhen(),
                    this._disposeReceivedIsPlayableWhen(),
                    this.aborts.length && (this.aborts.map(e => {
                        e()
                    }
                    ),
                        this.aborts = []);
                const e = (0,
                    y.A)().document;
                Yi.A.FULLSCREENCHANGES.forEach(t => {
                    e.removeEventListener(t, this._handleFullScreenChange)
                }
                ),
                    this.relatedMovieStreams = [],
                    (0,
                        y.A)().document.body.classList.remove("is-movie-page-theater-mode"),
                    (0,
                        y.A)().document.body.classList.remove("is-movie-page-existed-chat-moderator"),
                    (0,
                        y.A)().document.body.classList.remove("movie-page-user-active"),
                    (0,
                        y.A)().document.body.classList.remove("movie-page-user-inactive"),
                    (0,
                        y.A)().document.body.classList.remove("is-movie-page-top"),
                    (0,
                        y.A)().removeEventListener("resize", this.syncVideoWrapperSize, !1),
                    this.chatSettingStore.willUnload(),
                    this.commentStore.willUnload(),
                    this.movieStore.willUnload(),
                    this.chatStore.willUnload(),
                    this.chatModeratorStore.willUnload(),
                    this.userCardStore.willUnload(),
                    this.yellStore.willUnload(),
                    this.pollStore.willUnload(),
                    this.playlistStore.willUnload(),
                    this.socketStore.willUnload(),
                    this.adjustStore.willUnload(),
                    this.splitViewStore.willUnload(),
                    this._disposeMediaEndCallback(),
                    delete this.videoWrapperEl,
                    this.videoContainer = null,
                    this.stores && this.stores.stampStore.resetStamps()
            }
            setVideoWrapperSizeSync() {
                (0,
                    y.A)().addEventListener("resize", this.syncVideoWrapperSize, !1)
            }
            syncVideoWrapperSize() {
                this.videoWrapperEl && (this.videoWidth = this.videoWrapperEl.clientWidth,
                    this.videoHeight = this.videoWrapperEl.clientHeight)
            }
            enableCeroZ() {
                (0,
                    p.Ay)().setAgeVerification({
                        verifiedAt: new Date
                    }),
                    this.enabledCeroZ = !0
            }
            handleSocketMessage(e) {
                return ls(this, null, function* () {
                    var t, i, s, r, a;
                    if (!this.stores)
                        return;
                    const n = JSON.parse(e)
                        , l = Ye
                        , d = this.stores
                        , h = d.appStore
                        , u = this.movieStore
                        , c = this.chatStore
                        , p = this.stores.langStore;
                    [l.BREAK_TIME_START].includes(n.type) && this.socketStore.setSocketMessage(n);
                    let m = !1;
                    switch (u.isOwner && (m = !0),
                    Number(n.type)) {
                        case l.CHAT_ADD:
                            if (!this.movieStore.isUserChatReceivable)
                                break;
                            if (n.data.capture)
                                this.chatStore.addChats(this.movieStore.id, [this._toChatFromSocketCapture(n.data)]),
                                    this.fetchCapturesAndUpdate(),
                                    (0,
                                        y.A)().setTimeout(() => {
                                            this.fetchCapturesAndUpdate()
                                        }
                                            , 1e3 * v.qe.ONE_MINUTES);
                            else {
                                const e = n.data
                                    , t = this._toChatFromSocketChat(e);
                                this.chatStore.addChatAndUpdateYellInfo(t)
                            }
                            break;
                        case l.CHATLIST_MODE:
                            {
                                const e = n.data
                                    , o = v.vt + v.ap
                                    , a = e.interval || o
                                    , l = Math.floor(Math.floor(a / o) * v.Ln);
                                this.chatStore.fetchChatsAndUpdateChatlistQue(this.movieStore.id, {
                                    fromCreatedAt: null == (t = e.queries) ? void 0 : t.from_created_at,
                                    toCreatedAt: null == (i = e.queries) ? void 0 : i.to_created_at,
                                    isIncludingSystemMessage: null == (s = e.queries) ? void 0 : s.is_including_system_message,
                                    publishType: null == (r = e.queries) ? void 0 : r.publish_type,
                                    limit: l
                                });
                                break
                            }
                        case l.VIEWERS_COUNT:
                            const e = n.data;
                            this.movieStore.updateViews(Number(e.viewers), Number(e.live_viewers));
                            break;
                        case l.STREAM_START:
                            this.movieStore.startLiveStreaming();
                            break;
                        case l.STREAM_END:
                            this.finishLiveStreaming();
                            break;
                        case l.BLACKLIST_ADD:
                            const g = n.data;
                            this.chatStore.addOwnerBlacklist(Number(g.owner_to_banned_user_id)),
                                this.movieStore.isOwner && this.chatStore.fetchBlacklistAndUpdate();
                            break;
                        case l.BLACKLIST_DELETE:
                            const S = n.data;
                            this.chatStore.deleteOwnerBlacklist(Number(S.owner_to_banned_user_id)),
                                this.movieStore.isOwner && this.chatStore.fetchBlacklistAndUpdate();
                            break;
                        case l.MODERATOR_ADD:
                            const b = n.data;
                            this.chatStore.addModerator(Number(b.owner_to_moderator_user_id));
                            break;
                        case l.MODERATOR_DELETE:
                            const f = n.data;
                            this.chatStore.deleteModerator(Number(f.owner_to_moderator_user_id));
                            break;
                        case l.DASHBOARD_BROADCAST:
                            const C = n.data
                                , A = {
                                    publicType: u.publicType,
                                    permissions: u.permissions,
                                    hasPermissionForMemberOnly: u.hasPermissionForMemberOnly
                                }
                                , _ = u.chatPublicType;
                            u.updateReceivedBroadcastInfo(C);
                            const P = {
                                publicType: u.publicType,
                                permissions: u.permissions,
                                hasPermissionForMemberOnly: u.hasPermissionForMemberOnly
                            }
                                , M = u.chatPublicType;
                            if (h.currentQueryInfo.secretKey)
                                ;
                            else {
                                const e = ((e, t) => {
                                    var i, s, o, r;
                                    const a = e => !!e.subsProductId
                                        , n = null == (s = null == (i = e.permissions) ? void 0 : i.find(a)) ? void 0 : s.subsProductId
                                        , l = null == (r = null == (o = t.permissions) ? void 0 : o.find(a)) ? void 0 : r.subsProductId
                                        , d = !!t.publicType && e.publicType !== t.publicType
                                        , h = n !== l;
                                    switch (t.publicType) {
                                        case "all":
                                        case "member_trial":
                                            return d ? e.hasPermissionForMemberOnly ? "none" : "needToCallMovieApi" : "none";
                                        case "member":
                                            return d || h ? t.hasPermissionForMemberOnly ? e.hasPermissionForMemberOnly ? "none" : "needToCallMovieDetailApi" : "deleteMedia" : "none";
                                        default:
                                            return "none"
                                    }
                                }
                                )(A, P);
                                switch (e) {
                                    case "none":
                                        break;
                                    case "needToCallMovieApi":
                                        yield u.fetchMovieAndUpdate(h, {
                                            cacheBuster: `public_type_${P.publicType}`
                                        });
                                        break;
                                    case "needToCallMovieDetailApi":
                                        yield u.fetchMovieDetailAndUpdate(d);
                                        break;
                                    case "deleteMedia":
                                        u.resetMedia()
                                }
                            }
                            (!!P.publicType && A.publicType !== P.publicType || !!M && _ !== M) && (!u.isMemberOnly || u.isMemberOnlyChatPostable || u.isUserChatPublic || c.addSystemChatToChatQue(c.createSystemChat("memberOnly"))),
                                d.headStore.update(this, this.page),
                                void 0 !== C.orientation && u.rotatePlayer(C.orientation),
                                void 0 !== C.is_viewers_hidden && u.updateIsViewersHidden(1 === C.is_viewers_hidden);
                            const w = (null == (a = C.need_refresh) ? void 0 : a[0]) || "";
                            "movie" === w ? yield u.changeMoviesGameAndExtension({
                                c: C.cache_key || void 0
                            }) : "fes" === w && (yield u.fetchFesEntriesAndUpdate(C.cache_key || void 0));
                            break;
                        case l.SYSTEM_MESSAGE_V2:
                            const T = this._toSystemMessageFromSocketChat(n.data);
                            this.chatStore.addChats(this.movieStore.id, [T]),
                                "ext_fukubiki_win_prizes" === n.data.system_message.type ? this.splitViewStore.addNotificationChat(T) : "subs_share_win" === n.data.system_message.type && (this.splitViewStore.addNotificationChat(T),
                                    n.data.system_message.user.id === this.stores.userStore.user.id && this._fetchMembershipInterval());
                            break;
                        case l.CHATMODERATOR_ADD:
                            this.chatModeratorStore.addChatModeratorQue([this._toChatModeratorFromSocketChat(n.data)]);
                            break;
                        case l.CHATMODERATOR_UPDATE:
                            const E = this._toChatModeratorFromSocketChat(n.data)
                                , U = Number(E.chatId);
                            if (!U)
                                return;
                            if (!(yield this.chatModeratorStore.updatedChatModerator(E)))
                                return;
                            const L = this._toChatFromSocketChat(n.data);
                            L.postedAt = E.postedAt;
                            const k = this.chatStore.chats.map(e => e.id === U ? L : e);
                            k.sort((e, t) => {
                                const i = I.parse(e.postedAt || 0).getTime()
                                    , s = I.parse(t.postedAt || 0).getTime();
                                return i < s ? -1 : i > s ? 1 : e.id < t.id ? -1 : e.id > t.id ? 1 : 0
                            }
                            ),
                                (0,
                                    o.h5)(() => {
                                        this.chatStore.chats.replace(k)
                                    }
                                    );
                            break;
                        case l.CHATMODERATOR_DELETE:
                            const H = n.data
                                , O = Number(H.chat_id) || 0;
                            this.chatModeratorStore.deleteChatModeratorByChatModeratorId(H.id);
                            const D = this.chatStore.chats.find(e => e.id === O);
                            D && this.chatStore.chats.remove(D);
                            break;
                        case l.CHATMODERATOR_SHOWPLAYER:
                            this.chatModeratorStore.setLatestMessage(n.data.message, n.data.chat_id, n.data.link_url);
                            break;
                        case l.SUBSCRIBED:
                            const x = this._toLangMessageFromSocketChat(n.data);
                            x && (this.chatStore.addSystemChatForPurchasedToChatQue("subs", x),
                                this.splitViewStore.addPurchaseNotification("subs", x));
                            break;
                        case l.POLL_START:
                            m || (this.movieStore.initPoll(),
                                this.pollStore.show(),
                                this.movieStore.updatePollFromLegacy(n.data));
                            break;
                        case l.POLL_FINISH:
                            m || (this.pollStore.show(),
                                this.movieStore.updatePollFromLegacy(n.data));
                            break;
                        case l.PPV_TICKET_PURCHASED:
                            if (!u.hasPpvTicketProducts) {
                                const e = this._toLangMessageFromSocketChat(n.data);
                                e && (this.chatStore.addSystemChatForPurchasedToChatQue("ppv", e),
                                    this.splitViewStore.addPurchaseNotification("ppv", e))
                            }
                            break;
                        case l.EXTENSION_DISPLAY:
                            u.addDisplayExtension({
                                id: n.data.extension_key,
                                installationId: n.data.installation_key,
                                matchType: "exact"
                            }),
                                c.addSystemChatToChatQue(c.createSystemChat("extension", p.extension.displayChat.replace("{name}", this.movieStore.channel.name).replace("{extension.title}", n.data.name), () => u.setDisplayExtension({
                                    id: n.data.extension_key,
                                    installationId: n.data.installation_key,
                                    matchType: "exact"
                                })));
                            break;
                        case l.EXTENSION_NOTIFY:
                            const F = n.data
                                , B = F.extension_key || ""
                                , X = F.installation_key || ""
                                , W = F.match_type || ""
                                , N = u.extensions.find(e => "extension_key" === W ? e.id === B : ("installation_key" === W || e.id === B) && e.installationId === X);
                            N && Tt.A.emitExtensionNotify(this.v8.extensionStore, N, {
                                data: F.extension_data
                            });
                            break;
                        case l.WATCHING_STOP:
                            this.movieStore.stopToWatchingOnThisDevice();
                            break;
                        case l.MOVIE_SWITCH_TO_BACKUP:
                            this.movieStore.switchToBackupMedia(n.data.id);
                            break;
                        case l.YELL_REACTION:
                            const V = R.A.createModel(n.data);
                            this.chatStore.updateYellReaction(Number(n.data.target_id), V);
                            break;
                        case l.PLAYER_REFRESH:
                            this.updatePlayerRefresh(Date.now())
                    }
                })
            }
            reset(e) {
                this.chatStore = new re(this.chatSettingStore),
                    this.commentStore = new Ve,
                    this.yellStore = new Vi,
                    this.leagueStore = new tt;
                const t = this.stores;
                this.movieStore ? (this.movieStore.resetForTransition(),
                    this.isPlayableAd = void 0) : this.movieStore = new Wt,
                    this.isOffline = !1,
                    this.hideCapturePremiumAppealDialog(),
                    e && ((0,
                        ge.extendDeepWith)(this, e),
                        this.movieStore.updateChaptersFromLegacy(this.movieStore.chapters),
                        this.movieStore.ppvEvent && this.movieStore.updatePpv(this.movieStore.ppvEvent),
                        this.stores = t)
            }
            fetchCapturesAndUpdate() {
                return ls(this, null, function* () {
                    try {
                        const e = yield (0,
                            Gi.O1)({
                                movieId: this.movieStore.id
                            });
                        (0,
                            o.h5)(() => {
                                this._capturesOfMovie = e.map(st),
                                    this.isCompletedFetchRelatedMovies = !0
                            }
                            )
                    } catch (e) {
                        console.error(e),
                            (0,
                                o.h5)(() => {
                                    this.isCompletedFetchRelatedMovies = !0
                                }
                                ),
                            this.showErrorMessage("関連動画の取得に失敗しました")
                    }
                })
            }
            finishLiveStreaming() {
                this.isOffline = !0,
                    this.movieStore.finishLiveStreaming()
            }
            fetchFinishedRelatedMovieAndUpdate() {
                return ls(this, null, function* () {
                    try {
                        const i = yield (e = this.movieStore.id,
                            t = {
                                limit: this.RELATED_MOVIE_LIMIT
                            },
                            (0,
                                k.A)("GET", `/movies/${e}/related-movies`, {
                                    query: t,
                                    body: void 0
                                }));
                        (0,
                            o.h5)(() => {
                                this.relatedMovieStreams = i.slice(0, this.RELATED_MOVIE_SHOW_LIMIT).map(n.S8)
                            }
                            )
                    } catch (e) {
                        console.error(e),
                            this.showErrorMessage("関連動画の取得に失敗しました")
                    }
                    var e, t
                })
            }
            handleCurrentTimeChange(e) {
                var t;
                this.currentTime = e,
                    null == (t = this.stores) || t.moviePageStore.updateWhetherToSeekPlayer(!1)
            }
            handleSeekTimeChange() {
                const e = this.stores && this.stores.moviePageStore;
                e && e.refetchAllArchiveChats({
                    offset: v.g6
                })
            }
            handlePdtChange(e, t) {
                this.pdt = e,
                    this.pdtPlaybackTime = t
            }
            handleSeekPdtChange(e) {
                const t = this.movieStore;
                if (!t.startedAt)
                    return;
                const i = e ? new Date(e) : Ki.A.createModel(t.startedAt).add(this.currentTime || 0, "SECOND").toDate()
                    , s = (0,
                        Q.wO)(i);
                this.refetchAllArchiveChats({
                    dateStr: s,
                    offset: v.g6
                })
            }
            updateWhetherToSeekPlayer(e) {
                this.requireToSeekPlayer = e
            }
            handlePaused(e) {
                this.isPaused = e
            }
            handleManualPaused(e) {
                this.isManualPaused = e
            }
            handleVolumeChange(e, t) {
                this.volume = e,
                    this.muted = t
            }
            setLatest(e) {
                this.isLatest = e
            }
            setNonLinearAd(e) {
                this.isNonLinearAd = e
            }
            enterTheaterMode(e = {}) {
                this.isTheaterMode = !0,
                    this.chatStore.setChatTransitioning(!1),
                    (0,
                        y.A)().document.body.classList.add("is-movie-page-theater-mode"),
                    this.changeChatModeratorForTheaterMode(),
                    e.canceledLocalStorage || (0,
                        p.Ay)().setMoviePagePlayMode({
                            isTheaterMode: !0
                        })
            }
            exitTheaterMode(e = {}) {
                if (this.isTheaterMode = !1,
                    (0,
                        y.A)().document.body.classList.remove("is-movie-page-theater-mode"),
                    (0,
                        y.A)().document.body.classList.remove("is-movie-page-existed-chat-moderator"),
                    C.chrome()) {
                    const e = (0,
                        y.A)().document.getElementsByClassName("video-player-wrapper")[0];
                    e && (0,
                        y.A)().requestAnimationFrame(() => {
                            e.classList.add("player-wrapper-resize"),
                                (0,
                                    y.A)().requestAnimationFrame(() => {
                                        e.classList.remove("player-wrapper-resize")
                                    }
                                    )
                        }
                        )
                }
                e.canceledLocalStorage || (0,
                    p.Ay)().setMoviePagePlayMode({
                        isTheaterMode: !1
                    })
            }
            toggleTheaterMode() {
                this.isTheaterMode ? this.exitTheaterMode() : this.enterTheaterMode(),
                    this.chatStore.scrollToEnd({
                        scrollEnd: this.chatStore.scrollEndByTheaterMode
                    })
            }
            changeChatModeratorForTheaterMode() {
                this.chatModeratorStore.latestTelop.message ? (0,
                    y.A)().document.body.classList.add("is-movie-page-existed-chat-moderator") : (0,
                        y.A)().document.body.classList.remove("is-movie-page-existed-chat-moderator")
            }
            setChatPopoutWindow(e) {
                this.chatPopoutWindow = e
            }
            closeChatPopoutWindow() {
                this.chatPopoutWindow && (this.chatPopoutWindow.closed || this.chatPopoutWindow.close(),
                    this.chatPopoutWindow = null)
            }
            setExtensionPopoutWindow(e) {
                this.exntensionPopoutWindow = e
            }
            closeExtensionPopoutWindow() {
                this.exntensionPopoutWindow && (this.exntensionPopoutWindow.closed || this.exntensionPopoutWindow.close(),
                    this.exntensionPopoutWindow = null)
            }
            startAd(e) {
                this.isPlayableAd = !0,
                    this.vpos = e,
                    "midroll" === e && this.mridx++
            }
            stopAd() {
                this.isPlayableAd = !1,
                    this.vpos = void 0
            }
            startRectangleAd() {
                this.setIsVisibleRectangleAd(!0),
                    this._rectangleAdTimeoutId = (0,
                        y.A)().setTimeout(() => {
                            this.setIsVisibleRectangleAd(!1)
                        }
                            , v.TF)
            }
            updatePlayerType(e) {
                this.currentPlayerType = "",
                    this.playerTypeUpdateIntervalId && (0,
                        y.A)().clearTimeout(this.playerTypeUpdateIntervalId),
                    this.playerTypeUpdateIntervalId = (0,
                        y.A)().setTimeout(() => {
                            (0,
                                o.h5)(() => {
                                    this.currentPlayerType = e,
                                        this.playerTypeUpdateIntervalId = 0
                                }
                                )
                        }
                            , 100)
            }
            updatePlayerRefresh(e) {
                this.refreshTime = e
            }
            showCapturePremiumAppealDialog() {
                this.isCapturePremiumDialogVisible = !0
            }
            hideCapturePremiumAppealDialog() {
                this.isCapturePremiumDialogVisible = !1
            }
            handleRotateButtonClick() {
                const e = this.isChasingPlaybackInDvr ? this.movieStore.dvrOrientation : this.movieStore.orientation;
                this.movieStore.rotatePlayerWhenRotateButtonClick(e >= 90 ? e - 90 : 270)
            }
            handleContinuousBufferStalledError() {
                if (!this.stores)
                    return;
                if (this._sentContinuousBufferStalledErrorMessage)
                    return;
                this._sentContinuousBufferStalledErrorMessage = !0;
                const e = this.chatStore.createSystemChat("custom", this.stores.langStore.video.playbackNotBeStable);
                this.chatStore.addSystemChatToChatQue(e)
            }
            showMovieAlert() {
                this.isVisibleMovieAlert = !0
            }
            hideMovieAlert() {
                this.isVisibleMovieAlert = !1
            }
            changeChatPositionWithFullscreenMode(e) {
                oi.Ay.setChatPositionWithFullscreenMode(e),
                    this.chatPositionWithFullscreenMode = e
            }
            setVideoContainer(e) {
                this.videoContainer = e
            }
            setPlayingVideoBitrate(e) {
                this._playingVideobitrate = e
            }
            setPlayingVideoWidth(e) {
                this._playingVideoWidth = e
            }
            setPlayingVideoHeight(e) {
                this._playingVideoHeight = e
            }
            setIsVisibleRectangleAd(e) {
                this.isVisibleRectangleAd = e
            }
            addAndEditTelop(e, t, i, s, o) {
                return ls(this, null, function* () {
                    var r;
                    if (this.stores)
                        try {
                            if ("add" === e) {
                                const e = this.stores.appStore
                                    , i = "lowLatency" === (null == (r = e.currentPlayerInfo.level) ? void 0 : r.type) ? v.lT.LOW_LATENCY : v.lT.NORMAL;
                                yield (0,
                                    ss.Bt)(this.movieStore.id, t, i, {
                                        linkUrl: s,
                                        messagedAt: e.currentPlayerInfo.programDateTime
                                    })
                            } else {
                                const e = (0,
                                    A.seconds)(o)
                                    , r = Ki.A.createModel(this.stores.moviePageStore.movieStore.startedAt).add(e, "SECOND").toISOString();
                                yield (0,
                                    ss.v1)(this.movieStore.id, i, {
                                        message: t,
                                        linkUrl: s,
                                        messagedAt: r
                                    })
                            }
                        } catch (e) { }
                })
            }
            get canRenderAll() {
                if (!this.stores)
                    return !1;
                const e = this.stores.userStore;
                return !(!e.isCompletedInitialFetch || e.isLogined && !this.movieStore.isCompletedFetchMovieDetail)
            }
            get isNewPlayer() {
                return "hlsjs" === this.currentPlayerType
            }
            get isTheaterModeAvailable() {
                if (!this.stores)
                    return !0;
                const e = this.movieStore
                    , t = this.stores.userStore;
                return !(t.isCompletedInitialFetch && (!t.isLogined || e.isCompletedFetchMovieDetail) && (e.notFound || e.isFinishedMedia || e.isComingUp || e.isArchive && !e.isArchivePlayable || e.isMemberOnly && !e.isMemberOnlyPlayable))
            }
            get isEnabledYell() {
                return this.movieStore.monetizeStatus === v.Dv.ON && this.movieStore.enabledYell
            }
            get isYellTickerVisible() {
                return !!this.stores && !(!this.stores.appStore.isCompletedInitialRender || this.isTheaterMode || this.movieStore.isUploadedMovie || !this.yellStore.supporter)
            }
            get isChasingPlaybackInDvr() {
                return !(!this.movieStore.isDvr || !this.movieStore.isLiveStreaming) && !1 === this.isLatest
            }
            get isEnabledAd() {
                if (!this.stores)
                    return !1;
                if (C.ie())
                    return !1;
                if (C.ipad())
                    return !1;
                if (this.stores.userStore.isCompletedInitialFetch) {
                    if (this._userSetting.adHidden)
                        return !1;
                    if (this.movieStore.isComingUp)
                        return !1;
                    if (!this.movieStore.ad || !this.movieStore.ad.webStream)
                        return !1;
                    if (!this.stores.userStore.isLogined || this.movieStore.isCompletedFetchMovieDetail)
                        return (!this.movieStore.hasSubscribed || !this._userSetting.subsAdHidden) && this.movieStore.monetizeStatus === v.Dv.ON && this.movieStore.enabledAd && this.isPlayableAd
                }
            }
            get needsToPostViewLog() {
                if (!this.stores || !this.movieStore.stores)
                    return !1;
                if (this.hasAlreadyPostedViewLog)
                    return !1;
                const e = this.stores.userStore
                    , t = this.stores.moviePageStore.movieStore;
                return !(t.isArchive && !t.isArchivePlayable) && !t.exceedsDeviceLimit && (!(!e.isCompletedInitialFetch || e.isLogined) || t.isCompletedFetchMovieDetail)
            }
            get enabledClickCaptureButton() {
                return !(!this.isLatest || this.isPaused || this.isEnabledAd && !1 === this.isNonLinearAd || this.movieStore.isTrailer)
            }
            get _movieDescription() {
                const e = this.movieStore;
                return [e.introduction.replace(/\r?\n/g, ""), `${e.game.title}・${e.channel.name}`].filter(e => e).join(" ")
            }
            get capturesOfMovie() {
                const e = this.chatStore.blacklist.map(e => e.id);
                return this._capturesOfMovie.filter(t => !e.includes(t.captureChannel.userId || ""))
            }
            get movieForExtension() {
                var e, t;
                return {
                    id: this.movieStore.id,
                    user: {
                        id: this.movieStore.channel.id,
                        name: this.movieStore.channel.name,
                        iconImageUrl: this.movieStore.channel.iconImageUrl,
                        coverImageUrl: this.movieStore.channel.coverImageUrl
                    },
                    game: {
                        id: this.movieStore.game.id,
                        title: this.movieStore.game.title,
                        titleImageUrl: this.movieStore.game.titleImageUrl,
                        coverImageUrl: this.movieStore.game.coverImageUrl
                    },
                    onairStatus: "number" == typeof this.movieStore.onairStatus ? ["COMING_UP", "LIVE", "ARCHIVE"][this.movieStore.onairStatus] : "UPLOAD",
                    title: this.movieStore.title,
                    introduction: this.movieStore.introduction,
                    thumbnailUrl: this.movieStore.thumbnailUrl,
                    tags: this.movieStore.tags.concat(),
                    casts: this.movieStore.casts.map(e => ({
                        id: e.userId || e.userIdentifyId || "",
                        name: e.userName || "",
                        iconImageUrl: e.userIconImageUrl || "",
                        coverImageUrl: e.userCoverImageUrl || ""
                    })),
                    publicType: null == (e = this.movieStore.publicType) ? void 0 : e.toUpperCase(),
                    chatPublicType: null == (t = this.movieStore.chatPublicType) ? void 0 : t.toUpperCase(),
                    deviceType: ["WEB", "IOS", "ANDROID"][this.movieStore.deviceType],
                    liveViews: this.movieStore.liveViews,
                    totalViews: this.movieStore.totalViews,
                    isLowLatencyEnabled: this.movieStore.isLowLatency || !1,
                    isDvrEnabled: this.movieStore.isDvr || !1,
                    isCaptureEnabled: this.movieStore.isCaputure || !1,
                    isFixedPhraseEnabled: this.movieStore.isFixedPhrase || !1,
                    isAdEnabled: this.movieStore.enabledAd,
                    isYellEnabled: this.movieStore.enabledYell,
                    isOwnerYellEnabled: this.movieStore.enabledOwnerYell || !1,
                    isCastYellEnabled: this.movieStore.enabledCastYell || !1,
                    totalYells: this.movieStore.totalYells,
                    publishedAt: this.movieStore.publishedAt || "",
                    startedAt: this.movieStore.startedAt || "",
                    endedAt: this.movieStore.endedAt || "",
                    willStartAt: this.movieStore.willStartAt || "",
                    willEndAt: this.movieStore.willEndAt || "",
                    startTime: this.movieStore.startTime || 0,
                    playTime: this.movieStore.playTime || 0
                }
            }
            get isVisibleAdSystemMessage() {
                if (!this.stores)
                    return !1;
                const e = this.stores.userStore
                    , t = this.stores.moviePageStore.movieStore;
                return !(e.isLogined && !t.isCompletedFetchMovieDetail || e.user.isPremium || t.hasSubscribed || !this.isEnabledAd || this.isManualPaused || this.isNonLinearAd)
            }
            get updateSystemMessageType() {
                if (!this.stores)
                    return !1;
                const e = this.stores.appStore
                    , t = this.stores.userStore
                    , i = this.stores.moviePageStore.movieStore;
                return i.isCompletedFetchMovie && e.isCompletedInitialRender && t.isCompletedInitialFetch ? t.user.isPremium || i.isArchivePlayable ? this.isVisibleAdSystemMessage ? "adsense" : "none" : !t.isLogined && i.isArchive && i.isMemberTrial ? "login" : i.isPpvEnabled || i.isSubsEnabled ? "none" : i.isSpecial ? "special" : i.isArchive ? t.isLogined ? i.isArchivePlayable ? this.isVisibleAdSystemMessage ? (0,
                    C.ios)() && e.isMobile ? "none" : "adsense" : "none" : i.isCompletedFetchMovieDetail ? "archive" : "none" : "login" : this.isVisibleAdSystemMessage ? (0,
                        C.ios)() && e.isMobile ? "none" : "adsense" : "none" : "none"
            }
            get isChatVisible() {
                return !!this.stores && !(!this.stores.appStore.isCompletedInitialRender || this.isSplitViewPopoutPage || this.isTheaterMode && (this.isEnabledAd && !this.isNonLinearAd || !this.chatStore.isChatVisible))
            }
            get isCeroZ() {
                if (!this.stores)
                    return !1;
                const e = this.stores.appStore
                    , t = this.stores.moviePageStore.movieStore;
                if (!e.isCompletedInitialRender)
                    return !1;
                if (t.isCeroZ) {
                    if (this.enabledCeroZ)
                        return !1;
                    const e = (0,
                        p.Ay)().getAgeVerification();
                    if (!e.verifiedAt)
                        return !0;
                    const t = new Date
                        , i = new Date(e.verifiedAt);
                    if (t.getTime() - i.getTime() > 864e5)
                        return !0
                }
                return !1
            }
            get shouldVisibleRectangleAd() {
                return !(!this.stores || !this.stores.userStore.isCompletedInitialFetch || this.stores.userStore.isLogined && !this.movieStore.isCompletedFetchMovieDetail || this.movieStore.isComingUp || !this.movieStore.enabledAd || this.stores.userStore.user.isPremium || this.movieStore.isOwner || this.movieStore.hasSubscribed || this.stores.moviePageStore.isTheaterMode)
            }
            refetchAllArchiveChats(e = {}) {
                const t = this.movieStore
                    , i = this.chatStore;
                if (!t.isArchive && !i.isReproducingChatInDvr)
                    return;
                let s = "";
                if (e.dateStr)
                    s = e.dateStr;
                else if (t.startedAt) {
                    const e = Ki.A.createModel(t.startedAt).add(this.currentTime || 0, "SECOND").toDate();
                    s = (0,
                        Q.wO)(e)
                }
                if (!s)
                    return;
                this._loadArchiveChatTimeoutID && (0,
                    y.A)().clearTimeout(this._loadArchiveChatTimeoutID);
                const o = e.offset || 0;
                this._loadArchiveChatTimeoutID = (0,
                    y.A)().setTimeout(() => ls(this, null, function* () {
                        this._loadArchiveChatTimeoutID = null,
                            yield i.fetchChatAndReset(t.id, {
                                toCreatedAt: s
                            }),
                            i.fetchArchiveChatsAndUpdate(t.id, {
                                fromCreatedAt: s,
                                isIncludingSystemMessage: !0
                            })
                    }), o)
            }
            isDisabledChat(e) {
                if (this.stores && this.stores.userStore.user.id === e.userId)
                    return !1;
                const t = t => t.id === e.userId;
                return !(!this.movieStore.blacklist.some(t) && !this.chatStore.blacklist.some(t) && !e.isMuted && !e.isBlacklist && (!this.chatSettingStore.muteFreshUser || !e.isFresh) && (!this.chatSettingStore.muteWarnedUser || !e.isWarned) && !(this.chatSettingStore.muteForbiddenWord && e.message && e.hasBannedWord) && (!this.chatSettingStore.muteUnAuthenticatedUser || e.isAuthenticated))
            }
            capture(e) {
                return ls(this, null, function* () {
                    const t = (0,
                        qi.An)({
                            movieId: this.movieStore.id,
                            playerTime: e.playerTime
                        });
                    try {
                        const e = yield t;
                        if (30 === e.status)
                            return void (0,
                                o.h5)(() => {
                                    this.showCapturePremiumAppealDialog()
                                }
                                );
                        if (0 !== e.status)
                            return void this.showErrorMessage(e.message);
                        if (e.data) {
                            const t = e.data.items[0].capturePoint
                                , i = e.data.items[0].liveThumbnail
                                , s = e.data.items[0].id;
                            if (!s || !t)
                                return;
                            (0,
                                o.h5)(() => {
                                    this.isCapturePopupVisible = !0,
                                        this.isPolling = !0,
                                        this.captureResponse = {
                                            captureId: s,
                                            captureThumbnail: i || ""
                                        }
                                }
                                ),
                                this._confirmCapture({
                                    movieId: this.movieStore.movieId,
                                    capturePoint: t
                                }),
                                this._captureTimeoutTimer = (0,
                                    y.A)().setTimeout(() => {
                                        (0,
                                            o.h5)(() => {
                                                this._isCaptureTimeout = !0,
                                                    this.isPolling = !1,
                                                    this.hideCapturePopup()
                                            }
                                            ),
                                            this.stores && this.showErrorMessage(this.stores.langStore.capture.failed)
                                    }
                                        , v.PY)
                        }
                    } catch (e) {
                        console.error(e),
                            "unapproved user" === e.response.data.message ? this.stores && this.showErrorMessage(this.stores.langStore.user.unauthorizedMail) : this.stores && this.showErrorMessage(this.stores.langStore.capture.failed)
                    }
                })
            }
            showCapturePopup() {
                this.isCapturePopupVisible = !0
            }
            hideCapturePopup() {
                this.isCapturePopupVisible = !1
            }
            resetPostedViewLog() {
                this.hasAlreadyPostedViewLog = !1
            }
            getHeadData() {
                const e = this.movieStore
                    , t = ["OPENREC", e.game.title, "動画"].filter(e => e)
                    , i = e.thumbnailUrl === Qe.A.IMAGES.MOVIE && e.channel.coverImageUrl || e.thumbnailUrl;
                return {
                    title: e.title,
                    description: this._movieDescription,
                    keywords: t,
                    path: `${e.isUploadedMovie ? Qe.A.MOVIE : Qe.A.LIVE}/${e.id}`,
                    image: i,
                    transfer: {
                        type: e.isUploadedMovie ? "movie" : "live",
                        id: e.movieId,
                        identifyId: e.id
                    }
                }
            }
            getGATrackginData() {
                const e = this.movieStore;
                return e.id && e.channel.gaTrackingId ? {
                    gaTrackingId: e.channel.gaTrackingId
                } : null
            }
            showErrorMessage(e = "", t) {
                x.A.addMessage({
                    variant: t || "danger",
                    text: e
                })
            }
            seekPlayPosition(e) {
                const t = Math.max(e, 0);
                if (!this.stores)
                    return;
                if (!this.movieStore.startedAt)
                    return;
                if (this.movieStore.isLiveStreaming) {
                    if (!this.movieStore.isDvr)
                        return;
                    this.stores.userStore.isLogined || pt.A.show(Mt.A.bifurcateSignupAndSigninDialogId)
                }
                this.movieStore.isLiveStreaming && !this.isCompletedPrapareDvr && (this.setIsCompletedPrepareDvr(!0),
                    this.chatStore.setReproducingChatsInDvr(!0)),
                    this.movieStore.updateChatPostedPosition(t);
                const i = Ki.A.createModel(this.movieStore.startedAt).add(t, "SECOND").toDate()
                    , s = (0,
                        Q.wO)(i);
                this.refetchAllArchiveChats({
                    dateStr: s,
                    offset: v.g6
                })
            }
            setIsCompletedPrepareDvr(e) {
                this.isCompletedPrapareDvr = e
            }
            updateUserSetting(e, t) {
                this._userSetting.adHidden = e,
                    this._userSetting.subsAdHidden = t
            }
            startAdTimer() {
                this.stores && !this.stores.userStore.user.isPremium && this.movieStore.isArchive && "number" != typeof this._viewsLimitAdIntervalId && (this._isExecutePauseAdTimer = !1,
                    this._startedDate = Date.now() - this._elapsedTime,
                    this._viewsLimitAdIntervalId = (0,
                        y.A)().setTimeout(() => {
                            this.startAd("midroll"),
                                this.startRectangleAd(),
                                this._elapsedTime = 0,
                                this._startedDate = void 0,
                                this._viewsLimitAdIntervalId = void 0,
                                this.startAdTimer()
                        }
                            , this._adIntervalTime - this._elapsedTime))
            }
            pauseAdTimer() {
                this.isPaused && void 0 !== this._viewsLimitAdIntervalId && (this._isExecutePauseAdTimer || this._startedDate && (this._elapsedTime = Date.now() - this._startedDate,
                    (0,
                        y.A)().clearTimeout(this._viewsLimitAdIntervalId),
                    this._viewsLimitAdIntervalId = void 0,
                    this._isExecutePauseAdTimer = !0))
            }
            startRectangleAdTimer() {
                "number" != typeof this._rectangleAdIntervalId && (this.startRectangleAd(),
                    this._rectangleAdIntervalId = (0,
                        y.A)().setTimeout(() => {
                            this._rectangleAdIntervalId = void 0,
                                this.startRectangleAdTimer()
                        }
                            , v.CV))
            }
            startMidRolltimer() {
                this.movieStore.channel.isOfficial || this.movieStore.isLiveStreaming && "number" != typeof this._midRollTimeoutId && (this._remainingMidRollTime < 0 || (this._midRollStartTime = Date.now(),
                    this._midRollTimeoutId = (0,
                        y.A)().setTimeout(() => {
                            this.startAd("midroll"),
                                this._remainingMidRollTime = -1
                        }
                            , this._remainingMidRollTime)))
            }
            pauseMidRollTimer() {
                if (!this.isPaused)
                    return;
                if ("number" != typeof this._midRollTimeoutId)
                    return;
                if (this._remainingMidRollTime < 0)
                    return;
                (0,
                    y.A)().clearTimeout(this._midRollTimeoutId),
                    this._midRollTimeoutId = void 0;
                const e = Date.now() - this._midRollStartTime;
                this._remainingMidRollTime = this._remainingMidRollTime - e
            }
            _confirmCapture(e) {
                return ls(this, null, function* () {
                    try {
                        const t = yield (0,
                            Gi.HK)(e);
                        "capturing" !== t.captureStatus || this._isCaptureTimeout ? "done" === t.captureStatus ? ((0,
                            o.h5)(() => {
                                this.isPolling = !1,
                                    this.showCapturePopup()
                            }
                            ),
                            this._resetCaptureTimer()) : (this.stores && this.showErrorMessage(this.stores.langStore.capture.failed),
                                (0,
                                    o.h5)(() => {
                                        this.isPolling = !1
                                    }
                                    ),
                                this.hideCapturePopup(),
                                this._resetCaptureTimer()) : this._capturePollingTimer = (0,
                                    y.A)().setTimeout(() => {
                                        this._confirmCapture({
                                            movieId: e.movieId,
                                            capturePoint: e.capturePoint
                                        })
                                    }
                                        , v.zu)
                    } catch (e) {
                        console.error(e),
                            this.stores && this.showErrorMessage(this.stores.langStore.capture.failed),
                            this.hideCapturePopup(),
                            this._resetCaptureTimer()
                    }
                })
            }
            _willLoadForChat(e) {
                return ls(this, null, function* () {
                    const e = this.movieStore.id;
                    if (this.movieStore.isUserChatReceivable) {
                        if (this.movieStore.isLiveStreaming || this.movieStore.isComingUp)
                            try {
                                yield this.chatStore.fetchChatAndReset(e)
                            } catch (e) { }
                        this.chatModeratorStore.fetchChatsAndUpdate(e, {
                            isReset: !0,
                            cacheBuster: this.isStaff() ? (new Date).getTime() : void 0,
                            isLatest: !0
                        }),
                            this.yellStore.fetchTopSupporterAndUpdate({
                                movieId: e
                            })
                    } else
                        this.movieStore.isMemberOnly && !this.movieStore.isMemberOnlyChatPostable && this.chatStore.addSystemChatToChatQue(this.chatStore.createSystemChat("memberOnly"));
                    this._isUllSelectable() && this.chatStore.addSystemChatToChatQue(this.chatStore.createSystemChat("ullSelectable")),
                        this.stores && this.stores.appStore.currentPlayerInfo.level && this.chatStore.addSystemChatForLevelToChatQue(["playUll", "highBitrate"], this.stores.appStore.currentPlayerInfo.level),
                        this.chatStore.startIntervalToPopChatQue(),
                        this.stores && this.movieStore.channel.id === this.stores.userStore.user.id && this.splitViewStore.startIntervalToPopNotificationQue(),
                        this.stores && this.movieStore.channel.id !== this.stores.userStore.user.id && !this.movieStore.channel.isModerating && this.chatStore.fetchFixedPhrases(e)
                })
            }
            _willLoadForComment(e) {
                return ls(this, null, function* () {
                    if (this.movieStore.isUploadedMovie && (!this.movieStore.isMemberOnly || this.movieStore.isMemberOnlyCommentPostable || this.movieStore.isUserChatPublic)) {
                        try {
                            yield this.commentStore.fetchLatestCommentAndReset(this.movieStore.id)
                        } catch (e) { }
                        this.yellStore.fetchTopSupporterAndUpdate({
                            movieId: this.movieStore.id
                        })
                    }
                })
            }
            _fetchUsersMe() {
                return ls(this, null, function* () {
                    var e, t;
                    try {
                        return null == (t = null == (e = (yield (0,
                            ji.md)()).data) ? void 0 : e.items) ? void 0 : t[0]
                    } catch (e) {
                        return
                    }
                })
            }
            _createSessionId() {
                const e = (0,
                    a.A)();
                this._sessionId = e
            }
            _fetchMembershipInterval() {
                this._fetchMembershipIntervalId = (0,
                    y.A)().setTimeout(() => ls(this, null, function* () {
                        this.movieStore.memberShip || this._membershipFetchCount >= 10 || (yield this.movieStore.fetchMembershipsAndUpdate(),
                            this._membershipFetchCount++,
                            this._fetchMembershipInterval())
                    }), 1e3)
            }
            _postViewLog() {
                return ls(this, null, function* () {
                    var e;
                    if (!this.needsToPostViewLog)
                        return;
                    this.hasAlreadyPostedViewLog = !0;
                    const t = this.movieStore
                        , i = this.playlistStore;
                    try {
                        const r = yield (s = t.id,
                            o = {
                                chapterKey: null == (e = t.chapterOnFirstPlay) ? void 0 : e.id,
                                playListKey: i.playlistId
                            },
                            (0,
                                zi.A)("POST", `/movies/${s}/log`, {
                                    query: void 0,
                                    body: void 0,
                                    form: {
                                        playListKey: o.playListKey,
                                        chapterKey: o.chapterKey
                                    }
                                }));
                        r.errorMessage && console.error(r.errorMessage)
                    } catch (e) {
                        console.error(e)
                    }
                    var s, o
                })
            }
            _startPreRoll() {
                !this.movieStore.channel.isOfficial && this.movieStore.isLiveStreaming || this.startAd("preroll")
            }
            _disposeInitialFetchCallback() { }
            _disposeMovieDetailFetchCallback() { }
            _disposeLoginCallback() { }
            _disposeSubscribedCallback() { }
            _disposePpvTicketPurchaseCallback() { }
            _disposeTheaterModeAvailableReaction() { }
            _disposeLevelUpdateToChildWindow() { }
            _disposeDVRChangedReaction() { }
            _disposeViewLogPostAutorun() { }
            _disposeLoadMovieWhen() { }
            _disposeReceivedPdtWhen() { }
            _disposeReceivedIsPlayableWhen() { }
            _disposeMediaEndCallback() { }
            _toLangMessageFromSocketChat(e) {
                switch (this.stores ? this.stores.appStore.getLang() : "ja") {
                    case "en":
                        return e.message_en;
                    case "ko":
                        return e.message_ko;
                    case "zh":
                        return e.message_zh;
                    default:
                        return e.message
                }
            }
            _resetCaptureTimer() {
                this._capturePollingTimer && ((0,
                    y.A)().clearTimeout(this._capturePollingTimer),
                    delete this._capturePollingTimer),
                    this._captureTimeoutTimer && ((0,
                        y.A)().clearTimeout(this._captureTimeoutTimer),
                        delete this._captureTimeoutTimer),
                    this._isCaptureTimeout = !1
            }
            _handleFullScreenChange() {
                const e = !!f.getFullscreenElement();
                this.isFullscreenMode = e,
                    e ? (this.isPrevTheaterMode = this.isTheaterMode,
                        this.isPrevTheaterMode || this.enterTheaterMode({
                            canceledLocalStorage: !0
                        })) : this.isPrevTheaterMode || this.exitTheaterMode({
                            canceledLocalStorage: !0
                        })
            }
        }
        ns([o.sH.ref], ds.prototype, "stores", 2),
            ns([o.sH], ds.prototype, "currentPlayerType", 2),
            ns([o.sH], ds.prototype, "playerTypeUpdateIntervalId", 2),
            ns([o.sH], ds.prototype, "hasAlreadyPostedViewLog", 2),
            ns([o.sH], ds.prototype, "isCompletedFetchRelatedMovies", 2),
            ns([o.sH], ds.prototype, "videoWidth", 2),
            ns([o.sH], ds.prototype, "videoHeight", 2),
            ns([o.sH], ds.prototype, "chatArticleWidth", 2),
            ns([o.sH], ds.prototype, "relatedMovieStreams", 2),
            ns([o.sH], ds.prototype, "isFullscreenMode", 2),
            ns([o.sH], ds.prototype, "chatPopoutWindow", 2),
            ns([o.sH], ds.prototype, "exntensionPopoutWindow", 2),
            ns([o.sH], ds.prototype, "pdt", 2),
            ns([o.sH], ds.prototype, "pdtPlaybackTime", 2),
            ns([o.sH], ds.prototype, "isCompletedPrapareDvr", 2),
            ns([o.sH], ds.prototype, "isCapturePremiumDialogVisible", 2),
            ns([o.sH], ds.prototype, "isCapturePopupVisible", 2),
            ns([o.sH], ds.prototype, "isPolling", 2),
            ns([o.sH], ds.prototype, "captureResponse", 2),
            ns([o.sH], ds.prototype, "hasPpvPermission", 2),
            ns([o.sH], ds.prototype, "videoContainer", 2),
            ns([o.sH], ds.prototype, "isVisibleMovieAlert", 2),
            ns([o.sH], ds.prototype, "chatPositionWithFullscreenMode", 2),
            ns([o.sH], ds.prototype, "refreshTime", 2),
            ns([o.sH], ds.prototype, "isVisibleRectangleAd", 2),
            ns([o.sH], ds.prototype, "mridx", 2),
            ns([o.sH], ds.prototype, "_capturesOfMovie", 2),
            ns([o.sH], ds.prototype, "_userSetting", 2),
            ns([s.A], ds.prototype, "activate", 1),
            ns([s.A], ds.prototype, "inactivate", 1),
            ns([s.A], ds.prototype, "enterScrollTop", 1),
            ns([o.XI], ds.prototype, "willLoadOnServer", 1),
            ns([o.XI], ds.prototype, "willLoad", 1),
            ns([o.XI], ds.prototype, "willUnload", 1),
            ns([o.XI.bound], ds.prototype, "setVideoWrapperSizeSync", 1),
            ns([o.XI.bound], ds.prototype, "syncVideoWrapperSize", 1),
            ns([o.XI.bound], ds.prototype, "enableCeroZ", 1),
            ns([o.XI.bound], ds.prototype, "handleSocketMessage", 1),
            ns([o.XI], ds.prototype, "reset", 1),
            ns([o.XI.bound], ds.prototype, "fetchCapturesAndUpdate", 1),
            ns([o.XI], ds.prototype, "finishLiveStreaming", 1),
            ns([o.XI], ds.prototype, "fetchFinishedRelatedMovieAndUpdate", 1),
            ns([o.XI.bound], ds.prototype, "handleCurrentTimeChange", 1),
            ns([o.XI.bound], ds.prototype, "handleSeekTimeChange", 1),
            ns([o.XI.bound], ds.prototype, "handlePdtChange", 1),
            ns([o.XI.bound], ds.prototype, "handleSeekPdtChange", 1),
            ns([o.XI.bound], ds.prototype, "updateWhetherToSeekPlayer", 1),
            ns([o.XI.bound], ds.prototype, "handlePaused", 1),
            ns([o.XI.bound], ds.prototype, "handleManualPaused", 1),
            ns([o.XI.bound], ds.prototype, "handleVolumeChange", 1),
            ns([o.XI.bound], ds.prototype, "setLatest", 1),
            ns([o.XI.bound], ds.prototype, "setNonLinearAd", 1),
            ns([o.XI.bound], ds.prototype, "enterTheaterMode", 1),
            ns([o.XI.bound], ds.prototype, "exitTheaterMode", 1),
            ns([o.XI.bound], ds.prototype, "toggleTheaterMode", 1),
            ns([o.XI.bound], ds.prototype, "changeChatModeratorForTheaterMode", 1),
            ns([o.XI.bound], ds.prototype, "setChatPopoutWindow", 1),
            ns([o.XI.bound], ds.prototype, "closeChatPopoutWindow", 1),
            ns([o.XI.bound], ds.prototype, "setExtensionPopoutWindow", 1),
            ns([o.XI.bound], ds.prototype, "closeExtensionPopoutWindow", 1),
            ns([o.XI.bound], ds.prototype, "startAd", 1),
            ns([o.XI.bound], ds.prototype, "stopAd", 1),
            ns([o.XI.bound], ds.prototype, "startRectangleAd", 1),
            ns([o.XI.bound], ds.prototype, "updatePlayerType", 1),
            ns([o.XI.bound], ds.prototype, "updatePlayerRefresh", 1),
            ns([o.XI.bound], ds.prototype, "showCapturePremiumAppealDialog", 1),
            ns([o.XI.bound], ds.prototype, "hideCapturePremiumAppealDialog", 1),
            ns([s.A], ds.prototype, "handleRotateButtonClick", 1),
            ns([s.A], ds.prototype, "handleContinuousBufferStalledError", 1),
            ns([s.A], ds.prototype, "showMovieAlert", 1),
            ns([s.A], ds.prototype, "hideMovieAlert", 1),
            ns([o.XI.bound], ds.prototype, "changeChatPositionWithFullscreenMode", 1),
            ns([o.XI.bound], ds.prototype, "setVideoContainer", 1),
            ns([s.A], ds.prototype, "setPlayingVideoBitrate", 1),
            ns([s.A], ds.prototype, "setPlayingVideoWidth", 1),
            ns([s.A], ds.prototype, "setPlayingVideoHeight", 1),
            ns([s.A], ds.prototype, "setIsVisibleRectangleAd", 1),
            ns([s.A], ds.prototype, "addAndEditTelop", 1),
            ns([o.EW], ds.prototype, "canRenderAll", 1),
            ns([o.EW], ds.prototype, "isNewPlayer", 1),
            ns([o.EW], ds.prototype, "isTheaterModeAvailable", 1),
            ns([o.EW], ds.prototype, "isEnabledYell", 1),
            ns([o.EW], ds.prototype, "isYellTickerVisible", 1),
            ns([o.EW], ds.prototype, "isChasingPlaybackInDvr", 1),
            ns([o.EW], ds.prototype, "isEnabledAd", 1),
            ns([o.EW], ds.prototype, "needsToPostViewLog", 1),
            ns([o.EW], ds.prototype, "enabledClickCaptureButton", 1),
            ns([o.EW], ds.prototype, "_movieDescription", 1),
            ns([o.EW], ds.prototype, "capturesOfMovie", 1),
            ns([o.EW], ds.prototype, "movieForExtension", 1),
            ns([o.EW], ds.prototype, "isVisibleAdSystemMessage", 1),
            ns([o.EW], ds.prototype, "updateSystemMessageType", 1),
            ns([o.EW], ds.prototype, "isChatVisible", 1),
            ns([o.EW], ds.prototype, "isCeroZ", 1),
            ns([o.EW], ds.prototype, "shouldVisibleRectangleAd", 1),
            ns([o.XI.bound], ds.prototype, "capture", 1),
            ns([o.XI.bound], ds.prototype, "showCapturePopup", 1),
            ns([o.XI.bound], ds.prototype, "hideCapturePopup", 1),
            ns([o.XI.bound], ds.prototype, "resetPostedViewLog", 1),
            ns([s.A], ds.prototype, "showErrorMessage", 1),
            ns([s.A], ds.prototype, "seekPlayPosition", 1),
            ns([s.A], ds.prototype, "setIsCompletedPrepareDvr", 1),
            ns([s.A], ds.prototype, "updateUserSetting", 1),
            ns([o.XI.bound], ds.prototype, "startRectangleAdTimer", 1),
            ns([o.XI.bound], ds.prototype, "_confirmCapture", 1),
            ns([o.XI], ds.prototype, "_willLoadForChat", 1),
            ns([o.XI], ds.prototype, "_willLoadForComment", 1),
            ns([o.XI.bound], ds.prototype, "_fetchUsersMe", 1),
            ns([s.A], ds.prototype, "_createSessionId", 1),
            ns([o.XI.bound], ds.prototype, "_postViewLog", 1),
            ns([o.XI.bound], ds.prototype, "_handleFullScreenChange", 1)
    }
    ,
    64780: (e, t, i) => {
        "use strict";
        i.d(t, {
            Ay: () => U,
            cN: () => k,
            gb: () => L
        });
        var s = i(5205)
            , o = i(31370)
            , r = i(96540)
            , a = i(92568)
            , n = i(64349)
            , l = i(7750)
            , d = i(47268)
            , h = i(17306)
            , u = i(61889)
            , c = i(29463)
            , p = i(83279)
            , m = i(11456)
            , y = i(20656)
            , g = i(3644)
            , v = i(21928)
            , S = i(88316)
            , b = Object.defineProperty
            , f = Object.defineProperties
            , C = Object.getOwnPropertyDescriptor
            , I = Object.getOwnPropertyDescriptors
            , A = Object.getOwnPropertySymbols
            , _ = Object.prototype.hasOwnProperty
            , P = Object.prototype.propertyIsEnumerable
            , M = (e, t, i) => t in e ? b(e, t, {
                enumerable: !0,
                configurable: !0,
                writable: !0,
                value: i
            }) : e[t] = i
            , w = (e, t) => {
                for (var i in t || (t = {}))
                    _.call(t, i) && M(e, i, t[i]);
                if (A)
                    for (var i of A(t))
                        P.call(t, i) && M(e, i, t[i]);
                return e
            }
            , T = (e, t) => f(e, I(t))
            , E = (e, t, i, s) => {
                for (var o, r = s > 1 ? void 0 : s ? C(t, i) : t, a = e.length - 1; a >= 0; a--)
                    (o = e[a]) && (r = (s ? o(t, i, r) : o(r)) || r);
                return s && r && b(t, i, r),
                    r
            }
            ;
        let U = class extends r.Component {
            componentWillUnmount() {
                delete this._userNameEl
            }
            render() {
                var e, t, i, s, o;
                const a = this.props
                    , h = a.langStore
                    , u = a.isDeleted ? "メッセージが削除されました" : a.message
                    , c = a.userStore
                    , p = a.moviePageStore
                    , y = p.movieStore
                    , g = p.chatSettingStore
                    , v = y.isLeague || !!y.enabledCastYell
                    , S = a.isDisabled
                    , b = !!c.user.id && c.user.id === a.userId || !!c.user.recxuserId && c.user.recxuserId === a.recxuserId;
                let f;
                if ("log" === a.type) {
                    const l = a.reactionProduct && !a.isDeleted && r.createElement(J, {
                        reaction: {
                            id: a.reactionProduct.id,
                            label: a.reactionProduct.label,
                            reactionFile: a.reactionProduct.reactionFile,
                            count: (null == (e = a.reaction) ? void 0 : e.count) || 0,
                            isReaction: a.isReactioned || !1
                        },
                        size: "l",
                        onClick: a.onReactionClick
                    });
                    return r.createElement(O, {
                        className: a.isDeleted ? "opacity20" : "",
                        onClick: a.onClick
                    }, r.createElement(R, null, r.createElement(W, null, r.createElement(d.Ay, T(w({}, a), {
                        isSubsBadgeHidden: b ? g.isSubsBadgeHidden : a.isSubsBadgeHidden,
                        isSubsDurationHidden: b ? g.isSubsDurationHidden : a.isSubsDurationHidden,
                        onNameClick: this._handleNameClick,
                        innerRef: e => {
                            this._userNameEl = e
                        }
                        ,
                        isLightModeUserColor: !0,
                        displayOfficialIcon: !0
                    }))), !!a.createdAt && r.createElement(F, null, m.elapsedTime(h, a.createdAt))), r.createElement(N, null, !!u && !S && r.createElement(V, null, u), r.createElement(Y, null, r.createElement(G, null, r.createElement(K, {
                        src: a.imageUrl || "",
                        alt: `${a.quantity || 0}${h.yell.yell}`
                    }), !1), r.createElement(Q, null, a.toUser && v && r.createElement(z, null, r.createElement(q, null, "TO"), a.toUser && v && r.createElement($, {
                        src: a.toUser.userIconImageUrl,
                        alt: a.toUser.userName
                    }), r.createElement(j, {
                        className: "text-ellipsis"
                    }, a.toUser.userName)), r.createElement(n.A, {
                        quantity: a.quantity || 0,
                        size: "medium"
                    }), (null == (t = a.yell) ? void 0 : t.subsShareCount) && (null == (s = null == (i = a.yell) ? void 0 : i.subsShareBadge) ? void 0 : s.imageUrl) && r.createElement(Z, {
                        shareCount: a.yell.subsShareCount,
                        badgeImage: null == (o = a.yell.subsShareBadge) ? void 0 : o.imageUrl,
                        size: "l"
                    })), l)))
                }
                return f = this._getRankColor(),
                    r.createElement(D, {
                        className: a.isDeleted ? "opacity20" : "",
                        onClick: a.onClick
                    }, r.createElement(X, null, !!a.rank && r.createElement(x, {
                        style: {
                            color: f
                        }
                    }, a.rank > 99 ? "-" : a.rank), r.createElement(l.A, {
                        src: a.userIconImageUrl,
                        size: "small"
                    })), r.createElement(B, null, r.createElement(W, null, r.createElement(d.Ay, T(w({}, a), {
                        isSubsBadgeHidden: b ? g.isSubsBadgeHidden : a.isSubsBadgeHidden,
                        isSubsDurationHidden: b ? g.isSubsDurationHidden : a.isSubsDurationHidden,
                        onNameClick: this._handleNameClick,
                        innerRef: e => {
                            this._userNameEl = e
                        }
                        ,
                        isLightModeUserColor: !0,
                        displayOfficialIcon: !0
                    }))), r.createElement(N, null, a.toUser && v && r.createElement(z, null, r.createElement(q, null, "TO"), r.createElement(j, {
                        className: "text-ellipsis"
                    }, a.toUser.userName)), !!a.quantity && r.createElement(n.A, {
                        quantity: a.quantity,
                        size: "small"
                    }), !!u && !S && r.createElement(V, null, u))))
            }
            _getRankColor() {
                switch (this.props.rank) {
                    case 1:
                        return u.lm.ORANGE;
                    case 2:
                        return u.lm.ORANGE_LIGHT;
                    case 3:
                        return u.lm.YELLOW;
                    default:
                        return ""
                }
            }
            _handleNameClick() {
                return e = this,
                    t = function* () {
                        const e = this.props.moviePageStore.movieStore
                            , t = this.props.moviePageStore.userCardStore
                            , i = this.props.userStore
                            , s = this.props.rootStore;
                        if (this._userNameEl && t.showUserCard(this.props, this._userNameEl, s),
                            !i.user.id || !this.props.userId)
                            return;
                        const o = e.channel.id === i.user.id;
                        t.fetchOptionalUserCard(this.props.userId, o)
                    }
                    ,
                    new Promise((i, s) => {
                        var o = e => {
                            try {
                                a(t.next(e))
                            } catch (e) {
                                s(e)
                            }
                        }
                            , r = e => {
                                try {
                                    a(t.throw(e))
                                } catch (e) {
                                    s(e)
                                }
                            }
                            , a = e => e.done ? i(e.value) : Promise.resolve(e.value).then(o, r);
                        a((t = t.apply(e, null)).next())
                    }
                    );
                var e, t
            }
        }
            ;
        E([s.A], U.prototype, "_handleNameClick", 1),
            U = E([(0,
                o.WQ)("langStore", "dialogStore", "moviePageStore", "userStore", "rootStore"), o.PA], U);
        const L = e => {
            var t, i, s, o, r, a, n, l, d, u, c, p, m, y, g, b, f, C, I, A, _, P, M, w, T, E, U, L, k, H, O, D, x, F, R, B, X, W, N, V;
            const Y = null == (i = null == (t = e.yell) ? void 0 : t.subsShare) ? void 0 : i.subsProduct.subsBadges
                , Q = e.reactionStatsList && e.reactionStatsList.length > 0 ? e.reactionStatsList[0] : void 0;
            return {
                id: (null == (s = e.yell) ? void 0 : s.id) || 0,
                chatId: e.chatId || 0,
                userId: (null == (o = e.user) ? void 0 : o.id) || "",
                userIconImageUrl: (null == (r = e.user) ? void 0 : r.iconImageUrl) || "",
                userCoverImageUrl: (null == (a = e.user) ? void 0 : a.coverImageUrl) || "",
                message: e.message || "",
                userName: (null == (n = e.user) ? void 0 : n.nickname) || "",
                userColor: (null == (l = e.chatSetting) ? void 0 : l.nameColor) || "",
                createdAt: e.createdAt || "",
                quantity: (null == (d = e.yell) ? void 0 : d.yells) || 0,
                imageUrl: (null == (u = e.yell) ? void 0 : u.imageUrl) || "",
                subsBadge: {
                    id: (null == (p = null == (c = e.badges) ? void 0 : c[0]) ? void 0 : p.id) || 0,
                    imageUrl: (null == (y = null == (m = e.badges) ? void 0 : m[0]) ? void 0 : y.imageUrl) || "",
                    months: (null == (f = null == (b = null == (g = e.badges) ? void 0 : g[0]) ? void 0 : b.subscription) ? void 0 : f.months) || 0,
                    tier: (null == (I = null == (C = e.badges) ? void 0 : C[0]) ? void 0 : I.subscription) && (null == (P = null == (_ = null == (A = e.badges) ? void 0 : A[0]) ? void 0 : _.subscription) ? void 0 : P.tier) || 0,
                    label: (null == (w = null == (M = e.badges) ? void 0 : M[0]) ? void 0 : w.label) || ""
                },
                isOfficial: !!(null == (T = e.user) ? void 0 : T.isOfficial),
                isOfficialHidden: !!(null == (E = e.chatSetting) ? void 0 : E.isOfficialHidden),
                isPremium: !!(null == (U = e.user) ? void 0 : U.isPremium),
                isPremiumHidden: !!(null == (L = e.chatSetting) ? void 0 : L.isPremiumHidden),
                isSubsBadgeHidden: !!(null == (k = e.chatSetting) ? void 0 : k.isSubsBadgeHidden),
                isSubsDurationHidden: !!(null == (H = e.chatSetting) ? void 0 : H.isSubsDurationHidden),
                isFresh: !!(null == (O = e.user) ? void 0 : O.isFresh),
                isWarned: !!(null == (D = e.user) ? void 0 : D.isWarned),
                isModerator: !!e.isModerating,
                isDeleted: !!e.isDeleted,
                isAuthenticated: !0,
                toUser: {
                    userId: (null == (x = e.toUser) ? void 0 : x.id) || "",
                    userName: (null == (F = e.toUser) ? void 0 : F.nickname) || "",
                    userIconImageUrl: (null == (R = e.toUser) ? void 0 : R.iconImageUrl) || h.A.IMAGES.PROFILE
                },
                yell: {
                    id: (null == (B = e.yell) ? void 0 : B.id) || 0,
                    imageUrl: (null == (X = e.yell) ? void 0 : X.imageUrl) || "",
                    quantity: (null == (W = e.yell) ? void 0 : W.yells) || 0,
                    subsShareCount: (null == (V = null == (N = e.yell) ? void 0 : N.subsShare) ? void 0 : V.shareCount) || void 0,
                    subsShareBadge: (null == Y ? void 0 : Y[0]) ? {
                        id: Y[0].id || 0,
                        imageUrl: Y[0].imageUrl || ""
                    } : void 0
                },
                reaction: {
                    id: (null == Q ? void 0 : Q.id) || "",
                    reactionFile: (null == Q ? void 0 : Q.reactionFile) || "",
                    count: (null == Q ? void 0 : Q.count) || 0,
                    label: null == Q ? void 0 : Q.label,
                    isReaction: !1
                },
                v8User: e.user ? S.A.createModel(e.user) : void 0,
                chatCommentAppearanceSetting: e.chatSetting ? v.A.createModel(e.chatSetting) : void 0,
                userInChannel: {
                    isModerating: e.isModerating || !1,
                    membershipCardUrl: e.membershipCardUrl || ""
                },
                type: "log"
            }
        }
            , k = e => {
                var t, i, s, o, r, a, n, l, d, u, c, p, m, y, g, b, f, C, I, A, _, P, M, w, T, E, U, L;
                return {
                    id: e.rank || 0,
                    chatId: 0,
                    userId: (null == (t = e.user) ? void 0 : t.id) || "",
                    userIconImageUrl: (null == (i = e.user) ? void 0 : i.iconImageUrl) || "",
                    userCoverImageUrl: (null == (s = e.user) ? void 0 : s.coverImageUrl) || "",
                    message: e.lastMessage || "",
                    userName: (null == (o = e.user) ? void 0 : o.nickname) || "",
                    userColor: (null == (r = e.chatSetting) ? void 0 : r.nameColor) || "",
                    rank: e.rank || 0,
                    quantity: e.totalYells || 0,
                    subsBadge: {
                        id: (null == (n = null == (a = e.badges) ? void 0 : a[0]) ? void 0 : n.id) || 0,
                        imageUrl: (null == (d = null == (l = e.badges) ? void 0 : l[0]) ? void 0 : d.imageUrl) || "",
                        months: (null == (p = null == (c = null == (u = e.badges) ? void 0 : u[0]) ? void 0 : c.subscription) ? void 0 : p.months) || 0,
                        tier: (null == (g = null == (y = null == (m = e.badges) ? void 0 : m[0]) ? void 0 : y.subscription) ? void 0 : g.tier) || 0,
                        label: (null == (f = null == (b = e.badges) ? void 0 : b[0]) ? void 0 : f.label) || ""
                    },
                    isOfficial: !!(null == (C = e.user) ? void 0 : C.isOfficial),
                    isOfficialHidden: !!(null == (I = e.chatSetting) ? void 0 : I.isOfficialHidden),
                    isPremium: !!(null == (A = e.user) ? void 0 : A.isPremium),
                    isPremiumHidden: !!(null == (_ = e.chatSetting) ? void 0 : _.isPremiumHidden),
                    isSubsBadgeHidden: !!(null == (P = e.chatSetting) ? void 0 : P.isSubsBadgeHidden),
                    isSubsDurationHidden: !!(null == (M = e.chatSetting) ? void 0 : M.isSubsDurationHidden),
                    isFresh: !!(null == (w = e.user) ? void 0 : w.isFresh),
                    isWarned: !!(null == (T = e.user) ? void 0 : T.isWarned),
                    isModerator: !!e.isModerating,
                    isDeleted: !!e.isDeleted,
                    isAuthenticated: !0,
                    toUser: {
                        userId: (null == (E = e.toUser) ? void 0 : E.id) || "",
                        userName: (null == (U = e.toUser) ? void 0 : U.nickname) || "",
                        userIconImageUrl: (null == (L = e.toUser) ? void 0 : L.iconImageUrl) || h.A.IMAGES.PROFILE
                    },
                    v8User: e.user ? S.A.createModel(e.user) : void 0,
                    chatCommentAppearanceSetting: e.chatSetting ? v.A.createModel(e.chatSetting) : void 0,
                    userInChannel: {
                        isModerating: e.isModerating || !1,
                        membershipCardUrl: e.membershipCardUrl || ""
                    },
                    type: "ranking"
                }
            }
            , H = c.HI({
                default: {
                    dark: u.lm.LIGHT_BASIC
                }
            })
            , O = a.Ay.div.withConfig({
                componentId: "sc-1qrdct1-0"
            })([""])
            , D = a.Ay.div.withConfig({
                componentId: "sc-1qrdct1-1"
            })(["display:flex;align-items:center;"])
            , x = a.Ay.div.withConfig({
                componentId: "sc-1qrdct1-2"
            })(["font-size:", "rem;font-weight:", ";margin-right:", "rem;width:2rem;text-align:right;color:", ";"], u.SG.M, u.Y.MEDIUM, u.jS.S, H)
            , F = a.Ay.div.withConfig({
                componentId: "sc-1qrdct1-3"
            })(["flex:none;margin-left:", "rem;color:", ";"], u.jS.S, H)
            , R = a.Ay.div.withConfig({
                componentId: "sc-1qrdct1-4"
            })(["display:flex;"])
            , B = a.Ay.div.withConfig({
                componentId: "sc-1qrdct1-5"
            })(["margin-left:", "rem;min-width:0;"], u.jS.XXS)
            , X = a.Ay.div.withConfig({
                componentId: "sc-1qrdct1-6"
            })(["flex-shrink:0;align-self:flex-start;display:flex;align-items:center;"])
            , W = a.Ay.div.withConfig({
                componentId: "sc-1qrdct1-7"
            })(["margin:0;min-width:0;width:100%;"])
            , N = a.Ay.div.withConfig({
                componentId: "sc-1qrdct1-8"
            })(["margin:", "rem 0 0;"], u.jS.XXS)
            , V = a.Ay.p.withConfig({
                componentId: "sc-1qrdct1-9"
            })(["font-weight:", ";line-height:1.54;word-break:break-all;word-break:break-word;margin:", "rem 0 0;color:", ";"], u.Y.MEDIUM, u.jS.XXS, (0,
                p.getTheme)("text-weak"))
            , Y = a.Ay.div.withConfig({
                componentId: "sc-1qrdct1-10"
            })(["display:flex;align-items:center;margin-top:", "rem;"], u.jS.XXXS)
            , Q = a.Ay.div.withConfig({
                componentId: "sc-1qrdct1-11"
            })(["min-width:0;margin-left:", "rem;display:flex;flex-direction:column;align-items:flex-start;"], u.jS.S)
            , q = a.Ay.span.withConfig({
                componentId: "sc-1qrdct1-12"
            })(["flex:none;display:inline-flex;align-items:center;border-radius:4px;font-size:", "rem;font-weight:", ";height:1.4rem;padding:0 ", "rem;color:", ";"], u.SG.XXXS, u.Y.MEDIUM, u.jS.XXXS, (0,
                p.getTheme)("text-weak"))
            , j = a.Ay.span.withConfig({
                componentId: "sc-1qrdct1-13"
            })(["font-weight:", ";margin-left:", "rem;color:", ";"], u.Y.MEDIUM, u.jS.XXXS, (0,
                p.getTheme)("text-base"))
            , z = a.Ay.div.withConfig({
                componentId: "sc-1qrdct1-14"
            })(["width:100%;display:flex;align-items:center;margin-bottom:", "rem;"], u.jS.XXS)
            , G = a.Ay.div.withConfig({
                componentId: "sc-1qrdct1-15"
            })(["flex:none;position:relative;"])
            , K = a.Ay.img.withConfig({
                componentId: "sc-1qrdct1-16"
            })(["width:6.4rem;height:6.4rem;"])
            , $ = a.Ay.img.withConfig({
                componentId: "sc-1qrdct1-17"
            })(["border-radius:50%;height:2rem;width:2rem;", ""], !1)
            , Z = (0,
                a.Ay)(g.c).withConfig({
                    componentId: "sc-1qrdct1-18"
                })(["margin-top:0.4rem;"])
            , J = (0,
                a.Ay)(y.H).withConfig({
                    componentId: "sc-1qrdct1-19"
                })(["margin-left:auto;padding-right:1rem;padding-left:1rem;border-radius:2rem;::before,::after{border-radius:2rem;}"])
    }
    ,
    43462: (e, t, i) => {
        "use strict";
        i.r(t),
            i.d(t, {
                scrollTo: () => a
            });
        var s = i(25595)
            , o = i(69083);
        const r = Math.floor(Number(1e3 / o.uE))
            , a = (e, t, i, o, a) => {
                const n = e.scrollTop
                    , l = t - n
                    , d = e => e * r / i
                    , h = e => n + l * o(e);
                let u = 1
                    , c = !1
                    , p = d(u)
                    , m = h(p)
                    , y = 0;
                const g = () => {
                    (0,
                        s.A)().clearInterval(v),
                        (0,
                            s.A)().cancelAnimationFrame(S),
                        c = !0,
                        a && a()
                }
                    , v = (0,
                        s.A)().setInterval(() => {
                            p = d(u),
                                m = h(p),
                                p > 1 ? c || (y !== t && (e.scrollTop = t),
                                    g()) : u++
                        }
                            , r);
                let S = 0;
                const b = () => {
                    S = (0,
                        s.A)().requestAnimationFrame(() => {
                            y !== m ? (e.scrollTop = m,
                                y = m,
                                b()) : b()
                        }
                        )
                }
                    ;
                return b(),
                    () => {
                        c || g()
                    }
            }
    }
    ,
    44820: (e, t, i) => {
        "use strict";
        i.r(t),
            i.d(t, {
                easeInCubic: () => n,
                easeInOutCubic: () => d,
                easeInOutQuad: () => a,
                easeInOutQuart: () => c,
                easeInOutQuint: () => y,
                easeInQuad: () => o,
                easeInQuart: () => h,
                easeInQuint: () => p,
                easeOutCubic: () => l,
                easeOutQuad: () => r,
                easeOutQuart: () => u,
                easeOutQuint: () => m,
                linear: () => s
            });
        const s = e => e
            , o = e => e * e
            , r = e => e * (2 - e)
            , a = e => e < .5 ? e * e * 2 : e * (4 - 2 * e) - 1
            , n = e => e * e * e
            , l = e => --e * e * e + 1
            , d = e => e < .5 ? 4 * e * e * e : (e - 1) * (2 * e - 2) * (2 * e - 2) + 1
            , h = e => e * e * e * e
            , u = e => 1 - --e * e * e * e
            , c = e => e < .5 ? e * e * e * e * 8 : 1 - (e - 1) * e * e * e * 8
            , p = e => e * e * e * e * e
            , m = e => --e * e * e * e * e + 1
            , y = e => e < .5 ? e * e * e * e * e * 16 : (e - 1) * e * e * e * e * 16 + 1
    }
    ,
    43435: (e, t, i) => {
        "use strict";
        i.d(t, {
            A: () => m
        });
        var s = {};
        i.r(s),
            i.d(s, {
                outputHlsjsEventLogs: () => l,
                outputVideoEventLogs: () => n
            });
        var o = {};
        i.r(o),
            i.d(o, {
                seedRandom: () => p
            });
        var r = i(43462)
            , a = i(25595);
        const n = e => {
            ["abort", "canplay", "canplaythrough", "durationchange", "emptied", "encrypted", "ended", "error", "interruptbegin", "interruptend", "load", "loadeddata", "loadedmetadata", "loadstart", "mozaudioavailable", "pause", "play", "playing", "progress", "ratechange", "seeked", "seeking", "stalled", "suspend", "timeupdate", "volumechange", "waiting"].forEach(t => {
                e.addEventListener(t, e => {
                    console.log(e.type)
                }
                )
            }
            )
        }
            , l = e => {
                const t = (0,
                    a.A)().Hls.Events;
                Object.values(t).forEach(t => {
                    e.on(t, (e, t) => {
                        console.log(e),
                            console.log(t)
                    }
                    )
                }
                )
            }
            ;
        var d = i(44820)
            , h = i(55449)
            , u = i(97401)
            , c = i(53540);
        const p = (e = 0) => {
            let t = e;
            const i = 1e4 * Math.sin(t++);
            return i - Math.floor(i)
        }
            , m = {
                animater: r,
                debug: s,
                easing: d,
                element: h,
                is: u,
                keyboard: c,
                math: o,
                moment: i(28493),
                number: i(89496),
                object: i(86254),
                openrec: i(83279),
                string: i(54240),
                to: i(11456)
            }
    }
    ,
    28493: (e, t, i) => {
        "use strict";
        i.r(t),
            i.d(t, {
                add: () => n,
                floorMilliSeconds: () => s,
                parse: () => r,
                substract: () => a
            });
        const s = e => {
            const t = new Date(e);
            return t.setMilliseconds(0),
                t.toISOString()
        }
            , o = /^(\d{4})-?(\d{1,2})-?(\d{0,2})[^0-9]*(\d{1,2})?:?(\d{1,2})?:?(\d{1,2})?.?(\d{1,3})?$/
            , r = e => {
                if (void 0 === e)
                    return new Date;
                if ("number" == typeof e)
                    return new Date(e);
                if ("string" == typeof e) {
                    const t = e.match(o);
                    if (t)
                        return new Date(Number(t[1]), Number(t[2]) - 1, Number(t[3]) || 1, Number(t[4]) || 0, Number(t[5]) || 0, Number(t[6]) || 0, Number(t[7]) || 0)
                }
                return new Date(e)
            }
            , a = (e, t) => {
                const i = new Date(e)
                    , s = new Date(t);
                return i.setMilliseconds(0),
                    s.setMilliseconds(0),
                    Math.ceil((s.getTime() - i.getTime()) / 1e3)
            }
            , n = (e, t) => {
                const i = new Date(e);
                return i.setMilliseconds(0),
                    Math.ceil(i.getTime() + 1e3 * t)
            }
    }
    ,
    3279: () => { }
}]);
