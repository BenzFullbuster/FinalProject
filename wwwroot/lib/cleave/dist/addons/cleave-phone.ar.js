!function () {
  function n(n, t) {
    var e = n.split("."),
      l = U;
    e[0] in l || !l.execScript || l.execScript("var " + e[0]);
    for (var r; e.length && (r = e.shift()); )
      e.length || void 0 === t ? (l = l[r] ? l[r] : (l[r] = {})) : (l[r] = t);
  }
  function t(n, t) {
    function e() {}
    (e.prototype = t.prototype),
      (n.M = t.prototype),
      (n.prototype = new e()),
      (n.prototype.constructor = n),
      (n.N = function (n, e, l) {
        for (
          var r = Array(arguments.length - 2), u = 2;
          u < arguments.length;
          u++
        )
          r[u - 2] = arguments[u];
        return t.prototype[e].apply(n, r);
      });
  }
  function e(n, t) {
    null != n && this.a.apply(this, arguments);
  }
  function l(n) {
    n.b = "";
  }
  function r(n, t) {
    n.sort(t || u);
  }
  function u(n, t) {
    return n > t ? 1 : n < t ? -1 : 0;
  }
  function i(n) {
    var t,
      e = [],
      l = 0;
    for (t in n) e[l++] = n[t];
    return e;
  }
  function a(n, t) {
    (this.b = n), (this.a = {});
    for (var e = 0; e < t.length; e++) {
      var l = t[e];
      this.a[l.b] = l;
    }
  }
  function o(n) {
    return (
      (n = i(n.a)),
      r(n, function (n, t) {
        return n.b - t.b;
      }),
      n
    );
  }
  function s(n, t) {
    switch (
      ((this.b = n),
      (this.g = !!t.v),
      (this.a = t.c),
      (this.i = t.type),
      (this.h = !1),
      this.a)
    ) {
      case J:
      case K:
      case L:
      case O:
      case Z:
      case k:
      case Y:
        this.h = !0;
    }
    this.f = t.defaultValue;
  }
  function f() {
    (this.a = {}), (this.f = this.j().a), (this.b = this.g = null);
  }
  function p(n, t) {
    for (var e = o(n.j()), l = 0; l < e.length; l++) {
      var r = e[l],
        u = r.b;
      if (null != t.a[u]) {
        n.b && delete n.b[r.b];
        var i = 11 == r.a || 10 == r.a;
        if (r.g)
          for (var r = c(t, u) || [], a = 0; a < r.length; a++) {
            var s = n,
              f = u,
              h = i ? r[a].clone() : r[a];
            s.a[f] || (s.a[f] = []), s.a[f].push(h), s.b && delete s.b[f];
          }
        else
          (r = c(t, u)),
            i ? ((i = c(n, u)) ? p(i, r) : m(n, u, r.clone())) : m(n, u, r);
      }
    }
  }
  function c(n, t) {
    var e = n.a[t];
    if (null == e) return null;
    if (n.g) {
      if (!(t in n.b)) {
        var l = n.g,
          r = n.f[t];
        if (null != e)
          if (r.g) {
            for (var u = [], i = 0; i < e.length; i++) u[i] = l.b(r, e[i]);
            e = u;
          } else e = l.b(r, e);
        return (n.b[t] = e);
      }
      return n.b[t];
    }
    return e;
  }
  function h(n, t, e) {
    var l = c(n, t);
    return n.f[t].g ? l[e || 0] : l;
  }
  function g(n, t) {
    var e;
    if (null != n.a[t]) e = h(n, t, void 0);
    else
      n: {
        if (((e = n.f[t]), void 0 === e.f)) {
          var l = e.i;
          if (l === Boolean) e.f = !1;
          else if (l === Number) e.f = 0;
          else {
            if (l !== String) {
              e = new l();
              break n;
            }
            e.f = e.h ? "0" : "";
          }
        }
        e = e.f;
      }
    return e;
  }
  function d(n, t) {
    return n.f[t].g
      ? null != n.a[t]
        ? n.a[t].length
        : 0
      : null != n.a[t]
      ? 1
      : 0;
  }
  function m(n, t, e) {
    (n.a[t] = e), n.b && (n.b[t] = e);
  }
  function b(n, t) {
    var e,
      l = [];
    for (e in t) 0 != e && l.push(new s(e, t[e]));
    return new a(n, l);
  } /*

 Protocol Buffer 2 Copyright 2008 Google Inc.
 All other code copyright its respective owners.
 Copyright (C) 2010 The Libphonenumber Authors

 Licensed under the Apache License, Version 2.0 (the "License");
 you may not use this file except in compliance with the License.
 You may obtain a copy of the License at

 http://www.apache.org/licenses/LICENSE-2.0

 Unless required by applicable law or agreed to in writing, software
 distributed under the License is distributed on an "AS IS" BASIS,
 WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 See the License for the specific language governing permissions and
 limitations under the License.
*/
  function y() {
    f.call(this);
  }
  function v() {
    f.call(this);
  }
  function $() {
    f.call(this);
  }
  function _() {}
  function S() {}
  function w() {} /*

 Copyright (C) 2010 The Libphonenumber Authors.

 Licensed under the Apache License, Version 2.0 (the "License");
 you may not use this file except in compliance with the License.
 You may obtain a copy of the License at

 http://www.apache.org/licenses/LICENSE-2.0

 Unless required by applicable law or agreed to in writing, software
 distributed under the License is distributed on an "AS IS" BASIS,
 WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 See the License for the specific language governing permissions and
 limitations under the License.
*/
  function x() {
    this.a = {};
  }
  function A(n) {
    return 0 == n.length || un.test(n);
  }
  function N(n, t) {
    if (null == t) return null;
    t = t.toUpperCase();
    var e = n.a[t];
    if (null == e) {
      if (((e = tn[t]), null == e)) return null;
      (e = new w().a($.j(), e)), (n.a[t] = e);
    }
    return e;
  }
  function j(n) {
    return (n = nn[n]), null == n ? "ZZ" : n[0];
  }
  function R(n) {
    (this.H = RegExp(" ")),
      (this.C = ""),
      (this.m = new e()),
      (this.w = ""),
      (this.i = new e()),
      (this.u = new e()),
      (this.l = !0),
      (this.A = this.o = this.F = !1),
      (this.G = x.b()),
      (this.s = 0),
      (this.b = new e()),
      (this.B = !1),
      (this.h = ""),
      (this.a = new e()),
      (this.f = []),
      (this.D = n),
      (this.J = this.g = E(this, this.D));
  }
  function E(n, t) {
    var e;
    if (null != t && isNaN(t) && t.toUpperCase() in tn) {
      if (((e = N(n.G, t)), null == e))
        throw Error("Invalid region code: " + t);
      e = g(e, 10);
    } else e = 0;
    return (e = N(n.G, j(e))), null != e ? e : an;
  }
  function B(n) {
    for (var t = n.f.length, e = 0; e < t; ++e) {
      var r = n.f[e],
        u = g(r, 1);
      if (n.w == u) return !1;
      var i;
      i = n;
      var a = r,
        o = g(a, 1);
      if (-1 != o.indexOf("|")) i = !1;
      else {
        (o = o.replace(on, "\\d")), (o = o.replace(sn, "\\d")), l(i.m);
        var s;
        s = i;
        var a = g(a, 2),
          f = "999999999999999".match(o)[0];
        f.length < s.a.b.length
          ? (s = "")
          : ((s = f.replace(new RegExp(o, "g"), a)),
            (s = s.replace(RegExp("9", "g"), " "))),
          0 < s.length ? (i.m.a(s), (i = !0)) : (i = !1);
      }
      if (i) return (n.w = u), (n.B = pn.test(h(r, 4))), (n.s = 0), !0;
    }
    return (n.l = !1);
  }
  function F(n, t) {
    for (var e = [], l = t.length - 3, r = n.f.length, u = 0; u < r; ++u) {
      var i = n.f[u];
      0 == d(i, 3)
        ? e.push(n.f[u])
        : ((i = h(i, 3, Math.min(l, d(i, 3) - 1))),
          0 == t.search(i) && e.push(n.f[u]));
    }
    n.f = e;
  }
  function C(n, t) {
    n.i.a(t);
    var e = t;
    if (rn.test(e) || (1 == n.i.b.length && ln.test(e))) {
      var r,
        e = t;
      "+" == e ? ((r = e), n.u.a(e)) : ((r = en[e]), n.u.a(r), n.a.a(r)),
        (t = r);
    } else (n.l = !1), (n.F = !0);
    if (!n.l) {
      if (!n.F)
        if (P(n)) {
          if (q(n)) return I(n);
        } else if (
          (0 < n.h.length &&
            ((e = n.a.toString()),
            l(n.a),
            n.a.a(n.h),
            n.a.a(e),
            (e = n.b.toString()),
            (r = e.lastIndexOf(n.h)),
            l(n.b),
            n.b.a(e.substring(0, r))),
          n.h != H(n))
        )
          return n.b.a(" "), I(n);
      return n.i.toString();
    }
    switch (n.u.b.length) {
      case 0:
      case 1:
      case 2:
        return n.i.toString();
      case 3:
        if (!P(n)) return (n.h = H(n)), V(n);
        n.A = !0;
      default:
        return n.A
          ? (q(n) && (n.A = !1), n.b.toString() + n.a.toString())
          : 0 < n.f.length
          ? ((e = T(n, t)),
            (r = D(n)),
            0 < r.length
              ? r
              : (F(n, n.a.toString()),
                B(n) ? G(n) : n.l ? M(n, e) : n.i.toString()))
          : V(n);
    }
  }
  function I(n) {
    return (
      (n.l = !0), (n.A = !1), (n.f = []), (n.s = 0), l(n.m), (n.w = ""), V(n)
    );
  }
  function D(n) {
    for (var t = n.a.toString(), e = n.f.length, l = 0; l < e; ++l) {
      var r = n.f[l],
        u = g(r, 1);
      if (new RegExp("^(?:" + u + ")$").test(t))
        return (
          (n.B = pn.test(h(r, 4))),
          (t = t.replace(new RegExp(u, "g"), h(r, 2))),
          M(n, t)
        );
    }
    return "";
  }
  function M(n, t) {
    var e = n.b.b.length;
    return n.B && 0 < e && " " != n.b.toString().charAt(e - 1)
      ? n.b + " " + t
      : n.b + t;
  }
  function V(n) {
    var t = n.a.toString();
    if (3 <= t.length) {
      for (
        var e =
            n.o && 0 == n.h.length && 0 < d(n.g, 20)
              ? c(n.g, 20) || []
              : c(n.g, 19) || [],
          l = e.length,
          r = 0;
        r < l;
        ++r
      ) {
        var u = e[r];
        (0 < n.h.length && A(g(u, 4)) && !h(u, 6) && null == u.a[5]) ||
          ((0 != n.h.length || n.o || A(g(u, 4)) || h(u, 6)) &&
            fn.test(g(u, 2)) &&
            n.f.push(u));
      }
      return (
        F(n, t), (t = D(n)), 0 < t.length ? t : B(n) ? G(n) : n.i.toString()
      );
    }
    return M(n, t);
  }
  function G(n) {
    var t = n.a.toString(),
      e = t.length;
    if (0 < e) {
      for (var l = "", r = 0; r < e; r++) l = T(n, t.charAt(r));
      return n.l ? M(n, l) : n.i.toString();
    }
    return n.b.toString();
  }
  function H(n) {
    var t,
      e = n.a.toString(),
      r = 0;
    return (
      1 != h(n.g, 10)
        ? (t = !1)
        : ((t = n.a.toString()),
          (t = "1" == t.charAt(0) && "0" != t.charAt(1) && "1" != t.charAt(1))),
      t
        ? ((r = 1), n.b.a("1").a(" "), (n.o = !0))
        : null != n.g.a[15] &&
          ((t = new RegExp("^(?:" + h(n.g, 15) + ")")),
          (t = e.match(t)),
          null != t &&
            null != t[0] &&
            0 < t[0].length &&
            ((n.o = !0), (r = t[0].length), n.b.a(e.substring(0, r)))),
      l(n.a),
      n.a.a(e.substring(r)),
      e.substring(0, r)
    );
  }
  function P(n) {
    var t = n.u.toString(),
      e = new RegExp("^(?:\\+|" + h(n.g, 11) + ")"),
      e = t.match(e);
    return (
      null != e &&
      null != e[0] &&
      0 < e[0].length &&
      ((n.o = !0),
      (e = e[0].length),
      l(n.a),
      n.a.a(t.substring(e)),
      l(n.b),
      n.b.a(t.substring(0, e)),
      "+" != t.charAt(0) && n.b.a(" "),
      !0)
    );
  }
  function q(n) {
    if (0 == n.a.b.length) return !1;
    var t,
      r = new e();
    n: {
      if (((t = n.a.toString()), 0 != t.length && "0" != t.charAt(0)))
        for (var u, i = t.length, a = 1; 3 >= a && a <= i; ++a)
          if (((u = parseInt(t.substring(0, a), 10)), u in nn)) {
            r.a(t.substring(a)), (t = u);
            break n;
          }
      t = 0;
    }
    return (
      0 != t &&
      (l(n.a),
      n.a.a(r.toString()),
      (r = j(t)),
      "001" == r ? (n.g = N(n.G, "" + t)) : r != n.D && (n.g = E(n, r)),
      n.b.a("" + t).a(" "),
      (n.h = ""),
      !0)
    );
  }
  function T(n, t) {
    var e = n.m.toString();
    if (0 <= e.substring(n.s).search(n.H)) {
      var r = e.search(n.H),
        e = e.replace(n.H, t);
      return l(n.m), n.m.a(e), (n.s = r), e.substring(0, n.s + 1);
    }
    return 1 == n.f.length && (n.l = !1), (n.w = ""), n.i.toString();
  }
  var U = this;
  (e.prototype.b = ""),
    (e.prototype.set = function (n) {
      this.b = "" + n;
    }),
    (e.prototype.a = function (n, t, e) {
      if (((this.b += String(n)), null != t))
        for (var l = 1; l < arguments.length; l++) this.b += arguments[l];
      return this;
    }),
    (e.prototype.toString = function () {
      return this.b;
    });
  var Y = 1,
    k = 2,
    J = 3,
    K = 4,
    L = 6,
    O = 16,
    Z = 18;
  (f.prototype.set = function (n, t) {
    m(this, n.b, t);
  }),
    (f.prototype.clone = function () {
      var n = new this.constructor();
      return n != this && ((n.a = {}), n.b && (n.b = {}), p(n, this)), n;
    }),
    t(y, f);
  var z = null;
  t(v, f);
  var Q = null;
  t($, f);
  var W = null;
  (y.prototype.j = function () {
    var n = z;
    return (
      n ||
        (z = n =
          b(y, {
            0: { name: "NumberFormat", I: "i18n.phonenumbers.NumberFormat" },
            1: { name: "pattern", required: !0, c: 9, type: String },
            2: { name: "format", required: !0, c: 9, type: String },
            3: { name: "leading_digits_pattern", v: !0, c: 9, type: String },
            4: { name: "national_prefix_formatting_rule", c: 9, type: String },
            6: {
              name: "national_prefix_optional_when_formatting",
              c: 8,
              defaultValue: !1,
              type: Boolean,
            },
            5: {
              name: "domestic_carrier_code_formatting_rule",
              c: 9,
              type: String,
            },
          })),
      n
    );
  }),
    (y.j = y.prototype.j),
    (v.prototype.j = function () {
      var n = Q;
      return (
        n ||
          (Q = n =
            b(v, {
              0: {
                name: "PhoneNumberDesc",
                I: "i18n.phonenumbers.PhoneNumberDesc",
              },
              2: { name: "national_number_pattern", c: 9, type: String },
              9: { name: "possible_length", v: !0, c: 5, type: Number },
              10: {
                name: "possible_length_local_only",
                v: !0,
                c: 5,
                type: Number,
              },
              6: { name: "example_number", c: 9, type: String },
            })),
        n
      );
    }),
    (v.j = v.prototype.j),
    ($.prototype.j = function () {
      var n = W;
      return (
        n ||
          (W = n =
            b($, {
              0: {
                name: "PhoneMetadata",
                I: "i18n.phonenumbers.PhoneMetadata",
              },
              1: { name: "general_desc", c: 11, type: v },
              2: { name: "fixed_line", c: 11, type: v },
              3: { name: "mobile", c: 11, type: v },
              4: { name: "toll_free", c: 11, type: v },
              5: { name: "premium_rate", c: 11, type: v },
              6: { name: "shared_cost", c: 11, type: v },
              7: { name: "personal_number", c: 11, type: v },
              8: { name: "voip", c: 11, type: v },
              21: { name: "pager", c: 11, type: v },
              25: { name: "uan", c: 11, type: v },
              27: { name: "emergency", c: 11, type: v },
              28: { name: "voicemail", c: 11, type: v },
              29: { name: "short_code", c: 11, type: v },
              30: { name: "standard_rate", c: 11, type: v },
              31: { name: "carrier_specific", c: 11, type: v },
              33: { name: "sms_services", c: 11, type: v },
              24: { name: "no_international_dialling", c: 11, type: v },
              9: { name: "id", required: !0, c: 9, type: String },
              10: { name: "country_code", c: 5, type: Number },
              11: { name: "international_prefix", c: 9, type: String },
              17: {
                name: "preferred_international_prefix",
                c: 9,
                type: String,
              },
              12: { name: "national_prefix", c: 9, type: String },
              13: { name: "preferred_extn_prefix", c: 9, type: String },
              15: { name: "national_prefix_for_parsing", c: 9, type: String },
              16: {
                name: "national_prefix_transform_rule",
                c: 9,
                type: String,
              },
              18: {
                name: "same_mobile_and_fixed_line_pattern",
                c: 8,
                defaultValue: !1,
                type: Boolean,
              },
              19: { name: "number_format", v: !0, c: 11, type: y },
              20: { name: "intl_number_format", v: !0, c: 11, type: y },
              22: {
                name: "main_country_for_code",
                c: 8,
                defaultValue: !1,
                type: Boolean,
              },
              23: { name: "leading_digits", c: 9, type: String },
              26: {
                name: "leading_zero_possible",
                c: 8,
                defaultValue: !1,
                type: Boolean,
              },
            })),
        n
      );
    }),
    ($.j = $.prototype.j),
    (_.prototype.a = function (n) {
      throw (new n.b(), Error("Unimplemented"));
    }),
    (_.prototype.b = function (n, t) {
      if (11 == n.a || 10 == n.a)
        return t instanceof f ? t : this.a(n.i.prototype.j(), t);
      if (14 == n.a) {
        if ("string" == typeof t && X.test(t)) {
          var e = Number(t);
          if (0 < e) return e;
        }
        return t;
      }
      if (!n.h) return t;
      if (((e = n.i), e === String)) {
        if ("number" == typeof t) return String(t);
      } else if (
        e === Number &&
        "string" == typeof t &&
        ("Infinity" === t || "-Infinity" === t || "NaN" === t || X.test(t))
      )
        return Number(t);
      return t;
    });
  var X = /^-?[0-9]+$/;
  t(S, _),
    (S.prototype.a = function (n, t) {
      var e = new n.b();
      return (e.g = this), (e.a = t), (e.b = {}), e;
    }),
    t(w, S),
    (w.prototype.b = function (n, t) {
      return 8 == n.a ? !!t : _.prototype.b.apply(this, arguments);
    }),
    (w.prototype.a = function (n, t) {
      return w.M.a.call(this, n, t);
    }); /*

 Copyright (C) 2010 The Libphonenumber Authors

 Licensed under the Apache License, Version 2.0 (the "License");
 you may not use this file except in compliance with the License.
 You may obtain a copy of the License at

 http://www.apache.org/licenses/LICENSE-2.0

 Unless required by applicable law or agreed to in writing, software
 distributed under the License is distributed on an "AS IS" BASIS,
 WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 See the License for the specific language governing permissions and
 limitations under the License.
*/
  var nn = { 54: ["AR"] },
    tn = {
      AR: [
        null,
        [
          null,
          null,
          "(?:11|(?:[2368]|9\\d)\\d)\\d{8}",
          null,
          null,
          null,
          null,
          null,
          null,
          [10, 11],
          [6, 7, 8],
        ],
        [
          null,
          null,
          "11\\d{8}|(?:2(?:2(?:[013]\\d|2[13-79]|4[1-6]|5[2457]|6[124-8]|7[1-4]|8[13-6]|9[1267])|3(?:[07]\\d|1[467]|2[03-6]|3[13-8]|[49][2-6]|5[2-8]|6[013-9])|4(?:7[3-8]|9\\d)|6(?:[01346]\\d|2[24-6]|5[15-8])|80\\d|9(?:[012789]\\d|3[1-6]|4[02-9]|5[234]|6[2-46]))|3(?:3(?:2[79]|6\\d|8[2578])|4(?:0[0124-9]|[1-357]\\d|4[24-7]|6[02-9]|8[0-79]|9[1236-8])|5(?:[138]\\d|2[1245]|4[1-9]|6[2-4]|7[1-6])|6[24]\\d|7(?:[069]\\d|1[1568]|2[013-9]|3[145]|4[0-35-9]|5[14-8]|7[2-57]|8[0-24-9])|8(?:[01578]\\d|2[15-7]|3[0-24-9]|4[13-6]|6[1-357-9]|9[124]))|670\\d)\\d{6}",
          null,
          null,
          null,
          "1123456789",
          null,
          null,
          [10],
          [6, 7, 8],
        ],
        [
          null,
          null,
          "675\\d{7}|9(?:11[2-9]\\d{7}|(?:2(?:2[013]|3[067]|49|6[01346]|80|9[147-9])|3(?:36|4[1-358]|5[138]|6[24]|7[069]|8[013578]))[2-9]\\d{6}|(?:2(?:2(?:02|2[13-79]|4[1-6]|5[2457]|6[124-8]|7[1-4]|8[13-6]|9[1267])|3(?:02|1[467]|2[03-6]|3[13-8]|[49][2-6]|5[2-8])|47[3-578]|6(?:2[24-6]|4[6-8]|5[15-8])|9(?:0[1-3]|2\\d|3[1-6]|4[02568]|5[2-4]|6[2-46]|72|8[23]))|3(?:3(?:2[79]|8[2578])|4(?:0[0-24-9]|4[24-7]|6[02-9]|7[126]|9[1-36-8])|5(?:2[1245]|3[237]|4[1-46-9]|6[2-4]|7[1-6]|8[2-5])|7(?:1[1568]|2[15]|3[145]|4[13]|5[14-8]|7[2-57]|8[126])|8(?:2[15-7]|3[2578]|4[13-6]|5[4-8]|6[1-357-9]|9[124])))[2-9]\\d{5})",
          null,
          null,
          null,
          "91123456789",
          null,
          null,
          null,
          [6, 7, 8],
        ],
        [
          null,
          null,
          "800\\d{7}",
          null,
          null,
          null,
          "8001234567",
          null,
          null,
          [10],
        ],
        [
          null,
          null,
          "60[04579]\\d{7}",
          null,
          null,
          null,
          "6001234567",
          null,
          null,
          [10],
        ],
        [null, null, null, null, null, null, null, null, null, [-1]],
        [null, null, null, null, null, null, null, null, null, [-1]],
        [null, null, null, null, null, null, null, null, null, [-1]],
        "AR",
        54,
        "00",
        "0",
        null,
        null,
        "0?(?:(11|2(?:2(?:02?|[13]|2[13-79]|4[1-6]|5[2457]|6[124-8]|7[1-4]|8[13-6]|9[1267])|3(?:02?|1[467]|2[03-6]|3[13-8]|[49][2-6]|5[2-8]|[67])|4(?:7[3-578]|9)|6(?:[0136]|2[24-6]|4[6-8]?|5[15-8])|80|9(?:0[1-3]|[19]|2\\d|3[1-6]|4[02568]?|5[2-4]|6[2-46]|72?|8[23]?))|3(?:3(?:2[79]|6|8[2578])|4(?:0[0-24-9]|[12]|3[5-8]?|4[24-7]|5[4-68]?|6[02-9]|7[126]|8[2379]?|9[1-36-8])|5(?:1|2[1245]|3[237]?|4[1-46-9]|6[2-4]|7[1-6]|8[2-5]?)|6[24]|7(?:[069]|1[1568]|2[15]|3[145]|4[13]|5[14-8]|7[2-57]|8[126])|8(?:[01]|2[15-7]|3[2578]?|4[13-6]|5[4-8]?|6[1-357-9]|7[36-8]?|8[5-8]?|9[124])))?15)?",
        "9$1",
        null,
        null,
        [
          [null, "(\\d{3})(\\d{3})(\\d{4})", "$1-$2-$3", ["[68]"], "0$1"],
          [null, "(\\d{2})(\\d{4})", "$1-$2", ["[2-9]"], "$1"],
          [null, "(\\d{3})(\\d{4})", "$1-$2", ["[2-9]"], "$1"],
          [null, "(\\d{4})(\\d{4})", "$1-$2", ["[2-9]"], "$1"],
          [
            null,
            "(\\d)(\\d{2})(\\d{4})(\\d{4})",
            "$2 15-$3-$4",
            ["911"],
            "0$1",
          ],
          [
            null,
            "(\\d)(\\d{3})(\\d{3})(\\d{4})",
            "$2 15-$3-$4",
            [
              "9(?:2[2-4689]|3[3-8])",
              "9(?:2(?:2[013]|3[067]|49|6[01346]|8|9[147-9])|3(?:36|4[1-358]|5[138]|6|7[069]|8[013578]))",
              "9(?:2(?:2(?:0[013-9]|[13])|3(?:0[013-9]|[67])|49|6(?:[0136]|4[0-59])|8|9(?:[19]|44|7[013-9]|8[14]))|3(?:36|4(?:[12]|3[4-6]|[58]4)|5(?:1|3[0-24-689]|8[46])|6|7[069]|8(?:[01]|34|[578][45])))",
              "9(?:2(?:2(?:0[013-9]|[13])|3(?:0[013-9]|[67])|49|6(?:[0136]|4[0-59])|8|9(?:[19]|44|7[013-9]|8[14]))|3(?:36|4(?:[12]|3(?:4|5[014]|6[1-39])|[58]4)|5(?:1|3[0-24-689]|8[46])|6|7[069]|8(?:[01]|34|[578][45])))",
            ],
            "0$1",
          ],
          [
            null,
            "(\\d)(\\d{4})(\\d{2})(\\d{4})",
            "$2 15-$3-$4",
            ["9[23]"],
            "0$1",
          ],
          [
            null,
            "(\\d{2})(\\d{4})(\\d{4})",
            "$1 $2-$3",
            ["11"],
            "0$1",
            null,
            1,
          ],
          [
            null,
            "(\\d{3})(\\d{3})(\\d{4})",
            "$1 $2-$3",
            [
              "2(?:2[013]|3[067]|49|6[01346]|8|9[147-9])|3(?:36|4[1-358]|5[138]|6|7[069]|8[013578])",
              "2(?:2(?:0[013-9]|[13])|3(?:0[013-9]|[67])|49|6(?:[0136]|4[0-59])|8|9(?:[19]|44|7[013-9]|8[14]))|3(?:36|4(?:[12]|3[4-6]|[58]4)|5(?:1|3[0-24-689]|8[46])|6|7[069]|8(?:[01]|34|[578][45]))",
              "2(?:2(?:0[013-9]|[13])|3(?:0[013-9]|[67])|49|6(?:[0136]|4[0-59])|8|9(?:[19]|44|7[013-9]|8[14]))|3(?:36|4(?:[12]|3(?:4|5[014]|6[1-39])|[58]4)|5(?:1|3[0-24-689]|8[46])|6|7[069]|8(?:[01]|34|[578][45]))",
            ],
            "0$1",
            null,
            1,
          ],
          [
            null,
            "(\\d{4})(\\d{2})(\\d{4})",
            "$1 $2-$3",
            ["[23]"],
            "0$1",
            null,
            1,
          ],
          [null, "(\\d{3})", "$1", ["1[0-2]|911"], "$1"],
        ],
        [
          [null, "(\\d{3})(\\d{3})(\\d{4})", "$1-$2-$3", ["[68]"], "0$1"],
          [null, "(\\d)(\\d{2})(\\d{4})(\\d{4})", "$1 $2 $3-$4", ["911"]],
          [
            null,
            "(\\d)(\\d{3})(\\d{3})(\\d{4})",
            "$1 $2 $3-$4",
            [
              "9(?:2[2-4689]|3[3-8])",
              "9(?:2(?:2[013]|3[067]|49|6[01346]|8|9[147-9])|3(?:36|4[1-358]|5[138]|6|7[069]|8[013578]))",
              "9(?:2(?:2(?:0[013-9]|[13])|3(?:0[013-9]|[67])|49|6(?:[0136]|4[0-59])|8|9(?:[19]|44|7[013-9]|8[14]))|3(?:36|4(?:[12]|3[4-6]|[58]4)|5(?:1|3[0-24-689]|8[46])|6|7[069]|8(?:[01]|34|[578][45])))",
              "9(?:2(?:2(?:0[013-9]|[13])|3(?:0[013-9]|[67])|49|6(?:[0136]|4[0-59])|8|9(?:[19]|44|7[013-9]|8[14]))|3(?:36|4(?:[12]|3(?:4|5[014]|6[1-39])|[58]4)|5(?:1|3[0-24-689]|8[46])|6|7[069]|8(?:[01]|34|[578][45])))",
            ],
          ],
          [null, "(\\d)(\\d{4})(\\d{2})(\\d{4})", "$1 $2 $3-$4", ["9[23]"]],
          [
            null,
            "(\\d{2})(\\d{4})(\\d{4})",
            "$1 $2-$3",
            ["11"],
            "0$1",
            null,
            1,
          ],
          [
            null,
            "(\\d{3})(\\d{3})(\\d{4})",
            "$1 $2-$3",
            [
              "2(?:2[013]|3[067]|49|6[01346]|8|9[147-9])|3(?:36|4[1-358]|5[138]|6|7[069]|8[013578])",
              "2(?:2(?:0[013-9]|[13])|3(?:0[013-9]|[67])|49|6(?:[0136]|4[0-59])|8|9(?:[19]|44|7[013-9]|8[14]))|3(?:36|4(?:[12]|3[4-6]|[58]4)|5(?:1|3[0-24-689]|8[46])|6|7[069]|8(?:[01]|34|[578][45]))",
              "2(?:2(?:0[013-9]|[13])|3(?:0[013-9]|[67])|49|6(?:[0136]|4[0-59])|8|9(?:[19]|44|7[013-9]|8[14]))|3(?:36|4(?:[12]|3(?:4|5[014]|6[1-39])|[58]4)|5(?:1|3[0-24-689]|8[46])|6|7[069]|8(?:[01]|34|[578][45]))",
            ],
            "0$1",
            null,
            1,
          ],
          [
            null,
            "(\\d{4})(\\d{2})(\\d{4})",
            "$1 $2-$3",
            ["[23]"],
            "0$1",
            null,
            1,
          ],
        ],
        [null, null, null, null, null, null, null, null, null, [-1]],
        null,
        null,
        [null, null, "810\\d{7}", null, null, null, null, null, null, [10]],
        [
          null,
          null,
          "810\\d{7}",
          null,
          null,
          null,
          "8101234567",
          null,
          null,
          [10],
        ],
        null,
        null,
        [null, null, null, null, null, null, null, null, null, [-1]],
      ],
    };
  x.b = function () {
    return x.a ? x.a : (x.a = new x());
  };
  var en = {
      0: "0",
      1: "1",
      2: "2",
      3: "3",
      4: "4",
      5: "5",
      6: "6",
      7: "7",
      8: "8",
      9: "9",
      "０": "0",
      "１": "1",
      "２": "2",
      "３": "3",
      "４": "4",
      "５": "5",
      "６": "6",
      "７": "7",
      "８": "8",
      "９": "9",
      "٠": "0",
      "١": "1",
      "٢": "2",
      "٣": "3",
      "٤": "4",
      "٥": "5",
      "٦": "6",
      "٧": "7",
      "٨": "8",
      "٩": "9",
      "۰": "0",
      "۱": "1",
      "۲": "2",
      "۳": "3",
      "۴": "4",
      "۵": "5",
      "۶": "6",
      "۷": "7",
      "۸": "8",
      "۹": "9",
    },
    ln = RegExp("[+＋]+"),
    rn = RegExp("([0-9０-９٠-٩۰-۹])"),
    un = /^\(?\$1\)?$/,
    an = new $();
  m(an, 11, "NA");
  var on = /\[([^\[\]])*\]/g,
    sn = /\d(?=[^,}][^,}])/g,
    fn = RegExp(
      "^[-x‐-―−ー－-／  ­​⁠　()（）［］.\\[\\]/~⁓∼～]*(\\$\\d[-x‐-―−ー－-／  ­​⁠　()（）［］.\\[\\]/~⁓∼～]*)+$"
    ),
    pn = /[- ]/;
  (R.prototype.K = function () {
    (this.C = ""),
      l(this.i),
      l(this.u),
      l(this.m),
      (this.s = 0),
      (this.w = ""),
      l(this.b),
      (this.h = ""),
      l(this.a),
      (this.l = !0),
      (this.A = this.o = this.F = !1),
      (this.f = []),
      (this.B = !1),
      this.g != this.J && (this.g = E(this, this.D));
  }),
    (R.prototype.L = function (n) {
      return (this.C = C(this, n));
    }),
    n("Cleave.AsYouTypeFormatter", R),
    n("Cleave.AsYouTypeFormatter.prototype.inputDigit", R.prototype.L),
    n("Cleave.AsYouTypeFormatter.prototype.clear", R.prototype.K);
}.call("object" == typeof global && global ? global : window);
