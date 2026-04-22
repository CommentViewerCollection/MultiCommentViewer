/// ビルドプロファイルを取得
///
/// feature flags に基づいてビルドプロファイルを返します。
/// 優先順位: alpha > beta > stable > unknown
pub fn get_build_profile() -> String {
    #[cfg(feature = "alpha")]
    return "alpha".to_string();

    #[cfg(all(feature = "beta", not(feature = "alpha")))]
    return "beta".to_string();

    #[cfg(all(not(feature = "alpha"), not(feature = "beta"), feature = "stable"))]
    return "stable".to_string();

    // フィーチャーフラグが指定されていない場合
    #[cfg(all(not(feature = "alpha"), not(feature = "beta"), not(feature = "stable")))]
    "unknown".to_string()
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_build_profile() {
        let profile = get_build_profile();
        assert!(!profile.is_empty());
        // いずれかのプロファイルを返す
        assert!(["alpha", "beta", "stable", "unknown"].contains(&profile.as_str()));
    }
}
