import { defineStore } from 'pinia'
import { ref } from 'vue'
import { createPoll, getPoll, getPollResults, submitVote, closePoll } from '@/api/pollApi.js'

export const usePollStore = defineStore('poll', () => {
    const currentPoll = ref(null)
    const currentResults = ref(null)
    const loading = ref(false)
    const error = ref(null)
    const hasVoted = ref(false)

    async function fetchPoll(code) {
        loading.value = true
        error.value = null
        try {
            currentPoll.value = await getPoll(code)
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

    // optionId: Guid của option được chọn
    // questionId: lấy từ questions[0] vì hiện tại mỗi poll 1 question
    async function vote(code, optionId) {
        const voterToken = getOrCreateVoterToken()
        const questionId = currentPoll.value?.questions?.[0]?.id

        if (!questionId) throw new Error('Question ID not found')

        await submitVote(code, {
            questionId,
            optionId,
            voterToken
        })

        sessionStorage.setItem(`voted_${code}`, '1')
        hasVoted.value = true
        await fetchResults(code)
    }

    async function create(payload) {
        loading.value = true
        error.value = null
        try {
            return await createPoll(payload)
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
        if (currentResults.value) currentResults.value.status = 'closed'
    }

    // SignalR broadcast từ BE gửi lên PollResultResponse mới
    // Thay vì cộng dồn từng vote, replace thẳng bằng data mới để đồng bộ chính xác
    function applyLiveVoteUpdate(data) {
        if (!currentResults.value) return
        if (currentResults.value.code !== data.code) return
        currentResults.value = data
    }

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