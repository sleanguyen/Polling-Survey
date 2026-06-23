import { defineStore } from 'pinia'
import { ref } from 'vue'
import { createPoll, getPoll, getPollResults, submitVote, closePoll } from '@/api/pollApi.js'

export const usePollStore = defineStore('poll', () => {
  const currentPoll    = ref(null)
  const currentResults = ref(null)
  const loading        = ref(false)
  const error          = ref(null)
  const hasVoted       = ref(false)

  // ── Actions ────────────────────────────────────

  async function fetchPoll(code) {
    loading.value = true
    error.value = null
    try {
      currentPoll.value = await getPoll(code)
      // Check if user already voted (stored in sessionStorage)
      hasVoted.value = !!sessionStorage.getItem(`voted_${code}`)
    } catch (e) {
      error.value = e.message
    } finally {
      loading.value = false
    }
  }

  async function fetchResults(code) {
    loading.value = true
    error.value = null
    try {
      currentResults.value = await getPollResults(code)
    } catch (e) {
      error.value = e.message
    } finally {
      loading.value = false
    }
  }

  async function vote(code, optionIndex) {
    const voterToken = getOrCreateVoterToken()
    await submitVote(code, { optionIndex, voterToken })
    sessionStorage.setItem(`voted_${code}`, '1')
    hasVoted.value = true
    // Refresh results after voting
    await fetchResults(code)
  }

  async function create(payload) {
    loading.value = true
    error.value = null
    try {
      const poll = await createPoll(payload)
      return poll
    } catch (e) {
      error.value = e.message
      return null
    } finally {
      loading.value = false
    }
  }

  async function close(code) {
    await closePoll(code)
    if (currentPoll.value) currentPoll.value.status = 'closed'
  }

  // Called by SignalR when a live vote comes in
  function applyLiveVoteUpdate(data) {
    if (!currentResults.value) return
    if (currentResults.value.pollCode !== data.pollCode) return
    const option = currentResults.value.options[data.optionIndex]
    if (option) option.votes += 1
    currentResults.value.totalVotes += 1
  }

  // ── Helpers ────────────────────────────────────
  function getOrCreateVoterToken() {
    let token = localStorage.getItem('voterToken')
    if (!token) {
      token = crypto.randomUUID()
      localStorage.setItem('voterToken', token)
    }
    return token
  }

  return {
    currentPoll, currentResults, loading, error, hasVoted,
    fetchPoll, fetchResults, vote, create, close, applyLiveVoteUpdate
  }
})
