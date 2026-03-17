#[allow(dead_code)]
pub struct VideoId {
    value: String,
}
#[allow(dead_code)]
impl VideoId {
    pub fn new(value: &str) -> Self {
        VideoId {
            value: value.to_string(),
        }
    }
    pub fn value(&self) -> &str {
        &self.value
    }
}
