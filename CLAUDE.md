# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

MultiCommentViewer (mcv) is a comment viewer application that aggregates comments from multiple live streaming platforms simultaneously. This is an MVP implementation with a Tauri + React frontend and Rust + actix backend using an actor-based plugin architecture.

## Development Commands

### Initial Setup
```bash
# Install Rust dependencies (from project root)
cargo build

# Install frontend dependencies
cd apps/mcv
npm install
```

### Running the Application
```bash
# Development mode with hot reload (from apps/mcv)
npm run tauri dev
```

### Building and Testing
```bash
# Check Rust code without building
cargo check

# Build Rust workspace
cargo build

# Build for release
cargo build --release

# Run Rust tests
cargo test

# Build frontend
cd apps/mcv
npm run build
```

## Architecture Overview

### Core Concepts

- **core**: Main application handling UI management, plugin management, and comment aggregation
- **plugin**: Modular components for specific streaming platform support
- **plugin-host**: Actor-based isolation environment for plugins using actix
- **connection**: Instance representing a connection to a single streaming URL

### Message Flow (Actor Model)

The system uses actix's actor model for core-plugin communication:

1. **Plugin Registration**: `plugin-hello` → `plugin-added` broadcast
2. **Connection Lifecycle**: `add-connection` → `connection-added` → `connect` → `connected`
3. **Comment Streaming**: Plugins send `comment-received` messages to core
4. **Disconnection**: `disconnect` → `disconnected`

All messages follow kebab-case naming convention and include:
- `type`: Message type (e.g., "add-connection")
- `src`: Source (Core or Plugin with UUID)
- `dst`: Destination (Core or Plugin with UUID)
- `request_id`: Optional UUID for request-response pairing
- `timestamp`: Unix timestamp
- `payload`: JSON value with message-specific data

### Crate Structure

```
crates/
├── mcv-messages/          # Message type definitions (MessageType, payloads)
├── mcv-common/            # Shared utilities and constants
├── mcv-plugin-interface/  # Plugin trait definitions (Plugin, PluginHost)
└── mcv-core/             # Core logic (CoreActor, PluginManager, ConnectionManager)
```

### Plugin Implementation

Plugins must implement the `Plugin` trait:
```rust
#[async_trait]
pub trait Plugin: Send + Sync {
    async fn on_loaded(&mut self, host: &dyn PluginHost) -> Result<(), PluginError>;
    async fn on_message(&mut self, message: McvMessage, host: &dyn PluginHost) -> Result<(), PluginError>;
    async fn on_shutdown(&mut self) -> Result<(), PluginError>;
}
```

- Current implementation uses **static linking** for plugins (not dynamic loading)
- Each plugin runs in its own `PluginHostActor` for isolation
- Plugins communicate with core via `PluginHost::send_message()`

### Tauri Integration

- `apps/mcv/src-tauri/src/main.rs`: Initializes actix system, CoreActor, and registers plugins
- Tauri commands: `start_connection`, `add_connection`, `connect`, `disconnect`
- Events emitted to frontend: `comment-received`, `connected`, `disconnected`
- Core actor events are forwarded to React UI via Tauri's event system

### Frontend Structure

React + TypeScript + Tailwind CSS:
- `src/App.tsx`: Main component with connection controls and comment display
- Uses `@tauri-apps/api` for backend communication
- Listens to Tauri events for real-time comment updates

## Important Implementation Details

### Actix Message Naming Conflict

Be careful with actix's `Message` trait vs `mcv_messages::Message` struct:
```rust
// Always use explicit imports to avoid conflicts
use actix::prelude::*;  // Brings in actix::Message trait
use mcv_messages::{Message as McvMessage, MessageSource, MessageDestination, MessageType, *};
```

### Plugin Lifecycle

1. Plugin registered via `PluginManager::register_plugin()`
2. `PluginHostActor` created and started
3. `Plugin::on_loaded()` called → sends `plugin-hello`
4. Core responds with `plugin-added`
5. Plugin ready to handle connection messages

### Connection Flow

MVP simplification: `start_connection` command combines `add-connection` + `connect` for easier UI interaction.

## Current Limitations (MVP)

- No dynamic plugin loading (.dll/.so)
- No browser management/cookie extraction
- No complex input UI (URL/password fields)
- Single dummy plugin for testing
- No persistent storage or configuration

## Future Extension Points

See `docs/specifications.md` for detailed specifications including:
- Real streaming platform plugins (YouTube Live, Twitch, Niconico)
- Browser cookie integration
- Plugin distribution system
- Comment posting (bidirectional communication)
- Bouyomi-chan TTS integration
- Comment delay adjustment
