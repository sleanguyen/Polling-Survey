import axios from 'axios'
import { mockPolls, mockResults, createMockPoll } from './mockData.js'

const USE_MOCK = true

const http = axios.create({
  baseURL: '/api',
  timeout: 8000,
  headers: { 'Content-Type': 'application/json' }
})

// ── Polls ──────────────────────────────────────

export async function createPoll(payload) {
  if (USE_MOCK) { await delay(400); return createMockPoll(payload) }
  const { data } = await http.post('/polls', payload)
  return data
}

export async function getPoll(code) {
  if (USE_MOCK) {
    await delay(300)
    const poll = mockPolls.find(p => p.code === code)
    if (!poll) throw new Error('Poll not found')
    return poll
  }
  const { data } = await http.get(`/polls/${code}`)
  return data
}

export async function getPollResults(code) {
  if (USE_MOCK) {
    await delay(300)
    const result = mockResults[code]
    if (!result) throw new Error('Results not found')
    return result
  }
  const { data } = await http.get(`/polls/${code}/results`)
  return data
}

// ── Votes ──────────────────────────────────────

export async function submitVote(code, payload) {
  if (USE_MOCK) {
    await delay(500)
    const result = mockResults[code]
    if (result && payload.optionIndex !== undefined) {
      result.options[payload.optionIndex].votes += 1
      result.totalVotes += 1
    }
    return { success: true }
  }
  const { data } = await http.post(`/polls/${code}/vote`, payload)
  return data
}

// Merit: open_text response submission
export async function submitOpenText(code, payload) {
  if (USE_MOCK) {
    await delay(400)
    const result = mockResults[code]
    if (result) {
      if (!result.responses) result.responses = []
      result.responses.push({ text: payload.text, submittedAt: new Date().toISOString() })
      result.totalVotes += 1
    }
    return { success: true }
  }
  const { data } = await http.post(`/polls/${code}/respond`, payload)
  return data
}

// ── Creator ────────────────────────────────────

export async function closePoll(code) {
  if (USE_MOCK) {
    await delay(300)
    const poll = mockPolls.find(p => p.code === code)
    if (poll) poll.status = 'closed'
    if (mockResults[code]) mockResults[code].status = 'closed'
    return { success: true }
  }
  const { data } = await http.patch(`/polls/${code}/close`)
  return data
}

function delay(ms) { return new Promise(r => setTimeout(r, ms)) }
