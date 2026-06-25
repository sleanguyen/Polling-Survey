import * as signalR from '@microsoft/signalr'

// Toggle to false when the real backend hub is running
const USE_MOCK = true

let connection = null
const listeners = {}

// ── Helpers ────────────────────────────────────────────────────────────────

function emit(event, data) {
  if (listeners[event]) listeners[event].forEach(fn => fn(data))
}

// ── Mock tools (USE_MOCK = true) ───────────────────────────────────────────

if (typeof window !== 'undefined') {
  // Simulate a vote:   window.__mockVote('7fGh2', 2)
  window.__mockVote = (pollCode, optionIndex) => {
    emit('VoteUpdated', { pollCode, optionIndex })
    console.info(`[SignalR MOCK] VoteUpdated — poll:${pollCode} option:${optionIndex}`)
  }
  // Simulate poll closed:  window.__mockClose('7fGh2')
  window.__mockClose = (pollCode) => {
    emit('PollClosed', { pollCode })
    console.info(`[SignalR MOCK] PollClosed — poll:${pollCode}`)
  }
}

// ── Public API ─────────────────────────────────────────────────────────────

export async function connectToPoll(pollCode) {
  if (USE_MOCK) {
    console.info(`[SignalR MOCK] Ready for poll "${pollCode}".`)
    console.info(`  → window.__mockVote('${pollCode}', optionIndex)  to simulate a vote`)
    console.info(`  → window.__mockClose('${pollCode}')               to close the poll`)
    return
  }

  // ── Real SignalR connection ──────────────────────────────────────────────
  if (connection) await connection.stop()

  connection = new signalR.HubConnectionBuilder()
    .withUrl('/pollHub')
    .withAutomaticReconnect([0, 1000, 3000, 5000])   // retry intervals ms
    .configureLogging(signalR.LogLevel.Warning)
    .build()

  // Hub → client events
  connection.on('VoteUpdated', (data) => emit('VoteUpdated', data))
  connection.on('PollClosed',  (data) => emit('PollClosed',  data))

  // Reconnection lifecycle
  connection.onreconnecting(() => emit('ConnectionState', { state: 'reconnecting' }))
  connection.onreconnected(() => {
    emit('ConnectionState', { state: 'connected' })
    connection.invoke('JoinPoll', pollCode).catch(console.error)
  })
  connection.onclose(() => emit('ConnectionState', { state: 'disconnected' }))

  await connection.start()
  await connection.invoke('JoinPoll', pollCode)
  emit('ConnectionState', { state: 'connected' })
  console.info(`[SignalR] Connected to poll "${pollCode}"`)
}

export async function disconnect() {
  if (USE_MOCK) return
  if (connection) {
    await connection.stop()
    connection = null
  }
}

export function onEvent(event, handler) {
  if (!listeners[event]) listeners[event] = []
  listeners[event].push(handler)
}

export function offEvent(event, handler) {
  if (listeners[event])
    listeners[event] = listeners[event].filter(fn => fn !== handler)
}
