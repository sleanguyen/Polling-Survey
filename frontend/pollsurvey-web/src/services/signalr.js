import * as signalR from '@microsoft/signalr'

// Toggle to false when the real backend hub is running
const USE_MOCK = true

let connection = null
const listeners = {}

// ── Mock event bus (used when USE_MOCK = true) ─────────────────────────────

function emit(event, data) {
  if (listeners[event]) listeners[event].forEach(fn => fn(data))
}

/** Call this from DevTools console to simulate a live vote:
 *  window.__mockVote('7fGh2', 2)
 */
if (typeof window !== 'undefined') {
  window.__mockVote = (pollCode, optionIndex) => {
    emit('VoteUpdated', { pollCode, optionIndex, totalVotes: Math.floor(Math.random() * 100) })
  }
}

// ── Public API ─────────────────────────────────────────────────────────────

/**
 * Connect to the SignalR hub for a specific poll.
 * In mock mode, just registers the mock bus.
 */
export async function connectToPoll(pollCode) {
  if (USE_MOCK) {
    console.info(`[SignalR MOCK] Connected to poll ${pollCode}. Use window.__mockVote('${pollCode}', optionIndex) to simulate votes.`)
    return
  }

  if (connection) {
    await connection.stop()
  }

  connection = new signalR.HubConnectionBuilder()
    .withUrl('/pollHub')
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Warning)
    .build()

  connection.on('VoteUpdated', (data) => emit('VoteUpdated', data))
  connection.on('PollClosed', (data) => emit('PollClosed', data))

  await connection.start()
  await connection.invoke('JoinPoll', pollCode)
  console.info(`[SignalR] Connected to poll ${pollCode}`)
}

/**
 * Disconnect from the current hub connection.
 */
export async function disconnect() {
  if (USE_MOCK) return
  if (connection) {
    await connection.stop()
    connection = null
  }
}

/**
 * Register a handler for a SignalR event.
 * @param {'VoteUpdated'|'PollClosed'} event
 * @param {Function} handler
 */
export function onEvent(event, handler) {
  if (!listeners[event]) listeners[event] = []
  listeners[event].push(handler)
}

/**
 * Remove a handler for a SignalR event.
 */
export function offEvent(event, handler) {
  if (listeners[event]) {
    listeners[event] = listeners[event].filter(fn => fn !== handler)
  }
}
