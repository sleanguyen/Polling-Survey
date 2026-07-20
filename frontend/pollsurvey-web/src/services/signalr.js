import * as signalR from '@microsoft/signalr'

let connection = null
const listeners = {}

function emit(event, data) {
  if (listeners[event]) listeners[event].forEach(fn => fn(data))
}

export async function connectToPoll(pollCode) {
  if (connection) await connection.stop()

  const hubUrl = import.meta.env.VITE_API_BASE_URL
    ? `${import.meta.env.VITE_API_BASE_URL}/pollHub`
    : '/pollHub'

  connection = new signalR.HubConnectionBuilder()
    .withUrl(hubUrl)
    .withAutomaticReconnect([0, 1000, 3000, 5000])
    .configureLogging(signalR.LogLevel.Warning)
    .build()

  connection.onreconnecting(() => emit('ConnectionState', { state: 'reconnecting' }))
  connection.onreconnected(() => {
    emit('ConnectionState', { state: 'connected' })
    connection.invoke('JoinPoll', pollCode).catch(console.error)
  })
  connection.onclose(() => emit('ConnectionState', { state: 'disconnected' }))

  await connection.start()
  await connection.invoke('JoinPoll', pollCode)
  emit('ConnectionState', { state: 'connected' })
}

export async function disconnect(pollCode) {
  if (!connection) return
  try {
    if (pollCode) await connection.invoke('LeavePoll', pollCode)
  } catch { /* connection may already be closed */ }
  await connection.stop()
  connection = null
}

export function onEvent(event, handler) {
  if (!listeners[event]) listeners[event] = []
  listeners[event].push(handler)
}

export function offEvent(event, handler) {
  if (listeners[event])
    listeners[event] = listeners[event].filter(fn => fn !== handler)
}
