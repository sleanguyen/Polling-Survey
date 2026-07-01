# PollSurvey — Frontend

Vue 3 SPA for the PollSurvey project (AMD201 Group Assignment).

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | Vue 3 + Vite |
| State | Pinia |
| Routing | Vue Router 4 |
| Charts | Chart.js + vue-chartjs |
| Real-time | @microsoft/signalr |
| HTTP | axios |
| Styling | CSS custom properties (no framework) |
| Fonts | Playfair Display + Plus Jakarta Sans |

## Architecture

```
Browser (Vue SPA)
    │
    ├── HTTP (axios)  →  ASP.NET Core Web API  →  SQL Server
    │       /api/polls, /api/polls/:code, /api/polls/:code/vote ...
    │
    └── WebSocket (SignalR)  →  PollHub
            JoinPoll(code)
            ← ReceivePollUpdate (full result object after each vote)
```

## Project Structure

```
src/
├── api/
│   └── pollApi.js        # All HTTP calls to backend
├── services/
│   └── signalr.js        # SignalR hub connection
├── stores/
│   └── pollStore.js      # Pinia store (state + actions)
├── router/
│   └── index.js          # Vue Router routes
├── views/
│   ├── HomeView.vue      # Landing page with demo polls
│   ├── CreatePollView.vue# Poll creation form
│   ├── PollView.vue      # Voting page (all question types)
│   ├── ResultsView.vue   # Live results with SignalR
│   └── AnalyticsView.vue # Distinction: analytics dashboard
├── components/
│   ├── Navbar.vue
│   └── VoteChart.vue     # Chart.js bar chart
└── assets/
    └── main.css          # Design tokens + global styles
```

## Local Development

**Prerequisites:** Node.js 18+, backend running on `localhost:5000`

```bash
cd frontend/pollsurvey-web
npm install
npm run dev
# → http://localhost:5173
```

Vite proxies `/api` and `/pollHub` to `http://localhost:5000` automatically.

## Environment Variables

| Variable | Description | Example |
|---|---|---|
| `VITE_API_BASE_URL` | Backend base URL (production only) | `https://your-app.railway.app` |

In development, leave this unset — the Vite proxy handles routing.

## Deploy to Vercel

1. Push code to GitHub
2. Import repo at [vercel.com](https://vercel.com)
3. Set environment variable: `VITE_API_BASE_URL` = your Railway backend URL
4. Deploy — Vercel auto-detects Vite and runs `npm run build`

> **Important:** After backend is deployed on Railway, update `VITE_API_BASE_URL`
> in Vercel project settings → Redeploy.

## Docker (local / CI)

```bash
# Build
docker build -t pollsurvey-fe .

# Run
docker run -p 8080:80 pollsurvey-fe

# Open http://localhost:8080
```

Multi-stage build: Node 20 (build) → nginx:alpine (serve). Final image ~25 MB.

## API Contract Summary

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/polls` | Create poll |
| `GET` | `/api/polls/:code` | Get poll info |
| `POST` | `/api/polls/:code/vote` | Submit vote |
| `GET` | `/api/polls/:code/results` | Get results |
| `PATCH` | `/api/polls/:code/close` | Close poll |

SignalR Hub: `/pollHub`
- Client calls: `JoinPoll(code)`, `LeavePoll(code)`
- Server broadcasts: `ReceivePollUpdate(PollResultResponse)` after each vote
