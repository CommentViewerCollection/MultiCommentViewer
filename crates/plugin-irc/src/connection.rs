use uuid::Uuid;

pub struct Connection {
    id: Uuid,
}
impl Connection {
    pub fn new(conn_id: &Uuid) -> Self {
        Self { id: *conn_id }
    }
    pub fn stop(&mut self) {}
}
