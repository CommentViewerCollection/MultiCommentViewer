pub enum RuntimeEvent {
    Loaded,
    Message(Vec<u8>),
    Shutdown,
}