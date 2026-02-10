pub struct ClientId {
    client_id: String,
}
impl ClientId {
    pub fn new(client_id: &str) -> Self {
        ClientId {
            client_id: client_id.to_string(),
        }
    }
    pub fn value(&self) -> &str {
        &self.client_id
    }
}
