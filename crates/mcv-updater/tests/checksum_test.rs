use mcv_updater::UpdateChecker;
use tokio::io::AsyncWriteExt;

#[tokio::test]
async fn test_checksum_verification() {
    // テスト用の一時ファイルを作成
    let temp_dir = std::env::temp_dir();
    let test_file = temp_dir.join("mcv_updater_test.txt");

    // テストデータを書き込み
    let test_data = b"Hello, MultiCommentViewer!";
    let mut file = tokio::fs::File::create(&test_file).await.unwrap();
    file.write_all(test_data).await.unwrap();
    file.flush().await.unwrap();
    drop(file);

    // 正しいSHA-256ハッシュ（実際のファイルから計算された値）
    let expected_hash = "b72c5e9435cb62b3349c60c44373e686a8fa364638b336602b7bb4933f407ce4";

    let updater = UpdateChecker::new("https://api.example.com");

    // チェックサム検証
    let result = updater.verify_checksum(&test_file, expected_hash).await;
    assert!(result.is_ok(), "Checksum verification should succeed");
    assert_eq!(result.unwrap(), true);

    // 間違ったハッシュでテスト
    let wrong_hash = "0000000000000000000000000000000000000000000000000000000000000000";
    let result = updater.verify_checksum(&test_file, wrong_hash).await;
    assert!(
        result.is_err(),
        "Checksum verification should fail with wrong hash"
    );

    // クリーンアップ
    tokio::fs::remove_file(&test_file).await.ok();

    println!("Checksum verification test passed!");
}

#[tokio::test]
async fn test_checksum_with_empty_file() {
    let temp_dir = std::env::temp_dir();
    let test_file = temp_dir.join("mcv_updater_empty.txt");

    // 空のファイルを作成
    tokio::fs::File::create(&test_file).await.unwrap();

    // 空ファイルのSHA-256ハッシュ
    // echo -n "" | sha256sum
    // e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855
    let expected_hash = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";

    let updater = UpdateChecker::new("https://api.example.com");
    let result = updater.verify_checksum(&test_file, expected_hash).await;

    assert!(
        result.is_ok(),
        "Empty file checksum verification should succeed"
    );
    assert_eq!(result.unwrap(), true);

    // クリーンアップ
    tokio::fs::remove_file(&test_file).await.ok();

    println!("Empty file checksum test passed!");
}
