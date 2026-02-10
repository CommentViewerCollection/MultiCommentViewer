pub struct AuthToken {
    token: String,
}
impl AuthToken {
    pub fn new(token: &str) -> Self {
        AuthToken {
            token: token.to_string(),
        }
    }
    pub fn value(&self) -> &str {
        &self.token
    }
}