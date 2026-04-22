//! TwitCasting PlayerPage2.js から x-web-authorizekey 生成に使う SECRET を抽出するツール
//!
//! 使い方:
//!   extract-secret
//!   extract-secret --verbose

fn main() {
    let verbose = std::env::args().any(|a| a == "--verbose" || a == "-v");

    let src = twicas_secret_extractor::fetch_player_js().unwrap_or_else(|e| {
        eprintln!("エラー: PlayerPage2.js の取得に失敗: {e}");
        std::process::exit(1);
    });

    match twicas_secret_extractor::extract_secret(&src, verbose) {
        Ok(secret) => println!("{secret}"),
        Err(e) => {
            eprintln!("エラー: {e}");
            std::process::exit(1);
        }
    }
}
