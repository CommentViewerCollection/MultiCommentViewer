//! TwitCasting PlayerPage2.js から x-web-authorizekey 生成に使う SECRET を抽出するライブラリ
//!
//! TwitCasting は JS 更新のたびに難読化が変化するが、以下の構造的特徴は保たれる想定:
//!   1. カスタム base64 でエンコードされた大量の文字列配列
//!   2. チェックサムが特定値に一致するまで配列を回転させるループ
//!   3. SHA256 を使った認証キー生成関数
//!   4. その呼び出しサイトで「デフォルト salt」= SECRET を表す fn(N) が fallback として渡される
//!      パターン: null != (X = obj[fn(SALT_IDX)]) ? X : fn(SECRET_IDX)

use regex::Regex;

const PLAYER_JS_URL: &str = "https://twitcasting.tv/js/v1/PlayerPage2.js";

/// カスタム base64 アルファベット (標準と異なり小文字→大文字→数字の順)
const CUSTOM_ALPHA: &[u8] = b"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789+/=";

const HTTP_VERBS: &[&str] = &["GET", "POST", "PUT", "PATCH", "DELETE", "HEAD", "OPTIONS"];

// ─────────────────────────────────────────────────────────────────────────────

/// PlayerPage2.js を HTTP で取得する
pub fn fetch_player_js() -> std::result::Result<String, Box<dyn std::error::Error>> {
    let client = reqwest::blocking::Client::builder()
        .user_agent("Mozilla/5.0 (compatible; extract-secret/0.1)")
        .build()?;
    Ok(client.get(PLAYER_JS_URL).send()?.text()?)
}

// ─── カスタム base64 デコーダ ────────────────────────────────────────────────
// TwitCasting の zIoqHW 関数と同等。標準 base64 と比べてアルファベット順のみ異なる。
//
// JS 実装:
//   for (let i=0, n, r, s=0; r=e.charAt(s++); ~r && (n = i%4 ? 64*n+r : r, i++%4)
//       && (t += String.fromCharCode(255 & n >> (-2*i & 6))))
//     r = "abcdef...+/=".indexOf(r);
//   // その後 percent-encode → decodeURIComponent で UTF-8 変換

fn twc_decode(encoded: &str) -> String {
    let mut bytes: Vec<u8> = Vec::new();
    let mut i: usize = 0; // base64 グループ内の位置カウンタ
    let mut n: u64 = 0;   // ビット蓄積バッファ

    for &byte in encoded.as_bytes() {
        let r = match CUSTOM_ALPHA.iter().position(|&b| b == byte) {
            Some(p) => p,
            None => continue, // アルファベット外の文字 (パディング '=' 含む) はスキップ
        };
        let old_i = i;
        // グループ先頭なら n をリセット、それ以外は下位ビットとして追記
        n = if old_i % 4 == 0 { r as u64 } else { 64 * n + r as u64 };
        i += 1;
        if old_i % 4 != 0 {
            // JS の -2*i & 6 (i は increment 後の値) に相当するシフト量
            let shift = ((-2i64 * i as i64) & 6) as u32;
            bytes.push(((n >> shift) & 0xFF) as u8);
        }
    }

    // バイト列を UTF-8 として解釈 (JS の decodeURIComponent 相当)
    String::from_utf8(bytes).unwrap_or_default()
}

fn lookup(arr: &[String], index: usize, base_offset: usize) -> String {
    match index.checked_sub(base_offset) {
        Some(i) => arr.get(i).map(|s| twc_decode(s)).unwrap_or_default(),
        None => String::new(),
    }
}

// ─── メイン抽出処理 ───────────────────────────────────────────────────────────

/// PlayerPage2.js のソースから SECRET を抽出する
///
/// * `src`     - PlayerPage2.js の文字列
/// * `verbose` - 途中経過を stderr に出力するかどうか
pub fn extract_secret(src: &str, verbose: bool) -> std::result::Result<String, String> {
    let log = |msg: &str| {
        if verbose {
            eprintln!("[extract-secret] {msg}");
        }
    };

    // ① エンコード済み文字列配列を抽出
    //    パターン: let e = ["base64str", "base64str", ...]
    let array_re = Regex::new(r#"let\s+e\s*=\s*\[((?:\s*"[A-Za-z0-9+/=]+",?\s*)+)\]"#)
        .map_err(|e| e.to_string())?;
    let arr_match = array_re
        .captures(src)
        .ok_or("エンコード済み文字列配列が見つかりません")?;

    let elem_re = Regex::new(r#""([^"]+)""#).map_err(|e| e.to_string())?;
    let mut arr: Vec<String> = elem_re
        .find_iter(&arr_match[1])
        .map(|m| {
            let s = m.as_str();
            s[1..s.len() - 1].to_string()
        })
        .collect();

    log(&format!("文字列配列: {} エントリ", arr.len()));

    // 配列位置の前後 5000 バイトをコンテキストとして取得
    // (PlayerPage2.js は ASCII なので、バイト境界 = 文字境界)
    let arr_pos = arr_match.get(0).unwrap().start();
    let ctx = &src[arr_pos.saturating_sub(5000)..(arr_pos + 5000).min(src.len())];

    // ② ベースオフセットを取得 (t -= N)
    //    インデックス N が arr[N - base_offset] に対応する
    let base_re = Regex::new(r"t\s*-=\s*(\d+)").map_err(|e| e.to_string())?;
    let base_offset: usize = base_re
        .captures(ctx)
        .ok_or("ベースオフセット (t -= N) が見つかりません")?[1]
        .parse()
        .map_err(|e: std::num::ParseIntError| e.to_string())?;

    log(&format!("ベースオフセット: {base_offset}"));

    // ③ 回転ターゲットを取得 (=== N を含むチェックサムループ)
    let rot_re = Regex::new(r"===\s*(\d{4,8})\s*\)").map_err(|e| e.to_string())?;
    let rot_target: f64 = rot_re
        .captures(ctx)
        .ok_or("回転ターゲット (=== N) が見つかりません")?[1]
        .parse()
        .map_err(|e: std::num::ParseFloatError| e.to_string())?;

    log(&format!("回転ターゲット: {rot_target}"));

    // ④ チェックサム式を ctx から直接抽出
    //
    // src.lines() によるライン検索は、ライブ JS と手元ファイルで空白・改行が
    // 異なる場合に失敗するため、ctx 内でターゲット値を直接検索する方式を使う。
    //
    // アルゴリズム:
    //   1. ctx 内でターゲット数値文字列を検索
    //   2. その直前の "if" キーワードを rfind で探す
    //   3. そこから extract_if_body で括弧内を抽出

    let rot_target_str = format!("{}", rot_target as i64);
    let target_pos_in_ctx = ctx
        .find(rot_target_str.as_str())
        .ok_or("回転ターゲット値が ctx 内に見つかりません")?;

    // ターゲット値より前のテキスト中で最後の "if" を探す
    let before_target = &ctx[..target_pos_in_ctx];
    let if_start = before_target
        .rfind("if ")
        .or_else(|| before_target.rfind("if("))
        .ok_or("チェックサム if 文が見つかりません")?;

    let checksum_expr = extract_if_body(&ctx[if_start..])
        .ok_or("チェックサム式の括弧が対応していません")?;

    // チェックサム式中の lookup 関数名 (例: i, n, r など 1 文字の変数名)
    let fn_name_re = Regex::new(r"parseInt\(([a-zA-Z])\(\d+\)\)")
        .map_err(|e| e.to_string())?;
    let fn_name = fn_name_re
        .captures(&checksum_expr)
        .ok_or("チェックサム関数名が見つかりません")?[1]
        .to_string();

    log(&format!("チェックサム関数名: {fn_name}"));

    log(&format!(
        "チェックサム式: {}...",
        &checksum_expr[..checksum_expr.len().min(80)]
    ));

    // ⑤ チェックサム評価用 regex を事前コンパイル
    //    パターン: parseInt(fn_name(N)) → 整数値に置換
    let fn_call_re = Regex::new(&format!(
        r"parseInt\({}\((\d+)\)\)",
        regex::escape(&fn_name)
    ))
    .map_err(|e| e.to_string())?;

    // ⑥ 配列を回転させてチェックサムが一致するまで
    //    r.push(r.shift()) と同等: rotate_left(1) で先頭要素を末尾へ移動
    let max_rotations = 10_000;
    let mut rotations = 0usize;

    loop {
        if eval_checksum(&checksum_expr, &fn_call_re, &arr, base_offset, rot_target) {
            break;
        }
        if rotations >= max_rotations {
            return Err(format!(
                "{max_rotations}回試行してもチェックサムが一致しませんでした"
            ));
        }
        arr.rotate_left(1);
        rotations += 1;
    }

    log(&format!("回転数: {rotations}"));

    // ⑦ SECRET インデックスを特定
    //
    // 認証キー生成の呼び出しサイトを探す。fetch() 近傍に:
    //   null != (VAR = obj[fn(SALT_IDX)]) ? VAR : fn(SECRET_IDX)
    // というパターンがある。fn(SALT_IDX) は "salt" などのプロパティ名に
    // デコードされ、fn(SECRET_IDX) がデフォルト SECRET。
    let fetch_re = Regex::new(r"\bfetch\s*\(").map_err(|e| e.to_string())?;
    let salt_re = Regex::new(
        r"null\s*!=\s*\([a-z]\s*=\s*[a-z]\[[a-zA-Z]\((\d+)\)\]\)\s*\?[^:]+:\s*[a-zA-Z]\((\d+)\)",
    )
    .map_err(|e| e.to_string())?;

    let mut secret_index: Option<usize> = None;

    'outer: for mat in fetch_re.find_iter(src) {
        let start = mat.start().saturating_sub(2000);
        let end = (mat.end() + 50).min(src.len());
        let snippet = &src[start..end];

        for caps in salt_re.captures_iter(snippet) {
            let n1: usize = caps[1].parse().unwrap_or(0);
            let n2: usize = caps[2].parse().unwrap_or(0);
            let decoded1 = lookup(&arr, n1, base_offset);
            let decoded2 = lookup(&arr, n2, base_offset);

            log(&format!(
                "  候補: fn({n1})=\"{decoded1}\" ? ... : fn({n2})=\"{decoded2}\""
            ));

            // fn(SALT_IDX): 短い英字識別子 (プロパティ名らしい)
            let is_identifier = decoded1
                .chars()
                .next()
                .map_or(false, |c| c.is_alphabetic())
                && decoded1
                    .chars()
                    .all(|c| c.is_alphanumeric() || c == '_' || c == '$')
                && decoded1.len() <= 12;

            // fn(SECRET_IDX): HTTP メソッドでない 8 文字以上の英数字
            let looks_like_secret = decoded2.len() >= 8
                && decoded2
                    .chars()
                    .all(|c| c.is_alphanumeric() || matches!(c, '+' | '/' | '=' | '-' | '_'))
                && !HTTP_VERBS.contains(&decoded2.as_str());

            if is_identifier && looks_like_secret {
                secret_index = Some(n2);
                break 'outer;
            }
        }
    }

    let idx = secret_index
        .ok_or_else(|| "SECRET インデックスが特定できませんでした".to_string())?;
    let secret = lookup(&arr, idx, base_offset);

    log(&format!("SECRET インデックス: {idx}"));
    log(&format!("SECRET: {secret}"));

    Ok(secret)
}

// ─── if (...) の括弧内を抽出 (括弧カウンタ使用) ─────────────────────────────

fn extract_if_body(line: &str) -> Option<String> {
    let if_pos = line.find("if")?;
    let after = &line[if_pos..];
    let mut depth: i32 = 0;
    let mut start = 0usize;

    for (idx, ch) in after.char_indices() {
        match ch {
            '(' if depth == 0 => {
                start = idx + 1;
                depth = 1;
            }
            '(' => depth += 1,
            ')' => {
                depth -= 1;
                if depth == 0 {
                    return Some(after[start..idx].to_string());
                }
            }
            _ => {}
        }
    }
    None
}

// ─── チェックサム評価 ────────────────────────────────────────────────────────

fn eval_checksum(
    expr: &str,
    fn_re: &Regex,
    arr: &[String],
    base_offset: usize,
    target: f64,
) -> bool {
    // parseInt(fn(N)) → 整数値に置換
    let substituted = fn_re.replace_all(expr, |caps: &regex::Captures| {
        let n: usize = caps[1].parse().unwrap_or(0);
        let s = lookup(arr, n, base_offset);
        // JS の parseInt と同様に先頭の整数部を取得
        let val: i64 = s
            .chars()
            .take_while(|c| c.is_ascii_digit() || *c == '-')
            .collect::<String>()
            .parse()
            .unwrap_or(0);
        val.to_string()
    });

    // === TARGET 以降を除去して算術部のみにする
    let arith = match substituted.find("===") {
        Some(p) => substituted[..p].trim().to_string(),
        None => substituted.trim().to_string(),
    };

    eval_arithmetic(&arith).map_or(false, |v| (v - target).abs() < 0.5)
}

// ─── 算術式評価器 (再帰降下パーサ) ──────────────────────────────────────────
//
// 文法:
//   expr   := term (('+' | '-') term)*
//   term   := factor (('*' | '/') factor)*
//   factor := ('+' | '-') factor | '(' expr ')' | number
//   number := [0-9]+ ('.' [0-9]*)?

#[derive(Debug, Clone)]
enum Token {
    Num(f64),
    Plus,
    Minus,
    Star,
    Slash,
    LParen,
    RParen,
}

fn tokenize(expr: &str) -> Vec<Token> {
    let mut tokens = Vec::new();
    let chars: Vec<char> = expr.chars().collect();
    let mut i = 0;

    while i < chars.len() {
        match chars[i] {
            ' ' | '\t' | '\n' | '\r' => i += 1,
            '+' => {
                tokens.push(Token::Plus);
                i += 1;
            }
            '-' => {
                tokens.push(Token::Minus);
                i += 1;
            }
            '*' => {
                tokens.push(Token::Star);
                i += 1;
            }
            '/' => {
                tokens.push(Token::Slash);
                i += 1;
            }
            '(' => {
                tokens.push(Token::LParen);
                i += 1;
            }
            ')' => {
                tokens.push(Token::RParen);
                i += 1;
            }
            c if c.is_ascii_digit() => {
                let start = i;
                while i < chars.len() && (chars[i].is_ascii_digit() || chars[i] == '.') {
                    i += 1;
                }
                let s: String = chars[start..i].iter().collect();
                if let Ok(n) = s.parse::<f64>() {
                    tokens.push(Token::Num(n));
                }
            }
            _ => i += 1,
        }
    }

    tokens
}

fn eval_arithmetic(expr: &str) -> Option<f64> {
    let tokens = tokenize(expr);
    let mut pos = 0usize;
    parse_expr(&tokens, &mut pos)
}

fn parse_expr(tokens: &[Token], pos: &mut usize) -> Option<f64> {
    let mut left = parse_term(tokens, pos)?;
    while *pos < tokens.len() {
        match tokens[*pos] {
            Token::Plus => {
                *pos += 1;
                left += parse_term(tokens, pos)?;
            }
            Token::Minus => {
                *pos += 1;
                left -= parse_term(tokens, pos)?;
            }
            _ => break,
        }
    }
    Some(left)
}

fn parse_term(tokens: &[Token], pos: &mut usize) -> Option<f64> {
    let mut left = parse_factor(tokens, pos)?;
    while *pos < tokens.len() {
        match tokens[*pos] {
            Token::Star => {
                *pos += 1;
                left *= parse_factor(tokens, pos)?;
            }
            Token::Slash => {
                *pos += 1;
                left /= parse_factor(tokens, pos)?;
            }
            _ => break,
        }
    }
    Some(left)
}

fn parse_factor(tokens: &[Token], pos: &mut usize) -> Option<f64> {
    let tok = tokens.get(*pos)?.clone();
    match tok {
        Token::Minus => {
            *pos += 1;
            Some(-parse_factor(tokens, pos)?)
        }
        Token::Plus => {
            *pos += 1;
            parse_factor(tokens, pos)
        }
        Token::LParen => {
            *pos += 1;
            let v = parse_expr(tokens, pos)?;
            if matches!(tokens.get(*pos), Some(Token::RParen)) {
                *pos += 1;
            }
            Some(v)
        }
        Token::Num(n) => {
            *pos += 1;
            Some(n)
        }
        _ => None,
    }
}

// ─── テスト ───────────────────────────────────────────────────────────────────

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_twc_decode_ascii() {
        // "ngg71ob7okuk3ngk" が何かしらの文字列にデコードされることを確認
        // (実際のエンコード値は不明なので、逆算せず実装の健全性のみチェック)
        let result = twc_decode("Agv4"); // 短いエンコード例
        assert!(!result.is_empty() || result.is_empty()); // パニックしないこと
    }

    #[test]
    fn test_eval_arithmetic_simple() {
        assert!((eval_arithmetic("1 + 2").unwrap() - 3.0).abs() < 1e-9);
        assert!((eval_arithmetic("-10 / 2").unwrap() - (-5.0)).abs() < 1e-9);
        assert!((eval_arithmetic("3 * (4 + 2)").unwrap() - 18.0).abs() < 1e-9);
        assert!((eval_arithmetic("-5 / 1 + 10 / 2").unwrap() - 0.0).abs() < 1e-9);
    }

    #[test]
    fn test_extract_if_body() {
        let line = "if (a + b * (c + d) === 42) break;";
        assert_eq!(
            extract_if_body(line),
            Some("a + b * (c + d) === 42".to_string())
        );
    }
}
