# Real-Time Polling App

A live polling application built with ASP.NET Core and SignalR that enables presenters to create interactive polls and see results update in real-time as audience members vote.

## Features

- **Instant Poll Creation**: Create polls with 2-4 answer options and receive a unique 4-digit room code
- **Real-Time Results**: Watch vote counts and percentages update live via WebSocket connections
- **Mobile-Friendly**: Responsive design works seamlessly on phones, tablets, and desktops
- **Vote Deduplication**: Browser fingerprinting prevents duplicate votes from the same device
- **Poll Lifecycle Management**: Presenters can close polls to stop accepting new votes
- **No Signup Required**: Anonymous voting with simple room code sharing

## Screenshots

### Landing Page
The home page provides quick access to create or join a poll.

### Presenter View
Create a new poll by entering a question and 2-4 answer options. After creation, you'll receive a shareable room code.

### Voter View
Voters enter the 4-digit room code to join a poll and select their answer with a single tap.

### Live Dashboard
Real-time results display with animated bar charts showing vote counts and percentages as they come in.

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later

### Running the Application

1. Clone the repository:
   ```bash
   git clone <repository-url>
   cd 1-the-real-time-polling-app-focus-signalr-websockets
   ```

2. Run the application:
   ```bash
   dotnet run
   ```

3. Open your browser and navigate to:
   ```
   https://localhost:5001
   ```
   or
   ```
   http://localhost:5000
   ```

### Running Tests

```bash
dotnet test
```

## Usage Guide

### For Presenters

1. **Create a Poll**
   - Navigate to the "Create Poll" page
   - Enter your question (e.g., "What's your favorite programming language?")
   - Add 2-4 answer options
   - Click "Create Poll"

2. **Share the Room Code**
   - After creation, you'll see a 4-digit room code (e.g., `1234`)
   - Share this code with your audience verbally or display it on screen
   - Alternatively, share the direct voter link

3. **View Live Results**
   - Click "View Live Results" to open the dashboard
   - Watch as votes come in with real-time bar chart updates
   - The connection status indicator shows if you're receiving live updates

4. **Close the Poll**
   - When voting is complete, click "Close Poll"
   - This prevents any new votes from being submitted
   - Results remain visible on the dashboard

### For Voters

1. **Join a Poll**
   - Navigate to the "Join Poll" page
   - Enter the 4-digit room code provided by the presenter
   - Or use a direct link: `https://yoursite.com/voter.html?room=1234`

2. **Cast Your Vote**
   - Review the question and options
   - Click on your preferred answer
   - You'll see a confirmation that your vote was recorded

3. **One Vote Per Person**
   - Each browser can only vote once per poll
   - If you try to vote again, you'll see an "Already Voted" message

## Tech Stack

| Technology | Purpose |
|------------|---------|
| **ASP.NET Core 8** | Backend web framework |
| **SignalR** | Real-time WebSocket communication |
| **Minimal APIs** | Lightweight HTTP endpoints |
| **In-Memory Storage** | Poll and vote data storage (development) |
| **HTML/CSS/JavaScript** | Frontend UI (no framework dependencies) |

## Project Structure

```
├── Dtos/                    # Data transfer objects
│   ├── CreatePollRequest.cs
│   ├── CreatePollResponse.cs
│   ├── GetPollResponse.cs
│   ├── VoteRequest.cs
│   └── VoteUpdatePayload.cs
├── Hubs/
│   └── PollHub.cs           # SignalR hub for real-time updates
├── Middleware/
│   └── GlobalExceptionHandler.cs
├── Models/
│   ├── Poll.cs              # Poll domain model
│   └── PollOption.cs        # Poll option domain model
├── Repositories/
│   ├── IPollRepository.cs   # Repository interface
│   └── InMemoryPollRepository.cs
├── Services/
│   └── RoomCodeGenerator.cs # Unique room code generation
├── wwwroot/
│   ├── css/
│   │   └── styles.css       # Shared styles
│   ├── index.html           # Landing page
│   ├── presenter.html       # Poll creation page
│   ├── voter.html           # Voting page
│   ├── dashboard.html       # Live results page
│   └── favicon.svg          # App icon
├── Tests/                   # Unit and integration tests
└── Program.cs               # Application entry point
```

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/polls` | Create a new poll |
| `GET` | `/api/polls/{roomCode}` | Get poll details |
| `POST` | `/api/polls/{roomCode}/vote` | Cast a vote |
| `POST` | `/api/polls/{roomCode}/close` | Close a poll |

### SignalR Hub

Connect to `/hubs/poll` for real-time updates:

- **JoinRoom(roomCode)**: Subscribe to updates for a specific poll
- **VoteUpdated**: Received when a new vote is cast
- **PollClosed**: Received when the poll is closed

## Development Notes

- Polls are stored in memory and will be lost when the application restarts
- Room codes are 4-digit numbers generated randomly with collision checking
- Vote deduplication uses a browser-generated UUID stored in localStorage
- SignalR automatically handles reconnection with exponential backoff

## License

MIT License - feel free to use this project as a starting point for your own applications.
