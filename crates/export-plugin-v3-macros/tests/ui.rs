#[test]
fn export_plugin_v3_async_works() {
    trybuild::TestCases::new().pass("tests/ui/ok_basic.rs");
}
