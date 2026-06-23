import axios from 'axios'
import { mockPolls, mockResults, createMockPoll } from './mockData.js'

// Toggle this to false once the real backend is running
const USE_MOCK = true

const http = axios.create({
  baseURL: '/api',
  timeout: 8000,
  headers: { 'Content-Type': 'application/json' }
})

// ── Polls ──────────────────────────────────────

/**
 * POST /api/polls
 * @param {{ question: string, options: string[], expiresAt?: string }} payload
 */
export async function createPoll(payload) {
  if (USE_MOCK) {
    await delay(400)
    return createMockPoll(payload)
  }
  const { data } = await http.post('/polls', payload)
  return data
}

/**
 * GET /api/polls/{code}
 */
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

/**
 * GET /api/polls/{code}/results
 */
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

/**
 * POST /api/polls/{code}/vote
 * @param {string} code
 * @param {{ optionIndex: number, voterToken: string }} payload
 */
export async function submitVote(code, payload) {
  if (USE_MOCK) {
    await delay(500)
    // Mutate mock results so the chart updates
    const result = mockResults[code]
    if (result) {
      result.options[payload.optionIndex].votes += 1
      result.totalVotes += 1
    }
    return { success: true }
  }
  const { data } = await http.post(`/polls/${code}/vote`, payload)
  return data
}

// ── Creator actions ────────────────────────────

/**
 * PATCH /api/polls/{code}/close
 */
export async function closePoll(code) {
  if (USE_MOCK) {
    await delay(300)
    const poll = mockPolls.find(p => p.code === code)
    if (poll) poll.status = 'closed'
    return { success: true }
  }
  const { data } = await http.patch(`/polls/${code}/close`)
  return data
}

// ── Helpers ────────────────────────────────────
function delay(ms) {
  return new Promise(resolve => setTimeout(resolve, ms))
}
