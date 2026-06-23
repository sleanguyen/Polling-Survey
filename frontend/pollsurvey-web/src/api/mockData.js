// ─────────────────────────────────────────────
//  MOCK DATA — matches the backend API contract
//  Replace with real API calls once backend is ready
// ─────────────────────────────────────────────

export const mockPolls = [
  {
    id: 1,
    code: '7fGh2',
    question: 'What is your favourite programming language?',
    options: ['JavaScript', 'Python', 'C#', 'Go', 'Rust'],
    status: 'open',
    createdAt: '2025-06-01T08:00:00Z',
    expiresAt: null,
    totalVotes: 42
  },
  {
    id: 2,
    code: 'aB3xQ',
    question: 'Which frontend framework do you prefer?',
    options: ['Vue', 'React', 'Angular', 'Svelte'],
    status: 'open',
    createdAt: '2025-06-02T10:30:00Z',
    expiresAt: '2025-06-10T10:30:00Z',
    totalVotes: 28
  },
  {
    id: 3,
    code: 'zT9mK',
    question: 'How often do you write unit tests?',
    options: ['Always', 'Usually', 'Sometimes', 'Rarely', 'Never'],
    status: 'closed',
    createdAt: '2025-05-28T14:00:00Z',
    expiresAt: null,
    totalVotes: 75
  }
]

// GET /api/polls/{code}/results
export const mockResults = {
  '7fGh2': {
    pollCode: '7fGh2',
    question: 'What is your favourite programming language?',
    status: 'open',
    totalVotes: 42,
    options: [
      { index: 0, text: 'JavaScript', votes: 14 },
      { index: 1, text: 'Python',     votes: 12 },
      { index: 2, text: 'C#',         votes: 9  },
      { index: 3, text: 'Go',         votes: 5  },
      { index: 4, text: 'Rust',       votes: 2  }
    ]
  },
  'aB3xQ': {
    pollCode: 'aB3xQ',
    question: 'Which frontend framework do you prefer?',
    status: 'open',
    totalVotes: 28,
    options: [
      { index: 0, text: 'Vue',     votes: 11 },
      { index: 1, text: 'React',   votes: 10 },
      { index: 2, text: 'Angular', votes: 4  },
      { index: 3, text: 'Svelte',  votes: 3  }
    ]
  },
  'zT9mK': {
    pollCode: 'zT9mK',
    question: 'How often do you write unit tests?',
    status: 'closed',
    totalVotes: 75,
    options: [
      { index: 0, text: 'Always',    votes: 8  },
      { index: 1, text: 'Usually',   votes: 18 },
      { index: 2, text: 'Sometimes', votes: 27 },
      { index: 3, text: 'Rarely',    votes: 15 },
      { index: 4, text: 'Never',     votes: 7  }
    ]
  }
}

// Simulate a POST /api/polls response shape
// FIX: also saves into mockPolls + mockResults so getPoll/getResults can find it
export function createMockPoll(payload) {
  const code = Math.random().toString(36).substring(2, 7)

  const newPoll = {
    id: Date.now(),
    code,
    question: payload.question,
    options: payload.options,
    status: 'open',
    createdAt: new Date().toISOString(),
    expiresAt: payload.expiresAt || null,
    totalVotes: 0
  }

  // Save vào mockPolls để getPoll(code) tìm được
  mockPolls.push(newPoll)

  // Save vào mockResults để getPollResults(code) tìm được
  mockResults[code] = {
    pollCode: code,
    question: payload.question,
    status: 'open',
    totalVotes: 0,
    options: payload.options.map((text, index) => ({ index, text, votes: 0 }))
  }

  return newPoll
}
