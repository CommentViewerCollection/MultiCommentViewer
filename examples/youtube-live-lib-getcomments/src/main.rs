use anyhow::Result;
use clap::Parser;
use std::path::PathBuf;
use youtube_live_lib::{
    Continuation, Vid, extract_ytcfg, get_live_chat, get_live_chat_messages, get_yt_initial_data,
};
#[derive(Parser, Debug)]
struct Args {
    /// Name of the person to greet
    #[arg(long)]
    vid: String,
}
#[tokio::main]
async fn main() {
    let args = Args::parse();
    let vid = Vid::new(args.vid);
    if let Err(e) = get_comments(&vid).await {
        eprintln!("Error: {}", e);
    }
}
async fn get_comments(vid: &Vid) -> Result<()> {
    let local_app_data = std::env::var("LOCALAPPDATA").expect("Failed to get LOCALAPPDATA");
    let log_db_path = PathBuf::from(local_app_data)
        .join("MultiCommentViewer")
        .join("logs.db");
    // ログディレクトリを作成
    if let Some(parent) = log_db_path.parent() {
        std::fs::create_dir_all(parent).expect("Failed to create log directory");
    }

    mcv_log_core::init_logger(&log_db_path, env!("CARGO_PKG_VERSION"))
        .expect("Failed to initialize logger");

    let live_chat = get_live_chat(vid, &[]).await?;
    let yt_initial_data = get_yt_initial_data(&live_chat)?;
    let continuation = yt_initial_data.continuation();
    let _initial_actions = yt_initial_data.actions();
    let ytcfg = extract_ytcfg(&live_chat)?;

    let mut next_continuation: Continuation = continuation.to_owned();
    loop {
        match get_live_chat_messages(vid, &ytcfg, &next_continuation).await {
            Ok((g, _actions, _raw_body)) => {
                if let Some(c) = g {
                    next_continuation = c;
                } else {
                    println!("No more continuation.");
                    break;
                }
            }
            Err(e) => {
                eprintln!("Error getting live chat messages: {}", e);
                break;
            }
        }
        tokio::time::sleep(std::time::Duration::from_secs(5)).await;
    }
    Ok(())
}
