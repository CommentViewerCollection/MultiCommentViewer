26115: function(e, t, r) {
    "use strict";
    r.d(t, {
        Fh: () => v,
        Gi: () => m,
        ab: () => b,
        br: () => u,
        em: () => l,
        ev: () => c,
        hC: () => d,
        lt: () => y
    });
    var n, a, i, o, s, l, c, u, d, m, h = r(59018), _ = r(5373), p = r(14396), g = r(69034);
    class f extends h.Q {
        userId = _.M.zero;
        nickname;
        iconUrl;
        constructor(e) {
            super(),
                p.C.util.initPartial(e, this)
        }
        static runtime = p.C;
        static typeName = "dwango.nicolive.chat.data.atoms.ModeratorUserInfo";
        static fields = p.C.util.newFieldList(() => [{
            no: 1,
            name: "user_id",
            kind: "scalar",
            T: 3
        }, {
            no: 2,
            name: "nickname",
            kind: "scalar",
            T: 9,
            opt: !0
        }, {
            no: 3,
            name: "iconUrl",
            kind: "scalar",
            T: 9,
            opt: !0
        }]);
        static fromBinary(e, t) {
            return new f().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new f().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new f().fromJsonString(e, t)
        }
        static equals(e, t) {
            return p.C.util.equals(f, e, t)
        }
    }
    class v extends h.Q {
        operation = l.ADD;
        operator;
        updatedAt;
        constructor(e) {
            super(),
                p.C.util.initPartial(e, this)
        }
        static runtime = p.C;
        static typeName = "dwango.nicolive.chat.data.atoms.ModeratorUpdated";
        static fields = p.C.util.newFieldList(() => [{
            no: 1,
            name: "operation",
            kind: "enum",
            T: p.C.getEnumType(l)
        }, {
            no: 2,
            name: "operator",
            kind: "message",
            T: f
        }, {
            no: 3,
            name: "updatedAt",
            kind: "message",
            T: g.D
        }]);
        static fromBinary(e, t) {
            return new v().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new v().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new v().fromJsonString(e, t)
        }
        static equals(e, t) {
            return p.C.util.equals(v, e, t)
        }
    }
    (n = l || (l = {}))[n.ADD = 0] = "ADD",
        n[n.DELETE = 1] = "DELETE",
        p.C.util.setEnumType(l, "dwango.nicolive.chat.data.atoms.ModeratorUpdated.ModeratorOperation", [{
            no: 0,
            name: "ADD"
        }, {
            no: 1,
            name: "DELETE"
        }]);
    class y extends h.Q {
        operation = c.ADD;
        ssngId = _.M.zero;
        operator;
        type;
        source;
        updatedAt;
        operatorType = d.MODERATOR;
        constructor(e) {
            super(),
                p.C.util.initPartial(e, this)
        }
        static runtime = p.C;
        static typeName = "dwango.nicolive.chat.data.atoms.SSNGUpdated";
        static fields = p.C.util.newFieldList(() => [{
            no: 1,
            name: "operation",
            kind: "enum",
            T: p.C.getEnumType(c)
        }, {
            no: 2,
            name: "ssng_id",
            kind: "scalar",
            T: 3
        }, {
            no: 3,
            name: "operator",
            kind: "message",
            T: f
        }, {
            no: 4,
            name: "type",
            kind: "enum",
            T: p.C.getEnumType(u),
            opt: !0
        }, {
            no: 5,
            name: "source",
            kind: "scalar",
            T: 9,
            opt: !0
        }, {
            no: 6,
            name: "updatedAt",
            kind: "message",
            T: g.D,
            opt: !0
        }, {
            no: 7,
            name: "operator_type",
            kind: "enum",
            T: p.C.getEnumType(d)
        }]);
        static fromBinary(e, t) {
            return new y().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new y().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new y().fromJsonString(e, t)
        }
        static equals(e, t) {
            return p.C.util.equals(y, e, t)
        }
    }
    (a = c || (c = {}))[a.ADD = 0] = "ADD",
        a[a.DELETE = 1] = "DELETE",
        p.C.util.setEnumType(c, "dwango.nicolive.chat.data.atoms.SSNGUpdated.SSNGOperation", [{
            no: 0,
            name: "ADD"
        }, {
            no: 1,
            name: "DELETE"
        }]),
        (i = u || (u = {}))[i.USER = 0] = "USER",
        i[i.WORD = 1] = "WORD",
        i[i.COMMAND = 2] = "COMMAND",
        p.C.util.setEnumType(u, "dwango.nicolive.chat.data.atoms.SSNGUpdated.SSNGType", [{
            no: 0,
            name: "USER"
        }, {
            no: 1,
            name: "WORD"
        }, {
            no: 2,
            name: "COMMAND"
        }]),
        (o = d || (d = {}))[o.MODERATOR = 0] = "MODERATOR",
        o[o.BROADCASTER = 1] = "BROADCASTER",
        p.C.util.setEnumType(d, "dwango.nicolive.chat.data.atoms.SSNGUpdated.SSNGOperatorType", [{
            no: 0,
            name: "MODERATOR"
        }, {
            no: 1,
            name: "BROADCASTER"
        }]);
    class b extends h.Q {
        message;
        guidelineItems = [];
        updatedAt;
        constructor(e) {
            super(),
                p.C.util.initPartial(e, this)
        }
        static runtime = p.C;
        static typeName = "dwango.nicolive.chat.data.atoms.ModerationAnnouncement";
        static fields = p.C.util.newFieldList(() => [{
            no: 1,
            name: "message",
            kind: "scalar",
            T: 9,
            opt: !0
        }, {
            no: 2,
            name: "guidelineItems",
            kind: "enum",
            T: p.C.getEnumType(m),
            repeated: !0
        }, {
            no: 3,
            name: "updatedAt",
            kind: "message",
            T: g.D
        }]);
        static fromBinary(e, t) {
            return new b().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new b().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new b().fromJsonString(e, t)
        }
        static equals(e, t) {
            return p.C.util.equals(b, e, t)
        }
    }
    (s = m || (m = {}))[s.UNKNOWN = 0] = "UNKNOWN",
        s[s.SEXUAL = 1] = "SEXUAL",
        s[s.SPAM = 2] = "SPAM",
        s[s.SLANDER = 3] = "SLANDER",
        s[s.PERSONAL_INFORMATION = 4] = "PERSONAL_INFORMATION",
        p.C.util.setEnumType(m, "dwango.nicolive.chat.data.atoms.ModerationAnnouncement.GuidelineItem", [{
            no: 0,
            name: "UNKNOWN"
        }, {
            no: 1,
            name: "SEXUAL"
        }, {
            no: 2,
            name: "SPAM"
        }, {
            no: 3,
            name: "SLANDER"
        }, {
            no: 4,
            name: "PERSONAL_INFORMATION"
        }])
},
67154: function(e, t, r) {
    "use strict";
    r.d(t, {
        M: () => a,
        y: () => s
    });
    var n, a, i = r(59018), o = r(14396);
    class s extends i.Q {
        type = a.UNKNOWN;
        message = "";
        showInTelop = !1;
        showInList = !1;
        constructor(e) {
            super(),
                o.C.util.initPartial(e, this)
        }
        static runtime = o.C;
        static typeName = "dwango.nicolive.chat.data.atoms.SimpleNotificationV2";
        static fields = o.C.util.newFieldList(() => [{
            no: 1,
            name: "type",
            kind: "enum",
            T: o.C.getEnumType(a)
        }, {
            no: 2,
            name: "message",
            kind: "scalar",
            T: 9
        }, {
            no: 3,
            name: "show_in_telop",
            kind: "scalar",
            T: 8
        }, {
            no: 4,
            name: "show_in_list",
            kind: "scalar",
            T: 8
        }]);
        static fromBinary(e, t) {
            return new s().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new s().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new s().fromJsonString(e, t)
        }
        static equals(e, t) {
            return o.C.util.equals(s, e, t)
        }
    }
    (n = a || (a = {}))[n.UNKNOWN = 0] = "UNKNOWN",
        n[n.ICHIBA = 1] = "ICHIBA",
        n[n.EMOTION = 2] = "EMOTION",
        n[n.CRUISE = 3] = "CRUISE",
        n[n.PROGRAM_EXTENDED = 4] = "PROGRAM_EXTENDED",
        n[n.RANKING_IN = 5] = "RANKING_IN",
        n[n.VISITED = 6] = "VISITED",
        n[n.SUPPORTER_REGISTERED = 7] = "SUPPORTER_REGISTERED",
        n[n.USER_LEVEL_UP = 8] = "USER_LEVEL_UP",
        n[n.USER_FOLLOW = 9] = "USER_FOLLOW",
        o.C.util.setEnumType(a, "dwango.nicolive.chat.data.atoms.SimpleNotificationV2.NotificationType", [{
            no: 0,
            name: "UNKNOWN"
        }, {
            no: 1,
            name: "ICHIBA"
        }, {
            no: 2,
            name: "EMOTION"
        }, {
            no: 3,
            name: "CRUISE"
        }, {
            no: 4,
            name: "PROGRAM_EXTENDED"
        }, {
            no: 5,
            name: "RANKING_IN"
        }, {
            no: 6,
            name: "VISITED"
        }, {
            no: 7,
            name: "SUPPORTER_REGISTERED"
        }, {
            no: 8,
            name: "USER_LEVEL_UP"
        }, {
            no: 9,
            name: "USER_FOLLOW"
        }])
},
72387: function(e, t, r) {
    "use strict";
    r.d(t, {
        $m: () => V,
        DC: () => y,
        DU: () => R,
        Eb: () => ec,
        Eo: () => Q,
        FR: () => ee,
        GP: () => b,
        Ib: () => C,
        If: () => G,
        K$: () => ea,
        L0: () => T,
        Pt: () => z,
        T2: () => Z,
        UH: () => S,
        X9: () => J,
        XT: () => f,
        fC: () => A,
        jv: () => et,
        lM: () => w,
        m6: () => es,
        nq: () => x,
        pD: () => q,
        pL: () => I,
        q_: () => v,
        qj: () => er,
        ri: () => ei,
        ry: () => D,
        v4: () => E,
        wK: () => P
    });
    var n, a, i, o, s, l, c, u, d, m, h, _, p, g, f, v, y, b, E, T, S, C, A, w, P, x, R, I, L = r(59018), k = r(14396), M = r(28694), N = r(5373);
    class D extends L.Q {
        content = "";
        vpos = 0;
        accountStatus = f.Standard;
        name;
        rawUserId;
        hashedUserId;
        modifier;
        no = 0;
        constructor(e) {
            super(),
                k.C.util.initPartial(e, this)
        }
        static runtime = k.C;
        static typeName = "dwango.nicolive.chat.data.Chat";
        static fields = k.C.util.newFieldList(() => [{
            no: 1,
            name: "content",
            kind: "scalar",
            T: 9
        }, {
            no: 3,
            name: "vpos",
            kind: "scalar",
            T: 5
        }, {
            no: 4,
            name: "account_status",
            kind: "enum",
            T: k.C.getEnumType(f)
        }, {
            no: 2,
            name: "name",
            kind: "scalar",
            T: 9,
            opt: !0
        }, {
            no: 5,
            name: "raw_user_id",
            kind: "scalar",
            T: 3,
            opt: !0
        }, {
            no: 6,
            name: "hashed_user_id",
            kind: "scalar",
            T: 9,
            opt: !0
        }, {
            no: 7,
            name: "modifier",
            kind: "message",
            T: F
        }, {
            no: 8,
            name: "no",
            kind: "scalar",
            T: 5
        }]);
        static fromBinary(e, t) {
            return new D().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new D().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new D().fromJsonString(e, t)
        }
        static equals(e, t) {
            return k.C.util.equals(D, e, t)
        }
    }
    (n = f || (f = {}))[n.Standard = 0] = "Standard",
        n[n.Premium = 1] = "Premium",
        k.C.util.setEnumType(f, "dwango.nicolive.chat.data.Chat.AccountStatus", [{
            no: 0,
            name: "Standard"
        }, {
            no: 1,
            name: "Premium"
        }]);
    class F extends L.Q {
        position = v.naka;
        size = y.medium;
        color = {
            case: void 0
        };
        font = E.defont;
        opacity = T.Normal;
        constructor(e) {
            super(),
                k.C.util.initPartial(e, this)
        }
        static runtime = k.C;
        static typeName = "dwango.nicolive.chat.data.Chat.Modifier";
        static fields = k.C.util.newFieldList(() => [{
            no: 1,
            name: "position",
            kind: "enum",
            T: k.C.getEnumType(v)
        }, {
            no: 2,
            name: "size",
            kind: "enum",
            T: k.C.getEnumType(y)
        }, {
            no: 3,
            name: "named_color",
            kind: "enum",
            T: k.C.getEnumType(b),
            oneof: "color"
        }, {
            no: 4,
            name: "full_color",
            kind: "message",
            T: O,
            oneof: "color"
        }, {
            no: 5,
            name: "font",
            kind: "enum",
            T: k.C.getEnumType(E)
        }, {
            no: 6,
            name: "opacity",
            kind: "enum",
            T: k.C.getEnumType(T)
        }]);
        static fromBinary(e, t) {
            return new F().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new F().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new F().fromJsonString(e, t)
        }
        static equals(e, t) {
            return k.C.util.equals(F, e, t)
        }
    }
    (a = v || (v = {}))[a.naka = 0] = "naka",
        a[a.shita = 1] = "shita",
        a[a.ue = 2] = "ue",
        k.C.util.setEnumType(v, "dwango.nicolive.chat.data.Chat.Modifier.Pos", [{
            no: 0,
            name: "naka"
        }, {
            no: 1,
            name: "shita"
        }, {
            no: 2,
            name: "ue"
        }]),
        (i = y || (y = {}))[i.medium = 0] = "medium",
        i[i.small = 1] = "small",
        i[i.big = 2] = "big",
        k.C.util.setEnumType(y, "dwango.nicolive.chat.data.Chat.Modifier.Size", [{
            no: 0,
            name: "medium"
        }, {
            no: 1,
            name: "small"
        }, {
            no: 2,
            name: "big"
        }]),
        (o = b || (b = {}))[o.white = 0] = "white",
        o[o.red = 1] = "red",
        o[o.pink = 2] = "pink",
        o[o.orange = 3] = "orange",
        o[o.yellow = 4] = "yellow",
        o[o.green = 5] = "green",
        o[o.cyan = 6] = "cyan",
        o[o.blue = 7] = "blue",
        o[o.purple = 8] = "purple",
        o[o.black = 9] = "black",
        o[o.white2 = 10] = "white2",
        o[o.red2 = 11] = "red2",
        o[o.pink2 = 12] = "pink2",
        o[o.orange2 = 13] = "orange2",
        o[o.yellow2 = 14] = "yellow2",
        o[o.green2 = 15] = "green2",
        o[o.cyan2 = 16] = "cyan2",
        o[o.blue2 = 17] = "blue2",
        o[o.purple2 = 18] = "purple2",
        o[o.black2 = 19] = "black2",
        k.C.util.setEnumType(b, "dwango.nicolive.chat.data.Chat.Modifier.ColorName", [{
            no: 0,
            name: "white"
        }, {
            no: 1,
            name: "red"
        }, {
            no: 2,
            name: "pink"
        }, {
            no: 3,
            name: "orange"
        }, {
            no: 4,
            name: "yellow"
        }, {
            no: 5,
            name: "green"
        }, {
            no: 6,
            name: "cyan"
        }, {
            no: 7,
            name: "blue"
        }, {
            no: 8,
            name: "purple"
        }, {
            no: 9,
            name: "black"
        }, {
            no: 10,
            name: "white2"
        }, {
            no: 11,
            name: "red2"
        }, {
            no: 12,
            name: "pink2"
        }, {
            no: 13,
            name: "orange2"
        }, {
            no: 14,
            name: "yellow2"
        }, {
            no: 15,
            name: "green2"
        }, {
            no: 16,
            name: "cyan2"
        }, {
            no: 17,
            name: "blue2"
        }, {
            no: 18,
            name: "purple2"
        }, {
            no: 19,
            name: "black2"
        }]),
        (s = E || (E = {}))[s.defont = 0] = "defont",
        s[s.mincho = 1] = "mincho",
        s[s.gothic = 2] = "gothic",
        k.C.util.setEnumType(E, "dwango.nicolive.chat.data.Chat.Modifier.Font", [{
            no: 0,
            name: "defont"
        }, {
            no: 1,
            name: "mincho"
        }, {
            no: 2,
            name: "gothic"
        }]),
        (l = T || (T = {}))[l.Normal = 0] = "Normal",
        l[l.Translucent = 1] = "Translucent",
        k.C.util.setEnumType(T, "dwango.nicolive.chat.data.Chat.Modifier.Opacity", [{
            no: 0,
            name: "Normal"
        }, {
            no: 1,
            name: "Translucent"
        }]);
    class O extends L.Q {
        r = 0;
        g = 0;
        b = 0;
        constructor(e) {
            super(),
                k.C.util.initPartial(e, this)
        }
        static runtime = k.C;
        static typeName = "dwango.nicolive.chat.data.Chat.Modifier.FullColor";
        static fields = k.C.util.newFieldList(() => [{
            no: 1,
            name: "r",
            kind: "scalar",
            T: 5
        }, {
            no: 2,
            name: "g",
            kind: "scalar",
            T: 5
        }, {
            no: 3,
            name: "b",
            kind: "scalar",
            T: 5
        }]);
        static fromBinary(e, t) {
            return new O().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new O().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new O().fromJsonString(e, t)
        }
        static equals(e, t) {
            return k.C.util.equals(O, e, t)
        }
    }
    class B extends L.Q {
        content = "";
        name;
        modifier;
        link;
        constructor(e) {
            super(),
                k.C.util.initPartial(e, this)
        }
        static runtime = k.C;
        static typeName = "dwango.nicolive.chat.data.OperatorComment";
        static fields = k.C.util.newFieldList(() => [{
            no: 1,
            name: "content",
            kind: "scalar",
            T: 9
        }, {
            no: 2,
            name: "name",
            kind: "scalar",
            T: 9,
            opt: !0
        }, {
            no: 3,
            name: "modifier",
            kind: "message",
            T: F
        }, {
            no: 4,
            name: "link",
            kind: "scalar",
            T: 9,
            opt: !0
        }]);
        static fromBinary(e, t) {
            return new B().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new B().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new B().fromJsonString(e, t)
        }
        static equals(e, t) {
            return k.C.util.equals(B, e, t)
        }
    }
    class U extends L.Q {
        content = "";
        message = "";
        wait;
        constructor(e) {
            super(),
                k.C.util.initPartial(e, this)
        }
        static runtime = k.C;
        static typeName = "dwango.nicolive.chat.data.Jump";
        static fields = k.C.util.newFieldList(() => [{
            no: 1,
            name: "content",
            kind: "scalar",
            T: 9
        }, {
            no: 2,
            name: "message",
            kind: "scalar",
            T: 9
        }, {
            no: 4,
            name: "wait",
            kind: "message",
            T: M.d
        }]);
        static fromBinary(e, t) {
            return new U().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new U().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new U().fromJsonString(e, t)
        }
        static equals(e, t) {
            return k.C.util.equals(U, e, t)
        }
    }
    class H extends L.Q {
        uri = "";
        message = "";
        wait;
        constructor(e) {
            super(),
                k.C.util.initPartial(e, this)
        }
        static runtime = k.C;
        static typeName = "dwango.nicolive.chat.data.Redirect";
        static fields = k.C.util.newFieldList(() => [{
            no: 1,
            name: "uri",
            kind: "scalar",
            T: 9
        }, {
            no: 2,
            name: "message",
            kind: "scalar",
            T: 9
        }, {
            no: 4,
            name: "wait",
            kind: "message",
            T: M.d
        }]);
        static fromBinary(e, t) {
            return new H().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new H().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new H().fromJsonString(e, t)
        }
        static equals(e, t) {
            return k.C.util.equals(H, e, t)
        }
    }
    class G extends L.Q {
        message = {
            case: void 0
        };
        constructor(e) {
            super(),
                k.C.util.initPartial(e, this)
        }
        static runtime = k.C;
        static typeName = "dwango.nicolive.chat.data.SimpleNotification";
        static fields = k.C.util.newFieldList(() => [{
            no: 1,
            name: "ichiba",
            kind: "scalar",
            T: 9,
            oneof: "message"
        }, {
            no: 2,
            name: "quote",
            kind: "scalar",
            T: 9,
            oneof: "message"
        }, {
            no: 3,
            name: "emotion",
            kind: "scalar",
            T: 9,
            oneof: "message"
        }, {
            no: 4,
            name: "cruise",
            kind: "scalar",
            T: 9,
            oneof: "message"
        }, {
            no: 5,
            name: "program_extended",
            kind: "scalar",
            T: 9,
            oneof: "message"
        }, {
            no: 6,
            name: "ranking_in",
            kind: "scalar",
            T: 9,
            oneof: "message"
        }, {
            no: 8,
            name: "ranking_updated",
            kind: "scalar",
            T: 9,
            oneof: "message"
        }, {
            no: 7,
            name: "visited",
            kind: "scalar",
            T: 9,
            oneof: "message"
        }, {
            no: 9,
            name: "supporter_registered",
            kind: "scalar",
            T: 9,
            oneof: "message"
        }, {
            no: 10,
            name: "user_level_up",
            kind: "scalar",
            T: 9,
            oneof: "message"
        }]);
        static fromBinary(e, t) {
            return new G().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new G().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new G().fromJsonString(e, t)
        }
        static equals(e, t) {
            return k.C.util.equals(G, e, t)
        }
    }
    class q extends L.Q {
        itemId = "";
        advertiserUserId;
        advertiserName = "";
        point = N.M.zero;
        message = "";
        itemName = "";
        contributionRank;
        giftBarUpdate;
        constructor(e) {
            super(),
                k.C.util.initPartial(e, this)
        }
        static runtime = k.C;
        static typeName = "dwango.nicolive.chat.data.Gift";
        static fields = k.C.util.newFieldList(() => [{
            no: 1,
            name: "item_id",
            kind: "scalar",
            T: 9
        }, {
            no: 2,
            name: "advertiser_user_id",
            kind: "scalar",
            T: 3,
            opt: !0
        }, {
            no: 3,
            name: "advertiser_name",
            kind: "scalar",
            T: 9
        }, {
            no: 4,
            name: "point",
            kind: "scalar",
            T: 3
        }, {
            no: 5,
            name: "message",
            kind: "scalar",
            T: 9
        }, {
            no: 6,
            name: "item_name",
            kind: "scalar",
            T: 9
        }, {
            no: 7,
            name: "contribution_rank",
            kind: "scalar",
            T: 5,
            opt: !0
        }, {
            no: 8,
            name: "gift_bar_update",
            kind: "message",
            T: $,
            opt: !0
        }]);
        static fromBinary(e, t) {
            return new q().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new q().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new q().fromJsonString(e, t)
        }
        static equals(e, t) {
            return k.C.util.equals(q, e, t)
        }
    }
    class $ extends L.Q {
        currentLevel = 0;
        nextLevelRewardCount = 0;
        remainingPointsForNextLevel = 0;
        requiredPointsForNextLevel = 0;
        constructor(e) {
            super(),
                k.C.util.initPartial(e, this)
        }
        static runtime = k.C;
        static typeName = "dwango.nicolive.chat.data.Gift.GiftBarUpdate";
        static fields = k.C.util.newFieldList(() => [{
            no: 1,
            name: "current_level",
            kind: "scalar",
            T: 5
        }, {
            no: 2,
            name: "next_level_reward_count",
            kind: "scalar",
            T: 5
        }, {
            no: 3,
            name: "remaining_points_for_next_level",
            kind: "scalar",
            T: 5
        }, {
            no: 4,
            name: "required_points_for_next_level",
            kind: "scalar",
            T: 5
        }]);
        static fromBinary(e, t) {
            return new $().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new $().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new $().fromJsonString(e, t)
        }
        static equals(e, t) {
            return k.C.util.equals($, e, t)
        }
    }
    class V extends L.Q {
        versions = {
            case: void 0
        };
        constructor(e) {
            super(),
                k.C.util.initPartial(e, this)
        }
        static runtime = k.C;
        static typeName = "dwango.nicolive.chat.data.Nicoad";
        static fields = k.C.util.newFieldList(() => [{
            no: 1,
            name: "v0",
            kind: "message",
            T: W,
            oneof: "versions"
        }, {
            no: 2,
            name: "v1",
            kind: "message",
            T: Y,
            oneof: "versions"
        }]);
        static fromBinary(e, t) {
            return new V().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new V().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new V().fromJsonString(e, t)
        }
        static equals(e, t) {
            return k.C.util.equals(V, e, t)
        }
    }
    class W extends L.Q {
        latest;
        ranking = [];
        totalPoint = 0;
        constructor(e) {
            super(),
                k.C.util.initPartial(e, this)
        }
        static runtime = k.C;
        static typeName = "dwango.nicolive.chat.data.Nicoad.V0";
        static fields = k.C.util.newFieldList(() => [{
            no: 1,
            name: "latest",
            kind: "message",
            T: j
        }, {
            no: 2,
            name: "ranking",
            kind: "message",
            T: K,
            repeated: !0
        }, {
            no: 3,
            name: "total_point",
            kind: "scalar",
            T: 5
        }]);
        static fromBinary(e, t) {
            return new W().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new W().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new W().fromJsonString(e, t)
        }
        static equals(e, t) {
            return k.C.util.equals(W, e, t)
        }
    }
    class j extends L.Q {
        advertiser = "";
        point = 0;
        message;
        constructor(e) {
            super(),
                k.C.util.initPartial(e, this)
        }
        static runtime = k.C;
        static typeName = "dwango.nicolive.chat.data.Nicoad.V0.Latest";
        static fields = k.C.util.newFieldList(() => [{
            no: 1,
            name: "advertiser",
            kind: "scalar",
            T: 9
        }, {
            no: 2,
            name: "point",
            kind: "scalar",
            T: 5
        }, {
            no: 3,
            name: "message",
            kind: "scalar",
            T: 9,
            opt: !0
        }]);
        static fromBinary(e, t) {
            return new j().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new j().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new j().fromJsonString(e, t)
        }
        static equals(e, t) {
            return k.C.util.equals(j, e, t)
        }
    }
    class K extends L.Q {
        advertiser = "";
        rank = 0;
        message;
        userRank;
        constructor(e) {
            super(),
                k.C.util.initPartial(e, this)
        }
        static runtime = k.C;
        static typeName = "dwango.nicolive.chat.data.Nicoad.V0.Ranking";
        static fields = k.C.util.newFieldList(() => [{
            no: 1,
            name: "advertiser",
            kind: "scalar",
            T: 9
        }, {
            no: 2,
            name: "rank",
            kind: "scalar",
            T: 5
        }, {
            no: 3,
            name: "message",
            kind: "scalar",
            T: 9,
            opt: !0
        }, {
            no: 4,
            name: "user_rank",
            kind: "scalar",
            T: 5,
            opt: !0
        }]);
        static fromBinary(e, t) {
            return new K().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new K().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new K().fromJsonString(e, t)
        }
        static equals(e, t) {
            return k.C.util.equals(K, e, t)
        }
    }
    class Y extends L.Q {
        totalAdPoint = 0;
        message = "";
        constructor(e) {
            super(),
                k.C.util.initPartial(e, this)
        }
        static runtime = k.C;
        static typeName = "dwango.nicolive.chat.data.Nicoad.V1";
        static fields = k.C.util.newFieldList(() => [{
            no: 1,
            name: "total_ad_point",
            kind: "scalar",
            T: 5
        }, {
            no: 2,
            name: "message",
            kind: "scalar",
            T: 9
        }]);
        static fromBinary(e, t) {
            return new Y().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new Y().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new Y().fromJsonString(e, t)
        }
        static equals(e, t) {
            return k.C.util.equals(Y, e, t)
        }
    }
    class z extends L.Q {
        status = S.Unrestricted;
        followRestriction;
        constructor(e) {
            super(),
                k.C.util.initPartial(e, this)
        }
        static runtime = k.C;
        static typeName = "dwango.nicolive.chat.data.CommentLock";
        static fields = k.C.util.newFieldList(() => [{
            no: 1,
            name: "status",
            kind: "enum",
            T: k.C.getEnumType(S)
        }, {
            no: 2,
            name: "follow_restriction",
            kind: "message",
            T: X,
            opt: !0
        }]);
        static fromBinary(e, t) {
            return new z().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new z().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new z().fromJsonString(e, t)
        }
        static equals(e, t) {
            return k.C.util.equals(z, e, t)
        }
    }
    (c = S || (S = {}))[c.Unrestricted = 0] = "Unrestricted",
        c[c.Locked = 1] = "Locked",
        c[c.Restricted = 2] = "Restricted",
        k.C.util.setEnumType(S, "dwango.nicolive.chat.data.CommentLock.Status", [{
            no: 0,
            name: "Unrestricted"
        }, {
            no: 1,
            name: "Locked"
        }, {
            no: 2,
            name: "Restricted"
        }]);
    class X extends L.Q {
        minimumFollowDuration;
        constructor(e) {
            super(),
                k.C.util.initPartial(e, this)
        }
        static runtime = k.C;
        static typeName = "dwango.nicolive.chat.data.CommentLock.FollowRestriction";
        static fields = k.C.util.newFieldList(() => [{
            no: 1,
            name: "minimum_follow_duration",
            kind: "message",
            T: M.d
        }]);
        static fromBinary(e, t) {
            return new X().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new X().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new X().fromJsonString(e, t)
        }
        static equals(e, t) {
            return k.C.util.equals(X, e, t)
        }
    }
    class J extends L.Q {
        layout = C.Normal;
        constructor(e) {
            super(),
                k.C.util.initPartial(e, this)
        }
        static runtime = k.C;
        static typeName = "dwango.nicolive.chat.data.CommentMode";
        static fields = k.C.util.newFieldList(() => [{
            no: 1,
            name: "layout",
            kind: "enum",
            T: k.C.getEnumType(C)
        }]);
        static fromBinary(e, t) {
            return new J().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new J().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new J().fromJsonString(e, t)
        }
        static equals(e, t) {
            return k.C.util.equals(J, e, t)
        }
    }
    (u = C || (C = {}))[u.Normal = 0] = "Normal",
        u[u.SplitTop = 1] = "SplitTop",
        u[u.Background = 2] = "Background",
        k.C.util.setEnumType(C, "dwango.nicolive.chat.data.CommentMode.Layout", [{
            no: 0,
            name: "Normal"
        }, {
            no: 1,
            name: "SplitTop"
        }, {
            no: 2,
            name: "Background"
        }]);
    class Q extends L.Q {
        constructor(e) {
            super(),
                k.C.util.initPartial(e, this)
        }
        static runtime = k.C;
        static typeName = "dwango.nicolive.chat.data.GameUpdate";
        static fields = k.C.util.newFieldList(() => []);
        static fromBinary(e, t) {
            return new Q().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new Q().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new Q().fromJsonString(e, t)
        }
        static equals(e, t) {
            return k.C.util.equals(Q, e, t)
        }
    }
    class Z extends L.Q {
        position = A.off;
        size = w.small;
        duration;
        constructor(e) {
            super(),
                k.C.util.initPartial(e, this)
        }
        static runtime = k.C;
        static typeName = "dwango.nicolive.chat.data.FingerPrint";
        static fields = k.C.util.newFieldList(() => [{
            no: 1,
            name: "position",
            kind: "enum",
            T: k.C.getEnumType(A)
        }, {
            no: 2,
            name: "size",
            kind: "enum",
            T: k.C.getEnumType(w)
        }, {
            no: 4,
            name: "duration",
            kind: "message",
            T: M.d
        }]);
        static fromBinary(e, t) {
            return new Z().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new Z().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new Z().fromJsonString(e, t)
        }
        static equals(e, t) {
            return k.C.util.equals(Z, e, t)
        }
    }
    (d = A || (A = {}))[d.off = 0] = "off",
        d[d.hidarishita = 1] = "hidarishita",
        d[d.shita = 2] = "shita",
        d[d.migishita = 3] = "migishita",
        d[d.hidari = 4] = "hidari",
        d[d.naka = 5] = "naka",
        d[d.migi = 6] = "migi",
        d[d.hidariue = 7] = "hidariue",
        d[d.ue = 8] = "ue",
        d[d.migiue = 9] = "migiue",
        k.C.util.setEnumType(A, "dwango.nicolive.chat.data.FingerPrint.Position", [{
            no: 0,
            name: "off"
        }, {
            no: 1,
            name: "hidarishita"
        }, {
            no: 2,
            name: "shita"
        }, {
            no: 3,
            name: "migishita"
        }, {
            no: 4,
            name: "hidari"
        }, {
            no: 5,
            name: "naka"
        }, {
            no: 6,
            name: "migi"
        }, {
            no: 7,
            name: "hidariue"
        }, {
            no: 8,
            name: "ue"
        }, {
            no: 9,
            name: "migiue"
        }]),
        (m = w || (w = {}))[m.small = 0] = "small",
        m[m.middle = 1] = "middle",
        m[m.big = 2] = "big",
        k.C.util.setEnumType(w, "dwango.nicolive.chat.data.FingerPrint.Size", [{
            no: 0,
            name: "small"
        }, {
            no: 1,
            name: "middle"
        }, {
            no: 2,
            name: "big"
        }]);
    class ee extends L.Q {
        panel = P.Hidden;
        unqualifiedUser = x.Allowed;
        constructor(e) {
            super(),
                k.C.util.initPartial(e, this)
        }
        static runtime = k.C;
        static typeName = "dwango.nicolive.chat.data.TrialPanel";
        static fields = k.C.util.newFieldList(() => [{
            no: 1,
            name: "panel",
            kind: "enum",
            T: k.C.getEnumType(P)
        }, {
            no: 2,
            name: "unqualified_user",
            kind: "enum",
            T: k.C.getEnumType(x)
        }]);
        static fromBinary(e, t) {
            return new ee().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new ee().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new ee().fromJsonString(e, t)
        }
        static equals(e, t) {
            return k.C.util.equals(ee, e, t)
        }
    }
    (h = P || (P = {}))[h.Hidden = 0] = "Hidden",
        h[h.Display = 1] = "Display",
        k.C.util.setEnumType(P, "dwango.nicolive.chat.data.TrialPanel.Panel", [{
            no: 0,
            name: "Hidden"
        }, {
            no: 1,
            name: "Display"
        }]),
        (_ = x || (x = {}))[_.Allowed = 0] = "Allowed",
        _[_.Restricted = 1] = "Restricted",
        _[_.Forbidden = 2] = "Forbidden",
        k.C.util.setEnumType(x, "dwango.nicolive.chat.data.TrialPanel.Mode", [{
            no: 0,
            name: "Allowed"
        }, {
            no: 1,
            name: "Restricted"
        }, {
            no: 2,
            name: "Forbidden"
        }]);
    class et extends L.Q {
        state = R.Unknown;
        constructor(e) {
            super(),
                k.C.util.initPartial(e, this)
        }
        static runtime = k.C;
        static typeName = "dwango.nicolive.chat.data.ProgramStatus";
        static fields = k.C.util.newFieldList(() => [{
            no: 1,
            name: "state",
            kind: "enum",
            T: k.C.getEnumType(R)
        }]);
        static fromBinary(e, t) {
            return new et().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new et().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new et().fromJsonString(e, t)
        }
        static equals(e, t) {
            return k.C.util.equals(et, e, t)
        }
    }
    (p = R || (R = {}))[p.Unknown = 0] = "Unknown",
        p[p.Ended = 1] = "Ended",
        k.C.util.setEnumType(R, "dwango.nicolive.chat.data.ProgramStatus.State", [{
            no: 0,
            name: "Unknown"
        }, {
            no: 1,
            name: "Ended"
        }]);
    class er extends L.Q {
        tags = [];
        ownerLocked = !1;
        constructor(e) {
            super(),
                k.C.util.initPartial(e, this)
        }
        static runtime = k.C;
        static typeName = "dwango.nicolive.chat.data.TagUpdated";
        static fields = k.C.util.newFieldList(() => [{
            no: 1,
            name: "tags",
            kind: "message",
            T: en,
            repeated: !0
        }, {
            no: 2,
            name: "owner_locked",
            kind: "scalar",
            T: 8
        }]);
        static fromBinary(e, t) {
            return new er().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new er().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new er().fromJsonString(e, t)
        }
        static equals(e, t) {
            return k.C.util.equals(er, e, t)
        }
    }
    class en extends L.Q {
        text = "";
        locked = !1;
        reserved = !1;
        nicopediaUri;
        constructor(e) {
            super(),
                k.C.util.initPartial(e, this)
        }
        static runtime = k.C;
        static typeName = "dwango.nicolive.chat.data.TagUpdated.Tag";
        static fields = k.C.util.newFieldList(() => [{
            no: 1,
            name: "text",
            kind: "scalar",
            T: 9
        }, {
            no: 2,
            name: "locked",
            kind: "scalar",
            T: 8
        }, {
            no: 3,
            name: "reserved",
            kind: "scalar",
            T: 8
        }, {
            no: 4,
            name: "nicopedia_uri",
            kind: "scalar",
            T: 9,
            opt: !0
        }]);
        static fromBinary(e, t) {
            return new en().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new en().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new en().fromJsonString(e, t)
        }
        static equals(e, t) {
            return k.C.util.equals(en, e, t)
        }
    }
    class ea extends L.Q {
        viewers;
        comments;
        adPoints;
        giftPoints;
        timeshiftReservations;
        constructor(e) {
            super(),
                k.C.util.initPartial(e, this)
        }
        static runtime = k.C;
        static typeName = "dwango.nicolive.chat.data.Statistics";
        static fields = k.C.util.newFieldList(() => [{
            no: 1,
            name: "viewers",
            kind: "scalar",
            T: 3,
            opt: !0
        }, {
            no: 2,
            name: "comments",
            kind: "scalar",
            T: 3,
            opt: !0
        }, {
            no: 3,
            name: "ad_points",
            kind: "scalar",
            T: 3,
            opt: !0
        }, {
            no: 4,
            name: "gift_points",
            kind: "scalar",
            T: 3,
            opt: !0
        }, {
            no: 6,
            name: "timeshift_reservations",
            kind: "scalar",
            T: 3,
            opt: !0
        }]);
        static fromBinary(e, t) {
            return new ea().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new ea().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new ea().fromJsonString(e, t)
        }
        static equals(e, t) {
            return k.C.util.equals(ea, e, t)
        }
    }
    class ei extends L.Q {
        display;
        constructor(e) {
            super(),
                k.C.util.initPartial(e, this)
        }
        static runtime = k.C;
        static typeName = "dwango.nicolive.chat.data.Marquee";
        static fields = k.C.util.newFieldList(() => [{
            no: 1,
            name: "display",
            kind: "message",
            T: eo,
            opt: !0
        }]);
        static fromBinary(e, t) {
            return new ei().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new ei().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new ei().fromJsonString(e, t)
        }
        static equals(e, t) {
            return k.C.util.equals(ei, e, t)
        }
    }
    class eo extends L.Q {
        operatorComment;
        duration;
        constructor(e) {
            super(),
                k.C.util.initPartial(e, this)
        }
        static runtime = k.C;
        static typeName = "dwango.nicolive.chat.data.Marquee.Display";
        static fields = k.C.util.newFieldList(() => [{
            no: 1,
            name: "operator_comment",
            kind: "message",
            T: B
        }, {
            no: 3,
            name: "duration",
            kind: "message",
            T: M.d,
            opt: !0
        }]);
        static fromBinary(e, t) {
            return new eo().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new eo().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new eo().fromJsonString(e, t)
        }
        static equals(e, t) {
            return k.C.util.equals(eo, e, t)
        }
    }
    class es extends L.Q {
        question = "";
        choices = [];
        status = I.Closed;
        constructor(e) {
            super(),
                k.C.util.initPartial(e, this)
        }
        static runtime = k.C;
        static typeName = "dwango.nicolive.chat.data.Enquete";
        static fields = k.C.util.newFieldList(() => [{
            no: 1,
            name: "question",
            kind: "scalar",
            T: 9
        }, {
            no: 2,
            name: "choices",
            kind: "message",
            T: el,
            repeated: !0
        }, {
            no: 3,
            name: "status",
            kind: "enum",
            T: k.C.getEnumType(I)
        }]);
        static fromBinary(e, t) {
            return new es().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new es().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new es().fromJsonString(e, t)
        }
        static equals(e, t) {
            return k.C.util.equals(es, e, t)
        }
    }
    (g = I || (I = {}))[g.Closed = 0] = "Closed",
        g[g.Poll = 1] = "Poll",
        g[g.Result = 2] = "Result",
        k.C.util.setEnumType(I, "dwango.nicolive.chat.data.Enquete.Status", [{
            no: 0,
            name: "Closed"
        }, {
            no: 1,
            name: "Poll"
        }, {
            no: 2,
            name: "Result"
        }]);
    class el extends L.Q {
        description = "";
        perMille;
        constructor(e) {
            super(),
                k.C.util.initPartial(e, this)
        }
        static runtime = k.C;
        static typeName = "dwango.nicolive.chat.data.Enquete.Choice";
        static fields = k.C.util.newFieldList(() => [{
            no: 1,
            name: "description",
            kind: "scalar",
            T: 9
        }, {
            no: 3,
            name: "per_mille",
            kind: "scalar",
            T: 5,
            opt: !0
        }]);
        static fromBinary(e, t) {
            return new el().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new el().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new el().fromJsonString(e, t)
        }
        static equals(e, t) {
            return k.C.util.equals(el, e, t)
        }
    }
    class ec extends L.Q {
        to = {
            case: void 0
        };
        constructor(e) {
            super(),
                k.C.util.initPartial(e, this)
        }
        static runtime = k.C;
        static typeName = "dwango.nicolive.chat.data.MoveOrder";
        static fields = k.C.util.newFieldList(() => [{
            no: 1,
            name: "jump",
            kind: "message",
            T: U,
            oneof: "to"
        }, {
            no: 2,
            name: "redirect",
            kind: "message",
            T: H,
            oneof: "to"
        }]);
        static fromBinary(e, t) {
            return new ec().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new ec().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new ec().fromJsonString(e, t)
        }
        static equals(e, t) {
            return k.C.util.equals(ec, e, t)
        }
    }
},
58756: function(e, t, r) {
    "use strict";
    r.d(t, {
        Yc: () => $,
        YI: () => W,
        nK: () => v,
        t_: () => Y
    });
    var n, a, i, o, s, l, c, u, d, m, h, _, p, g, f, v, y = r(59018), b = r(14396), E = r(69034), T = r(5373), S = r(72387), C = r(26115);
    class A extends y.Q {
        chat;
        hitorizumo;
        douglas;
        boops;
        codeForPartnerSystem = "";
        douglasScore;
        constructor(e) {
            super(),
                b.C.util.initPartial(e, this)
        }
        static runtime = b.C;
        static typeName = "dwango.nicolive.chat.data.atoms.PreCensored";
        static fields = b.C.util.newFieldList(() => [{
            no: 1,
            name: "chat",
            kind: "message",
            T: S.ry
        }, {
            no: 2,
            name: "hitorizumo",
            kind: "message",
            T: w
        }, {
            no: 3,
            name: "douglas",
            kind: "message",
            T: P
        }, {
            no: 4,
            name: "boops",
            kind: "message",
            T: x
        }, {
            no: 5,
            name: "code_for_partner_system",
            kind: "scalar",
            T: 9
        }, {
            no: 6,
            name: "douglas_score",
            kind: "scalar",
            T: 1,
            opt: !0
        }]);
        static fromBinary(e, t) {
            return new A().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new A().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new A().fromJsonString(e, t)
        }
        static equals(e, t) {
            return b.C.util.equals(A, e, t)
        }
    }
    class w extends y.Q {
        reason = d.CommentBan;
        constructor(e) {
            super(),
                b.C.util.initPartial(e, this)
        }
        static runtime = b.C;
        static typeName = "dwango.nicolive.chat.data.atoms.PreCensored.Hitorizumo";
        static fields = b.C.util.newFieldList(() => [{
            no: 1,
            name: "reason",
            kind: "enum",
            T: b.C.getEnumType(d)
        }]);
        static fromBinary(e, t) {
            return new w().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new w().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new w().fromJsonString(e, t)
        }
        static equals(e, t) {
            return b.C.util.equals(w, e, t)
        }
    }
    (n = d || (d = {}))[n.CommentBan = 0] = "CommentBan",
        n[n.IpBan = 1] = "IpBan",
        b.C.util.setEnumType(d, "dwango.nicolive.chat.data.atoms.PreCensored.Hitorizumo.Reason", [{
            no: 0,
            name: "CommentBan"
        }, {
            no: 1,
            name: "IpBan"
        }]);
    class P extends y.Q {
        strength = m.Weak;
        constructor(e) {
            super(),
                b.C.util.initPartial(e, this)
        }
        static runtime = b.C;
        static typeName = "dwango.nicolive.chat.data.atoms.PreCensored.Douglas";
        static fields = b.C.util.newFieldList(() => [{
            no: 1,
            name: "strength",
            kind: "enum",
            T: b.C.getEnumType(m)
        }]);
        static fromBinary(e, t) {
            return new P().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new P().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new P().fromJsonString(e, t)
        }
        static equals(e, t) {
            return b.C.util.equals(P, e, t)
        }
    }
    (a = m || (m = {}))[a.Weak = 0] = "Weak",
        a[a.Medium = 1] = "Medium",
        a[a.Strong = 2] = "Strong",
        a[a.Stronger = 3] = "Stronger",
        b.C.util.setEnumType(m, "dwango.nicolive.chat.data.atoms.PreCensored.Douglas.Strength", [{
            no: 0,
            name: "Weak"
        }, {
            no: 1,
            name: "Medium"
        }, {
            no: 2,
            name: "Strong"
        }, {
            no: 3,
            name: "Stronger"
        }]);
    class x extends y.Q {
        violation = {
            case: void 0
        };
        constructor(e) {
            super(),
                b.C.util.initPartial(e, this)
        }
        static runtime = b.C;
        static typeName = "dwango.nicolive.chat.data.atoms.PreCensored.Boops";
        static fields = b.C.util.newFieldList(() => [{
            no: 1,
            name: "global",
            kind: "message",
            T: R,
            oneof: "violation"
        }, {
            no: 2,
            name: "channel",
            kind: "message",
            T: R,
            oneof: "violation"
        }, {
            no: 3,
            name: "namanushi",
            kind: "message",
            T: R,
            oneof: "violation"
        }, {
            no: 4,
            name: "program",
            kind: "message",
            T: R,
            oneof: "violation"
        }]);
        static fromBinary(e, t) {
            return new x().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new x().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new x().fromJsonString(e, t)
        }
        static equals(e, t) {
            return b.C.util.equals(x, e, t)
        }
    }
    class R extends y.Q {
        modifier;
        user;
        word;
        constructor(e) {
            super(),
                b.C.util.initPartial(e, this)
        }
        static runtime = b.C;
        static typeName = "dwango.nicolive.chat.data.atoms.PreCensored.Boops.Regulation";
        static fields = b.C.util.newFieldList(() => [{
            no: 2,
            name: "modifier",
            kind: "enum",
            T: b.C.getEnumType(h),
            opt: !0
        }, {
            no: 3,
            name: "user",
            kind: "enum",
            T: b.C.getEnumType(_),
            opt: !0
        }, {
            no: 4,
            name: "word",
            kind: "scalar",
            T: 9,
            opt: !0
        }]);
        static fromBinary(e, t) {
            return new R().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new R().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new R().fromJsonString(e, t)
        }
        static equals(e, t) {
            return b.C.util.equals(R, e, t)
        }
    }
    (i = h || (h = {}))[i.Pos = 0] = "Pos",
        i[i.Size = 1] = "Size",
        i[i.Color = 2] = "Color",
        i[i.Font = 3] = "Font",
        i[i.Opacity = 4] = "Opacity",
        b.C.util.setEnumType(h, "dwango.nicolive.chat.data.atoms.PreCensored.Boops.Regulation.Modifier", [{
            no: 0,
            name: "Pos"
        }, {
            no: 1,
            name: "Size"
        }, {
            no: 2,
            name: "Color"
        }, {
            no: 3,
            name: "Font"
        }, {
            no: 4,
            name: "Opacity"
        }]),
        (o = _ || (_ = {}))[o.Banned = 0] = "Banned",
        b.C.util.setEnumType(_, "dwango.nicolive.chat.data.atoms.PreCensored.Boops.Regulation.User", [{
            no: 0,
            name: "Banned"
        }]);
    class I extends y.Q {
        chat;
        messageId = "";
        sourceLiveId = T.M.zero;
        mode = p.UNKNOWN;
        constructor(e) {
            super(),
                b.C.util.initPartial(e, this)
        }
        static runtime = b.C;
        static typeName = "dwango.nicolive.chat.data.atoms.ForwardedChat";
        static fields = b.C.util.newFieldList(() => [{
            no: 1,
            name: "chat",
            kind: "message",
            T: S.ry
        }, {
            no: 2,
            name: "message_id",
            kind: "scalar",
            T: 9
        }, {
            no: 3,
            name: "source_live_id",
            kind: "scalar",
            T: 3
        }, {
            no: 4,
            name: "mode",
            kind: "enum",
            T: b.C.getEnumType(p)
        }]);
        static fromBinary(e, t) {
            return new I().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new I().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new I().fromJsonString(e, t)
        }
        static equals(e, t) {
            return b.C.util.equals(I, e, t)
        }
    }
    (s = p || (p = {}))[s.UNKNOWN = 0] = "UNKNOWN",
        s[s.FROM_CRUISE = 1] = "FROM_CRUISE",
        s[s.COLLAB_SHARING = 2] = "COLLAB_SHARING",
        b.C.util.setEnumType(p, "dwango.nicolive.chat.data.atoms.ForwardedChat.ForwardingMode", [{
            no: 0,
            name: "UNKNOWN"
        }, {
            no: 1,
            name: "FROM_CRUISE"
        }, {
            no: 2,
            name: "COLLAB_SHARING"
        }]);
    var L = r(67154)
        , k = r(76005);
    class M extends y.Q {
        type = "";
        playId = "";
        ignorable = !1;
        transient = !1;
        parameters;
        constructor(e) {
            super(),
                b.C.util.initPartial(e, this)
        }
        static runtime = b.C;
        static typeName = "dwango.nicolive.chat.data.atoms.AkashicMessageEvent";
        static fields = b.C.util.newFieldList(() => [{
            no: 1,
            name: "type",
            kind: "scalar",
            T: 9
        }, {
            no: 2,
            name: "playId",
            kind: "scalar",
            T: 9
        }, {
            no: 3,
            name: "ignorable",
            kind: "scalar",
            T: 8
        }, {
            no: 4,
            name: "transient",
            kind: "scalar",
            T: 8
        }, {
            no: 5,
            name: "parameters",
            kind: "message",
            T: k._k
        }]);
        static fromBinary(e, t) {
            return new M().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new M().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new M().fromJsonString(e, t)
        }
        static equals(e, t) {
            return b.C.util.equals(M, e, t)
        }
    }
    class N extends y.Q {
        epoch = T.M.zero;
        join = [];
        continuation = [];
        shared = [];
        constructor(e) {
            super(),
                b.C.util.initPartial(e, this)
        }
        static runtime = b.C;
        static typeName = "dwango.nicolive.chat.data.atoms.AkashicStateRouting";
        static fields = b.C.util.newFieldList(() => [{
            no: 1,
            name: "epoch",
            kind: "scalar",
            T: 3
        }, {
            no: 2,
            name: "join",
            kind: "message",
            T: M,
            repeated: !0
        }, {
            no: 3,
            name: "continuation",
            kind: "message",
            T: M,
            repeated: !0
        }, {
            no: 4,
            name: "shared",
            kind: "message",
            T: M,
            repeated: !0
        }]);
        static fromBinary(e, t) {
            return new N().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new N().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new N().fromJsonString(e, t)
        }
        static equals(e, t) {
            return b.C.util.equals(N, e, t)
        }
    }
    class D extends y.Q {
        auditionEnabled;
        constructor(e) {
            super(),
                b.C.util.initPartial(e, this)
        }
        static runtime = b.C;
        static typeName = "dwango.nicolive.chat.data.atoms.FeaturesUpdated";
        static fields = b.C.util.newFieldList(() => [{
            no: 1,
            name: "audition_enabled",
            kind: "scalar",
            T: 8,
            opt: !0
        }]);
        static fromBinary(e, t) {
            return new D().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new D().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new D().fromJsonString(e, t)
        }
        static equals(e, t) {
            return b.C.util.equals(D, e, t)
        }
    }
    class F extends y.Q {
        data = {
            case: void 0
        };
        constructor(e) {
            super(),
                b.C.util.initPartial(e, this)
        }
        static runtime = b.C;
        static typeName = "dwango.nicolive.chat.data.NicoliveMessage";
        static fields = b.C.util.newFieldList(() => [{
            no: 1,
            name: "chat",
            kind: "message",
            T: S.ry,
            oneof: "data"
        }, {
            no: 7,
            name: "simple_notification",
            kind: "message",
            T: S.If,
            oneof: "data"
        }, {
            no: 8,
            name: "gift",
            kind: "message",
            T: S.pD,
            oneof: "data"
        }, {
            no: 9,
            name: "nicoad",
            kind: "message",
            T: S.$m,
            oneof: "data"
        }, {
            no: 13,
            name: "game_update",
            kind: "message",
            T: S.Eo,
            oneof: "data"
        }, {
            no: 17,
            name: "tag_updated",
            kind: "message",
            T: S.qj,
            oneof: "data"
        }, {
            no: 18,
            name: "moderator_updated",
            kind: "message",
            T: C.Fh,
            oneof: "data"
        }, {
            no: 19,
            name: "ssng_updated",
            kind: "message",
            T: C.lt,
            oneof: "data"
        }, {
            no: 20,
            name: "overflowed_chat",
            kind: "message",
            T: S.ry,
            oneof: "data"
        }, {
            no: 21,
            name: "pre_censored",
            kind: "message",
            T: A,
            oneof: "data"
        }, {
            no: 22,
            name: "forwarded_chat",
            kind: "message",
            T: I,
            oneof: "data"
        }, {
            no: 23,
            name: "simple_notification_v2",
            kind: "message",
            T: L.y,
            oneof: "data"
        }, {
            no: 24,
            name: "akashic_message_event",
            kind: "message",
            T: M,
            oneof: "data"
        }, {
            no: 25,
            name: "features_updated",
            kind: "message",
            T: D,
            oneof: "data"
        }]);
        static fromBinary(e, t) {
            return new F().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new F().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new F().fromJsonString(e, t)
        }
        static equals(e, t) {
            return b.C.util.equals(F, e, t)
        }
    }
    class O extends y.Q {
        epoch = T.M.zero;
        items = [];
        constructor(e) {
            super(),
                b.C.util.initPartial(e, this)
        }
        static runtime = b.C;
        static typeName = "dwango.nicolive.chat.data.atoms.IchibaLauncherItemSet";
        static fields = b.C.util.newFieldList(() => [{
            no: 1,
            name: "epoch",
            kind: "scalar",
            T: 3
        }, {
            no: 2,
            name: "items",
            kind: "message",
            T: B,
            repeated: !0
        }]);
        static fromBinary(e, t) {
            return new O().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new O().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new O().fromJsonString(e, t)
        }
        static equals(e, t) {
            return b.C.util.equals(O, e, t)
        }
    }
    class B extends y.Q {
        id = "";
        entityId = "";
        serviceName = "";
        title = "";
        launchDialogType = g.SMALL;
        thumbnailUrl = "";
        thumbnailWidth = 0;
        thumbnailHeight = 0;
        launchUrl = "";
        addedUserId;
        reportUrl;
        running = !1;
        constructor(e) {
            super(),
                b.C.util.initPartial(e, this)
        }
        static runtime = b.C;
        static typeName = "dwango.nicolive.chat.data.atoms.IchibaLauncherItemSet.Item";
        static fields = b.C.util.newFieldList(() => [{
            no: 1,
            name: "id",
            kind: "scalar",
            T: 9
        }, {
            no: 2,
            name: "entityId",
            kind: "scalar",
            T: 9
        }, {
            no: 3,
            name: "serviceName",
            kind: "scalar",
            T: 9
        }, {
            no: 4,
            name: "title",
            kind: "scalar",
            T: 9
        }, {
            no: 5,
            name: "launchDialogType",
            kind: "enum",
            T: b.C.getEnumType(g)
        }, {
            no: 6,
            name: "thumbnailUrl",
            kind: "scalar",
            T: 9
        }, {
            no: 7,
            name: "thumbnailWidth",
            kind: "scalar",
            T: 5
        }, {
            no: 8,
            name: "thumbnailHeight",
            kind: "scalar",
            T: 5
        }, {
            no: 9,
            name: "launchUrl",
            kind: "scalar",
            T: 9
        }, {
            no: 10,
            name: "addedUserId",
            kind: "scalar",
            T: 9,
            opt: !0
        }, {
            no: 11,
            name: "reportUrl",
            kind: "scalar",
            T: 9,
            opt: !0
        }, {
            no: 12,
            name: "running",
            kind: "scalar",
            T: 8
        }]);
        static fromBinary(e, t) {
            return new B().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new B().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new B().fromJsonString(e, t)
        }
        static equals(e, t) {
            return b.C.util.equals(B, e, t)
        }
    }
    (l = g || (g = {}))[l.SMALL = 0] = "SMALL",
        l[l.RICHIVIEW = 1] = "RICHIVIEW",
        b.C.util.setEnumType(g, "dwango.nicolive.chat.data.atoms.IchibaLauncherItemSet.Item.LaunchDialogType", [{
            no: 0,
            name: "SMALL"
        }, {
            no: 1,
            name: "RICHIVIEW"
        }]);
    class U extends y.Q {
        epoch = T.M.zero;
        state = f.PASSTHROUGH;
        constructor(e) {
            super(),
                b.C.util.initPartial(e, this)
        }
        static runtime = b.C;
        static typeName = "dwango.nicolive.chat.data.atoms.StreamStateChange";
        static fields = b.C.util.newFieldList(() => [{
            no: 1,
            name: "epoch",
            kind: "scalar",
            T: 3
        }, {
            no: 2,
            name: "state",
            kind: "enum",
            T: b.C.getEnumType(f)
        }]);
        static fromBinary(e, t) {
            return new U().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new U().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new U().fromJsonString(e, t)
        }
        static equals(e, t) {
            return b.C.util.equals(U, e, t)
        }
    }
    (c = f || (f = {}))[c.PASSTHROUGH = 0] = "PASSTHROUGH",
        c[c.PROCESSED = 1] = "PROCESSED",
        b.C.util.setEnumType(f, "dwango.nicolive.chat.data.atoms.StreamStateChange.State", [{
            no: 0,
            name: "PASSTHROUGH"
        }, {
            no: 1,
            name: "PROCESSED"
        }]);
    class H extends y.Q {
        statistics;
        enquete;
        moveOrder;
        marquee;
        commentLock;
        commentMode;
        trialPanel;
        fingerPrint;
        programStatus;
        moderationAnnouncement;
        ichibaLauncher;
        streamStateChange;
        akashicState;
        constructor(e) {
            super(),
                b.C.util.initPartial(e, this)
        }
        static runtime = b.C;
        static typeName = "dwango.nicolive.chat.data.NicoliveState";
        static fields = b.C.util.newFieldList(() => [{
            no: 1,
            name: "statistics",
            kind: "message",
            T: S.K$,
            opt: !0
        }, {
            no: 2,
            name: "enquete",
            kind: "message",
            T: S.m6,
            opt: !0
        }, {
            no: 3,
            name: "move_order",
            kind: "message",
            T: S.Eb,
            opt: !0
        }, {
            no: 4,
            name: "marquee",
            kind: "message",
            T: S.ri,
            opt: !0
        }, {
            no: 5,
            name: "comment_lock",
            kind: "message",
            T: S.Pt,
            opt: !0
        }, {
            no: 6,
            name: "comment_mode",
            kind: "message",
            T: S.X9,
            opt: !0
        }, {
            no: 7,
            name: "trial_panel",
            kind: "message",
            T: S.FR,
            opt: !0
        }, {
            no: 8,
            name: "finger_print",
            kind: "message",
            T: S.T2,
            opt: !0
        }, {
            no: 9,
            name: "program_status",
            kind: "message",
            T: S.jv,
            opt: !0
        }, {
            no: 10,
            name: "moderation_announcement",
            kind: "message",
            T: C.ab,
            opt: !0
        }, {
            no: 11,
            name: "ichiba_launcher",
            kind: "message",
            T: O,
            opt: !0
        }, {
            no: 12,
            name: "stream_state_change",
            kind: "message",
            T: U,
            opt: !0
        }, {
            no: 13,
            name: "akashic_state",
            kind: "message",
            T: N,
            opt: !0
        }]);
        static fromBinary(e, t) {
            return new H().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new H().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new H().fromJsonString(e, t)
        }
        static equals(e, t) {
            return b.C.util.equals(H, e, t)
        }
    }
    class G extends y.Q {
        origin = {
            case: void 0
        };
        constructor(e) {
            super(),
                b.C.util.initPartial(e, this)
        }
        static runtime = b.C;
        static typeName = "dwango.nicolive.chat.data.NicoliveOrigin";
        static fields = b.C.util.newFieldList(() => [{
            no: 1,
            name: "chat",
            kind: "message",
            T: q,
            oneof: "origin"
        }]);
        static fromBinary(e, t) {
            return new G().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new G().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new G().fromJsonString(e, t)
        }
        static equals(e, t) {
            return b.C.util.equals(G, e, t)
        }
    }
    class q extends y.Q {
        liveId = T.M.zero;
        constructor(e) {
            super(),
                b.C.util.initPartial(e, this)
        }
        static runtime = b.C;
        static typeName = "dwango.nicolive.chat.data.NicoliveOrigin.Chat";
        static fields = b.C.util.newFieldList(() => [{
            no: 1,
            name: "live_id",
            kind: "scalar",
            T: 3
        }]);
        static fromBinary(e, t) {
            return new q().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new q().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new q().fromJsonString(e, t)
        }
        static equals(e, t) {
            return b.C.util.equals(q, e, t)
        }
    }
    class $ extends y.Q {
        meta;
        payload = {
            case: void 0
        };
        constructor(e) {
            super(),
                b.C.util.initPartial(e, this)
        }
        static runtime = b.C;
        static typeName = "dwango.nicolive.chat.service.edge.ChunkedMessage";
        static fields = b.C.util.newFieldList(() => [{
            no: 1,
            name: "meta",
            kind: "message",
            T: V
        }, {
            no: 2,
            name: "message",
            kind: "message",
            T: F,
            oneof: "payload"
        }, {
            no: 4,
            name: "state",
            kind: "message",
            T: H,
            oneof: "payload"
        }, {
            no: 5,
            name: "signal",
            kind: "enum",
            T: b.C.getEnumType(v),
            oneof: "payload"
        }]);
        static fromBinary(e, t) {
            return new $().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new $().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new $().fromJsonString(e, t)
        }
        static equals(e, t) {
            return b.C.util.equals($, e, t)
        }
    }
    (u = v || (v = {}))[u.Flushed = 0] = "Flushed",
        b.C.util.setEnumType(v, "dwango.nicolive.chat.service.edge.ChunkedMessage.Signal", [{
            no: 0,
            name: "Flushed"
        }]);
    class V extends y.Q {
        id = "";
        at;
        origin;
        constructor(e) {
            super(),
                b.C.util.initPartial(e, this)
        }
        static runtime = b.C;
        static typeName = "dwango.nicolive.chat.service.edge.ChunkedMessage.Meta";
        static fields = b.C.util.newFieldList(() => [{
            no: 1,
            name: "id",
            kind: "scalar",
            T: 9
        }, {
            no: 2,
            name: "at",
            kind: "message",
            T: E.D
        }, {
            no: 3,
            name: "origin",
            kind: "message",
            T: G
        }]);
        static fromBinary(e, t) {
            return new V().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new V().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new V().fromJsonString(e, t)
        }
        static equals(e, t) {
            return b.C.util.equals(V, e, t)
        }
    }
    class W extends y.Q {
        messages = [];
        next;
        snapshot;
        constructor(e) {
            super(),
                b.C.util.initPartial(e, this)
        }
        static runtime = b.C;
        static typeName = "dwango.nicolive.chat.service.edge.PackedSegment";
        static fields = b.C.util.newFieldList(() => [{
            no: 1,
            name: "messages",
            kind: "message",
            T: $,
            repeated: !0
        }, {
            no: 2,
            name: "next",
            kind: "message",
            T: j
        }, {
            no: 3,
            name: "snapshot",
            kind: "message",
            T: K
        }]);
        static fromBinary(e, t) {
            return new W().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new W().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new W().fromJsonString(e, t)
        }
        static equals(e, t) {
            return b.C.util.equals(W, e, t)
        }
    }
    class j extends y.Q {
        uri = "";
        constructor(e) {
            super(),
                b.C.util.initPartial(e, this)
        }
        static runtime = b.C;
        static typeName = "dwango.nicolive.chat.service.edge.PackedSegment.Next";
        static fields = b.C.util.newFieldList(() => [{
            no: 1,
            name: "uri",
            kind: "scalar",
            T: 9
        }]);
        static fromBinary(e, t) {
            return new j().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new j().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new j().fromJsonString(e, t)
        }
        static equals(e, t) {
            return b.C.util.equals(j, e, t)
        }
    }
    class K extends y.Q {
        uri = "";
        constructor(e) {
            super(),
                b.C.util.initPartial(e, this)
        }
        static runtime = b.C;
        static typeName = "dwango.nicolive.chat.service.edge.PackedSegment.StateSnapshot";
        static fields = b.C.util.newFieldList(() => [{
            no: 1,
            name: "uri",
            kind: "scalar",
            T: 9
        }]);
        static fromBinary(e, t) {
            return new K().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new K().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new K().fromJsonString(e, t)
        }
        static equals(e, t) {
            return b.C.util.equals(K, e, t)
        }
    }
    class Y extends y.Q {
        entry = {
            case: void 0
        };
        constructor(e) {
            super(),
                b.C.util.initPartial(e, this)
        }
        static runtime = b.C;
        static typeName = "dwango.nicolive.chat.service.edge.ChunkedEntry";
        static fields = b.C.util.newFieldList(() => [{
            no: 2,
            name: "backward",
            kind: "message",
            T: J,
            oneof: "entry"
        }, {
            no: 3,
            name: "previous",
            kind: "message",
            T: X,
            oneof: "entry"
        }, {
            no: 1,
            name: "segment",
            kind: "message",
            T: X,
            oneof: "entry"
        }, {
            no: 4,
            name: "next",
            kind: "message",
            T: z,
            oneof: "entry"
        }]);
        static fromBinary(e, t) {
            return new Y().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new Y().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new Y().fromJsonString(e, t)
        }
        static equals(e, t) {
            return b.C.util.equals(Y, e, t)
        }
    }
    class z extends y.Q {
        at = T.M.zero;
        constructor(e) {
            super(),
                b.C.util.initPartial(e, this)
        }
        static runtime = b.C;
        static typeName = "dwango.nicolive.chat.service.edge.ChunkedEntry.ReadyForNext";
        static fields = b.C.util.newFieldList(() => [{
            no: 1,
            name: "at",
            kind: "scalar",
            T: 3
        }]);
        static fromBinary(e, t) {
            return new z().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new z().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new z().fromJsonString(e, t)
        }
        static equals(e, t) {
            return b.C.util.equals(z, e, t)
        }
    }
    class X extends y.Q {
        from;
        until;
        uri = "";
        constructor(e) {
            super(),
                b.C.util.initPartial(e, this)
        }
        static runtime = b.C;
        static typeName = "dwango.nicolive.chat.service.edge.MessageSegment";
        static fields = b.C.util.newFieldList(() => [{
            no: 1,
            name: "from",
            kind: "message",
            T: E.D
        }, {
            no: 2,
            name: "until",
            kind: "message",
            T: E.D
        }, {
            no: 3,
            name: "uri",
            kind: "scalar",
            T: 9
        }]);
        static fromBinary(e, t) {
            return new X().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new X().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new X().fromJsonString(e, t)
        }
        static equals(e, t) {
            return b.C.util.equals(X, e, t)
        }
    }
    class J extends y.Q {
        until;
        segment;
        snapshot;
        constructor(e) {
            super(),
                b.C.util.initPartial(e, this)
        }
        static runtime = b.C;
        static typeName = "dwango.nicolive.chat.service.edge.BackwardSegment";
        static fields = b.C.util.newFieldList(() => [{
            no: 1,
            name: "until",
            kind: "message",
            T: E.D
        }, {
            no: 2,
            name: "segment",
            kind: "message",
            T: j
        }, {
            no: 3,
            name: "snapshot",
            kind: "message",
            T: K
        }]);
        static fromBinary(e, t) {
            return new J().fromBinary(e, t)
        }
        static fromJson(e, t) {
            return new J().fromJson(e, t)
        }
        static fromJsonString(e, t) {
            return new J().fromJsonString(e, t)
        }
        static equals(e, t) {
            return b.C.util.equals(J, e, t)
        }
    }
},
