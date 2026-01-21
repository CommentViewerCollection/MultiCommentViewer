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
5. **Comment Posting**: `send-comment` → post to platform → result via existing messages

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
├── mcv-tracing/          # Plugin tracing integration (auto log forwarding to Core)
├── mcv-logger/           # Core logging system (SQLite + remote sending)
├── mcv-core/             # Core logic (CoreActor, PluginManager, ConnectionManager)
└── plugin-dummy/         # Dummy plugin for testing and development
```

### Comment Posting System

The comment posting system uses the `send-comment` message type for posting comments to streaming platforms.

**Message Type:**
- `send-comment`: Core → Plugin with comment text

**Behavior:**
- **Real streaming plugins**: Post the comment to the streaming platform, results via existing messages (`connected`, `disconnected`, etc.)
- **DummyPlugin**: Reuses this for command execution to simulate various scenarios

**DummyPlugin Commands (via send-comment):**
- `disconnect` - Simulate server-side disconnection
- `connect` - Request reconnection (requires UI action)
- `pause` - Pause comment generation
- `resume` - Resume comment generation
- `rate <seconds>` - Set comment generation interval (0 = random)
- `comment <user> <text>` - Generate manual comment
- `help` - Show available commands
- `status` - Show connection status

**UI Implementation:**
The main UI includes a comment posting section below the DataGrid with:
- Connection selector (combobox)
- Comment/command input field
- Send button

### Multiple Connection Management

Plugins manage multiple connections independently using `HashMap<Uuid, Arc<AtomicBool>>`:

```rust
pub struct DummyPlugin {
    plugin_id: Uuid,
    connections: HashMap<Uuid, Arc<AtomicBool>>,      // connection_id → running flag
    comment_rates: HashMap<Uuid, Arc<RwLock<u64>>>,   // connection_id → rate
    paused: HashMap<Uuid, Arc<AtomicBool>>,           // connection_id → paused flag
}
```

This ensures that operations on one connection (pause, disconnect, rate change) do not affect other connections.

### Plugin Logging System (mcv-tracing)

Plugins can use standard tracing macros (`tracing::error!()`, `tracing::warn!()`, etc.) for logging, which are automatically forwarded to Core via LogEntry messages and integrated with mcv-logger.

**Setup in plugin:**
```rust
use mcv_tracing;

impl Plugin for MyPlugin {
    async fn on_loaded(&mut self, host: Arc<dyn PluginHost>) -> Result<(), PluginError> {
        // Initialize tracing
        mcv_tracing::init_tracing(
            self.plugin_id,
            Arc::clone(&host),
            env!("CARGO_PKG_VERSION"),
            "info", // or "debug", "trace", etc.
        )?;

        // Now you can use tracing macros
        tracing::info!("Plugin loaded successfully");

        // ... rest of initialization
    }
}
```

**Usage in plugin code:**
```rust
// Structured logging with fields
tracing::debug!(connection_id = %conn_id, "Processing message");
tracing::info!(user = %username, "User connected");
tracing::warn!(count = connections.len(), "High connection count");
tracing::error!(error = %e, "Failed to process request");
```

**Features:**
- Automatic forwarding to Core via LogEntry messages
- Structured logging support (fields, spans)
- Works in plugin dependencies (any crate used by the plugin)
- Error-level logs automatically capture stack traces
- Integration with mcv-logger (SQLite storage + remote sending)

**Log-error/log-warn commands:**
- The DummyPlugin's `log-error`, `log-warn`, `log-info`, `log-debug` commands are separate test features
- They manually construct LogEntry messages for testing purposes
- mcv-tracing provides automatic logging for production use

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
- Tauri commands: `add_connection`, `remove_connection`, `rename_connection`, `connect`, `disconnect`, `get_connections`, `send_comment`
- Events emitted to frontend: `comment-received`, `connected`, `disconnected`
- Core actor events are forwarded to React UI via Tauri's event system

### Frontend Structure

React + TypeScript + Tailwind CSS:
- `src/App.tsx`: Main component with connection controls and comment display
- Uses `@tauri-apps/api` for backend communication
- Listens to Tauri events for real-time comment updates
- Uses `my-dataview` package (in `packages/`) for high-performance comment display with virtual scrolling
- Comment posting section for posting comments (DummyPlugin uses this for commands)

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

1. User clicks "接続を追加" → `add_connection` command → Creates connection with default name (#1, #2, etc.)
2. User clicks "接続" button → `connect` command → Plugin starts comment generation
3. User clicks "切断" button → `disconnect` command → Plugin stops comment generation
4. User can rename connections, and the name persists in the connection manager
5. User can delete connections (only when disconnected)

### Event-Driven Architecture

The application uses an event-driven architecture to avoid polling:
- Backend emits `connected`, `disconnected`, and `comment-received` events
- Frontend listens to these events and updates UI accordingly
- No polling intervals or timers are used for connection state synchronization

## Current Limitations (MVP)

- No dynamic plugin loading (.dll/.so)
- No browser management/cookie extraction
- No complex input UI (URL/password fields)
- Single dummy plugin for testing
- No persistent storage or configuration

## Test Coverage

The project includes comprehensive tests for core functionality:

### Unit Tests
- **mcv-messages** (4 tests): Message serialization, payload validation, send-comment tests
- **plugin-dummy** (11 tests): Command handling, connection management, pause/resume/rate control
- **mcv-core** (3 tests): Plugin manager, connection manager, basic lifecycle tests

### Integration Tests
- **mcv-core/tests** (5 tests): Comment posting routing, connection lifecycle, multiple connections, rename/delete operations

Run tests with:
```bash
# All tests
cargo test --workspace

# Specific package
cargo test --package mcv-messages
cargo test --package plugin-dummy
cargo test --package mcv-core

# Integration tests only
cargo test --test command_system_test
```

## Future Extension Points

See `docs/specifications.md` for detailed specifications including:
- Real streaming platform plugins (YouTube Live, Twitch, Niconico)
- Browser cookie integration
- Plugin distribution system
- Comment posting (bidirectional communication)
- Bouyomi-chan TTS integration
- Comment delay adjustment
