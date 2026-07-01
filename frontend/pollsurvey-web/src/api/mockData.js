// ─────────────────────────────────────────────────────────────────────────────
//  MOCK DATA — Week 2: added question_type support
//  question_type: 'multiple_choice' | 'yes_no' | 'rating' | 'open_text'
// ─────────────────────────────────────────────────────────────────────────────

export const mockPolls = [
  {
    id: 1,
    code: '7fGh2',
    question: 'What is your favourite programming language?',
    question_type: 'multiple_choice',
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
    question_type: 'multiple_choice',
    options: ['Vue', 'React', 'Angular', 'Svelte'],
    status: 'open',
    createdAt: '2025-06-02T10:30:00Z',
    expiresAt: new Date(Date.now() + 2 * 60 * 60 * 1000).toISOString(), // expires in 2h
    totalVotes: 28
  },
  {
    id: 3,
    code: 'zT9mK',
    question: 'How often do you write unit tests?',
    question_type: 'multiple_choice',
    options: ['Always', 'Usually', 'Sometimes', 'Rarely', 'Never'],
    status: 'closed',
    createdAt: '2025-05-28T14:00:00Z',
    expiresAt: null,
    totalVotes: 75
  },
  // Merit: yes_no type
  {
    id: 4,
    code: 'yn001',
    question: 'Do you prefer working from home?',
    question_type: 'yes_no',
    options: ['Yes', 'No'],
    status: 'open',
    createdAt: '2025-06-03T09:00:00Z',
    expiresAt: null,
    totalVotes: 55
  },
  // Merit: rating type
  {
    id: 5,
    code: 'rt001',
    question: 'How would you rate this workshop?',
    question_type: 'rating',
    options: ['1', '2', '3', '4', '5'],
    status: 'open',
    createdAt: '2025-06-03T10:00:00Z',
    expiresAt: null,
    totalVotes: 30
  },
  // Merit: open_text type
  {
    id: 6,
    code: 'ot001',
    question: 'What feature would you most like to see added?',
    question_type: 'open_text',
    options: [],
    status: 'open',
    createdAt: '2025-06-03T11:00:00Z',
    expiresAt: null,
    totalVotes: 0
  }
]

export const mockResults = {
  '7fGh2': {
    pollCode: '7fGh2',
    question: 'What is your favourite programming language?',
    question_type: 'multiple_choice',
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
    question_type: 'multiple_choice',
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
    question_type: 'multiple_choice',
    status: 'closed',
    totalVotes: 75,
    options: [
      { index: 0, text: 'Always',    votes: 8  },
      { index: 1, text: 'Usually',   votes: 18 },
      { index: 2, text: 'Sometimes', votes: 27 },
      { index: 3, text: 'Rarely',    votes: 15 },
      { index: 4, text: 'Never',     votes: 7  }
    ]
  },
  'yn001': {
    pollCode: 'yn001',
    question: 'Do you prefer working from home?',
    question_type: 'yes_no',
    status: 'open',
    totalVotes: 55,
    options: [
      { index: 0, text: 'Yes', votes: 38 },
      { index: 1, text: 'No',  votes: 17 }
    ]
  },
  'rt001': {
    pollCode: 'rt001',
    question: 'How would you rate this workshop?',
    question_type: 'rating',
    status: 'open',
    totalVotes: 30,
    options: [
      { index: 0, text: '1', votes: 1 },
      { index: 1, text: '2', votes: 2 },
      { index: 2, text: '3', votes: 7 },
      { index: 3, text: '4', votes: 12 },
      { index: 4, text: '5', votes: 8 }
    ]
  },
  'ot001': {
    pollCode: 'ot001',
    question: 'What feature would you most like to see added?',
    question_type: 'open_text',
    status: 'open',
    totalVotes: 0,
    options: [],
    responses: []
  }
}

export function createMockPoll(payload) {
  const code = Math.random().toString(36).substring(2, 7)
  const question_type = payload.question_type || 'multiple_choice'

  const newPoll = {
    id: Date.now(),
    code,
    question: payload.question,
    question_type,
    options: payload.options || [],
    status: 'open',
    createdAt: new Date().toISOString(),
    expiresAt: payload.expiresAt || null,
    totalVotes: 0
  }

  mockPolls.push(newPoll)

  mockResults[code] = {
    pollCode: code,
    question: payload.question,
    question_type,
    status: 'open',
    totalVotes: 0,
    options: (payload.options || []).map((text, index) => ({ index, text, votes: 0 })),
    responses: question_type === 'open_text' ? [] : undefined
  }

  return newPoll
}
