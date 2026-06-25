import axios from 'axios'

const http = axios.create({
    baseURL: '/api',
    timeout: 8000,
    headers: { 'Content-Type': 'application/json' }
})

// POST /api/polls
export async function createPoll(payload) {
    const request = {
        title: payload.question,
        expiresAt: payload.expiresAt || null,
        questions: [
            {
                text: payload.question,
                type: 'multiple_choice',
                order: 1,
                options: payload.options.map((o, index) => ({
                    text: o,
                    order: index + 1
                }))
            }
        ]
    }
    const { data } = await http.post('/polls', request)
    return data
}

// GET /api/polls/{code}
export async function getPoll(code) {
    const { data } = await http.get(`/polls/${code}`)
    return data
}

// GET /api/polls/{code}/results
export async function getPollResults(code) {
    const { data } = await http.get(`/polls/${code}/results`)
    return data
}

// POST /api/polls/{code}/vote
// BE nhận: { questionId, optionId, voterToken }
export async function submitVote(code, payload) {
    const { data } = await http.post(`/polls/${code}/vote`, payload)
    return data
}

// PATCH /api/polls/{code}/close
export async function closePoll(code) {
    const { data } = await http.patch(`/polls/${code}/close`)
    return data
}