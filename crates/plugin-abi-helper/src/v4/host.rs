use crate::abi::v4::HostRuntimev4;

pub struct Host<'a> {
    pub raw: &'a HostRuntimev4,
}

impl<'a> Host<'a> {
    pub fn log(&self, level: i32, msg: &str) {
        unsafe {
            (self.raw.log)(level, msg.as_ptr(), msg.len());
        }
    }

    pub fn send_message(&self, json: &[u8]) {
        unsafe {
            (self.raw.send_message)(json.as_ptr(), json.len());
        }
    }
}